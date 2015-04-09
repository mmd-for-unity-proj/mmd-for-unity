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
        public float scale = 1;

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
            // ファイル
            pmdFile = EditorGUILayout.ObjectField("PMD File", pmdFile, typeof(UnityEngine.Object), true) as UnityEngine.Object;
            var path = AssetDatabase.GetAssetPath(pmdFile);
            
            // ファイルのバリデーション
            if (pmdFile != null)
            {
                if (!path.ToLower().Contains(".pmd"))
                    pmdFile = null;
            }

            /// ここにシェーダ書く
            shader = Shader.Find("MMD/MMD Shader");
            /// ここまで

            scale = EditorGUILayout.FloatField(scale);

            var argument = new MMD.Body.Argument.PMDArgument(path, scale, shader);

            if (GUILayout.Button("Convert") && pmdFile != null)
            {
                Debug.Log("Convert Start");
                var converter = new MMD.Body.Converter.PMDConverter(argument);
                converter.Import();
                pmdFile = null;
                Debug.Log("Convert End");
            }
        }
    }
}
