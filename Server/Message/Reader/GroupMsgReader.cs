﻿using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.System;
using System.Linq;

namespace LunaServer.Message.Reader
{
    public class GroupMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var data = (GroupBaseMsgData)messageData;
            switch (data.GroupMessageType)
            {
                case GroupMessageType.ListRequest:
                    MessageQueuer.SendToClient<GroupSrvMsg>(client, new GroupListResponseMsgData
                    {
                        Groups = GroupSystem.Groups.Values.ToArray()
                    });
                    break;
                case GroupMessageType.CreateGroup:
                    GroupSystem.CreateGroup(client.PlayerName, ((GroupCreateMsgData) data).GroupName);
                    break;
                case GroupMessageType.RemoveGroup:
                    GroupSystem.RemoveGroup(client.PlayerName, ((GroupRemoveMsgData)data).GroupName);
                    break;
                case GroupMessageType.GroupUpdate:
                    GroupSystem.UpdateGroup(client.PlayerName, ((GroupUpdateMsgData)data).Group);
                    break;
            }
        }
    }
}