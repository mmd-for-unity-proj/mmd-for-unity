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
            }

            public class MorphVertex : Chunk
            {
                public uint vertexIndex;
                public Vector3 offset;
            }
        }
    }
}