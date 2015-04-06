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
        public GameObject pmdFile;

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
            GUILayout.BeginHorizontal();
            GUILayout.Label("PMD File");
            pmdFile = EditorGUILayout.ObjectField(pmdFile, typeof(GameObject), true) as GameObject;
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Convert"))
            {

            }
        }
    }
}
