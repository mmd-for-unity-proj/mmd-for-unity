using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Misc;

namespace MMDIKBakerLibrary.Model
{
    static class ModelConverter
    {
        public static MMDBoneManager BuildBoneManager(MikuMikuDance.Model.Ver1.MMDModel1 model)
        {
            List<MMDBone> bones;
            List<MMDIK> iks;
            bones = new List<MMDBone>();
            iks = new List<MMDIK>();
            Matrix[] absPoses = new Matrix[model.Bones.LongLength];
            //各ボーンの絶対座標を計算
            for (long i = 0; i < model.Bones.LongLength; ++i)
            {
                Matrix.CreateTranslation((decimal)model.Bones[i].BoneHeadPos[0],
                                (decimal)model.Bones[i].BoneHeadPos[1],
                                (decimal)model.Bones[i].BoneHeadPos[2],
                                out absPoses[i]);
            }
            for (long i = 0; i < model.Bones.LongLength; ++i)
            {
                Matrix localMatrix;
                if (model.Bones[i].ParentBoneIndex != 0xffff)
                {
                    Matrix parentInv;
                    Matrix.Invert(ref absPoses[model.Bones[i].ParentBoneIndex], out parentInv);
                    Matrix.Multiply(ref parentInv, ref absPoses[i], out localMatrix);
                }
                else
                {
                    localMatrix = absPoses[i];
                }
                SQTTransform bindPose = SQTTransform.FromMatrix(localMatrix);
                Matrix inverseBindPose;
                Matrix.Invert(ref absPoses[i], out inverseBindPose);
                bones.Add(new MMDBone(model.Bones[i].BoneName, bindPose, inverseBindPose, model.Bones[i].ParentBoneIndex));
            }
            for (long i = 0; i < model.IKs.LongLength; ++i)
            {
                List<int> ikChildBones = new List<int>();
                foreach (var ikc in model.IKs[i].IKChildBoneIndex)
                    ikChildBones.Add(ikc);
                iks.Add(new MMDIK(model.IKs[i].IKBoneIndex, model.IKs[i].IKTargetBoneIndex, model.IKs[i].Iterations, model.IKs[i].AngleLimit, ikChildBones));
            }
            //ボーンインデックス→ボーンオブジェクト化
            IKSetup(iks, bones);
            return new MMDBoneManager(bones, iks);
        }
        /// <summary>
        /// ボーンインデックス→ボーンオブジェクト化
        /// </summary>
        public static void IKSetup(List<MMDIK> iks, List<MMDBone> bones)
        {
            foreach (var ik in iks)
            {
                ik.IKBone = bones[ik.IKBoneIndex];
                ik.IKTargetBone = bones[ik.IKTargetBoneIndex];
                List<MMDBone> ikchilds = new List<MMDBone>();
                foreach (var ikci in ik.ikChildBoneIndex)
                    ikchilds.Add(bones[ikci]);
                ik.IKChildBones = new List<MMDBone>(ikchilds);
            }
        }
    }
}
