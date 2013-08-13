using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Motion;
using MMDIKBakerLibrary.Misc;

namespace MMDIKBakerLibrary.Model
{
    class MMDBoneManager
    {
        public IIKSolver IKSolver = new CCDSolver();
        public IIKLimitter IKLimitter = new DefaltIKLimitter();
        private List<MMDBone> bones;
        private List<MMDIK> iks;
        private Dictionary<string, bool> ik_dict = new Dictionary<string, bool>();
        Dictionary<string, int> boneDic;
        /// <summary>
        /// ボーン取得
        /// </summary>
        /// <param name="index">ボーン番号</param>
        /// <returns>ボーンオブジェクト</returns>
        public MMDBone this[int index] { get { return bones[index]; } }
        /// <summary>
        /// ボーン取得
        /// </summary>
        /// <param name="key">ボーン名</param>
        /// <returns>ボーンオブジェクト</returns>
        public MMDBone this[string key] { get { return bones[boneDic[key]]; } }
        /// <summary>
        /// ボーン数
        /// </summary>
        public int Count { get { return bones.Count; } }

        public MMDBoneManager(List<MMDBone> bones, List<MMDIK> iks)
        {
            this.bones = bones;
            this.iks = iks;
            boneDic = new Dictionary<string, int>();
            for (int i = 0; i < bones.Count; i++)
            {
                boneDic.Add(bones[i].Name, i);
            }
            foreach (MMDBone bone in bones)
            {
                ik_dict[bone.Name] = false;
            }
            foreach (MMDIK ik in iks)
            {
                foreach (MMDBone ikchild in ik.IKChildBones)
                {
                    ik_dict[ikchild.Name] = true;
                }
            }
        }

        public bool IsUnderIK(string boneName)
        {
            bool result;
            if (!ik_dict.TryGetValue(boneName, out result))
            {
                return false;
            }
            return result;
        }
        /// <summary>
        /// グローバルトランスフォームの更新
        /// </summary>
        public virtual void CalcGlobalTransform()
        {
            bones[0].LocalTransform.CreateMatrix(out bones[0].GlobalTransform);
            for (int i = 1; i < bones.Count; ++i)
            {
                int parentBone = bones[i].SkeletonHierarchy;
                Matrix local;
                bones[i].LocalTransform.CreateMatrix(out local);
                if (parentBone > bones.Count)
                {
                    bones[i].GlobalTransform = local;
                }
                else
                {
                    Matrix.Multiply(ref local, ref bones[parentBone].GlobalTransform, out bones[i].GlobalTransform);
                }
            }
        }
        /// <summary>
        /// IK計算
        /// </summary>
        public virtual void CalcIK()
        {
            bool UpdateFlag = false;
            for (int i = 0; i < iks.Count; ++i)
            {
                if (IKSolver.Solve(iks[i], this))
                    UpdateFlag = true;
            }
            if (UpdateFlag)
                CalcGlobalTransform();
        }
        public void bake(uint frameNo, MMDMotion afterMotion)
        {
            foreach (KeyValuePair<string, bool> it in ik_dict)
            {
                if (it.Value && afterMotion.BoneFrames.ContainsKey(it.Key))
                {
                    Matrix globalTrans = this[it.Key].GlobalTransform;
                    Matrix parentTrans, invParentTrans;
                    if (this[it.Key].SkeletonHierarchy >= bones.Count)
                    {
                        parentTrans = Matrix.Identity;
                    }
                    else
                    {
                        parentTrans = this[this[it.Key].SkeletonHierarchy].GlobalTransform;
                    }
                    Matrix.Invert(ref parentTrans, out invParentTrans);
                    Matrix LocalTrans;
                    Matrix.Multiply(ref globalTrans, ref invParentTrans, out LocalTrans);
                    Matrix BindPose = this[it.Key].BindPose.CreateMatrix();
                    Matrix invBindPose;
                    Matrix.Invert(ref BindPose, out invBindPose);
                    Matrix subPose;
                    Vector3 scale, Location;
                    Quaternion quaternion;
                    Matrix.Multiply(ref LocalTrans, ref invBindPose, out subPose);
                    subPose.Decompose(out scale, out quaternion, out Location);
                    MMDBoneKeyFrame keyframe = new MMDBoneKeyFrame();
                    if (afterMotion.BoneFrames[it.Key].Count == 0 && frameNo > 0)
                    {
                        keyframe.FrameNo = 0;
                        keyframe.BoneName = it.Key;
                        keyframe.Curve = MathHelper.CreateIdentityCurve();
                        keyframe.Location = Location;
                        keyframe.Quatanion = quaternion;
                        keyframe.Scales = scale;
                        afterMotion.BoneFrames[it.Key].Add(keyframe);
                        keyframe = new MMDBoneKeyFrame();
                    }
                    keyframe.FrameNo = frameNo;
                    keyframe.BoneName = it.Key;
                    keyframe.Curve = MathHelper.CreateIdentityCurve();
                    keyframe.Location = Location;
                    keyframe.Quatanion = quaternion;
                    keyframe.Scales = scale;
                    afterMotion.BoneFrames[it.Key].Add(keyframe);
                }
            }


            
        }
    }
}
