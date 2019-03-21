﻿using System;
using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Wrapper for transmitting the ksp RDTech objects (for the tech nodes).
    /// </summary>
    public class TechNodeInfo
    {
        public string Id;
        public int NumBytes;
        public byte[] Data = new byte[0];

        public TechNodeInfo()
        {

        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copyFrom"></param>
        public TechNodeInfo(TechNodeInfo copyFrom)
        {
            Id = string.Copy(copyFrom.Id);
            NumBytes = copyFrom.NumBytes;
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            Array.Copy(copyFrom.Data, Data, NumBytes);
        }

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Id = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return Id.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
