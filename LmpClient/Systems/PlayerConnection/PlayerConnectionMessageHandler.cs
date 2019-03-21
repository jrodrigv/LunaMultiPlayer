﻿using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.Status;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.PlayerConnection;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System.Collections.Concurrent;

namespace LmpClient.Systems.PlayerConnection
{
    public class PlayerConnectionMessageHandler : SubSystem<PlayerConnectionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is PlayerConnectionBaseMsgData msgData)) return;

            var playerName = msgData.PlayerName;
            switch (msgData.PlayerConnectionMessageType)
            {
                case PlayerConnectionMessageType.Join:
                    LunaScreenMsg.PostScreenMessage($"{playerName} has joined the server", 3f, ScreenMessageStyle.UPPER_CENTER);
                    break;
                case PlayerConnectionMessageType.Leave:
                    WarpSystem.Singleton.RemovePlayer(playerName);
                    StatusSystem.Singleton.RemovePlayer(playerName);
                    LunaScreenMsg.PostScreenMessage($"{playerName} has left the server", 3f, ScreenMessageStyle.UPPER_CENTER);
                    break;
            }
        }
    }
}
