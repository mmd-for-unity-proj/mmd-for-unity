using UnityEngine;
using UnityEditor;
using System.Collections;
using MMD.PMD;
using System.IO;

#if !(UNITY_3_5 || UNITY_3_4 || UNITY_3_3)
namespace MMD
{
	[CustomEditor(typeof(PMDScriptableObject))]
    public class PMDInspector : Editor
    {
        // PMD Load option
        public PMDConverter.ShaderType shader_type;
        public bool rigidFlag;
        public bool use_mecanim;
        public bool use_ik;
        public float scale;
        public bool is_pmx_base_import;

        // last selected item
        private static ModelAgent model_agent;
        private static string message = "";

        /// <summary>
        /// pmd_headerとデフォルトコンフィグの設定
        /// </summary>
        private void setup()
        {
            // デフォルトコンフィグ
			var config = MMD.Config.LoadAndCreate();
            shader_type = config.pmd_config.shader_type;
            rigidFlag = config.pmd_config.rigidFlag;
            use_mecanim = config.pmd_config.use_mecanim;
            use_ik = config.pmd_config.use_ik;
            scale = config.pmd_config.scale;
            is_pmx_base_import = config.pmd_config.is_pmx_base_import;
			
            // モデル情報
            if (config.inspector_config.use_pmd_preload)
            {
				var obj = (PMDScriptableObject)target;
				model_agent = new ModelAgent(obj.assetPath);
            }
            else
            {
                model_agent = null;
            }
        }

        /// <summary>
        /// Inspector上のGUI描画処理を行います
        /// </summary>
        public override void OnInspectorGUI()
        {
            setup();

            // GUIの有効化
            GUI.enabled = !EditorApplication.isPlaying;

            // シェーダの種類
            shader_type = (PMDConverter.ShaderType)EditorGUILayout.EnumPopup("Shader Type", shader_type);

            // 剛体を入れるかどうか
            rigidFlag = EditorGUILayout.Toggle("Rigidbody", rigidFlag);

            // Mecanimを使うかどうか
            GUI.enabled = false;
            use_mecanim = EditorGUILayout.Toggle("Use Mecanim", false);
            GUI.enabled = !EditorApplication.isPlaying;

            // IKを使うかどうか
            use_ik = EditorGUILayout.Toggle("Use IK", use_ik);

            // スケール
            scale = EditorGUILayout.Slider("Scale", scale, 0.001f, 1.0f);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(" ");
                if (GUILayout.Button("0.085", EditorStyles.miniButtonLeft)) {
                    scale = 0.085f;
                }
                if (GUILayout.Button("1.0", EditorStyles.miniButtonRight)) {
                    scale = 1.0f;
                }
            }
            EditorGUILayout.EndHorizontal();

            // PMX Baseでインポートするかどうか
            is_pmx_base_import = EditorGUILayout.Toggle("Use PMX Base Import", is_pmx_base_import);

            // Convertボタン
            EditorGUILayout.Space();
            if (message.Length != 0)
            {
                GUILayout.Label(message);
            }
            else
            {
                if (GUILayout.Button("Convert to Prefab"))
                {
                    if (null == model_agent) {
						var obj = (PMDScriptableObject)target;
                        model_agent = new ModelAgent(obj.assetPath);
                    }
                    model_agent.CreatePrefab(shader_type, rigidFlag, use_mecanim, use_ik, scale, is_pmx_base_import);
                    message = "Loading done.";
                }
            }
            GUILayout.Space(40);

            // モデル情報
            if (model_agent == null) return;
            EditorGUILayout.LabelField("Model Name");
            GUI.enabled = false;
            EditorGUILayout.TextArea(model_agent.name);
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Comment");
            GUI.enabled = false;
            EditorGUILayout.TextArea(model_agent.comment, GUILayout.Height(300));
            GUI.enabled = true;
        }
    }
}
#endif
