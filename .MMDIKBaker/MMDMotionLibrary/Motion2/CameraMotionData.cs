using System;
using System.IO;
using DWORD = System.UInt32;
using WORD = System.UInt16;

namespace MikuMikuDance.Motion.Motion2
{
    /// <summary>
    /// カメラモーションデータ
    /// </summary>
    public class CameraMotionData
    {
        /// <summary>
        /// フレーム番号
        /// </summary>
        public DWORD FrameNo { get; set; }
        /// <summary>
        /// 長さ
        /// </summary>
        public float Length { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public float[] Location { get; protected set; }
        /// <summary>
        /// 回転
        /// </summary>
        public float[] Rotate { get; protected set; }
        /// <summary>
        /// 補完データ
        /// </summary>
        public byte[][] Interpolation { get; protected set; }
        /// <summary>
        /// 視野角
        /// </summary>
        public WORD ViewingAngle { get; protected set; }
        /// <summary>
        /// 不明データ
        /// </summary>
        /// <remarks>だれか教えてくれ(´・ω・｀)</remarks>
        public byte[] Unknown { get; protected set; }
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public CameraMotionData()
        {
            Location = new float[3];
            Rotate = new float[3];
            Interpolation = new byte[6][];
            for (int i = 0; i < 6; i++)
                Interpolation[i] = new byte[4];
            Unknown = new byte[3];
        }

        internal void Read(BinaryReader reader, float CoordZ)
        {
            FrameNo = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Length = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < 3; i++)
                Location[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < 3; i++)
                Rotate[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 4; j++)
                    Interpolation[i][j] = reader.ReadByte();
            ViewingAngle = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            for (int i = 0; i < 3; i++)
                Unknown[i] = reader.ReadByte();
            Location[2] *= CoordZ;
            Rotate[2] *= CoordZ;
            /*Rotate[0] *= CoordZ;
            Rotate[1] *= CoordZ;*/
        }

        internal void Write(BinaryWriter writer, float CoordZ)
        {
            writer.Write((DWORD)FrameNo);
            writer.Write((Single)Length);
            writer.Write((Single)Location[0]);
            writer.Write((Single)Location[1]);
            writer.Write((Single)Location[2] * CoordZ);
            writer.Write((Single)Rotate[0]);
            writer.Write((Single)Rotate[1]);
            writer.Write((Single)Rotate[2] * CoordZ);
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 4; j++)
                    writer.Write((byte)Interpolation[i][j]);
            writer.Write((WORD)ViewingAngle);
            for (int i = 0; i < 3; i++)
                writer.Write((byte)Unknown[i]);
        }
    }
}
