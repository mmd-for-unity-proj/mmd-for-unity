using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD
{
    namespace PMD
    {
        // PMDのフォーマットクラス
        public class PMDFormat
        {
            public string path;			// フルパス
            public string name;			// 拡張子とパス抜きのファイルの名前
            public string folder;		// ファイル名抜きのパス

            public Header head;
            public VertexList vertex_list;
            public FaceVertexList face_vertex_list;
            public MaterialList material_list;
            public BoneList bone_list;
            public IKList ik_list;
            public SkinList skin_list;
            public SkinNameList skin_name_list;
            public BoneNameList bone_name_list;
            public BoneDisplayList bone_display_list;
            public EnglishHeader eg_head;
            public EnglishBoneNameList eg_bone_name_list;
            public EnglishSkinNameList eg_skin_name_list;
            public EnglishBoneDisplayList eg_bone_display_list;
            public ToonTextureList toon_texture_list;
            public RigidbodyList rigidbody_list;
            public RigidbodyJointList rigidbody_joint_list;

            public class Header
            {
                public byte[] magic; // "Pmd"
                public float version; // 00 00 80 3F == 1.00
                public string model_name;
                public string comment;
            }

            public class VertexList
            {
                public uint vert_count; // 頂点数
                public Vertex[] vertex;  // 頂点データ(38bytes/頂点)
            }

            public class Vertex
            {
                public Vector3 pos; // x, y, z // 座標
                public Vector3 normal_vec; // nx, ny, nz // 法線ベクトル
                public Vector2 uv; // u, v // UV座標 // MMDは頂点UV
                public ushort[] bone_num; // ボーン番号1、番号2 // モデル変形(頂点移動)時に影響
                public byte bone_weight; // ボーン1に与える影響度 // min:0 max:100 // ボーン2への影響度は、(100 - bone_weight)
                public byte edge_flag; // 0:通常、1:エッジ無効 // エッジ(輪郭)が有効の場合
            }

            // 面頂点リスト
            public class FaceVertexList
            {
                public uint face_vert_count; // 頂点数
                public ushort[] face_vert_index; // 頂点番号(3個/面)
            }

            public class MaterialList
            {
                public uint material_count; // 材質数
                public Material[] material; // 材質データ(70bytes/material)
            }

            public class Material
            {
                public Color diffuse_color; // dr, dg, db // 減衰色
                public float alpha;
                public float specularity;
                public Color specular_color; // sr, sg, sb // 光沢色
                public Color mirror_color; // mr, mg, mb // 環境色(ambient)
                public byte toon_index; // toon??.bmp // 0.bmp:0xFF, 1(01).bmp:0x00 ・・・ 10.bmp:0x09
                public byte edge_flag; // 輪郭、影
                public uint face_vert_count; // 面頂点数 // インデックスに変換する場合は、材質0から順に加算
                public string texture_file_name; // テクスチャファイル名またはスフィアファイル名 // 20バイトぎりぎりまで使える(終端の0x00は無くても動く)
                public string sphere_map_name;	// スフィアマップ用

                /*
                テクスチャファイル名またはスフィアファイル名の補足：

                テクスチャファイルにスフィアファイルを乗算または加算する場合
                (MMD 5.12以降)
                "テクスチャ名.bmp*スフィア名.sph" で乗算
                "テクスチャ名.bmp*スフィア名.spa" で加算

                (MMD 5.11)
                "テクスチャ名.bmp/スフィア名.sph" で乗算

                (MMD 5.09あたり-)
                "テクスチャ名.bmp" または "スフィア名.sph"
                */
            }

            public class BoneList
            {
                public ushort bone_count; // ボーン数
                public Bone[] bone; // ボーンデータ(39bytes/bone)
            }

            public class Bone
            {
                public string bone_name; // ボーン名
                public ushort parent_bone_index; // 親ボーン番号(ない場合は0xFFFF)
                public ushort tail_pos_bone_index; // tail位置のボーン番号(チェーン末端の場合は0xFFFF) // 親：子は1：多なので、主に位置決め用
                public byte bone_type; // ボーンの種類
                public ushort ik_parent_bone_index; // IKボーン番号(影響IKボーン。ない場合は0)
                public Vector3 bone_head_pos; // x, y, z // ボーンのヘッドの位置

                /*
                ・ボーンの種類
                0:回転 1:回転と移動 2:IK 3:不明 4:IK影響下 5:回転影響下 6:IK接続先 7:非表示 8:捻り 9:回転運動
                */
            }

            public class IKList
            {
                public ushort ik_data_count; // IKデータ数
                public IK[] ik_data; // IKデータ((11+2*ik_chain_length)/IK)
            }

            public class IK
            {
                public ushort ik_bone_index; // IKボーン番号
                public ushort ik_target_bone_index; // IKターゲットボーン番号 // IKボーンが最初に接続するボーン
                public byte ik_chain_length; // IKチェーンの長さ(子の数)
                public ushort iterations; // 再帰演算回数 // IK値1
                public float control_weight; // IKの影響度 // IK値2
                public ushort[] ik_child_bone_index; // IK影響下のボーン番号
            }

            public class SkinList
            {
                public ushort skin_count; // 表情数
                public SkinData[] skin_data; // 表情データ((25+16*skin_vert_count)/skin)
            }

            public class SkinData
            {
                public string skin_name; //　表情名
                public uint skin_vert_count; // 表情用の頂点数
                public byte skin_type; // 表情の種類 // 0：base、1：まゆ、2：目、3：リップ、4：その他
                public SkinVertexData[] skin_vert_data; // 表情用の頂点のデータ(16bytes/vert)
            }

            public class SkinVertexData
            {
                // 実際の頂点を参照するには
                // int num = vertex_count - skin_vert_count;
                // skin_vert[num]みたいな形で参照しないと無理
                public uint skin_vert_index; // 表情用の頂点の番号(頂点リストにある番号)
                public Vector3 skin_vert_pos; // x, y, z // 表情用の頂点の座標(頂点自体の座標)
            }

            // 表情用枠名
            public class SkinNameList
            {
                public byte skin_disp_count;
                public ushort[] skin_index;		// 表情番号
            }

            // ボーン用枠名
            public class BoneNameList
            {
                public byte bone_disp_name_count;
                public string[] disp_name;		// 50byte
            }

            // ボーン枠用表示リスト
            public class BoneDisplayList
            {
                public uint bone_disp_count;
                public BoneDisplay[] bone_disp;
            }

            public class BoneDisplay
            {
                public ushort bone_index;		// 枠用ボーン番号 
                public byte bone_disp_frame_index;	// 表示枠番号 
            }

            /// <summary>
            /// 英語表記用ヘッダ
            /// </summary>
            public class EnglishHeader
            {
                public byte english_name_compatibility;	// 01で英名対応 
                public string model_name_eg;	// 20byte
                public string comment_eg;	// 256byte
            }

            /// <summary>
            /// 英語表記用ボーンの英語名
            /// </summary>
            public class EnglishBoneNameList
            {
                public string[] bone_name_eg;	// 20byte * bone_count
            }

            public class EnglishSkinNameList
            {
                // baseは英名が登録されない 
                public string[] skin_name_eg;	// 20byte * skin_count-1
            }

            public class EnglishBoneDisplayList
            {
                public string[] disp_name_eg;	// 50byte * bone_disp_name_count
            }

            public class ToonTextureList
            {
                public string[] toon_texture_file;	// 100byte * 10個固定 
            }

            public class RigidbodyList
            {
                public uint rigidbody_count;
                public PMD.PMDFormat.Rigidbody[] rigidbody;
            }

            /// <summary>
            /// 剛体
            /// </summary>
            public class Rigidbody
            {
                public string rigidbody_name; // 諸データ：名称 ,20byte
                public int rigidbody_rel_bone_index;// 諸データ：関連ボーン番号 
                public byte rigidbody_group_index; // 諸データ：グループ 
                public ushort rigidbody_group_target; // 諸データ：グループ：対象 // 0xFFFFとの差
                public byte shape_type;  // 形状：タイプ(0:球、1:箱、2:カプセル)  
                public float shape_w;	// 形状：半径(幅) 
                public float shape_h;	// 形状：高さ 
                public float shape_d;	// 形状：奥行 
                public Vector3 pos_pos;	 // 位置：位置(x, y, z) 
                public Vector3 pos_rot;	 // 位置：回転(rad(x), rad(y), rad(z)) 
                public float rigidbody_weight; // 諸データ：質量 // 00 00 80 3F // 1.0
                public float rigidbody_pos_dim; // 諸データ：移動減 // 00 00 00 00
                public float rigidbody_rot_dim; // 諸データ：回転減 // 00 00 00 00
                public float rigidbody_recoil; // 諸データ：反発力 // 00 00 00 00
                public float rigidbody_friction; // 諸データ：摩擦力 // 00 00 00 00
                public byte rigidbody_type; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
            }

            public class RigidbodyJointList
            {
                public uint joint_count;
                public Joint[] joint;
            }

            public class Joint
            {
                public string joint_name;	// 20byte
                public uint joint_rigidbody_a; // 諸データ：剛体A 
                public uint joint_rigidbody_b; // 諸データ：剛体B 
                public Vector3 joint_pos; // 諸データ：位置(x, y, z) // 諸データ：位置合せでも設定可 
                public Vector3 joint_rot; // 諸データ：回転(rad(x), rad(y), rad(z)) 
                public Vector3 constrain_pos_1; // 制限：移動1(x, y, z) 
                public Vector3 constrain_pos_2; // 制限：移動2(x, y, z) 
                public Vector3 constrain_rot_1; // 制限：回転1(rad(x), rad(y), rad(z)) 
                public Vector3 constrain_rot_2; // 制限：回転2(rad(x), rad(y), rad(z)) 
                public Vector3 spring_pos; // ばね：移動(x, y, z) 
                public Vector3 spring_rot; // ばね：回転(rad(x), rad(y), rad(z)) 
            }
        }
    }
}