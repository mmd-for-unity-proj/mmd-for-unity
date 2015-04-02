using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class VertexList : ChunkList<Vertex, uint> { }

            public class Vertex : Chunk
            {
                public Vector3 position;
                public Vector3 normal;
                public Vector2 uv;
                public ushort[] boneNumber;
                public byte[] boneWeight;
                public byte edgeFlag;

                public override void Read(BinaryReader r)
                {
                    
                }
            }
        }
    }
}