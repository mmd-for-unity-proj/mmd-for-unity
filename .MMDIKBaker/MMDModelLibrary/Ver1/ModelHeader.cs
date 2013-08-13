using System.IO;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// モデルヘッダ情報を表すクラス
    /// </summary>
    public class ModelHeader
    {
        /// <summary>
        /// モデル名
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// モデルコメント
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// モデル名(英語、拡張)
        /// </summary>
        public string ModelNameEnglish { get; set; }
        /// <summary>
        /// モデルコメント(英語、拡張)
        /// </summary>
        public string CommentEnglish { get; set; }
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelHeader()
        {
            ModelName = "";
            Comment = "";
            ModelNameEnglish = null;
            CommentEnglish = null;
        }
        internal void Read(BinaryReader reader)
        {
            //モデル名
            ModelName = MMDModel1.GetString(reader.ReadBytes(20));
            //コメント
            Comment = MMDModel1.GetString(reader.ReadBytes(256));
        }

        internal void ReadExpantion(BinaryReader reader)
        {
            ModelNameEnglish = MMDModel1.GetString(reader.ReadBytes(20));
            CommentEnglish = MMDModel1.GetString(reader.ReadBytes(256));
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(MMDModel1.GetBytes(ModelName, 20));
            writer.Write(MMDModel1.GetBytes(Comment, 256));
        }

        internal void WriteExpantion(BinaryWriter writer)
        {
            writer.Write(MMDModel1.GetBytes(ModelNameEnglish, 20));
            writer.Write(MMDModel1.GetBytes(CommentEnglish, 256));
        }
    }
    
}
