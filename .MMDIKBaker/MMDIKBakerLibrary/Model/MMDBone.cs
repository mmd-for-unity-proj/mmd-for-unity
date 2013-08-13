using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Misc;

namespace MMDIKBakerLibrary.Model
{
    class MMDBone
    {
        /// <summary>
        /// ボーンの名前
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// ボーンのバインドポーズ行列
        /// </summary>
        public readonly SQTTransform BindPose;
        /// <summary>
        /// ボーンのバインドポーズ逆行列
        /// </summary>
        /// <remarks>変更しないこと……</remarks>
        public Matrix InverseBindPose;
        /// <summary>
        /// ボーンの親のボーンのインデックス情報
        /// </summary>
        /// <remarks>親が無い場合は-1</remarks>
        public readonly int SkeletonHierarchy;

        /// <summary>
        /// ボーンのローカル変換行列
        /// </summary>
        /// <remarks>アニメーション再生時にはアニメーションにより上書きされる</remarks>
        public SQTTransform LocalTransform;

        /// <summary>
        /// ボーンのグローバル変換行列
        /// </summary>
        /// <remarks>毎フレームごとにLocalTransformから計算され、上書きされる</remarks>
        public Matrix GlobalTransform;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="bindPose">バインドポーズ</param>
        /// <param name="inverseBindPose">逆バインドポーズ</param>
        /// <param name="skeletonHierarchy">親ボーン番号</param>
        public MMDBone(string name, SQTTransform bindPose, Matrix inverseBindPose, int skeletonHierarchy)
        {
            Name = name;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            SkeletonHierarchy = skeletonHierarchy;
            LocalTransform = bindPose;
            GlobalTransform = Matrix.Identity;
        }
    }
}
