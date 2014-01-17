using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MMD.PMD;

namespace MMD
{
	public class PMDConverter
	{
		/// <summary>
		/// シェーダの種類
		/// </summary>
		public enum ShaderType
		{
			Default,		/// Unityのデフォルトシェーダ
			HalfLambert,	/// もやっとしたLambertっぽくなる
			MMDShader		/// MMDっぽいシェーダ
		}
		
		/// <summary>
		/// GameObjectを作成する
		/// </summary>
		/// <param name='format'>内部形式データ</param>
		/// <param name='shader_type'>シェーダーの種類</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='use_mecanim'>Mecanimを使用するか</param>
		/// <param name='use_ik'>IKを使用するか</param>
		/// <param name='scale'>スケール</param>
		public static GameObject CreateGameObject(PMDFormat format, ShaderType shader_type, bool use_rigidbody, bool use_mecanim, bool use_ik, float scale) {
			PMDConverter converter = new PMDConverter();
			return converter.CreateGameObject_(format, shader_type, use_rigidbody, use_mecanim, use_ik, scale);
		}

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		/// <remarks>
		/// ユーザーに依るインスタンス作成を禁止する
		/// </remarks>
		private PMDConverter() {}

		private GameObject CreateGameObject_(PMDFormat format, ShaderType shader_type, bool use_rigidbody, bool use_mecanim, bool use_ik, float scale) {
			format_ = format;
			shader_type_ = shader_type;
			use_rigidbody_ = use_rigidbody;
			use_mecanim_ = use_mecanim;
			use_ik_ = use_ik;
			scale_ = scale;
			root_game_object_ = new GameObject(format_.name);
		
			Mesh mesh = CreateMesh();					// メッシュの生成・設定
			Material[] materials = CreateMaterials();	// マテリアルの生成・設定
			GameObject[] bones = CreateBones();			// ボーンの生成・設定
	
			// バインドポーズの作成
			BuildingBindpose(mesh, materials, bones);
	
			MMDEngine engine = root_game_object_.AddComponent<MMDEngine>();
			//スケール・エッジ幅
			engine.scale = scale_;
			engine.outline_width = default_outline_width;
			engine.material_outline_widths = Enumerable.Repeat(1.0f, materials.Length).ToArray();
	
			// IKの登録
			if (use_ik_)
				engine.ik_list = EntryIKSolver(bones);
	
			// 剛体関連
			if (use_rigidbody_)
			{
				try
				{
					var rigids = CreateRigids(bones);
					AssignRigidbodyToBone(bones, rigids);
					SetRigidsSettings(bones, rigids);
					GameObject[] joints = SettingJointComponent(bones, rigids);
					GlobalizeRigidbody(joints);
	
					// 非衝突グループ
					List<int>[] ignoreGroups = SettingIgnoreRigidGroups(rigids);
					int[] groupTarget = GetRigidbodyGroupTargets(rigids);
	
					MMDEngine.Initialize(engine, groupTarget, ignoreGroups, rigids);
				}
				catch { }
			}
	
			// Mecanim設定
			if (use_mecanim_) {
				AvatarSettingScript avatar_setting = new AvatarSettingScript(root_game_object_, bones);
				avatar_setting.SettingHumanAvatar();
				
				string path = format_.folder + "/";
				string name = GetFilePathString(format_.name);
				string file_name = path + name + ".avatar.asset";
				avatar_setting.CreateAsset(file_name);
			} else {
				root_game_object_.AddComponent<Animation>();	// アニメーション追加
			}

			return root_game_object_;
		}

		Vector3[] EntryVertices()
		{
			int vcount = (int)format_.vertex_list.vert_count;
			Vector3[] vpos = new Vector3[vcount];
			for (int i = 0; i < vcount; i++)
				vpos[i] = format_.vertex_list.vertex[i].pos * scale_;
			return vpos;
		}
		
		Vector3[] EntryNormals()
		{
			int vcount = (int)format_.vertex_list.vert_count;
			Vector3[] normals = new Vector3[vcount];
			for (int i = 0; i < vcount; i++)
				normals[i] = format_.vertex_list.vertex[i].normal_vec;
			return normals;
		}
		
		Vector2[] EntryUVs()
		{
			int vcount = (int)format_.vertex_list.vert_count;
			Vector2[] uvs = new Vector2[vcount];
			for (int i = 0; i < vcount; i++)
				uvs[i] = format_.vertex_list.vertex[i].uv;
			return uvs;
		}
		
		BoneWeight[] EntryBoneWeights()
		{
			int vcount = (int)format_.vertex_list.vert_count;
			BoneWeight[] weights = new BoneWeight[vcount];
			for (int i = 0; i < vcount; i++)
			{
				weights[i].boneIndex0 = (int)format_.vertex_list.vertex[i].bone_num[0];
				weights[i].boneIndex1 = (int)format_.vertex_list.vertex[i].bone_num[1];
				weights[i].weight0 = (float)format_.vertex_list.vertex[i].bone_weight / 100.0f;
				weights[i].weight1 = 1.0f - weights[i].weight0;
			}
			return weights;
		}
		
