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
                public byte boneWeight;
                public byte boneWeight1;
                public byte boneWeight2;
                public byte edgeFlag;

                public override void Read(BinaryReader r)
                {
                    position = ReadVector3(r);
                    normal = ReadVector3(r);
                    uv = ReadVector2(r);
                    boneNumber = new ushort[2];
                    boneNumber[0] = ReadUShort(r);
                    boneNumber[1] = ReadUShort(r);
                    boneWeight = ReadByte(r);
                    boneWeight1 = boneWeight;
                    boneWeight2 = (byte)(100 - (int)boneWeight1);
                    edgeFlag = ReadByte(r);
                }
            }
        }
    }
}