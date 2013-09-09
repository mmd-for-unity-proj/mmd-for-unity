using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// MMDEngine用Inspector拡張
/// </summary>
[CustomEditor(typeof(MMDEngine))]
public sealed class MMDEngineEditor : Editor
{
	/// <summary>
	/// スタティックコンストラクタ
	/// </summary>
	static MMDEngineEditor()
	{
		ik_list_display_ = false;
		shader_display_ = false;
	}
	
	/// <summary>
	/// 初回処理
	/// </summary>
	public void Awake()
	{
	}
	
	/// <summary>
	/// Inspector描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		bool is_dirty = false;
		
		is_dirty = OnInspectorGUIforOutlineWidth() || is_dirty;
		is_dirty = OnInspectorGUIforUseRigidbody() || is_dirty;
		is_dirty = OnInspectorGUIforIkList() || is_dirty;
		is_dirty = OnInspectorGUIforShaderList() || is_dirty;

		if (is_dirty) {
			//更新が有ったなら
			//Inspector更新
			EditorUtility.SetDirty(target);
		}
	}

	/// <summary>
	/// エッジ幅の為のInspector描画
	/// </summary>
	/// <returns>更新が有ったか(true:更新有り, false:未更新)</returns>
	private bool OnInspectorGUIforOutlineWidth()
	{
		MMDEngine self = (MMDEngine)target;
		bool is_update = false;
		
		float outline_width = self.outline_width;
		outline_width = EditorGUILayout.Slider("Outline Width", outline_width, 0.0f, 2.0f);
		if (self.outline_width != outline_width) {
			//変更が掛かったなら
			//Undo登録
			Undo.RegisterUndo(self, "Outline Width Change");
			//更新
			self.outline_width = outline_width;
			
			is_update = true;
		}
		return is_update;
	}
	
	/// <summary>
	/// リジッドボティ使用の為のInspector描画
	/// </summary>
	/// <returns>更新が有ったか(true:更新有り, false:未更新)</returns>
	private bool OnInspectorGUIforUseRigidbody()
	{
		MMDEngine self = (MMDEngine)target;
		bool is_update = false;
		
		bool use_rigidbody = self.useRigidbody;
		use_rigidbody = EditorGUILayout.Toggle("Use Rigidbody", use_rigidbody);
		if (self.useRigidbody != use_rigidbody) {
			//変更が掛かったなら
			//Undo登録
			Undo.RegisterUndo(self, "Use Rigidbody Change");
			//更新
			self.useRigidbody = use_rigidbody;
			
			is_update = true;
		}
		return is_update;
	}
	
	/// <summary>
	/// IKリストの為のInspector描画
	/// </summary>
	/// <returns>更新が有ったか(true:更新有り, false:未更新)</returns>
	private bool OnInspectorGUIforIkList()
	{
		MMDEngine self = (MMDEngine)target;
		bool is_update = false;
		
		//IKリストツリータイトル
		ik_list_display_ = EditorGUILayout.Foldout(ik_list_display_, "IK List");
		//IKリストツリー内部
		if (ik_list_display_) {
			//IKリストを表示するなら
			GUIStyle style = new GUIStyle();
			style.margin.left = 10;
			EditorGUILayout.BeginVertical(style);
			{
				foreach (CCDIKSolver ik in self.ik_list) {
					bool enabled = ik.enabled;
					enabled = EditorGUILayout.Toggle(ik.name, enabled);
					if (ik.enabled != enabled) {
						//変更が掛かったなら
						//Undo登録
						Undo.RegisterUndo(ik, "Enabled Change");
						//更新
						ik.enabled = enabled;
						//改変したIKのInspector更新
						EditorUtility.SetDirty(ik);
						
						is_update = true;
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		return is_update;
	}
	
	/// <summary>
	/// シェーダーリストの為のInspector描画
	/// </summary>
	/// <returns>更新が有ったか(true:更新有り, false:未更新)</returns>
	private bool OnInspectorGUIforShaderList()
	{
		MMDEngine self = (MMDEngine)target;
		bool is_update = false;
		
		//シェーダーリストタイトル
		shader_display_ = EditorGUILayout.Foldout(shader_display_, "Shader List");
		//シェーダーリスト内部
		if (shader_display_) {
			//シェーダーリストを表示するなら
			SkinnedMeshRenderer[] renderers = self.GetComponentsInChildren<SkinnedMeshRenderer>();
			Material[] materials = renderers.SelectMany(x=>x.sharedMaterials).ToArray();
			if (1 < renderers.Length) {
				//rendererが複数有る(≒PMX)なら
				//PMXでは名前の先頭にはマテリアルインデックスが有るのでそれを参考にソート
				//PMDではrendererが1つしか無く、かつソート済みの為不要
				System.Array.Sort(materials, (x,y)=>{ 
												string x_name = x.name.Substring(0, x.name.IndexOf('_'));
												string y_name = y.name.Substring(0, y.name.IndexOf('_'));
												int x_int, y_int;
												Int32.TryParse(x_name, out x_int);
												Int32.TryParse(y_name, out y_int);
												return x_int - y_int;
											});
			}
			GUIStyle style = new GUIStyle();
			style.margin.left = 10;
			EditorGUILayout.BeginVertical(style);
			{
				//タイトル
				EditorGUILayout.BeginHorizontal();
				{
					//ラベル
					EditorGUILayout.LabelField("Material", GUILayout.Width(64));
					//シェーダー
					EditorGUILayout.LabelField(new GUIContent("Tr", "Transparent"), GUILayout.Width(20));
					EditorGUILayout.LabelField(new GUIContent("Eg", "Edge"), GUILayout.Width(20));
					EditorGUILayout.LabelField(new GUIContent("Rv", "Reversible"), GUILayout.Width(20));
					EditorGUILayout.LabelField(new GUIContent("Cs", "CastShadow"), GUILayout.Width(20));
#if MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER	//影受け無しのシェーダはまだ無いので無効化
					EditorGUILayout.LabelField(new GUIContent("Rs", "ReceiveShadow"), GUILayout.Width(20));
#endif //MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER
					EditorGUILayout.LabelField("", GUILayout.Width(20));
					EditorGUILayout.LabelField(new GUIContent("Hi", "Highlight"), GUILayout.Width(20));
					EditorGUILayout.LabelField(new GUIContent("Hd", "Hidden"), GUILayout.Width(20));
				}
				EditorGUILayout.EndHorizontal();
				
				//描画用配列
				var parameter_table = new[]{new {flag=ShaderFlag.Transparent, reverse=false}
											, new {flag=ShaderFlag.Outline, reverse=false}
											, new {flag=ShaderFlag.CullBack, reverse=true} //背景カリングON/OFFはわかり辛いので、MMDの様に両面描画ON/OFFで扱う
											, new {flag=ShaderFlag.NoCastShadow, reverse=true} //影放たないON/OFFの2重否定を止めて影放つON/OFFで扱う
#if MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER	//影受け無しのシェーダはまだ無いので無効化
											, new {flag=ShaderFlag.NoReceiveShadow, reverse=true} //影受けないON/OFFの2重否定を止めて影受けるON/OFFで扱う
#endif //MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER
											, new {flag=(ShaderFlag)0, reverse=false}
											, new {flag=ShaderFlag.Highlight, reverse=false}
											, new {flag=ShaderFlag.Hidden, reverse=false}
											};
				//マテリアル
				int material_index = 0;
				foreach (var material in materials) {
					EditorGUILayout.BeginHorizontal();
					{
						//ラベル
						EditorGUILayout.LabelField(new GUIContent((material_index++).ToString(), material.name), GUILayout.Width(64));
						//シェーダー
						if (IsMmdShader(material)) {
							//MMDシェーダーなら
							bool is_change_shader = false;
							ShaderFlag flag = AnalyzeShaderFlag(material);
							foreach (var param in parameter_table) {
								if (0 != param.flag) {
									//描画用
									bool enable_old = 0 != ((int)flag & (int)param.flag);
									if (param.reverse) {
										enable_old = !enable_old;
									}
									bool enable = EditorGUILayout.Toggle(enable_old, GUILayout.Width(20));
									if (enable_old != enable) {
										//更新
										if (param.reverse) {
											enable = !enable;
										}
										flag = (ShaderFlag)((int)flag ^ (int)param.flag);
										
										is_change_shader = true;
									}
								} else {
									//スペース用
									EditorGUILayout.LabelField("", GUILayout.Width(20));
								}
							}
							if (is_change_shader) {
								//変更が掛かったなら
								//Undo登録
								Undo.RegisterUndo(material, "Shader Change");

								SetShader(material, flag);
								is_update = true;
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
		}
		return is_update;
	}

	/// <summary>
	/// MMDシェーダー確認
	/// </summary>
	/// <returns>MMDシェーダーか</returns>
	/// <param name='material'>マテリアル</param>
	static bool IsMmdShader(Material material) {
		bool result = false;
		if (null != material.shader) {
			//シェーダーを持っているなら
			if (0 == material.shader.name.IndexOf("MMD/")) {
				//Mfuシェーダーなら
				if (-1 == material.shader.name.IndexOf("MMD/HalfLambertOutline")) {
					//エッジ付きハーフランバートシェーダーで無いなら
					result = true;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// シェーダーフラグの解析
	/// </summary>
	/// <returns>シェーダーフラグ</returns>
	/// <param name='material'>マテリアル</param>
	static ShaderFlag AnalyzeShaderFlag(Material material) {
		ShaderFlag result = ShaderFlag.MmdShader;
		string name = material.shader.name;
		if (0 == name.IndexOf("MMD/Dummy")) {
			//デバッグ(ハイライト・非表示)シェーダーなら
			//_DummyOriginalShaderTypeパラメータに埋め込んでいるのでそれを使用
			float original_shader_type = 0.0f;
			if (material.HasProperty("_DummyOriginalShaderType")) {
				original_shader_type = material.GetFloat("_DummyOriginalShaderType");
			}
			result = (ShaderFlag)(int)original_shader_type;
			//デバッグカラーからデバッグ内のシェーダ判別
			Color color;
			if (material.HasProperty("_DummyColor")) {
				color = material.GetColor("_DummyColor");
			} else {
				color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
			}
			if (0.0f < color.a) {
				//透過で無いなら
				//ハイライト
				result = result | ShaderFlag.Highlight;
			} else {
				//透過なら
				//非表示
				result = result | ShaderFlag.Hidden;
			}
		} else if (0 == name.IndexOf("MMD/")) {
			//通常シェーダーなら
			if (-1 != name.IndexOf("Transparent/")) {
				//透過なら
				result = (result | ShaderFlag.Transparent);
			}
			if (-1 != name.IndexOf("-Outline")) {
				//エッジ有りなら
				result = (result | ShaderFlag.Outline);
			}
			if (-1 != name.IndexOf("-CullBack")) {
				//背面カリングなら
				result = (result | ShaderFlag.CullBack);
			}
			if (-1 != name.IndexOf("-NoCastShadow")) {
				//影を放たないなら
				result = (result | ShaderFlag.NoCastShadow);
			}
#if MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER	//影受け無しのシェーダはまだ無いので無効化
			if (-1 != name.IndexOf("-NoReceiveShadow")) {
				//影受け無しなら
				result = (result | ShaderFlag.NoReceiveShadow);
			}
#endif //MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER
		} else {
			//MMDシェーダー以外なら
			result = (ShaderFlag)0;
		}
		return result;
	}
	
	/// <summary>
	/// シェーダーフラグからのシェーダー設定
	/// </summary>
	/// <param name='material'>マテリアル</param>
	/// <param name='flag'>シェーダーフラグ</param>
	static void SetShader(Material material, ShaderFlag flag) {
		if (0 != (flag & ShaderFlag.MmdShader)) {
			//Mmdシェーダーなら
			material.shader = CreateShaderFromShaderFlag(flag);
			if (0 != (flag & ShaderFlag.Hidden)) {
				//非表示なら
				float original_shader_type = (float)(int)flag;
				material.SetFloat("_DummyOriginalShaderType", original_shader_type);
				material.SetColor("_DummyColor", new Color(1.0f, 1.0f, 1.0f, 0.0f));
			} else if (0 != (flag & ShaderFlag.Highlight)) {
				//ハイライトなら
				float original_shader_type = (float)(int)flag;
				material.SetFloat("_DummyOriginalShaderType", original_shader_type);
				material.SetColor("_DummyColor", new Color(1.0f, 0.0f, 1.0f, 1.0f));
			}
		}
	}

	/// <summary>
	/// シェーダーフラグからのシェーダー作成
	/// </summary>
	/// <returns>シェーダー</returns>
	/// <param name='flag'>シェーダーフラグ</param>
	static Shader CreateShaderFromShaderFlag(ShaderFlag flag) {
		Shader result;
		string path = "MMD/";
		if (0 != (flag & ShaderFlag.Transparent)) {
			path += "Transparent/";
		}
		path += "PMDMaterial";
		if (0 != (flag & ShaderFlag.Outline)) {
			path += "-with-Outline";
		}
		if (0 != (flag & ShaderFlag.CullBack)) {
			path += "-CullBack";
		}
		if (0 != (flag & ShaderFlag.NoCastShadow)) {
			path += "-NoCastShadow";
		}
#if MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER	//影受け無しのシェーダはまだ無いので無効化
		if (0 != (flag & ShaderFlag.NoReceiveShadow)) {
			path += "-NoReceiveShadow";
		}
#endif //MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER
		result = Shader.Find(path);
		
		//デバッグ系
		if (0 != (flag & ShaderFlag.Highlight)) {
			//ハイライト
			string original_shader_name = result.name;
			result = Shader.Find("MMD/Dummy");
			result.name = original_shader_name + "+Highlight";
		} else if (0 != (flag & ShaderFlag.Hidden)) {
			//非表示
			string original_shader_name = result.name;
			result = Shader.Find("MMD/Dummy");
			result.name = original_shader_name + "+Hidden";
		}
		
		return result;
	}
	
	[Flags]
	private enum ShaderFlag {
		MmdShader		= 1<< 0,	//MMDシェーダー
		Transparent		= 1<< 1,	//透過
		Outline			= 1<< 2,	//エッジ有り
		CullBack		= 1<< 3,	//背面カリング
		NoCastShadow	= 1<< 4,	//影放ち無し
		NoReceiveShadow	= 1<< 5,	//影受け無し
		Highlight		= 1<< 6,	//ハイライト
		Hidden			= 1<< 7,	//非表示
	}

	private static	bool	ik_list_display_;	//IKリストの表示
	private static	bool	shader_display_;	//シェーダーリストの表示
}
