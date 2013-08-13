using System;
using System.IO;
using WORD = System.UInt16;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// ボーン
    /// </summary>
    public class ModelBone
    {
        /// <summary>
        /// ボーン名
        /// </summary>
        public string BoneName { get; set; } //20byte分char ボーン名
        /// <summary>
        /// 親ボーン番号
        /// </summary>
        /// <remarks>無い場合は0xFFFFを代入</remarks>
        public WORD ParentBoneIndex { get; set; } // 親ボーン番号(ない場合は0xFFFF)
        /// <summary>
        /// tail位置のボーン番号
        /// </summary>
        /// <remarks>チェーン末端の場合は0xFFFF。 親：子は1：多なので、主に位置決め用</remarks>
        public WORD TailPosBoneIndex { get; set; } // tail位置のボーン番号(チェーン末端の場合は0xFFFF) // 親：子は1：多なので、主に位置決め用
        /// <summary>
        /// ボーンの種類
        /// </summary>
        /// <remarks>0:回転 1:回転と移動 2:IK 3:不明 4:IK影響下 5:回転影響下 6:IK接続先 7:非表示 8:捻り 9:回転運動</remarks>
        public byte BoneType { get; set; } // ボーンの種類
        /// <summary>
        /// IKボーン番号
        /// </summary>
        /// <remarks>影響IKボーン。ない場合は0</remarks>
        public WORD IKParentBoneIndex { get; set; } // IKボーン番号(影響IKボーン。ない場合は0)
        /// <summary>
        /// ボーンのヘッドの位置(x,y,z)
        /// </summary>
        public float[] BoneHeadPos { get; private set; } // x, y, z // ボーンのヘッドの位置
        /// <summary>
        /// ボーン名(英語、拡張)
        /// </summary>
        public string BoneNameEnglish { get; set; }////20byte分char ボーン名(英語、拡張(無い場合はnull))
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelBone()
        {
            BoneHeadPos = new float[3];
        }
        internal void Read(BinaryReader reader, float CoordZ, float scale)
        {
            BoneHeadPos = new float[3];
            BoneName = MMDModel1.GetString(reader.ReadBytes(20));
            ParentBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            TailPosBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            BoneType = reader.ReadByte();
            IKParentBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            for (int i = 0; i < BoneHeadPos.Length; i++)
                BoneHeadPos[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            //英名拡張はReadではnullにする(あるならReadEngilishで上書きされる)
            BoneNameEnglish = null;
            BoneHeadPos[2] = BoneHeadPos[2] * CoordZ;
        }

        //英名拡張分読み込み
        internal void ReadExpantion(BinaryReader reader)
        {
            BoneNameEnglish = MMDModel1.GetString(reader.ReadBytes(20));
        }

        internal void Write(BinaryWriter writer, float CoordZ, float scale)
        {
            BoneHeadPos[2] = BoneHeadPos[2] * CoordZ * scale;
            writer.Write(MMDModel1.GetBytes(BoneName, 20));
            writer.Write(ParentBoneIndex);
            writer.Write(TailPosBoneIndex);
            writer.Write(BoneType);
            writer.Write(IKParentBoneIndex);
            for (int i = 0; i < BoneHeadPos.Length; i++)
                writer.Write(BoneHeadPos[i]);
        }

        internal void WriteExpantion(BinaryWriter writer)
        {
            writer.Write(MMDModel1.GetBytes(BoneNameEnglish, 20));
        }
    }
}
