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
        rigidFlag = config.pmd_config.rigidFlag;
        use_mecanim = config.pmd_config.use_mecanim;
        use_ik = config.pmd_config.use_ik; 
        is_pmx_base_import = config.pmd_config.is_pmx_base_import;
    }
	
	void OnGUI() {
		const int height = 20;
		int width = (int)position.width;// -16;
		
		pmdFile = EditorGUI.ObjectField(
			new Rect(0, 0, width, height), "PMD File" , pmdFile, typeof(Object), false);
		
		// シェーダの種類
		shader_type = (PMDConverter.ShaderType)EditorGUI.EnumPopup(new Rect(0, height, width, height), "Shader Type", shader_type);

		// 剛体を入れるかどうか
		rigidFlag = EditorGUI.Toggle(new Rect(0, height * 2, width / 2, height), "Rigidbody", rigidFlag);

		// Mecanimを使うかどうか
		use_mecanim = false; // EditorGUI.Toggle(new Rect(0, height * 3, width / 2, height), "Use Mecanim", use_mecanim);

		// IKを使うかどうか
		use_ik = EditorGUI.Toggle(new Rect(0, height * 4, width / 2, height), "Use IK", use_ik);

		// PMX Baseでインポートするかどうか
		is_pmx_base_import = EditorGUI.Toggle(new Rect(0, height * 5, width / 2, height), "Use PMX Base Import", is_pmx_base_import);
		
		int buttonHeight = height * 6;
		if (pmdFile != null) {
			if (GUI.Button(new Rect(0, buttonHeight, width / 2, height), "Convert")) {
				LoadModel();
				pmdFile = null;		// 読み終わったので空にする 
			}
		} else {
			EditorGUI.LabelField(new Rect(0, buttonHeight, width, height), "Missing", "Select PMD File");
		}
	}

	void LoadModel() {
		string file_path = AssetDatabase.GetAssetPath(pmdFile);
		MMD.ModelAgent model_agent = new MMD.ModelAgent(file_path);
		model_agent.CreatePrefab(shader_type, rigidFlag, use_mecanim, use_ik, is_pmx_base_import);
		
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
