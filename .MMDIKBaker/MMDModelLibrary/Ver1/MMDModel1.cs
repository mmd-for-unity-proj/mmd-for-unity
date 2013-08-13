using System;
using System.IO;
using System.Text;
using DWORD = System.UInt32;
using WORD = System.UInt16;

namespace MikuMikuDance.Model.Ver1
{
    /// <summary>
    /// MMDのモデルver1.0を表すクラス
    /// </summary>
    public class MMDModel1 : MMDModel
    {
        //members and properties...
        /// <summary>
        /// Version番号。MMDModelから継承されます
        /// </summary>
        public float Version
        {
            get { return 1.0f; }
        }
        /// <summary>
        /// ヘッダ情報
        /// </summary>
        public ModelHeader Header { get; protected set; }
        /// <summary>
        /// 頂点リスト
        /// </summary>
        public ModelVertex[] Vertexes { get; set; }
        /// <summary>
        /// 面リスト
        /// </summary>
        public WORD[] FaceVertexes { get; set; }
        /// <summary>
        /// 材質リスト
        /// </summary>
        public ModelMaterial[] Materials { get; set; }
        /// <summary>
        /// ボーンリスト
        /// </summary>
        public ModelBone[] Bones { get; set; }
        /// <summary>
        /// IKボーンリスト
        /// </summary>
        public ModelIK[] IKs { get; set; }
        /// <summary>
        /// 表情リスト
        /// </summary>
        public ModelSkin[] Skins { get; set; }//表情リスト
        /// <summary>
        /// 表情枠用の表情番号リスト
        /// </summary>
        public WORD[] SkinIndex { get; set; }
        /// <summary>
        /// ボーン枠用の枠名リスト
        /// </summary>
        public ModelBoneDispName[] BoneDispNames { get; set; }//ボーン枠用枠名リスト
        /// <summary>
        /// ボーン枠用表示リスト
        /// </summary>
        public ModelBoneDisp[] BoneDisps { get; set; }//ボーン枠用表示リスト
        /// <summary>
        /// 拡張1(英語拡張)使用フラグ
        /// </summary>
        public bool Expantion { get; set; }
        /// <summary>
        /// 拡張2(トゥーン指定)使用フラグ
        /// </summary>
        /// <remarks>拡張1がtrueでないと出力時トゥーン指定は書きだされない</remarks>
        public bool ToonExpantion { get; set; }
        /// <summary>
        /// トゥーンテクスチャリスト(拡張)
        /// </summary>
        public string[] ToonFileNames { get; protected set; }//トゥーンテクスチャリスト(拡張)、10個固定
        const int NumToonFileName = 10;

        /// <summary>
        /// 拡張3(物理演算拡張)使用フラグ
        /// </summary>
        /// <remarks>拡張1,2がtrueでないと物理演算項目は書きだされない</remarks>
        public bool PhysicsExpantion { get; set; }
        /// <summary>
        /// 物理演算用の剛体リスト(拡張)
        /// </summary>
        public ModelRigidBody[] RigidBodies { get; set; }//物理演算、剛体リスト(拡張)
        /// <summary>
        /// 物理演算用のジョイントリスト(拡張)
        /// </summary>
        public ModelJoint[] Joints { get; set; }//物理演算、ジョイントリスト(拡張)
        /// <summary>
        /// 保持しているデータの座標き
        /// </summary>
        public CoordinateType Coordinate { get; protected set; }
        //座標変換用ヘルパ関数
        float CoordZ { get { return (float)Coordinate; } }

        //constructor
        /// <summary>
        /// 既定のコンストラクタ
        /// </summary>
        public MMDModel1()
        {
            Header = new ModelHeader();
            Vertexes = null;
            Expantion = false;
            ToonExpantion = false;
            PhysicsExpantion = false;
            ToonFileNames = new string[NumToonFileName];
            Coordinate = CoordinateType.LeftHandedCoordinate;
        }
        
