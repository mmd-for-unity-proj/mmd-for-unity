using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MMD.PMX;
using MMD.PMD;

public partial class PMXLoaderScript {

	/// <summary>
	/// PMDファイルのヘッダー取得
	/// </summary>
	/// <param name='pmx_header'>PMXヘッダー</param>
	/// <returns>ヘッダー</returns>
	public static PMDFormat.Header PMX2PMD(PMXFormat.Header pmx_header) {
		PMDFormat.Header pmd_header = ConvertHeader(pmx_header);
		return pmd_header;
	}

	/// <summary>
	/// PMDファイルの取得
	/// </summary>
	/// <param name='pmx'>PMXファイル</param>
	/// <returns>内部形式データ</returns>
	public static PMDFormat PMX2PMD(PMXFormat pmx) {
		PMDFormat result = new PMDFormat();
		ConvertPathes(result, pmx);
		result.head = ConvertHeader(pmx.header);
		result.vertex_list = ConvertVertexList(pmx);
		result.face_vertex_list = ConvertFaceVertexList(pmx);
		result.material_list = ConvertMaterialList(pmx);
		result.bone_list = ConvertBoneList(pmx); 
		result.ik_list = ConvertIKList(pmx);
		result.skin_list = ConvertSkinList(pmx);
		result.skin_name_list = ConvertSkinNameList(pmx);
		result.bone_name_list = ConvertBoneNameList(pmx);
		result.bone_display_list = ConvertBoneDisplayList(pmx);
		result.eg_head = ConvertEnglishHeader(pmx);
		result.eg_bone_name_list = ConvertEnglishBoneNameList(pmx);
		result.eg_skin_name_list = ConvertEnglishSkinNameList(pmx);
		result.eg_bone_display_list = ConvertEnglishBoneDisplayList(pmx);
		result.toon_texture_list = ConvertToonTextureList(pmx);
		result.rigidbody_list = ConvertRigidbodyList(pmx);
		result.rigidbody_joint_list = ConvertRigidbodyJointList(pmx);
		return result;
	}

	private static void ConvertPathes(PMDFormat pmd, PMXFormat pmx) {
		pmd.path = pmx.meta_header.path;
		pmd.name = pmx.meta_header.name;
		pmd.folder = pmx.meta_header.folder;
	}
	
	private static PMDFormat.Header ConvertHeader(PMXFormat.Header pmx_header) {
		PMDFormat.Header result = new PMDFormat.Header();
		result.magic = pmx_header.magic;
		result.version = pmx_header.version;
		result.model_name = pmx_header.model_name;
		result.comment = pmx_header.comment;
		return result;
	}

	private static PMDFormat.VertexList ConvertVertexList(PMXFormat pmx) {
		PMDFormat.VertexList result = new PMDFormat.VertexList();
		result.vert_count = (uint)pmx.vertex_list.vertex.Length;
		result.vertex = new PMDFormat.Vertex[result.vert_count];
		for (int i = 0; i < result.vert_count; i++) {
			result.vertex[i] = ConvertVertex(pmx, i);
		}
		return result;
	}