		// 頂点座標やUVなどの登録だけ
		void EntryAttributesForMesh(Mesh mesh)
		{
			//mesh.vertexCount = (int)format_.vertex_list.vert_count;
			mesh.vertices = EntryVertices();
			mesh.normals = EntryNormals();
			mesh.uv = EntryUVs();
			mesh.boneWeights = EntryBoneWeights();
		}
		
		void SetSubMesh(Mesh mesh)
		{
			// マテリアル対サブメッシュ
			// サブメッシュとはマテリアルに適用したい面頂点データのこと
			// 面ごとに設定するマテリアルはここ
			mesh.subMeshCount = (int)format_.material_list.material_count;
			
			int sum = 0;
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				int count = (int)format_.material_list.material[i].face_vert_count;
				int[] indices = new int[count];
				
				// 面頂点は材質0から順番に加算されている
				for (int j = 0; j < count; j++)
					indices[j] = format_.face_vertex_list.face_vert_index[j+sum];
				mesh.SetTriangles(indices, i);
				sum += (int)format_.material_list.material[i].face_vert_count;
			}
		}
		
		// メッシュをProjectに登録
		void CreateAssetForMesh(Mesh mesh)
		{
			AssetDatabase.CreateAsset(mesh, format_.folder + "/" + format_.name + ".asset");
		}
		
		Mesh CreateMesh()
		{
			Mesh mesh = new Mesh();
			EntryAttributesForMesh(mesh);
			SetSubMesh(mesh);
			CreateAssetForMesh(mesh);
			return mesh;
		}
		
		//透過マテリアル確認(true:透過, false:不透明)
		bool IsTransparentMaterial(PMD.PMDFormat.Material model_material, Texture2D texture) {
			bool result = false;
			result = result || (model_material.alpha < 0.98f); //0.98f以上は不透明と見做す(0.98fに影生成情報を埋め込んでいる為)
			if (null != texture) {
				result = result || texture.alphaIsTransparency;
			}
			return result;
		}
		
		//エッジマテリアル確認(true:エッジ有り, false:無エッジ)
		bool IsEdgeMaterial(PMD.PMDFormat.Material model_material) {
			bool result;
			if (0 == model_material.edge_flag) {
				//エッジ無し
				result = false;
			} else {
				//エッジ有りなら
				result = true;
			}
			return result;
		}
		
		//背面カリングマテリアル確認(true:背面カリングする, false:背面カリングしない)
		bool IsCullBackMaterial(PMD.PMDFormat.Material model_material) {
			bool result;
			if (1.0f <= model_material.alpha) {
				//不透明なら
				//背面カリングする
				result = true;
			} else if (0.99f <= model_material.alpha) {
				//不透明の両面描画なら
				//背面カリングしない
				result = false;
			} else {
				//透過なら
				//背面カリングしない
				result = false;
			}
			return result;
		}
		
		//無影マテリアル確認(true:無影, false:影放ち)
		bool IsNoCastShadowMaterial(PMD.PMDFormat.Material model_material) {
			bool result;
			if (0 == model_material.edge_flag) {
				//エッジ無し
				//無影
				result = true;
			} else {
				//エッジ有りなら
				//影放ち
				result = false;
			}
			return result;
		}
		
		//影受け無しマテリアル確認(true:影受け無し, false:影受け)
		bool IsNoReceiveShadowMaterial(PMD.PMDFormat.Material model_material) {
			bool result;
			if (0.98f == model_material.alpha) { //浮動小数点の比較だけど、0.98fとの同値確認でPMXエディタの0.98と一致したので一旦これで。
				//影受け無し(不透明度が0.98fは特別扱いで影受け無し)なら
				result = true;
			} else {
				//影受け有りなら
				result = false;
			}
			return result;
		}
		
		string GetMMDShaderPath(PMD.PMDFormat.Material model_material, Texture2D texture) {
			string result = "MMD/";
			if (IsTransparentMaterial(model_material, texture)) {
				result += "Transparent/";
			}
			result += "PMDMaterial";
			if (IsEdgeMaterial(model_material)) {
				result += "-with-Outline";
			}
			if (IsCullBackMaterial(model_material)) {
				result += "-CullBack";
			}
			if (IsNoCastShadowMaterial(model_material)) {
				result += "-NoCastShadow";
			}
#if MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER	//影受け無しのシェーダはまだ無いので無効化
			if (IsNoReceiveShadowMaterial(model_material)) {
				result += "-NoReceiveShadow";
			}
#endif //MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER
			return result;
		}

