using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MMD.Format.Common;
using MMD.Format.PMD;

namespace MMD.Adapter.PMD
{
    public class BoneAdapter
    {
        public static List<GameObject> BoneObjects(List<Bone> bones)
        {
            var objects = new List<GameObject>(bones.Count);

            for (int i = 0; i < objects.Count; ++i)
            {
                var gameObject = new GameObject(bones[i].name);
                objects.Add(gameObject);
            }

            return objects;
        }

        public static List<Transform> Transforms(List<GameObject> gameObjects, List<Bone> bones)
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

        public static void Parent(List<Transform> transforms, List<Bone> bones)
        {
            for (int i = 0; i < transforms.Count; ++i)
            {
                if (bones[i].parentBoneIndex == 0xFFFF) continue;

                var parent = transforms[bones[i].parentBoneIndex];
                transforms[i].parent = parent;
            }
        }
    }
}
