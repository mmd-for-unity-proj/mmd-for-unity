using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MMD.Adapter.PMD;
using UnityEngine;
using MMD.Component;

namespace MMD.Builder.PMD
{
    public class PMDModelBuilder
    {
        SkinnedMeshRenderer renderer = new SkinnedMeshRenderer();
        Mesh mesh = new Mesh();

        public PMDModelBuilder()
        {
            renderer.sharedMesh = mesh;
        }

        public void Read(MMD.Format.PMDFormat format)
        {
            mesh.vertices = ModelAdapter.Vertices(format.Vertices);
            mesh.uv = ModelAdapter.UVs(format.Vertices);
            mesh.normals = ModelAdapter.Nolmals(format.Vertices);
            mesh.SetTriangles(ModelAdapter.Triangles(format.Faces), 0);

            var boneAdapter = new BoneAdapter(format.Bones);
            renderer.bones = boneAdapter.BoneTransforms;
            renderer.rootBone = renderer.bones[0];

            // 明日はウェイトやります
        }
    }
}
