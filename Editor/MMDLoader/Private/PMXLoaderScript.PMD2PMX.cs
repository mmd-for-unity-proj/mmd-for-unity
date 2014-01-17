using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MMD.PMX;
using MMD.PMD;

public partial class PMXLoaderScript {

	/// <summary>
	/// PMXファイルのヘッダー取得
	/// </summary>
	/// <param name='pmd_header'>PMDヘッダー</param>
	/// <returns>ヘッダー</returns>
	public static PMXFormat.Header PMD2PMX(PMDFormat.Header pmd_header) {
		PMXFormat.Header pmx_header = ConvertHeader(pmd_header, null, null);
		return pmx_header;
	}

	/// <summary>
	/// PMXファイルの取得
	/// </summary>
	/// <param name='pmd'>PMDファイル</param>
	/// <returns>内部形式データ</returns>
	public static PMXFormat PMD2PMX(PMDFormat pmd) {
		PMXFormat result = new PMXFormat();
		result.meta_header = CreateMetaHeader(pmd);
		result.header = ConvertHeader(pmd.head, pmd.eg_head, pmd);
		result.vertex_list = ConvertVertexList(pmd);
		result.face_vertex_list = ConvertFaceVertexList(pmd);
		result.texture_list = ConvertTextureList(pmd);
		result.material_list = ConvertMaterialList(pmd, x=>CreateTextureIndex(ref result.texture_list.texture_file, x));
		result.bone_list = ConvertBoneList(pmd); 
		result.morph_list = ConvertMorphList(pmd);
		result.display_frame_list = ConvertDisplayFrameList(pmd);
		result.rigidbody_list = ConvertRigidbodyList(pmd);
		result.rigidbody_joint_list = ConvertRigidbodyJointList(pmd);
		return result;
	}

	/// <summary>
	/// テクスチャインデックス取得用関数
	/// </summary>
	/// <returns>テクスチャインデックス</returns>
	/// <param name="list">テクスチャリスト</param>
	/// <param name="name">検索するテクスチャ名</param>
	private static uint CreateTextureIndex(ref string[] list, string name) {
		uint result = uint.MaxValue;
		for (int i = 0, i_max = list.Length; i < i_max; ++i) {
			if (name == list[i]) {
				//発見したらインデックスを返す
				result = (uint)i;
				break;
			}
		}
		if (uint.MaxValue == result) {
			//未発見なら
			//末尾に登録して返す
			string[] new_list = new string[list.Length + 1];
			System.Array.Copy(list, new_list, list.Length);
			new_list[list.Length] = name; //最後に追加
			result = (uint)list.Length;
			list = new_list;
		}
		return result;
	}
	
	private static PMXFormat.MetaHeader CreateMetaHeader(PMDFormat pmd) {
		PMXFormat.MetaHeader result = new PMXFormat.MetaHeader();
		result.path = pmd.path;
		result.name = pmd.name;
		result.folder = pmd.folder;
		return result;
	}
	
	private static PMXFormat.Header ConvertHeader(PMDFormat.Header pmd_header, PMDFormat.EnglishHeader pmd_english_header, PMDFormat pmd) {
		PMXFormat.Header result = new PMXFormat.Header();
		result.magic = pmd_header.magic;
		result.version = pmd_header.version;

		result.dataSize = 0;
		result.encodeMethod = PMXFormat.Header.StringCode.Utf16le;
		result.additionalUV = 0;
		result.vertexIndexSize = PMXFormat.Header.IndexSize.Byte1;
		result.textureIndexSize = PMXFormat.Header.IndexSize.Byte1;
		result.materialIndexSize = PMXFormat.Header.IndexSize.Byte1;
		result.boneIndexSize = PMXFormat.Header.IndexSize.Byte1;
		result.morphIndexSize = PMXFormat.Header.IndexSize.Byte1;
		result.rigidbodyIndexSize = PMXFormat.Header.IndexSize.Byte1;
		if (null != pmd) {
			result.vertexIndexSize = GetIndexSize(pmd.vertex_list.vertex.Length);
			result.textureIndexSize = GetIndexSize(pmd.toon_texture_list.toon_texture_file.Length);
			result.materialIndexSize = GetIndexSize(pmd.material_list.material.Length);
			result.boneIndexSize = GetIndexSize(pmd.bone_list.bone.Length);
			result.morphIndexSize = GetIndexSize(pmd.skin_list.skin_data.Length);
			result.rigidbodyIndexSize = GetIndexSize(pmd.rigidbody_list.rigidbody.Length);
		}

		result.model_name = pmd_header.model_name;
		result.comment = pmd_header.comment;
		result.model_english_name = "";
		result.english_comment = "";
		if (null != pmd_english_header) {
			result.model_english_name = pmd_english_header.model_name_eg;
			result.english_comment = pmd_english_header.comment_eg;
		} else if (null != pmd) {
			result.model_english_name = pmd.eg_head.model_name_eg;
			result.english_comment = pmd.eg_head.comment_eg;
		}
		return result;
	}

