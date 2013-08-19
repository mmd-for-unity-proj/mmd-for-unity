using UnityEngine;
using System.Collections;
using UnityEditor;

public class VMDLoaderWindow : EditorWindow {
	Object vmdFile;
	GameObject pmdPrefab;
	bool createAnimationFile;
	int interpolationQuality;

	[MenuItem ("Plugins/MMD Loader/VMD Loader")]
	static void Init() {
		var window = (VMDLoaderWindow)EditorWindow.GetWindow<VMDLoaderWindow>(true, "VMDLoader");
		window.Show();
	}
	
    public VMDLoaderWindow()
    {
        // デフォルトコンフィグ
        var config = MMD.Config.LoadAndCreate();
        createAnimationFile = config.vmd_config.createAnimationFile;
        interpolationQuality = config.vmd_config.interpolationQuality;
    }

    void OnGUI() {
		const int height = 20;
		int top = 0;
		
		pmdPrefab = EditorGUI.ObjectField(
			new Rect(0, top, position.width - 16, height), "PMD Prefab", pmdPrefab, typeof(GameObject), false) as GameObject;
		top += height + 2;
		
		vmdFile = EditorGUI.ObjectField(
			new Rect(0, top, position.width - 16, height), "VMD File", vmdFile, typeof(Object), false);
		top += height + 2;
		
 		createAnimationFile = EditorGUI.Toggle(
			new Rect(0, top, position.width - 16, height), "Create Asset", createAnimationFile);
		top += height + 2;

		interpolationQuality=EditorGUI.IntSlider (
			new Rect(0, top, position.width - 16, height), "Interpolation Quality", interpolationQuality, 1, 10);
		top += height + 2;		

		if (pmdPrefab != null && vmdFile != null) 
		{
			if (GUI.Button(new Rect(0, top, position.width / 2, 16), "Convert"))
			{
				LoadMotion();
				vmdFile = null;
			}
		} 
		else 
		{
			if (pmdPrefab == null)
				EditorGUI.LabelField(new Rect(0, top, position.width, height), "Missing", "Select PMD Prefab");
			else if (vmdFile == null)
				EditorGUI.LabelField(new Rect(0, top, position.width, height), "Missing", "Select VMD File");
			else
				EditorGUI.LabelField(new Rect(0, top, position.width, height), "Missing", "Select PMD and VMD");
		}
	}

	void LoadMotion() {
		string file_path = AssetDatabase.GetAssetPath(vmdFile);
		MMD.MotionAgent motion_agent = new MMD.MotionAgent(file_path);
		motion_agent.CreateAnimationClip(pmdPrefab, createAnimationFile,interpolationQuality);
	}
}