	private static PMDFormat.Vertex ConvertVertex(PMXFormat pmx, int vertex_index) {
		PMDFormat.Vertex result = new PMDFormat.Vertex();
		PMXFormat.Vertex pmx_vertex = pmx.vertex_list.vertex[vertex_index];
		result.pos = pmx_vertex.pos;
		result.normal_vec = pmx_vertex.normal_vec;
		result.uv = pmx_vertex.uv;
		switch (pmx_vertex.bone_weight.method) {
		case PMXFormat.Vertex.WeightMethod.BDEF1: goto case PMXFormat.Vertex.WeightMethod.BDEF2;
		case PMXFormat.Vertex.WeightMethod.BDEF2:
			//BDEF1,BDEF2なら
			//完全互換
			result.bone_num = new ushort[]{(ushort)pmx_vertex.bone_weight.bone1_ref, (ushort)pmx_vertex.bone_weight.bone2_ref};
			result.bone_weight = (byte)(pmx_vertex.bone_weight.bone1_weight * 100.0f + 0.5f);
			break;
		case PMXFormat.Vertex.WeightMethod.BDEF4:
			//BDEF4なら
			//ボーン3,4の情報を捨てる
			result.bone_num = new ushort[]{(ushort)pmx_vertex.bone_weight.bone1_ref, (ushort)pmx_vertex.bone_weight.bone2_ref};
			float bone1_weight = pmx_vertex.bone_weight.bone1_weight / (pmx_vertex.bone_weight.bone1_weight + pmx_vertex.bone_weight.bone2_weight);
			result.bone_weight = (byte)(bone1_weight * 100.0f + 0.5f);
			break;
		case PMXFormat.Vertex.WeightMethod.SDEF:
			//SDEFなら
			//SDEFの情報を捨ててBDEF2扱い
			goto case PMXFormat.Vertex.WeightMethod.BDEF2;
		case PMXFormat.Vertex.WeightMethod.QDEF:
			//QDEFなら
			//BDEF4と同じ対応
			goto case PMXFormat.Vertex.WeightMethod.BDEF4;
		default:
			throw new System.ArgumentOutOfRangeException();
		}
		result.edge_flag = (byte)((0.5f <= pmx_vertex.edge_magnification)? 1: 0);
		return result;
	}

	private static PMDFormat.FaceVertexList ConvertFaceVertexList(PMXFormat pmx) {
		PMDFormat.FaceVertexList result = new PMDFormat.FaceVertexList();
		result.face_vert_count = (uint)pmx.face_vertex_list.face_vert_index.Length;
		result.face_vert_index = pmx.face_vertex_list.face_vert_index.Select(x=>(ushort)x).ToArray();
		return result;
	}

	private static PMDFormat.MaterialList ConvertMaterialList(PMXFormat pmx) {
		PMDFormat.MaterialList result = new PMDFormat.MaterialList();
		result.material_count = (uint)pmx.material_list.material.Length;
		result.material = new PMDFormat.Material[result.material_count];
		for (int i = 0; i < result.material_count; i++) {
			result.material[i] = ConvertMaterial(pmx, i);
		}
		return result;
	}
	
	private static PMDFormat.Material ConvertMaterial(PMXFormat pmx, int material_index) {
		PMDFormat.Material result = new PMDFormat.Material();
		PMXFormat.Material pmx_material = pmx.material_list.material[material_index];
		result.diffuse_color = pmx_material.diffuse_color;
		result.alpha = result.diffuse_color.a;
		result.diffuse_color.a = 1.0f;
		result.specularity = pmx_material.specularity;
		result.specular_color = pmx_material.specular_color;
		result.mirror_color = pmx_material.ambient_color;
		if (0 < pmx_material.common_toon) {
			//共通トゥーン
			string toon_name = "toon" + pmx_material.common_toon.ToString("00") + ".bmp";
			int toon_name_index = -1;
			for (int i = 0, i_max = pmx.texture_list.texture_file.Length; i < i_max; ++i) {
				if (toon_name == pmx.texture_list.texture_file[i]) {
					//既存の pmx.texture_list.texture_file に有れば
					//使う
					toon_name_index = i;
					break;
				}
			}
			if (toon_name_index < 0) {
				//既存の pmx.texture_list.texture_file になければ
				//後ろに追加する
				string[] texture_file = new string[pmx.texture_list.texture_file.Length + 1];
				System.Array.Copy(pmx.texture_list.texture_file, texture_file, pmx.texture_list.texture_file.Length);
				texture_file[pmx.texture_list.texture_file.Length] = toon_name; //最後に追加
				toon_name_index = pmx.texture_list.texture_file.Length;
				pmx.texture_list.texture_file = texture_file;
			}
			result.toon_index = (byte)toon_name_index;
		} else {
			//自前トゥーン
			result.toon_index = (byte)pmx_material.toon_texture_index;
		}
		result.edge_flag = (byte)((0 != ((int)pmx_material.flag & (int)PMXFormat.Material.Flag.Edge))? 1: 0);
		result.face_vert_count = pmx_material.face_vert_count;
		if (uint.MaxValue != pmx_material.sphere_texture_index) {
			string tex = pmx.texture_list.texture_file[pmx_material.sphere_texture_index];
			result.sphere_map_name = ((0 == tex.IndexOf("./"))? tex.Substring(2): tex); //"./"開始なら除去
#if MFU_PMX2PMD_SPHERE_MAP_NAME_ADD_EXTENSION_FOR_SPHERE_MODE //スフィアモードの為に、拡張子を付加する(ファイルパスが変更されるのでMMDConverter側で2重拡張子に対応する迄無効化)
			switch (pmx_material.sphere_mode) {
			case PMXFormat.Material.SphereMode.AddSphere:
				result.sphere_map_name += ".spa";
				break;
			case PMXFormat.Material.SphereMode.MulSphere:
				result.sphere_map_name += ".sph";
				break;
			default:
				break;
			}
#endif
		}
		if (uint.MaxValue != pmx_material.usually_texture_index) {
			string tex = pmx.texture_list.texture_file[pmx_material.usually_texture_index];
			result.texture_file_name = ((0 == tex.IndexOf("./"))? tex.Substring(2): tex); //"./"開始なら除去
		}
		return result;
	}

