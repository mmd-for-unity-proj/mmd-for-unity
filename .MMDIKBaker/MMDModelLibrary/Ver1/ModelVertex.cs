using System;
using System.IO;
using WORD = System.UInt16;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// モデルの頂点
    /// </summary>
    public class ModelVertex
    {
        /// <summary>
        /// x,y,z座標
        /// </summary>
        public float[] Pos { get; private set; } // x, y, z // 座標
        /// <summary>
        /// x,y,zの法線ベクトル
        /// </summary>
        public float[] NormalVector { get; private set; } // nx, ny, nz // 法線ベクトル
        /// <summary>
        /// UV座標(頂点UV)
        /// </summary>
        public float[] UV { get; private set; } // u, v // UV座標 // MMDは頂点UV
        /// <summary>
        /// ボーン番号
        /// </summary>
        /// <remarks>番号は1または2。モデル変形(頂点移動)時に影響</remarks>
        public WORD[] BoneNum { get; private set; } // ボーン番号1、番号2 // モデル変形(頂点移動)時に影響
        /// <summary>
        /// 影響度
        /// </summary>
        /// <remarks>ボーン1に与える影響度。min:0 max:100。ボーン2への影響度は、(100 - BoneWeight)</remarks>
        public byte BoneWeight { get; private set; } // ボーン1に与える影響度 // min:0 max:100 // ボーン2への影響度は、(100 - bone_weight)
        /// <summary>
        /// エッジフラグ
        /// </summary>
        /// <remarks>0:通常、1:エッジ無効(エッジ(輪郭)が有効の場合)</remarks>
        public byte NonEdgeFlag { get; private set; } // 0:通常、1:エッジ無効 // エッジ(輪郭)が有効の場合
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelVertex()
        {
            Pos = new float[3];
            NormalVector = new float[3];
            UV = new float[2];
            BoneNum = new WORD[2];
            BoneWeight = 0;
            NonEdgeFlag = 0;
        }
        internal void Read(BinaryReader reader, float CoordZ, float scale)
        {
            //サイズ
            Pos = new float[3];
            NormalVector = new float[3];
            UV = new float[2];
            BoneNum = new WORD[2];
            for (int i = 0; i < Pos.Length; i++)
                Pos[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            for (int i = 0; i < NormalVector.Length; i++)
                NormalVector[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < UV.Length; i++)
                UV[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < BoneNum.Length; i++)
                BoneNum[i] = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            BoneWeight = reader.ReadByte();
            NonEdgeFlag = reader.ReadByte();
            Pos[2] = Pos[2] * CoordZ;
            NormalVector[2] = NormalVector[2] * CoordZ;
        }

        internal void Write(BinaryWriter writer, float CoordZ, float scale)
        {
            Pos[2] = Pos[2] * CoordZ;
            NormalVector[2] = NormalVector[2] * CoordZ*scale;
            for (int i = 0; i < Pos.Length; i++)
                writer.Write(Pos[i]);
            for (int i = 0; i < NormalVector.Length; i++)
                writer.Write(NormalVector[i]);
            for (int i = 0; i < UV.Length; i++)
                writer.Write(UV[i]);
            for (int i = 0; i < BoneNum.Length; i++)
                writer.Write(BoneNum[i]);
            writer.Write(BoneWeight);
            writer.Write(NonEdgeFlag);
        }

    }
}
