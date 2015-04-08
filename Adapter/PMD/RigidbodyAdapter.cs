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
        public GameObject RigidbodyRoot { get; set; }

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

        void SettingRigidbodyType(List<MMD.Format.PMD.Rigidbody> rigidbodies, GameObject[] bones)
        {
            for (int i = 0;i < rigidbodies.Count; ++i)
            {
                var targetBoneIndex = rigidbodies[i].boneIndex;

                switch (rigidbodies[i].rigidbodyType)
                {
                    case 0:     // ボーン追従
                        RigidbodyComponents[i].useGravity = false;
                        RigidbodyComponents[i].isKinematic = true;
                        Rigidbodies[i].transform.parent = bones[targetBoneIndex].transform; // 一度ボーンと接続する
                        bones[targetBoneIndex].transform.parent = RigidbodyRoot.transform;  // 接続したボーンをルートに出して，グローバルにする
                        break;

                    case 1:     // 物理演算
                    case 2:     // 位置合わせ
                        RigidbodyComponents[i].useGravity = true;
                        RigidbodyComponents[i].isKinematic = false;
                        Rigidbodies[i].transform.parent = RigidbodyRoot.transform;          // 剛体をルートと接続する
                        bones[targetBoneIndex].transform.parent = Rigidbodies[i].transform; // ボーンは剛体に制御される側なので，剛体をボーンの親にする
                        break;
                }
            }
        }

        public void Read(MMD.Format.PMDFormat format, GameObject[] bones)
        {
            Rigidbodies = new List<GameObject>(format.Rigidbodies.Count);
            RigidbodyComponents = new List<Rigidbody>(format.Rigidbodies.Count);
            MMDPhysics = new List<Engine.MMDPhysics>(format.Rigidbodies.Count);
            RigidbodyRoot = new GameObject("Rigidbodies");
            
            // 剛体の読み込み
            CreateRigidbodyObjects(format.Rigidbodies, bones);
            colliderAdapter.Read(format.Rigidbodies, bones, MMDPhysics);
            SettingRigidbodyType(format.Rigidbodies, bones);
        }
    }
}
