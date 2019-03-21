﻿using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.Scenario
{
    public class ScenarioMessageHandler : SubSystem<ScenarioSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ScenarioBaseMsgData msgData)) return;

            switch (msgData.ScenarioMessageType)
            {
                case ScenarioMessageType.Data:
                    QueueAllReceivedScenarios(msgData);
                    break;
                case ScenarioMessageType.Proto:
                    var data = (ScenarioProtoMsgData)msgData;
                    QueueScenarioBytes(data.ScenarioData.Module, data.ScenarioData.Data, data.ScenarioData.NumBytes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void QueueAllReceivedScenarios(ScenarioBaseMsgData msgData)
        {
            var data = (ScenarioDataMsgData) msgData;
            for (var i = 0; i < data.ScenarioCount; i++)
            {
                QueueScenarioBytes(data.ScenariosData[i].Module, data.ScenariosData[i].Data, data.ScenariosData[i].NumBytes);
            }

            if (MainSystem.NetworkState < ClientState.ScenariosSynced)
                MainSystem.NetworkState = ClientState.ScenariosSynced;
        }

        private static void QueueScenarioBytes(string scenarioModule, byte[] scenarioData, int numBytes)
        {
            var scenarioNode = scenarioData.DeserializeToConfigNode(numBytes);
            if (scenarioNode != null)
            {
                var entry = new ScenarioEntry
                {
                    ScenarioModule = scenarioModule,
                    ScenarioNode = scenarioNode
                };
                System.ScenarioQueue.Enqueue(entry);
            }
            else
            {
                LunaLog.LogError($"[LMP]: Scenario data has been lost for {scenarioModule}");
            }
        }
    }
}