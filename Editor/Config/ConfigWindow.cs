using UnityEngine;
using UnityEditor;

namespace MMD
{
    public class ConfigWindow : EditorWindow
    {
        private static Config config;
        private static string path;

        [MenuItem("Plugins/MMD Loader/Config")]
        public static void Init()
        {
            GetWindow<ConfigWindow>("MFU Config");
        }

        // Windowが有効化されたとき
        //   フォーカスが外れて戻ってきたときや再度開かれたときなど
        void OnEnable()
        {
            // オブジェクトを「Hierarchy」に表示しない。また、アセットの中にあれば、プロジェクトビューに表示しない
            // オブジェクトがシーンに保存されない。また、新しいシーンを読んでも、オブジェクトが破棄されない
            hideFlags = HideFlags.HideAndDontSave;

            if (config == null)
            {
                // 読み込む
                config = MMD.Config.LoadAndCreate();

                // なかったら作成する
                if (config == null)
                {
                    path = MMD.Config.GetConfigPath();
                    config = CreateInstance<Config>();
                    AssetDatabase.CreateAsset(config, path);
                    EditorUtility.SetDirty(config);
                }
            }
        }

        // ウィンドウの描画処理
        void OnGUI()
        {
            // たいとる
            EditorGUILayout.LabelField("MMD for Unity Configuration");
            EditorGUILayout.Space();

            // あとは任せる
            config.OnGUI();

        }
    }
}