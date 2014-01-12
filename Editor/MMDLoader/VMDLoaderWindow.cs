using UnityEngine;
using System.Collections;
using UnityEditor;

public class VMDLoaderWindow : EditorWindow {
	Object vmdFile;
	GameObject pmdPrefab;
	MMD.VMDImportConfig vmd_config;

	[MenuItem ("MMD for Unity/VMD Loader")]
	static void Init() {
		var window = (VMDLoaderWindow)EditorWindow.GetWindow<VMDLoaderWindow>(true, "VMDLoader");
		window.Show();
	}
	
	void OnEnable()
	{
		// デフォルトコンフィグ
		vmdFile = null;
		pmdPrefab = null;
		vmd_config = MMD.Config.LoadAndCreate().vmd_config.Clone();
	}

    void OnGUI() {
		// GUIの有効化
		GUI.enabled = !EditorApplication.isPlaying;
		
		// GUI描画
		pmdPrefab = (GameObject)EditorGUILayout.ObjectField("PMD Prefab", pmdPrefab, typeof(GameObject), false);
		vmdFile = EditorGUILayout.ObjectField("VMD File", vmdFile, typeof(Object), false);
		vmd_config.OnGUIFunction();

		{
			bool gui_enabled_old = GUI.enabled;
			GUI.enabled = !EditorApplication.isPlaying && (pmdPrefab != null) && (vmdFile != null);
			if (GUILayout.Button("Convert")) {
				LoadMotion();
				vmdFile = null;
			}
			GUI.enabled = gui_enabled_old;
		}
	}

	void LoadMotion() {
		string file_path = AssetDatabase.GetAssetPath(vmdFile);
		MMD.MotionAgent motion_agent = new MMD.MotionAgent(file_path);
		motion_agent.CreateAnimationClip(pmdPrefab
										, vmd_config.createAnimationFile
										, vmd_config.interpolationQuality
										);
	}
}
