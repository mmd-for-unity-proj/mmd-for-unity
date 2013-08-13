using System;
using System.IO;
using WORD = System.UInt16;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// IKボーン
    /// </summary>
    public class ModelIK
    {
        /// <summary>
        /// IKボーン番号
        /// </summary>
        public WORD IKBoneIndex { get; set; } // IKボーン番号
        /// <summary>
        /// IKターゲットボーン番号
        /// </summary>
        /// <remarks>IKボーンが最初に接続するボーン</remarks>
        public WORD IKTargetBoneIndex { get; set; } // IKターゲットボーン番号 // IKボーンが最初に接続するボーン
        //byte ik_chain_length;//読み込んでるが、ik_child_bone_indexで参照可のため、メンバにしない
        /// <summary>
        /// 再帰演算回数
        /// </summary>
        /// <remarks>IK値1</remarks>
        public WORD Iterations { get; set; } // 再帰演算回数 // IK値1
        /// <summary>
        /// 一回のIK計算での角度制限
        /// </summary>
        /// <remarks>IK値2</remarks>
        public float AngleLimit { get; set; } // IKの影響度 // IK値2
        /// <summary>
        /// IK影響下のボーン番号リスト
        /// </summary>
        public WORD[] IKChildBoneIndex { get; set; } // IK影響下のボーン番号-サイズはik_chain_length

        internal void Read(BinaryReader reader)
        {
            IKBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            IKTargetBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            byte ik_chain_length = reader.ReadByte();
            Iterations = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            AngleLimit = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            IKChildBoneIndex = new WORD[ik_chain_length];
            for (int i = 0; i < ik_chain_length; i++)
                IKChildBoneIndex[i] = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(IKBoneIndex);
            writer.Write(IKTargetBoneIndex);
            writer.Write((byte)IKChildBoneIndex.Length);
            writer.Write(Iterations);
            writer.Write(AngleLimit);
            for (int i = 0; i < IKChildBoneIndex.Length; i++)
                writer.Write(IKChildBoneIndex[i]);
        }
    }
}
