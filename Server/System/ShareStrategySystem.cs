﻿using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public static class ShareStrategySystem
    {
        public static void StrategyReceived(ClientStructure client, ShareProgressStrategyMsgData data)
        {
            LunaLog.Debug($"strategy changed: {data.Strategy.Name}");

            //Send the strategy update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteStrategyDataToFile(data.Strategy);
        }
    }
}
