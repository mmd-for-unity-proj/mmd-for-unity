using System.IO;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// ボーン枠用の枠名
    /// </summary>
    public class ModelBoneDispName
    {
        /// <summary>
        /// ボーン枠用の枠名
        /// </summary>
        public string BoneDispName { get; set; }//ボーン枠用枠名
        /// <summary>
        /// ボーン枠用の枠名(英語、拡張)
        /// </summary>
        public string BoneDispNameEnglish { get; set; }//ボーン枠用枠名(英語、拡張)
        internal void Read(BinaryReader reader)
        {
            BoneDispName = MMDModel1.GetString(reader.ReadBytes(50));
            BoneDispNameEnglish = null;
        }
        internal void ReadExpantion(BinaryReader reader)
        {
            BoneDispNameEnglish = MMDModel1.GetString(reader.ReadBytes(50));
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(MMDModel1.GetBytes(BoneDispName, 50));
        }

        internal void WriteExpantion(BinaryWriter writer)
        {
            writer.Write(MMDModel1.GetBytes(BoneDispNameEnglish, 50));
        }
    }
}
