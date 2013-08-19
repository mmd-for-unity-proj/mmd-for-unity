using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MMD.PMX;

namespace MMD
{
	public class PMXConverter
	{
		/// <summary>
		/// GameObjectを作成する
		/// </summary>
		/// <param name='name'>内部形式データ</param>
		/// <param name='shader_type'>シェーダーの種類</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='use_mecanim'>Mecanimを使用するか</param>
		/// <param name='use_ik'>IKを使用するか</param>
		public static GameObject CreateGameObject(PMXFormat format, bool use_rigidbody, bool use_mecanim, bool use_ik) {
			PMXConverter converter = new PMXConverter();
			return converter.CreateGameObject_(format, use_rigidbody, use_mecanim, use_ik);
		}

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		/// <remarks>
		/// ユーザーに依るインスタンス作成を禁止する
		/// </remarks>
		private PMXConverter() {}

		/// <summary>
		/// GameObjectを作成する
		/// </summary>
		/// <param name='name'>内部形式データ</param>
		/// <param name='shader_type'>シェーダーの種類</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='use_mecanim'>Mecanimを使用するか</param>
		/// <param name='use_ik'>IKを使用するか</param>
		private GameObject CreateGameObject_(PMXFormat format, bool use_rigidbody, bool use_mecanim, bool use_ik) {
			format_ = format;
			use_rigidbody_ = use_rigidbody;
			use_mecanim_ = use_mecanim;
			use_ik_ = use_ik;
			root_game_object_ = new GameObject(format_.meta_header.name);
			MMDEngine engine = root_game_object_.AddComponent<MMDEngine>(); //MMDEngine追加
		
			Mesh mesh = CreateMesh();					// メッシュの生成・設定
			Material[] materials = CreateMaterials();	// マテリアルの生成・設定
			GameObject[] bones = CreateBones();			// ボーンの生成・設定
			CreateMorph(bones);							// モーフの生成・設定

	
			// バインドポーズの作成
			BuildingBindpose(mesh, materials, bones);
			root_game_object_.AddComponent<Animation>();	// アニメーションを追加
	
			// IKの登録
			if (use_ik_) {
				engine.bone_controllers = EntryBoneController(bones);
				engine.ik_list = engine.bone_controllers.Where(x=>null != x.ik_solver)
														.Select(x=>x.ik_solver)
														.ToArray();
			}
	
			// 剛体関連
			if (use_rigidbody_) {
				GameObject[] rigids = CreateRigids();
				AssignRigidbodyToBone(bones, rigids);
				SetRigidsSettings(bones, rigids);
				GameObject[] joints = CreateJoints(rigids);
				GlobalizeRigidbody(joints);

				// 非衝突グループ
				List<int>[] ignoreGroups = SettingIgnoreRigidGroups(rigids);
				int[] groupTarget = GetRigidbodyGroupTargets(rigids);

				MMDEngine.Initialize(engine, groupTarget, ignoreGroups, rigids);
			}
	
			// Mecanim設定 (not work yet..)
#if UNITY_4_0 || UNITY_4_1
			if (use_mecanim_) {
				AvatarSettingScript avt_setting = new AvatarSettingScript(root_game_object_);
				avt_setting.SettingAvatar();
			}
#endif

			return root_game_object_;
		}
		
		/// <summary>
		/// メッシュ作成
		/// </summary>
		/// <returns>メッシュ</returns>
		Mesh CreateMesh()
		{
			Mesh mesh = new Mesh();
			EntryAttributesForMesh(mesh);
			SetSubMesh(mesh);
			CreateAssetForMesh(mesh);
			return mesh;
		}
		
		/// <summary>
		/// メッシュに基本情報(頂点座標・法線・UV・ボーンウェイト)を登録する
		/// </summary>
		/// <param name='mesh'>対象メッシュ</param>
		void EntryAttributesForMesh(Mesh mesh)
		{
			mesh.vertices = format_.vertex_list.vertex.Select(x=>x.pos).ToArray();
			mesh.normals = format_.vertex_list.vertex.Select(x=>x.normal_vec).ToArray();
			mesh.uv = format_.vertex_list.vertex.Select(x=>x.uv).ToArray();
			if (0 < format_.header.additionalUV) {
				//追加UVが1つ以上有れば
				//1つ目のみ登録
				mesh.uv2 = format_.vertex_list.vertex.Select(x=>new Vector2(x.add_uv[0].x, x.add_uv[0].y)).ToArray();
			}
			mesh.boneWeights = format_.vertex_list.vertex.Select(x=>ConvertBoneWeight(x.bone_weight)).ToArray();
			mesh.colors = format_.vertex_list.vertex.Select(x=>new Color(0.0f, 0.0f, 0.0f, x.edge_magnification * 0.25f)).ToArray(); //不透明度にエッジ倍率を0.25倍した情報を仕込む
		}
		
		/// <summary>
		/// ボーンウェイトをUnity用に変換する
		/// </summary>
		/// <returns>Unity用ボーンウェイト</returns>
		/// <param name='bone_weight'>PMX用ボーンウェイト</param>
		BoneWeight ConvertBoneWeight(PMXFormat.BoneWeight bone_weight)
		{
			//HACK: 取り敢えずボーンウェイトタイプを考えずにBDEFx系として登録する
			BoneWeight result = new BoneWeight();
			switch (bone_weight.method) {
			case PMXFormat.Vertex.WeightMethod.BDEF1: goto case PMXFormat.Vertex.WeightMethod.BDEF4;
			case PMXFormat.Vertex.WeightMethod.BDEF2: goto case PMXFormat.Vertex.WeightMethod.BDEF4;
			case PMXFormat.Vertex.WeightMethod.BDEF4:
				//BDEF4なら
				result.boneIndex0 = (int)bone_weight.bone1_ref;
				result.weight0 = bone_weight.bone1_weight;
				result.boneIndex1 = (int)bone_weight.bone2_ref;;
				result.weight1 = bone_weight.bone2_weight;
				result.boneIndex2 = (int)bone_weight.bone3_ref;
				result.weight2 = bone_weight.bone3_weight;
				result.boneIndex3 = (int)bone_weight.bone4_ref;
				result.weight3 = bone_weight.bone4_weight;
				break;
			case PMXFormat.Vertex.WeightMethod.SDEF:
				//SDEFなら
				//HACK: BDEF4と同じ対応
				goto case PMXFormat.Vertex.WeightMethod.BDEF4;
			case PMXFormat.Vertex.WeightMethod.QDEF:
				//QDEFなら
				//HACK: BDEF4と同じ対応
				goto case PMXFormat.Vertex.WeightMethod.BDEF4;
			default:
				throw new System.ArgumentOutOfRangeException();
			}
			return result;
		}
		
		/// <summary>
		/// メッシュにサブメッシュを登録する
		/// </summary>
		/// <param name='mesh'>対象メッシュ</param>
		void SetSubMesh(Mesh mesh)
		{
			// マテリアル対サブメッシュ
			// サブメッシュとはマテリアルに適用したい面頂点データのこと
			// 面ごとに設定するマテリアルはここ
			mesh.subMeshCount = format_.material_list.material.Length;
			
			int sum = 0;
			for (int i = 0, i_max = mesh.subMeshCount; i < i_max; ++i) {
				int count = (int)format_.material_list.material[i].face_vert_count;
				// 面頂点は材質0から順番に加算されている
				int[] indices = format_.face_vertex_list.face_vert_index.Skip(sum).Take(count).Select(x=>(int)x).ToArray(); //[sum](含む)から[sum+count](含まず)迄取り出し
				mesh.SetTriangles(indices, i);
				sum += (int)format_.material_list.material[i].face_vert_count;
			}
		}
		
