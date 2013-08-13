using System;
using System.IO;
using WORD = System.UInt16;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// ボーン枠用表示データ
    /// </summary>
    public class ModelBoneDisp
    {
        /// <summary>
        /// 枠用ボーン番号
        /// </summary>
        public WORD BoneIndex { get; set; } // 枠用ボーン番号
        /// <summary>
        /// 表示枠用番号
        /// </summary>
        public byte BoneDispFrameIndex { get; set; }  // 表示枠番号

        internal void Read(BinaryReader reader)
        {
            BoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            BoneDispFrameIndex = reader.ReadByte();
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(BoneIndex);
            writer.Write(BoneDispFrameIndex);
        }
    }
}
