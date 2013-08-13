using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Model;
using MMDIKBakerLibrary.Motion;
using MMDMotion2 = MikuMikuDance.Motion.Motion2.MMDMotion2;
using MMDModel1 = MikuMikuDance.Model.Ver1.MMDModel1;
namespace MMDIKBakerLibrary
{
    public class IKBaker
    {
        public static MMDMotion2 bake(MMDMotion2 motion, MMDModel1 model)
        {
            //ボーン取得
            MMDBoneManager boneManager = ModelConverter.BuildBoneManager(model);
            //ベイク前のモーションとベイク後のモーションを準備
            MMDMotion beforeMotion = MotionConverter.Convert(motion);
            MMDMotion afterMotion = CreateAfterMotionPrototype(beforeMotion, boneManager);
            //アニメーションプレイヤーを作成
            AnimationPlayer player = new AnimationPlayer(boneManager);
            player.SetMotion(beforeMotion);
            //ベイクしていく
            uint frameNo = 0;
            bool ExitFlag = false;
            while (!ExitFlag)
            {
                ExitFlag = !player.Update();
                //ボーンのグローバル行列更新
                boneManager.CalcGlobalTransform();
                //IK更新
                boneManager.CalcIK();
                //ベイク
                boneManager.bake(frameNo, afterMotion);
                ++frameNo;
            }
            //元のMMDMotion2に直して返却
            return MotionConverter.Convert(afterMotion, motion.ModelName);
        }

        private static MMDMotion CreateAfterMotionPrototype(MMDMotion beforeMotion, MMDBoneManager boneManager)
        {
            //ベイク対象外の情報をコピー
            MMDMotion result = new MMDMotion();
            //表情は参照をコピーしておく
            result.FaceFrames = beforeMotion.FaceFrames;
            //ボーンは必要分だけ参照をコピー
            result.BoneFrames = new Dictionary<string, List<MMDBoneKeyFrame>>();
            foreach (KeyValuePair<string, List<MMDBoneKeyFrame>> boneSet in beforeMotion.BoneFrames)
            {
                if (!boneManager.IsUnderIK(boneSet.Key))
                {
                    result.BoneFrames.Add(boneSet.Key, boneSet.Value);
                }
                else
                {
                    result.BoneFrames.Add(boneSet.Key, new List<MMDBoneKeyFrame>());
                }
            }
            return result;
        }
    }
}
