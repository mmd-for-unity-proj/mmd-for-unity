using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class RigidbodyAdapter
    {
        List<GameObject> rigidbodies;
        List<Rigidbody> rigidbodyComponents;
        ColliderAdapter colliderAdapter = new ColliderAdapter();
        float scale;

        void CreateRigidbodyObjects(List<MMD.Format.PMD.Rigidbody> rigids, GameObject[] bones)
        {
            for (int i = 0; i < rigids.Count; ++i)
            {
                var bone = bones[rigids[i].boneIndex];
                var rigid = new GameObject("r" + rigids[i].name);
                var rigidComponent = rigid.AddComponent<Rigidbody>();
                rigidbodies.Add(rigid);
                rigidbodyComponents.Add(rigidComponent);
            }
        }

        void ReadJoints(List<MMD.Format.PMD.Joint> joints)
        {

        }

        public void Read(MMD.Format.PMDFormat format, GameObject[] bones)
        {
            this.scale = scale;
            rigidbodies = new List<GameObject>(format.Rigidbodies.Count);
            rigidbodyComponents = new List<Rigidbody>(format.Rigidbodies.Count);
            
            // 剛体の読み込み
            CreateRigidbodyObjects(format.Rigidbodies, bones);
            colliderAdapter.Read(format.Rigidbodies, bones);

            // ジョイントの読み込み
            ReadJoints(format.Joints);
        }
    }
}