		/// <summary>
		/// メッシュをProjectに登録する
		/// </summary>
		/// <param name='mesh'>対象メッシュ</param>
		void CreateAssetForMesh(Mesh mesh)
		{
			string path = format_.meta_header.folder + "/" + format_.meta_header.name + ".asset";
			AssetDatabase.CreateAsset(mesh, path);
		}
		
		/// <summary>
		/// マテリアル作成
		/// </summary>
		/// <returns>マテリアル</returns>
		Material[] CreateMaterials()
		{
			Material[] materials = EntryAttributesForMaterials();
			CreateAssetForMaterials(materials);
			return materials;
		}

		/// <summary>
		/// マテリアルに基本情報(シェーダー・カラー・テクスチャ)を登録する
		/// </summary>
		/// <returns>マテリアル</returns>
		Material[] EntryAttributesForMaterials()
		{
			return format_.material_list.material.Select(x=>ConvertMaterial(x)).ToArray();
		}
		
		/// <summary>
		/// マテリアルをUnity用に変換する
		/// </summary>
		/// <returns>Unity用マテリアル</returns>
		/// <param name='material'>PMX用マテリアル</param>
		Material ConvertMaterial(PMXFormat.Material material)
		{
			//先にテクスチャ情報を検索
			Texture2D main_texture = null;
			if (material.usually_texture_index < format_.texture_list.texture_file.Length) {
				string texture_file_name = format_.texture_list.texture_file[material.usually_texture_index];
				string path = format_.meta_header.folder + "/" + texture_file_name;
				main_texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
			}
			
			//マテリアルに設定
			Material result = new Material(Shader.Find(GetMmdShaderPath(material, main_texture)));
		
			// シェーダに依って値が有ったり無かったりするが、設定してもエラーに為らない様なので全部設定
			result.SetColor("_Color", material.diffuse_color);
			result.SetColor("_AmbColor", material.ambient_color);
			result.SetFloat("_Opacity", material.diffuse_color.a);
			result.SetColor("_SpecularColor", material.specular_color);
			result.SetFloat("_Shininess", material.specularity);
			// エッジ
			result.SetFloat("_OutlineWidth", material.edge_size);
			result.SetColor("_OutlineColor", material.edge_color);

			// スフィアテクスチャ
			if (material.sphere_texture_index < format_.texture_list.texture_file.Length) {
				string sphere_texture_file_name = format_.texture_list.texture_file[material.sphere_texture_index];
				string path = format_.meta_header.folder + "/" + sphere_texture_file_name;
				Texture2D sphere_texture = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
				
				switch (material.sphere_mode) {
				case PMXFormat.Material.SphereMode.AddSphere: // 加算
					result.SetTexture("_SphereAddTex", sphere_texture);
					result.SetTextureScale("_SphereAddTex", new Vector2(1, -1));
					break;
				case PMXFormat.Material.SphereMode.MulSphere: // 乗算
					result.SetTexture("_SphereMulTex", sphere_texture);
					result.SetTextureScale("_SphereMulTex", new Vector2(1, -1));
					break;
				case PMXFormat.Material.SphereMode.SubTexture: // サブテクスチャ
					//サブテクスチャ用シェーダーが無いので設定しない
					break;
				default:
					//empty.
					break;
					
				}
			}
			
			// トゥーンテクスチャ
			{
				string toon_texture_name = null;
				if (0 < material.common_toon) {
					//共通トゥーン
					toon_texture_name = "toon" + material.common_toon.ToString("00") + ".bmp";
				} else if (material.toon_texture_index < format_.texture_list.texture_file.Length) {
					//自前トゥーン
					toon_texture_name = format_.texture_list.texture_file[material.toon_texture_index];
				}
				if (!string.IsNullOrEmpty(toon_texture_name)) {
					string resource_path = UnityEditor.AssetDatabase.GetAssetPath(Shader.Find("MMD/HalfLambertOutline"));
					Texture2D toon_texture = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(resource_path, typeof(Texture2D));
					result.SetTexture("_ToonTex", toon_texture);
					result.SetTextureScale("_ToonTex", new Vector2(1, -1));
				}
			}

			// テクスチャが空でなければ登録
			if (null != main_texture) {
				result.mainTexture = main_texture;
				result.mainTextureScale = new Vector2(1, -1);
			}
			
			return result;
		}
		
		/// <summary>
		/// MMDシェーダーパスの取得
		/// </summary>
		/// <returns>MMDシェーダーパス</returns>
		/// <param name='material'>シェーダーを設定するマテリアル</param>
		/// <param name='texture'>シェーダーに設定するメインテクスチャ</param>
		string GetMmdShaderPath(PMXFormat.Material material, Texture2D texture) {
			string result = "MMD/";
			if (IsTransparentMaterial(material, texture)) {
				result += "Transparent/";
			}
			result += "PMDMaterial";
			if (IsEdgeMaterial(material)) {
				result += "-with-Outline";
			}
			if (IsCullBackMaterial(material)) {
				result += "-CullBack";
			}
			if (IsNoCastShadowMaterial(material)) {
				result += "-NoCastShadow";
			}
#if MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER	//影受け無しのシェーダはまだ無いので無効化
			if (IsNoReceiveShadowMaterial(material)) {
				result += "-NoReceiveShadow";
			}
#endif //MFU_ENABLE_NO_RECEIVE_SHADOW_SHADER
			return result;
		}

		/// <summary>
		/// 透過マテリアル確認
		/// </summary>
		/// <returns>true:透過, false:不透明</returns>
		/// <param name='material'>シェーダーを設定するマテリアル</param>
		/// <param name='texture'>シェーダーに設定するメインテクスチャ</param>
		bool IsTransparentMaterial(PMXFormat.Material material, Texture2D texture) {
			bool result = false;
			result = result || (material.diffuse_color.a < 1.0f);
			if (null != texture) {
#if UNITY_4_2
				result = result || texture.alphaIsTransparency;
#else
				// TODO: 上記if内の代替コード必須
				// result = result;
#endif
			}
			return result;
		}
		
		/// <summary>
		/// エッジマテリアル確認
		/// </summary>
		/// <returns>true:エッジ有り, false:無エッジ</returns>
		/// <param name='material'>シェーダーを設定するマテリアル</param>
		bool IsEdgeMaterial(PMXFormat.Material material) {
			bool result;
			if (0 != (PMXFormat.Material.Flag.Edge & material.flag)) {
				//エッジ有りなら
				result = true;
			} else {
				//エッジ無し
				result = false;
			}
			return result;
		}
		
		/// <summary>
		/// 背面カリングマテリアル確認
		/// </summary>
		/// <returns>true:背面カリングする, false:背面カリングしない</returns>
		/// <param name='material'>シェーダーを設定するマテリアル</param>
		bool IsCullBackMaterial(PMXFormat.Material material) {
			bool result;
			if (0 != (PMXFormat.Material.Flag.Reversible & material.flag)) {
				//両面描画なら
				//背面カリングしない
				result = false;
			} else {
				//両面描画で無いなら
				//背面カリングする
				result = true;
			}
			return result;
		}
		
		/// <summary>
		/// 無影マテリアル確認
		/// </summary>
		/// <returns>true:無影, false:影放ち</returns>
		/// <param name='material'>シェーダーを設定するマテリアル</param>
		bool IsNoCastShadowMaterial(PMXFormat.Material material) {
			bool result;
			if (0 != (PMXFormat.Material.Flag.CastShadow & material.flag)) {
				//影放ち
				result = false;
			} else {
				//無影
				result = true;
			}
			return result;
		}
		
