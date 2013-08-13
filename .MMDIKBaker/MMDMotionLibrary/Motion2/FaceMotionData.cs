using System;
using System.IO;
using DWORD = System.UInt32;

namespace MikuMikuDance.Motion.Motion2
{
    /// <summary>
    /// フェイスモーションデータ
    /// </summary>
    public class FaceMotionData
    {
        /// <summary>
        /// 表情適応割合
        /// </summary>
        public float Rate { get; set; }
        /// <summary>
        /// 表情名
        /// </summary>
        public string FaceName { get; set; }//[15];
        /// <summary>
        /// フレームナンバー
        /// </summary>
        public DWORD FrameNo { get; set; }
        internal void Read(BinaryReader reader)
        {
            FaceName = MMDMotion2.GetString(reader.ReadBytes(15));
            FrameNo = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            //Wait = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Rate = BitConverter.ToSingle(reader.ReadBytes(4), 0);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(MMDMotion2.GetBytes(FaceName, 15));
            writer.Write((DWORD)FrameNo);
            writer.Write((Single)Rate);
        }
    }
}
