using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class JointAdapter
    {
        public List<ConfigurableJoint> Joints { get; set; }

        public JointAdapter()
        {

        }

        ConfigurableJoint AddComponent(int addComponentIndex, List<GameObject> bones)
        {
            var joint = bones[addComponentIndex].AddComponent<ConfigurableJoint>();
            Joints.Add(joint);
            return joint;
        }

        void SetReference(int connectIndex, ConfigurableJoint joint, List<Rigidbody> rigidbodies)
        {
            joint.connectedBody = rigidbodies[connectIndex];
        }

        ConfigurableJointMotion JudgeLock(float min, float max)
        {
            if (min == max && min == 0f)
                return ConfigurableJointMotion.Locked;
            return ConfigurableJointMotion.Free;
        }

        void TestingLock(MMD.Format.PMD.Joint mmdJoint, ConfigurableJoint joint)
        {
            joint.xMotion = JudgeLock(mmdJoint.constrainPosition1.x, mmdJoint.constrainPosition2.x);
            joint.yMotion = JudgeLock(mmdJoint.constrainPosition1.y, mmdJoint.constrainPosition2.y);
            joint.zMotion = JudgeLock(mmdJoint.constrainPosition1.z, mmdJoint.constrainPosition2.z);
            joint.angularXMotion = JudgeLock(mmdJoint.constrainRotation1.x, mmdJoint.constrainRotation2.x);
            joint.angularYMotion = JudgeLock(mmdJoint.constrainRotation1.y, mmdJoint.constrainRotation2.y);
            joint.angularZMotion = JudgeLock(mmdJoint.constrainRotation1.z, mmdJoint.constrainRotation2.z);
        }

        void AddLimitComponent(MMD.Format.PMD.Joint mmdJoint, ConfigurableJoint joint)
        {
            var limiter = joint.gameObject.AddComponent<MMD.Engine.MMDLimitter>();
            limiter.MinLimitMotion = MMD.Adapter.Utility.ToVector3(mmdJoint.constrainPosition1);
            limiter.MaxLimitMotion = MMD.Adapter.Utility.ToVector3(mmdJoint.constrainPosition2);
            limiter.MinLimitAngular = MMD.Adapter.Utility.ToVector3(mmdJoint.constrainRotation1);
            limiter.MaxLimitAngular = MMD.Adapter.Utility.ToVector3(mmdJoint.constrainRotation2);
        }

        void SettingData(MMD.Format.PMD.Joint mmdJoint, ConfigurableJoint joint)
        {
            // ジョイントの位置
            var position = new Vector3(mmdJoint.position.x, mmdJoint.position.y, mmdJoint.position.z);
            joint.anchor = position - joint.transform.position;

            // ジョイントの回転
            joint.axis = MMD.Adapter.Utility.ToQuaternion(mmdJoint.rotation) * Vector3.right;

            /// ばね係数をここに設定する
            ///
            ///

            TestingLock(mmdJoint, joint);
            AddLimitComponent(mmdJoint, joint);
        }

        public void Read(List<MMD.Format.PMD.Joint> joints, List<Rigidbody> rigidbodies, List<GameObject> bones)
        {
            Joints = new List<ConfigurableJoint>(joints.Count);

            for (int i = 0; i < joints.Count; ++i)
            {
                var joint = AddComponent((int)joints[i].rigidbodyB, bones);
                SetReference((int)joints[i].rigidbodyA, joint, rigidbodies);
                SettingData(joints[i], joint);
            }
        }
    }
}
