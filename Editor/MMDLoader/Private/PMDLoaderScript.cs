using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MMD.PMD;

public class PMDLoaderScript {

	/// <summary>
	/// PMDファイルのヘッダー取得
	/// </summary>
	/// <param name='file_path'>PMDファイルのパス</param>
	/// <returns>ヘッダー</returns>
	public static PMDFormat.Header GetHeader(string file_path) {
		PMDLoaderScript loader = new PMDLoaderScript();
		return loader.GetHeader_(file_path);
	}

	/// <summary>
	/// PMDファイルのインポート
	/// </summary>
	/// <param name='file_path'>PMDファイルのパス</param>
	/// <returns>内部形式データ</returns>
	public static PMDFormat Import(string file_path) {
		PMDLoaderScript loader = new PMDLoaderScript();
		return loader.Import_(file_path);
	}

	/// <summary>
	/// デフォルトコンストラクタ
	/// </summary>
	/// <remarks>
	/// ユーザーに依るインスタンス作成を禁止する
	/// </remarks>
	private PMDLoaderScript() {}

	private PMDFormat.Header GetHeader_(string file_path) {
		PMDFormat.Header result;
		using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
		using (BinaryReader bin = new BinaryReader(stream)) {
			file_path_ = null;
			binary_reader_ = bin;
			result = ReadHeader();
		}
		return result;
	}

	private PMDFormat Import_(string file_path) {
		using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
		using (BinaryReader bin = new BinaryReader(stream)) {
			file_path_ = file_path;
			binary_reader_ = bin;
			Read();
		}
		return format_;
	}

	private PMDFormat Read() {
		format_ = new PMDFormat();
		EntryPathes();
		
		try {
			format_.head = ReadHeader();
			format_.vertex_list = ReadVertexList();
			format_.face_vertex_list = ReadFaceVertexList();
			format_.material_list = ReadMaterialList();
			format_.bone_list = ReadBoneList(); 
			format_.ik_list = ReadIKList();
			format_.skin_list = ReadSkinList();
			format_.skin_name_list = ReadSkinNameList();
			format_.bone_name_list = ReadBoneNameList();
			format_.bone_display_list = ReadBoneDisplayList();
			format_.eg_head = ReadEnglishHeader();
			if (format_.eg_head.english_name_compatibility != 0) {
				format_.eg_bone_name_list = ReadEnglishBoneNameList(format_.bone_list.bone_count);
				format_.eg_skin_name_list = ReadEnglishSkinNameList(format_.skin_list.skin_count);
				format_.eg_bone_display_list = ReadEnglishBoneDisplayList(format_.bone_name_list.bone_disp_name_count);
			}
			format_.toon_texture_list = ReadToonTextureList();
			format_.rigidbody_list = ReadRigidbodyList();
			format_.rigidbody_joint_list = ReadRigidbodyJointList();
		} catch {
			Debug.Log("Don't read full format");
		}
		return format_;
	}

	private void EntryPathes() {
		format_.path = file_path_;
		format_.name = Path.GetFileNameWithoutExtension(file_path_); // .pmdを抜かす
		format_.folder = Path.GetDirectoryName(file_path_); // PMDが格納されているフォルダ
	}
	
	private PMDFormat.Header ReadHeader() {
		PMDFormat.Header result = new PMDFormat.Header();
		result.magic = binary_reader_.ReadBytes(3);
		result.version = binary_reader_.ReadSingle();
		result.model_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		result.comment = ConvertByteToString(binary_reader_.ReadBytes(256), System.Environment.NewLine);
		return result;
	}

	private PMDFormat.VertexList ReadVertexList() {
		PMDFormat.VertexList result = new PMDFormat.VertexList();
		result.vert_count = binary_reader_.ReadUInt32();
		result.vertex = new PMDFormat.Vertex[result.vert_count];
		for (int i = 0; i < result.vert_count; i++) {
			result.vertex[i] = ReadVertex();
		}
		return result;
	}

