using System;
using System.IO;
using System.Text;
using DWORD = System.UInt32;

namespace MikuMikuDance.Motion.Motion2
{
    /// <summary>
    /// MMDのモーションデータVer2を読み込むためのクラス
    /// </summary>
    /// <remarks>
    /// ver2はモーションのヘッダがVocaloid Motion Data 0002と書かれてるから。
    /// 正式なバージョンは知らんｗ
    /// </remarks>
    public class MMDMotion2 : MMDMotion
    {
        internal MotionData[] m_Motions;
        /// <summary>
        /// モーションで使用するモデル名
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// ボーンモーションリスト
        /// </summary>
        public MotionData[] Motions { get { return m_Motions; }  set { m_Motions = value; } }
        /// <summary>
        /// フェイスモーションリスト
        /// </summary>
        public FaceMotionData[] FaceMotions { get; set; }
        /// <summary>
        /// カメラモーションリスト
        /// </summary>
        public CameraMotionData[] CameraMotions { get; set; }
        /// <summary>
        /// ライトモーションリスト
        /// </summary>
        public LightMotionData[] LightMotions { get; set; }
        //members and properties...
        /// <summary>
        /// Version番号。MMDMotionから継承されます
        /// </summary>
        public int Version
        {
            get { return 2; }
        }
        /// <summary>
        /// 保持しているデータの座標き
        /// </summary>
        public CoordinateType Coordinate { get; set; }
        //座標変換用ヘルパ関数
        float CoordZ { get { return (float)Coordinate; } }

        //methods...
        /// <summary>
        /// Read関数
        /// </summary>
        /// <remarks>この関数はMotionManagerから呼び出される。呼び出し時にはマジック文字とバージョン番号が読まれた状態で渡される</remarks>
        /// <param name="reader">ヘッダ読み込み済みのBinaryReader</param>
        /// <param name="coordinate">変換先座標系</param>
        /// <param name="scale">スケーリング値</param>
        public void Read(BinaryReader reader, CoordinateType coordinate, float scale)
        {
            Coordinate = coordinate;//座標系セット
            //モデル名読み込み
            ModelName = GetString(reader.ReadBytes(20));
            //ボーンモーションデータ読み込み
            DWORD motion_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Motions = new MotionData[motion_count];
            for (long i = 0; i < Motions.LongLength; i++)
            {
                Motions[i] = new MotionData();
                Motions[i].Read(reader, CoordZ, scale);
            }
            //フェイスモーションデータ読み込み
            DWORD face_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            FaceMotions = new FaceMotionData[face_count];
            for (long i = 0; i < FaceMotions.LongLength; i++)
            {
                FaceMotions[i] = new FaceMotionData();
                FaceMotions[i].Read(reader);
            }
            //カメラモーション読み込み
            DWORD camera_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            CameraMotions = new CameraMotionData[camera_count];
            for (long i = 0; i < CameraMotions.LongLength; i++)
            {
                CameraMotions[i] = new CameraMotionData();
                CameraMotions[i].Read(reader, CoordZ);
            }
            //ライトモーション読み込み
            DWORD light_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            LightMotions = new LightMotionData[light_count];
            for (long i = 0; i < LightMotions.LongLength; i++)
            {
                LightMotions[i] = new LightMotionData();
                LightMotions[i].Read(reader, CoordZ);
            }
        }
        /// <summary>
        /// ヘッダ以外の書き出し
        /// </summary>
        /// <param name="writer">ファイル書き出し用のBinaryWriter</param>
        /// <param name="scale">スケーリング値</param>
        public void Write(BinaryWriter writer, float scale)
        {
            //モデル名読み込み
            writer.Write(GetBytes(ModelName, 20));
            //ボーンモーションデータ読み込み
            writer.Write((DWORD)Motions.LongLength);
            for (long i = 0; i < Motions.LongLength; i++)
            {
                Motions[i].Write(writer, CoordZ, scale);
            }
            //フェイスモーションデータ読み込み
            writer.Write((DWORD)FaceMotions.LongLength);
            for (long i = 0; i < FaceMotions.LongLength; i++)
            {
                FaceMotions[i].Write(writer);
            }
            //カメラモーション読み込み
            writer.Write((DWORD)CameraMotions.LongLength);
            for (long i = 0; i < CameraMotions.LongLength; i++)
            {
                CameraMotions[i].Write(writer, CoordZ);
            }
            //ライトモーション読み込み
            writer.Write((DWORD)LightMotions.LongLength);
            for (long i = 0; i < LightMotions.LongLength; i++)
            {
                LightMotions[i].Write(writer, CoordZ);
            }
        }
#if false
        /// <summary>
        /// スケーリング
        /// </summary>
        /// <param name="ScaleFactor">拡大倍率</param>
        public void Scale(float ScaleFactor)
        {
            if (ScaleFactor <= 0)
                throw new ApplicationException("ScaleFactorは正の実数である必要があります。");
            //ボーン
            for (long i = 0; i < Motions.LongLength; i++)
            {
                for (int j = 0; j < Motions[i].Location.Length; j++)
                {
                    Motions[i].Location[j] = Motions[i].Location[j] * ScaleFactor;
                }
                
            }
            //カメラ
            for (long i = 0; i < CameraMotions.LongLength; i++)
            {
                CameraMotions[i].Length=CameraMotions[i].Length*ScaleFactor;
                for (int j = 0; j < CameraMotions[i].Location.Length; j++)
                    CameraMotions[i].Location[j] = CameraMotions[i].Location[j] * ScaleFactor;
            }
            //ライトモーション
            for (long i = 0; i < LightMotions.LongLength; i++)
            {
                for (int j = 0; j < LightMotions[i].Location.Length; j++)
                    LightMotions[i].Location[j] = LightMotions[i].Location[j] * ScaleFactor;
            }
        }
#endif
        //internal statics...
        internal static Encoding encoder = Encoding.GetEncoding("shift-jis");
        internal static string GetString(byte[] bytes)
        {
            int i;
            for (i = 0; i < bytes.Length; i++)
                if (bytes[i] == 0)
                    break;
            if (i < bytes.Length)
                return encoder.GetString(bytes, 0, i);
            return encoder.GetString(bytes);
        }
        internal static byte[] GetBytes(string input, long size)
        {
            byte[] result = new byte[size];
            for (long i = 0; i < size; i++)
                result[i] = 0;
            if (input == "")
                return result;
            byte[] strs = encoder.GetBytes(input);
            for (long i = 0; i < strs.LongLength; i++)
                if (i < result.LongLength)
                    result[i] = strs[i];
            if (result.LongLength <= strs.LongLength)
                return result;
            result[strs.LongLength] = 0;
            for (long i = strs.LongLength + 1; i < result.Length; i++)
                result[i] = 0xFD;//何故かこれが挿入されているのでこれを挿入
            return result;
        }
    }
}