		// 色の生成
		void EntryColors(Material[] mats)
		{
			// マテリアルの生成 
			for (int i = 0; i < mats.Length; i++)
			{
				// PMDフォーマットのマテリアルを取得 
				PMD.PMDFormat.Material pmdMat = format_.material_list.material[i];
				
				//先にテクスチャ情報を検索
				Texture2D main_texture = null;
				if (pmdMat.texture_file_name != "") {
					string path = format_.folder + "/" + pmdMat.texture_file_name;
					main_texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
				}
				
				//マテリアルに設定
				switch (shader_type_)
				{
					case ShaderType.Default:	// デフォルト
						mats[i] = new Material(Shader.Find("Transparent/Diffuse"));
						mats[i].color = pmdMat.diffuse_color;
						Color cbuf = mats[i].color;
						cbuf.a = pmdMat.alpha;	// これでいいのか？
						mats[i].color = cbuf;
						break;

					case ShaderType.HalfLambert:	// ハーフランバート
						mats[i] = new Material(Shader.Find("Custom/CharModel"));
						mats[i].SetFloat("_Cutoff", 1 - pmdMat.alpha);
						mats[i].color = pmdMat.diffuse_color;
						break;

					case ShaderType.MMDShader:
						mats[i] = new Material(Shader.Find(GetMMDShaderPath(pmdMat, main_texture)));
					
						// シェーダに依って値が有ったり無かったりするが、設定してもエラーに為らない様なので全部設定
						mats[i].SetColor("_Color", pmdMat.diffuse_color);
						mats[i].SetColor("_AmbColor", pmdMat.mirror_color);
						mats[i].SetFloat("_Opacity", pmdMat.alpha);
						mats[i].SetColor("_SpecularColor", pmdMat.specular_color);
						mats[i].SetFloat("_Shininess", pmdMat.specularity);
						// エッジ
						const float c_default_scale = 0.085f; //0.085fの時にMMDと一致する様にしているので、それ以外なら補正
						mats[i].SetFloat("_OutlineWidth", default_outline_width * scale_ / c_default_scale);
						mats[i].SetColor("_OutlineColor", default_outline_color);
					
						// ここでスフィアマップ
						string path = format_.folder + "/" + pmdMat.sphere_map_name;
						Texture sphere_map;

						if (System.IO.File.Exists(path))
						{	//　ファイルの存在を確認
							sphere_map = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
							
							// 乗算と加算判定
							string ext = System.IO.Path.GetExtension(pmdMat.sphere_map_name);
							switch (ext) {
							case ".spa": // 加算
								mats[i].SetTexture("_SphereAddTex", sphere_map);
								mats[i].SetTextureScale("_SphereAddTex", new Vector2(1, -1));
								break;
							case ".sph": // 乗算
								mats[i].SetTexture("_SphereMulTex", sphere_map);
								mats[i].SetTextureScale("_SphereMulTex", new Vector2(1, -1));
								break;
							default:
								// 加算扱い
								goto case ".spa";
							}
						}

						// トゥーンの位置を取得
						string toon_name = pmdMat.toon_index != 0xFF ?
							format_.toon_texture_list.toon_texture_file[pmdMat.toon_index] : "toon00.bmp";
						string resource_path = UnityEditor.AssetDatabase.GetAssetPath(Shader.Find("MMD/HalfLambertOutline"));
						resource_path = System.IO.Path.GetDirectoryName(resource_path);	// resourceディレクトリを取得
						resource_path += "/toon/" + toon_name;

						// トゥーンが存在するか確認
						if (!System.IO.File.Exists(resource_path))
						{
							// 自前トゥーンの可能性がある
							resource_path = format_.folder + "/" + format_.toon_texture_list.toon_texture_file[pmdMat.toon_index];
							if (!System.IO.File.Exists(resource_path))
							{
								Debug.LogError("Do not exists toon texture: " + format_.toon_texture_list.toon_texture_file[pmdMat.toon_index]);
								break;
							}
						}

						// テクスチャの割り当て
						Texture toon_tex = UnityEditor.AssetDatabase.LoadAssetAtPath(resource_path, typeof(Texture)) as Texture;
						mats[i].SetTexture("_ToonTex", toon_tex);
						mats[i].SetTextureScale("_ToonTex", new Vector2(1, -1));
						break;
				}

				// テクスチャが空でなければ登録
				if (null != main_texture) {
					mats[i].mainTexture = main_texture;
					mats[i].mainTextureScale = new Vector2(1, -1);
				}
			}
		}
		
		// マテリアルに必要な色などを登録
		Material[] EntryAttributesForMaterials()
		{
			int count = (int)format_.material_list.material_count;
			Material[] mats = new Material[count];
			EntryColors(mats);
			return mats;
		}
		
		// マテリアルの登録
		void CreateAssetForMaterials(Material[] mats)
		{
			// 適当なフォルダに投げる
			string path = format_.folder + "/Materials/";
			if (!System.IO.Directory.Exists(path)) { 
				AssetDatabase.CreateFolder(format_.folder, "Materials");
			}
			
			for (int i = 0; i < mats.Length; i++)
			{
				string fname = path + format_.name + "_material" + i + ".asset";
				AssetDatabase.CreateAsset(mats[i], fname);
			}
		}
		
