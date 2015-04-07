using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace MMD.Body.Window
{
    public class PMDImporterWindow : EditorWindow
    {
        public UnityEngine.Object pmdFile;
        public Shader shader;
        public float scale;

        [MenuItem("MMD for Unity/PMD Importer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<PMDImporterWindow>("PMD Importer");
        }

        public PMDImporterWindow()
        {

        }

        void OnGUI()
        {
            pmdFile = EditorGUILayout.ObjectField("PMD File", pmdFile, typeof(UnityEngine.Object), true) as UnityEngine.Object;
            var path = AssetDatabase.GetAssetPath(pmdFile);
            
            if (pmdFile != null)
            {
                if (!path.ToLower().Contains(".pmd"))
                    pmdFile = null;
            }

            /// ここにシェーダ書く
            ///
            /// ここまで

            /// ここにスケールを書く
            /// 
            /// ここまで

            if (GUILayout.Button("Convert") && pmdFile != null)
            {
                var converter = new MMD.Body.Converter.PMDConverter(path, scale);
                converter.Import(shader);
                pmdFile = null;
            }
        }
    }
}
