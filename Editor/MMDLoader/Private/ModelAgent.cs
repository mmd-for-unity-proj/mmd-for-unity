using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MMD.PMD;

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
				header_ = PMXLoaderScript.GetPmdHeader(file_path_);
			} catch (System.FormatException) {
				//PMXとして読み込めなかったら
				//PMDとして読み込む
				header_ = PMDLoaderScript.GetHeader(file_path_);
			}
			format_ = null;
		}
		
		/// <summary>
		/// プレファブを作成する
		/// </summary>
		/// <param name='shader_type'>シェーダーの種類</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='use_mecanim'>Mecanimを使用するか</param>
		/// <param name='use_ik'>IKを使用するか</param>
		/// <param name='scale'>スケール</param>
		/// <param name='is_pmx_base_import'>PMX Baseでインポートするか</param>
		public void CreatePrefab(PMD.PMDConverter.ShaderType shader_type, bool use_rigidbody, bool use_mecanim, bool use_ik, float scale, bool is_pmx_base_import) {
			GameObject game_object;
			Object prefab;
			if (is_pmx_base_import) {
				//PMX Baseでインポートする
				//PMXファイルのインポート
				PMX.PMXFormat format = PMXLoaderScript.Import(file_path_);
				//ゲームオブジェクトの作成
				game_object = PMXConverter.CreateGameObject(format, use_rigidbody, use_mecanim, use_ik, scale);
	
				// プレファブに登録
				prefab = PrefabUtility.CreateEmptyPrefab(format.meta_header.folder + "/" + format.meta_header.name + ".prefab");
			} else {
				//V2エクスポーターを使用しない
				//PMDファイルのインポート
				if (null == format_) {
					//まだ読み込んでいないなら読むこむ
					try {
						//PMX読み込みを試みる
						format_ = PMXLoaderScript.PmdImport(file_path_);
					} catch (System.FormatException) {
						//PMXとして読み込めなかったら
						//PMDとして読み込む
						format_ = PMDLoaderScript.Import(file_path_);
					}
					header_ = format_.head;
				}
	
				//ゲームオブジェクトの作成
				game_object = PMDConverter.CreateGameObject(format_, shader_type, use_rigidbody, use_mecanim, use_ik, scale);
	
				// プレファブに登録
				prefab = PrefabUtility.CreateEmptyPrefab(format_.folder + "/" + format_.name + ".prefab");
			}
			PrefabUtility.ReplacePrefab(game_object, prefab);
			
			// アセットリストの更新
			AssetDatabase.Refresh();
	
			// 一度，表示されているモデルを削除して新しくPrefabのインスタンスを作る
			GameObject.DestroyImmediate(game_object);
			PrefabUtility.InstantiatePrefab(prefab);
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
			if (null != format_) {
				result = format_.eg_head.model_name_eg;
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
			if (null != format_) {
				result = format_.eg_head.comment_eg;
			}
			return result;
		}}
		
		string 				file_path_;
		PMDFormat.Header	header_;
		PMDFormat			format_;
	}
}