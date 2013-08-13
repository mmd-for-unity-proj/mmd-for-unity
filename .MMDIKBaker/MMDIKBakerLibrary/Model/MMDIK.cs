using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Model
{
    class MMDIK
    {
        /// <summary>
        /// 目標位置となるボーン
        /// </summary>
        public MMDBone IKBone { get; internal set; }
        /// <summary>
        /// エフェクタとなるボーン
        /// </summary>
        public MMDBone IKTargetBone { get; internal set; }
        /// <summary>
        /// 再帰演算回数
        /// </summary>
        public readonly UInt16 Iteration;
        /// <summary>
        /// IKの影響度
        /// </summary>
        public readonly float ControlWeight;
        /// <summary>
        /// IK影響下のボーン
        /// </summary>
        public List<MMDBone> IKChildBones { get; internal set; }

        internal int IKBoneIndex;
        internal int IKTargetBoneIndex;
        internal List<int> ikChildBoneIndex;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ikBoneIndex">IKボーンとなるボーン</param>
        /// <param name="iktargetindex">エフェクタとなるボーン</param>
        /// <param name="iteration">再帰演算回数</param>
        /// <param name="controlWeight">IKの影響度</param>
        /// <param name="ikChildBones">IK影響下のボーン</param>
        public MMDIK(int ikBoneIndex, int iktargetindex, UInt16 iteration, float controlWeight, List<int> ikChildBones)
        {
            IKBoneIndex = ikBoneIndex;
            IKTargetBoneIndex = iktargetindex;
            Iteration = iteration;
            ControlWeight = controlWeight;
            ikChildBoneIndex = ikChildBones;
        }
    }
}
