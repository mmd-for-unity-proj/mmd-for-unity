using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using MMD.PMD;

namespace MMD
{
    /// <summary>
    /// MFU全体で必要そうなコンフィグ管理
    /// </summary>
    [Serializable]
    public class Config : ScriptableObject
    {
        public InspectorConfig inspector_config;
        public DefaultPMDImportConfig pmd_config;
        public DefaultVMDImportConfig vmd_config;

        private List<ConfigBase> update_list;
        public void OnEnable()
        {
            // Inspectorで編集をさせない
            hideFlags = HideFlags.NotEditable;
            if (pmd_config == null)
            {
                // ここで初期化処理を書く
                pmd_config = new DefaultPMDImportConfig();
                vmd_config = new DefaultVMDImportConfig();
                inspector_config = new InspectorConfig();
            }
            if (update_list == null)
            {
                update_list = new List<ConfigBase>();
                update_list.Add(inspector_config);
                update_list.Add(pmd_config);
                update_list.Add(vmd_config);
            }
        }

        /// <summary>
        /// GUI描画処理
        /// </summary>
        public void OnGUI()
        {
            if (update_list == null) return;
            update_list.ForEach((item) =>
            {
                item.OnGUI();
            });
        }

        /// <summary>
        /// Configが配置された場所から保存先を生成します
        /// </summary>
        /// <returns>アセット保存先のパス</returns>
        public static string GetConfigPath()
        {
            var path = AssetDatabase.GetAllAssetPaths().Where(item => item.Contains("Config.cs")).First();
            path = path.Substring(0, path.LastIndexOf('/') + 1) + "Config.asset";
            return path;
        }

        /// <summary>
        /// Config.assetを読み込みます。なかったら作ります。
        /// </summary>
        /// <returns>読み込んで生成したConfigオブジェクト</returns>
        public static Config LoadAndCreate()
        {
            var path = Config.GetConfigPath();
            var config = (Config)AssetDatabase.LoadAssetAtPath(path, typeof(Config));

            //// なかったら作成する
            if (config == null)
            {
                config = CreateInstance<Config>();
                AssetDatabase.CreateAsset(config, path);
                EditorUtility.SetDirty(config);
            }
			Debug.Log(config);
            return config;
        }
    }

    /// <summary>
    ///インスペクタのコンフィグ
    /// </summary>
    [Serializable]
    public class InspectorConfig : ConfigBase
    {
        public bool use_pmd_preload = false;
        public bool use_vmd_preload = false;

        public InspectorConfig()
        {
            this.title = "Inspector Config";
        }

        public override void OnGUI()
        {
            base.OnGUI(() =>
                {
                    use_pmd_preload = EditorGUILayout.Toggle("Use PMD Preload", use_pmd_preload);
                    use_vmd_preload = EditorGUILayout.Toggle("Use VMD Preload", use_vmd_preload);
                }
            );
        }
    }

    /// <summary>
    /// PMDインポートのデフォルトコンフィグ
    /// </summary>
    [Serializable]
    public class DefaultPMDImportConfig : ConfigBase
    {
        public PMDConverter.ShaderType shader_type = PMDConverter.ShaderType.MMDShader;
        public bool use_mecanim = false;
        public bool rigidFlag = true;
        public bool use_ik = true;
        public float scale = 0.085f;
        public bool is_pmx_base_import = false;

        public DefaultPMDImportConfig()
        {
            this.title = "Default PMD Import Config";
        }

        public override void OnGUI()
        {
            base.OnGUI(() =>
                {
                    shader_type = (PMDConverter.ShaderType)EditorGUILayout.EnumPopup("Shader Type", shader_type);
                    rigidFlag = EditorGUILayout.Toggle("Rigidbody", rigidFlag);
                    use_mecanim = false;
                    use_ik = EditorGUILayout.Toggle("Use IK", use_ik);
                    is_pmx_base_import = EditorGUILayout.Toggle("Use PMX Base Import", is_pmx_base_import);
                }
            );
        }
    }

    /// <summary>
    /// VMDインポートのデフォルトコンフィグ
    /// </summary>
    [Serializable]
    public class DefaultVMDImportConfig : ConfigBase
    {
        public bool createAnimationFile;
        public int interpolationQuality;

        public DefaultVMDImportConfig()
        {
            this.title = "Default VMD Import Config";
        }

        public override void OnGUI()
        {
            base.OnGUI(() =>
                {
                    createAnimationFile = EditorGUILayout.Toggle("Create Asset", createAnimationFile);
                    interpolationQuality = EditorGUILayout.IntSlider("Interpolation Quality", interpolationQuality, 1, 10);
                }
            );
        }
    }

    /// <summary>
    /// コンフィグ用のベースクラスです
    /// </summary>
    public class ConfigBase
    {
        /// <summary>
        /// このコンフィグのタイトルを指定します
        /// </summary>
        protected string title = "";

        /// <summary>
        /// 開け閉めの状態
        /// </summary>
        private bool fold = true;

        /// <summary>
        /// GUI処理を行います
        /// </summary>
        /// <param name="OnGUIFunction">引数・戻り値なしのラムダ式</param>
        public void OnGUI(Action OnGUIFunction)
        {
            fold = EditorGUILayout.Foldout(fold, title);
            if (fold)
                OnGUIFunction();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// GUI処理を行います
        /// </summary>
        public virtual void OnGUI()
        {
        }
    }
}
