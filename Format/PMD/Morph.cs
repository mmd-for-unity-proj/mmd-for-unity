using System.Collections;
using System.Collections.Generic;
using MMD.Format.Common;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MorphList : ChunkList<Morph, ushort> 
            {
                public List<Morph> Morphs { get { return elements; } }
            }

            public class Morph : Chunk
            {
                public string name;
                public uint vertexCount;
                public byte morphType;
                public List<MorphVertex> vertices;

                public override void Read(System.IO.BinaryReader r, float scale)
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
                public Vector3 offset = new Vector3();

                public override void Read(System.IO.BinaryReader r, float scale)
                {
                    vertexIndex = ReadUInt(r);
                    offset.Read(r, scale);
                }
            }
        }
    }
}