		/// <summary>
		/// 影受け無しマテリアル確認
		/// </summary>
		/// <returns>true:影受け無し, false:影受け</returns>
		/// <param name='material'>シェーダーを設定するマテリアル</param>
		bool IsNoReceiveShadowMaterial(PMXFormat.Material material) {
			bool result;
			if (0 != (PMXFormat.Material.Flag.ReceiveSelfShadow & material.flag)) {
				//影受け
				result = false;
			} else {
				//影受け無し
				result = true;
			}
			return result;
		}
		
		/// <summary>
		/// マテリアルをProjectに登録する
		/// </summary>
		/// <param name='mesh'>対象マテリアル</param>
		void CreateAssetForMaterials(Material[] materials) {
			// 適当なフォルダに投げる
			string path = format_.meta_header.folder + "/Materials/";
			if (!System.IO.Directory.Exists(path)) { 
				AssetDatabase.CreateFolder(format_.meta_header.folder, "Materials");
			}
			
			for (int i = 0, i_max = materials.Length; i < i_max; ++i) {
				string file_name = path + i.ToString() + "_" + format_.material_list.material[i].name + ".asset";
				AssetDatabase.CreateAsset(materials[i], file_name);
			}
		}
		
		/// <summary>
		/// ボーン作成
		/// </summary>
		/// <returns>ボーンのゲームオブジェクト</returns>
		GameObject[] CreateBones()
		{
			GameObject[] bones = EntryAttributeForBones();
			AttachParentsForBone(bones);
			return bones;
		}
		
		/// <summary>
		/// ボーンに基本情報(名前・位置)を登録する
		/// </summary>
		/// <returns>ボーンのゲームオブジェクト</returns>
		GameObject[] EntryAttributeForBones()
		{
			return format_.bone_list.bone.Select(x=>{
				GameObject game_object = new GameObject(x.bone_name);
				game_object.transform.position = x.bone_position;
				return game_object;
			}).ToArray();
		}
		
		/// <summary>
		/// 親子関係の構築
		/// </summary>
		/// <param name='bones'>ボーンのゲームオブジェクト</param>
		void AttachParentsForBone(GameObject[] bones)
		{
			//モデルルートを生成してルートの子供に付ける
			Transform model_root_transform = (new GameObject("Model")).transform;
			model_root_transform.parent = root_game_object_.transform;

			for (int i = 0, i_max = format_.bone_list.bone.Length; i < i_max; ++i) {
				uint parent_bone_index = format_.bone_list.bone[i].parent_bone_index;
				if (parent_bone_index < (uint)bones.Length) {
					//親のボーンが有るなら
					//それの子に為る
					bones[i].transform.parent = bones[parent_bone_index].transform;
				} else {
					//親のボーンが無いなら
					//モデルルートの子に為る
					bones[i].transform.parent = model_root_transform;
				}
			}
		}
		
		/// <summary>
		/// モーフ作成
		/// </summary>
		void CreateMorph(GameObject[] bones)
		{
			//表情ルートを生成してルートの子供に付ける
			GameObject expression_root = new GameObject("Expression");
			Transform expression_root_transform = expression_root.transform;
			expression_root_transform.parent = root_game_object_.transform;

			//表情マネージャー
			MorphManager morph_manager = expression_root.AddComponent<MorphManager>();
			morph_manager.uv_morph = new MorphManager.UvMorphPack[1 + format_.header.additionalUV]; //UVモーフ数設定

			//個別モーフスクリプト作成
			GameObject[] morphs = new GameObject[format_.morph_list.morph_data.Length];
			for (int i = 0, i_max = format_.morph_list.morph_data.Length; i < i_max; ++i) {
				morphs[i] = new GameObject(format_.morph_list.morph_data[i].morph_name);
				// 表情を親ボーンに付ける
				morphs[i].transform.parent = expression_root_transform;
			}
			
			//グループモーフ作成
			CreateGroupMorph(morph_manager, morphs);
			//ボーンモーフ
			morph_manager.bones = bones.Select(x=>x.transform).ToArray();
			CreateBoneMorph(morph_manager, morphs);
			//頂点モーフ作成
			CreateVertexMorph(morph_manager, morphs);
			//UV・追加UVモーフ作成
			CreateUvMorph(morph_manager, morphs);
			//材質モーフ作成
			CreateMaterialMorph(morph_manager, morphs);
			//モーフ一覧設定(モーフコンポーネントの情報を拾う為、最後に設定する)
			morph_manager.morphs = morphs.Select(x=>x.GetComponent<MorphBase>()).ToArray();
		}

