using System;
using System.IO;
using DWORD = System.UInt32;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// 表情用の頂点のデータ
    /// </summary>
    public class ModelSkinVertexData
    {
        
        //base時＝表情用の頂点の番号(頂点リストにある番号)
        //base以外=表情用の頂点の番号(baseの番号。skin_vert_index)
        /// <summary>
        /// 頂点番号
        /// </summary>
        /// <remarks>
        /// base時＝表情用の頂点の番号(頂点リストにある番号)
        /// base以外=表情用の頂点の番号(baseの番号。skin_vert_index)
        /// </remarks>
        public DWORD SkinVertIndex { get; set; }

        //base時=x, y, z // 表情用の頂点の座標(頂点自体の座標)
        //base以外=x, y, z // 表情用の頂点の座標オフセット値(baseに対するオフセット)
        /// <summary>
        /// 表情用の頂点の座標リスト
        /// </summary>
        /// <remarks>
        /// base時=x, y, z // 表情用の頂点の座標(頂点自体の座標)
        /// base以外=x, y, z // 表情用の頂点の座標オフセット値(baseに対するオフセット)
        /// </remarks>
        public float[] SkinVertPos { get; private set; } // 
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelSkinVertexData()
        {
            SkinVertPos = new float[3];
        }
        internal void Read(BinaryReader reader, float CoordZ, float scale)
        {
            SkinVertIndex = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            for (int i = 0; i < SkinVertPos.Length; i++)
                SkinVertPos[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0)* scale;
            SkinVertPos[2] *= CoordZ;
        }

        internal void Write(BinaryWriter writer, float CoordZ, float scale)
        {
            SkinVertPos[2] *= CoordZ;
            writer.Write(SkinVertIndex);
            for (int i = 0; i < SkinVertPos.Length; i++)
                writer.Write(SkinVertPos[i] * scale);
        }
    }
    /// <summary>
    /// 表情
    /// </summary>
    public class ModelSkin
    {
        /// <summary>
        /// 表情名
        /// </summary>
        public string SkinName { get; set; } //　表情名(char[20])
        //public DWORD skin_vert_count { get; set; } // 表情用の頂点数-SkinVertDatasのLengthで参照
        /// <summary>
        /// 表情の種類
        /// </summary>
        /// <remarks>0：base、1：まゆ、2：目、3：リップ、4：その他</remarks>
        public byte SkinType { get; set; } // 表情の種類(byte) // 0：base、1：まゆ、2：目、3：リップ、4：その他
        /// <summary>
        /// 表情用の頂点のデータリスト
        /// </summary>
        public ModelSkinVertexData[] SkinVertDatas { get; set; } // 表情用の頂点のデータ(16Bytes/vert) *skin_vert_count
        /// <summary>
        /// 表情名(英語、拡張)
        /// </summary>
        public string SkinNameEnglish { get; set; }//表示名(char[20]、英語)(拡張)
        internal void Read(BinaryReader reader, float CoordZ, float scale)
        {
            SkinName = MMDModel1.GetString(reader.ReadBytes(20));
            DWORD skin_vert_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            SkinType = reader.ReadByte();
            SkinVertDatas = new ModelSkinVertexData[skin_vert_count];
            for (int i = 0; i < SkinVertDatas.Length; i++)
            {
                SkinVertDatas[i] = new ModelSkinVertexData();
                SkinVertDatas[i].Read(reader, CoordZ, scale);
            }
            SkinNameEnglish = null;
        }
        internal void ReadExpantion(BinaryReader reader)
        {
            SkinNameEnglish = MMDModel1.GetString(reader.ReadBytes(20));
        }

        internal void Write(BinaryWriter writer, float CoordZ, float scale)
        {
            writer.Write(MMDModel1.GetBytes(SkinName,20));
            writer.Write((DWORD)SkinVertDatas.Length);
            writer.Write(SkinType);
            for (int i = 0; i < SkinVertDatas.Length; i++)
            {
                SkinVertDatas[i].Write(writer, CoordZ, scale);
            }
        }

        internal void WriteExpantion(BinaryWriter writer)
        {
            writer.Write(MMDModel1.GetBytes(SkinNameEnglish, 20));
        }
    }
}