        //methods...
        /// <summary>
        /// Read関数
        /// </summary>
        /// <remarks>この関数はModelManagerから呼び出される。呼び出し時にはマジック文字とバージョン番号が読まれた状態で渡される</remarks>
        /// <param name="reader">マジック文字とバージョン番号読み込み済みのBinaryReader</param>
        /// <param name="coordinate">変換先座標系</param>
        /// <param name="scale">スケーリング値</param>
        public void Read(BinaryReader reader, CoordinateType coordinate, float scale)
        {
            Coordinate = coordinate;//座標系セット
            //通常ヘッダ読み込み(英語ヘッダはBoneIndexの後(ミクなら0x00071167)に書かれている
            Header.Read(reader);
            //頂点リスト読み込み
            DWORD num_vertex = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Vertexes = new ModelVertex[num_vertex];
            for (DWORD i = 0; i < num_vertex; i++)
            {
                Vertexes[i] = new ModelVertex();
                Vertexes[i].Read(reader, CoordZ, scale);
            }
            //面リスト読み込み
            DWORD face_vert_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            FaceVertexes = new WORD[face_vert_count];
            for (DWORD i = 0; i < face_vert_count; i++)
            {
                FaceVertexes[i] = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            }
            //材質リスト読み込み
            DWORD material_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            Materials = new ModelMaterial[material_count];
            for (DWORD i = 0; i < material_count; i++)
            {
                Materials[i] = new ModelMaterial();
                Materials[i].Read(reader);
            }
            //ボーンリスト読み込み
            WORD bone_count = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            Bones = new ModelBone[bone_count];
            for (WORD i = 0; i < bone_count; i++)
            {
                Bones[i] = new ModelBone();
                Bones[i].Read(reader, CoordZ, scale);
            }
            //IKリスト読み込み
            WORD ik_count = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            IKs = new ModelIK[ik_count];
            for (WORD i = 0; i < ik_count; i++)
            {
                IKs[i] = new ModelIK();
                IKs[i].Read(reader);
            }
            //表情リスト読み込み
            WORD skin_count = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            Skins = new ModelSkin[skin_count];
            for (WORD i = 0; i < skin_count; i++)
            {
                Skins[i] = new ModelSkin();
                Skins[i].Read(reader, CoordZ, scale);
            }
            //表情枠用表示リスト
            byte skin_disp_count = reader.ReadByte();
            SkinIndex = new WORD[skin_disp_count];
            for (byte i = 0; i < SkinIndex.Length; i++)
                SkinIndex[i] = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            //ボーン枠用枠名リスト
            byte bone_disp_name_count = reader.ReadByte();
            BoneDispNames = new ModelBoneDispName[bone_disp_name_count];
            for (byte i = 0; i < BoneDispNames.Length; i++)
            {
                BoneDispNames[i] = new ModelBoneDispName();
                BoneDispNames[i].Read(reader);
            }
            //ボーン枠用表示リスト
            DWORD bone_disp_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
            BoneDisps = new ModelBoneDisp[bone_disp_count];
            for (DWORD i = 0; i < BoneDisps.Length; i++)
            {
                BoneDisps[i] = new ModelBoneDisp();
                BoneDisps[i].Read(reader);
            }
            //英語表記フラグ
            Expantion = (reader.ReadByte() != 0);
            if (Expantion)
            {
                //英語ヘッダ
                Header.ReadExpantion(reader);
                //ボーンリスト(英語)
                for (WORD i = 0; i < bone_count; i++)
                {
                    Bones[i].ReadExpantion(reader);
                }
                //スキンリスト(英語)
                for (WORD i = 0; i < skin_count; i++)
                {
                    if (Skins[i].SkinType != 0)//baseのスキンには英名無し
                        Skins[i].ReadExpantion(reader);
                }
                //ボーン枠用枠名リスト(英語)
                for (byte i = 0; i < BoneDispNames.Length; i++)
                {
                    BoneDispNames[i].ReadExpantion(reader);
                }
            }
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                ToonExpantion = false;
            else
            {
                ToonExpantion = true;
                //トゥーンテクスチャリスト
                ToonFileNames = new string[NumToonFileName];//10個固定
                for (int i = 0; i < ToonFileNames.Length; i++)
                {
                    ToonFileNames[i] = GetString(reader.ReadBytes(100));
                }
            }
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                PhysicsExpantion = false;
            else
            {
                PhysicsExpantion = true;
                //剛体リスト
                DWORD rididbody_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
                RigidBodies = new ModelRigidBody[rididbody_count];
                for (DWORD i = 0; i < rididbody_count; i++)
                {
                    RigidBodies[i] = new ModelRigidBody();
                    RigidBodies[i].ReadExpantion(reader, CoordZ, scale);
                }
                //ジョイントリスト
                DWORD joint_count = BitConverter.ToUInt32(reader.ReadBytes(4), 0);
                Joints = new ModelJoint[joint_count];
                for (DWORD i = 0; i < joint_count; i++)
                {
                    Joints[i] = new ModelJoint();
                    Joints[i].ReadExpantion(reader, CoordZ, scale);
                }
            }
            
        }
        /// <summary>
        /// Write関数
        /// </summary>
        /// <remarks>この関数はModelManagerから呼び出される。呼び出し時にはマジック文字とバージョン番号が書かれた状態で渡される</remarks>
        /// <param name="writer">マジック文字とバージョン番号書き込み済みのBinaryWriter</param>
        /// <param name="scale">スケール</param>
        public void Write(BinaryWriter writer, float scale)
        {
            //通常ヘッダ書きだし(英語ヘッダはBoneIndexの後(ミクなら0x00071167)に書かれている
            Header.Write(writer);
            //頂点リスト書きだし
            if (Vertexes == null)
                writer.Write((DWORD)0);
            else
            {
                writer.Write((DWORD)Vertexes.LongLength);
                for (DWORD i = 0; i < Vertexes.LongLength; i++)
                {
                    if (Vertexes[i] == null)
                        throw new ArgumentNullException("Vertexes[" + i.ToString() + "]がnull");
                    Vertexes[i].Write(writer, CoordZ, scale);
                }
            }
            //面リスト書きだし
            if (FaceVertexes == null)
                writer.Write((DWORD)0);
            else
            {
                writer.Write((DWORD)FaceVertexes.LongLength);
                for (DWORD i = 0; i < FaceVertexes.LongLength; i++)
                {
                    writer.Write(FaceVertexes[i]);
                }
            }
            //材質リスト書きだし
            if (Materials == null)
                writer.Write((DWORD)0);
            else
            {
                writer.Write((DWORD)Materials.LongLength);
                for (DWORD i = 0; i < Materials.LongLength; i++)
                {
                    if (Materials[i] == null)
                        throw new ArgumentNullException("Materials[" + i.ToString() + "]がnull");
                    Materials[i].Write(writer);
                }
            }
            //ボーンリスト書きだし
            if (Bones == null)
                writer.Write((WORD)0);
            else
            {
                writer.Write((WORD)Bones.Length);
                for (WORD i = 0; i < Bones.Length; i++)
                {
                    if (Bones[i] == null)
                        throw new ArgumentNullException("Bones[" + i.ToString() + "]がnull");
                    Bones[i].Write(writer, CoordZ,scale);
                }
            }
            //IKリスト書きだし
            if (IKs == null)
                writer.Write((WORD)0);
            else
            {
                writer.Write((WORD)IKs.Length);
                for (WORD i = 0; i < IKs.Length; i++)
                {
                    if (IKs[i] == null)
                        throw new ArgumentNullException("IKs[" + i.ToString() + "]がnull");
                    IKs[i].Write(writer);
                }
            }
            //表情リスト書きだし
            if (Skins == null)
                writer.Write((WORD)0);
            else
            {
                writer.Write((WORD)Skins.Length);
                for (WORD i = 0; i < Skins.Length; i++)
                {
                    if (Skins[i] == null)
                        throw new ArgumentNullException("Skins[" + i.ToString() + "]がnull");
                    Skins[i].Write(writer, CoordZ,scale);
                }
            }
            //表情枠用表示リスト書きだし
            if (SkinIndex == null)
                writer.Write((byte)0);
            else
            {
                writer.Write((byte)SkinIndex.Length);

                for (byte i = 0; i < SkinIndex.Length; i++)
                {
                    writer.Write(SkinIndex[i]);
                }
            }
            //ボーン枠用枠名リスト
            if (BoneDispNames == null)
                writer.Write((byte)0);
            else
            {
                writer.Write((byte)BoneDispNames.Length);
                for (byte i = 0; i < BoneDispNames.Length; i++)
                {
                    if(BoneDispNames[i]==null)
                        throw new ArgumentNullException("BoneDispNames[" + i.ToString() + "]がnull");
                    BoneDispNames[i].Write(writer);
                }
            }
            //ボーン枠用表示リスト
            if (BoneDisps == null)
                writer.Write((DWORD)0);
            else
            {
                writer.Write((DWORD)BoneDisps.Length);
                for (DWORD i = 0; i < BoneDisps.Length; i++)
                {
                    if (BoneDisps[i] == null)
                        throw new ArgumentNullException("BoneDisps[" + i.ToString() + "]がnull");
                    BoneDisps[i].Write(writer);
                }
            }
            //英語表記フラグ
            writer.Write((byte)(Expantion ? 1 : 0));
            if (Expantion)
            {
                //英語ヘッダ
                Header.WriteExpantion(writer);
                //ボーンリスト(英語)
                if (Bones != null)
                {
                    for (WORD i = 0; i < Bones.Length; i++)
                    {
                        Bones[i].WriteExpantion(writer);
                    }
                }
                //スキンリスト(英語)
                if (Skins != null)
                {
                    for (WORD i = 0; i < Skins.Length; i++)
                    {
                        if (Skins[i].SkinType != 0)//baseのスキンには英名無し
                            Skins[i].WriteExpantion(writer);
                    }
                }
                //ボーン枠用枠名リスト(英語)
                if (BoneDispNames != null)
                {
                    for (byte i = 0; i < BoneDispNames.Length; i++)
                    {
                        BoneDispNames[i].WriteExpantion(writer);
                    }
                }
            }
            if (ToonExpantion)
            {
                //トゥーンテクスチャリスト
                for (int i = 0; i < ToonFileNames.Length; i++)
                {
                    writer.Write(GetBytes(ToonFileNames[i], 100));
                }
                if (PhysicsExpantion)
                {
                    //剛体リスト
                    if (RigidBodies == null)
                        writer.Write((DWORD)0);
                    else
                    {
                        writer.Write((DWORD)RigidBodies.LongLength);
                        for (long i = 0; i < RigidBodies.LongLength; i++)
                        {
                            if (RigidBodies[i] == null)
                                throw new ArgumentNullException("RididBodies[" + i.ToString() + "]がnull");
                            RigidBodies[i].WriteExpantion(writer, CoordZ,scale);
                        }
                    }
                    //ジョイントリスト
                    if (Joints == null)
                        writer.Write((DWORD)0);
                    else
                    {
                        writer.Write((DWORD)Joints.LongLength);
                        for (long i = 0; i < Joints.LongLength; i++)
                        {
                            if (Joints[i] == null)
                                throw new ArgumentNullException("Joints[" + i.ToString() + "]がnull");
                            Joints[i].WriteExpantion(writer, CoordZ,scale);
                        }
                    }
                }
            }
            
        }
#if false
        /// <summary>
        /// スケーリング
        /// </summary>
        /// <param name="ScaleFactor">拡大倍率</param>
        public void Scale(float ScaleFactor)
        {
            if(ScaleFactor<=0)
                throw new ApplicationException("ScaleFactorは正の実数である必要があります。");
            //頂点
            for (long i = 0; i < Vertexes.LongLength; i++)
            {
                for (int j = 0; j < Vertexes[i].Pos.Length; j++)
                    Vertexes[i].Pos[j] = Vertexes[i].Pos[j] * ScaleFactor;
            }
            //ボーン
            for (long i = 0; i < Bones.LongLength; i++)
            {
                for (int j = 0; j < Bones[i].BoneHeadPos.Length; j++)
                    Bones[i].BoneHeadPos[j] = Vertexes[i].Pos[j] * ScaleFactor;
            }
            //表情
            for (long i = 0; i < Skins.LongLength; i++)
            {
                for (long j = 0; j < Skins[i].SkinVertDatas.LongLength; j++)
                {
                    for (int k = 0; k < Skins[i].SkinVertDatas[j].SkinVertPos.Length; k++)
                        Skins[i].SkinVertDatas[j].SkinVertPos[k] = Skins[i].SkinVertDatas[j].SkinVertPos[k] * ScaleFactor;
                }
            }
            if (PhysicsExpantion)
            {
                //剛体
                for (long i = 0; i < RigidBodies.LongLength; i++)
                {
                    for (int j = 0; j < RigidBodies[i].Position.Length; j++)
                        RigidBodies[i].Position[j] = RigidBodies[i].Position[j] * ScaleFactor;
                }
                //ジョイント
                for (long i = 0; i < Joints.LongLength; i++)
                {
                    for (int j = 0; j < Joints[i].ConstrainPosition1.Length; j++)
                    {
                        Joints[i].ConstrainPosition1[j] = Joints[i].ConstrainPosition1[j] * ScaleFactor;
                        Joints[i].ConstrainPosition2[j] = Joints[i].ConstrainPosition2[j] * ScaleFactor;
                        Joints[i].Position[j] = Joints[i].Position[j] * ScaleFactor;
                        Joints[i].SpringPosition[j] = Joints[i].SpringPosition[j] * ScaleFactor;
                    }
                }
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
