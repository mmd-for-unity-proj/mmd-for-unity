// Inspectorからインポートなどができるようになります
// 他スクリプトと競合してしまう時はコメントアウトしてください

#define USE_INSPECTOR

//----------

#if USE_INSPECTOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MMD
{
    [CustomEditor(typeof(Object))]
    public class InspectorBase : Editor
    {
        static Editor editor;

        public InspectorBase()
        {
            var ext = Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
            if (ext == ".pmd" || ext == ".pmx")
            {
                editor = Editor.CreateEditor(Selection.activeObject, typeof(PMDInspector));
            }
            else if (ext == ".vmd")
            {
                editor = Editor.CreateEditor(Selection.activeObject, typeof(VMDInspector));
            }
            else 
            {
                editor = null;
            }
        }

        // Inspector上のGUI描画処理
        public override void OnInspectorGUI()
        {
            if (editor != null)
                editor.OnInspectorGUI();
            else
                DrawDefaultInspector();
        }

        // Inspector上のPreviewエリア描画処理
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (editor != null)
                editor.OnPreviewGUI(r, background);
        }
    }
}

#endif
