using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MMD.PMX;
using MMD.PMD;
	
public partial class PMXLoaderScript {

	/// <summary>
	/// PMXファイルのヘッダー取得
	/// </summary>
	/// <param name='file_path'>PMDファイルのパス</param>
	/// <returns>ヘッダー</returns>
	public static PMXFormat.Header GetHeader(string file_path) {
		PMXLoaderScript loader = new PMXLoaderScript();
		return loader.GetHeader_(file_path);
	}

	/// <summary>
	/// PMXファイルのインポート
	/// </summary>
	/// <param name='file_path'>PMDファイルのパス</param>
	/// <returns>内部形式データ</returns>
	public static PMXFormat Import(string file_path) {
		PMXLoaderScript loader = new PMXLoaderScript();
		return loader.Import_(file_path);
	}

	/// <summary>
	/// デフォルトコンストラクタ
	/// </summary>
	/// <remarks>
	/// ユーザーに依るインスタンス作成を禁止する
	/// </remarks>
	private PMXLoaderScript() {}

	private PMXFormat.Header GetHeader_(string file_path) {
		PMXFormat.Header result;
		using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
		using (BinaryReader bin = new BinaryReader(stream)) {
			file_path_ = null;
			binary_reader_ = bin;
			result = ReadHeader();
		}
		return result;
	}

	private PMXFormat Import_(string file_path) {
		using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
		using (BinaryReader bin = new BinaryReader(stream)) {
			file_path_ = file_path;
			binary_reader_ = bin;
			Read();
		}
		return format_;
	}

	private PMXFormat Read() {
		format_ = new PMXFormat();
		format_.meta_header = CreateMetaHeader();
		format_.header = ReadHeader();
		format_.vertex_list = ReadVertexList();
		format_.face_vertex_list = ReadFaceVertexList();
		format_.texture_list = ReadTextureList();
		format_.material_list = ReadMaterialList();
		format_.bone_list = ReadBoneList(); 
		format_.morph_list = ReadMorphList();
		format_.display_frame_list = ReadDisplayFrameList();
		format_.rigidbody_list = ReadRigidbodyList();
		format_.rigidbody_joint_list = ReadRigidbodyJointList();
		return format_;
	}

	private PMXFormat.MetaHeader CreateMetaHeader() {
		PMXFormat.MetaHeader result = new PMXFormat.MetaHeader();
		result.path = file_path_;
		result.name = Path.GetFileNameWithoutExtension(file_path_); // .pmdを抜かす
		result.folder = Path.GetDirectoryName(file_path_); // PMDが格納されているフォルダ
		return result;
	}
	
	private PMXFormat.Header ReadHeader() {
		PMXFormat.Header result = new PMXFormat.Header();
		result.magic = binary_reader_.ReadBytes(4);
		if (Encoding.ASCII.GetString(result.magic) != "PMX ") {
			throw new System.FormatException();
		}
		result.version = binary_reader_.ReadSingle();
		binary_reader_.ReadByte();
		result.encodeMethod = (PMXFormat.Header.StringCode)binary_reader_.ReadByte();
		result.additionalUV = binary_reader_.ReadByte();
		result.vertexIndexSize = (PMXFormat.Header.IndexSize)binary_reader_.ReadByte();
		result.textureIndexSize = (PMXFormat.Header.IndexSize)binary_reader_.ReadByte();
		result.materialIndexSize = (PMXFormat.Header.IndexSize)binary_reader_.ReadByte();
		result.boneIndexSize = (PMXFormat.Header.IndexSize)binary_reader_.ReadByte();
		result.morphIndexSize = (PMXFormat.Header.IndexSize)binary_reader_.ReadByte();
		result.rigidbodyIndexSize = (PMXFormat.Header.IndexSize)binary_reader_.ReadByte();
		
		string_code_ = result.encodeMethod;
		result.model_name = ReadString();
		result.model_english_name = ReadString();
		result.comment = ReadString();
		result.english_comment = ReadString();

		return result;
	}

