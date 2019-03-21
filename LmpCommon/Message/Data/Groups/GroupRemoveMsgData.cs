﻿using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Groups
{
    public class GroupRemoveMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupRemoveMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.RemoveGroup;

        public string GroupName;

        public override string ClassName { get; } = nameof(GroupRemoveMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(GroupName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            GroupName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + GroupName.GetByteCount();
        }
    }
}
