﻿using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselPartSyncUiFieldSys
{
    public class VesselPartSyncUiFieldMessageHandler : SubSystem<VesselPartSyncUiFieldSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPartSyncUiFieldMsgData msgData)) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

            if (!System.VesselPartsUiFieldsSyncs.ContainsKey(msgData.VesselId))
            {
                System.VesselPartsUiFieldsSyncs.TryAdd(msgData.VesselId, new VesselPartSyncUiFieldQueue());
            }

            if (System.VesselPartsUiFieldsSyncs.TryGetValue(msgData.VesselId, out var queue))
            {
                queue.Enqueue(msgData);
            }
        }
    }
}
