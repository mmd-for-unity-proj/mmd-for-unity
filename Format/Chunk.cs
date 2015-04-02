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

            protected void ReadItems<ElemType>(BinaryReader r, List<ElemType> elements, int size)
                where ElemType : Chunk, new()
            {
                elements = new List<ElemType>(size);
                for (int i = 0; i < size; ++i)
                {
                    var elem = new ElemType();
                    elem.Read(r);
                    elements.Add(elem);
                }
            }

            protected void ReadStrings(BinaryReader r, List<string> elements, int count, int length)
            {
                elements = new List<string>(count);
                for (int i = 0; i < count; ++i)
                {
                    elements.Add(ReadString(r, length));
                }
            }

            protected int ReadCount<CountType>(BinaryReader r)
                where CountType : struct
            {
                CountType testObject = new CountType();

                if (testObject is uint)
                {
                    return (int)r.ReadUInt32();
                }
                else if (testObject is ushort)
                {
                    return (int)r.ReadUInt16();
                }
                else if (testObject is byte)
                {
                    return (int)r.ReadByte();
                }

                throw new ArgumentException("無効な型です");
            }
        }
    }
}