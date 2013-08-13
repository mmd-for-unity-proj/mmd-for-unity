using UnityEngine;
using System.Collections;

/// <summary>
/// 物理演算管理(基本的にInspector拡張用に付加するだけで、このクラス自体は何もしない)
/// </summary>
public class PhysicsManager : MonoBehaviour {

	/// <summary>
	/// 初回更新前処理
	/// </summary>
	public void Start()
	{
		//再生時には不要なので、ビルド版では削除
		connect_bone_list = null;
	}

	/// <summary>
	/// hysicsに移動する前のGameObject
	/// </summary>
	[System.Serializable]
	public class ConnectBone {
		public GameObject	joint;
		public GameObject	parent;
		
		public ConnectBone(GameObject j = null, GameObject p = null) {joint = j; parent = p;}
	}

	public ConnectBone[] connect_bone_list = null;	// //hysicsに移動する前のGameObjectリスト
}