	private PMXFormat.VertexList ReadVertexList() {
		PMXFormat.VertexList result = new PMXFormat.VertexList();
		uint vert_count = binary_reader_.ReadUInt32();
		result.vertex = new PMXFormat.Vertex[vert_count];
		for (uint i = 0, i_max = (uint)result.vertex.Length; i < i_max; ++i) {
			result.vertex[i] = ReadVertex();
		}
		return result;
	}

	private PMXFormat.Vertex ReadVertex() {
		PMXFormat.Vertex result = new PMXFormat.Vertex();
		result.pos = ReadSinglesToVector3(binary_reader_);
		result.normal_vec = ReadSinglesToVector3(binary_reader_);
		result.uv = ReadSinglesToVector2(binary_reader_);
		result.add_uv = new Vector4[format_.header.additionalUV];
		for (int i = 0; i < format_.header.additionalUV; i++) {
			result.add_uv[i] = ReadSinglesToVector4(binary_reader_);
		}
		PMXFormat.Vertex.WeightMethod weight_method = (PMXFormat.Vertex.WeightMethod)binary_reader_.ReadByte();
		switch(weight_method) {
		case PMXFormat.Vertex.WeightMethod.BDEF1:
			result.bone_weight = ReadBoneWeightBDEF1();
			break;
		case PMXFormat.Vertex.WeightMethod.BDEF2:
			result.bone_weight = ReadBoneWeightBDEF2();
			break;
		case PMXFormat.Vertex.WeightMethod.BDEF4:
			result.bone_weight = ReadBoneWeightBDEF4();
			break;
		case PMXFormat.Vertex.WeightMethod.SDEF:
			result.bone_weight = ReadBoneWeightSDEF();
			break;
		case PMXFormat.Vertex.WeightMethod.QDEF:
			result.bone_weight = ReadBoneWeightQDEF();
			break;
		default:
			result.bone_weight = null;
			throw new System.FormatException();
		}
		result.edge_magnification = binary_reader_.ReadSingle();
		return result;
	}

	private PMXFormat.BoneWeight ReadBoneWeightBDEF1() {
		PMXFormat.BDEF1 result = new PMXFormat.BDEF1();
		result.bone1_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		return result;
	}
	private PMXFormat.BoneWeight ReadBoneWeightBDEF2() {
		PMXFormat.BDEF2 result = new PMXFormat.BDEF2();
		result.bone1_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone2_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone1_weight = binary_reader_.ReadSingle();
		return result;
	}
	private PMXFormat.BoneWeight ReadBoneWeightBDEF4() {
		PMXFormat.BDEF4 result = new PMXFormat.BDEF4();
		result.bone1_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone2_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone3_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone4_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone1_weight = binary_reader_.ReadSingle();
		result.bone2_weight = binary_reader_.ReadSingle();
		result.bone3_weight = binary_reader_.ReadSingle();
		result.bone4_weight = binary_reader_.ReadSingle();
		return result;
	}
	private PMXFormat.BoneWeight ReadBoneWeightSDEF() {
		PMXFormat.SDEF result = new PMXFormat.SDEF();
		result.bone1_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone2_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone1_weight = binary_reader_.ReadSingle();
		result.c_value = ReadSinglesToVector3(binary_reader_);
		result.r0_value = ReadSinglesToVector3(binary_reader_);
		result.r1_value = ReadSinglesToVector3(binary_reader_);
		return result;
	}
	private PMXFormat.BoneWeight ReadBoneWeightQDEF() {
		PMXFormat.BDEF4 result = new PMXFormat.BDEF4();
		result.bone1_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone2_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone3_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone4_ref = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.bone1_weight = binary_reader_.ReadSingle();
		result.bone2_weight = binary_reader_.ReadSingle();
		result.bone3_weight = binary_reader_.ReadSingle();
		result.bone4_weight = binary_reader_.ReadSingle();
		return result;
	}
	
