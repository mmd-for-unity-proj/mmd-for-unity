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
        [MenuItem("MMD for Unity/PMD Importer")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow<PMDImporterWindow>("PMD Importer");
        }

        void OnGUI()
        {
            
        }
    }
}
