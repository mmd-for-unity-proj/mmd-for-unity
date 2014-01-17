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
        static Config config_ = null;
        public InspectorConfig inspector_config = null;
        public PMDImportConfig pmd_config = null;
        public VMDImportConfig vmd_config = null;

        private List<ConfigBase> update_list = null;
        public void OnEnable()
        {
            if (inspector_config == null)
            {
                inspector_config = new InspectorConfig();
            }
            if (pmd_config == null)
            {
                pmd_config = new PMDImportConfig();
            }
            if (vmd_config == null)
            {
                vmd_config = new VMDImportConfig();
            }
            if (update_list == null)
            {
                update_list = new List<ConfigBase>();
                update_list.Add(inspector_config);
                update_list.Add(pmd_config);
                update_list.Add(vmd_config);
            }

            hideFlags = HideFlags.None; //以前の書き換え不可assetが残っているかもしれないので明示的に書き換え可能を設定
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

            //変更確認
            if (GUI.changed) {
                EditorUtility.SetDirty(config_);
            }
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
            if (config_ == null)
            {
                var path = Config.GetConfigPath();
                config_ = (Config)AssetDatabase.LoadAssetAtPath(path, typeof(Config));
                
                //// なかったら作成する
                if (config_ == null)
                {
                    config_ = CreateInstance<Config>();
                    AssetDatabase.CreateAsset(config_, path);
                    EditorUtility.SetDirty(config_);
                }
            }
            return config_;
        }
    }

    /// <summary>
    ///インスペクタのコンフィグ
    /// </summary>
    [Serializable]
    public class InspectorConfig : ConfigBase
    {
        public bool use_pmd_preload = true;
        public bool use_vmd_preload = true;

        public override string GetTitle()
        {
            return "Inspector Config";
        }

        public override void OnGUIFunction()
        {
            use_pmd_preload = EditorGUILayout.Toggle("Use PMD Preload", use_pmd_preload);
            use_vmd_preload = EditorGUILayout.Toggle("Use VMD Preload", use_vmd_preload);
        }

        public InspectorConfig Clone()
        {
            return (InspectorConfig)MemberwiseClone();
        }
    }

    /// <summary>
    /// PMDインポートのコンフィグ
    /// </summary>
    [Serializable]
    public class PMDImportConfig : ConfigBase
    {
        public PMDConverter.ShaderType shader_type = PMDConverter.ShaderType.MMDShader;
        public PMXConverter.AnimationType animation_type = PMXConverter.AnimationType.LegacyAnimation;
        public bool rigidFlag = true;
        public bool use_ik = true;
        public float scale = 0.085f;
        public bool is_pmx_base_import = true;

        public override string GetTitle()
        {
            return "Default PMD Import Config";
        }

        public override void OnGUIFunction()
        {
            shader_type = (PMDConverter.ShaderType)EditorGUILayout.EnumPopup("Shader Type", shader_type);
            rigidFlag = EditorGUILayout.Toggle("Rigidbody", rigidFlag);
            animation_type = (PMXConverter.AnimationType)EditorGUILayout.EnumPopup("Animation Type", animation_type);
            use_ik = EditorGUILayout.Toggle("Use IK", use_ik);
            scale = EditorGUILayout.Slider("Scale", scale, 0.001f, 1.0f);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(" ");
                if (GUILayout.Button("Original", EditorStyles.miniButtonLeft)) {
                    scale = 0.085f;
                }
                if (GUILayout.Button("1.0", EditorStyles.miniButtonRight)) {
                    scale = 1.0f;
                }
            }
            EditorGUILayout.EndHorizontal();
            is_pmx_base_import = EditorGUILayout.Toggle("Use PMX Base Import", is_pmx_base_import);
        }

        public PMDImportConfig Clone()
        {
            return (PMDImportConfig)MemberwiseClone();
        }
    }

    /// <summary>
    /// VMDインポートのコンフィグ
    /// </summary>
    [Serializable]
    public class VMDImportConfig : ConfigBase
    {
        public bool createAnimationFile = false;
        public int interpolationQuality = 1;

        public override string GetTitle()
        {
            return "Default VMD Import Config";
        }

        public override void OnGUIFunction()
        {
            createAnimationFile = EditorGUILayout.Toggle("Create Asset", createAnimationFile);
            interpolationQuality = EditorGUILayout.IntSlider("Interpolation Quality", interpolationQuality, 1, 10);
        }

        public VMDImportConfig Clone()
        {
            return (VMDImportConfig)MemberwiseClone();
        }
    }

    /// <summary>
    /// コンフィグ用のベースクラスです
    /// </summary>
    public class ConfigBase
    {
        /// <summary>
        /// 開け閉めの状態
        /// </summary>
        private bool fold = true;

        /// <summary>
        /// GUI処理を行います
        /// </summary>
        public void OnGUI()
        {
            var title = GetTitle();
            fold = EditorGUILayout.Foldout(fold, title);
            if (fold) {
                OnGUIFunction();
            }
            EditorGUILayout.Space();
        }

        /// <summary>
        /// このコンフィグのタイトルを取得します
        /// </summary>
        public virtual string GetTitle()
        {
            return "";
        }

        /// <summary>
        /// GUI処理を行います
        /// </summary>
        public virtual void OnGUIFunction()
        {
        }
    }
}
