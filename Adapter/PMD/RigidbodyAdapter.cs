using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class RigidbodyAdapter
    {
        public List<GameObject> Rigidbodies { get; set; }
        public List<Rigidbody> RigidbodyComponents { get; set; }
        public List<MMD.Engine.MMDPhysics> MMDPhysics { get; set; }
        
        ColliderAdapter colliderAdapter = new ColliderAdapter();

        public List<Collider> Colliders { get { return colliderAdapter.Colliders; } set { colliderAdapter.Colliders = value; } }
        public List<PhysicMaterial> PhysicMaterials { get { return colliderAdapter.PhysicMaterials; } set { colliderAdapter.PhysicMaterials = value; } }

        void SettingRigidbody(MMD.Format.PMD.Rigidbody rigidbody, Rigidbody component, GameObject gameObject)
        {
            // 物理系数の設定
            component.mass = rigidbody.weight;
            component.drag = rigidbody.positionDiminish;
            component.angularDrag = rigidbody.rotationDiminish;

            // 位置の設定
            var transform = gameObject.transform;
            transform.position = new Vector3(rigidbody.position.x, rigidbody.position.y, rigidbody.position.z);
            transform.rotation = Quaternion.Euler(rigidbody.rotation.x, rigidbody.rotation.y, rigidbody.rotation.z);
        }

        void CreateRigidbodyObjects(List<MMD.Format.PMD.Rigidbody> rigids, GameObject[] bones)
        {
            for (int i = 0; i < rigids.Count; ++i)
            {
                var bone = bones[rigids[i].boneIndex];
                var rigid = new GameObject("r" + rigids[i].name);
                var rigidComponent = rigid.AddComponent<Rigidbody>();
                var mmdphysics = rigid.AddComponent<MMD.Engine.MMDPhysics>();

                Rigidbodies.Add(rigid);
                RigidbodyComponents.Add(rigidComponent);
                MMDPhysics.Add(mmdphysics);

                SettingRigidbody(rigids[i], rigidComponent, rigid);
            }
        }

        void SettingRigidbodyType(List<MMD.Format.PMD.Rigidbody> rigidbodies)
        {
            for (int i = 0;i < rigidbodies.Count; ++i)
            {
                switch (rigidbodies[i].rigidbodyType)
                {
                    case 0:
                        RigidbodyComponents[i].useGravity = false;
                        RigidbodyComponents[i].isKinematic = true;
                        break;

                    case 1:
                    case 2:
                        RigidbodyComponents[i].useGravity = true;
                        RigidbodyComponents[i].isKinematic = false;
                        break;
                }
            }
        }

        void ReadJoints(List<MMD.Format.PMD.Joint> joints)
        {

        }

        public void Read(MMD.Format.PMDFormat format, GameObject[] bones)
        {
            Rigidbodies = new List<GameObject>(format.Rigidbodies.Count);
            RigidbodyComponents = new List<Rigidbody>(format.Rigidbodies.Count);
            
            // 剛体の読み込み
            CreateRigidbodyObjects(format.Rigidbodies, bones);
            colliderAdapter.Read(format.Rigidbodies, bones);
            SettingRigidbodyType(format.Rigidbodies);

            // ジョイントの読み込み
            ReadJoints(format.Joints);
        }
    }
}
