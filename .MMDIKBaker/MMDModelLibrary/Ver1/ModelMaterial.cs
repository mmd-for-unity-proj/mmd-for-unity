using System;
using System.IO;
using DWORD = System.UInt32;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// モデルの材質
    /// </summary>
    public class ModelMaterial
    {
        /// <summary>
        /// rgbの減衰色
        /// </summary>
        public float[] DiffuseColor { get; private set; } // dr, dg, db // 減衰色
        /// <summary>
        /// α値
        /// </summary>
        public float Alpha { get; set; }
        /// <summary>
        /// 光沢
        /// </summary>
        public float Specularity { get; set; }
        /// <summary>
        /// 光沢色(rgb)
        /// </summary>
        public float[] SpecularColor { get; private set; } // sr, sg, sb // 光沢色
        /// <summary>
        /// 環境色(ambient)(rgb)
        /// </summary>
        public float[] MirrorColor { get; private set; } // mr, mg, mb // 環境色(ambient)
        /// <summary>
        /// 使用するトゥーンbmp番号
        /// </summary>
        /// <remarks>使用する場合は0から9までの番号。使用しない場合は0xFF</remarks>
        public byte ToonIndex { get; set; } // toon??.bmp // 0.bmp:0xFF, 1(01).bmp:0x00 ・・・ 10.bmp:0x09
        /// <summary>
        /// 輪郭、影
        /// </summary>
        public byte EdgeFlag { get; set; } // 輪郭、影
        /// <summary>
        /// 面頂点数
        /// </summary>
        public DWORD FaceVertCount { get; set; } // 面頂点数 // インデックスに変換する場合は、材質0から順に加算
        /// <summary>
        /// テクスチャファイル名
        /// </summary>
        public string TextureFileName { get; set; } //20byte分char テクスチャファイル名 // 20バイトぎりぎりまで使える(終端の0x00は無くても動く)
        /// <summary>
        /// スフィアマップファイル名
        /// </summary>
        public string SphereTextureFileName { get; set; }
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelMaterial()
        {
            DiffuseColor = new float[3];
            SpecularColor = new float[3];
            MirrorColor = new float[3];
            TextureFileName = "";
            SphereTextureFileName = "";
        }
        internal void Read(BinaryReader reader)
        {
            DiffuseColor = new float[3];
            SpecularColor = new float[3];
            MirrorColor = new float[3];
            for (int i = 0; i < DiffuseColor.Length; i++)
                DiffuseColor[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Alpha = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Specularity = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < SpecularColor.Length; i++)
                SpecularColor[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < MirrorColor.Length; i++)
                MirrorColor[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            ToonIndex = reader.ReadByte();
            EdgeFlag = reader.ReadByte();
            FaceVertCount = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            string FileName = MMDModel1.GetString(reader.ReadBytes(20));
            string[] FileNames = FileName.Split('*');
            TextureFileName = "";
            SphereTextureFileName = "";
            foreach (string s in FileNames)
            {
                string ext = Path.GetExtension(s).ToLower();
                if (ext == ".sph" || ext == ".spa")
                {
                    SphereTextureFileName = s.Trim();
                }
                else
                {
                    TextureFileName = s.Trim();
                }
            }
        }

        internal void Write(BinaryWriter writer)
        {
            for (int i = 0; i < DiffuseColor.Length; i++)
                writer.Write(DiffuseColor[i]);
            writer.Write(Alpha);
            writer.Write(Specularity);
            for (int i = 0; i < SpecularColor.Length; i++)
                writer.Write(SpecularColor[i]);
            for (int i = 0; i < MirrorColor.Length; i++)
                writer.Write(MirrorColor[i]);
            writer.Write(ToonIndex);
            writer.Write(EdgeFlag);
            writer.Write(FaceVertCount);
            string FileName = TextureFileName;
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = SphereTextureFileName;
            }
            else
            {
                if (!string.IsNullOrEmpty(SphereTextureFileName))
                {
                    FileName += "*" + SphereTextureFileName;
                }
            }
            writer.Write(MMDModel1.GetBytes(FileName, 20));
        }
    }
}
