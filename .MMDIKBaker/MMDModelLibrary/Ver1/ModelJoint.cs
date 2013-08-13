using System;
using System.IO;
using DWORD = System.UInt32;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// 物理演算用のジョイント
    /// </summary>
    public class ModelJoint
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } // 諸データ：名称 // 右髪1(char*20)
        /// <summary>
        /// 剛体A
        /// </summary>
        public DWORD RigidBodyA { get; set; } // 諸データ：剛体A
        /// <summary>
        /// 剛体B
        /// </summary>
        public DWORD RigidBodyB { get; set; } // 諸データ：剛体B
        /// <summary>
        /// 位置(x, y, z)
        /// </summary>
        public float[] Position { get;private set; } //float*3 諸データ：位置(x, y, z) // 諸データ：位置合せでも設定可
        /// <summary>
        /// 回転(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] Rotation { get; private set; } //float*3 諸データ：回転(rad(x), rad(y), rad(z))
        /// <summary>
        /// 移動制限1(x, y, z)
        /// </summary>
        public float[] ConstrainPosition1 { get; private set; } //float*3 制限：移動1(x, y, z)
        /// <summary>
        /// 移動制限2(x, y, z)
        /// </summary>
        public float[] ConstrainPosition2 { get; private set; } //float*3 制限：移動2(x, y, z)
        /// <summary>
        /// 回転制限1(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] ConstrainRotation1 { get; private set; } //float*3 制限：回転1(rad(x), rad(y), rad(z))
        /// <summary>
        /// 回転制限2(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] ConstrainRotation2 { get; private set; } //float*3 制限：回転2(rad(x), rad(y), rad(z))
        /// <summary>
        /// 平行移動に対するばねの戻る強さ：移動(x, y, z)
        /// </summary>
        public float[] SpringPosition { get; private set; } //float*3 ばね：移動(x, y, z)
        /// <summary>
        /// 回転に対するばねの戻る強さ：回転(rad(x), rad(y), rad(z))
        /// </summary>
        public float[] SpringRotation { get; private set; } //float*3 ばね：回転(rad(x), rad(y), rad(z))
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelJoint()
        {
            Position = new float[3];
            Rotation = new float[3];
            ConstrainPosition1 = new float[3];
            ConstrainPosition2 = new float[3];
            ConstrainRotation1 = new float[3];
            ConstrainRotation2 = new float[3];
            SpringPosition = new float[3];
            SpringRotation = new float[3];
        }
        internal void ReadExpantion(BinaryReader reader, float CoordZ, float scale)
        {
            Name = MMDModel1.GetString(reader.ReadBytes(20));
            RigidBodyA = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            RigidBodyB = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            for (int i = 0; i < Position.Length; i++)
                Position[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) *scale;
            for (int i = 0; i < Rotation.Length; i++)
                Rotation[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < ConstrainPosition1.Length; i++)
                ConstrainPosition1[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) *scale;
            for (int i = 0; i < ConstrainPosition2.Length; i++)
                ConstrainPosition2[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) *scale;
            for (int i = 0; i < ConstrainRotation1.Length; i++)
                ConstrainRotation1[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < ConstrainRotation2.Length; i++)
                ConstrainRotation2[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < SpringPosition.Length; i++)
                SpringPosition[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            for (int i = 0; i < SpringRotation.Length; i++)
                SpringRotation[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Position[2] *= CoordZ;
            //メモ：左手→右手では位置が変換される際に一緒に回転成分が変換されるため、回転の変換は必要ない
            //ただし、ジョイントの回転(使用してないっぽい)は変換しておく
            ConstrainRotation1[0] *= CoordZ;
            ConstrainRotation1[1] *= CoordZ;
            ConstrainRotation2[0] *= CoordZ;
            ConstrainRotation2[1] *= CoordZ;
            ConstrainPosition1[2] *= CoordZ;
            ConstrainPosition2[2] *= CoordZ;
        }

        internal void WriteExpantion(BinaryWriter writer, float CoordZ, float scale)
        {
            Position[2] *= CoordZ;
            ConstrainRotation1[0] *= CoordZ;
            ConstrainRotation1[1] *= CoordZ;
            ConstrainRotation2[0] *= CoordZ;
            ConstrainRotation2[1] *= CoordZ;
            ConstrainPosition1[2] *= CoordZ;
            ConstrainPosition2[2] *= CoordZ;
            writer.Write(MMDModel1.GetBytes(Name, 20));
            writer.Write(RigidBodyA);
            writer.Write(RigidBodyB);
            for (int i = 0; i < Position.Length; i++)
                writer.Write(Position[i]* scale);
            for (int i = 0; i < Rotation.Length; i++)
                writer.Write(Rotation[i]);
            for (int i = 0; i < ConstrainPosition1.Length; i++)
                writer.Write(ConstrainPosition1[i]*scale);
            for (int i = 0; i < ConstrainPosition2.Length; i++)
                writer.Write(ConstrainPosition2[i]*scale);
            for (int i = 0; i < ConstrainRotation1.Length; i++)
                writer.Write(ConstrainRotation1[i]);
            for (int i = 0; i < ConstrainRotation2.Length; i++)
                writer.Write(ConstrainRotation2[i]);
            for (int i = 0; i < SpringPosition.Length; i++)
                writer.Write(SpringPosition[i]*scale);
            for (int i = 0; i < SpringRotation.Length; i++)
                writer.Write(SpringRotation[i]);
        }
    }
}
