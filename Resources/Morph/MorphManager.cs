using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// モーフ管理クラス
/// </summary>
public class MorphManager : MonoBehaviour
{
	// 表情の種類
	public enum PanelType
	{
		Base,
		EyeBrow,
		Eye,
		Lip,
		Other,
	}

	public Mesh mesh;	// メッシュ
	private Mesh renderer_shared_mesh_;	// レンダー共有メッシュ
	public Material[] materials;	// マテリアル
	private Material[] renderer_shared_materials_;	// レンダー共有マテリアル

	/// <summary>
	/// 頂点モーフ
	/// </summary>
	[System.Serializable]
	public class VertexMorphPack {
		public int[] indices;			// 頂点インデックス
		public Vector3[] source;		// 頂点元座標
		public VertexMorph[] script;	// 頂点モーフのスクリプト配列
		
		public VertexMorphPack(int[] i = null, Vector3[] s = null, VertexMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public VertexMorphPack vertex_morph = null;	// 頂点モーフ

	/// <summary>
	/// UVモーフ
	/// </summary>
	[System.Serializable]
	public class UvMorphPack {
		public int[] indices;		// UVインデックス
		public Vector2[] source;	// UV元座標
		public UvMorph[] script;	// UVモーフのスクリプト配列
		
		public UvMorphPack(int[] i = null, Vector2[] s = null, UvMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public UvMorphPack[] uv_morph;			// UVモーフ
	
	///材質モーフ
	[System.Serializable]
	public class MaterialMorphPack {
		public int[] indices;		// 材質インデックス
		public MaterialMorph.MaterialMorphParameter[] source;	// 材質元データ
		public MaterialMorph[] script;	// 材質モーフのスクリプト配列
		
		public MaterialMorphPack(int[] i = null, MaterialMorph.MaterialMorphParameter[] s = null, MaterialMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public MaterialMorphPack material_morph;	// 材質モーフ


	/// <summary>
	/// 初回更新前処理
	/// </summary>
	void Start()
	{
		SkinnedMeshRenderer smr = transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>();
		
		//メッシュ
		renderer_shared_mesh_ = (Mesh)Instantiate(mesh); //複製して、書き換えはそちらで行う
		renderer_shared_mesh_.name = "MorphManager/" + mesh.name;
		smr.sharedMesh = renderer_shared_mesh_;
		
		//マテリアル
		renderer_shared_materials_ = smr.materials;
	}
	
	/// <summary>
	/// 後更新処理
	/// </summary>
	void LateUpdate()
	{
		//描画確認
		if(!transform.parent.renderer.enabled) {
			//描画されていないなら
			//実行しない
			return;
		}
		
		//頂点モーフ計算
		ComputeVertexMorph();
		
		//UVモーフ計算
		ComputeUvMorph();
		
		//材質モーフ計算
		ComputeMaterialMorph();
	}

	/// <summary>
	/// 頂点モーフ計算
	/// </summary>
	void ComputeVertexMorph()
	{
		if (0 < vertex_morph.indices.Length) {
			//各表情の合成ベクトルを初期化しておく
			Vector3[] composite = new Vector3[vertex_morph.source.Length];
			System.Array.Copy(vertex_morph.source, composite, vertex_morph.source.Length);
	
			// 表情ごとに計算する
			foreach (var morph in vertex_morph.script) {
				morph.Compute(composite);
			}
	
			// ここで計算結果を入れていく
			var vtxs = renderer_shared_mesh_.vertices;	// 配列を受け入れ
			for (int i = 0, i_max = vertex_morph.indices.Length; i < i_max; ++i) {
				vtxs[vertex_morph.indices[i]] = composite[i];
			}
			renderer_shared_mesh_.vertices = vtxs;	// ここで反映
		}
	}
	
	/// <summary>
	/// UVモーフ計算
	/// </summary>
	void ComputeUvMorph()
	{
		for (int i = 0, i_max = System.Math.Min(uv_morph.Length, 2); i < i_max; ++i) {
			//各表情の合成ベクトルを初期化しておく
			Vector2[] composite = new Vector2[uv_morph[i].source.Length];
			System.Array.Copy(uv_morph[i].source, composite, uv_morph[i].source.Length);
			
			// 表情ごとに計算する
			foreach (var morph in uv_morph[i].script) {
				morph.Compute(composite);
			}
			
			// ここで計算結果を入れていく
			var uvs = ((0 == i)? renderer_shared_mesh_.uv: renderer_shared_mesh_.uv2);	// 配列を受け入れ
			for (int k = 0, k_max = uv_morph[i].indices.Length; k < k_max; ++k) {
				uvs[uv_morph[i].indices[k]] = composite[k];
			}
			if (0 == i) {
				renderer_shared_mesh_.uv = uvs;	// ここで反映
			} else {
				renderer_shared_mesh_.uv2 = uvs;	// ここで反映
			}
		}
	}
	
	/// <summary>
	/// 材質モーフ計算
	/// </summary>
	void ComputeMaterialMorph()
	{
		//各材質を初期化しておく
		MaterialMorph.MaterialMorphParameter[] composite = new MaterialMorph.MaterialMorphParameter[material_morph.source.Length];
		System.Array.Copy(material_morph.source, composite, material_morph.source.Length);
		
		// 表情ごとに計算する
		foreach (var morph in material_morph.script) {
			morph.Compute(composite);
		}
		
		// ここで計算結果を入れていく
		for (int i = 0, i_max = composite.Length; i < i_max; ++i) {
			renderer_shared_materials_[material_morph.indices[i]].SetFloat("_Opacity", composite[i].color.a);
			composite[i].color.a = 1.0f;
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_Color", composite[i].color);
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_AmbColor", composite[i].ambient);
			renderer_shared_materials_[material_morph.indices[i]].SetFloat("_Shininess", composite[i].ambient.a);
			composite[i].ambient.a = 1.0f;
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_SpecularColor", composite[i].ambient);
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_OutlineColor", composite[i].outline_color);
			renderer_shared_materials_[material_morph.indices[i]].SetFloat("_OutlineWidth", composite[i].outline_width);
#if MFU_CHANGEABLE_TEXTURE_COLOR_SHADER //テクスチャカラーの変更出来るシェーダーが無いので無効化
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_MainTexColor", composite[i].texture_color);
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_SphereTexColor", composite[i].sphere_color);
			renderer_shared_materials_[material_morph.indices[i]].SetColor("_ToonTexColor", composite[i].toon_color);
#endif //MFU_CHANGEABLE_TEXTURE_COLOR_SHADER
		}
	}
}
