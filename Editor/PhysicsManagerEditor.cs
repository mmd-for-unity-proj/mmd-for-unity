using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 物理演算用Inspector拡張
/// </summary>
[CustomEditor(typeof(PhysicsManager))]
public sealed class PhysicsManagerEditor : Editor
{
	/// <summary>
	/// 初回処理
	/// </summary>
	public void Awake()
	{
		PhysicsManager self = (PhysicsManager)target;
		if (null != self.connect_bone_list) {
			joint_list_ = self.connect_bone_list.Where(x=>(null != x.joint))
												.Select(x=>x.joint.GetComponent<ConfigurableJoint>())
												.ToArray();
		} else {
			joint_list_ = null;
		}
	}
	
	/// <summary>
	/// Inspector描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		if (null != joint_list_) {
			//再生時は書き換え不可
			bool old_gui_enabled = GUI.enabled;
			bool crnt_gui_enabled = !Application.isPlaying;
			GUI.enabled = crnt_gui_enabled;
			
			foreach(var joint in joint_list_) {
				GameObject parent_object = GetOriginalParent(joint);
				if (null == parent_object) {
					//Physicsに移動する前のGameObjectが無いなら
					//ラベルのみ
					GUI.enabled = false;
					EditorGUILayout.LabelField(joint.name);
					GUI.enabled = crnt_gui_enabled;
				} else if (joint.rigidbody.isKinematic) {
					//ボーン追従(isKinematic)オブジェクト付属のジョイントなら
					//移動不可
					GUI.enabled = false;
					EditorGUILayout.Toggle(joint.name, false);
					GUI.enabled = crnt_gui_enabled;
				} else if (Selection.activeTransform == joint.transform.parent) {
					//Physicsに属しているなら
					//移動可
					if (true != EditorGUILayout.Toggle(joint.name, true)) {
						//変更なら
						//ジョイントツリーに移動
						joint.transform.parent = parent_object.transform;
					}
				} else if (parent_object.transform == joint.transform.parent) {
					//ジョイントツリーに属している
					//移動可
					if (false != EditorGUILayout.Toggle(joint.name, false)) {
						//変更なら
						//Physicsに移動
						joint.transform.parent = Selection.activeTransform;
					}
				} else {
					//手動で移動したと思われる
					//ラベルのみ
					GUI.enabled = false;
					EditorGUILayout.LabelField(joint.name);
					GUI.enabled = crnt_gui_enabled;
				}
			}
			
			GUI.enabled = old_gui_enabled;
		}
	}
	
	/// <summary>
	/// Physicsに移動する前のGameObjectを取得する
	/// </summary>
	private GameObject GetOriginalParent(ConfigurableJoint joint)
	{
		PhysicsManager self = (PhysicsManager)target;
		return self.connect_bone_list.Where(x=>(joint.gameObject == x.joint))
									.Select(x=>x.parent)
									.FirstOrDefault();
	}

	private ConfigurableJoint[] joint_list_;	//移動対象リスト
}
