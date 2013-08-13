using System;
using System.IO;
using DWORD = System.UInt32;

namespace MikuMikuDance.Motion.Motion2
{
    /// <summary>
    /// ボーンモーションデータ
    /// </summary>
    public class MotionData
    {
        /// <summary>
        /// ボーン名
        /// </summary>
        public string BoneName { get; set; }//[15];
        /// <summary>
        /// フレーム番号
        /// </summary>
        public DWORD FrameNo { get; set; }
        /// <summary>
        /// 位置ベクトル
        /// </summary>
        public float[] Location { get; protected set; }
        /// <summary>
        /// クォータニオン
        /// </summary>
        public float[] Quatanion { get; protected set; }
        /// <summary>
        /// 補完データ
        /// </summary>
        public byte[][][] Interpolation { get; protected set; }
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public MotionData()
        {
            Location = new float[3];
            Quatanion = new float[4];
            Interpolation = new byte[4][][];
            for (int i = 0; i < 4; i++)
            {
                Interpolation[i] = new byte[4][];
                for (int j = 0; j < 4; j++)
                    Interpolation[i][j] = new byte[4];
            }
        }
        internal void Read(BinaryReader reader, float CoordZ, float scale)
        {
            BoneName = MMDMotion2.GetString(reader.ReadBytes(15));
            FrameNo = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            for (int i = 0; i < 3; i++)
                Location[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            for (int i = 0; i < 4; i++)
                Quatanion[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        Interpolation[i][j][k] = reader.ReadByte();
            Location[2] *= CoordZ;
            Quatanion[0] *= CoordZ;
            Quatanion[1] *= CoordZ;
        }

        internal void Write(BinaryWriter writer, float CoordZ, float scale)
        {
            writer.Write(MMDMotion2.GetBytes(BoneName, 15));
            writer.Write((DWORD)FrameNo);
            writer.Write((Single)Location[0]*scale);
            writer.Write((Single)Location[1]*scale);
            writer.Write((Single)Location[2] * CoordZ*scale);
            writer.Write((Single)Quatanion[0] * CoordZ);
            writer.Write((Single)Quatanion[1] * CoordZ);
            writer.Write((Single)Quatanion[2]);
            writer.Write((Single)Quatanion[3]);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        writer.Write((byte)Interpolation[i][j][k]);
        }
    }
}
