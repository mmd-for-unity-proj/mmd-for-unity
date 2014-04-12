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

    string file_path_;
    VMDFormat format_;

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
		return loader.ImportFromFile(file_path);
	}

    public static VMDFormat Import(byte[] byte_data) {
        VMDLoaderScript loader = new VMDLoaderScript();
        return loader.ImportFromBytes(byte_data);
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
			result = new VMDFormat.Header(bin);
		}
		return result;
	}

    private VMDFormat ImportFromBytes(byte[] byte_data)
    {
        using (MemoryStream stream = new MemoryStream(byte_data))
        {
            SetupBinaryReader(stream);
        }
        return format_;
    }

	private VMDFormat ImportFromFile(string file_path) {
        using (FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            file_path_ = file_path;
            SetupBinaryReader(stream);
            EntryPathes();
        }
		return format_;
	}

    private void SetupBinaryReader(Stream stream)
    {
        using (BinaryReader bin = new BinaryReader(stream))
        {
            format_ = new VMDFormat(bin);
        }
    }

	private void EntryPathes() {
		format_.path = file_path_;
		format_.name = Path.GetFileNameWithoutExtension(file_path_); // .vmdを抜かす
		format_.folder = Path.GetDirectoryName(file_path_); // VMDが格納されているフォルダ
	}
}