	private PMDFormat.Vertex ReadVertex() {
		PMDFormat.Vertex result = new PMDFormat.Vertex();
		result.pos = ReadSinglesToVector3(binary_reader_);
		result.normal_vec = ReadSinglesToVector3(binary_reader_);
		result.uv = ReadSinglesToVector2(binary_reader_);
		result.bone_num = ReadUInt16s(binary_reader_, 2);
		result.bone_weight = binary_reader_.ReadByte();
		result.edge_flag = binary_reader_.ReadByte();
		return result;
	}

	private PMDFormat.FaceVertexList ReadFaceVertexList() {
		PMDFormat.FaceVertexList result = new PMDFormat.FaceVertexList();
		result.face_vert_count = binary_reader_.ReadUInt32();
		result.face_vert_index = ReadUInt16s(binary_reader_, result.face_vert_count);
		return result;
	}

	private PMDFormat.MaterialList ReadMaterialList() {
		PMDFormat.MaterialList result = new PMDFormat.MaterialList();
		result.material_count = binary_reader_.ReadUInt32();
		result.material = new PMDFormat.Material[result.material_count];
		for (int i = 0; i < result.material_count; i++) {
			result.material[i] = ReadMaterial();
		}
		return result;
	}
	
	private PMDFormat.Material ReadMaterial() {
		PMDFormat.Material result = new PMDFormat.Material();
		result.diffuse_color = ReadSinglesToColor(binary_reader_, 1);
		result.alpha = binary_reader_.ReadSingle();
		result.specularity = binary_reader_.ReadSingle();
		result.specular_color = ReadSinglesToColor(binary_reader_, 1);
		result.mirror_color = ReadSinglesToColor(binary_reader_, 1);
		result.toon_index = binary_reader_.ReadByte();
		result.edge_flag = binary_reader_.ReadByte();
		result.face_vert_count = binary_reader_.ReadUInt32();
		
		// テクスチャ名の抜き出し
		// スフィアマップも行う
		string buf = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		
		//Debug by Wilfrem: テクスチャが無い場合を考慮していない
		//Debug by Wilfrem: テクスチャはfoo.bmp*bar.sphのパターンだけなのか？ bar.sph*foo.bmpのパターンがあり得るのでは？ 対策をしておくべき
		//Debug by GRGSIBERIA: スフィアマップとテクスチャが逆になる現象が発生したので修正
		//Debug by GRGSIBERIA: "./テクスチャ名"で始まるモデルで異常発生したので修正
		if (!string.IsNullOrEmpty(buf.Trim())) {
			string[] textures = buf.Trim().Split('*');
			foreach (var tex in textures) {
				string texNameEndAssignVar = "";
				string ext = Path.GetExtension(tex);
				if (ext == ".sph" || ext == ".spa") {
					result.sphere_map_name = tex;
				/*} else if (string.IsNullOrEmpty(tex)) {
					result.texture_file_name=""; */
				} else {
					if (tex.Split('/')[0] == ".") {
						// テクスチャ名の後端に"./"があった場合の回避処理 
						string[] texNameBuf = tex.Split('/');
						for (int i = 1; i < texNameBuf.Length-1; i++) {
							texNameEndAssignVar += texNameBuf[i] + "/";
						}
						texNameEndAssignVar += texNameBuf[texNameBuf.Length-1];
					} else {
						// 特に異常がない場合はそのまま代入 
						texNameEndAssignVar = tex;
					}
#if UNITY_STANDALONE_OSX
					// MACの場合，濁点のあるひらがなを使うと動かないらしいので対策
					// http://sourceforge.jp/ticket/browse.php?group_id=6158&tid=31929
					texNameEndAssignVar = texNameEndAssignVar.Normalize(NormalizationForm.FormKD);
#endif
					result.texture_file_name = texNameEndAssignVar;
				}
			}
		} else {
			result.sphere_map_name="";
			result.texture_file_name="";
		}
		if (string.IsNullOrEmpty(result.texture_file_name)) {
			result.texture_file_name = "";
		}
		return result;
	}

