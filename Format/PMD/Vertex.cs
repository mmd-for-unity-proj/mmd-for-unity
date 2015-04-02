using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class VertexList
            {
                public uint vertexCount;
                public List<Vertex> vertices;
            }

            public class Vertex
            {
                public Vector3 position;
                public Vector3 normal;
                public Vector2 uv;
                public ushort[] boneNumber;
                public byte[] boneWeight;
                public byte edgeFlag;
            }
        }
    }
}