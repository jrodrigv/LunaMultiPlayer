﻿using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.VesselUtilities;
using LmpCommon.Locks;

namespace LmpClient.Systems.VesselLockSys
{
    public class VesselLockEvents : SubSystem<VesselLockSystem>
    {
        /// <summary>
        /// This event is called after a vessel has changed. Also called when starting a flight
        /// </summary>
        public void OnVesselChange(Vessel vessel)
        {
            //Safety check
            if (vessel == null) return;

            //In case we are reloading our current own vessel we DON'T want to release our locks
            //As that would mean that an spectator could get the control of our vessel while we are reloading it.
            //Therefore we just ignore this whole thing to avoid releasing our locks.
            //Reloading our own current vessel is a bad practice so this case should not happen anyway...
            if (LockSystem.LockQuery.GetControlLockOwner(vessel.id) == SettingsSystem.CurrentSettings.PlayerName)
                return;

            //Release all vessel CONTROL locks as we are switching to a NEW vessel.
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Control);

            if (LockSystem.LockQuery.ControlLockExists(vessel.id) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
            {
                //We switched to a vessel that is controlled by another player so start spectating
                System.StartSpectating(vessel.id);
            }
            else
            {
                if (VesselCommon.IsSpectating)
                    VesselLockSystem.Singleton.StopSpectating();
                if (FlightDriver.flightStarted)
                {
                    LockSystem.Singleton.AcquireControlLock(vessel.id);
                }
            }
        }

        /// <summary>
        /// Remove the spectate lock AFTER the scene is loaded
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            if (data != GameScenes.FLIGHT)
            {
                //If we are going to anything that is not flight, remove spectator, control and update locks

                VesselLockSystem.Singleton.StopSpectating();
                LockSystem.Singleton.ReleaseAllPlayerSpecifiedLocks(LockType.Control, LockType.Update, LockType.Kerbal);
            }

            if (data == GameScenes.FLIGHT || data == GameScenes.TRACKSTATION)
            {
                //If we are going to flight scene or tracking station try to get as many unloaded update locks as possible
                foreach (var vessel in FlightGlobals.Vessels)
                {
                    if (!LockSystem.LockQuery.UnloadedUpdateLockExists(vessel.id))
                        LockSystem.Singleton.AcquireUnloadedUpdateLock(vessel.id);
                }
            }
            else
            {
                //We are going to KSC/Editor/Menu so we must release all our locks
                LockSystem.Singleton.ReleaseAllPlayerSpecifiedLocks(LockType.UnloadedUpdate);
            }
        }

        /// <summary>
        /// When a vessel gets loaded try to acquire it's update lock if we can
        /// </summary>
        public void VesselLoaded(Vessel vessel)
        {
            if (!LockSystem.LockQuery.UpdateLockExists(vessel.id) && !VesselCommon.IsSpectating)
            {
                LockSystem.Singleton.AcquireUpdateLock(vessel.id);
            }
        }

        /// <summary>
        /// If we get the Update lock, force the getting of the unloaded update lock.
        /// If we get a control lock, force getting the update and unloaded update
        /// </summary>
        public void LockAcquire(LockDefinition lockDefinition)
        {
            switch (lockDefinition.Type)
            {
                case LockType.Control:
                    if (lockDefinition.PlayerName == SettingsSystem.CurrentSettings.PlayerName)
                    {
                        if (VesselCommon.IsSpectating)
                            VesselLockSystem.Singleton.StopSpectating();
                        LockSystem.Singleton.AcquireUpdateLock(lockDefinition.VesselId, true);
                        LockSystem.Singleton.AcquireUnloadedUpdateLock(lockDefinition.VesselId, true);
                        LockSystem.Singleton.AcquireKerbalLock(lockDefinition.VesselId, true);

                        //As we got the lock of that vessel, remove its FS and position updates
                        //This is done so even if the vessel has queued updates, we ignore them as we are controlling it
                        VesselCommon.RemoveVesselFromSystems(lockDefinition.VesselId);
                    }
                    else
                    {
                        //Somebody else got the control of our active vessel so start spectating
                        if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == lockDefinition.VesselId)
                        {
                            System.StartSpectating(lockDefinition.VesselId);
                        }

                        //If some other player got the control lock release the update lock in case we have it
                        if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(lockDefinition.VesselId, SettingsSystem.CurrentSettings.PlayerName))
                        {
                            LockSystem.LockStore.RemoveLock(LockSystem.LockQuery.GetUpdateLock(lockDefinition.VesselId));
                            LockSystem.LockStore.AddOrUpdateLock(new LockDefinition(LockType.UnloadedUpdate, lockDefinition.PlayerName, lockDefinition.VesselId));
                        }

                        //If some other player got the control lock release the unloaded update lock in case we have it
                        if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(lockDefinition.VesselId, SettingsSystem.CurrentSettings.PlayerName))
                        {
                            LockSystem.LockStore.RemoveLock(LockSystem.LockQuery.GetUnloadedUpdateLock(lockDefinition.VesselId));
                            LockSystem.LockStore.AddOrUpdateLock(new LockDefinition(LockType.UnloadedUpdate, lockDefinition.PlayerName, lockDefinition.VesselId));
                        }