	private static PMXFormat.Header.IndexSize GetIndexSize(int size) {
		PMXFormat.Header.IndexSize result;
		if ((int)ushort.MaxValue <= size) {
			result = PMXFormat.Header.IndexSize.Byte4;
		} else if ((int)byte.MaxValue <= size) {
			result = PMXFormat.Header.IndexSize.Byte2;
		} else {
			result = PMXFormat.Header.IndexSize.Byte1;
		}
		return result;
	}

	private static PMXFormat.VertexList ConvertVertexList(PMDFormat pmd) {
		PMXFormat.VertexList result = new PMXFormat.VertexList();
		result.vertex = pmd.vertex_list.vertex.Select(x=>ConvertVertex(x)).ToArray();
		return result;
	}

	private static PMXFormat.Vertex ConvertVertex(PMDFormat.Vertex pmd_vertex) {
		PMXFormat.Vertex result = new PMXFormat.Vertex();
		result.pos = pmd_vertex.pos;
		result.normal_vec = pmd_vertex.normal_vec;
		result.uv = pmd_vertex.uv;
		result.add_uv = new Vector4[0];
		if (100 == pmd_vertex.bone_weight) {
			//1頂点
			PMXFormat.BDEF1 bone_weight = new PMXFormat.BDEF1();
			bone_weight.bone1_ref = pmd_vertex.bone_num[0];
			result.bone_weight = bone_weight;
		} else {
			//2頂点
			PMXFormat.BDEF2 bone_weight = new PMXFormat.BDEF2();
			bone_weight.bone1_ref = pmd_vertex.bone_num[0];
			bone_weight.bone2_ref = pmd_vertex.bone_num[1];
			bone_weight.bone1_weight = pmd_vertex.bone_weight / 100.0f;
			result.bone_weight = bone_weight;
		}
		result.edge_magnification = (float)pmd_vertex.edge_flag;
		return result;
	}

	private static PMXFormat.FaceVertexList ConvertFaceVertexList(PMDFormat pmd) {
		PMXFormat.FaceVertexList result = new PMXFormat.FaceVertexList();
		result.face_vert_index = pmd.face_vertex_list.face_vert_index.Select(x=>(uint)x).ToArray();
		return result;
	}
	
	private static PMXFormat.TextureList ConvertTextureList(PMDFormat pmd) {
		PMXFormat.TextureList result = new PMXFormat.TextureList();
		result.texture_file = pmd.toon_texture_list.toon_texture_file.ToArray(); //複製する
		return result;
	}

	private static PMXFormat.MaterialList ConvertMaterialList(PMDFormat pmd, System.Func<string, uint> get_texture_index) {
		PMXFormat.MaterialList result = new PMXFormat.MaterialList();
		result.material = new PMXFormat.Material[pmd.material_list.material.Length];
		for (int i = 0; i < result.material.Length; i++) {
			result.material[i] = ConvertMaterial(pmd, i, get_texture_index);
		}
		return result;
	}
	
