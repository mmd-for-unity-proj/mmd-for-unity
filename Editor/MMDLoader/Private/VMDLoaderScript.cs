using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using MMD.VMD;

public class VMDLoaderScript {

	/// <summary>
	/// VMDファイルのヘッダー取得
	/// </summary>
	/// <param name='file_path'>VMDファイルのパス</param>
	/// <returns>ヘッダー</returns>
	public static VMDFormat.Header GetHeader(string file_path) {
		VMDLoaderScript loader = new VMDLoaderScript();
		return loader.GetHeader_(file_path);
	}

	/// <summary>
	/// VMDファイルのインポート
	/// </summary>
	/// <param name='file_path'>VMDファイルのパス</param>
	/// <returns>内部形式データ</returns>
	public static VMDFormat Import(string file_path) {
		VMDLoaderScript loader = new VMDLoaderScript();
		return loader.Import_(file_path);
	}

	/// <summary>
	/// デフォルトコンストラクタ
	/// </summary>
	/// <remarks>
	/// ユーザーに依るインスタンス作成を禁止する
	/// </remarks>
	private VMDLoaderScript() {}

	private VMDFormat.Header GetHeader_(string file_path) {
		VMDFormat.Header result;
		using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
		using (BinaryReader bin = new BinaryReader(stream)) {
			file_path_ = null;
			binary_reader_ = bin;
			result = ReadHeader();
		}
		return result;
	}

	private VMDFormat Import_(string file_path) {
		using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
		using (BinaryReader bin = new BinaryReader(stream)) {
			file_path_ = file_path;
			binary_reader_ = bin;
			Read();
		}
		return format_;
	}

	private VMDFormat Read() {
		format_ = new VMDFormat();
		EntryPathes();
		
		// 読み込み失敗した場合はだいたいデータがない
		// 失敗しても読み込み続けることがあるので例外でキャッチして残りはnullにしておく
		int read_count = 0;
		try {
			format_.header = ReadHeader(); read_count++;
			format_.motion_list = ReadMotionList(); read_count++;
			format_.skin_list = ReadSkinList(); read_count++;
			format_.camera_list = ReadCameraList(); read_count++;
			format_.light_list = ReadLightList(); read_count++;
			format_.self_shadow_list = ReadSelfShadowList(); read_count++;
		} catch (EndOfStreamException e) {
			Debug.Log(e.Message);
			if (read_count <= 0)
				format_.header = null;
			if (read_count <= 1 || format_.motion_list.motion_count <= 0)
				format_.motion_list = null;
			if (read_count <= 2 || format_.skin_list.skin_count <= 0)
				format_.skin_list = null;
			if (read_count <= 3 || format_.camera_list.camera_count <= 0)
				format_.camera_list = null;
			if (read_count <= 4 || format_.light_list.light_count <= 0)
				format_.light_list = null;
			if (read_count <= 5 || format_.self_shadow_list.self_shadow_count <= 0) 
				format_.self_shadow_list = null;
		}
		return format_;
	}

	private void EntryPathes() {
		format_.path = file_path_;
		format_.name = Path.GetFileNameWithoutExtension(file_path_); // .vmdを抜かす
		format_.folder = Path.GetDirectoryName(file_path_); // VMDが格納されているフォルダ
	}

	public VMDFormat.Header ReadHeader() {
		VMDFormat.Header result = new VMDFormat.Header();
		result.vmd_header = ConvertByteToString(binary_reader_.ReadBytes(30), "");
		result.vmd_model_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
		return result;
	}
	
	private VMDFormat.MotionList ReadMotionList() {
		VMDFormat.MotionList result = new VMDFormat.MotionList();
		result.motion_count = binary_reader_.ReadUInt32();
		result.motion = new Dictionary<string, List<VMDFormat.Motion>>();
		
		// 一度バッファに貯めてソートする
		VMDFormat.Motion[] buf = new VMDFormat.Motion[result.motion_count];
		for (int i = 0; i < result.motion_count; i++) {
			buf[i] = ReadMotion();
		}
		Array.Sort(buf, (x,y)=>((int)x.flame_no-(int)y.flame_no));
		
		// モーションの数だけnewされないよね？
		for (int i = 0; i < result.motion_count; i++) {
			try { result.motion.Add(buf[i].bone_name, new List<VMDFormat.Motion>()); }
			catch {}
		}
		
		// dictionaryにどんどん登録
		for (int i = 0; i < result.motion_count; i++) {
			result.motion[buf[i].bone_name].Add(buf[i]);
		}
		
		return result;
	}
	
	private VMDFormat.Motion ReadMotion() {
		VMDFormat.Motion result = new VMDFormat.Motion();
		result.bone_name = ConvertByteToString(binary_reader_.ReadBytes(15), "");
		result.flame_no = binary_reader_.ReadUInt32();
		result.location = ReadSinglesToVector3(binary_reader_);
		result.rotation = ReadSinglesToQuaternion(binary_reader_);
		result.interpolation = binary_reader_.ReadBytes(64);
		return result;
	}
	
