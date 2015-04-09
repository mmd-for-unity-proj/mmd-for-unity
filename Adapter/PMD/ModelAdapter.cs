using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MMD.Format.Common;
using MMD.Format.PMD;

namespace MMD.Adapter.PMD
{
    public class ModelAdapter
    {
        public Mesh Mesh { get; set; }

        public ModelAdapter()
        {
            Mesh = new Mesh();
        }

        public void Read(MMD.Format.PMDFormat format)
        {
            Mesh.vertices = Vertices(format.Vertices);
            Mesh.uv = UVs(format.Vertices);
            Mesh.normals = Nolmals(format.Vertices);
            SetTriangles(Mesh, format.Faces, format.Materials);
        }

        UnityEngine.Vector3[] Vertices(List<MMD.Format.PMD.Vertex> vertices)
        {
            var vectors = new UnityEngine.Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i] = MMD.Adapter.Utility.ToVector3(vertices[i].position);
            }

            return vectors;
        }

        UnityEngine.Vector3[] Nolmals(List<MMD.Format.PMD.Vertex> vertices)
        {
            var vectors = new UnityEngine.Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i] = MMD.Adapter.Utility.ToVector3(vertices[i].normal);
            }

            return vectors;
        }

        UnityEngine.Vector2[] UVs(List<MMD.Format.PMD.Vertex> vertices)
        {
            var vectors = new UnityEngine.Vector2[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                vectors[i].x = vertices[i].uv.x;
                vectors[i].y = vertices[i].uv.y;
            }

            return vectors;
        }

        void SetTriangles(Mesh mesh, List<Face> faces, List<MMD.Format.PMD.Material> materials)
        {
            // マテリアルごとにポリゴンを分ける
            mesh.subMeshCount = materials.Count;

            int total = 0;
            for (int submesh = 0; submesh < materials.Count; ++submesh)
            {
                int faceCount = (int)materials[submesh].assignedFaceConut;

                int[] indices = new int[faceCount * 3];

                for (int i = 0; i < faceCount; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        indices[i * 3 + j] = faces[total + i][j];
                    }
                }


                mesh.SetTriangles(indices, submesh);

                total += faceCount;
            }
            Debug.Log("f"+(faces.Count * 3).ToString());
            Debug.Log("t"+total.ToString());
        }
    }
}