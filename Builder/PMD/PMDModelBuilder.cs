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
        Material[] materials;
        Texture[] textures;

        public PMDModelBuilder()
        {
            renderer.sharedMesh = mesh;
        }

        public void Read(MMD.Format.PMDFormat format, Shader shader)
        {
            // メッシュの参照
            var modelAdapter = new ModelAdapter(format);
            mesh = modelAdapter.Mesh;

            // ボーンの参照
            var boneAdapter = new BoneAdapter(format.Bones);
            renderer.bones = boneAdapter.BoneTransforms.ToArray();

            // ウェイトの参照
            mesh.boneWeights = boneAdapter.Weights(format.Vertices);

            // マテリアルの参照
            var materialAdapter = new MaterialAdapter(shader, format);
            materials = materialAdapter.Materials.ToArray();
            textures = materialAdapter.Textures.ToArray();
        }
    }
}
