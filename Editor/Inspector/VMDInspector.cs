using UnityEngine;
using UnityEditor;
using System.Collections;
using MMD.PMD;
using System.IO;

#if !(UNITY_3_5 || UNITY_3_4 || UNITY_3_3)

namespace MMD
{
	[CustomEditor(typeof(VMDScriptableObject))]
    public class VMDInspector : Editor
    {
        // VMD Load option
        public bool createAnimationFile;
        public int interpolationQuality;
        public GameObject pmdPrefab;

        // last selected item
        private static MotionAgent motion_agent;
        private static string message = "";

        /// <summary>
        /// 選択されているオブジェクトがVMDファイルかチェックします
        /// </summary>
        /// <returns>VMDファイルであればそのパスを、異なればnullを返します。</returns>
        void setup()
        {
            // デフォルトコンフィグ
            var config = MMD.Config.LoadAndCreate();
            createAnimationFile = config.vmd_config.createAnimationFile;
            interpolationQuality = config.vmd_config.interpolationQuality;

            // モデル情報
            if (config.inspector_config.use_vmd_preload)
            {
				var obj = (VMDScriptableObject)target;
                motion_agent = new MotionAgent(obj.assetPath);
            }
            else
            {
                motion_agent = null;
            }
        }

        /// <summary>
        /// Inspector上のGUI描画処理を行います
        /// </summary>
        public override void OnInspectorGUI()
        {
            setup();

            // GUIの有効化
            GUI.enabled = true;

            pmdPrefab = EditorGUILayout.ObjectField("PMD Prefab", pmdPrefab, typeof(Object), false) as GameObject;
            createAnimationFile = EditorGUILayout.Toggle("Create Asset", createAnimationFile);
            interpolationQuality = EditorGUILayout.IntSlider("Interpolation Quality", interpolationQuality, 1, 10);

            // Convertボタン
            EditorGUILayout.Space();
            if (message.Length != 0)
            {
                GUILayout.Label(message);
            }
            else
            {
                if (GUILayout.Button("Convert"))
                {
                    if (null == motion_agent) {
						var obj = (VMDScriptableObject)target;
                        motion_agent = new MotionAgent(obj.assetPath);
                    }
                    motion_agent.CreateAnimationClip(pmdPrefab, createAnimationFile, interpolationQuality);
                    message = "Loading done.";
                }
            }
            GUILayout.Space(40);

            // モデル情報
            if (motion_agent == null) return;
            EditorGUILayout.LabelField("Model Name");
            GUI.enabled = false;
            EditorGUILayout.TextArea(motion_agent.model_name);
            GUI.enabled = true;
        }
    }
}
#endif