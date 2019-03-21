﻿using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageSender : SubSystem<VesselUpdateSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselUpdate(Vessel vessel)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselUpdateMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.Name = vessel.vesselName;
            msgData.Type = vessel.vesselType.ToString();
            msgData.DistanceTraveled = vessel.distanceTraveled;

            msgData.Situation = vessel.situation.ToString();
            msgData.Landed = vessel.Landed;
            msgData.LandedAt = vessel.landedAt;
            msgData.DisplayLandedAt = vessel.displaylandedAt;
            msgData.Splashed = vessel.Splashed;
            msgData.MissionTime = vessel.missionTime;
            msgData.LaunchTime = vessel.launchTime;
            msgData.LastUt = vessel.lastUT;
            msgData.Persistent = vessel.isPersistent;
            msgData.RefTransformId = vessel.referenceTransformId;

            msgData.AutoClean = vessel.AutoClean;
            msgData.AutoCleanReason = vessel.AutoCleanReason;
            msgData.WasControllable = vessel.IsControllable;
            msgData.Stage = vessel.currentStage;
            msgData.Com[0] = vessel.localCoM.x;
            msgData.Com[1] = vessel.localCoM.y;
            msgData.Com[2] = vessel.localCoM.z;
            
            SendMessage(msgData);
        }
    }
}
