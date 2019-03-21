﻿using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Motd;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class MotdSrvMsg : SrvMsgBase<MotdBaseMsgData>
    {
        /// <inheritdoc />
        internal MotdSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(MotdSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)MotdMessageType.Reply] = typeof(MotdReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Motd;
        protected override int DefaultChannel => 12;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}