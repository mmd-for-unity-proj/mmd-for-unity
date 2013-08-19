using UnityEngine;
using UnityEditor;
using System.Collections;

public class LoadedWindow : EditorWindow 
{
	/// <summary>
	/// メッセージ用テキスト
	/// </summary>
	public string Text { get; set; }

	const int width = 400;

	const int height = 300;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <returns>ウィンドウ</returns>
	public static LoadedWindow Init()
	{
		var window = EditorWindow.GetWindow<LoadedWindow>("PMD file loaded!") as LoadedWindow;
		var pos = window.position;
		pos.height = LoadedWindow.height;
		pos.width = LoadedWindow.width;
		window.position = pos;
		return window;
	}

	void OnGUI()
	{
		EditorGUI.TextArea(new Rect(0, 0, LoadedWindow.width, LoadedWindow.height - 30), this.Text);

		if (GUI.Button(new Rect(0, height - 30, LoadedWindow.width, 30), "OK"))
			Close();
	}
}
