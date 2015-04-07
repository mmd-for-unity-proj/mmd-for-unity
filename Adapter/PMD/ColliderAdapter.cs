using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class ColliderAdapter
    {
        void AddSphereCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {

        }

        void AddBoxCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {

        }

        void AddCapsuleCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {

        }

        void AddCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {
            switch (rigidbody.shapeType)
            {
                case 0:
                    bone.AddComponent<SphereCollider>();
                    break;
                case 1:
                    bone.AddComponent<BoxCollider>();
                    break;
                case 2:
                    bone.AddComponent<CapsuleCollider>();
                    break;
            }
        }

        public void Read(List<MMD.Format.PMD.Rigidbody> rigidbodies, GameObject[] bones)
        {
            for (int i = 0; i < bones.Length; ++i)
            {

            }
        }
    }
}
