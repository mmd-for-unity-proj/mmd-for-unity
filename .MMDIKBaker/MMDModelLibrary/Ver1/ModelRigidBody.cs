using System;
using System.IO;
using WORD = System.UInt16;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// 剛体(物理演算用)
    /// </summary>
    public class ModelRigidBody
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } // 諸データ：名称 // 頭(20byte char)
        /// <summary>
        /// 関連ボーン番号
        /// </summary>
        public WORD RelatedBoneIndex { get; set; } // 諸データ：関連ボーン番号 // 03 00 == 3 // 頭
        /// <summary>
        /// グループ番号
        /// </summary>
        public byte GroupIndex { get; set; } // 諸データ：グループ // 00
        /// <summary>
        /// 衝突対象グループ
        /// </summary>
        /// <remarks>各ビットがグループ番号に対応しており、ビットが立ってなければそのグループとは衝突しないという実装</remarks>
        public WORD GroupTarget { get; set; } // 諸データ：グループ：対象 // 0xFFFFとの差 // 38 FE
        /// <summary>
        /// 形状
        /// </summary>
        /// <remarks>0:球、1:箱、2:カプセル</remarks>
        public byte ShapeType { get; set; } // 形状：タイプ(0:球、1:箱、2:カプセル) // 00 // 球
        /// <summary>
        /// 半径(幅)
        /// </summary>
        public float ShapeWidth { get; set; } // 形状：半径(幅) // CD CC CC 3F // 1.6
        /// <summary>
        /// 高さ
        /// </summary>
        public float ShapeHeight { get; set; } // 形状：高さ // CD CC CC 3D // 0.1
        /// <summary>
        /// 奥行き
        /// </summary>
        public float ShapeDepth { get; set; } // 形状：奥行 // CD CC CC 3D // 0.1
        /// <summary>
        /// 位置(x,y,z)
        /// </summary>
        public float[] Position { get; protected set; } //float*3 位置：位置(x, y, z)
        /// <summary>
        /// 回転
        /// </summary>
        public float[] Rotation { get; protected set; } //float*3 位置：回転(rad(x), rad(y), rad(z))
        /// <summary>
        /// 質量
        /// </summary>
        public float Weight { get; set; } // 諸データ：質量 // 00 00 80 3F // 1.0
        /// <summary>
        /// ダンピング１
        /// </summary>
        public float LinerDamping { get; set; } // 諸データ：移動減 // 00 00 00 00
        /// <summary>
        /// ダンピング２
        /// </summary>
        public float AngularDamping { get; set; } // 諸データ：回転減 // 00 00 00 00
        /// <summary>
        /// 反発係数
        /// </summary>
        public float Restitution { get; set; } // 諸データ：反発力 // 00 00 00 00
        /// <summary>
        /// 摩擦力
        /// </summary>
        public float Friction { get; set; } // 諸データ：摩擦力 // 00 00 00 00
        /// <summary>
        /// 剛体タイプ
        /// </summary>
        /// <remarks>0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)</remarks>
        public byte Type { get; set; } // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public ModelRigidBody()
        {
            Position = new float[3];
            Rotation = new float[3];
        }

        internal void ReadExpantion(BinaryReader reader, float CoordZ, float scale)
        {
            Name = MMDModel1.GetString(reader.ReadBytes(20));
            RelatedBoneIndex = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            GroupIndex = reader.ReadByte();
            GroupTarget = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            ShapeType = reader.ReadByte();
            ShapeWidth = BitConverter.ToSingle(reader.ReadBytes(4), 0)*scale;
            ShapeHeight = BitConverter.ToSingle(reader.ReadBytes(4), 0)*scale;
            ShapeDepth = BitConverter.ToSingle(reader.ReadBytes(4), 0)*scale;
            for (int i = 0; i < Position.Length; i++)
                Position[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0) * scale;
            for (int i = 0; i < Rotation.Length; i++)
                Rotation[i] = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Weight = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            LinerDamping = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            AngularDamping = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Restitution = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Friction = BitConverter.ToSingle(reader.ReadBytes(4), 0);
            Type = reader.ReadByte();
            Position[2] *= CoordZ;
            //メモ：左手→右手では位置が変換される際に一緒に回転成分が変換されるため、回転の変換は必要ない……のだが
            //剛体はモデルと違い、位置と回転情報だけなので、回転を変換する必要がある
            Rotation[0] *= CoordZ;
            Rotation[1] *= CoordZ;
        }

        internal void WriteExpantion(BinaryWriter writer, float CoordZ, float scale)
        {
            Position[2] *= CoordZ;
            Rotation[0] *= CoordZ;
            Rotation[1] *= CoordZ; 
            writer.Write(MMDModel1.GetBytes(Name, 20));
            writer.Write(RelatedBoneIndex);
            writer.Write(GroupIndex);
            writer.Write(GroupTarget);
            writer.Write(ShapeType);
            writer.Write(ShapeWidth * scale);
            writer.Write(ShapeHeight * scale);
            writer.Write(ShapeDepth * scale);
            for (int i = 0; i < Position.Length; i++)
                writer.Write(Position[i] * scale);
            for (int i = 0; i < Rotation.Length; i++)
                writer.Write(Rotation[i]);
            writer.Write(Weight);
            writer.Write(LinerDamping);
            writer.Write(AngularDamping);
            writer.Write(Restitution);
            writer.Write(Friction);
            writer.Write(Type);
        }
    }
}
