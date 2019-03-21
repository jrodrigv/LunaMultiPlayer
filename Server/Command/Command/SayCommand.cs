﻿using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings.Structures;

namespace Server.Command.Command
{
    public class SayCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            LunaLog.Normal($"Broadcasting {commandArgs}");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ChatMsgData>();
            msgData.From = GeneralSettings.SettingsStore.ConsoleIdentifier;
            msgData.Text = commandArgs;

            MessageQueuer.SendToAllClients<ChatSrvMsg>(msgData);

            return true;
        }
    }
}
