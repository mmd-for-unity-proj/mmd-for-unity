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

        List<Rigidbody> rigidbodies;

        ConfigurableJoint AddComponent(int addComponentIndex)
        {
            var joint = rigidbodies[addComponentIndex].gameObject.AddComponent<ConfigurableJoint>();
            Joints.Add(joint);
            return joint;
        }

        void ConnectBody(int connectIndex, ConfigurableJoint joint, List<Rigidbody> rigidbodies)
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

        JointDrive SetPositionDrive(float spring, bool angular = true)
        {
            var drive = new JointDrive();
            drive.mode = JointDriveMode.Position;
            spring = Mathf.Abs(spring);
            if (angular)
                drive.positionSpring = Mathf.Clamp(spring, 0, 180);
            return drive;
        }

        void SettingData(MMD.Format.PMD.Joint mmdJoint, MMD.Format.PMD.Rigidbody mmdRigid, ConfigurableJoint joint)
        {
            // ジョイントの位置
            var position = new Vector3(mmdJoint.position.x, mmdJoint.position.y, mmdJoint.position.z);
            joint.anchor = position - joint.transform.position;

            // ジョイントの回転
            joint.axis = MMD.Adapter.Utility.ToQuaternion(mmdJoint.rotation) * Vector3.right;

            // ばね係数をここに設定する
            joint.xDrive = SetPositionDrive(mmdJoint.springPositoin.x, false);
            joint.yDrive = SetPositionDrive(mmdJoint.springPositoin.y, false);
            joint.zDrive = SetPositionDrive(mmdJoint.springPositoin.z, false);

            // ばね回転の設定
            joint.angularXDrive = SetPositionDrive(mmdJoint.springRotation.x * Mathf.Rad2Deg);
            float average = (Mathf.Abs(mmdJoint.springRotation.y) + Mathf.Abs(mmdJoint.springRotation.z)) * 0.5f;   // YZ軸が同一なので，平均してClampする
            joint.angularYZDrive = SetPositionDrive(average * Mathf.Rad2Deg);

            TestingLock(mmdJoint, joint);
            //AddLimitComponent(mmdJoint, joint);
        }

        public void Read(Format.PMDFormat format, List<Rigidbody> rigidbodies, GameObject[] bones)
        {
            var joints = format.Joints;
            var mmdrigids = format.Rigidbodies;
            
            Joints = new List<ConfigurableJoint>(joints.Count);
            this.rigidbodies = rigidbodies;

            for (int i = 0; i < joints.Count; ++i)
            {
                var joint = AddComponent((int)joints[i].rigidbodyB);

                ConnectBody((int)joints[i].rigidbodyA, joint, rigidbodies);

                SettingData(joints[i], mmdrigids[(int)joints[i].rigidbodyB], joint);
            }
        }
    }
}
