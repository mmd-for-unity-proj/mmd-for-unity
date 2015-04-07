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
            public virtual void Read(BinaryReader r, float scale) { }
            public virtual void Write(BinaryWriter w) { }
            public virtual void Write(BinaryWriter w, float scale) { }

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
                return ConvertByteToString(bytes, "");
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

            string ConvertByteToString(byte[] bytes, string line_feed_code)
            {
                // パディングの消去, 文字を詰める
                if (bytes[0] == 0) return "";

                int count;
                for (count = 0; count < bytes.Length; count++)
                    if (bytes[count] == 0) break;

                byte[] buf = new byte[count]; // NULL文字を含めるとうまく行かない
                for (int i = 0; i < count; i++)
                    buf[i] = bytes[i];

#if UNITY_STANDALONE_OSX
                buf = Encoding.Convert(Encoding.GetEncoding(932), Encoding.UTF8, buf);
#else
                buf = Encoding.Convert(Encoding.GetEncoding(0), Encoding.UTF8, buf);
#endif

                string result = Encoding.UTF8.GetString(buf);

                //改行コード統一(もしくは除去)
                if (Environment.NewLine != line_feed_code)
                    result = result.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", line_feed_code);
                return result;
            }

            protected List<string> ReadStrings(BinaryReader r, int count, int length)
            {
                var elements = new List<string>(count);
                for (int i = 0; i < count; ++i)
                {
                    elements.Add(ReadString(r, length));
                }
                return elements;
            }

            protected int ReadCount<CountType>(BinaryReader r)
                where CountType : struct
            {
                var testObject = new CountType().GetType().ToString();

                if (testObject == "System.UInt32")
                {
                    return (int)r.ReadUInt32();
                }
                else if (testObject == "System.UInt16")
                {
                    return (int)r.ReadUInt16();
                }
                else if (testObject == "System.Byte")
                {
                    return (int)r.ReadByte();
                }

                throw new ArgumentException("無効な型です");
            }
        }
    }
}