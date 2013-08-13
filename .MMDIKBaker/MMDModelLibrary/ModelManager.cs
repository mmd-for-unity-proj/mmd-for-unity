using System;
using System.Collections.Generic;
using System.IO;
using MikuMikuDance.Model.Ver1;

namespace MikuMikuDance.Model
{
    /// <summary>
    /// MikuMikuDance(MMD)モデルの入出力の管理を行うFactory Class
    /// </summary>
    public static class ModelManager
    {
        /// <summary>
        /// ライブラリユーザー拡張用
        /// </summary>
        /// <remarks>ここにMMDモデルを継承したクラスと使用するバージョン番号を登録すると、既存クラスの代わりに、登録したクラスが使用される</remarks>
        public static readonly Dictionary<float, Type> OriginalObjects = new Dictionary<float, Type>();
        /// <summary>
        /// ファイルからMMDモデルを読み込む
        /// </summary>
        /// <param name="filename">MMDモデルファイル</param>
        /// <param name="coordinate">変換先座標系</param>
        /// <param name="scale">スケーリング値</param>
        /// <returns>MMDモデルオブジェクト</returns>
        ///  <remarks>MMDの座標系は右手座標系です</remarks>
        public static MMDModel Read(string filename, CoordinateType coordinate, float scale=1f)
        {
            //フルパス取得
            filename = Path.GetFullPath(filename);
            //ファイルチェック
            if (!File.Exists(filename))
                throw new FileNotFoundException("MMDモデルファイル:" + filename + "が見つかりません");
            //戻り値用変数
            MMDModel result = null;
            //ファイルリーダー
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fs);
                //マジック文字列
                string magic = MMDModel1.encoder.GetString(reader.ReadBytes(3));
                if (magic != "Pmd")
                    throw new FileLoadException("MMDモデルファイルではありません");
                //バージョン
                float version = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                if (OriginalObjects.ContainsKey(version) &&
                    OriginalObjects[version].BaseType == typeof(MMDModel))
                {//このバージョンで使用し、利用可能型
                    result = (MMDModel)OriginalObjects[version].InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, null);
                }
                else
                {
                    if (version == 1.0)
                        result = new MMDModel1();
                    else
                        throw new FileLoadException("version=" + version.ToString() + "モデルは対応していません");
                }
                result.Read(reader, coordinate, scale);
                if (fs.Length != fs.Position)
                    Console.WriteLine("警告：ファイル末尾以降に不明データ?");
                fs.Close();
            }
            return result;
        }
        /// <summary>
        /// ファイルにMMDモデルを書きだす
        /// </summary>
        /// <param name="filename">書きだすファイル名</param>
        /// <param name="model">モデルオブジェクト</param>
        /// <param name="scale">スケーリング値</param>
        public static void Write(string filename, MMDModel model, float scale = 1f)
        {
            //フルパス取得
            filename = Path.GetFullPath(filename);
            //ファイルリーダー
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(fs);

                //マジック文字列
                writer.Write(MMDModel1.encoder.GetBytes("Pmd"));
                //バージョン
                writer.Write(model.Version);
                //中身の書きだし
                try
                {
                    model.Write(writer, scale);
                }
                catch (NullReferenceException e)
                {
                    throw new ArgumentNullException("modelの中の変数がnull", e);
                }
                fs.Close();
            }
        }
        /// <summary>
        /// ファイルからMMDモデルを読み込む
        /// </summary>
        /// <param name="filename">MMDモデルファイル</param>
        /// <returns>MMDモデルオブジェクト</returns>
        /// <remarks>MMDの座標系は右手座標系です</remarks>
        public static MMDModel Read(string filename)
        {
            return Read(filename, CoordinateType.LeftHandedCoordinate);
        }
    }
}
