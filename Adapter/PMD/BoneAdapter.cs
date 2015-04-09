using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MMD.Format.Common;
using MMD.Format.PMD;
using MMD.Component;

namespace MMD.Adapter.PMD
{
    public class BoneAdapter
    {
        List<GameObject> gameObjects = new List<GameObject>();
        List<Bone> bones = new List<Bone>();
        List<Transform> boneTransforms = new List<Transform>();

        public void Read(List<Bone> bones)
        {
            this.bones = bones;
            RootBone = new GameObject("Bones");

            gameObjects = CreateGameObjects();
            boneTransforms = CreateTransforms();
            Parent();
        }

        public List<Transform> BoneTransforms { get { return boneTransforms; } }
        public List<GameObject> GameObjects { get { return gameObjects; } }
        public GameObject RootBone { get; set; }

        List<GameObject> CreateGameObjects()
        {
            var objects = new List<GameObject>(bones.Count);

            for (int i = 0; i < bones.Count; ++i)
            {
                var gameObject = new GameObject(bones[i].name);
                objects.Add(gameObject);
            }

            return objects;
        }

        List<Transform> CreateTransforms()
        {
            var transforms = new Transform[bones.Count];

            for (int i = 0; i < bones.Count; ++i)
            {
                var transform = gameObjects[i].transform;

                transform.position = MMD.Adapter.Utility.ToVector3(bones[i].position);

                transforms[i] = transform;
            }

            return new List<Transform>(transforms);
        }

        void Parent()
        {
            for (int i = 0; i < bones.Count; ++i)
            {
                // 親がいない場合はルートに繋ぐ
                int pindex = (int)bones[i].parentBoneIndex;
                if (pindex < 65535)
                    boneTransforms[i].parent = gameObjects[pindex].transform;
                else
                    boneTransforms[i].parent = RootBone.transform;
            }
        }

        List<MMDBone> PMDBoneComponent()
        {
            List<MMDBone> boneComponents = new List<MMDBone>(bones.Count);

            for (int i = 0; i < bones.Count; ++i)
            {
                var component = gameObjects[i].AddComponent<MMDBone>();
                component.BoneType = (BoneType)bones[i].boneType;
                component.InfluencedIKBone = gameObjects[bones[i].ikBoneIndex];
                component.TailBone = gameObjects[bones[i].tailBoneIndex];

                boneComponents.Add(component);
            }

            return boneComponents;
        }

        public BoneWeight[] Weights(List<Vertex> vertices)
        {
            var weights = new BoneWeight[vertices.Count];

            for (int i = 0; i < vertices.Count; ++i)
            {
                weights[i].boneIndex0 = vertices[i].boneNumber[0];
                weights[i].weight0 = vertices[i].boneWeight1;

                if (vertices[i].boneWeight1 < 100)
                {
                    weights[i].boneIndex1 = vertices[i].boneNumber[1];
                    weights[i].weight1 = vertices[i].boneWeight2;
                }
            }

            return weights;
        }
    }
}
