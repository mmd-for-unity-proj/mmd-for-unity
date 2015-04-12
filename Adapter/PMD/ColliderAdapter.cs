using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class ColliderAdapter
    {
        public List<Collider> Colliders { get; set; }
        public List<PhysicMaterial> PhysicMaterials { get; set; }

        Collider AddSphereCollider(MMD.Format.PMD.Rigidbody mmdRigidbody, GameObject rigidbody)
        {
            var collider = rigidbody.AddComponent<SphereCollider>();
            collider.radius = mmdRigidbody.shape.x;
            return collider;
        }

        Collider AddBoxCollider(MMD.Format.PMD.Rigidbody mmdRigidbody, GameObject rigidbody)
        {
            var collider = rigidbody.AddComponent<BoxCollider>();
            collider.size = new Vector3(mmdRigidbody.shape.x, mmdRigidbody.shape.y, mmdRigidbody.shape.z);
            return collider;
        }

        Collider AddCapsuleCollider(MMD.Format.PMD.Rigidbody mmdRigidbody, GameObject rigidbody)
        {
            var collider = rigidbody.AddComponent<CapsuleCollider>();
            collider.radius = mmdRigidbody.shape.x;
            collider.height = mmdRigidbody.shape.y;
            return collider;
        }

        Collider AddCollider(MMD.Format.PMD.Rigidbody mmdRigidbody, GameObject rigidbody)
        {
            Collider retval = null;

            switch (mmdRigidbody.shapeType)
            {
                case 0:
                    retval = AddSphereCollider(mmdRigidbody, rigidbody);
                    break;
                case 1:
                    retval = AddBoxCollider(mmdRigidbody, rigidbody);
                    break;
                case 2:
                    retval = AddCapsuleCollider(mmdRigidbody, rigidbody);
                    break;
            }
            Colliders.Add(retval);
            return retval;
        }

        void AddPhysicMaterial(MMD.Format.PMD.Rigidbody rigidbody, Collider collider)
        {
            var material = new PhysicMaterial("p" + rigidbody.name);

            material.dynamicFriction = rigidbody.friction;
            material.staticFriction = rigidbody.friction;
            material.bounciness = rigidbody.recoil;

            PhysicMaterials.Add(material);
        }

        bool[] CreateTargetIndices(MMD.Format.PMD.Rigidbody rigidbody)
        {
            var targetIndices = new bool[16];
            for (int i = 0; i < targetIndices.Length; ++i)
                targetIndices[i] = ((0xFFFF - rigidbody.groupTarget) & (1 << i)) == 1;
            return targetIndices;
        }

        void CompareRigidbodyGroup(int ignoreIndex, List<MMD.Format.PMD.Rigidbody> rigidbodies, List<Collider> ignoreColliders)
        {
            for (int j = 0; j < rigidbodies.Count; ++j)
            {
                if (ignoreIndex == rigidbodies[j].groupIndex)
                    ignoreColliders.Add(Colliders[j]);
            }
        }

        void ConstructIgnoreColliders(MMD.Engine.MMDPhysics physic, MMD.Format.PMD.Rigidbody rigidbody, List<MMD.Format.PMD.Rigidbody> rigidbodies)
        {
            var targetIndices = CreateTargetIndices(rigidbody);

            var ignoreColliders = new List<Collider>(rigidbodies.Count);

            for (int i = 0; i < targetIndices.Length; ++i)
            {
                if (targetIndices[i])
                    CompareRigidbodyGroup(i, rigidbodies, ignoreColliders);
            }

            physic.ignoreColliders = ignoreColliders.ToArray();
        }

        public void Read(List<MMD.Format.PMD.Rigidbody> mmdRigidbodies, List<GameObject> rigidbodies, List<MMD.Engine.MMDPhysics> mmdPhysics)
        {
            PhysicMaterials = new List<PhysicMaterial>(mmdRigidbodies.Count);
            Colliders = new List<Collider>(mmdRigidbodies.Count);

            for (int i = 0; i < mmdRigidbodies.Count; ++i)
            {
                var collider = AddCollider(mmdRigidbodies[i], rigidbodies[i]);
                AddPhysicMaterial(mmdRigidbodies[i], collider);
            }
        }
    }
}