	private static PMXFormat.Material ConvertMaterial(PMDFormat pmd, int material_index, System.Func<string, uint> get_texture_index) {
		PMXFormat.Material result = new PMXFormat.Material();
		PMDFormat.Material pmd_material = pmd.material_list.material[material_index];

		result.name = "材質_" + material_index.ToString();
		result.english_name = "Material_" + material_index.ToString();
		result.diffuse_color = new Color(pmd_material.diffuse_color.r, pmd_material.diffuse_color.g, pmd_material.diffuse_color.b, pmd_material.alpha);
		result.specularity = pmd_material.specularity;
		result.specular_color = pmd_material.specular_color;
		result.ambient_color = pmd_material.mirror_color;
		result.flag = new PMXFormat.Material.Flag();
		if (pmd_material.alpha < 1.0f) {
			result.flag |= PMXFormat.Material.Flag.Reversible;
		}
		if (0 != pmd_material.edge_flag) {
			result.flag |= PMXFormat.Material.Flag.Edge | PMXFormat.Material.Flag.CastShadow | PMXFormat.Material.Flag.CastSelfShadow;
		}
		if (!(0.98f == pmd_material.alpha)) { //浮動小数点の比較だけど、0.98fとの同値確認でPMXエディタの0.98と一致したので一旦これで。
			result.flag |= PMXFormat.Material.Flag.ReceiveSelfShadow;
		}
		result.edge_color = Color.black;
		result.edge_size = 1.0f;
		result.usually_texture_index = uint.MaxValue;
		if (!string.IsNullOrEmpty(pmd_material.texture_file_name)) {
			result.usually_texture_index = get_texture_index(pmd_material.texture_file_name);
		}
		result.sphere_texture_index = uint.MaxValue;
		result.sphere_mode = PMXFormat.Material.SphereMode.Null;
		if (!string.IsNullOrEmpty(pmd_material.sphere_map_name)) {
			result.sphere_texture_index = get_texture_index(pmd_material.sphere_map_name);
			switch (System.IO.Path.GetExtension(pmd_material.sphere_map_name)) {
			case ".sph":	result.sphere_mode = PMXFormat.Material.SphereMode.MulSphere;	break;
			case ".spa":	result.sphere_mode = PMXFormat.Material.SphereMode.AddSphere;	break;
			default:																		break;
			}
		}
		result.common_toon = pmd_material.toon_index;
		result.toon_texture_index = ((0 < pmd_material.toon_index)? pmd_material.toon_index: uint.MaxValue);
		result.memo = "";
		result.face_vert_count = pmd_material.face_vert_count;
		return result;
	}

	private static PMXFormat.BoneList ConvertBoneList(PMDFormat pmd) {
		PMXFormat.BoneList result = new PMXFormat.BoneList();
		result.bone = new PMXFormat.Bone[pmd.bone_list.bone.Length];
		for (int i = 0, i_max = pmd.bone_list.bone.Length; i < i_max; ++i) {
			result.bone[i] = ConvertBone(pmd, i);
		}
		return result;
	}

	private static PMXFormat.Bone ConvertBone(PMDFormat pmd, int bone_index) {
		PMXFormat.Bone result = new PMXFormat.Bone();
		PMDFormat.Bone pmd_bone = pmd.bone_list.bone[bone_index];

		result.bone_name = pmd_bone.bone_name;
		result.bone_english_name = ((null != pmd.eg_bone_name_list)? pmd.eg_bone_name_list.bone_name_eg[bone_index]: null);
		result.bone_position = pmd_bone.bone_head_pos;
		result.parent_bone_index = ((ushort.MaxValue == pmd_bone.parent_bone_index)? uint.MaxValue: (uint)pmd_bone.parent_bone_index);
		result.transform_level = 0;
		switch (pmd_bone.bone_type) {
		case 0: //回転
			result.bone_flag = PMXFormat.Bone.Flag.Movable | PMXFormat.Bone.Flag.DisplayFlag | PMXFormat.Bone.Flag.CanOperate;
			break;
		case 1: //回転と移動
			result.bone_flag = PMXFormat.Bone.Flag.Movable | PMXFormat.Bone.Flag.Rotatable | PMXFormat.Bone.Flag.DisplayFlag | PMXFormat.Bone.Flag.CanOperate;
			break;
		case 2: //IK
			result.bone_flag = PMXFormat.Bone.Flag.IkFlag | PMXFormat.Bone.Flag.DisplayFlag | PMXFormat.Bone.Flag.CanOperate;
			break;
		case 3: //不明
			goto default;
		case 4: //IK影響下
			goto default;
		case 5: //回転影響下
			goto default; //付与親に変換しないといけないのかも
		case 6: //IK接続先
			goto default;
		case 7: //非表示
			goto default;
		case 8: //捻り
			goto default;
		case 9: //回転運動
			goto default;
		default:
			result.bone_flag = new PMXFormat.Bone.Flag();
			break;
		}
		result.position_offset = Vector3.zero;
		result.connection_index = 0;
		result.additional_parent_index = 0;
		result.additional_rate = 0.0f;
		result.axis_vector = Vector3.zero;
		result.x_axis_vector = Vector3.zero;
		result.z_axis_vector = Vector3.zero;
		result.key_value = 0;
		result.ik_data = ConvertIKData(pmd.ik_list.ik_data.Where(x=>x.ik_bone_index==bone_index).FirstOrDefault());
		return result;
	}

