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
    }
}
