using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using MMD.VMD;
using MMD;

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

    public static VMDFormat Import(byte[] byte_data) {
        VMDLoaderScript loader = new VMDLoaderScript();
        return loader.Import_(byte_data);
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
			result = new VMDFormat.Header(binary_reader_);
		}
		return result;
	}

    private VMDFormat Import_(byte[] byte_data)
    {
        using (MemoryStream stream = new MemoryStream(byte_data))
        {
            SetupBinaryReader(stream);
        }
        return format_;
    }

	private VMDFormat Import_(string file_path) {
        using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            file_path_ = file_path;
            EntryPathes();
            SetupBinaryReader(stream);
        }
		return format_;
	}

    private void SetupBinaryReader(Stream stream)
    {
        using (BinaryReader bin = new BinaryReader(stream))
        {
            binary_reader_ = bin;
            Read();
        }
    }

	private VMDFormat Read() {
		format_ = new VMDFormat();
		
		// 読み込み失敗した場合はだいたいデータがない
		// 失敗しても読み込み続けることがあるので例外でキャッチして残りはnullにしておく
		int read_count = 0;
		try {
			format_.header = new VMDFormat.Header(binary_reader_); read_count++;
			format_.motion_list = new VMDFormat.MotionList(binary_reader_); read_count++;
			format_.skin_list = new VMDFormat.SkinList(binary_reader_); read_count++;
			format_.camera_list = new VMDFormat.CameraList(binary_reader_); read_count++;
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
	
	private VMDFormat.LightList ReadLightList() {
		VMDFormat.LightList result = new VMDFormat.LightList();
		result.light_count = binary_reader_.ReadUInt32();
		result.light = new VMDFormat.LightData[result.light_count];
		for (int i = 0; i < result.light_count; i++) {
			result.light[i] = ReadLightData();
		}
		
		Array.Sort(result.light, (x,y)=>((int)x.frame_no-(int)y.frame_no));
		return result;
	}
	
	private VMDFormat.LightData ReadLightData() {
		VMDFormat.LightData result = new VMDFormat.LightData();
		result.frame_no = binary_reader_.ReadUInt32();
        result.rgb = ToFormatUtil.ReadSinglesToColor(binary_reader_, 1);
        result.location = ToFormatUtil.ReadSinglesToVector3(binary_reader_);
		return result;
	}
	
	private VMDFormat.SelfShadowList ReadSelfShadowList() {
		VMDFormat.SelfShadowList result = new VMDFormat.SelfShadowList();
		result.self_shadow_count = binary_reader_.ReadUInt32();
		result.self_shadow = new VMDFormat.SelfShadowData[result.self_shadow_count];
		for (int i = 0; i < result.self_shadow_count; i++) {
			result.self_shadow[i] = ReadSelfShadowData();
		}
		
		Array.Sort(result.self_shadow, (x,y)=>((int)x.frame_no-(int)y.frame_no));
		return result;
	}
	
	private VMDFormat.SelfShadowData ReadSelfShadowData() {
		VMDFormat.SelfShadowData result = new VMDFormat.SelfShadowData();
		result.frame_no = binary_reader_.ReadUInt32();
		result.mode = binary_reader_.ReadByte();
		result.distance = binary_reader_.ReadSingle();
		return result;
	}
		
	string			file_path_;
	BinaryReader	binary_reader_;
	VMDFormat		format_;
}
