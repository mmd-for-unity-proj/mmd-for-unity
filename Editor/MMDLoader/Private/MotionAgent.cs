using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace MMD {
	
	public class MotionAgent {
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name='file'>読み込むファイルパス</param>
		public MotionAgent(string file_path) {
			if (string.IsNullOrEmpty(file_path)) {
				throw new System.ArgumentException();
			}
			file_path_ = file_path;
			header_ = VMDLoaderScript.GetHeader(file_path_); //VMD読み込み
			format_ = null;
		}
		
		/// <summary>
		/// アニメーションクリップを作成する
		/// </summary>
		/// <param name='assign_pmd'>使用するPMDのGameObject</param>
		/// <param name='create_asset'>Prefab外に作成するか(true:Prefab外に作成, false:Prefab内蔵)</param>
		/// <param name='interpolationQuality'>補完曲線品質</param>
		public void CreateAnimationClip(GameObject assign_pmd, bool create_asset, int interpolationQuality) {
			//VMDファイルのインポート
			if (null == format_) {
				//まだ読み込んでいないなら読むこむ
				format_ = VMDLoaderScript.Import(file_path_); //VMD読み込み
				header_ = format_.header;
			}

			//アニメーションクリップの作成
			AnimationClip animation_clip = VMDConverter.CreateAnimationClip(format_, assign_pmd, interpolationQuality);

			// ここで登録
			//anim.AddClip(animation_clip, animation_clip.name);

			if (create_asset) {
				// フォルダを生成してアニメーションのファイルを書き出す
				string prefab_folder = AssetDatabase.GetAssetPath(assign_pmd);
				prefab_folder = Path.GetDirectoryName(prefab_folder);

				if (!Directory.Exists(prefab_folder + "/Animation"))
					AssetDatabase.CreateFolder(prefab_folder, "Animation");

				AssetDatabase.CreateAsset(animation_clip, prefab_folder + "/Animation/" + animation_clip.name + ".anim");
			}
			else
			{
				// こちらはPrefabの中に入れるタイプ
				AssetDatabase.AddObjectToAsset(animation_clip, AssetDatabase.GetAssetPath(assign_pmd));
			}
			
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animation_clip));
		}

		/// <summary>
		/// アニメーション名取得
		/// </summary>
		/// <value>アニメーション名</value>
		public string name {get{
			string result = null;
			if (null != format_) {
				result = format_.name;
			}
			return result;
		}}
		
		/// <summary>
		/// アニメーション名取得
		/// </summary>
		/// <value>アニメーション名</value>
		public string model_name {get{
			string result = null;
			if (null != header_) {
				result = header_.vmd_model_name;
			}
			return result;
		}}
		string					file_path_;
		VMD.VMDFormat.Header	header_;
		VMD.VMDFormat			format_;
	}
}