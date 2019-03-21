﻿using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusInfo
    {
        public string PlayerName;
        public string VesselText;
        public string StatusText;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(VesselText);
            lidgrenMsg.Write(StatusText);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
            VesselText = lidgrenMsg.ReadString();
            StatusText = lidgrenMsg.ReadString();
        }

        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + VesselText.GetByteCount() + StatusText.GetByteCount();
        }
    }
}
