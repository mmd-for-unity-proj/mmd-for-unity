using UnityEngine;
using System.Collections;
using UnityEditor;
using MMD.PMD;

public class PMDLoaderWindow : EditorWindow {
	Object pmdFile = null;
	bool rigidFlag = true;
	bool use_mecanim = true;
	PMDConverter.ShaderType shader_type = PMDConverter.ShaderType.MMDShader;

	bool use_ik = true;
	float scale = 0.085f;
	bool is_pmx_base_import = false;

	[MenuItem("Plugins/MMD Loader/PMD Loader")]
	static void Init() {        
        var window = (PMDLoaderWindow)EditorWindow.GetWindow<PMDLoaderWindow>(true, "PMDLoader");
		window.Show();
	}

    public PMDLoaderWindow()
    {
        // デフォルトコンフィグ
		var config = MMD.Config.LoadAndCreate();
		shader_type = config.pmd_config.shader_type;
		Debug.Log("test");
		rigidFlag = config.pmd_config.rigidFlag;
		use_mecanim = config.pmd_config.use_mecanim;
		use_ik = config.pmd_config.use_ik;
		is_pmx_base_import = config.pmd_config.is_pmx_base_import;
    }
	
	void OnGUI() {
		pmdFile = EditorGUILayout.ObjectField("PMD File" , pmdFile, typeof(Object));
		
		// シェーダの種類
		shader_type = (PMDConverter.ShaderType)EditorGUILayout.EnumPopup("Shader Type", shader_type);

		// 剛体を入れるかどうか
		rigidFlag = EditorGUILayout.Toggle("Rigidbody", rigidFlag);

		// Mecanimを使うかどうか
		bool old_gui_enabled = GUI.enabled;
		GUI.enabled = false;
		use_mecanim = EditorGUILayout.Toggle("Use Mecanim", false);
		GUI.enabled = old_gui_enabled;

		// IKを使うかどうか
		use_ik = EditorGUILayout.Toggle("Use IK", use_ik);

		// スケール
		scale = EditorGUILayout.Slider("Scale", scale, 0.001f, 1.0f);
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel(" ");
			if (GUILayout.Button("0.085", EditorStyles.miniButtonLeft)) {
				scale = 0.085f;
			}
			if (GUILayout.Button("1.0", EditorStyles.miniButtonRight)) {
				scale = 1.0f;
			}
		}
		EditorGUILayout.EndHorizontal();

		// PMX Baseでインポートするかどうか
		is_pmx_base_import = EditorGUILayout.Toggle("Use PMX Base Import", is_pmx_base_import);
		
		if (pmdFile != null) {
			if (GUILayout.Button("Convert")) {
				LoadModel();
				pmdFile = null;		// 読み終わったので空にする 
			}
		} else {
			EditorGUILayout.LabelField("Missing", "Select PMD File");
		}
	}

	void LoadModel() {
		string file_path = AssetDatabase.GetAssetPath(pmdFile);
		MMD.ModelAgent model_agent = new MMD.ModelAgent(file_path);
		model_agent.CreatePrefab(shader_type, rigidFlag, use_mecanim, use_ik, scale, is_pmx_base_import);
		
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