	private static PMDFormat.BoneList ConvertBoneList(PMXFormat pmx) {
		PMDFormat.BoneList result = new PMDFormat.BoneList();
		result.bone_count = (ushort)pmx.bone_list.bone.Length;
		result.bone = new PMDFormat.Bone[result.bone_count];
		for (int i = 0; i < result.bone_count; i++) {
			result.bone[i] = ConvertBone(pmx, i);
		}
		return result;
	}

	private static PMDFormat.Bone ConvertBone(PMXFormat pmx, int bone_index) {
		PMDFormat.Bone result = new PMDFormat.Bone();
		PMXFormat.Bone pmx_bone = pmx.bone_list.bone[bone_index];
		result.bone_name = pmx_bone.bone_name;
		result.parent_bone_index = (ushort)((uint.MaxValue == pmx_bone.parent_bone_index)? ushort.MaxValue: pmx_bone.parent_bone_index);
		result.tail_pos_bone_index = ushort.MaxValue;
#if false
		//PMD側 0:回転 1:回転と移動 2:IK 3:不明 4:IK影響下 5:回転影響下 6:IK接続先 7:非表示 8:捻り 9:回転運動
		result.bone_type = pmx_bone.bone_flag;
		//PMX側
		public enum Flag {
			Connection				= 1<< 0,
			Rotatable				= 1<< 1,
			Movable					= 1<< 2,
			DisplayFlag				= 1<< 3,
			CanOperate				= 1<< 4,
			IkFlag					= 1<< 5,
			AddRotation				= 1<< 8,
			AddMove					= 1<< 9,
			FixedAxis				= 1<<10,
			LocalAxis				= 1<<11,
			PhysicsTransform		= 1<<12,
			ExternalParentTransform	= 1<<13,
		}
#else
		result.bone_type = 2; //TODO: ボーンタイプを入れる
#endif
		if (null != pmx_bone.ik_data) {
			result.ik_parent_bone_index = (ushort)pmx_bone.ik_data.ik_bone_index;
		}
		result.bone_head_pos = pmx_bone.bone_position;
		return result;
	}

	private static PMDFormat.IKList ConvertIKList(PMXFormat pmx) {
		PMDFormat.IKList result = new PMDFormat.IKList();
		uint index = 0;
		result.ik_data = pmx.bone_list.bone.Select(x=>new {data=x, index=index++}) //ボーン通しインデックスが要るのでデータと通しインデックスをパック
											.Where(x=>((0 != ((int)PMXFormat.Bone.Flag.IkFlag & (int)x.data.bone_flag)) && (null != x.data.ik_data))) //IkFlagが有効でik_dataデータを持つもののみに絞る
											.Select(x=>ConvertIK(x.data.ik_data, x.index)) //ConvertIK()を呼び出す
											.ToArray();
		result.ik_data_count = (ushort)result.ik_data.Length;
		return result;
	}

