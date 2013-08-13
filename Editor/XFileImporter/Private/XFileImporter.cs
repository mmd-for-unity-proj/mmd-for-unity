using UnityEngine;
using System.Collections;

/*
 * 1. Google Sketchupのプラグインをインストール 
 * http://www.3drad.com/Google-SketchUp-To-DirectX-XNA-Exporter-Plug-in.htm
 * （プラグインはSketchupをインストールしたフォルダのPluginsフォルダにスクリプトを入れればOK） 
 * 
 * 2. Xファイルの出力 
 * 適当にフォルダを指定して出力 
 * 
 * 3. Projectへの読み込み 
 * 出力したフォルダごとProjectに投げればOK 
 * 
 * 4. スクリプトの適用 
 * 適当なGameObjectにこのスクリプトを適用させる 
 * xFile変数にロードしたXファイルをD&Dして実行すれば出来上がり 
 * 
 * 諸注意 
 * Xファイルは方言が多いので、現在は1.のプラグインで出力したファイルのみ対応 
 */

public class XFileImporter {
	
	// Use this for initialization
	public static void Import(Object xFile) {
		xfile.XFileConverter cnv = new xfile.XFileConverter(xFile);
		
		Object prefab = cnv.CreatePrefab();
		Material[] material = cnv.CreateMaterials();
		Mesh mesh = cnv.CreateMesh();
		cnv.ReplacePrefab(prefab, mesh, material);
	}
}
