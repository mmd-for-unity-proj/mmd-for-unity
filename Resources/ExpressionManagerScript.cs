using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 表情の管理クラス
/// </summary>
public class ExpressionManagerScript : MonoBehaviour
{
	public int[] indices;

	public Vector3[] source;		// 元頂点, source_position

	public Vector3[] composite;

	public Mesh mesh;	// メッシュ
	private Mesh renderer_shared_mesh;	// レンダー共有メッシュ

	public MMDSkinsScript[] skin_script;	// 子供の表情のスクリプト配列

	//int lip_count = 0;
	//int eye_count = 0;
	//int eye_brow_count = 0;
	//int other_count = 0;

	void Init()
	{
		// meshの取得
		if (!mesh) {
			//後方互換用コード(新コンバータでは mesh は必ず設定される)
			//メッシュが設定されていなければ SkinnedMeshRenderer から取得する
			mesh = transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		}
		renderer_shared_mesh = (Mesh)Instantiate(mesh); //複製して、書き換えはそちらで行う
		renderer_shared_mesh.name = "ExpressionManagerScript/" + mesh.name;
		transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = renderer_shared_mesh;

		// 頂点インデックス取得
		indices = transform.FindChild("base").GetComponent<MMDSkinsScript>().targetIndices;

		skin_script = GetSkinScripts();		// 表情に付いているスクリプトを取得

		// 元頂点配列に入れていく
		source = new Vector3[indices.Length];
		for (int i = 0; i < indices.Length; i++)
			source[i] = mesh.vertices[indices[i]];

		// 合成するベクトル配列
		composite = new Vector3[indices.Length];
		for (int i = 0; i < indices.Length; i++)
			composite[i] = Vector3.zero;
	}

	void Start()
	{
		Init();
	}

	// SkinScriptの配列を子供の表情から探して拾ってくる
	MMDSkinsScript[] GetSkinScripts()
	{
		// 表情のスクリプトを拾ってくる
		var scripts = new MMDSkinsScript[transform.childCount];
		for (int i = 0; i < scripts.Length; i++)
			scripts[i] = transform.GetChild(i).GetComponent<MMDSkinsScript>();

		return scripts;
	}

	void LateUpdate()
	{
		Renderer renderer=transform.parent.GetComponent<Renderer>();
		if(!renderer.enabled)
			return;
		var vtxs = renderer_shared_mesh.vertices;	// 配列を受け入れ

		//各表情の合成ベクトルを初期化しておく
		for (int i = 0; i < indices.Length; i++)
			composite[i] = Vector3.zero;
		// 表情ごとに計算する
		foreach (var s in this.skin_script)
		{
			s.Compute(composite);
		}

		// ここで計算結果を入れていく
		for (int i = 0; i < indices.Length; i++)
		{
			vtxs[indices[i]] = source[i] + composite[i];
		}

		renderer_shared_mesh.vertices = vtxs;	// ここで反映
	}
	
}
