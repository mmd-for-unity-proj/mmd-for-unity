using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Model;

namespace MMDIKBakerLibrary.Misc
{
    /// <summary>
    /// CCD-IKソルバー
    /// </summary>
    /// <remarks>Cyclic-Coordinate-Descent(CCD)法によるIK計算クラス</remarks>
    class CCDSolver : IIKSolver
    {
        const double errToleranceSq = 1.0e-8f;

        /// <summary>
        /// IKのソルブ
        /// </summary>
        /// <param name="ik">対象IK</param>
        /// <param name="BoneManager">ボーンマネージャ</param>
        /// <returns>呼び出し側でUpdateGlobalをもう一度呼ぶ場合はtrue</returns>
        public bool Solve(MMDIK ik, MMDBoneManager BoneManager)
        {
#if SlimDX
            Vector4 localTargetPos = Vector4.Zero;
            Vector4 localEffectorPos = Vector4.Zero;
#else
            Vector3 localTargetPos = Vector3.Zero;
            Vector3 localEffectorPos = Vector3.Zero;
#endif
            //エフェクタとなるボーンを取得
            MMDBone effector = ik.IKTargetBone;
            //IK対象のボーンのGlobalを更新(別のIK影響下のボーンからIKチェインが出ている場合があるので)
            Matrix local;
            for (int i = ik.IKChildBones.Count - 1; i >= 0; --i)
            {//順番に親子関係になっている。(Processorでチェックかけてある
                //GlobalTransformを仮更新
                int parentBone = ik.IKChildBones[i].SkeletonHierarchy;
                ik.IKChildBones[i].LocalTransform.CreateMatrix(out local);
                Matrix.Multiply(ref local, ref BoneManager[parentBone].GlobalTransform, out ik.IKChildBones[i].GlobalTransform);
            }
            effector.LocalTransform.CreateMatrix(out local);
            Matrix.Multiply(ref local, ref BoneManager[effector.SkeletonHierarchy].GlobalTransform, out effector.GlobalTransform);

            //ターゲット位置の取得
            Vector3 targetPos;
            Matrix.GetTranslation(ref ik.IKBone.GlobalTransform, out targetPos);

            //最大ループ回数分ループ
            for (int it = 0; it < ik.Iteration; ++it)
            {
                for (int nodeIndex = 0; nodeIndex < ik.IKChildBones.Count; ++nodeIndex)
                {//子ノードを子から順番に……
                    MMDBone node = ik.IKChildBones[nodeIndex];
                    //エフェクタの位置
                    Vector3 effectorPos;
                    Matrix.GetTranslation(ref effector.GlobalTransform, out effectorPos);
                    // 注目ノードの位置の取得
                    Vector3 jointPos;
                    Matrix.GetTranslation(ref node.GlobalTransform, out jointPos);

                    // ワールド座標系から注目ノードの局所座標系への変換
                    Matrix invCoord;
                    Matrix.Invert(ref node.GlobalTransform, out invCoord);
                    // 各ベクトルの座標変換を行い、検索中のボーンi基準の座標系にする
                    // (1) 注目ノード→エフェクタ位置へのベクトル(a)(注目ノード)
                    Vector3.Transform(ref effectorPos, ref invCoord, out localEffectorPos);
                    // (2) 基準関節i→目標位置へのベクトル(b)(ボーンi基準座標系)
                    Vector3.Transform(ref targetPos, ref invCoord, out localTargetPos);
#if SlimDX
                    //念のため……
                    // (1) 基準関節→エフェクタ位置への方向ベクトル
                    Vector3 basis2Effector = Vector3.Normalize(new Vector3(localEffectorPos.X, localEffectorPos.Y, localEffectorPos.Z));
                    // (2) 基準関節→目標位置への方向ベクトル
                    Vector3 basis2Target = Vector3.Normalize(new Vector3(localTargetPos.X, localTargetPos.Y, localTargetPos.Z));
#else
                    // (1) 基準関節→エフェクタ位置への方向ベクトル
                    Vector3 basis2Effector = Vector3.Normalize(localEffectorPos);
                    // (2) 基準関節→目標位置への方向ベクトル
                    Vector3 basis2Target = Vector3.Normalize(localTargetPos);
#endif

                    // 回転角
                    float rotationDotProduct = (float)Vector3.Dot(basis2Effector, basis2Target);
                    float rotationAngle = (float)Math.Acos(rotationDotProduct);

                    //回転量制限をかける
                    if (rotationAngle > MathHelper.Pi * ik.ControlWeight * (nodeIndex + 1))
                        rotationAngle = MathHelper.Pi * ik.ControlWeight * (nodeIndex + 1);
                    if (rotationAngle < -MathHelper.Pi * ik.ControlWeight * (nodeIndex + 1))
                        rotationAngle = -MathHelper.Pi * ik.ControlWeight * (nodeIndex + 1);

                    // 回転軸
                    Vector3 rotationAxis = Vector3.Cross(basis2Effector, basis2Target);
                    BoneManager.IKLimitter.Adjust(node.Name, ref rotationAxis);
                    rotationAxis.Normalize();
                    
                    if (!float.IsNaN(rotationAngle) && rotationAngle > 1.0e-3f && !rotationAxis.NaN)
                    {
                        // 関節回転量の補正
                        Quaternion subRot = Quaternion.CreateFromAxisAngle(rotationAxis, (decimal)rotationAngle);
                        Quaternion.Multiply(ref subRot, ref node.LocalTransform.Rotation, out node.LocalTransform.Rotation);
                        BoneManager.IKLimitter.Adjust(node);
                        //関係ノードのグローバル座標更新
                        for (int i = nodeIndex; i >= 0; --i)
                        {//順番に親子関係になっている。(Processorでチェックかけてある
                            //GlobalTransformを仮更新
                            int parentBone = ik.IKChildBones[i].SkeletonHierarchy;
                            ik.IKChildBones[i].LocalTransform.CreateMatrix(out local);
                            Matrix.Multiply(ref local, ref BoneManager[parentBone].GlobalTransform, out ik.IKChildBones[i].GlobalTransform);
                        }
                        effector.LocalTransform.CreateMatrix(out local);
                        Matrix.Multiply(ref local, ref BoneManager[effector.SkeletonHierarchy].GlobalTransform, out effector.GlobalTransform);
                    }
                }
            }
            return true;//UpdateGlobalをもう一度呼ぶ
            //IKチェインにぶら下がってるIK影響外のボーンを更新するため。
        }


    }
}
