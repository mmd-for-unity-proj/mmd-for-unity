// Inspectorからインポートなどができるようになります
// 他スクリプトと競合してしまう時はコメントアウトしてください

#define USE_INSPECTOR

//----------

#if USE_INSPECTOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MMD
{
	[InitializeOnLoad]
	public class InspectorBase : Editor
	{
		static InspectorBase()
		{
			EntryEditorApplicationUpdate();
		}

		[DidReloadScripts]
		static void OnDidReloadScripts()
		{
			EntryEditorApplicationUpdate();
		}

		static void EntryEditorApplicationUpdate()
		{
			EditorApplication.update += Update;
		}

		static void Update()
		{
			if (Selection.objects.Length != 0)
			{
				string path = AssetDatabase.GetAssetPath(Selection.activeObject);
				string extension = Path.GetExtension(path).ToLower();

				if (extension == ".pmd" || extension == ".pmx")
				{
					SetupScriptableObject<PMDScriptableObject>(path);
				}
				else if (extension == ".vmd")
				{
					SetupScriptableObject<VMDScriptableObject>(path);
				}
			}
		}

		static void SetupScriptableObject<T>(string path) where T : ScriptableObjectBase
		{
			int count = Selection.objects.OfType<T>().Count();
			if (count != 0) return;
			T scriptableObject = ScriptableObject.CreateInstance<T>();
			scriptableObject.assetPath = path;
			Selection.activeObject = scriptableObject;
			EditorUtility.UnloadUnusedAssets();
		}
	}
}

#endif
