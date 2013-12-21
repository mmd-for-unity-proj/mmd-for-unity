using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AlphaReadableTexture : System.IDisposable {

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="texture_path_list">テクスチャ相対パスリスト</param>
	/// <param name="current_directory">カレントディレクトリ("/"終わり、テクスチャの相対パス基点)</param>
	/// <param name="temporary_directory">解析作業用ディレクトリ("/"終わり、このディレクトリの下に解析作業用ディレクトリを作ります)</param>
	public AlphaReadableTexture(string[] texture_path_list, string current_directory, string temporary_directory)
	{
		texture_path_list_ = texture_path_list;
		current_directory_ = current_directory;
		temporary_directory_ = temporary_directory + directory_name + "/";

		//テクスチャ作成
		foreach (string texture_path in texture_path_list_.Where(x=>!string.IsNullOrEmpty(x)).Distinct()) {
			CreateReadableTexture(texture_path);
		}
		AssetDatabase.Refresh();
		//テクスチャ取得
		textures_ = texture_path_list_.Select(x=>GetReadableTexture(x)).ToArray();
	}
	
	/// <summary>
	/// 読み込み可能テクスチャの取得
	/// </summary>
	/// <value>読み込み可能テクスチャ</value>
	public Texture2D[] textures {get{return textures_;}}

	/// <summary>
	/// Disposeインターフェース
	/// </summary>
	public void Dispose()
	{
		//テクスチャ破棄
		foreach (string texture_path in texture_path_list_.Where(x=>!string.IsNullOrEmpty(x)).Distinct()) {
			DeleteReadableTexture(texture_path);
		}
		//ディレクトリの破棄
		string path = Application.dataPath + "/../" + temporary_directory_; //"Asset/"が被るので1階層上がる
		if (System.IO.Directory.Exists(path)) {
			System.IO.Directory.Delete(path, true);
		}
	}
	
	/// <summary>
	/// 解析対象ディレクトリ名の取得
	/// </summary>
	/// <value>The directory_name.</value>
	public static string directory_name {get{return "AlphaReadableTextureDirectory.MmdForUnity";}}
	
	/// <summary>
	/// 読み込み可能テクスチャの作成
	/// </summary>
	/// <param name="texture_path">テクスチャパス</param>
	private void CreateReadableTexture(string texture_path)
	{
		if (!string.IsNullOrEmpty(texture_path)) {
			string base_texture_path = current_directory_ + texture_path;
			string readable_texture_path = temporary_directory_ + texture_path;
			CreateDirectoryPath(System.IO.Path.GetDirectoryName(readable_texture_path));
			bool is_copy_success = AssetDatabase.CopyAsset(base_texture_path, readable_texture_path);
			if (!is_copy_success) {
				throw new System.InvalidOperationException();
			}
		}
	}
	
	/// <summary>
	/// 読み込み可能テクスチャの取得
	/// </summary>
	/// <returns>読み込み可能テクスチャ</returns>
	/// <param name="texture_path">テクスチャパス</param>
	private Texture2D GetReadableTexture(string texture_path)
	{
		Texture2D result = null;
		if (!string.IsNullOrEmpty(texture_path)) {
			string readable_texture_path = temporary_directory_ + texture_path;
			result = (Texture2D)AssetDatabase.LoadAssetAtPath(readable_texture_path, typeof(Texture2D));
		}
		return result;
	}
	
	/// <summary>
	/// 読み込み可能テクスチャの削除
	/// </summary>
	/// <param name="texture_path">テクスチャパス</param>
	private void DeleteReadableTexture(string texture_path)
	{
		if (!string.IsNullOrEmpty(texture_path)) {
			string readable_texture_path = temporary_directory_ + texture_path;
			AssetDatabase.DeleteAsset(readable_texture_path);
		}
	}
	
	/// <summary>
	/// ディレクトリの作成(親ディレクトリが無ければ再帰的に作成)
	/// </summary>
	/// <param name="path">ディレクトリパス</param>
	private static void CreateDirectoryPath(string path)
	{
		//親ディレクトリ作成
		string parent_path = System.IO.Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(parent_path) && !System.IO.Directory.Exists(parent_path)) {
			CreateDirectoryPath(parent_path);
		}
		//カレントディレクトリ作成
		if (!System.IO.Directory.Exists(path)) {
			string name = System.IO.Path.GetFileName(path);
			AssetDatabase.CreateFolder(parent_path, name);
		}
	}
	
	private Texture2D[]	textures_;				//読み込み可能テクスチャ
	private string[]	texture_path_list_;		//解析するテクスチャリスト
	private string		current_directory_;		//カレントディレクトリ
	private string		temporary_directory_;	//解析作業用ディレクトリ
}
