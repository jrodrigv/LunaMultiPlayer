﻿using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class VesselCliMsg : CliMsgBase<VesselBaseMsgData>
    {
        /// <inheritdoc />
        internal VesselCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(VesselCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)VesselMessageType.Proto] = typeof(VesselProtoMsgData),
            [(ushort)VesselMessageType.Remove] = typeof(VesselRemoveMsgData),
            [(ushort)VesselMessageType.Position] = typeof(VesselPositionMsgData),
            [(ushort)VesselMessageType.Flightstate] = typeof(VesselFlightStateMsgData),
            [(ushort)VesselMessageType.Update] = typeof(VesselUpdateMsgData),
            [(ushort)VesselMessageType.Resource] = typeof(VesselResourceMsgData),
            [(ushort)VesselMessageType.Sync] = typeof(VesselSyncMsgData),
            [(ushort)VesselMessageType.PartSyncField] = typeof(VesselPartSyncFieldMsgData),
            [(ushort)VesselMessageType.PartSyncUiField] = typeof(VesselPartSyncUiFieldMsgData),
            [(ushort)VesselMessageType.PartSyncCall] = typeof(VesselPartSyncCallMsgData),
            [(ushort)VesselMessageType.ActionGroup] = typeof(VesselActionGroupMsgData),
            [(ushort)VesselMessageType.Fairing] = typeof(VesselFairingMsgData),
            [(ushort)VesselMessageType.Decouple] = typeof(VesselDecoupleMsgData),
            [(ushort)VesselMessageType.Couple] = typeof(VesselCoupleMsgData),
            [(ushort)VesselMessageType.Undock] = typeof(VesselUndockMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Vessel;
        protected override int DefaultChannel => IsUnreliableMessage() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsUnreliableMessage() ? 
            NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsUnreliableMessage()
        {
            return Data.SubType == (ushort) VesselMessageType.Position || Data.SubType == (ushort) VesselMessageType.Flightstate
                   || Data.SubType == (ushort)VesselMessageType.Update || Data.SubType == (ushort)VesselMessageType.Resource;
        }
    }
}