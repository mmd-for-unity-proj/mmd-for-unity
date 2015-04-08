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

        void SettingData(MMD.Format.PMD.Joint mmdJoint, ConfigurableJoint joint)
        {
            // ジョイントの位置
            var position = new Vector3(mmdJoint.position.x, mmdJoint.position.y, mmdJoint.position.z);
            joint.anchor = position - joint.transform.position;

            // ジョイントの回転
            joint.axis = MMD.Adapter.Utility.ToQuaternion(mmdJoint.rotation) * Vector3.right;
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
