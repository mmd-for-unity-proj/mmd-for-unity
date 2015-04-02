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
                public uint VertexCount { get; set; }
                public List<Vertex> Vertices { get; set; }
            }

            public class Vertex
            {
                public Vector3 Position { get; set; }
                public Vector3 Normal { get; set; }
                public Vector2 UV { get; set; }
                
            }
        }
    }
}