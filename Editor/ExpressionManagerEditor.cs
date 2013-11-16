using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// 表情用Inspector拡張
/// </summary>
[CustomEditor(typeof(ExpressionManagerScript))]
public sealed class ExpressionManagerEditor : Editor
{
	/// <summary>
	/// スタティックコンストラクタ
	/// </summary>
	static ExpressionManagerEditor()
	{
		skin_displays_ = new bool[Enum.GetValues(typeof(MMDSkinsScript.SkinType)).Length];
		for (int i = 0, i_max = skin_displays_.Length; i < i_max; ++i) {
			skin_displays_[i] = true;
		}
	}
	
	/// <summary>
	/// 初回処理
	/// </summary>
	public void Awake()
	{
		children_ = new List<Transform>[Enum.GetValues(typeof(MMDSkinsScript.SkinType)).Length];
		for (int i = 0, i_max = children_.Length; i < i_max; ++i) {
			children_[i] = new List<Transform>();
		}
		//子登録
		foreach (Transform child in Selection.activeTransform) {
			MMDSkinsScript skins = child.GetComponent<MMDSkinsScript>();
			children_[(int)skins.skinType].Add(child);
		}
	}
	
	/// <summary>
	/// Inspector描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		ExpressionManagerScript self = (ExpressionManagerScript)target;
		bool is_dirty = false;
		
		//mesh
		self.mesh = (Mesh)EditorGUILayout.ObjectField("Mesh", self.mesh, typeof(Mesh), false);
		
		//skin_script
		for (int i = 0, i_max = children_.Length; i < i_max; ++i) {
			if (MMDSkinsScript.SkinType.Base == (MMDSkinsScript.SkinType)i) {
				//Baseだったらスキップ
				continue;
			}
			
			//スキンツリータイトル
			string skin_name = ((MMDSkinsScript.SkinType)i).ToString();
			skin_displays_[i] = EditorGUILayout.Foldout(skin_displays_[i], skin_name);
			//スキンツリー内部
			if (skin_displays_[i]) {
				//このスキンを表示するなら
				GUIStyle style = new GUIStyle();
				style.margin.left = 10;
				EditorGUILayout.BeginVertical(style);
				{
					//モーフオブジェクト走査
					foreach (Transform child in children_[i]) {
						float value = child.localPosition.z;
						value = EditorGUILayout.Slider(child.name, value, 0.0f, 1.0f);
						if (child.localPosition.z != value) {
							//変更が掛かったなら
							//Undo登録
#if !UNITY_4_2 //4.3以降
							Undo.RecordObject(child, "Expression Change");
#else
							Undo.RegisterUndo(child, "Expression Change");
#endif
							//Z位置更新
							Vector3 position = child.localPosition;
							position.z = value;
							child.localPosition = position;
							//改変モーフオブジェクトのInspector更新
							EditorUtility.SetDirty(child.transform);
							
							is_dirty = true;
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
			
		}

		if (is_dirty) {
			//更新が有ったなら
			//Inspector更新
			EditorUtility.SetDirty(target);
		}
	}
	
	private static	bool[]				skin_displays_;	//スキンの表示
	private			List<Transform>[]	children_;		//スキン別子モーフオブジェクト
}