	private PMXFormat.FaceVertexList ReadFaceVertexList() {
		PMXFormat.FaceVertexList result = new PMXFormat.FaceVertexList();
		uint face_vert_count = binary_reader_.ReadUInt32();
		result.face_vert_index = new uint[face_vert_count];
		for (uint i = 0, i_max = (uint)result.face_vert_index.Length; i < i_max; ++i) {
			result.face_vert_index[i] = CastIntRead(binary_reader_, format_.header.vertexIndexSize);
		}
		return result;
	}

	private PMXFormat.TextureList ReadTextureList() {
		PMXFormat.TextureList result = new PMXFormat.TextureList();
		uint texture_file_count = binary_reader_.ReadUInt32();
		result.texture_file = new string[texture_file_count];
		for (uint i = 0, i_max = (uint)result.texture_file.Length; i < i_max; ++i) {
			result.texture_file[i] = ReadString();
			//"./"開始なら削除する
			if (('.' == result.texture_file[i][0]) && (1 == result.texture_file[i].IndexOfAny(new[]{'/', '\\'}, 1, 1))) {
				result.texture_file[i] = result.texture_file[i].Substring(2);
			}
		}
		return result;
	}

	private PMXFormat.MaterialList ReadMaterialList() {
		PMXFormat.MaterialList result = new PMXFormat.MaterialList();
		uint material_count = binary_reader_.ReadUInt32();
		result.material = new PMXFormat.Material[material_count];
		for (uint i = 0, i_max = (uint)result.material.Length; i < i_max; ++i) {
			result.material[i] = ReadMaterial();
		}
		return result;
	}
	
	private PMXFormat.Material ReadMaterial() {
		PMXFormat.Material result = new PMXFormat.Material();
		result.name = ReadString();
		result.english_name = ReadString();
		result.diffuse_color = ReadSinglesToColor(binary_reader_); // dr, dg, db, da // 減衰色
		result.specular_color = ReadSinglesToColor(binary_reader_, 1); // sr, sg, sb // 光沢色
		result.specularity = binary_reader_.ReadSingle();
		result.ambient_color = ReadSinglesToColor(binary_reader_, 1); // mr, mg, mb // 環境色(ambient)
		result.flag = (PMXFormat.Material.Flag)binary_reader_.ReadByte();
		result.edge_color = ReadSinglesToColor(binary_reader_); // r, g, b, a
		result.edge_size = binary_reader_.ReadSingle();
		result.usually_texture_index = CastIntRead(binary_reader_, format_.header.textureIndexSize);
		result.sphere_texture_index = CastIntRead(binary_reader_, format_.header.textureIndexSize);
		result.sphere_mode = (PMXFormat.Material.SphereMode)binary_reader_.ReadByte();
		result.common_toon = binary_reader_.ReadByte();
		PMXFormat.Header.IndexSize texture_index_size = ((result.common_toon == 0)? format_.header.textureIndexSize: PMXFormat.Header.IndexSize.Byte1);
		result.toon_texture_index = CastIntRead(binary_reader_, texture_index_size);
		result.memo = ReadString();
		result.face_vert_count = binary_reader_.ReadUInt32(); // 面頂点数 // インデックスに変換する場合は、材質0から順に加算
		return result;
	}

	private PMXFormat.BoneList ReadBoneList() {
		PMXFormat.BoneList result = new PMXFormat.BoneList();
		uint bone_count = binary_reader_.ReadUInt32();
		result.bone = new PMXFormat.Bone[bone_count];
		for (uint i = 0, i_max = (uint)result.bone.Length; i < i_max; ++i) {
			result.bone[i] = ReadBone();
		}
		return result;
	}