	private static PMXFormat.IK_Data ConvertIKData(PMDFormat.IK pmd_ik) {
		PMXFormat.IK_Data result = null;
		if (null != pmd_ik) {
			result = new PMXFormat.IK_Data();
			result.ik_bone_index = pmd_ik.ik_target_bone_index;
			result.iterations = pmd_ik.iterations;
			result.limit_angle = pmd_ik.control_weight * 4.0f; //PMXConverter側で4倍されるので逆補正
			result.ik_link = pmd_ik.ik_child_bone_index.Select(x=>ConvertIKLink(x)).ToArray();
		}
		return result;
	}
	
	private static PMXFormat.IK_Link ConvertIKLink(ushort ik_child_bone_index) {
		PMXFormat.IK_Link result = new PMXFormat.IK_Link();
		result.target_bone_index = ik_child_bone_index;
		return result;
	}
	
	private static PMXFormat.MorphList ConvertMorphList(PMDFormat pmd) {
		PMXFormat.MorphList result = new PMXFormat.MorphList();
		//頂点インデックス用辞書の作成
		PMDFormat.SkinData pmd_skin_data_base = pmd.skin_list.skin_data.Where(x=>0==x.skin_type).First();
		Dictionary<uint, uint> morph_vertex_index_dictionary = new Dictionary<uint, uint>(pmd_skin_data_base.skin_vert_data.Length);
		for (uint i = 0, i_max = (uint)pmd_skin_data_base.skin_vert_data.Length; i < i_max; ++i) {
			morph_vertex_index_dictionary.Add(i, pmd_skin_data_base.skin_vert_data[i].skin_vert_index);
		}
		//base以外の変換
		result.morph_data = new PMXFormat.MorphData[pmd.skin_list.skin_data.Where(x=>0!=x.skin_type).Count()]; //base分を除外
		int morph_data_count = 0;
		for (int i = 0, i_max = pmd.skin_list.skin_data.Length; i < i_max; ++i) {
			if (0 != pmd.skin_list.skin_data[i].skin_type) {
				//base以外なら
				string eg_skin_name = (((null != pmd.eg_skin_name_list) && (1 <= i))? pmd.eg_skin_name_list.skin_name_eg[i - 1]: null);
				result.morph_data[morph_data_count++] = ConvertMorphData(pmd.skin_list.skin_data[i], eg_skin_name, morph_vertex_index_dictionary);
			}
		}
		return result;
	}

	private static PMXFormat.MorphData ConvertMorphData(PMDFormat.SkinData pmd_skin, string pmd_eg_skin_name, Dictionary<uint, uint> morph_vertex_index_dictionary) {
		PMXFormat.MorphData result = new PMXFormat.MorphData();
		result.morph_name = pmd_skin.skin_name;
		result.morph_english_name = pmd_eg_skin_name;
		result.handle_panel = (PMXFormat.MorphData.Panel)pmd_skin.skin_type;
		result.morph_type = PMXFormat.MorphData.MorphType.Vertex;
		result.morph_offset = pmd_skin.skin_vert_data.Select(x=>ConvertVertexMorphOffset(x, morph_vertex_index_dictionary)).ToArray();
		return result;
	}

	private static PMXFormat.VertexMorphOffset ConvertVertexMorphOffset(PMDFormat.SkinVertexData pmd_skin_vertex_data, Dictionary<uint, uint> morph_vertex_index_dictionary) {
		PMXFormat.VertexMorphOffset result = new PMXFormat.VertexMorphOffset();
		result.vertex_index = morph_vertex_index_dictionary[pmd_skin_vertex_data.skin_vert_index];
		result.position_offset = pmd_skin_vertex_data.skin_vert_pos;
		return result;
	}

	private static PMXFormat.DisplayFrameList ConvertDisplayFrameList(PMDFormat pmd) {
		PMXFormat.DisplayFrameList result = new PMXFormat.DisplayFrameList();
		result.display_frame = new PMXFormat.DisplayFrame[pmd.bone_display_list.bone_disp.Length];
		for (int i = 0, i_max = result.display_frame.Length; i < i_max; ++i) {
			result.display_frame[i] = ConvertDisplayFrame(pmd, i);
		}
		return result;
	}

