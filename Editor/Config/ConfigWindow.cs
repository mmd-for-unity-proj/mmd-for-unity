using UnityEngine;
using UnityEditor;

namespace MMD
{
    public class ConfigWindow : EditorWindow
    {
        private Config config;

        [MenuItem("MMD for Unity/Config")]
        public static void Init()
        {
            GetWindow<ConfigWindow>("MFU Config");
        }

        // Windowが有効化されたとき
        //   フォーカスが外れて戻ってきたときや再度開かれたときなど
        void OnEnable()
        {
            if (config == null)
            {
                // 読み込む
                config = MMD.Config.LoadAndCreate();
            }
        }

        // ウィンドウの描画処理
        void OnGUI()
        {
            // あとは任せる
            config.OnGUI();
        }
    }
}