	private PMXFormat.Bone ReadBone() {
		PMXFormat.Bone result = new PMXFormat.Bone();
		result.bone_name = ReadString();
		result.bone_english_name = ReadString();
		result.bone_position = ReadSinglesToVector3(binary_reader_);
		result.parent_bone_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.transform_level = binary_reader_.ReadInt32();
		result.bone_flag = (PMXFormat.Bone.Flag)binary_reader_.ReadUInt16();
		
		if ((result.bone_flag & PMXFormat.Bone.Flag.Connection) == 0) {
			// 座標オフセットで指定
			result.position_offset = ReadSinglesToVector3(binary_reader_);
		} else {
			// ボーンで指定
			result.connection_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		}
		if ((result.bone_flag & (PMXFormat.Bone.Flag.AddRotation | PMXFormat.Bone.Flag.AddMove)) != 0) {
			result.additional_parent_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
			result.additional_rate = binary_reader_.ReadSingle();
		}
		if ((result.bone_flag & PMXFormat.Bone.Flag.FixedAxis) != 0) {
			result.axis_vector = ReadSinglesToVector3(binary_reader_);
		}
		if ((result.bone_flag & PMXFormat.Bone.Flag.LocalAxis) != 0) {
			result.x_axis_vector = ReadSinglesToVector3(binary_reader_);
			result.z_axis_vector = ReadSinglesToVector3(binary_reader_);
		}
		if((result.bone_flag & PMXFormat.Bone.Flag.ExternalParentTransform) != 0) {
			result.key_value = binary_reader_.ReadUInt32();
		}
		if((result.bone_flag & PMXFormat.Bone.Flag.IkFlag) != 0) {
			result.ik_data = ReadIkData();
		}
		return result;
	}

	private PMXFormat.IK_Data ReadIkData() {
		PMXFormat.IK_Data result = new PMXFormat.IK_Data();
		result.ik_bone_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.iterations = binary_reader_.ReadUInt32();
		result.limit_angle = binary_reader_.ReadSingle();
		uint ik_link_count = binary_reader_.ReadUInt32();
		result.ik_link = new PMXFormat.IK_Link[ik_link_count];
		for (uint i = 0, i_max = (uint)result.ik_link.Length; i < i_max; ++i) {
			result.ik_link[i] = ReadIkLink();
		}
		return result;
	}

	private PMXFormat.IK_Link ReadIkLink() {
		PMXFormat.IK_Link result = new PMXFormat.IK_Link();
		result.target_bone_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.angle_limit = binary_reader_.ReadByte();
		if (result.angle_limit == 1) {
			result.lower_limit = ReadSinglesToVector3(binary_reader_);
			result.upper_limit = ReadSinglesToVector3(binary_reader_);
		}
		return result;
	}

	private PMXFormat.MorphList ReadMorphList() {
		PMXFormat.MorphList result = new PMXFormat.MorphList();
		uint morph_count = binary_reader_.ReadUInt32();
		result.morph_data = new PMXFormat.MorphData[morph_count];
		for (uint i = 0, i_max = (uint)result.morph_data.Length; i < i_max; ++i) {
			result.morph_data[i] = ReadMorphData();
		}
		return result;
	}

