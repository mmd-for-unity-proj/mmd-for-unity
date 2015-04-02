using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MorphList : ChunkList<Morph, ushort> { }

            public class Morph : Chunk
            {
                public string name;
                public uint vertexCount;
                public byte morphType;
                public List<MorphVertex> vertices;

                public override void Read(System.IO.BinaryReader r)
                {
                    name = ReadString(r, 20);
                    vertexCount = ReadUInt(r);
                    morphType = ReadByte(r);
                    ReadItems(r, vertices, (int)vertexCount);
                }
            }

            public class MorphVertex : Chunk
            {
                public uint vertexIndex;
                public Vector3 offset;

                public override void Read(System.IO.BinaryReader r)
                {
                    vertexIndex = ReadUInt(r);
                    offset = ReadVector3(r);
                }
            }
        }
    }
}