	/// <summary>
	/// 表情リスト
	/// </summary>
	private VMDFormat.SkinList ReadSkinList() {
		VMDFormat.SkinList result = new VMDFormat.SkinList();
		result.skin_count = binary_reader_.ReadUInt32();
		result.skin = new Dictionary<string, List<VMDFormat.SkinData>>();
		
		// 一度バッファに貯めてソートする
		VMDFormat.SkinData[] buf = new VMDFormat.SkinData[result.skin_count];
		for (int i = 0; i < result.skin_count; i++) {
			buf[i] = ReadSkinData();
		}
		Array.Sort(buf, (x,y)=>((int)x.flame_no-(int)y.flame_no));
		
		// 全てのモーションを探索し、利用されているボーンを特定する
		for (int i = 0; i < result.skin_count; i++) {
			try { result.skin.Add(buf[i].skin_name, new List<VMDFormat.SkinData>()); }
			catch {
				//重複している場合はこの処理に入る
			}
		}
		
		// 辞書に登録する作業
		for (int i = 0; i < result.skin_count; i++) {
			result.skin[buf[i].skin_name].Add(buf[i]);
		}

		return result;
	}
	
	private VMDFormat.SkinData ReadSkinData() {
		VMDFormat.SkinData result = new VMDFormat.SkinData();
		result.skin_name = ConvertByteToString(binary_reader_.ReadBytes(15), "");
		result.flame_no = binary_reader_.ReadUInt32();
		result.weight = binary_reader_.ReadSingle();
		return result;
	}
	
	private VMDFormat.CameraList ReadCameraList() {
		VMDFormat.CameraList result = new VMDFormat.CameraList();
		result.camera_count = binary_reader_.ReadUInt32();
		result.camera = new VMDFormat.CameraData[result.camera_count];
		for (int i = 0; i < result.camera_count; i++) {
			result.camera[i] = ReadCameraData();
		}
		Array.Sort(result.camera, (x,y)=>((int)x.flame_no-(int)y.flame_no));
		return result;
	}
	
	private VMDFormat.CameraData ReadCameraData() {
		VMDFormat.CameraData result = new VMDFormat.CameraData();
		result.flame_no = binary_reader_.ReadUInt32();
		result.length = binary_reader_.ReadSingle();
		result.location = ReadSinglesToVector3(binary_reader_);
		result.rotation = ReadSinglesToVector3(binary_reader_);
		result.interpolation = binary_reader_.ReadBytes(24);
		result.viewing_angle = binary_reader_.ReadUInt32();
		result.perspective = binary_reader_.ReadByte();
		return result;
	}
	
	private VMDFormat.LightList ReadLightList() {
		VMDFormat.LightList result = new VMDFormat.LightList();
		result.light_count = binary_reader_.ReadUInt32();
		result.light = new VMDFormat.LightData[result.light_count];
		for (int i = 0; i < result.light_count; i++) {
			result.light[i] = ReadLightData();
		}
		
		Array.Sort(result.light, (x,y)=>((int)x.flame_no-(int)y.flame_no));
		return result;
	}
	
	private VMDFormat.LightData ReadLightData() {
		VMDFormat.LightData result = new VMDFormat.LightData();
		result.flame_no = binary_reader_.ReadUInt32();
		result.rgb = ReadSinglesToColor(binary_reader_, 1);
		result.location = ReadSinglesToVector3(binary_reader_);
		return result;
	}
	
	private VMDFormat.SelfShadowList ReadSelfShadowList() {
		VMDFormat.SelfShadowList result = new VMDFormat.SelfShadowList();
		result.self_shadow_count = binary_reader_.ReadUInt32();
		result.self_shadow = new VMDFormat.SelfShadowData[result.self_shadow_count];
		for (int i = 0; i < result.self_shadow_count; i++) {
			result.self_shadow[i] = ReadSelfShadowData();
		}
		
		Array.Sort(result.self_shadow, (x,y)=>((int)x.flame_no-(int)y.flame_no));
		return result;
	}
	
	private VMDFormat.SelfShadowData ReadSelfShadowData() {
		VMDFormat.SelfShadowData result = new VMDFormat.SelfShadowData();
		result.flame_no = binary_reader_.ReadUInt32();
		result.mode = binary_reader_.ReadByte();
		result.distance = binary_reader_.ReadSingle();
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

	private static Quaternion ReadSinglesToQuaternion(BinaryReader bin)
	{
		const int count = 4;
		float[] result = new float[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = bin.ReadSingle();
			if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
		}
		return new Quaternion(result[0], result[1], result[2], result[3]);
	}
	
	string			file_path_;
	BinaryReader	binary_reader_;
	VMDFormat		format_;
}