		// マテリアルの生成
		Material[] CreateMaterials()
		{
			Material[] materials;
			materials = EntryAttributesForMaterials();
			CreateAssetForMaterials(materials);
			return materials;
		}

		// 親子関係の構築
		void AttachParentsForBone(GameObject[] bones)
		{
			for (int i = 0; i < bones.Length; i++)
			{
				int index = format_.bone_list.bone[i].parent_bone_index;
				if (index != 0xFFFF)
					bones[i].transform.parent = bones[index].transform;
				else
					bones[i].transform.parent = root_game_object_.transform;
			}
		}

		// ボーンの位置決めや親子関係の整備など
		GameObject[] EntryAttributeForBones()
		{
			int count = format_.bone_list.bone_count;
			GameObject[] bones = new GameObject[count];
			
			for (int i = 0; i < count; i++) {
				bones[i] = new GameObject(format_.bone_list.bone[i].bone_name);
				bones[i].transform.name = bones[i].name;
				bones[i].transform.position = format_.bone_list.bone[i].bone_head_pos * scale_;
			}
			return bones;
		}
		
		// ボーンの生成
		GameObject[] CreateBones()
		{
			GameObject[] bones;
			bones = EntryAttributeForBones();
			AttachParentsForBone(bones);
			CreateSkinBone(bones);
			return bones;
		}

		// 表情ボーンの生成を行う
		void CreateSkinBone(GameObject[] bones)
		{
			// 表情ルートを生成してルートの子供に付ける
			GameObject skin_root = new GameObject("Expression");
			if (skin_root.GetComponent<ExpressionManagerScript>() == null)
				skin_root.AddComponent<ExpressionManagerScript>();
			skin_root.transform.parent = root_game_object_.transform;
			
			for (int i = 0; i < format_.skin_list.skin_count; i++)
			{
				// 表情を親ボーンに付ける
				GameObject skin = new GameObject(format_.skin_list.skin_data[i].skin_name);
				skin.transform.parent = skin_root.transform;
				var script = skin.AddComponent<MMDSkinsScript>();

				// モーフの情報を入れる
				AssignMorphVectorsForSkin(format_.skin_list.skin_data[i], format_.vertex_list, script);
			}
		}

		// モーフ情報（頂点インデックス、モーフ先頂点など）を記録する
		void AssignMorphVectorsForSkin(PMD.PMDFormat.SkinData data, PMD.PMDFormat.VertexList vtxs, MMDSkinsScript script)
		{
			uint count = data.skin_vert_count;
			int[] indices = new int[count];
			Vector3[] morph_target = new Vector3[count];

			for (int i = 0; i < count; i++)
			{
				// ここで設定する
				indices[i] = (int)data.skin_vert_data[i].skin_vert_index;

				// モーフ先 - 元頂点
				//morph_target[i] = (data.skin_vert_data[i].skin_vert_pos - vtxs.vertex[indices[i]].pos).normalized;
				//morph_target[i] = data.skin_vert_data[i].skin_vert_pos - vtxs.vertex[indices[i]].pos;
				morph_target[i] = data.skin_vert_data[i].skin_vert_pos * scale_;
			}

			// スクリプトに記憶させる
			script.morphTarget = morph_target;
			script.targetIndices = indices;

			switch (data.skin_type)
			{
				case 0:
					script.skinType = MMDSkinsScript.SkinType.Base;
					script.gameObject.name = "base";
					break;

				case 1:
					script.skinType = MMDSkinsScript.SkinType.EyeBrow;
					break;

				case 2:
					script.skinType = MMDSkinsScript.SkinType.Eye;
					break;

				case 3:
					script.skinType = MMDSkinsScript.SkinType.Lip;
					break;

				case 4:
					script.skinType = MMDSkinsScript.SkinType.Other;
					break;
			}
		}

		// バインドポーズの作成
		void BuildingBindpose(Mesh mesh, Material[] materials, GameObject[] bones)
		{
			// 行列とかトランスフォームとか
			Matrix4x4[] bindpose = new Matrix4x4[bones.Length];
			Transform[] trans = new Transform[bones.Length];
			for (int i = 0; i < bones.Length; i++) {
				trans[i] = bones[i].transform;
				bindpose[i] = bones[i].transform.worldToLocalMatrix;
			}
			
			// ここで本格的な適用
			SkinnedMeshRenderer smr = root_game_object_.AddComponent<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
			mesh.bindposes = bindpose;
			smr.sharedMesh = mesh;
			smr.bones = trans;
			smr.materials = materials;
			smr.receiveShadows = false; //影を受けない
			ExpressionManagerScript ems = root_game_object_.GetComponentInChildren<ExpressionManagerScript>();
			ems.mesh = mesh;
		}
		
