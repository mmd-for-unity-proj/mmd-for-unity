using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace MMD
{
    namespace Format
    {
        public abstract class Chunk
        {
            public virtual void Read(BinaryReader r) { }
            public virtual void Write(BinaryWriter w) { }

            protected Vector3 ReadVector3(BinaryReader r)
            {
                float x = r.ReadSingle();
                float y = r.ReadSingle();
                float z = r.ReadSingle();
                return new Vector3(x, y, z);
            }

            protected Vector2 ReadVector2(BinaryReader r)
            {
                float x = r.ReadSingle();
                float y = r.ReadSingle();
                return new Vector2(x, y);
            }

            protected uint ReadUInt(BinaryReader r)
            {
                return r.ReadUInt32();
            }

            protected ushort ReadUShort(BinaryReader r)
            {
                return r.ReadUInt16();
            }

            protected byte ReadByte(BinaryReader r)
            {
                return r.ReadByte();
            }

            protected float ReadFloat(BinaryReader r)
            {
                return r.ReadSingle();
            }

            protected string ReadString(BinaryReader r, int length)
            {
                var bytes = r.ReadBytes(length);
                return Encoding.GetEncoding("Shift_JIS").GetString(bytes);
            }
        }
    }
}