	private PMDFormat.BoneList ReadBoneList() {
		PMDFormat.BoneList result = new PMDFormat.BoneList();
		result.bone_count = binary_reader_.ReadUInt16();
		//Debug.Log("BoneCount:"+bone_count);
		result.bone = new PMDFormat.Bone[result.bone_count];
		for (int i = 0; i < result.bone_count; i++) {
			result.bone[i] = ReadBone();
		}
		return result;
	}

	private PMDFormat.Bone ReadBone() {
		PMDFormat.Bone result = new PMDFormat.Bone();
		result.bone_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		result.parent_bone_index = binary_reader_.ReadUInt16();
		result.tail_pos_bone_index = binary_reader_.ReadUInt16();
		result.bone_type = binary_reader_.ReadByte();
		result.ik_parent_bone_index = binary_reader_.ReadUInt16();
		result.bone_head_pos = ReadSinglesToVector3(binary_reader_);
		return result;
	}

	private PMDFormat.IKList ReadIKList() {
		PMDFormat.IKList result = new PMDFormat.IKList();
		result.ik_data_count = binary_reader_.ReadUInt16();
		//Debug.Log("IKDataCount:"+ik_data_count);
		result.ik_data = new PMDFormat.IK[result.ik_data_count];
		for (int i = 0; i < result.ik_data_count; i++) {
			result.ik_data[i] = ReadIK();
		}
		return result;
	}

	private PMDFormat.IK ReadIK() {
		PMDFormat.IK result = new PMDFormat.IK();
		result.ik_bone_index = binary_reader_.ReadUInt16();
		result.ik_target_bone_index = binary_reader_.ReadUInt16();
		result.ik_chain_length = binary_reader_.ReadByte();
		result.iterations = binary_reader_.ReadUInt16();
		result.control_weight = binary_reader_.ReadSingle();
		result.ik_child_bone_index = ReadUInt16s(binary_reader_, result.ik_chain_length);
		return result;
	}

	private PMDFormat.SkinList ReadSkinList() {
		PMDFormat.SkinList result = new PMDFormat.SkinList();
		result.skin_count = binary_reader_.ReadUInt16();
		//Debug.Log("SkinCount:"+skin_count);
		result.skin_data = new PMDFormat.SkinData[result.skin_count];
		for (int i = 0; i < result.skin_count; i++) {
			result.skin_data[i] = ReadSkinData();
		}
		return result;
	}

	private PMDFormat.SkinData ReadSkinData() {
		PMDFormat.SkinData result = new PMDFormat.SkinData();
		result.skin_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		result.skin_vert_count = binary_reader_.ReadUInt32();
		result.skin_type = binary_reader_.ReadByte();
		result.skin_vert_data = new PMDFormat.SkinVertexData[result.skin_vert_count];
		for (int i = 0; i < result.skin_vert_count; i++) {
			result.skin_vert_data[i] = ReadSkinVertexData();
		}
		return result;
	}

	private PMDFormat.SkinVertexData ReadSkinVertexData() {
		PMDFormat.SkinVertexData result = new PMDFormat.SkinVertexData();
		result.skin_vert_index = binary_reader_.ReadUInt32();
		result.skin_vert_pos = ReadSinglesToVector3(binary_reader_);
		return result;
	}

				
	private PMDFormat.SkinNameList ReadSkinNameList() {
		PMDFormat.SkinNameList result = new PMDFormat.SkinNameList();
		result.skin_disp_count = binary_reader_.ReadByte();
		result.skin_index = ReadUInt16s(binary_reader_, result.skin_disp_count);
		return result;
	}
	
	private PMDFormat.BoneNameList ReadBoneNameList() {
		PMDFormat.BoneNameList result = new PMDFormat.BoneNameList();
		result.bone_disp_name_count = binary_reader_.ReadByte();
		result.disp_name = new string[result.bone_disp_name_count];
		for (int i = 0; i < result.bone_disp_name_count; i++) {
			result.disp_name[i] = ConvertByteToString(binary_reader_.ReadBytes(50), "");
		}
		return result;
	}
	
