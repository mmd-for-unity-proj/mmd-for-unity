using UnityEngine;
using UnityEditor;
using System.Collections;
using MMD.PMD;
using System.IO;

namespace MMD
{
    [CustomEditor(typeof(VMDScriptableObject))]
    public class VMDInspector : Editor
    {
        // VMD Load option
        public GameObject pmdPrefab;
        VMDImportConfig vmd_config;

        // last selected item
        private MotionAgent motion_agent;
        private string message = "";

        /// <summary>
        /// 有効化処理
        /// </summary>
        void OnEnable()
        {
            // デフォルトコンフィグ
            var config = MMD.Config.LoadAndCreate();
            vmd_config = config.vmd_config.Clone();

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
            // GUIの有効化
            GUI.enabled = !EditorApplication.isPlaying;

            pmdPrefab = (GameObject)EditorGUILayout.ObjectField("PMD Prefab", pmdPrefab, typeof(Object), false);
            vmd_config.OnGUIFunction();

            // Convertボタン
            EditorGUILayout.Space();
            if (message.Length != 0)
            {
                GUILayout.Label(message);
            }
            else
            {
                bool gui_enabled_old = GUI.enabled;
                GUI.enabled = (null != pmdPrefab);
                if (GUILayout.Button("Convert"))
                {
                    if (null == motion_agent) {
                        var obj = (VMDScriptableObject)target;
                        motion_agent = new MotionAgent(obj.assetPath);
                    }
                    motion_agent.CreateAnimationClip(pmdPrefab
                                                    , vmd_config.createAnimationFile
                                                    , vmd_config.interpolationQuality
                                                    );
                    message = "Loading done.";
                }
                GUI.enabled = gui_enabled_old;
            }
            GUILayout.Space(40);

            // モデル情報
            if (motion_agent == null) return;
            EditorGUILayout.LabelField("Model Name");
            EditorGUILayout.LabelField(motion_agent.model_name, EditorStyles.textField);
        }
    }
}
