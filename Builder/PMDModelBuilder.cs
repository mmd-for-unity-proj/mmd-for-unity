using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MMD.Adapter.PMD;
using UnityEngine;
using MMD.Component;

namespace MMD.Builder.PMD
{
    public class Physics
    {
        public string name;
        public PhysicMaterial material;

        public Physics(string name, PhysicMaterial material)
        {
            this.name = name;
            this.material = material;
        }
    }

    public class ModelBuilder
    {
        public SkinnedMeshRenderer Renderer { get; set; }

        public Mesh Mesh { get; set; }

        public Material[] Materials { get; set; }

        public Texture[] Textures { get; set; }

        public GameObject[] Bones { get; set; }

        public GameObject[] Rigidbodies { get; set; }
        
        PhysicMaterial[] PhysicMaterials { get; set; }

        public List<Physics> Physics { get; set; }

        public GameObject RigidbodyRoot { get; set; }

        public GameObject RootBone { get; set; }

        public ModelBuilder(SkinnedMeshRenderer renderer)
        {
            Mesh = new UnityEngine.Mesh();
            Renderer = renderer;
            Renderer.sharedMesh = Mesh;
        }

        public void Read(MMD.Format.PMDFormat format, Shader shader)
        {
            // メッシュの参照
            var modelAdapter = new ModelAdapter();
            modelAdapter.Read(format);
            Mesh = modelAdapter.Mesh;

            // ボーンの参照
            var boneAdapter = new BoneAdapter();
            boneAdapter.Read(format.Bones);
            Renderer.bones = boneAdapter.BoneTransforms.ToArray();
            Bones = boneAdapter.GameObjects.ToArray();
            RootBone = boneAdapter.RootBone;

            // ウェイトの参照
            Mesh.boneWeights = boneAdapter.Weights(format.Vertices);

            // マテリアルの参照
            var materialAdapter = new MaterialAdapter();
            materialAdapter.Read(shader, format);

            Materials = materialAdapter.Materials.ToArray();
            Textures = materialAdapter.Textures.ToArray();

            // 剛体の参照
            var rigidbodyAdapter = new RigidbodyAdapter();
            rigidbodyAdapter.Read(format, Bones);
            Rigidbodies = rigidbodyAdapter.Rigidbodies.ToArray();
            PhysicMaterials = rigidbodyAdapter.PhysicMaterials.ToArray();
            RigidbodyRoot = rigidbodyAdapter.RigidbodyRoot;

            var rigidbodyComponents = rigidbodyAdapter.RigidbodyComponents;

            // ジョイントの参照
            var jointAdapter = new JointAdapter();
            jointAdapter.Read(format.Joints, rigidbodyComponents, Bones);

            // 物理マテリアルの名前をどうにかする
            Physics = new List<Physics>(PhysicMaterials.Length);
            for (int i = 0; i < PhysicMaterials.Length; ++i)
            {
                Physics.Add(new Physics(format.Rigidbodies[i].name, PhysicMaterials[i]));
            }
        }
    }
}