	private static PMXFormat.DisplayFrame ConvertDisplayFrame(PMDFormat pmd, int bone_display_index) {
		PMXFormat.DisplayFrame result = new PMXFormat.DisplayFrame();
		PMDFormat.BoneDisplay pmd_bone_display = pmd.bone_display_list.bone_disp[bone_display_index];
		result.display_name = pmd.bone_name_list.disp_name[pmd_bone_display.bone_disp_frame_index - 1];
		result.display_english_name = ((null != pmd.eg_bone_display_list)? pmd.eg_bone_display_list.disp_name_eg[pmd_bone_display.bone_disp_frame_index - 1]: null);
		result.special_frame_flag = new byte();
		result.display_element = new []{new PMXFormat.DisplayElement()};
		result.display_element[0].element_target = pmd_bone_display.bone_disp_frame_index;
		result.display_element[0].element_target_index = pmd_bone_display.bone_index;
		return result;
	}

	private static PMXFormat.RigidbodyList ConvertRigidbodyList(PMDFormat pmd) {
		PMXFormat.RigidbodyList result = new PMXFormat.RigidbodyList();
		result.rigidbody = new PMXFormat.Rigidbody[pmd.rigidbody_list.rigidbody.Length];
		for (int i = 0, i_max = result.rigidbody.Length; i < i_max; ++i) {
			result.rigidbody[i] = ConvertRigidbody(pmd, i);
		}
		return result;
	}
	
	private static PMXFormat.Rigidbody ConvertRigidbody(PMDFormat pmd, int rigidbody_index) {
		PMXFormat.Rigidbody result = new PMXFormat.Rigidbody();
		PMDFormat.Rigidbody pmd_rigidbody = pmd.rigidbody_list.rigidbody[rigidbody_index];
		result.name = pmd_rigidbody.rigidbody_name;
		result.english_name = "";
		result.rel_bone_index = (uint)pmd_rigidbody.rigidbody_rel_bone_index;
		result.group_index = pmd_rigidbody.rigidbody_group_index;
		result.ignore_collision_group = pmd_rigidbody.rigidbody_group_target;
		result.shape_type = (PMXFormat.Rigidbody.ShapeType)pmd_rigidbody.shape_type;
		result.shape_size = new Vector3(pmd_rigidbody.shape_w, pmd_rigidbody.shape_h, pmd_rigidbody.shape_d);
		result.collider_position = pmd_rigidbody.pos_pos;
		if (pmd_rigidbody.rigidbody_rel_bone_index < pmd.bone_list.bone.Length) {
			result.collider_position += pmd.bone_list.bone[pmd_rigidbody.rigidbody_rel_bone_index].bone_head_pos;
		} else {
			result.collider_position += pmd.bone_list.bone[0].bone_head_pos;
		}
		result.collider_rotation = pmd_rigidbody.pos_rot;
		result.weight = pmd_rigidbody.rigidbody_weight;
		result.position_dim = pmd_rigidbody.rigidbody_pos_dim;
		result.rotation_dim = pmd_rigidbody.rigidbody_rot_dim;
		result.recoil = pmd_rigidbody.rigidbody_recoil;
		result.friction = pmd_rigidbody.rigidbody_friction;
		result.operation_type = (PMXFormat.Rigidbody.OperationType)pmd_rigidbody.rigidbody_type;
		return result;
	}
	
	private static PMXFormat.RigidbodyJointList ConvertRigidbodyJointList(PMDFormat pmx) {
		PMXFormat.RigidbodyJointList result = new PMXFormat.RigidbodyJointList();
		result.joint = pmx.rigidbody_joint_list.joint.Select(x=>ConvertJoint(x)).ToArray();
		return result;
	}
	
	private static PMXFormat.Joint ConvertJoint(PMDFormat.Joint pmd_joint) {
		PMXFormat.Joint result = new PMXFormat.Joint();
		result.name = pmd_joint.joint_name;
		result.english_name = "";
		result.operation_type = PMXFormat.Joint.OperationType.Spring6DOF;
		result.rigidbody_a = pmd_joint.joint_rigidbody_a;
		result.rigidbody_b = pmd_joint.joint_rigidbody_b;
		result.position = pmd_joint.joint_pos;
		result.rotation = pmd_joint.joint_rot;
		result.constrain_pos_lower = pmd_joint.constrain_pos_1;
		result.constrain_pos_upper = pmd_joint.constrain_pos_2;
		result.constrain_rot_lower = pmd_joint.constrain_rot_1;
		result.constrain_rot_upper = pmd_joint.constrain_rot_2;
		result.spring_position = pmd_joint.spring_pos;
		result.spring_rotation = pmd_joint.spring_rot;
		return result;
	}
}
