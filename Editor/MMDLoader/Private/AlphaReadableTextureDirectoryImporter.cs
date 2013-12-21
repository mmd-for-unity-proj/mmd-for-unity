using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
	
public class AlphaReadableTextureDirectoryImporter : AssetPostprocessor {

	/// <summary>
	/// テクスチャプリプロセッサ
	/// </summary>
	void OnPreprocessTexture() {
		if (-1 != assetPath.IndexOf(AlphaReadableTexture.directory_name)) {
			//MmdForUnityの解析用ディレクトリなら
			TextureImporter importer = (TextureImporter)assetImporter;
			importer.isReadable = true; //読み込み可能とする
			importer.textureFormat = TextureImporterFormat.Alpha8; //アルファのみ
			importer.mipmapEnabled = false; //mipmapを作成しない
			if (importer.DoesSourceTextureHaveAlpha()) {
				//アルファが有れば
				//透過フラグを立てる
				importer.alphaIsTransparency = true;
			} else {
				//アルファが無ければ
				//解像度を最小化
				importer.maxTextureSize = 1;
			}
		}
	}
}