		// IKの登録
		//   IKは基本的にスクリプトを利用
		CCDIKSolver[] EntryIKSolver(GameObject[] bones)
		{
			PMD.PMDFormat.IKList ik_list = format_.ik_list;

			CCDIKSolver[] iksolvers = new CCDIKSolver[ik_list.ik_data_count];
			for (int i = 0; i < ik_list.ik_data_count; i++)
			{
				PMD.PMDFormat.IK ik = ik_list.ik_data[i];

				bones[ik.ik_bone_index].AddComponent<CCDIKSolver>();
				CCDIKSolver solver = bones[ik.ik_bone_index].GetComponent<CCDIKSolver>();
				solver.target = bones[ik.ik_target_bone_index].transform;
				solver.controll_weight = ik.control_weight * 4; // PMDファイルは4倍らしい
				solver.iterations = ik.iterations;
				solver.chains = new Transform[ik.ik_chain_length];
				for (int j = 0; j < ik.ik_chain_length; j++)
					solver.chains[j] = bones[ik.ik_child_bone_index[j]].transform;

				if (!(bones[ik.ik_bone_index].name.Contains("足") || bones[ik.ik_bone_index].name.Contains("つま先")))
				{
					solver.enabled = false;
				}
				iksolvers[i] = solver;
			}

			return iksolvers;
		}

		// Sphere Colliderの設定
		Collider EntrySphereCollider(PMDFormat.Rigidbody rigid, GameObject obj)
		{
			SphereCollider collider = obj.AddComponent<SphereCollider>();
			collider.radius = rigid.shape_w * scale_;
			return collider;
		}

		// Box Colliderの設定
		Collider EntryBoxCollider(PMDFormat.Rigidbody rigid, GameObject obj)
		{
			BoxCollider collider = obj.AddComponent<BoxCollider>();
			collider.size = new Vector3(
				rigid.shape_w * 2f * scale_,
				rigid.shape_h * 2f * scale_, 
				rigid.shape_d * 2f * scale_);
			return collider;
		}

		// Capsule Colliderの設定
		Collider EntryCapsuleCollider(PMDFormat.Rigidbody rigid, GameObject obj)
		{
			CapsuleCollider collider = obj.AddComponent<CapsuleCollider>();
			collider.radius = rigid.shape_w * scale_;
			collider.height = (rigid.shape_h + rigid.shape_w * 2) * scale_;
			return collider;
		}

		// 物理素材の定義
		PhysicMaterial CreatePhysicMaterial(PMDFormat.Rigidbody rigid)
		{
			PhysicMaterial material = new PhysicMaterial(format_.name + "_r" + rigid.rigidbody_name);
			material.bounciness = rigid.rigidbody_recoil;
			material.staticFriction = rigid.rigidbody_friction;
			material.dynamicFriction = rigid.rigidbody_friction;

			AssetDatabase.CreateAsset(material, format_.folder + "/Physics/" + GetFilePathString(material.name) + ".asset");
			return material;
		}

		// Unity側のRigidbodyの設定を行う
		void UnityRigidbodySetting(PMDFormat.Rigidbody rigidbody, GameObject targetBone, bool setted=false)
		{
			// rigidbodyの調整
			if (!setted)
			{
				targetBone.rigidbody.isKinematic = (0 == rigidbody.rigidbody_type);
				targetBone.rigidbody.mass = Mathf.Max(float.Epsilon, rigidbody.rigidbody_weight);
				targetBone.rigidbody.drag = rigidbody.rigidbody_pos_dim;
				targetBone.rigidbody.angularDrag = rigidbody.rigidbody_rot_dim;
			}
			else
			{
				// Rigidbodyはボーンに対して適用されるので複数ある場合は平均を取る
				targetBone.rigidbody.mass += rigidbody.rigidbody_weight;
				targetBone.rigidbody.drag += rigidbody.rigidbody_pos_dim;
				targetBone.rigidbody.angularDrag += rigidbody.rigidbody_rot_dim;
				targetBone.rigidbody.mass *= 0.5f;
				targetBone.rigidbody.drag *= 0.5f;
				targetBone.rigidbody.angularDrag *= 0.5f;
			}
		}