	private static PMDFormat.IK ConvertIK(PMXFormat.IK_Data pmx_ik_data, uint bone_index) {
		PMDFormat.IK result = new PMDFormat.IK();
		result.ik_bone_index = (ushort)bone_index;
		result.ik_target_bone_index = (ushort)pmx_ik_data.ik_bone_index;
		result.ik_chain_length = (byte)pmx_ik_data.ik_link.Length;
		result.iterations = (ushort)pmx_ik_data.iterations;
		result.control_weight = pmx_ik_data.limit_angle / 4.0f; //MMDConverter側で4倍されるので逆補正
		result.ik_child_bone_index = pmx_ik_data.ik_link.Select(x=>(ushort)x.target_bone_index).ToArray();
		return result;
	}

	private static PMDFormat.SkinList ConvertSkinList(PMXFormat pmx) {
		PMDFormat.SkinList result = new PMDFormat.SkinList();
		result.skin_count = (ushort)pmx.morph_list.morph_data.Length;
		result.skin_data = new PMDFormat.SkinData[result.skin_count + 1]; //base分が含まれていないので確保
		//base設定
		PMDFormat.SkinData base_skin = CreateBaseSkinData(pmx);
		result.skin_data[0] = base_skin;
		//頂点インデックス逆引き用辞書の作成
		Dictionary<uint, uint> skin_vertex_index_reverse_dictionary = new Dictionary<uint, uint>((int)base_skin.skin_vert_count);
		for (uint i = 0, i_max = base_skin.skin_vert_count; i < i_max; ++i) {
			skin_vertex_index_reverse_dictionary.Add(base_skin.skin_vert_data[i].skin_vert_index, i);
		}
		//base以外の設定
		for (int i = 0; i < result.skin_count; i++) {
			result.skin_data[i + 1] = ConvertSkinData(pmx, i, skin_vertex_index_reverse_dictionary);
		}
		return result;
	}

	private static PMDFormat.SkinData ConvertSkinData(PMXFormat pmx, int skin_index, Dictionary<uint, uint> skin_vertex_index_reverse_dictionary) {
		PMDFormat.SkinData result = new PMDFormat.SkinData();
		PMXFormat.MorphData pmx_morph = pmx.morph_list.morph_data[skin_index];
		result.skin_name = pmx_morph.morph_name;
		result.skin_vert_count = (uint)pmx_morph.morph_offset.Length;
		result.skin_type = (byte)pmx_morph.handle_panel;
		result.skin_vert_data = new PMDFormat.SkinVertexData[result.skin_vert_count];
		for (int i = 0; i < result.skin_vert_count; i++) {
			result.skin_vert_data[i] = ConvertSkinVertexData(pmx, skin_index, i, skin_vertex_index_reverse_dictionary);
		}
		return result;
	}

	private static PMDFormat.SkinVertexData ConvertSkinVertexData(PMXFormat pmx, int skin_index, int skin_vertex_index, Dictionary<uint, uint> skin_vertex_index_reverse_dictionary) {
		PMDFormat.SkinVertexData result = new PMDFormat.SkinVertexData();
		PMXFormat.MorphData pmx_morph = pmx.morph_list.morph_data[skin_index];
		PMXFormat.MorphOffset pmx_offset = pmx.morph_list.morph_data[skin_index].morph_offset[skin_vertex_index];
		switch (pmx_morph.morph_type) {
		case PMXFormat.MorphData.MorphType.Vertex: //頂点変形モーフなら
			PMXFormat.VertexMorphOffset vertex_pmx_offset = (PMXFormat.VertexMorphOffset)pmx_offset;
			result.skin_vert_index = skin_vertex_index_reverse_dictionary[vertex_pmx_offset.vertex_index];
			result.skin_vert_pos = vertex_pmx_offset.position_offset;
			break;
		default: //頂点変形モーフ以外なら
			//全て無効化
			result.skin_vert_index = skin_vertex_index_reverse_dictionary.FirstOrDefault().Value;
			result.skin_vert_pos = Vector3.zero;
			break;
		}
		return result;
	}

