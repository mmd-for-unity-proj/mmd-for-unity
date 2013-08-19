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
	public Transform[] bones;	// ボーン
	public MorphBase[] morphs;	// モーフ

	/// <summary>
	/// グループモーフ
	/// </summary>
	[System.Serializable]
	public class GroupMorphPack {
		public int[] indices;			// グループインデックス
		public float[] source;			// グループ元データ
		public GroupMorph[] script;		// グループモーフのスクリプト配列
		
		public GroupMorphPack(int[] i = null, float[] s = null, GroupMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public GroupMorphPack group_morph = null;	// グループモーフ

	/// <summary>
	/// ボーンモーフ
	/// </summary>
	[System.Serializable]
	public class BoneMorphPack {
		public int[] indices;							// ボーンインデックス
		public BoneMorph.BoneMorphParameter[] source;	// ボーン元データ
		public BoneMorph[] script;						// ボーンモーフのスクリプト配列
		
		public BoneMorphPack(int[] i = null, BoneMorph.BoneMorphParameter[] s = null, BoneMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public BoneMorphPack bone_morph = null;				// ボーンモーフ

	/// <summary>
	/// 頂点モーフ
	/// </summary>
	[System.Serializable]
	public class VertexMorphPack {
		public int[] indices;					// 頂点インデックス
		public Vector3[] source;				// 頂点元データ
		public VertexMorph[] script;			// 頂点モーフのスクリプト配列
		
		public VertexMorphPack(int[] i = null, Vector3[] s = null, VertexMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public VertexMorphPack vertex_morph = null;	// 頂点モーフ

	/// <summary>
	/// UVモーフ
	/// </summary>
	[System.Serializable]
	public class UvMorphPack {
		public int[] indices;		// UVインデックス
		public Vector2[] source;	// UV元データ
		public UvMorph[] script;	// UVモーフのスクリプト配列
		
		public UvMorphPack(int[] i = null, Vector2[] s = null, UvMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public UvMorphPack[] uv_morph;	// UVモーフ
	
	/// <summary>
	/// 材質モーフ
	/// </summary>
	[System.Serializable]
	public class MaterialMorphPack {
		public int[] indices;									// 材質インデックス
		public MaterialMorph.MaterialMorphParameter[] source;	// 材質元データ
		public MaterialMorph[] script;							// 材質モーフのスクリプト配列
		
		public MaterialMorphPack(int[] i = null, MaterialMorph.MaterialMorphParameter[] s = null, MaterialMorph[] c = null) {indices = i; source = s; script = c;}
	}
	public MaterialMorphPack material_morph;					// 材質モーフ

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
		
		//グループモーフ計算
		ComputeGroupMorph();
		
		//ボーンモーフ計算
		ComputeBoneMorph();
		
		//頂点モーフ計算
		ComputeVertexMorph();
		
		//UVモーフ計算
		ComputeUvMorph();
		
		//材質モーフ計算
		ComputeMaterialMorph();
	}

	/// <summary>
	/// グループモーフ計算
	/// </summary>
	void ComputeGroupMorph()
	{
		if (0 < group_morph.indices.Length) {
			//各表情の合成ベクトルを初期化しておく
			float[] composite = new float[group_morph.source.Length];
			System.Array.Copy(group_morph.source, composite, group_morph.source.Length);
	
			// 表情ごとに計算する
			foreach (var morph in group_morph.script) {
				morph.Compute(composite);
			}
	
			// ここで計算結果を入れていく
			for (int i = 0, i_max = group_morph.indices.Length; i < i_max; ++i) {
				MorphBase morph_base = morphs[group_morph.indices[i]];
				morph_base.group_weight = composite[i];	// ここで反映
			}
		}
	}
	

	/// <summary>
	/// ボーンモーフ計算
	/// </summary>
	void ComputeBoneMorph()
	{
		if (0 < bone_morph.indices.Length) {
			//各表情の合成ベクトルを初期化しておく
			BoneMorph.BoneMorphParameter[] composite = new BoneMorph.BoneMorphParameter[bone_morph.source.Length];
			System.Array.Copy(bone_morph.source, composite, bone_morph.source.Length);
	
			// 表情ごとに計算する
			foreach (var morph in bone_morph.script) {
				morph.Compute(composite);
			}
	
			// ここで計算結果を入れていく
			for (int i = 0, i_max = bone_morph.indices.Length; i < i_max; ++i) {
				bones[bone_morph.indices[i]].localPosition = composite[i].position;
				bones[bone_morph.indices[i]].localRotation = composite[i].rotation;
			}
		}
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
		MaterialMorph.MaterialMorphParameter[] composite_mul = Enumerable.Repeat(MaterialMorph.MaterialMorphParameter.one, material_morph.source.Length).ToArray();
		MaterialMorph.MaterialMorphParameter[] composite_add = Enumerable.Repeat(MaterialMorph.MaterialMorphParameter.zero, material_morph.source.Length).ToArray();
		
		// 表情ごとに計算する
		foreach (var morph in material_morph.script) {
			morph.Compute(composite_mul, composite_add);
		}
		
		//全材質計算
		if (-1 == material_morph.indices.LastOrDefault()) {
			//最後に-1(≒uint.MaxValue)が有れば
			//全材質に反映
			MaterialMorph.MaterialMorphParameter composite_mul_all = composite_mul.Last();
			MaterialMorph.MaterialMorphParameter composite_add_all = composite_add.Last();
			for (int i = 0, i_max = material_morph.source.Length - 1; i < i_max; ++i) {
				composite_mul[i] *= composite_mul_all;
				composite_add[i] += composite_add_all;
			}
		}
		
		// ここで計算結果を入れていく
		for (int i = 0, i_max = material_morph.source.Length - 1; i < i_max; ++i) {
			int index = material_morph.indices[i];
			ApplyMaterialMorph(renderer_shared_materials_[index], material_morph.source[i], composite_mul[i], composite_add[i]);
		}
	}

	/// <summary>
	/// 材質モーフ反映
	/// </summary>
	/// <param name='material'>反映先マテリアル</param>
	/// <param name='composite'>反映するデータ</param>
	private static void ApplyMaterialMorph(Material material, MaterialMorph.MaterialMorphParameter source, MaterialMorph.MaterialMorphParameter composite_mul, MaterialMorph.MaterialMorphParameter composite_add) {
		MaterialMorph.MaterialMorphParameter composite = source * composite_mul + composite_add;
		material.SetColor("_Color", composite.color);
		material.SetFloat("_Opacity", composite.color.a);
		material.SetColor("_AmbColor", composite.ambient);
		material.SetColor("_SpecularColor", composite.specular);
		material.SetFloat("_Shininess", composite.specular.a);
		material.SetColor("_OutlineColor", composite.outline_color);
		material.SetFloat("_OutlineWidth", composite.outline_width);
#if MFU_CHANGEABLE_TEXTURE_COLOR_SHADER //テクスチャカラーの変更出来るシェーダーが無いので無効化
		material.SetColor("_MainTexColor", composite.texture_color);
		material.SetColor("_SphereTexColor", composite.sphere_color);
		material.SetColor("_ToonTexColor", composite.toon_color);
#endif //MFU_CHANGEABLE_TEXTURE_COLOR_SHADER
	}
}
