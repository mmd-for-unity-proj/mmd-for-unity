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
    public class InspectorBase : Editor
    {
		[DidReloadScripts]
		static void OnDidReloadScripts()
		{
			EditorApplication.update += () =>
			{
				if (Selection.objects.Length != 0)
				{
					string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
					string extension = Path.GetExtension(assetPath).ToLower();

					if (extension == ".pmd" || extension == ".pmx")
					{
						PMDInspector.pmd_path = assetPath;
						SetupScriptableObject<PMDScriptableObject>(assetPath);
					}
					else if (extension == ".vmd")
					{
						VMDInspector.vmd_path = assetPath;
						SetupScriptableObject<PMDScriptableObject>(assetPath);
					}
				}
			};
		}

		static void SetupScriptableObject<T>(string assetPath) where T : ScriptableObjectBase
		{
			int count = Selection.objects.OfType<T>().Count();
			if (count != 0) return;
			T scriptableObject = ScriptableObject.CreateInstance<T>();
			scriptableObject.assetPath = assetPath;
			Selection.activeObject = scriptableObject;
			EditorUtility.UnloadUnusedAssets();
		}
    }
}

#endif
