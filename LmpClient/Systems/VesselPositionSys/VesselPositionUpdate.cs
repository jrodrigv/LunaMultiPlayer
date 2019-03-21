﻿using LmpClient.Extensions;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselPositionSys.ExtensionMethods;
using LmpClient.Systems.Warp;
using LmpClient.VesselUtilities;
using LmpCommon;
using LmpCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LmpClient.Systems.VesselPositionSys
{
    /// <summary>
    /// This class handle the vessel position updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselPositionUpdate
    {
        #region Fields

        private Vessel _vessel;
        public Vessel Vessel
        {
            get
            {
                if (_vessel == null)
                    _vessel = FlightGlobals.FindVessel(VesselId);
                return _vessel;
            }
        }

        public CelestialBody Body => GetBody(BodyIndex);

        public VesselPositionUpdate Target { get; set; }

        #region Message Fields

        public Guid VesselId { get; set; }
        public int BodyIndex { get; set; }
        public bool Landed { get; set; }
        public bool Splashed { get; set; }
        public double[] LatLonAlt { get; set; } = new double[3];
        public double[] VelocityVector { get; set; } = new double[3];
        public double[] NormalVector { get; set; } = new double[3];
        public double[] Orbit { get; set; } = new double[8];
        public float[] SrfRelRotation { get; set; } = new float[4];
        public float PingSec { get; set; }
        public float HeightFromTerrain { get; set; }
        public double GameTimeStamp { get; set; }
        public int SubspaceId { get; set; }
        public bool HackingGravity { get; set; }

        #endregion

        #region Vessel position information fields

        public Orbit KspOrbit { get; set; } = new Orbit();
        public Vector3d Velocity => new Vector3d(VelocityVector[0], VelocityVector[1], VelocityVector[2]);
        public Quaternion SurfaceRelRotation => new Quaternion(SrfRelRotation[0], SrfRelRotation[1], SrfRelRotation[2], SrfRelRotation[3]);
        public Vector3 Normal => new Vector3d(NormalVector[0], NormalVector[1], NormalVector[2]);

        #endregion

        #region Interpolation fields

        private double MaxInterpolationDuration => WarpSystem.Singleton.SubspaceIsEqualOrInThePast(Target.SubspaceId) ?
            TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds * 2
            : double.MaxValue;

        private int MessageCount => VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(VesselId, out var queue) ? queue.Count : 0;
        public double TimeDifference { get; private set; }
        public double ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || CurrentFrame >= NumFrames;

        public double InterpolationDuration => LunaMath.Clamp(Target.GameTimeStamp - GameTimeStamp + ExtraInterpolationTime, 0, MaxInterpolationDuration);
        public float LerpPercentage => Mathf.Clamp01(CurrentFrame / NumFrames);

        public float CurrentFrame { get; set; }
        public int NumFrames => (int)(InterpolationDuration / Time.fixedDeltaTime) + 1;

        #endregion

        #endregion

        #region Constructor

        public VesselPositionUpdate() { }

        public VesselPositionUpdate(VesselPositionMsgData msgData)
        {
            VesselId = msgData.VesselId;
            BodyIndex = msgData.BodyIndex;
            SubspaceId = msgData.SubspaceId;
            PingSec = msgData.PingSec;
            HeightFromTerrain = msgData.HeightFromTerrain;
            Landed = msgData.Landed;
            Splashed = msgData.Splashed;
            GameTimeStamp = msgData.GameTime;
            HackingGravity = msgData.HackingGravity;

            Array.Copy(msgData.SrfRelRotation, SrfRelRotation, 4);
            Array.Copy(msgData.LatLonAlt, LatLonAlt, 3);
            Array.Copy(msgData.VelocityVector, VelocityVector, 3);
            Array.Copy(msgData.NormalVector, NormalVector, 3);
            Array.Copy(msgData.Orbit, Orbit, 8);
        }

        public void CopyFrom(VesselPositionUpdate update)
        {
            VesselId = update.VesselId;
            BodyIndex = update.BodyIndex;
            SubspaceId = update.SubspaceId;
            PingSec = update.PingSec;
            HeightFromTerrain = update.HeightFromTerrain;
            Landed = update.Landed;
            Splashed = update.Splashed;
            GameTimeStamp = update.GameTimeStamp;
            HackingGravity = update.HackingGravity;

            Array.Copy(update.SrfRelRotation, SrfRelRotation, 4);
            Array.Copy(update.LatLonAlt, LatLonAlt, 3);
            Array.Copy(update.VelocityVector, VelocityVector, 3);
            Array.Copy(update.NormalVector, NormalVector, 3);
            Array.Copy(update.Orbit, Orbit, 8);
        }

        #endregion

        #region Main method

        /// <summary>
        /// Updates the vessel position with the current data
        /// </summary>
        public void UpdateVesselWithPositionData()
        {
            if (Body == null) return;

            if (InterpolationFinished && VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(VesselId, out var queue) && queue.TryDequeue(out var targetUpdate))
            {
                if (Target == null)
                {
                    //We enter in this clause when it's the first iteration
                    GameTimeStamp = targetUpdate.GameTimeStamp - TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds;
                    PingSec = 0;

                    CopyFrom(targetUpdate);
                }
                else
                {
                    CopyFrom(Target);
                }

                CurrentFrame = 0;

                if (Target != null)
                {
                    Target.CopyFrom(targetUpdate);
                    VesselPositionSystem.TargetVesselUpdateQueue[VesselId].Recycle(targetUpdate);
                }
                else
                {
                    Target = targetUpdate;
                }

                AdjustExtraInterpolationTimes();
                InitializeOrbits();
            }

            if (Target == null) return;

            //ALWAYS set the position of the vessel even if we don't have anything in the queue. Otherwise its position will shake
            Vessel.SetVesselPosition(this, Target, LerpPercentage);
        }

        /// <summary>
        /// Call this method to apply a vessel update using interpolation and advance the frame count
        /// </summary>
        public void ApplyInterpolatedVesselUpdate()
        {
            try
            {
                //ALWAYS set the position of the vessel even if we don't have anything in the queue. Otherwise its position will shake
                UpdateVesselWithPositionData();
            }
            catch (Exception e)
            {
                LunaLog.LogError($"ApplyInterpolations: {e}");
            }
            finally
            {
                CurrentFrame++;
            }
        }

        private void InitializeOrbits()
        {
            var lanFixFactor = GetLanFixFactor(GameTimeStamp, SubspaceId, Vessel, Body);
            KspOrbit.SetOrbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3] + lanFixFactor, Orbit[4], Orbit[5], CalculateEpochTime(Orbit[6]), Body);

            lanFixFactor = GetLanFixFactor(Target.GameTimeStamp, Target.SubspaceId, Vessel, Target.Body);
            Target.KspOrbit.SetOrbit(Target.Orbit[0], Target.Orbit[1], Target.Orbit[2], Target.Orbit[3] + lanFixFactor, Target.Orbit[4], Target.Orbit[5], 
                CalculateTargetEpochTime(Target.Orbit[6]), Target.Body);

            var meanAnomalyFixFactor = GetMeanAnomalyFixFactor(GameTimeStamp, SubspaceId, Vessel, KspOrbit);
            KspOrbit.SetOrbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3] + lanFixFactor, Orbit[4], Orbit[5] + meanAnomalyFixFactor, Orbit[6], Body);

            meanAnomalyFixFactor = GetMeanAnomalyFixFactor(Target.GameTimeStamp, Target.SubspaceId, Vessel, Target.KspOrbit);
            Target.KspOrbit.SetOrbit(Target.Orbit[0], Target.Orbit[1], Target.Orbit[2], Target.Orbit[3] + lanFixFactor, Target.Orbit[4], Target.Orbit[5] + meanAnomalyFixFactor, Target.Orbit[6], Target.Body);
        }

        private double CalculateTargetEpochTime(double targetEpoch)
        {
            if (SubspaceId == -1 || WarpSystem.Singleton.CurrentlyWarping || WarpSystem.Singleton.SubspaceIsInThePast(SubspaceId))
                return targetEpoch;

            return Planetarium.GetUniversalTime() + (Target.GameTimeStamp - GameTimeStamp);
        }

        private double CalculateEpochTime(double currentEpoch)
        {
            if (SubspaceId == -1 || WarpSystem.Singleton.CurrentlyWarping || WarpSystem.Singleton.SubspaceIsInThePast(SubspaceId))
                return currentEpoch;

            return Planetarium.GetUniversalTime();
        }

        /// <summary>
        /// Here we adjust the Mean anomaly according to the time of the subspace where the player send the message.
        /// If we don't do this then the vessel will be in a incorrect position along the orbit as the epoch is not the same
        /// </summary>
        private static double GetMeanAnomalyFixFactor(double timestamp, int subspaceId, Vessel vessel, Orbit orbit)
        {
            //If the vessel is in orbit we return 0 as we want to see the vessel IN THE FUTURE. This makes the behaviour closer to what KSP in single player does
            if (vessel && (vessel.situation >= Vessel.Situations.ORBITING || vessel.loaded && subspaceId == -1))
                return 0;

            //If the vessel is in atmosphere, we must show the REAL position of the vessel as if we use the projection, the vessel might be inside kerbin
            //if we are in a different subspace
            if (subspaceId == -1 && timestamp < TimeSyncSystem.UniversalTime)
                return (orbit.getObtAtUT(TimeSyncSystem.UniversalTime) - orbit.getObtAtUT(timestamp)) * orbit.meanMotion;

            if (WarpSystem.Singleton.CurrentlyWarping || WarpSystem.Singleton.SubspaceIsInThePast(subspaceId))
            {
                var timeDiff = WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(subspaceId);
                return (orbit.getObtAtUT(TimeSyncSystem.UniversalTime) - orbit.getObtAtUT(TimeSyncSystem.UniversalTime - timeDiff)) * orbit.meanMotion;
            }

            return 0;
        }

        /// <summary>
        /// Here we adjust the LAN according to the time of the subspace where the player send the message.
        /// If we don't do this, then the orbit will be shifted in the longitude axis as your planet might be more
        /// advanced in time so your planet rotations will not match
        /// </summary>
        private static double GetLanFixFactor(double timestamp, int subspaceId, Vessel vessel, CelestialBody body)
        {
            //If the vessel is in orbit we return 0 as we want to see the vessel IN THE FUTURE. This makes the behaviour closer to what KSP in single player does
            if (vessel && vessel.situation >= Vessel.Situations.ORBITING)
                return 0;

            //If the vessel is in atmosphere, we must show the REAL position of the vessel as if we use the projection, the vessel might be inside kerbin
            //if we are in a different subspace
            if (body.SiderealDayLength() > 0)
            {
                if (subspaceId == -1 && timestamp < TimeSyncSystem.UniversalTime)
                    return Math.Abs((TimeSyncSystem.UniversalTime - timestamp) * 360 / body.SiderealDayLength());

                if (WarpSystem.Singleton.CurrentlyWarping || WarpSystem.Singleton.SubspaceIsInThePast(subspaceId))
                {
                    var timeDiff = WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(subspaceId);
                    return Math.Abs(timeDiff * 360 / body.SiderealDayLength());
                }
            }

            return 0;
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced.
        /// The idea is that we replay the message at the correct time that is GameTimeWhenMessageWasSent+InterpolationOffset
        /// In order to adjust we increase or decrease the interpolation duration so next packet matches the time more perfectly
        /// </summary>
        public void AdjustExtraInterpolationTimes()
        {
            TimeDifference = TimeSyncSystem.UniversalTime - GameTimeStamp - VesselCommon.PositionAndFlightStateMessageOffsetSec(PingSec);

            if (WarpSystem.Singleton.CurrentlyWarping || SubspaceId == -1)
            {
                //This is the case when the message was received while warping or we are warping.

                /* We are warping:
                 * While WE warp if we receive a message that is from before our time, we want to skip it as fast as possible!
                 * If the packet is in the future then we must interpolate towards it
                 *
                 * Player was warping:
                 * The message was received when HE was warping. We don't know his final subspace time BUT if the message was sent
                 * in a time BEFORE ours, we can skip it as fast as possible.
                 * If the packet is in the future then we must interpolate towards it
                 *
                 * Bear in mind that even if the interpolation against the future packet is long because he is in the future,
                 * when we stop warping this method will be called
                 *
                 * Also, we don't remove messages if we are close to the min recommended value
                 *
                 */

                if (TimeDifference > 0)
                {
                    //This means that we are behind and we must consume the message fast
                    CurrentFrame = float.MaxValue;
                }
                else
                {
                    ExtraInterpolationTime = GetInterpolationFixFactor();
                }
            }
            else
            {
                //This is the easiest case, the message comes from the same or a past subspace

                //IN past or same subspaces we want to be SettingsSystem.CurrentSettings.InterpolationOffset seconds BEHIND the player position
                if (WarpSystem.Singleton.SubspaceIsInThePast(SubspaceId))
                {
                    /* The subspace is in the past so REMOVE the difference to normalize it
                     * Example: P1 subspace is +7 seconds. Your subspace is + 30 seconds
                     * Packet TimeDifference will be 23 seconds but in reality it should be 0
                     * So, we remove the time difference between subspaces (30 - 7 = 23)
                     * And now the TimeDifference - 23 = 0
                     */
                    var timeToAdd = Math.Abs(WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(SubspaceId));
                    TimeDifference -= timeToAdd;
                }

                ExtraInterpolationTime = (TimeDifference > 0 ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        /// <summary>
        /// This gives the fix factor. It scales up or down depending on the error we have
        /// </summary>
        private double GetInterpolationFixFactor()
        {
            //The minimum fix factor is Time.fixedDeltaTime. Usually 0.02 s

            var errorInSeconds = Math.Abs(Math.Abs(TimeDifference));
            var errorInFrames = errorInSeconds / Time.fixedDeltaTime;

            //We cannot fix errors that are below the delta time!
            if (errorInFrames < 1)
                return 0;

            if (errorInFrames <= 2)
            {
                //The error is max 2 frames ahead/below
                return Time.fixedDeltaTime;
            }
            if (errorInFrames <= 5)
            {
                //The error is max 5 frames ahead/below
                return Time.fixedDeltaTime * 2;
            }
            if (errorInSeconds <= 2.5)
            {
                //The error is max 2.5 SECONDS ahead/below
                return Time.fixedDeltaTime * errorInFrames / 2;
            }

            //The error is really big...
            return Time.fixedDeltaTime * errorInFrames;
        }

        #endregion

        #region Private

        #region Helper methods

        private static CelestialBody GetBody(int bodyIndex)
        {
            try
            {
                return FlightGlobals.Bodies[bodyIndex];
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #endregion
    }
}
