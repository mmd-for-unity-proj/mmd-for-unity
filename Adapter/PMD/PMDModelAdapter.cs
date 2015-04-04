using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MMD.Format.Common;
using MMD.Format.PMD;

namespace MMD.Adapter.PMD
{
    public class PMDModelAdapter
    {
        public static UnityEngine.Vector3[] Vertices(List<MMD.Format.PMD.Vertex> vertices)
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

        public static UnityEngine.Vector3[] Nolmals(List<MMD.Format.PMD.Vertex> vertices)
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

        public static UnityEngine.Vector2[] UVs(List<MMD.Format.PMD.Vertex> vertices)
        {
            var vectors = new UnityEngine.Vector2[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i].x = vertices[i].uv.x;
                vectors[i].y = vertices[i].uv.y;
            }

            return vectors;
        }

        public static int[] Triangles(List<Face> faces)
        {
            int[] trinagles = new int[faces.Count * 3];

            for (int i = 0; i < faces.Count; ++i)
            {
                int index = i * 3;
                var face = faces[i];
                trinagles[index] = face[0];
                trinagles[++index] = face[1];
                trinagles[++index] = face[2];
            }

            return trinagles;
        }
    }
}