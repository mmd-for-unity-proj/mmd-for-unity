using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MMD
{
    namespace Format
    {
        public class ChunkListBase<ElemType, CountType> : Chunk
            where ElemType : new()
            where CountType : struct
        {
            protected List<ElemType> elements = new List<ElemType>();
            public ElemType this[int i] { get { return elements[i]; } set { elements[i] = value; } }
            public int Count { get { return elements.Count; } }
            
        }

        public class StringChunkList : Chunk
        {
            protected List<string> elements = new List<string>();
            public string this[int i] { get { return elements[i]; } set { elements[i] = value; } }
            public int Count { get { return elements.Count; } }

            protected void ReadStrings<CountType>(BinaryReader r, int count)
                where CountType : struct
            {

            }
        }

        public class StructChunkList<ElemType, CountType> : ChunkListBase<ElemType, CountType>
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
                var count = ReadCount<CountType>(r);
                elements = new List<ElemType>(count);

                for (int i = 0; i < count; ++i)
                {
                    var elem = new ElemType();
                    elem.Read(r);
                    elements.Add(elem);
                }
            }
        }
    }
}