	private PMXFormat.MorphData ReadMorphData() {
		PMXFormat.MorphData result = new PMXFormat.MorphData();
		result.morph_name = ReadString();
		result.morph_english_name = ReadString();
		result.handle_panel = (PMXFormat.MorphData.Panel)binary_reader_.ReadByte();
		result.morph_type = (PMXFormat.MorphData.MorphType)binary_reader_.ReadByte();
		uint morph_offset_count = binary_reader_.ReadUInt32();
		result.morph_offset = new PMXFormat.MorphOffset[morph_offset_count];
		for (uint i = 0, i_max = (uint)result.morph_offset.Length; i < i_max; ++i) {
			switch(result.morph_type) {
			case PMXFormat.MorphData.MorphType.Group:
			case PMXFormat.MorphData.MorphType.Flip:
				result.morph_offset[i] = ReadGroupMorphOffset();
				break;
			case PMXFormat.MorphData.MorphType.Vertex:
				result.morph_offset[i] = ReadVertexMorphOffset();
				break;
			case PMXFormat.MorphData.MorphType.Bone:
				result.morph_offset[i] = ReadBoneMorphOffset();
				break;
			case PMXFormat.MorphData.MorphType.Uv:
			case PMXFormat.MorphData.MorphType.Adduv1:
			case PMXFormat.MorphData.MorphType.Adduv2:
			case PMXFormat.MorphData.MorphType.Adduv3:
			case PMXFormat.MorphData.MorphType.Adduv4:
				result.morph_offset[i] = ReadUVMorphOffset();
				break;
			case PMXFormat.MorphData.MorphType.Material:
				result.morph_offset[i] = ReadMaterialMorphOffset();
				break;
			case PMXFormat.MorphData.MorphType.Impulse:
				result.morph_offset[i] = ReadImpulseMorphOffset();
				break;
			default:
				throw new System.FormatException();
			}
		}
		return result;
	}
	private PMXFormat.MorphOffset ReadGroupMorphOffset() {
		PMXFormat.GroupMorphOffset result = new PMXFormat.GroupMorphOffset();
		result.morph_index = CastIntRead(binary_reader_, format_.header.morphIndexSize);
		result.morph_rate = binary_reader_.ReadSingle();
		return result;
	}
	private PMXFormat.MorphOffset ReadVertexMorphOffset() {
		PMXFormat.VertexMorphOffset result = new PMXFormat.VertexMorphOffset();
		result.vertex_index = CastIntRead(binary_reader_, format_.header.vertexIndexSize);
		result.position_offset = ReadSinglesToVector3(binary_reader_);
		return result;
	}
	private PMXFormat.MorphOffset ReadBoneMorphOffset() {
		PMXFormat.BoneMorphOffset result = new PMXFormat.BoneMorphOffset();
		result.bone_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.move_value = ReadSinglesToVector3(binary_reader_);
		result.rotate_value = ReadSinglesToQuaternion(binary_reader_);
		return result;
	}
	private PMXFormat.MorphOffset ReadUVMorphOffset() {
		PMXFormat.UVMorphOffset result = new PMXFormat.UVMorphOffset();
		result.vertex_index = CastIntRead(binary_reader_, format_.header.vertexIndexSize);
		result.uv_offset = ReadSinglesToVector4(binary_reader_);
		return result;
	}
	private PMXFormat.MorphOffset ReadMaterialMorphOffset() {
		PMXFormat.MaterialMorphOffset result = new PMXFormat.MaterialMorphOffset();
		result.material_index = CastIntRead(binary_reader_, format_.header.materialIndexSize);
		result.offset_method = (PMXFormat.MaterialMorphOffset.OffsetMethod)binary_reader_.ReadByte();
		result.diffuse = ReadSinglesToColor(binary_reader_);
		result.specular = ReadSinglesToColor(binary_reader_, 1);
		result.specularity = binary_reader_.ReadSingle();
		result.ambient = ReadSinglesToColor(binary_reader_, 1);
		result.edge_color = ReadSinglesToColor(binary_reader_);
		result.edge_size = binary_reader_.ReadSingle();
		result.texture_coefficient = ReadSinglesToColor(binary_reader_);
		result.sphere_texture_coefficient = ReadSinglesToColor(binary_reader_);
		result.toon_texture_coefficient = ReadSinglesToColor(binary_reader_);
		return result;
	}
	private PMXFormat.MorphOffset ReadImpulseMorphOffset() {
		PMXFormat.ImpulseMorphOffset result = new PMXFormat.ImpulseMorphOffset();
		result.rigidbody_index = CastIntRead(binary_reader_, format_.header.morphIndexSize);
		result.local_flag = binary_reader_.ReadByte();
		result.move_velocity = ReadSinglesToVector3(binary_reader_);
		result.rotation_torque = ReadSinglesToVector3(binary_reader_);
		return result;
	}