	private PMDFormat.BoneDisplayList ReadBoneDisplayList() {
		PMDFormat.BoneDisplayList result = new PMDFormat.BoneDisplayList();
		result.bone_disp_count = binary_reader_.ReadUInt32();
		result.bone_disp = new PMDFormat.BoneDisplay[result.bone_disp_count];
		for (int i = 0; i < result.bone_disp_count; i++) {
			result.bone_disp[i] = ReadBoneDisplay();
		}
		return result;
	}
	
	private PMDFormat.BoneDisplay ReadBoneDisplay() {
		PMDFormat.BoneDisplay result = new PMDFormat.BoneDisplay();
		result.bone_index = binary_reader_.ReadUInt16();
		result.bone_disp_frame_index = binary_reader_.ReadByte();
		return result;
	}
	
	private PMDFormat.EnglishHeader ReadEnglishHeader() {
		PMDFormat.EnglishHeader result = new PMDFormat.EnglishHeader();
		result.english_name_compatibility = binary_reader_.ReadByte();
		
		if (result.english_name_compatibility != 0) {
			// 英語名対応あり
			result.model_name_eg = ConvertByteToString(binary_reader_.ReadBytes(20), "");
			result.comment_eg = ConvertByteToString(binary_reader_.ReadBytes(256), System.Environment.NewLine);
		}
		return result;
	}
	
	private PMDFormat.EnglishBoneNameList ReadEnglishBoneNameList(int boneCount) {
		PMDFormat.EnglishBoneNameList result = new PMDFormat.EnglishBoneNameList();
		result.bone_name_eg = new string[boneCount];
		for (int i = 0; i < boneCount; i++) {
			result.bone_name_eg[i] = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		}
		return result;
	}
	
	private PMDFormat.EnglishSkinNameList ReadEnglishSkinNameList(int skinCount) {
		PMDFormat.EnglishSkinNameList result = new PMDFormat.EnglishSkinNameList();
		result.skin_name_eg = new string[skinCount];
		for (int i = 0; i < skinCount - 1; i++) {
			result.skin_name_eg[i] = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		}
		return result;
	}
	
	private PMDFormat.EnglishBoneDisplayList ReadEnglishBoneDisplayList(int boneDispNameCount) {
		PMDFormat.EnglishBoneDisplayList result = new PMDFormat.EnglishBoneDisplayList();
		result.disp_name_eg = new string[boneDispNameCount];
		for (int i = 0; i < boneDispNameCount; i++) {
			result.disp_name_eg[i] = ConvertByteToString(binary_reader_.ReadBytes(50), "");
		}
		return result;
	}
	
	private PMDFormat.ToonTextureList ReadToonTextureList() {
		PMDFormat.ToonTextureList result = new PMDFormat.ToonTextureList();
		result.toon_texture_file = new string[10];
		for (int i = 0; i < result.toon_texture_file.Length; i++) {
			result.toon_texture_file[i] = ConvertByteToString(binary_reader_.ReadBytes(100), "");
		}
		return result;
	}
	
	private PMDFormat.RigidbodyList ReadRigidbodyList() {
		PMDFormat.RigidbodyList result = new PMDFormat.RigidbodyList();
		result.rigidbody_count = binary_reader_.ReadUInt32();
		result.rigidbody = new PMDFormat.Rigidbody[result.rigidbody_count];
		for (int i = 0; i < result.rigidbody_count; i++) {
			result.rigidbody[i] = ReadRigidbody();
		}
		return result;
	}
	
	private PMDFormat.Rigidbody ReadRigidbody() {
		PMDFormat.Rigidbody result = new PMDFormat.Rigidbody();
		result.rigidbody_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		result.rigidbody_rel_bone_index = binary_reader_.ReadUInt16();
		result.rigidbody_group_index = binary_reader_.ReadByte();
		result.rigidbody_group_target = binary_reader_.ReadUInt16();
		result.shape_type = binary_reader_.ReadByte();
		result.shape_w = binary_reader_.ReadSingle();
		result.shape_h = binary_reader_.ReadSingle();
		result.shape_d = binary_reader_.ReadSingle();
		result.pos_pos = ReadSinglesToVector3(binary_reader_);
		result.pos_rot = ReadSinglesToVector3(binary_reader_);
		result.rigidbody_weight = binary_reader_.ReadSingle();
		result.rigidbody_pos_dim = binary_reader_.ReadSingle();
		result.rigidbody_rot_dim = binary_reader_.ReadSingle();
		result.rigidbody_recoil = binary_reader_.ReadSingle();
		result.rigidbody_friction = binary_reader_.ReadSingle();
		result.rigidbody_type = binary_reader_.ReadByte();
		return result;
	}
	