		// 剛体の値を代入する
		void SetRigidsSettings(GameObject[] bones, GameObject[] rigid)
		{
			PMDFormat.RigidbodyList list = format_.rigidbody_list;
			for (int i = 0; i < list.rigidbody_count; i++)	// iは剛体番号
			{
				// 剛体の関連ボーンのインデックス
				int rigidRefIndex = list.rigidbody[i].rigidbody_rel_bone_index;

				// ローカル座標の確定
				Vector3 localPos = list.rigidbody[i].pos_pos * scale_;// - rigid[i].transform.position;

				// ここで位置の決定
				if (rigidRefIndex >= ushort.MaxValue)
				{
					// 関連ボーン無し
					if (rigid[i].rigidbody == null)
						rigid[i].AddComponent<Rigidbody>();
					UnityRigidbodySetting(list.rigidbody[i], rigid[i]);
					rigid[i].transform.localPosition = localPos;
					
					// 関連ボーンなしの剛体はセンターボーンに接続している
					rigid[i].transform.position = localPos + format_.bone_list.bone[0].bone_head_pos * scale_;
					// 回転の値を決める
					Vector3 rot = list.rigidbody[i].pos_rot * Mathf.Rad2Deg;
					rigid[i].transform.rotation = Quaternion.Euler(rot);
				}
				else
					// 関連ボーン有り
				{	// とりあえずここで剛体を追加・設定
					if (bones[rigidRefIndex].rigidbody == null)
						bones[rigidRefIndex].AddComponent<Rigidbody>();
					UnityRigidbodySetting(list.rigidbody[i], bones[rigidRefIndex]);
					rigid[i].transform.localPosition = localPos;
					// 回転の値を決める
					Vector3 rot = list.rigidbody[i].pos_rot * Mathf.Rad2Deg;
					rigid[i].transform.rotation = Quaternion.Euler(rot);
				}
				
			}
		}

		// 剛体の生成
		GameObject[] CreateRigids(GameObject[] bones)
		{
			PMDFormat.RigidbodyList list = format_.rigidbody_list;
			if (!System.IO.Directory.Exists(System.IO.Path.Combine(format_.folder, "Physics")))
			{
				AssetDatabase.CreateFolder(format_.folder, "Physics");
			}
			
			// 剛体の登録 
			GameObject[] rigid = new GameObject[list.rigidbody_count];
			for (int i = 0; i < list.rigidbody_count; i++)
			{
				rigid[i] = new GameObject("r" + list.rigidbody[i].rigidbody_name);
				//rigid[i].AddComponent<Rigidbody>();		// 剛体本体にはrigidbodyは適用しない

				// 各種Colliderの設定
				Collider collider = null;
				switch (list.rigidbody[i].shape_type)
				{
					case 0:
						collider = EntrySphereCollider(list.rigidbody[i], rigid[i]);
						break;

					case 1:
						collider = EntryBoxCollider(list.rigidbody[i], rigid[i]);
						break;

					case 2:
						collider = EntryCapsuleCollider(list.rigidbody[i], rigid[i]);
						break;
				}

				// マテリアルの設定
				collider.material = CreatePhysicMaterial(list.rigidbody[i]);
			}
			return rigid;
		}

		// ジョインに依って接続している(されている)剛体番号を検索する
		int SearchConnectRigidByJoint(int rigidIndex)
		{
			for (int i = 0; i < format_.rigidbody_joint_list.joint_count; i++)
			{
				int joint_rigidbody_a = (int)format_.rigidbody_joint_list.joint[i].joint_rigidbody_a;
				int joint_rigidbody_b = (int)format_.rigidbody_joint_list.joint[i].joint_rigidbody_b;
				if (joint_rigidbody_b == rigidIndex)
				{
					return joint_rigidbody_a;
				}
				else if (joint_rigidbody_a == rigidIndex)
				{
					return joint_rigidbody_b;
				}
			}
			// 接続剛体は発見出来ず
			return -1;
		}

		// 関連ボーンなしの剛体から親のボーンを探してくる
		// rigidIndexは剛体番号
		int GetTargetRigidBone(int rigidIndex)
		{
			// 接続剛体Aを探す
			int targetRigid = SearchConnectRigidByJoint(rigidIndex);

			// 接続剛体Aの関連ボーンを探す
			int ind = format_.rigidbody_list.rigidbody[targetRigid].rigidbody_rel_bone_index;
			
			// MaxValueを引けば接続剛体Aの関連ボーンに接続されるようになっている
			if (ind >= ushort.MaxValue)
				format_.rigidbody_list.rigidbody[rigidIndex].rigidbody_rel_bone_index = ushort.MaxValue + (ushort)ind;
			
			return (int)ind;
		}

		// 剛体ボーンを
		void AssignRigidbodyToBone(GameObject[] bones, GameObject[] rigids)
		{
			// 剛体の数だけ回す
			for (int i = 0; i < rigids.Length; i++)
			{
				// 剛体を親ボーンに格納
				int refIndex = format_.rigidbody_list.rigidbody[i].rigidbody_rel_bone_index;
				if (refIndex != ushort.MaxValue)
				{	// 65535が最大値
					rigids[i].transform.parent = bones[refIndex].transform;
				}
				else
				{
					// ジョイントから接続剛体B＝現在の剛体名で探し出す
					int boneIndex = GetTargetRigidBone(i);

					// 接続剛体Aの関連ボーンに剛体を接続
					rigids[i].transform.parent = bones[boneIndex].transform;
				}
			}
		}

