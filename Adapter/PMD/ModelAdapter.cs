using System.Collections;

using UnityEngine;
using MMD.Format.Common;
using MMD.Format.PMD;

namespace MMD.Adapter.PMD
{
    public class ModelAdapter
    {
        public UnityEngine.Vector3[] Vertices(VertexList vertices)
        {
            var vectors = new UnityEngine.Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i].x = vertices[i].position.x;
                vectors[i].y = vertices[i].position.y;
                vectors[i].z = vertices[i].position.z;
            }

            return vectors;
        }

        public UnityEngine.Vector3[] Nolmals(VertexList vertices)
        {
            var vectors = new UnityEngine.Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i].x = vertices[i].normal.x;
                vectors[i].y = vertices[i].normal.y;
                vectors[i].z = vertices[i].normal.z;
            }

            return vectors;
        }

        public UnityEngine.Vector2[] UVs(VertexList vertices)
        {
            var vectors = new UnityEngine.Vector2[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i].x = vertices[i].uv.x;
                vectors[i].y = vertices[i].uv.y;
            }

            return vectors;
        }
    }
}