using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MMD {
	
	public class ModelAgent {
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name='file'>読み込むファイルパス</param>
		public ModelAgent(string file_path) {
			if (string.IsNullOrEmpty(file_path)) {
				throw new System.ArgumentException();
			}
			file_path_ = file_path;
			header_ = null;
			try {
				//PMX読み込みを試みる
				header_ = PMXLoaderScript.GetHeader(file_path_);
			} catch (System.FormatException) {
				//PMXとして読み込めなかったら
				//PMDとして読み込む
				PMD.PMDFormat.Header pmd_header = PMDLoaderScript.GetHeader(file_path_);
				header_ = PMXLoaderScript.PMD2PMX(pmd_header);
			}
		}
		
		/// <summary>
		/// プレファブを作成する
		/// </summary>
		/// <param name='shader_type'>シェーダーの種類</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='animation_type'>アニメーションタイプ</param>
		/// <param name='use_ik'>IKを使用するか</param>
		/// <param name='scale'>スケール</param>
		/// <param name='is_pmx_base_import'>PMX Baseでインポートするか</param>
		public void CreatePrefab(PMDConverter.ShaderType shader_type, bool use_rigidbody, PMXConverter.AnimationType animation_type, bool use_ik, float scale, bool is_pmx_base_import) {
			GameObject game_object;
			string prefab_path;
			if (is_pmx_base_import) {
				//PMX Baseでインポートする
				//PMXファイルのインポート
				PMX.PMXFormat pmx_format = null;
				try {
					//PMX読み込みを試みる
					pmx_format = PMXLoaderScript.Import(file_path_);
				} catch (System.FormatException) {
					//PMXとして読み込めなかったら
					//PMDとして読み込む
					PMD.PMDFormat pmd_format = PMDLoaderScript.Import(file_path_);
					pmx_format = PMXLoaderScript.PMD2PMX(pmd_format);
				}
				header_ = pmx_format.header;
				//ゲームオブジェクトの作成
				game_object = PMXConverter.CreateGameObject(pmx_format, use_rigidbody, animation_type, use_ik, scale);
	
				// プレファブパスの設定
				prefab_path = pmx_format.meta_header.folder + "/" + pmx_format.meta_header.name + ".prefab";
			} else {
				//PMXエクスポーターを使用しない
				//PMDファイルのインポート
				PMD.PMDFormat pmd_format = null;
				try {
					//PMX読み込みを試みる
					PMX.PMXFormat pmx_format = PMXLoaderScript.Import(file_path_);
					pmd_format = PMXLoaderScript.PMX2PMD(pmx_format);
				} catch (System.FormatException) {
					//PMXとして読み込めなかったら
					//PMDとして読み込む
					pmd_format = PMDLoaderScript.Import(file_path_);
				}
				header_ = PMXLoaderScript.PMD2PMX(pmd_format.head);
	
				//ゲームオブジェクトの作成
				bool use_mecanim = PMXConverter.AnimationType.LegacyAnimation == animation_type;
				game_object = PMDConverter.CreateGameObject(pmd_format, shader_type, use_rigidbody, use_mecanim, use_ik, scale);
	
				// プレファブパスの設定
				prefab_path = pmd_format.folder + "/" + pmd_format.name + ".prefab";
			}
			// プレファブ化
			PrefabUtility.CreatePrefab(prefab_path, game_object, ReplacePrefabOptions.ConnectToPrefab);
			
			// アセットリストの更新
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// モデル名取得
		/// </summary>
		/// <value>モデル名</value>
		public string name {get{
			string result = null;
			if (null != header_) {
				result = header_.model_name;
			}
			return result;
		}}
	
		/// <summary>
		/// 英語表記モデル名取得
		/// </summary>
		/// <value>英語表記モデル名</value>
		public string english_name {get{
			string result = null;
			if (null != header_) {
				result = header_.model_english_name;
			}
			return result;
		}}
	
		/// <summary>
		/// モデル製作者からのコメント取得
		/// </summary>
		/// <value>モデル製作者からのコメント</value>
		public string comment {get{
			string result = null;
			if (null != header_) {
				result = header_.comment;
			}
			return result;
		}}
	
		/// <summary>
		/// モデル製作者からの英語コメント取得
		/// </summary>
		/// <value>モデル製作者からの英語コメント</value>
		public string english_comment {get{
			string result = null;
			if (null != header_) {
					result = header_.english_comment;
			}
			return result;
		}}
		
		string 					file_path_;
		PMX.PMXFormat.Header	header_;
	}
}