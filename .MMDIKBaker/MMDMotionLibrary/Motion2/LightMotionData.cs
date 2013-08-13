using System;
using System.IO;
using DWORD = System.UInt32;

namespace MikuMikuDance.Motion.Motion2
{
    /// <summary>
    /// ライトモーションデータ
    /// </summary>
    public class LightMotionData
    {
        /// <summary>
        /// フレームナンバー
        /// </summary>
        public DWORD FrameNo { get; set; }
        /// <summary>
        /// ライトの色
        /// </summary>
        public float[] Color { get; protected set; }
        /// <summary>
        /// ライトの位置
        /// </summary>
        public float[] Location { get; protected set; }
        internal void Read(BinaryReader reader, float CoordZ)
        {
            FrameNo = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Color = new float[3];
            Location = new float[3];
            for (int i = 0; i < 3; i++)
                Color[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < 3; i++)
                Location[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Location[2] *= CoordZ;
        }

        internal void Write(BinaryWriter writer, float CoordZ)
        {
            writer.Write((DWORD)FrameNo);
            for (int i = 0; i < 3; i++)
                writer.Write((Single)Color[i]);
            writer.Write((Single)Location[0]);
            writer.Write((Single)Location[1]);
            writer.Write((Single)Location[2] * CoordZ);
        }
    }
}
