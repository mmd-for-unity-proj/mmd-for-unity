using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Misc;

namespace MMDIKBakerLibrary.Motion
{
    static class MotionConverter
    {
        internal static MMDMotion Convert(MikuMikuDance.Motion.Motion2.MMDMotion2 input)
        {
            MMDMotion result = new MMDMotion();
            //ボーンモーションデータの変換
            MMDBoneKeyFrame[] BoneFrames = new MMDBoneKeyFrame[input.Motions.LongLength];
            for (long i = 0; i < input.Motions.LongLength; i++)
            {
                BoneFrames[i] = new MMDBoneKeyFrame();
                BoneFrames[i].BoneName = input.Motions[i].BoneName;
                BoneFrames[i].FrameNo = input.Motions[i].FrameNo;

                BoneFrames[i].Curve = new BezierCurve[4];
                for (int j = 0; j < BoneFrames[i].Curve.Length; j++)
                {
                    BezierCurve curve = new BezierCurve();
                    curve.v1 = new Vector2((float)input.Motions[i].Interpolation[0][0][j] / 128f, (float)input.Motions[i].Interpolation[0][1][j] / 128f);
                    curve.v2 = new Vector2((float)input.Motions[i].Interpolation[0][2][j] / 128f, (float)input.Motions[i].Interpolation[0][3][j] / 128f);
                    BoneFrames[i].Curve[j] = curve;
                }
                BoneFrames[i].Scales = new Vector3(1, 1, 1);
                BoneFrames[i].Location = new Vector3((decimal)input.Motions[i].Location[0], (decimal)input.Motions[i].Location[1], (decimal)input.Motions[i].Location[2]);
                BoneFrames[i].Quatanion = new Quaternion((decimal)input.Motions[i].Quatanion[0], (decimal)input.Motions[i].Quatanion[1], (decimal)input.Motions[i].Quatanion[2], (decimal)input.Motions[i].Quatanion[3]);
                BoneFrames[i].Quatanion.Normalize();
            }
            result.BoneFrames = MotionHelper.SplitBoneMotion(BoneFrames);
            //表情モーションの変換
            MMDFaceKeyFrame[] FaceFrames = new MMDFaceKeyFrame[input.FaceMotions.LongLength];
            for (long i = 0; i < input.FaceMotions.Length; i++)
            {
                FaceFrames[i] = new MMDFaceKeyFrame();
                FaceFrames[i].Rate = input.FaceMotions[i].Rate;
                FaceFrames[i].FaceName = input.FaceMotions[i].FaceName;
                FaceFrames[i].FrameNo = input.FaceMotions[i].FrameNo;
                float temp = input.FaceMotions[i].FrameNo;
            }
            result.FaceFrames = MotionHelper.SplitFaceMotion(FaceFrames);
            //カメラモーションは無視(使わんので)
            //ライトモーションは無視(使わんので)
            //変換したデータを返却
            return result;
        }

        public static MikuMikuDance.Motion.Motion2.MMDMotion2 Convert(MMDMotion input, string modelName)
        {
            MikuMikuDance.Motion.Motion2.MMDMotion2 result = new MikuMikuDance.Motion.Motion2.MMDMotion2();
            result.ModelName = modelName;
            result.Coordinate = MikuMikuDance.Motion.CoordinateType.RightHandedCoordinate;
            //ボーンモーションデータの変換
            MMDBoneKeyFrame[] BoneFrames= MotionHelper.ImplodeBoneMotion(input.BoneFrames);
            MikuMikuDance.Motion.Motion2.MotionData[] BoneMotionData = new MikuMikuDance.Motion.Motion2.MotionData[BoneFrames.LongLength];
            for (long i = 0; i < BoneFrames.LongLength; i++)
            {
                BoneMotionData[i] = new MikuMikuDance.Motion.Motion2.MotionData();
                BoneMotionData[i].BoneName = BoneFrames[i].BoneName;
                BoneMotionData[i].FrameNo = BoneFrames[i].FrameNo;

                for (int j = 0; j < BoneFrames[i].Curve.Length; j++)
                {
                    BoneMotionData[i].Interpolation[0][0][j]=(byte)MathHelper.Clamp(BoneFrames[i].Curve[j].v1.X*128f,0,255);
                    BoneMotionData[i].Interpolation[0][1][j]=(byte)MathHelper.Clamp(BoneFrames[i].Curve[j].v1.Y*128f,0,255);
                    BoneMotionData[i].Interpolation[0][2][j]=(byte)MathHelper.Clamp(BoneFrames[i].Curve[j].v2.X*128f,0,255);
                    BoneMotionData[i].Interpolation[0][3][j]=(byte)MathHelper.Clamp(BoneFrames[i].Curve[j].v2.Y*128f,0,255);
                }

                BoneMotionData[i].Location[0] = (float)BoneFrames[i].Location.X;
                BoneMotionData[i].Location[1] = (float)BoneFrames[i].Location.Y;
                BoneMotionData[i].Location[2] = (float)BoneFrames[i].Location.Z;

                BoneFrames[i].Quatanion.Normalize();
                BoneMotionData[i].Quatanion[0] = (float)BoneFrames[i].Quatanion.X;
                BoneMotionData[i].Quatanion[1] = (float)BoneFrames[i].Quatanion.Y;
                BoneMotionData[i].Quatanion[2] = (float)BoneFrames[i].Quatanion.Z;
                BoneMotionData[i].Quatanion[3] = (float)BoneFrames[i].Quatanion.W;
            }
            result.Motions = BoneMotionData;
            //表情モーションの変換
            MMDFaceKeyFrame[] FaceFrames = MotionHelper.ImplodeFaceMotion(input.FaceFrames);
            MikuMikuDance.Motion.Motion2.FaceMotionData[] FaceMotionData = new MikuMikuDance.Motion.Motion2.FaceMotionData[FaceFrames.LongLength];
            for (long i = 0; i < FaceFrames.LongLength; i++)
            {
                FaceMotionData[i] = new MikuMikuDance.Motion.Motion2.FaceMotionData();
                FaceMotionData[i].Rate = FaceFrames[i].Rate;
                FaceMotionData[i].FaceName = FaceFrames[i].FaceName;
                FaceMotionData[i].FrameNo = FaceFrames[i].FrameNo;
            }
            result.FaceMotions = FaceMotionData;
            //カメラモーションは無視(使わんので)
            result.CameraMotions = new MikuMikuDance.Motion.Motion2.CameraMotionData[0];
            //ライトモーションは無視(使わんので)
            result.LightMotions = new MikuMikuDance.Motion.Motion2.LightMotionData[0];

            return result;
        }
    }
}
