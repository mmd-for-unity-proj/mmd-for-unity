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
        List<PMDBone> boneComponents = new List<PMDBone>();

        public void Read(List<Bone> bones)
        {
            this.bones = bones;

            gameObjects = GameObject();
            boneTransforms = Transform();
            boneComponents = PMDBoneComponent();
            Parent();
        }

        public List<Transform> BoneTransforms { get { return boneTransforms; } }
        public List<GameObject> GameObjects { get { return gameObjects; } }

        List<GameObject> GameObject()
        {
            var objects = new List<GameObject>(bones.Count);

            for (int i = 0; i < objects.Count; ++i)
            {
                var gameObject = new GameObject(bones[i].name);
                objects.Add(gameObject);
            }

            return objects;
        }

        List<Transform> Transform()
        {
            var transforms = new Transform[bones.Count];

            for (int i = 0; i < bones.Count; ++i)
            {
                var transform = gameObjects[i].transform;

                UnityEngine.Vector3 position;
                position.x = bones[i].position.x;
                position.y = bones[i].position.y;
                position.z = bones[i].position.z;
                transform.position = position;

                transforms[i] = transform;
            }

            return new List<Transform>(transforms);
        }

        void Parent()
        {
            for (int i = 0; i < boneTransforms.Count; ++i)
            {
                if (bones[i].parentBoneIndex == 0xFFFF) continue;

                var parent = boneTransforms[bones[i].parentBoneIndex];
                boneTransforms[i].parent = parent;
            }
        }

        List<PMDBone> PMDBoneComponent()
        {
            List<PMDBone> boneComponents = new List<PMDBone>(bones.Count);

            for (int i = 0; i < bones.Count; ++i)
            {
                var component = new PMDBone();
                component.BoneType = (BoneType)bones[i].boneType;
                component.InfluencedIKBone = gameObjects[bones[i].ikBoneIndex];
                component.TailBone = gameObjects[bones[i].tailBoneIndex];

                gameObjects[i].AddComponent<PMDBone>();

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