	private PMXFormat.DisplayFrameList ReadDisplayFrameList() {
		PMXFormat.DisplayFrameList result = new PMXFormat.DisplayFrameList();
		uint display_frame_count = binary_reader_.ReadUInt32();
		result.display_frame = new PMXFormat.DisplayFrame[display_frame_count];
		for (uint i = 0, i_max = (uint)result.display_frame.Length; i < i_max; ++i) {
			result.display_frame[i] = ReadDisplayFrame();
		}
		return result;
	}

				
	private PMXFormat.DisplayFrame ReadDisplayFrame() {
		PMXFormat.DisplayFrame result = new PMXFormat.DisplayFrame();
		result.display_name = ReadString();
		result.display_english_name = ReadString();
		result.special_frame_flag = binary_reader_.ReadByte();
		uint display_element_count = binary_reader_.ReadUInt32();
		result.display_element = new PMXFormat.DisplayElement[display_element_count];
		for (uint i = 0, i_max = (uint)result.display_element.Length; i < i_max; ++i) {
			result.display_element[i] = ReadDisplayElement();
		}
		return result;
	}
	
	private PMXFormat.DisplayElement ReadDisplayElement() {
		PMXFormat.DisplayElement result = new PMXFormat.DisplayElement();
		result.element_target = binary_reader_.ReadByte();
		PMXFormat.Header.IndexSize element_target_index_size = ((result.element_target == 0) ? format_.header.boneIndexSize : format_.header.morphIndexSize);
		result.element_target_index = CastIntRead(binary_reader_, element_target_index_size);
		return result;
	}
	
	private PMXFormat.RigidbodyList ReadRigidbodyList() {
		PMXFormat.RigidbodyList result = new PMXFormat.RigidbodyList();
		uint rigidbody_count = binary_reader_.ReadUInt32();
		result.rigidbody = new PMXFormat.Rigidbody[rigidbody_count];
		for (uint i = 0, i_max = (uint)result.rigidbody.Length; i < i_max; ++i) {
			result.rigidbody[i] = ReadRigidbody();
		}
		return result;
	}
	
	private PMXFormat.Rigidbody ReadRigidbody() {
		PMXFormat.Rigidbody result = new PMXFormat.Rigidbody();
		result.name = ReadString();
		result.english_name = ReadString();
		result.rel_bone_index = CastIntRead(binary_reader_, format_.header.boneIndexSize);
		result.group_index = binary_reader_.ReadByte();
		result.ignore_collision_group = binary_reader_.ReadUInt16();
		result.shape_type = (PMXFormat.Rigidbody.ShapeType)binary_reader_.ReadByte();
		result.shape_size = ReadSinglesToVector3(binary_reader_);
		result.collider_position = ReadSinglesToVector3(binary_reader_);
		result.collider_rotation = ReadSinglesToVector3(binary_reader_);
		result.weight = binary_reader_.ReadSingle();
		result.position_dim = binary_reader_.ReadSingle();
		result.rotation_dim = binary_reader_.ReadSingle();
		result.recoil = binary_reader_.ReadSingle();
		result.friction = binary_reader_.ReadSingle();
		result.operation_type = (PMXFormat.Rigidbody.OperationType)binary_reader_.ReadByte();
		return result;
	}
	
	private PMXFormat.RigidbodyJointList ReadRigidbodyJointList() {
		PMXFormat.RigidbodyJointList result = new PMXFormat.RigidbodyJointList();
		uint joint_count = binary_reader_.ReadUInt32();
		result.joint = new PMXFormat.Joint[joint_count];
		for (uint i = 0, i_max = (uint)result.joint.Length; i < i_max; ++i) {
			result.joint[i] = ReadJoint();
		}
		return result;
	}
	