		// 移動や回転制限
		void SetMotionAngularLock(PMDFormat.Joint joint, ConfigurableJoint conf)
		{
			SoftJointLimit jlim;

			// Motionの固定
			if (joint.constrain_pos_1.x == 0f && joint.constrain_pos_2.x == 0f)
				conf.xMotion = ConfigurableJointMotion.Locked;
			else
				conf.xMotion = ConfigurableJointMotion.Limited;

			if (joint.constrain_pos_1.y == 0f && joint.constrain_pos_2.y == 0f)
				conf.yMotion = ConfigurableJointMotion.Locked;
			else
				conf.yMotion = ConfigurableJointMotion.Limited;

			if (joint.constrain_pos_1.z == 0f && joint.constrain_pos_2.z == 0f)
				conf.zMotion = ConfigurableJointMotion.Locked;
			else
				conf.zMotion = ConfigurableJointMotion.Limited;

			// 角度の固定
			if (joint.constrain_rot_1.x == 0f && joint.constrain_rot_2.x == 0f)
				conf.angularXMotion = ConfigurableJointMotion.Locked;
			else
			{
				conf.angularXMotion = ConfigurableJointMotion.Limited;
				float hlim = Mathf.Max(-joint.constrain_rot_1.x, -joint.constrain_rot_2.x); //回転方向が逆なので負数
				float llim = Mathf.Min(-joint.constrain_rot_1.x, -joint.constrain_rot_2.x);
				SoftJointLimit jhlim = new SoftJointLimit();
				jhlim.limit = Mathf.Clamp(hlim * Mathf.Rad2Deg, -180.0f, 180.0f);
				conf.highAngularXLimit = jhlim;

				SoftJointLimit jllim = new SoftJointLimit();
				jllim.limit = Mathf.Clamp(llim * Mathf.Rad2Deg, -180.0f, 180.0f);
				conf.lowAngularXLimit = jllim;
			}

			if (joint.constrain_rot_1.y == 0f && joint.constrain_rot_2.y == 0f)
				conf.angularYMotion = ConfigurableJointMotion.Locked;
			else
			{
				// 値がマイナスだとエラーが出るので注意
				conf.angularYMotion = ConfigurableJointMotion.Limited;
				float lim = Mathf.Min(Mathf.Abs(joint.constrain_rot_1.y), Mathf.Abs(joint.constrain_rot_2.y));//絶対値の小さい方
				jlim = new SoftJointLimit();
				jlim.limit = lim * Mathf.Clamp(Mathf.Rad2Deg, 0.0f, 180.0f);
				conf.angularYLimit = jlim;
			}

			if (joint.constrain_rot_1.z == 0f && joint.constrain_rot_2.z == 0f)
				conf.angularZMotion = ConfigurableJointMotion.Locked;
			else
			{
				conf.angularZMotion = ConfigurableJointMotion.Limited;
				float lim = Mathf.Min(Mathf.Abs(-joint.constrain_rot_1.z), Mathf.Abs(-joint.constrain_rot_2.z));//絶対値の小さい方//回転方向が逆なので負数
				jlim = new SoftJointLimit();
				jlim.limit = Mathf.Clamp(lim * Mathf.Rad2Deg, 0.0f, 180.0f);
				conf.angularZLimit = jlim;
			}
		}

		// ばねの設定など
		void SetDrive(PMDFormat.Joint joint, ConfigurableJoint conf)
		{
			JointDrive drive;

			// Position
			if (joint.spring_pos.x != 0f)
			{
				drive = new JointDrive();
				drive.positionSpring = joint.spring_pos.x * scale_;
				conf.xDrive = drive;
			}
			if (joint.spring_pos.y != 0f)
			{
				drive = new JointDrive();
				drive.positionSpring = joint.spring_pos.y * scale_;
				conf.yDrive = drive;
			}
			if (joint.spring_pos.z != 0f)
			{
				drive = new JointDrive();
				drive.positionSpring = joint.spring_pos.z * scale_;
				conf.zDrive = drive;
			}

			// Angular
			if (joint.spring_rot.x != 0f)
			{
				drive = new JointDrive();
				drive.mode = JointDriveMode.PositionAndVelocity;
				drive.positionSpring = joint.spring_rot.x;
				conf.angularXDrive = drive;
			}
			if (joint.spring_rot.y != 0f || joint.spring_rot.z != 0f)
			{
				drive = new JointDrive();
				drive.mode = JointDriveMode.PositionAndVelocity;
				drive.positionSpring = (joint.spring_rot.y + joint.spring_rot.z) * 0.5f;
				conf.angularYZDrive = drive;
			}
		}

		// ConfigurableJointの値を設定する
		void SetAttributeConfigurableJoint(PMDFormat.Joint joint, ConfigurableJoint conf)
		{
			SetMotionAngularLock(joint, conf);
			SetDrive(joint, conf);
		}

