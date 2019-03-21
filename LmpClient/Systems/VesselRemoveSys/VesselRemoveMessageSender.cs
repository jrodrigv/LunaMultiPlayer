﻿using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageSender : SubSystem<VesselRemoveSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg)));
        }

        /// <summary>
        /// Sends a vessel remove to the server. If keepVesselInRemoveList is set to true, the vessel will be removed for good and the server
        /// will skip future updates related to this vessel
        /// </summary>
        public void SendVesselRemove(Vessel vessel, bool keepVesselInRemoveList = true)
        {
            if (vessel == null) return;

            SendVesselRemove(vessel.id, keepVesselInRemoveList);
        }

        /// <summary>
        /// Sends a vessel remove to the server. If keepVesselInRemoveList is set to true, the vessel will be removed for good and the server
        /// will skip future updates related to this vessel
        /// </summary>
        public void SendVesselRemove(Guid vesselId, bool keepVesselInRemoveList = true)
        {
            LunaLog.Log($"[LMP]: Removing {vesselId} from the server");
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselRemoveMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.AddToKillList = keepVesselInRemoveList;

            SendMessage(msgData);
        }
    }
}