	private PMXFormat.Joint ReadJoint() {
		PMXFormat.Joint result = new PMXFormat.Joint();
		result.name = ReadString();
		result.english_name = ReadString();
		result.operation_type = (PMXFormat.Joint.OperationType)binary_reader_.ReadByte();
		switch (result.operation_type) {
		case PMXFormat.Joint.OperationType.Spring6DOF:
			result.rigidbody_a = CastIntRead(binary_reader_, format_.header.rigidbodyIndexSize);
			result.rigidbody_b = CastIntRead(binary_reader_, format_.header.rigidbodyIndexSize);
			result.position = ReadSinglesToVector3(binary_reader_);
			result.rotation = ReadSinglesToVector3(binary_reader_);
			result.constrain_pos_lower = ReadSinglesToVector3(binary_reader_);
			result.constrain_pos_upper = ReadSinglesToVector3(binary_reader_);
			result.constrain_rot_lower = ReadSinglesToVector3(binary_reader_);
			result.constrain_rot_upper = ReadSinglesToVector3(binary_reader_);
			result.spring_position = ReadSinglesToVector3(binary_reader_);
			result.spring_rotation = ReadSinglesToVector3(binary_reader_);
			break;
		default:
			//empty.
			break;
		}
		return result;
	}

	private string ReadString()
	{
		string result;
		int stringLength = binary_reader_.ReadInt32();
		byte[] buf = binary_reader_.ReadBytes(stringLength);
		switch (string_code_) {
		case PMXFormat.Header.StringCode.Utf16le:
			result = Encoding.Unicode.GetString(buf);
			break;
		case PMXFormat.Header.StringCode.Utf8:
			result = Encoding.UTF8.GetString(buf);
			break;
		default:
			throw new System.InvalidOperationException();
		}
		return result;
	}

	private uint CastIntRead(BinaryReader bin, PMXFormat.Header.IndexSize index_size)
	{
		uint result = 0;
		switch(index_size) {
		case PMXFormat.Header.IndexSize.Byte1:
			result = (uint)binary_reader_.ReadByte();
			if (byte.MaxValue == result) {
				result = uint.MaxValue;
			}
			break;
		case PMXFormat.Header.IndexSize.Byte2:
			result = (uint)binary_reader_.ReadUInt16();
			if (ushort.MaxValue == result) {
				result = uint.MaxValue;
			}
			break;
		case PMXFormat.Header.IndexSize.Byte4:
			result = binary_reader_.ReadUInt32();
			break;
		default:
			throw new System.ArgumentOutOfRangeException();
		}
		return result;
	}

	private static Vector4 ReadSinglesToVector4(BinaryReader binary_reader_)
	{
		const int count = 4;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = binary_reader_.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Vector4(result[0], result[1], result[2], result[3]);
	}
		
	private static Vector3 ReadSinglesToVector3(BinaryReader binary_reader_)
	{
		const int count = 3;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = binary_reader_.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Vector3(result[0], result[1], result[2]);
	}
		
	private static Vector2 ReadSinglesToVector2(BinaryReader binary_reader_)
	{
		const int count = 2;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = binary_reader_.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Vector2(result[0], result[1]);
	}
	
	private static Color ReadSinglesToColor(BinaryReader binary_reader_)
	{
		const int count = 4;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = binary_reader_.ReadSingle();
		}
		return new Color(result[0], result[1], result[2], result[3]);
	}
		
	private static Color ReadSinglesToColor(BinaryReader binary_reader_, float fix_alpha)
	{
		const int count = 3;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = binary_reader_.ReadSingle();
		}
		return new Color(result[0], result[1], result[2], fix_alpha);
	}

	private Quaternion ReadSinglesToQuaternion(BinaryReader binary_reader_)
	{
		const int count = 4;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = binary_reader_.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Quaternion(result[0], result[1], result[2], result[3]);
	}
	
	string						file_path_;
	BinaryReader				binary_reader_;
	PMXFormat					format_;
	PMXFormat.Header.StringCode	string_code_;
}
