using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 表情用Inspector拡張
/// </summary>
[CustomEditor(typeof(MorphManager))]
public sealed class MorphManagerEditor : Editor
{
	/// <summary>
	/// スタティックコンストラクタ
	/// </summary>
	static MorphManagerEditor()
	{
		panel_displays_ = new bool[System.Enum.GetValues(typeof(MorphManager.PanelType)).Length];
		for (int i = 0, i_max = panel_displays_.Length; i < i_max; ++i) {
			panel_displays_[i] = true;
		}
	}
	
	/// <summary>
	/// 初回処理
	/// </summary>
	public void Awake()
	{
		MorphManager self = (MorphManager)target;

		children_ = new Transform[System.Enum.GetValues(typeof(MorphManager.PanelType)).Length][];
		//子登録
		for (int i = 0, i_max = children_.Length; i < i_max; ++i) {
			children_[i] = self.vertex_morph.script.Where(x=>x.panel == (MorphManager.PanelType)i)
													.Select(x=>x.transform)
													.Concat(self.uv_morph.SelectMany(x=>x.script)
																		.Where(x=>x.panel == (MorphManager.PanelType)i)
																		.Select(x=>x.transform)
															)
													.Concat(self.material_morph.script.Where(x=>x.panel == (MorphManager.PanelType)i)
																						.Select(x=>x.transform)
															)
													.Concat(self.bone_morph.script.Where(x=>x.panel == (MorphManager.PanelType)i)
																						.Select(x=>x.transform)
															)
													.Concat(self.group_morph.script.Where(x=>x.panel == (MorphManager.PanelType)i)
																						.Select(x=>x.transform)
															)
													.ToArray();
		}
	}
	
	/// <summary>
	/// Inspector描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		bool is_dirty = false;
		
		is_dirty = OnInspectorGUIforPanelList() || is_dirty;

		if (is_dirty) {
			//更新が有ったなら
			//Inspector更新
			EditorUtility.SetDirty(target);
		}
	}
	
	/// <summary>
	/// パネルリストの為のInspector描画
	/// </summary>
	/// <returns>更新が有ったか(true:更新有り, false:未更新)</returns>
	private bool OnInspectorGUIforPanelList()
	{
		bool is_update = false;
		
		for (int i = 0, i_max = children_.Length; i < i_max; ++i) {
			if (MorphManager.PanelType.Base == (MorphManager.PanelType)i) {
				//Baseだったらスキップ
				continue;
			}
			
			//パネルツリータイトル
			string panel_name = ((MMDSkinsScript.SkinType)i).ToString();
			panel_displays_[i] = EditorGUILayout.Foldout(panel_displays_[i], panel_name);
			//パネルツリー内部
			if (panel_displays_[i]) {
				//このパネルを表示するなら
				++EditorGUI.indentLevel;
				EditorGUILayout.BeginVertical();
				{
					//モーフオブジェクト走査
					foreach (Transform child in children_[i]) {
						float value = child.localPosition.z;
						value = EditorGUILayout.Slider(child.name, value, 0.0f, 1.0f);
						if (child.localPosition.z != value) {
							//変更が掛かったなら
							//Undo登録
#if !UNITY_4_2 //4.3以降
							Undo.RecordObject(child, "Morph Change");
#else
							Undo.RegisterUndo(child, "Morph Change");
#endif
							//Z位置更新
							Vector3 position = child.localPosition;
							position.z = value;
							child.localPosition = position;
							//改変モーフオブジェクトのInspector更新
							EditorUtility.SetDirty(child.transform);
							
							is_update = true;
						}
					}
				}
				EditorGUILayout.EndVertical();
				--EditorGUI.indentLevel;
			}
		}
		return is_update;
	}
	
	private static	bool[]			panel_displays_;	//パネルの表示
	private			Transform[][]	children_;			//パネル別子モーフオブジェクト
}