                        //TODO:We should release the kerbals locks?
                    }
                    break;
                case LockType.Update:
                    if (lockDefinition.PlayerName != SettingsSystem.CurrentSettings.PlayerName)
                    {
                        //If some other player got the update lock release the unloaded update lock  just in case we have it
                        if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(lockDefinition.VesselId, SettingsSystem.CurrentSettings.PlayerName))
                        {
                            LockSystem.LockStore.RemoveLock(LockSystem.LockQuery.GetUnloadedUpdateLock(lockDefinition.VesselId));
                            LockSystem.LockStore.AddOrUpdateLock(new LockDefinition(LockType.UnloadedUpdate, lockDefinition.PlayerName, lockDefinition.VesselId));
                        }
                        
                        //TODO:We should release the kerbals locks?
                    }
                    else
                    {
                        LockSystem.Singleton.AcquireUnloadedUpdateLock(lockDefinition.VesselId, true);
                        LockSystem.Singleton.AcquireKerbalLock(lockDefinition.VesselId, true);
                    }
                    break;
            }
        }

        /// <summary>
        /// If a player releases an update or unloadedupdate lock try to get it.
        /// If he releases a control lock and we are spectating try to get the current vessel control lock
        /// </summary>
        public void LockReleased(LockDefinition lockDefinition)
        {
            switch (lockDefinition.Type)
            {
                case LockType.Control:
                    if (VesselCommon.IsSpectating && FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == lockDefinition.VesselId)
                    {
                        LockSystem.Singleton.AcquireControlLock(lockDefinition.VesselId);
                    }
                    break;
                case LockType.UnloadedUpdate:
                case LockType.Update:
                    if (HighLogic.LoadedScene < GameScenes.FLIGHT || VesselCommon.IsSpectating) return;

                    var vessel = FlightGlobals.FindVessel(lockDefinition.VesselId);
                    if (vessel != null)
                    {
                        switch (lockDefinition.Type)
                        {
                            case LockType.Update:
                                if (vessel.loaded)
                                    LockSystem.Singleton.AcquireUpdateLock(lockDefinition.VesselId);
                                break;
                            case LockType.UnloadedUpdate:
                                LockSystem.Singleton.AcquireUnloadedUpdateLock(lockDefinition.VesselId);
                                break;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// When a vessel is being unloaded, release it's update lock
        /// </summary>
        public void VesselUnloading(Vessel vessel)
        {
            if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                LockSystem.Singleton.ReleaseUpdateLock(vessel.id);
        }

        /// <summary>
        /// This event is triggered after we started a flight and we are already in the final vessel.
        /// It's important that we get the control lock once the FlightDriver has started!
        /// Otherwise another player will see your vessel going up in the air while you're loading
        /// as you have the control lock -> the immortal system makes you immortal -> the FlightIntegrator_FixedUpdate is skipped on
        /// that vessel.
        /// </summary>
        public void FlightStarted()
        {
            if (FlightGlobals.ActiveVessel != null && !LockSystem.LockQuery.ControlLockExists(FlightGlobals.ActiveVessel.id))
            {
                LockSystem.Singleton.AcquireControlLock(FlightGlobals.ActiveVessel.id);
            }
        }
    }
}