		/// <summary>
		/// グループモーフ作成
		/// </summary>
		/// <param name='morph_manager'>表情マネージャー</param>
		/// <param name='morphs'>モーフのゲームオブジェクト</param>
		void CreateGroupMorph(MorphManager morph_manager, GameObject[] morphs)
		{
			//インデックスと元データの作成
			List<uint> original_indices = format_.morph_list.morph_data.Where(x=>(PMXFormat.MorphData.MorphType.Group == x.morph_type)) //該当モーフに絞る
																		.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.GroupMorphOffset)y).morph_index)) //インデックスの取り出しと連結
																		.Distinct() //重複したインデックスの削除
																		.ToList(); //ソートに向けて一旦リスト化
			original_indices.Sort(); //ソート
			int[] indices = original_indices.Select(x=>(int)x).ToArray();
			float[] source = Enumerable.Repeat(0.0f, indices.Length) //インデックスを用いて、元データをパック
										.ToArray();
			
			//インデックス逆引き用辞書の作成
			Dictionary<uint, uint> index_reverse_dictionary = new Dictionary<uint, uint>();
			for (uint i = 0, i_max = (uint)indices.Length; i < i_max; ++i) {
				index_reverse_dictionary.Add((uint)indices[i], i);
			}

			//個別モーフスクリプトの作成
			GroupMorph[] script = Enumerable.Range(0, format_.morph_list.morph_data.Length)
											.Where(x=>PMXFormat.MorphData.MorphType.Group == format_.morph_list.morph_data[x].morph_type) //該当モーフに絞る
											.Select(x=>AssignGroupMorph(morphs[x], format_.morph_list.morph_data[x], index_reverse_dictionary))
											.ToArray();

			//表情マネージャーにインデックス・元データ・スクリプトの設定
			morph_manager.group_morph = new MorphManager.GroupMorphPack(indices, source, script);
		}

		/// <summary>
		/// グループモーフ設定
		/// </summary>
		/// <returns>グループモーフスクリプト</returns>
		/// <param name='morph'>モーフのゲームオブジェクト</param>
		/// <param name='data'>PMX用モーフデータ</param>
		/// <param name='index_reverse_dictionary'>インデックス逆引き用辞書</param>
		GroupMorph AssignGroupMorph(GameObject morph, PMXFormat.MorphData data, Dictionary<uint, uint> index_reverse_dictionary)
		{
			GroupMorph result = morph.AddComponent<GroupMorph>();
			result.panel = (MorphManager.PanelType)data.handle_panel;
			result.indices = data.morph_offset.Select(x=>((PMXFormat.GroupMorphOffset)x).morph_index) //インデックスを取り出し
												.Select(x=>(int)index_reverse_dictionary[x]) //逆変換を掛ける
												.ToArray();
			result.values = data.morph_offset.Select(x=>((PMXFormat.GroupMorphOffset)x).morph_rate).ToArray();
			return result;
		}

		/// <summary>
		/// ボーンモーフ作成
		/// </summary>
		/// <param name='morph_manager'>表情マネージャー</param>
		/// <param name='morphs'>モーフのゲームオブジェクト</param>
		void CreateBoneMorph(MorphManager morph_manager, GameObject[] morphs)
		{
			//インデックスと元データの作成
			List<uint> original_indices = format_.morph_list.morph_data.Where(x=>(PMXFormat.MorphData.MorphType.Bone == x.morph_type)) //該当モーフに絞る
																		.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.BoneMorphOffset)y).bone_index)) //インデックスの取り出しと連結
																		.Distinct() //重複したインデックスの削除
																		.ToList(); //ソートに向けて一旦リスト化
			original_indices.Sort(); //ソート
			int[] indices = original_indices.Select(x=>(int)x).ToArray();
			BoneMorph.BoneMorphParameter[] source = indices.Where(x=>x<format_.bone_list.bone.Length)
															.Select(x=>{  //インデックスを用いて、元データをパック
																	PMXFormat.Bone y = format_.bone_list.bone[x];
																	BoneMorph.BoneMorphParameter result = new BoneMorph.BoneMorphParameter();
																	result.position = y.bone_position;
																	if (y.parent_bone_index < (uint)format_.bone_list.bone.Length) {
																		//親が居たらローカル座標化
																		result.position -= format_.bone_list.bone[y.parent_bone_index].bone_position;
																	}
																	result.rotation = Quaternion.identity;
																	return result;
																})
															.ToArray();
			
			//インデックス逆引き用辞書の作成
			Dictionary<uint, uint> index_reverse_dictionary = new Dictionary<uint, uint>();
			for (uint i = 0, i_max = (uint)indices.Length; i < i_max; ++i) {
				index_reverse_dictionary.Add((uint)indices[i], i);
			}

			//個別モーフスクリプトの作成
			BoneMorph[] script = Enumerable.Range(0, format_.morph_list.morph_data.Length)
											.Where(x=>PMXFormat.MorphData.MorphType.Bone == format_.morph_list.morph_data[x].morph_type) //該当モーフに絞る
											.Select(x=>AssignBoneMorph(morphs[x], format_.morph_list.morph_data[x], index_reverse_dictionary))
											.ToArray();

			//表情マネージャーにインデックス・元データ・スクリプトの設定
			morph_manager.bone_morph = new MorphManager.BoneMorphPack(indices, source, script);
		}

		/// <summary>
		/// ボーンモーフ設定
		/// </summary>
		/// <returns>ボーンモーフスクリプト</returns>
		/// <param name='morph'>モーフのゲームオブジェクト</param>
		/// <param name='data'>PMX用モーフデータ</param>
		/// <param name='index_reverse_dictionary'>インデックス逆引き用辞書</param>
		BoneMorph AssignBoneMorph(GameObject morph, PMXFormat.MorphData data, Dictionary<uint, uint> index_reverse_dictionary)
		{
			BoneMorph result = morph.AddComponent<BoneMorph>();
			result.panel = (MorphManager.PanelType)data.handle_panel;
			result.indices = data.morph_offset.Select(x=>((PMXFormat.BoneMorphOffset)x).bone_index) //インデックスを取り出し
												.Select(x=>(int)index_reverse_dictionary[x]) //逆変換を掛ける
												.ToArray();
			result.values = data.morph_offset.Select(x=>{
														PMXFormat.BoneMorphOffset y = (PMXFormat.BoneMorphOffset)x;
														BoneMorph.BoneMorphParameter param = new BoneMorph.BoneMorphParameter();
														param.position = y.move_value;
														param.rotation = y.rotate_value;
														return param;
													})
											.ToArray();
			return result;
		}

		/// <summary>
		/// 頂点モーフ作成
		/// </summary>
		/// <param name='morph_manager'>表情マネージャー</param>
		/// <param name='morphs'>モーフのゲームオブジェクト</param>
		void CreateVertexMorph(MorphManager morph_manager, GameObject[] morphs)
		{
			//インデックスと元データの作成
			List<uint> original_indices = format_.morph_list.morph_data.Where(x=>(PMXFormat.MorphData.MorphType.Vertex == x.morph_type)) //該当モーフに絞る
																		.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.VertexMorphOffset)y).vertex_index)) //インデックスの取り出しと連結
																		.Distinct() //重複したインデックスの削除
																		.ToList(); //ソートに向けて一旦リスト化
			original_indices.Sort(); //ソート
			int[] indices = original_indices.Select(x=>(int)x).ToArray();
			Vector3[] source = indices.Select(x=>format_.vertex_list.vertex[x].pos) //インデックスを用いて、元データをパック
									.ToArray();
			
			//インデックス逆引き用辞書の作成
			Dictionary<uint, uint> index_reverse_dictionary = new Dictionary<uint, uint>();
			for (uint i = 0, i_max = (uint)indices.Length; i < i_max; ++i) {
				index_reverse_dictionary.Add((uint)indices[i], i);
			}

			//個別モーフスクリプトの作成
			VertexMorph[] script = Enumerable.Range(0, format_.morph_list.morph_data.Length)
											.Where(x=>PMXFormat.MorphData.MorphType.Vertex == format_.morph_list.morph_data[x].morph_type) //該当モーフに絞る
											.Select(x=>AssignVertexMorph(morphs[x], format_.morph_list.morph_data[x], index_reverse_dictionary))
											.ToArray();

			//表情マネージャーにインデックス・元データ・スクリプトの設定
			morph_manager.vertex_morph = new MorphManager.VertexMorphPack(indices, source, script);
		}

		/// <summary>
		/// 頂点モーフ設定
		/// </summary>
		/// <returns>頂点モーフスクリプト</returns>
		/// <param name='morph'>モーフのゲームオブジェクト</param>
		/// <param name='data'>PMX用モーフデータ</param>
		/// <param name='index_reverse_dictionary'>インデックス逆引き用辞書</param>
		VertexMorph AssignVertexMorph(GameObject morph, PMXFormat.MorphData data, Dictionary<uint, uint> index_reverse_dictionary)
		{
			VertexMorph result = morph.AddComponent<VertexMorph>();
			result.panel = (MorphManager.PanelType)data.handle_panel;
			result.indices = data.morph_offset.Select(x=>((PMXFormat.VertexMorphOffset)x).vertex_index) //インデックスを取り出し
												.Select(x=>(int)index_reverse_dictionary[x]) //逆変換を掛ける
												.ToArray();
			result.values = data.morph_offset.Select(x=>((PMXFormat.VertexMorphOffset)x).position_offset).ToArray();
			return result;
		}

		/// <summary>
		/// UV・追加UVモーフ作成
		/// </summary>
		/// <param name='morph_manager'>表情マネージャー</param>
		/// <param name='morphs'>モーフのゲームオブジェクト</param>
		void CreateUvMorph(MorphManager morph_manager, GameObject[] morphs)
		{
			for (int morph_type_index = 0, morph_type_index_max = 1 + format_.header.additionalUV; morph_type_index < morph_type_index_max; ++morph_type_index) {
				//モーフタイプ
				PMXFormat.MorphData.MorphType morph_type;
				switch (morph_type_index) {
				case 0:	morph_type = PMXFormat.MorphData.MorphType.Uv;	break;
				case 1:	morph_type = PMXFormat.MorphData.MorphType.Adduv1;	break;
				case 2:	morph_type = PMXFormat.MorphData.MorphType.Adduv2;	break;
				case 3:	morph_type = PMXFormat.MorphData.MorphType.Adduv3;	break;
				case 4:	morph_type = PMXFormat.MorphData.MorphType.Adduv4;	break;
				default:	throw new System.ArgumentOutOfRangeException();
				}

				//インデックスと元データの作成
				List<uint> original_indices = format_.morph_list.morph_data.Where(x=>(morph_type == x.morph_type)) //該当モーフに絞る
																			.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.UVMorphOffset)y).vertex_index)) //インデックスの取り出しと連結
																			.Distinct() //重複したインデックスの削除
																			.ToList(); //ソートに向けて一旦リスト化
				original_indices.Sort(); //ソート
				int[] indices = original_indices.Select(x=>(int)x).ToArray();
				Vector2[] source;
				if (0 == morph_type_index) {
					//通常UV
					source = indices.Select(x=>format_.vertex_list.vertex[x].uv) //インデックスを用いて、元データをパック
									.Select(x=>new Vector2(x.x, x.y))
									.ToArray();
				} else {
					//追加UV
					source = indices.Select(x=>format_.vertex_list.vertex[x].add_uv[morph_type_index - 1]) //インデックスを用いて、元データをパック
									.Select(x=>new Vector2(x.x, x.y))
									.ToArray();
				}
	
				//インデックス逆引き用辞書の作成
				Dictionary<uint, uint> index_reverse_dictionary = new Dictionary<uint, uint>();
				for (uint i = 0, i_max = (uint)indices.Length; i < i_max; ++i) {
					index_reverse_dictionary.Add((uint)indices[i], i);
				}
				
				//個別モーフスクリプトの作成
				UvMorph[] script = Enumerable.Range(0, format_.morph_list.morph_data.Length)
											.Where(x=>morph_type == format_.morph_list.morph_data[x].morph_type) //該当モーフに絞る
											.Select(x=>AssignUvMorph(morphs[x], format_.morph_list.morph_data[x], index_reverse_dictionary))
											.ToArray();
				
				//表情マネージャーにインデックス・元データ・スクリプトの設定
				morph_manager.uv_morph[morph_type_index] = new MorphManager.UvMorphPack(indices, source, script);
			}
		}

		/// <summary>
		/// UV・追加UVモーフ設定
		/// </summary>
		/// <returns>UVモーフスクリプト</returns>
		/// <param name='morph'>モーフのゲームオブジェクト</param>
		/// <param name='data'>PMX用モーフデータ</param>
		/// <param name='index_reverse_dictionary'>インデックス逆引き用辞書</param>
		UvMorph AssignUvMorph(GameObject morph, PMXFormat.MorphData data, Dictionary<uint, uint> index_reverse_dictionary)
		{
			UvMorph result = morph.AddComponent<UvMorph>();
			result.panel = (MorphManager.PanelType)data.handle_panel;
			result.indices = data.morph_offset.Select(x=>((PMXFormat.UVMorphOffset)x).vertex_index) //インデックスを取り出し
												.Select(x=>(int)index_reverse_dictionary[x]) //逆変換を掛ける
												.ToArray();
			result.values = data.morph_offset.Select(x=>((PMXFormat.UVMorphOffset)x).uv_offset)
												.Select(x=>new Vector2(x.x, x.y))
												.ToArray();
			return result;
		}

		/// <summary>
		/// 材質モーフ作成
		/// </summary>
		/// <param name='morph_manager'>表情マネージャー</param>
		/// <param name='morphs'>モーフのゲームオブジェクト</param>
		void CreateMaterialMorph(MorphManager morph_manager, GameObject[] morphs)
		{
			//インデックスと元データの作成
			List<uint> original_indices = format_.morph_list.morph_data.Where(x=>(PMXFormat.MorphData.MorphType.Material == x.morph_type)) //該当モーフに絞る
																		.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.MaterialMorphOffset)y).material_index)) //インデックスの取り出しと連結
																		.Distinct() //重複したインデックスの削除
																		.ToList(); //ソートに向けて一旦リスト化
			original_indices.Sort(); //ソート
			if (uint.MaxValue == original_indices.LastOrDefault()) {
				//最後が uint.MaxValue(≒-1) なら
				//全材質対象が存在するので全インデックスを取得
				original_indices = Enumerable.Range(0, format_.material_list.material.Length + 1).Select(x=>(uint)x).ToList();
				original_indices[format_.material_list.material.Length] = uint.MaxValue; //uint.MaxValueを忘れない
			}
			int[] indices = original_indices.Select(x=>(int)x).ToArray();
			MaterialMorph.MaterialMorphParameter[] source = indices.Where(x=>x<format_.material_list.material.Length)
																	.Select(x=>{  //インデックスを用いて、元データをパック
																			MaterialMorph.MaterialMorphParameter result = new MaterialMorph.MaterialMorphParameter();
																			if (0 <= x) {
																				//-1(全材質対象)で無いなら
																				//元データを取得
																				PMXFormat.Material y = format_.material_list.material[x];
																				result.color = y.diffuse_color;
																				result.specular = new Color(y.specular_color.r, y.specular_color.g, y.specular_color.b, y.specularity);
																				result.ambient = y.ambient_color;
																				result.outline_color = y.edge_color;
																				result.outline_width = y.edge_size;
																				result.texture_color = Color.white;
																				result.sphere_color = Color.white;
																				result.toon_color = Color.white;
																			} else {
																				//-1(全材質対象)なら
																				//適当にでっち上げる
																				result = MaterialMorph.MaterialMorphParameter.zero;
																			}
																			return result;
																		})
																	.ToArray();
			
			//インデックス逆引き用辞書の作成
			Dictionary<uint, uint> index_reverse_dictionary = new Dictionary<uint, uint>();
			for (uint i = 0, i_max = (uint)indices.Length; i < i_max; ++i) {
				index_reverse_dictionary.Add((uint)indices[i], i);
			}

			//個別モーフスクリプトの作成
			MaterialMorph[] script = Enumerable.Range(0, format_.morph_list.morph_data.Length)
												.Where(x=>PMXFormat.MorphData.MorphType.Material == format_.morph_list.morph_data[x].morph_type) //該当モーフに絞る
												.Select(x=>AssignMaterialMorph(morphs[x], format_.morph_list.morph_data[x], index_reverse_dictionary))
												.ToArray();

			//表情マネージャーにインデックス・元データ・スクリプトの設定
			morph_manager.material_morph = new MorphManager.MaterialMorphPack(indices, source, script);
		}

		/// <summary>
		/// 材質モーフ設定
		/// </summary>
		/// <returns>材質モーフスクリプト</returns>
		/// <param name='morph'>モーフのゲームオブジェクト</param>
		/// <param name='data'>PMX用モーフデータ</param>
		/// <param name='index_reverse_dictionary'>インデックス逆引き用辞書</param>
		MaterialMorph AssignMaterialMorph(GameObject morph, PMXFormat.MorphData data, Dictionary<uint, uint> index_reverse_dictionary)
		{
			MaterialMorph result = morph.AddComponent<MaterialMorph>();
			result.panel = (MorphManager.PanelType)data.handle_panel;
			result.indices = data.morph_offset.Select(x=>((PMXFormat.MaterialMorphOffset)x).material_index) //インデックスを取り出し
												.Select(x=>(int)index_reverse_dictionary[x]) //逆変換を掛ける
												.ToArray();
			result.values = data.morph_offset.Select(x=>{
														PMXFormat.MaterialMorphOffset y = (PMXFormat.MaterialMorphOffset)x;
														MaterialMorph.MaterialMorphParameter param = new MaterialMorph.MaterialMorphParameter();
														param.color = y.diffuse;
														param.specular = new Color(y.specular.r, y.specular.g, y.specular.b, y.specularity);
														param.ambient = y.ambient;
														param.outline_color = y.edge_color;
														param.outline_width = y.edge_size;
														param.texture_color = y.texture_coefficient;
														param.sphere_color = y.sphere_texture_coefficient;
														param.toon_color = y.toon_texture_coefficient;
														return param;
													})
											.ToArray();
			result.operation = data.morph_offset.Select(x=>(MaterialMorph.OperationType)((PMXFormat.MaterialMorphOffset)x).offset_method)
												.ToArray();
			return result;
		}

		/// <summary>
		/// バインドポーズの作成
		/// </summary>
		/// <param name='mesh'>対象メッシュ</param>
		/// <param name='materials'>設定するマテリアル</param>
		/// <param name='bones'>設定するボーン</param>
		void BuildingBindpose(Mesh mesh, Material[] materials, GameObject[] bones)
		{
			// ここで本格的な適用
			//スキンメッシュレンダラー設定
			SkinnedMeshRenderer smr = root_game_object_.AddComponent<SkinnedMeshRenderer>();
			mesh.bindposes = bones.Select(x=>x.transform.worldToLocalMatrix).ToArray();
			smr.sharedMesh = mesh;
			smr.bones = bones.Select(x=>x.transform).ToArray();
			smr.materials = materials;
			smr.receiveShadows = false; //影を受けない
			//表情マネージャ設定
			MorphManager mm = root_game_object_.GetComponentInChildren<MorphManager>();
			mm.mesh = mesh;
			mm.materials = materials;
		}
		
		/// <summary>
		/// IK作成
		/// </summary>
		/// <returns>ボーンコントローラースクリプト</returns>
		/// <param name='bones'>ボーンのゲームオブジェクト</param>
		BoneController[] EntryBoneController(GameObject[] bones)
		{
			//BoneControllerが他のBoneControllerを参照するので先に全ボーンに付与
			foreach (var bone in bones) {
				bone.AddComponent<BoneController>();
			}
			BoneController[] result = Enumerable.Range(0, format_.bone_list.bone.Length)
												.OrderBy(x=>(int)(PMXFormat.Bone.Flag.PhysicsTransform & format_.bone_list.bone[x].bone_flag)) //物理後変形を後方へ
												.ThenBy(x=>format_.bone_list.bone[x].transform_level) //変形階層で安定ソート
												.Select(x=>ConvertBoneController(format_.bone_list.bone[x], x, bones)) //ConvertIk()を呼び出す
												.ToArray();
			return result;
		}
		
		/// <summary>
		/// ボーンをボーンコントローラースクリプトに変換する
		/// </summary>
		/// <returns>ボーンコントローラースクリプト</returns>
		/// <param name='ik_data'>PMX用ボーンデータ</param>
		/// <param name='bone_index'>該当IKデータのボーン通しインデックス</param>
		/// <param name='bones'>ボーンのゲームオブジェクト</param>
		BoneController ConvertBoneController(PMXFormat.Bone bone, int bone_index, GameObject[] bones)
		{
			BoneController result = bones[bone_index].GetComponent<BoneController>();
			if (0.0f != bone.additional_rate) {
				//付与親が有るなら
				result.additive_parent = bones[bone.additional_parent_index].GetComponent<BoneController>();
				result.additive_rate = bone.additional_rate;
				result.add_local = (0 != (PMXFormat.Bone.Flag.AddLocal & bone.bone_flag));
				result.add_move = (0 != (PMXFormat.Bone.Flag.AddMove & bone.bone_flag));
				result.add_rotate = (0 != (PMXFormat.Bone.Flag.AddRotation & bone.bone_flag));
			}
			if (0 != (PMXFormat.Bone.Flag.IkFlag & bone.bone_flag)) {
				//IKが有るなら
				result.ik_solver = bones[bone_index].AddComponent<CCDIKSolver>();
				result.ik_solver.target = bones[bone.ik_data.ik_bone_index].transform;
				result.ik_solver.controll_weight = bone.ik_data.limit_angle / 4; //HACK: CCDIKSolver側で4倍している様なので此処で逆補正
				result.ik_solver.iterations = (int)bone.ik_data.iterations;
				result.ik_solver.chains = bone.ik_data.ik_link.Select(x=>x.target_bone_index).Select(x=>bones[x].transform).ToArray();
				//IK制御下のBoneController登録
				result.ik_solver_targets = Enumerable.Repeat(result.ik_solver.target, 1)
													.Concat(result.ik_solver.chains)
													.Select(x=>x.GetComponent<BoneController>())
													.ToArray();
				
				//IK制御先のボーンについて、物理演算の挙動を調べる
				var operation_types = Enumerable.Repeat(bone.ik_data.ik_bone_index, 1) //IK対象先をEnumerable化
												.Concat(bone.ik_data.ik_link.Select(x=>x.target_bone_index)) //IK制御下を追加
												.Join(format_.rigidbody_list.rigidbody, x=>x, y=>y.rel_bone_index, (x,y)=>y.operation_type); //剛体リストから関連ボーンにIK対象先・IK制御下と同じボーンを持つ物を列挙し、剛体タイプを返す
				foreach (var operation_type in operation_types) {
					if (PMXFormat.Rigidbody.OperationType.Static != operation_type) {
						//ボーン追従で無い(≒物理演算)なら
						//IK制御の無効化
						result.ik_solver.enabled = false;
						break;
					}
				}
			}
			return result;
		}
		
		/// <summary>
		/// 剛体作成
		/// </summary>
		/// <returns>剛体</returns>
		GameObject[] CreateRigids()
		{
			if (!System.IO.Directory.Exists(System.IO.Path.Combine(format_.meta_header.folder, "Physics"))) {
				AssetDatabase.CreateFolder(format_.meta_header.folder, "Physics");
			}
			
			// 剛体の登録
			GameObject[] result = format_.rigidbody_list.rigidbody.Select(x=>ConvertRigidbody(x)).ToArray();
			for (uint i = 0, i_max = (uint)result.Length; i < i_max; ++i) {
				// マテリアルの設定
				result[i].collider.material = CreatePhysicMaterial(format_.rigidbody_list.rigidbody, i);
				
			}
			
			return result;
		}

		/// <summary>
		/// 剛体をUnity用に変換する
		/// </summary>
		/// <returns>Unity用剛体ゲームオブジェクト</returns>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		GameObject ConvertRigidbody(PMXFormat.Rigidbody rigidbody)
		{
			GameObject result = new GameObject("r" + rigidbody.name);
			//result.AddComponent<Rigidbody>();	// 1つのゲームオブジェクトに複数の剛体が付く事が有るので本体にはrigidbodyを適用しない
			
			//位置・回転の設定
			result.transform.position = rigidbody.collider_position;
			result.transform.rotation = Quaternion.Euler(rigidbody.collider_rotation * Mathf.Rad2Deg);
			
			// Colliderの設定
			switch (rigidbody.shape_type) {
			case PMXFormat.Rigidbody.ShapeType.Sphere:
				EntrySphereCollider(rigidbody, result);
				break;
			case PMXFormat.Rigidbody.ShapeType.Box:
				EntryBoxCollider(rigidbody, result);
				break;
			case PMXFormat.Rigidbody.ShapeType.Capsule:
				EntryCapsuleCollider(rigidbody, result);
				break;
			default:
				throw new System.ArgumentException();
			}
			return result;
		}
		
		/// <summary>
		/// Sphere Colliderの設定
		/// </summary>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='obj'>Unity用剛体ゲームオブジェクト</param>
		void EntrySphereCollider(PMXFormat.Rigidbody rigidbody, GameObject obj)
		{
			SphereCollider collider = obj.AddComponent<SphereCollider>();
			collider.radius = rigidbody.shape_size.x;
		}

		/// <summary>
		/// Box Colliderの設定
		/// </summary>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='obj'>Unity用剛体ゲームオブジェクト</param>
		void EntryBoxCollider(PMXFormat.Rigidbody rigidbody, GameObject obj)
		{
			BoxCollider collider = obj.AddComponent<BoxCollider>();
			collider.size = new Vector3(
				rigidbody.shape_size.x * 2.0f,
				rigidbody.shape_size.y * 2.0f, 
				rigidbody.shape_size.z * 2.0f);
		}

		/// <summary>
		/// Capsule Colliderの設定
		/// </summary>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='obj'>Unity用剛体ゲームオブジェクト</param>
		void EntryCapsuleCollider(PMXFormat.Rigidbody rigidbody, GameObject obj)
		{
			CapsuleCollider collider = obj.AddComponent<CapsuleCollider>();
			collider.radius = rigidbody.shape_size.x;
			collider.height = rigidbody.shape_size.y + rigidbody.shape_size.x * 2.0f;
		}

		/// <summary>
		/// 物理素材の作成
		/// </summary>
		/// <returns>物理素材</returns>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='index'>剛体インデックス</param>
		PhysicMaterial CreatePhysicMaterial(PMXFormat.Rigidbody[] rigidbodys, uint index)
		{
			PMXFormat.Rigidbody rigidbody = rigidbodys[index];
			PhysicMaterial material = new PhysicMaterial(format_.meta_header.name + "_r" + rigidbody.name);
			material.bounciness = rigidbody.recoil;
			material.staticFriction = rigidbody.friction;
			material.dynamicFriction = rigidbody.friction;

			AssetDatabase.CreateAsset(material, format_.meta_header.folder + "/Physics/" + index.ToString() + "_" + rigidbody.name + ".asset");
			return material;
		}

		/// <summary>
		/// 剛体とボーンを接続する
		/// </summary>
		/// <param name='bones'>ボーンのゲームオブジェクト</param>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		void AssignRigidbodyToBone(GameObject[] bones, GameObject[] rigids)
		{
			// 物理演算ルートを生成してルートの子供に付ける
			Transform physics_root_transform = (new GameObject("Physics", typeof(PhysicsManager))).transform;
			physics_root_transform.parent = root_game_object_.transform;
			
			// 剛体の数だけ回す
			for (uint i = 0, i_max = (uint)rigids.Length; i < i_max; ++i) {
				// 剛体を親ボーンに格納
				uint rel_bone_index = GetRelBoneIndexFromNearbyRigidbody(i);
				if (rel_bone_index < bones.Length) {
					//親と為るボーンが有れば
					//それの子と為る
					rigids[i].transform.parent = bones[rel_bone_index].transform;
				} else {
					//親と為るボーンが無ければ
					//物理演算ルートの子と為る
					rigids[i].transform.parent = physics_root_transform;
				}
			}
		}

		/// <summary>
		/// 剛体とボーンを接続する
		/// </summary>
		/// <param name='bones'>ボーンのゲームオブジェクト</param>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		uint GetRelBoneIndexFromNearbyRigidbody(uint rigidbody_index)
		{
			uint bone_count = (uint)format_.bone_list.bone.Length;
			//関連ボーンを探す
			uint result = format_.rigidbody_list.rigidbody[rigidbody_index].rel_bone_index;
			if (result < bone_count) {
				//関連ボーンが有れば
				return result;
			}
			//関連ボーンが無ければ
			//ジョイントに接続されている剛体の関連ボーンを探しに行く
			//HACK: 深さ優先探索に為っているけれど、関連ボーンとの類似性を考えれば幅優先探索の方が良いと思われる

			//ジョイントのAを探しに行く(自身はBに接続されている)
			var joint_a_list = format_.rigidbody_joint_list.joint.Where(x=>x.rigidbody_b == rigidbody_index) //自身がBに接続されているジョイントに絞る
																.Where(x=>x.rigidbody_a < bone_count) //Aが有効な剛体に縛る
																.Select(x=>x.rigidbody_a); //Aを返す
			foreach (var joint_a in joint_a_list) {
				result = GetRelBoneIndexFromNearbyRigidbody(joint_a);
				if (result < bone_count) {
					//関連ボーンが有れば
					return result;
				}
			}
			//ジョイントのAに無ければ
			//ジョイントのBを探しに行く(自身はAに接続されている)
			var joint_b_list = format_.rigidbody_joint_list.joint.Where(x=>x.rigidbody_a == rigidbody_index) //自身がAに接続されているジョイントに絞る
																.Where(x=>x.rigidbody_b < bone_count) //Bが有効な剛体に縛る
																.Select(x=>x.rigidbody_b); //Bを返す
			foreach (var joint_b in joint_b_list) {
				result = GetRelBoneIndexFromNearbyRigidbody(joint_b);
				if (result < bone_count) {
					//関連ボーンが有れば
					return result;
				}
			}
			//それでも無ければ
			//諦める
			result = uint.MaxValue;
			return result;
		}

		/// <summary>
		/// 剛体の値を設定する
		/// </summary>
		/// <param name='bones'>ボーンのゲームオブジェクト</param>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		void SetRigidsSettings(GameObject[] bones, GameObject[] rigid)
		{
			uint bone_count = (uint)format_.bone_list.bone.Length;
			for (uint i = 0, i_max = (uint)format_.rigidbody_list.rigidbody.Length; i < i_max; ++i) {
				PMXFormat.Rigidbody rigidbody = format_.rigidbody_list.rigidbody[i];
				GameObject target;
				if (rigidbody.rel_bone_index < bone_count) {
					//関連ボーンが有るなら
					//関連ボーンに付与する
					target = bones[rigidbody.rel_bone_index];
				} else {
					//関連ボーンが無いなら
					//剛体に付与する
					target = rigid[i];
				}
				UnityRigidbodySetting(rigidbody, target);
			}
		}

		/// <summary>
		/// Unity側のRigidbodyの設定を行う
		/// </summary>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='targetBone'>設定対象のゲームオブジェクト</param>
		void UnityRigidbodySetting(PMXFormat.Rigidbody pmx_rigidbody, GameObject target)
		{
			Rigidbody rigidbody = target.GetComponent<Rigidbody>();
			if (null != rigidbody) {
				//既にRigidbodyが付与されているなら
				//質量は合算する
				rigidbody.mass += pmx_rigidbody.weight;
				//減衰値は平均を取る
				rigidbody.drag = (rigidbody.drag + pmx_rigidbody.position_dim) * 0.5f;
				rigidbody.angularDrag = (rigidbody.angularDrag + pmx_rigidbody.rotation_dim) * 0.5f;
			} else {
				//まだRigidbodyが付与されていないなら
				rigidbody = target.AddComponent<Rigidbody>();
				rigidbody.isKinematic = (PMXFormat.Rigidbody.OperationType.Static == pmx_rigidbody.operation_type);
				rigidbody.mass = Mathf.Max(float.Epsilon, pmx_rigidbody.weight);
				rigidbody.drag = pmx_rigidbody.position_dim;
				rigidbody.angularDrag = pmx_rigidbody.rotation_dim;
			}
		}

		/// <summary>
		/// ジョイント作成
		/// </summary>
		/// <returns>ジョイントのゲームオブジェクト</returns>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		GameObject[] CreateJoints(GameObject[] rigids)
		{
			// ConfigurableJointの設定
			GameObject[] joints = SetupConfigurableJoint(rigids);
			return joints;
		}

		/// <summary>
		/// ConfigurableJointの設定
		/// </summary>
		/// <remarks>
		/// 先に設定してからFixedJointを設定する
		/// </remarks>
		/// <returns>ジョイントのゲームオブジェクト</returns>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		GameObject[] SetupConfigurableJoint(GameObject[] rigids)
		{
			List<GameObject> result_list = new List<GameObject>();
			foreach (PMXFormat.Joint joint in format_.rigidbody_joint_list.joint) {
				//相互接続する剛体の取得
				Transform transform_a = rigids[joint.rigidbody_a].transform;
				Rigidbody rigidbody_a = transform_a.rigidbody;
				if (null == rigidbody_a) {
					rigidbody_a = transform_a.parent.rigidbody;
				}
				Transform transform_b = rigids[joint.rigidbody_b].transform;
				Rigidbody rigidbody_b = transform_b.rigidbody;
				if (null == rigidbody_b) {
					rigidbody_b = transform_b.parent.rigidbody;
				}
				if (rigidbody_a != rigidbody_b) {
					//接続する剛体が同じ剛体を指さないなら
					//(本来ならPMDの設定が間違っていない限り同一を指す事は無い)
					//ジョイント設定
					ConfigurableJoint config_joint = rigidbody_b.gameObject.AddComponent<ConfigurableJoint>();
					config_joint.connectedBody = rigidbody_a;
					SetAttributeConfigurableJoint(joint, config_joint);
					
					result_list.Add(config_joint.gameObject);
				}
			}
			return result_list.ToArray();
		}

		/// <summary>
		/// ConfigurableJointの値を設定する
		/// </summary>
		/// <param name='joint'>PMX用ジョイントデータ</param>
		/// <param name='conf'>Unity用ジョイント</param>
		void SetAttributeConfigurableJoint(PMXFormat.Joint joint, ConfigurableJoint conf)
		{
			SetMotionAngularLock(joint, conf);
			SetDrive(joint, conf);
		}

		/// <summary>
		/// ジョイントに移動・回転制限のパラメータを設定する
		/// </summary>
		/// <param name='joint'>PMX用ジョイントデータ</param>
		/// <param name='conf'>Unity用ジョイント</param>
		void SetMotionAngularLock(PMXFormat.Joint joint, ConfigurableJoint conf)
		{
			SoftJointLimit jlim;

			// Motionの固定
			if (joint.constrain_pos_lower.x == 0.0f && joint.constrain_pos_upper.x == 0.0f) {
				conf.xMotion = ConfigurableJointMotion.Locked;
			} else {
				conf.xMotion = ConfigurableJointMotion.Limited;
			}

			if (joint.constrain_pos_lower.y == 0.0f && joint.constrain_pos_upper.y == 0.0f) {
				conf.yMotion = ConfigurableJointMotion.Locked;
			} else {
				conf.yMotion = ConfigurableJointMotion.Limited;
			}

			if (joint.constrain_pos_lower.z == 0.0f && joint.constrain_pos_upper.z == 0.0f) {
				conf.zMotion = ConfigurableJointMotion.Locked;
			} else {
				conf.zMotion = ConfigurableJointMotion.Limited;
			}

			// 角度の固定
			if (joint.constrain_rot_lower.x == 0.0f && joint.constrain_rot_upper.x == 0.0f) {
				conf.angularXMotion = ConfigurableJointMotion.Locked;
			} else {
				conf.angularXMotion = ConfigurableJointMotion.Limited;
				float hlim = Mathf.Max(-joint.constrain_rot_lower.x, -joint.constrain_rot_upper.x); //回転方向が逆なので負数
				float llim = Mathf.Min(-joint.constrain_rot_lower.x, -joint.constrain_rot_upper.x);
				SoftJointLimit jhlim = new SoftJointLimit();
				jhlim.limit = Mathf.Clamp(hlim * Mathf.Rad2Deg, -180.0f, 180.0f);
				conf.highAngularXLimit = jhlim;

				SoftJointLimit jllim = new SoftJointLimit();
				jllim.limit = Mathf.Clamp(llim * Mathf.Rad2Deg, -180.0f, 180.0f);
				conf.lowAngularXLimit = jllim;
			}

			if (joint.constrain_rot_lower.y == 0.0f && joint.constrain_rot_upper.y == 0.0f) {
				conf.angularYMotion = ConfigurableJointMotion.Locked;
			} else {
				// 値がマイナスだとエラーが出るので注意
				conf.angularYMotion = ConfigurableJointMotion.Limited;
				float lim = Mathf.Min(Mathf.Abs(joint.constrain_rot_lower.y), Mathf.Abs(joint.constrain_rot_upper.y));//絶対値の小さい方
				jlim = new SoftJointLimit();
				jlim.limit = lim * Mathf.Clamp(Mathf.Rad2Deg, 0.0f, 180.0f);
				conf.angularYLimit = jlim;
			}

			if (joint.constrain_rot_lower.z == 0f && joint.constrain_rot_upper.z == 0f) {
				conf.angularZMotion = ConfigurableJointMotion.Locked;
			} else {
				conf.angularZMotion = ConfigurableJointMotion.Limited;
				float lim = Mathf.Min(Mathf.Abs(-joint.constrain_rot_lower.z), Mathf.Abs(-joint.constrain_rot_upper.z));//絶対値の小さい方//回転方向が逆なので負数
				jlim = new SoftJointLimit();
				jlim.limit = Mathf.Clamp(lim * Mathf.Rad2Deg, 0.0f, 180.0f);
				conf.angularZLimit = jlim;
			}
		}

		/// <summary>
		/// ジョイントにばねなどのパラメータを設定する
		/// </summary>
		/// <param name='joint'>PMX用ジョイントデータ</param>
		/// <param name='conf'>Unity用ジョイント</param>
		void SetDrive(PMXFormat.Joint joint, ConfigurableJoint conf)
		{
			JointDrive drive;

			// Position
			if (joint.spring_position.x != 0.0f) {
				drive = new JointDrive();
				drive.positionSpring = joint.spring_position.x;
				conf.xDrive = drive;
			}
			if (joint.spring_position.y != 0.0f) {
				drive = new JointDrive();
				drive.positionSpring = joint.spring_position.y;
				conf.yDrive = drive;
			}
			if (joint.spring_position.z != 0.0f) {
				drive = new JointDrive();
				drive.positionSpring = joint.spring_position.z;
				conf.zDrive = drive;
			}

			// Angular
			if (joint.spring_rotation.x != 0.0f) {
				drive = new JointDrive();
				drive.mode = JointDriveMode.PositionAndVelocity;
				drive.positionSpring = joint.spring_rotation.x;
				conf.angularXDrive = drive;
			}
			if (joint.spring_rotation.y != 0.0f || joint.spring_rotation.z != 0.0f) {
				drive = new JointDrive();
				drive.mode = JointDriveMode.PositionAndVelocity;
				drive.positionSpring = (joint.spring_rotation.y + joint.spring_rotation.z) * 0.5f;
				conf.angularYZDrive = drive;
			}
		}

		/// <summary>
		/// 剛体のグローバル座標化
		/// </summary>
		/// <param name='joints'>ジョイントのゲームオブジェクト</param>
		void GlobalizeRigidbody(GameObject[] joints)
		{
			Transform physics_root_transform = root_game_object_.transform.Find("Physics");
			PhysicsManager physics_manager = physics_root_transform.gameObject.GetComponent<PhysicsManager>();

			if ((null != joints) && (0 < joints.Length)) {
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

		/// <summary>
		/// 非衝突剛体の設定
		/// </summary>
		/// <returns>非衝突剛体のリスト</returns>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		List<int>[] SettingIgnoreRigidGroups(GameObject[] rigids)
		{
			// 非衝突グループ用リストの初期化
			const int MaxGroup = 16;	// グループの最大数
			List<int>[] result = new List<int>[MaxGroup];
			for (int i = 0, i_max = MaxGroup; i < i_max; ++i) {
				result[i] = new List<int>();
			}

			// それぞれの剛体が所属している非衝突グループを追加していく
			for (int i = 0, i_max = format_.rigidbody_list.rigidbody.Length; i < i_max; ++i) {
				result[format_.rigidbody_list.rigidbody[i].group_index].Add(i);
			}
			return result;
		}

		/// <summary>
		/// グループターゲットの取得
		/// </summary>
		/// <returns>グループターゲット</returns>
		/// <param name='rigids'>剛体のゲームオブジェクト</param>
		int[] GetRigidbodyGroupTargets(GameObject[] rigids)
		{
			return format_.rigidbody_list.rigidbody.Select(x=>(int)x.ignore_collision_group).ToArray();
		}
	
		GameObject	root_game_object_;
		PMXFormat	format_;
		bool		use_rigidbody_;
		bool		use_mecanim_;
		bool		use_ik_;
	}
}
