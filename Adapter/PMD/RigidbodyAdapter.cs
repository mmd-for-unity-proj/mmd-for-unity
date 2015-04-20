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

        GameObject[] bones;

        void SettingRigidbody(MMD.Format.PMD.Rigidbody rigidbody, Rigidbody component, GameObject refBone)
        {
            // 物理系数の設定
            component.mass = Mathf.Max(Mathf.Epsilon, rigidbody.weight);    // なんでコレやるんだっけな……
            component.drag = rigidbody.positionDiminish;
            component.angularDrag = rigidbody.rotationDiminish;

            // 位置の設定
            var transform = component.gameObject.transform;

            transform.position = MMD.Adapter.Utility.ToVector3(rigidbody.position);
            transform.rotation = MMD.Adapter.Utility.ToQuaternion(rigidbody.rotation);
        }

        void CreateRigidbodyObjects(List<MMD.Format.PMD.Rigidbody> rigids)
        {
            for (int i = 0; i < rigids.Count; ++i)
            {
                var rigid = new GameObject("r" + rigids[i].name);
                var rigidComponent = rigid.AddComponent<Rigidbody>();
                var mmdphysics = rigid.AddComponent<MMD.Engine.MMDPhysics>();

                Rigidbodies.Add(rigid);
                RigidbodyComponents.Add(rigidComponent);
                MMDPhysics.Add(mmdphysics);

                var bone = GetReferenceBone(rigids[i]);
                SettingRigidbody(rigids[i], rigidComponent, bone);
            }
        }

        void SetGravityAndKinematic(Rigidbody component, bool gravity, bool kinematic)
        {
            component.useGravity = gravity;
            component.isKinematic = kinematic;
        }

        void SettingRigidbodyTypeEach(MMD.Format.PMD.Rigidbody rigidbody, Rigidbody component, GameObject bone)
        {
            var componentTransform = component.gameObject.transform;

            switch (rigidbody.rigidbodyType)
            {
                case 0:     // ボーン追従
                    SetGravityAndKinematic(component, false, true);
                    componentTransform.parent = bone.transform;
                    break;

                case 1:     // 物理演算
                case 2:     // 位置合わせ
                    SetGravityAndKinematic(component, true, false);
                    componentTransform.parent = RigidbodyRoot.transform;
                    break;
            }
        }

        GameObject GetReferenceBone(MMD.Format.PMD.Rigidbody rigidbody)
        {
            int refBone = rigidbody.boneIndex;
            return refBone < 0xFFFF ? bones[refBone] : null;
        }

        void SettingRigidbodyType(List<MMD.Format.PMD.Rigidbody> rigidbodies)
        {
            for (int i = 0;i < rigidbodies.Count; ++i)
            {
                var bone = GetReferenceBone(rigidbodies[i]);

                SettingRigidbodyTypeEach(rigidbodies[i], RigidbodyComponents[i], bone);
            }
        }

        public void Read(MMD.Format.PMDFormat format, GameObject[] bones)
        {
            Rigidbodies = new List<GameObject>(format.Rigidbodies.Count);
            RigidbodyComponents = new List<Rigidbody>(format.Rigidbodies.Count);
            MMDPhysics = new List<Engine.MMDPhysics>(format.Rigidbodies.Count);
            RigidbodyRoot = new GameObject("Rigidbodies");
            this.bones = bones;
            
            // 剛体だけ作成する
            CreateRigidbodyObjects(format.Rigidbodies);

            // 当たり判定の設定
            colliderAdapter.Read(format.Rigidbodies, Rigidbodies, MMDPhysics);

            // 剛体の種類ごとに
            SettingRigidbodyType(format.Rigidbodies);
        }
    }
}
