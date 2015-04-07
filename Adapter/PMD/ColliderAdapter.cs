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

        Collider AddSphereCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {
            var collider = bone.AddComponent<SphereCollider>();
            collider.radius = rigidbody.shape.x;
            return collider;
        }

        Collider AddBoxCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {
            var collider = bone.AddComponent<BoxCollider>();
            collider.size = new Vector3(rigidbody.shape.x, rigidbody.shape.y, rigidbody.shape.z);
            return collider;
        }

        Collider AddCapsuleCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {
            var collider = bone.AddComponent<CapsuleCollider>();
            collider.radius = rigidbody.shape.x;
            collider.height = rigidbody.shape.y;
            return collider;
        }

        Collider AddCollider(MMD.Format.PMD.Rigidbody rigidbody, GameObject bone)
        {
            Collider retval = null;

            switch (rigidbody.shapeType)
            {
                case 0:
                    retval = AddSphereCollider(rigidbody, bone);
                    break;
                case 1:
                    retval = AddBoxCollider(rigidbody, bone);
                    break;
                case 2:
                    retval = AddCapsuleCollider(rigidbody, bone);
                    break;
            }
            Colliders.Add(retval);
            return retval;
        }

        void AddPhysicMaterial(MMD.Format.PMD.Rigidbody rigidbody, Collider collider)
        {
            var material = new PhysicMaterial("p" + rigidbody.name);

            PhysicMaterials.Add(material);
        }

        public void Read(List<MMD.Format.PMD.Rigidbody> rigidbodies, GameObject[] bones)
        {
            PhysicMaterials = new List<PhysicMaterial>(rigidbodies.Count);
            Colliders = new List<Collider>(rigidbodies.Count);

            for (int i = 0; i < bones.Length; ++i)
            {
                var collider = AddCollider(rigidbodies[i], bones[i]);
                AddPhysicMaterial(rigidbodies[i], collider);
            }
        }
    }
}
