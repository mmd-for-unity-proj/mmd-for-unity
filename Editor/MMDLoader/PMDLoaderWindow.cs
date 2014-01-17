using UnityEngine;
using System.Collections;
using UnityEditor;

public class PMDLoaderWindow : EditorWindow {
	Object pmdFile;
	MMD.PMDImportConfig pmd_config;

	[MenuItem("MMD for Unity/PMD Loader")]
	static void Init() {
		var window = (PMDLoaderWindow)EditorWindow.GetWindow<PMDLoaderWindow>(true, "PMDLoader");
		window.Show();
	}

	void OnEnable()
	{
		// デフォルトコンフィグ
		pmdFile = null;
		pmd_config = MMD.Config.LoadAndCreate().pmd_config.Clone();
	}
	
	void OnGUI() {
		// GUIの有効化
		GUI.enabled = !EditorApplication.isPlaying;
		
		// GUI描画
		pmdFile = EditorGUILayout.ObjectField("PMD File" , pmdFile, typeof(Object), false);
		pmd_config.OnGUIFunction();
		
		{
			bool gui_enabled_old = GUI.enabled;
			GUI.enabled = !EditorApplication.isPlaying && (pmdFile != null);
			if (GUILayout.Button("Convert")) {
				LoadModel();
				pmdFile = null;		// 読み終わったので空にする 
			}
			GUI.enabled = gui_enabled_old;
		}
	}

	void LoadModel() {
		string file_path = AssetDatabase.GetAssetPath(pmdFile);
		MMD.ModelAgent model_agent = new MMD.ModelAgent(file_path);
		model_agent.CreatePrefab(pmd_config.shader_type
								, pmd_config.rigidFlag
								, pmd_config.animation_type
								, pmd_config.use_ik
								, pmd_config.scale
								, pmd_config.is_pmx_base_import
								);
		
		// 読み込み完了メッセージ
		var window = LoadedWindow.Init();
		window.Text = string.Format(
			"----- model name -----\n{0}\n\n----- comment -----\n{1}",
			model_agent.name,
			model_agent.comment
		);
		window.Show();
	}
}