	private static PMDFormat.SkinData CreateBaseSkinData(PMXFormat pmx) {
		PMDFormat.SkinData result = new PMDFormat.SkinData();
		result.skin_name = "base";
		result.skin_type = (byte)PMXFormat.MorphData.Panel.Base;
		var all_vertex_index_list = pmx.morph_list.morph_data.Where(x=>(PMXFormat.MorphData.MorphType.Vertex == x.morph_type)) //頂点変形ボーンに絞る
															.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.VertexMorphOffset)y).vertex_index)) //頂点インデックスの取り出しと連結
															.Distinct() //重複した頂点インデックスの削除
															.ToList(); //ソートに向けて一旦リスト化
		all_vertex_index_list.Sort(); //ソート
		result.skin_vert_data = all_vertex_index_list.Select(x=>{
																PMDFormat.SkinVertexData skin_vertex_data = new PMDFormat.SkinVertexData();
																skin_vertex_data.skin_vert_index = x;
																skin_vertex_data.skin_vert_pos = pmx.vertex_list.vertex[x].pos;
																return skin_vertex_data;
															}) //頂点インデックスを用いて、元の頂点座標をパック
													.ToArray();
		result.skin_vert_count = (uint)result.skin_vert_data.Length;
		return result;
	}

	private static PMDFormat.SkinNameList ConvertSkinNameList(PMXFormat pmx) {
		PMDFormat.SkinNameList result = new PMDFormat.SkinNameList();
		result.skin_disp_count = (byte)pmx.morph_list.morph_data.Length;
		result.skin_index = new ushort[result.skin_disp_count];
		for (int i = 0, i_max = result.skin_index.Length; i < i_max; ++i) {
			result.skin_index[i] = (ushort)i;
		}
		return result;
	}

	private static PMDFormat.BoneNameList ConvertBoneNameList(PMXFormat pmx) {
		PMDFormat.BoneNameList result = new PMDFormat.BoneNameList();
		result.bone_disp_name_count = (byte)pmx.bone_list.bone.Length;
		result.disp_name = pmx.bone_list.bone.Select(x=>x.bone_name).ToArray();
		return result;
	}

	private static PMDFormat.BoneDisplayList ConvertBoneDisplayList(PMXFormat pmx) {
		PMDFormat.BoneDisplayList result = new PMDFormat.BoneDisplayList();
		result.bone_disp_count = (uint)pmx.bone_list.bone.Length;
		result.bone_disp = new PMDFormat.BoneDisplay[result.bone_disp_count];
		for (int i = 0; i < result.bone_disp_count; i++) {
			result.bone_disp[i] = ConvertBoneDisplay(pmx, i);
		}
		return result;
	}

	private static PMDFormat.BoneDisplay ConvertBoneDisplay(PMXFormat pmx, int bone_index) {
		PMDFormat.BoneDisplay result = new PMDFormat.BoneDisplay();
		result.bone_index = (ushort)bone_index;
		result.bone_disp_frame_index = (byte)bone_index;
		return result;
	}

	private static PMDFormat.EnglishHeader ConvertEnglishHeader(PMXFormat pmx) {
		PMDFormat.EnglishHeader result = new PMDFormat.EnglishHeader();
		result.english_name_compatibility = 1;
		result.model_name_eg = pmx.header.model_english_name;
		result.comment_eg = pmx.header.english_comment;
		return result;
	}

	private static PMDFormat.EnglishBoneNameList ConvertEnglishBoneNameList(PMXFormat pmx) {
		PMDFormat.EnglishBoneNameList result = new PMDFormat.EnglishBoneNameList();
		result.bone_name_eg = pmx.bone_list.bone.Select(x=>x.bone_english_name).ToArray();
		return result;
	}

	private static PMDFormat.EnglishSkinNameList ConvertEnglishSkinNameList(PMXFormat pmx) {
		PMDFormat.EnglishSkinNameList result = new PMDFormat.EnglishSkinNameList();
		result.skin_name_eg = pmx.morph_list.morph_data.Select(x=>x.morph_english_name).ToArray();
		return result;
	}

	private static PMDFormat.EnglishBoneDisplayList ConvertEnglishBoneDisplayList(PMXFormat pmx) {
		PMDFormat.EnglishBoneDisplayList result = new PMDFormat.EnglishBoneDisplayList();
		result.disp_name_eg = pmx.display_frame_list.display_frame.Select(x=>x.display_english_name).ToArray();
		return result;
	}

	private static PMDFormat.ToonTextureList ConvertToonTextureList(PMXFormat pmx) {
		PMDFormat.ToonTextureList result = new PMDFormat.ToonTextureList();
		result.toon_texture_file = pmx.texture_list.texture_file.Select(x=>((0 == x.IndexOf("./"))? x.Substring(2): x)) //"./"開始なら除去
																.ToArray();
		return result;
	}

	private static PMDFormat.RigidbodyList ConvertRigidbodyList(PMXFormat pmx) {
		PMDFormat.RigidbodyList result = new PMDFormat.RigidbodyList();
		result.rigidbody_count = (uint)pmx.rigidbody_list.rigidbody.Length;
		result.rigidbody = new PMDFormat.Rigidbody[result.rigidbody_count];
		for (int i = 0; i < result.rigidbody_count; i++) {
			result.rigidbody[i] = ConvertRigidbody(pmx, i);
		}
		return result;
	}
	
	private static PMDFormat.Rigidbody ConvertRigidbody(PMXFormat pmx, int rigidbody_index) {
		PMDFormat.Rigidbody result = new PMDFormat.Rigidbody();
		PMXFormat.Rigidbody pmx_rigidbody = pmx.rigidbody_list.rigidbody[rigidbody_index];
		result.rigidbody_name = pmx_rigidbody.name;
		result.rigidbody_rel_bone_index = (int)pmx_rigidbody.rel_bone_index;
		result.rigidbody_group_index = pmx_rigidbody.group_index;
		result.rigidbody_group_target = pmx_rigidbody.ignore_collision_group;
		result.shape_type = (byte)pmx_rigidbody.shape_type;
		result.shape_w = pmx_rigidbody.shape_size.x;
		result.shape_h = pmx_rigidbody.shape_size.y;
		result.shape_d = pmx_rigidbody.shape_size.z;
		result.pos_pos = pmx_rigidbody.collider_position;
		if (pmx_rigidbody.rel_bone_index < pmx.bone_list.bone.Length) {
			result.pos_pos -= pmx.bone_list.bone[pmx_rigidbody.rel_bone_index].bone_position;
		} else {
			result.pos_pos -= pmx.bone_list.bone[0].bone_position;
		}
		result.pos_rot = pmx_rigidbody.collider_rotation;
		result.rigidbody_weight = pmx_rigidbody.weight;
		result.rigidbody_pos_dim = pmx_rigidbody.position_dim;
		result.rigidbody_rot_dim = pmx_rigidbody.rotation_dim;
		result.rigidbody_recoil = pmx_rigidbody.recoil;
		result.rigidbody_friction = pmx_rigidbody.friction;
		result.rigidbody_type = (byte)pmx_rigidbody.operation_type;
		return result;
	}
	
	private static PMDFormat.RigidbodyJointList ConvertRigidbodyJointList(PMXFormat pmx) {
		PMDFormat.RigidbodyJointList result = new PMDFormat.RigidbodyJointList();
		result.joint_count = (uint)pmx.rigidbody_joint_list.joint.Length;
		result.joint = new PMDFormat.Joint[result.joint_count];
		for (int i = 0; i < result.joint_count; i++) {
			result.joint[i] = ConvertJoint(pmx, i);
		}
		return result;
	}
	
	private static PMDFormat.Joint ConvertJoint(PMXFormat pmx, int joint_index) {
		PMDFormat.Joint result = new PMDFormat.Joint();
		PMXFormat.Joint pmx_joint = pmx.rigidbody_joint_list.joint[joint_index];
		result.joint_name = pmx_joint.name;
		result.joint_rigidbody_a = pmx_joint.rigidbody_a;
		result.joint_rigidbody_b = pmx_joint.rigidbody_b;
		result.joint_pos = pmx_joint.position;
		result.joint_rot = pmx_joint.rotation;
		result.constrain_pos_1 = pmx_joint.constrain_pos_lower;
		result.constrain_pos_2 = pmx_joint.constrain_pos_upper;
		result.constrain_rot_1 = pmx_joint.constrain_rot_lower;
		result.constrain_rot_2 = pmx_joint.constrain_rot_upper;
		result.spring_pos = pmx_joint.spring_position;
		result.spring_rot = pmx_joint.spring_rotation;
		return result;
	}
}