		// ConfigurableJointの設定
		// 先に設定してからFixedJointを設定する
		GameObject[] SetupConfigurableJoint(GameObject[] rigids)
		{
			List<GameObject> result_list = new List<GameObject>();
			foreach (PMDFormat.Joint joint in format_.rigidbody_joint_list.joint) {
				//相互接続する剛体の取得
				Transform transform_a = rigids[joint.joint_rigidbody_a].transform;
				Transform transform_b = rigids[joint.joint_rigidbody_b].transform;
				Rigidbody rigidbody_a = transform_a.rigidbody;
				if (null == rigidbody_a) {
					rigidbody_a = transform_a.parent.rigidbody;
				}
				Rigidbody rigidbody_b = transform_b.rigidbody;
				if (null == rigidbody_b) {
					rigidbody_b = transform_b.parent.rigidbody;
				}
				if (rigidbody_a != rigidbody_b) {
					//接続する剛体が同じ剛体を指さないなら
					//(本来ならPMDの設定が間違っていない限り同一を指す事は無いが、MFUでは関連ボーンを持たない剛体はセンターボーンに纏められる為に依り起こり得る)
					//ジョイント設定
					ConfigurableJoint config_joint = rigidbody_b.gameObject.AddComponent<ConfigurableJoint>();
					config_joint.connectedBody = rigidbody_a;
					SetAttributeConfigurableJoint(joint, config_joint);
					
					result_list.Add(config_joint.gameObject);
				}
			}
			return result_list.ToArray();
		}

		// ジョイントの設定
		// ジョイントはボーンに対して適用される
		GameObject[] SettingJointComponent(GameObject[] bones, GameObject[] rigids)
		{
			// ConfigurableJointの設定
			GameObject[] joints = SetupConfigurableJoint(rigids);
			return joints;
		}

		// 剛体のグローバル座標化
		void GlobalizeRigidbody(GameObject[] joints)
		{
			if ((null != joints) && (0 < joints.Length)) {
				// 物理演算ルートを生成してルートの子供に付ける
				GameObject physics_root = new GameObject("Physics");
				PhysicsManager physics_manager = physics_root.AddComponent<PhysicsManager>();
				physics_root.transform.parent = root_game_object_.transform;
				Transform physics_root_transform = physics_root.transform;
				
				// PhysicsManagerに移動前の状態を覚えさせる(幾つか重複しているので重複は削除)
				physics_manager.connect_bone_list = joints.Select(x=>x.gameObject)
															.Distinct()
															.Select(x=>new PhysicsManager.ConnectBone(x, x.transform.parent.gameObject))
															.ToArray();
				
				//isKinematicで無くConfigurableJointを持つ場合はグローバル座標化
				foreach (ConfigurableJoint joint in joints.Where(x=>!x.GetComponent<Rigidbody>().isKinematic)
															.Select(x=>x.GetComponent<ConfigurableJoint>())) {
					joint.transform.parent = physics_root_transform;
				}
			}
		}

		// 非衝突剛体の設定
		List<int>[] SettingIgnoreRigidGroups(GameObject[] rigids)
		{
			// 非衝突グループ用リストの初期化
			const int MaxGroup = 16;	// グループの最大数
			List<int>[] ignoreRigid = new List<int>[MaxGroup];
			for (int i = 0; i < 16; i++) ignoreRigid[i] = new List<int>();

			// それぞれの剛体が所属している非衝突グループを追加していく
			PMDFormat.RigidbodyList list = format_.rigidbody_list;
			for (int i = 0; i < list.rigidbody_count; i++)
				ignoreRigid[list.rigidbody[i].rigidbody_group_index].Add(i);
			return ignoreRigid;
		}

		// グループターゲット
		int[] GetRigidbodyGroupTargets(GameObject[] rigids)
		{
			int[] result = new int[rigids.Length];
			for (int i = 0; i < rigids.Length; i++)
			{
				result[i] = format_.rigidbody_list.rigidbody[i].rigidbody_group_target;
			}
			return result;
		}
		
		/// <summary>
		/// ファイルパス文字列の取得
		/// </summary>
		/// <returns>ファイルパスに使用可能な文字列</returns>
		/// <param name='src'>ファイルパスに使用したい文字列</param>
		private static string GetFilePathString(string src) {
			return src.Replace('\\', '＼')
						.Replace('/',  '／')
						.Replace(':',  '：')
						.Replace('*',  '＊')
						.Replace('?',  '？')
						.Replace('"',  '”')
						.Replace('<',  '＜')
						.Replace('>',  '＞')
						.Replace('|',  '｜')
						.Replace("\n",  string.Empty)
						.Replace("\r",  string.Empty);
		}
		
		static float default_outline_width = 0.2f;			//標準エッジ幅
		static Color default_outline_color = Color.black;	//標準エッジ色
		
		GameObject	root_game_object_;
		PMDFormat	format_;
		ShaderType	shader_type_;
		bool		use_rigidbody_;
		bool		use_mecanim_;
		bool		use_ik_;
		float		scale_;
	}
}