	private PMDFormat.RigidbodyJointList ReadRigidbodyJointList() {
		PMDFormat.RigidbodyJointList result = new PMDFormat.RigidbodyJointList();
		result.joint_count = binary_reader_.ReadUInt32();
		result.joint = new PMDFormat.Joint[result.joint_count];
		for (int i = 0; i < result.joint_count; i++) {
			result.joint[i] = ReadJoint();
		}
		return result;
	}
	
	private PMDFormat.Joint ReadJoint() {
		PMDFormat.Joint result = new PMDFormat.Joint();
		result.joint_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		result.joint_rigidbody_a = binary_reader_.ReadUInt32(); 
		result.joint_rigidbody_b = binary_reader_.ReadUInt32();
		result.joint_pos = ReadSinglesToVector3(binary_reader_);
		result.joint_rot = ReadSinglesToVector3(binary_reader_);
		result.constrain_pos_1 = ReadSinglesToVector3(binary_reader_);
		result.constrain_pos_2 = ReadSinglesToVector3(binary_reader_);
		result.constrain_rot_1 = ReadSinglesToVector3(binary_reader_);
		result.constrain_rot_2 = ReadSinglesToVector3(binary_reader_);
		result.spring_pos = ReadSinglesToVector3(binary_reader_);
		result.spring_rot = ReadSinglesToVector3(binary_reader_);
		return result;
	}

	// ShiftJISからUTF-8に変換してstringで返す
	private static string ConvertByteToString(byte[] bytes, string line_feed_code = null)
	{
		// パディングの消去, 文字を詰める
		if (bytes[0] == 0) return "";
		int count;
		for (count = 0; count < bytes.Length; count++) if (bytes[count] == 0) break;
		byte[] buf = new byte[count];		// NULL文字を含めるとうまく行かない
		for (int i = 0; i < count; i++) {
			buf[i] = bytes[i];
		}

#if UNITY_STANDALONE_OSX
		buf = Encoding.Convert(Encoding.GetEncoding(932), Encoding.UTF8, buf);
#else
		buf = Encoding.Convert(Encoding.GetEncoding(0), Encoding.UTF8, buf);
#endif
		string result = Encoding.UTF8.GetString(buf);
		if (null != line_feed_code) {
			//改行コード統一(もしくは除去)
			result = result.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", line_feed_code);
		}
		return result;
	}

	private static Vector3 ReadSinglesToVector3(BinaryReader bin)
	{
		const int count = 3;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = bin.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Vector3(result[0], result[1], result[2]);
	}
		
	private static Vector2 ReadSinglesToVector2(BinaryReader bin)
	{
		const int count = 2;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = bin.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Vector2(result[0], result[1]);
	}
	
	private static Color ReadSinglesToColor(BinaryReader bin)
	{
		const int count = 4;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = bin.ReadSingle();
		}
		return new Color(result[0], result[1], result[2], result[3]);
	}
		
	private static Color ReadSinglesToColor(BinaryReader bin, float fix_alpha)
	{
		const int count = 3;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = bin.ReadSingle();
		}
		return new Color(result[0], result[1], result[2], fix_alpha);
	}

	private static ushort[] ReadUInt16s(BinaryReader bin, uint count)
	{
		ushort[] result = new ushort[count];
		for (uint i = 0; i < count; i++)
		{
			result[i] = bin.ReadUInt16();
		}
		return result;
	}
		
	string			file_path_;
	BinaryReader	binary_reader_;
	PMDFormat		format_;
}
