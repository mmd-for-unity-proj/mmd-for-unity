using System;
using System.IO;
using MikuMikuDance.Motion.Motion2;

namespace MikuMikuDance.Motion
{
    /// <summary>
    /// MikuMikuDance(MMD)モーションの読み込みを行うFactory Class
    /// </summary>
    public static class MotionManager
    {
        /// <summary>
        /// ファイルからMMDモーションを読み込む
        /// </summary>
        /// <param name="filename">MMDモーションファイル</param>
        /// <param name="coordinate">変換先座標系</param>
        /// <returns>MMDモーションオブジェクト</returns>
        /// <param name="scale">スケーリング値</param>
        public static MMDMotion Read(string filename, CoordinateType coordinate, float scale=1.0f)
        {
            //フルパス取得
            filename = Path.GetFullPath(filename);
            //ファイルチェック
            if (!File.Exists(filename))
                throw new FileNotFoundException("MMDモーションファイル:" + filename + "が見つかりません");
            //戻り値用変数
            MMDMotion result = null;
            //ファイルリーダー
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fs);
                //マジック文字列
                string magic = MMDMotion2.GetString(reader.ReadBytes(30));
                if (magic.Substring(0, 20) != "Vocaloid Motion Data")
                    throw new FileLoadException("MMDモーションファイルではありません");
                //バージョン
                int version = Convert.ToInt32(magic.Substring(21));
                if (version == 2)
                    result = new MMDMotion2();
                else
                    throw new FileLoadException("version=" + version.ToString() + "モデルは対応していません");

                result.Read(reader, coordinate,scale);
                if (fs.Length != fs.Position)
                    Console.WriteLine("警告：ファイル末尾以降に不明データ?");
                fs.Close();
            }
            return result;
        }
        /// <summary>
        /// ファイルからMMDモーションを読み込む
        /// </summary>
        /// <param name="filename">MMDモーションファイル</param>
        /// <param name="scale">スケーリング値</param>
        /// <returns>MMDモーションオブジェクト</returns>
        public static MMDMotion Read(string filename, float scale=0.1f)
        {
            return Read(filename, CoordinateType.LeftHandedCoordinate, scale);
        }
        /// <summary>
        /// ファイルへの書き出し
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="motion">モーション</param>
        /// <param name="scale">スケーリング値</param>
        public static void Write(string filename, MMDMotion motion, float scale=1f)
        {
            //フルパス取得
            filename = Path.GetFullPath(filename);
            //ファイルリーダー
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                //マジック文字列
                if (motion is MMDMotion2)
                {
                    writer.Write(MMDMotion2.GetBytes("Vocaloid Motion Data 0002", 25));
                    writer.Write((byte)0);
                    writer.Write(MMDMotion2.GetBytes("JKLM", 4));
                }
                else
                    new NotImplementedException("その他のバーションは未作成");

                motion.Write(writer, scale);
                fs.Close();
            }
        }
    }
}
