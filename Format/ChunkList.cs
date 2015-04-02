using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace MMD
{
    namespace Format
    {
        public class ChunkListBase<ElemType, CountType> : Chunk
            where ElemType : new()
            where CountType : struct
        {
            protected List<ElemType> elements;
            public ElemType this[int i] { get { return elements[i]; } set { elements[i] = value; } }

            protected uint ReadCount(BinaryReader r)
            {
                CountType testObject = new CountType();

                if (testObject is uint)
                {
                    return r.ReadUInt32();
                }
                else if (testObject is ushort)
                {
                    return r.ReadUInt16();
                }
                else if (testObject is byte)
                {
                    return r.ReadByte();
                }

                throw new ArgumentException("無効な型です");
            }
        }

        public class ChunkStructList<ElemType, CountType> : Chunk
            where ElemType : new()
            where CountType : struct
        {
            
        }

        /// <summary>
        /// チャンクのリスト
        /// </summary>
        /// <typeparam name="ElemType">要素の型</typeparam>
        /// <typeparam name="CountType">カウントの型</typeparam>
        public class ChunkList<ElemType, CountType> : ChunkListBase<ElemType, CountType>
            where ElemType : Chunk, new()
            where CountType : struct
        {
            public override void Read(BinaryReader r)
            {
                CountType testObject = new CountType();
                var count = ReadCount(r);

                for (uint i = 0; i < count; ++i)
                {
                    var elem = new ElemType();
                    elem.Read(r);
                    elements.Add(new ElemType());
                }
            }
        }
    }
}
