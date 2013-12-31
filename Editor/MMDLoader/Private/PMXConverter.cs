using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MMD.PMX;

namespace MMD
{
	public class PMXConverter : System.IDisposable
	{
		/// <summary>
		/// アニメーションタイプ
		/// </summary>
		public enum AnimationType {
			GenericMecanim,		//汎用アバターでのMecanim
			HumanMecanim,		//人型アバターでのMecanim
			LegacyAnimation,	//旧式アニメーション
		}
		
		/// <summary>
		/// GameObjectを作成する
		/// </summary>
		/// <param name='format'>内部形式データ</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='animation_type'>アニメーションタイプ</param>
		/// <param name='use_ik'>IKを使用するか</param>
		/// <param name='scale'>スケール</param>
		public static GameObject CreateGameObject(PMXFormat format, bool use_rigidbody, AnimationType animation_type, bool use_ik, float scale) {
			GameObject result;
			using (PMXConverter converter = new PMXConverter()) {
				result = converter.CreateGameObject_(format, use_rigidbody, animation_type, use_ik, scale);
			}
			return result;
		}

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		/// <remarks>
		/// ユーザーに依るインスタンス作成を禁止する
		/// </remarks>
		private PMXConverter() {}

		/// <summary>
		/// Disposeインターフェース
		/// </summary>
		public void Dispose()
		{
			if (null != alpha_readable_texture_) {
				alpha_readable_texture_.Dispose();
			}
		}
		
		/// <summary>
		/// GameObjectを作成する
		/// </summary>
		/// <param name='format'>内部形式データ</param>
		/// <param name='use_rigidbody'>剛体を使用するか</param>
		/// <param name='animation_type'>アニメーションタイプ</param>
		/// <param name='use_ik'>IKを使用するか</param>
		/// <param name='scale'>スケール</param>
		private GameObject CreateGameObject_(PMXFormat format, bool use_rigidbody, AnimationType animation_type, bool use_ik, float scale) {
			format_ = format;
			use_ik_ = use_ik;
			scale_ = scale;
			root_game_object_ = new GameObject(format_.meta_header.name);
			MMDEngine engine = root_game_object_.AddComponent<MMDEngine>(); //MMDEngine追加
			//スケール・エッジ幅
			engine.scale = scale_;
			engine.outline_width = 1.0f;
			engine.material_outline_widths = format.material_list.material.Select(x=>x.edge_size).ToArray();
			engine.enable_render_queue = false; //初期値無効
			const int c_render_queue_transparent = 3000;
			engine.render_queue_value = c_render_queue_transparent;
			
			MeshCreationInfo[] creation_info = CreateMeshCreationInfo();				// メッシュを作成する為の情報を作成
			Mesh[] mesh = CreateMesh(creation_info);									// メッシュの生成・設定
			Material[][] materials = CreateMaterials(creation_info);					// マテリアルの生成・設定
			GameObject[] bones = CreateBones();											// ボーンの生成・設定
			SkinnedMeshRenderer[] renderers = BuildingBindpose(mesh, materials, bones);	// バインドポーズの作成
			CreateMorph(mesh, materials, bones, renderers, creation_info);				// モーフの生成・設定
			
	
			// BoneController・IKの登録(use_ik_を使った判定はEntryBoneController()の中で行う)
			{
				engine.bone_controllers = EntryBoneController(bones);
				engine.ik_list = engine.bone_controllers.Where(x=>null != x.ik_solver)
														.Select(x=>x.ik_solver)
														.ToArray();
			}
	
			// 剛体関連
			if (use_rigidbody) {
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
	
			// Mecanim設定
			if (AnimationType.LegacyAnimation != animation_type) {
				//アニメーター追加
				AvatarSettingScript avatar_setting = new AvatarSettingScript(root_game_object_, bones);
				switch (animation_type) {
				case AnimationType.GenericMecanim: //汎用アバターでのMecanim
					avatar_setting.SettingGenericAvatar();
					break;
				case AnimationType.HumanMecanim: //人型アバターでのMecanim
					avatar_setting.SettingHumanAvatar();
					break;
				default:
					throw new System.ArgumentException();
				}
				
				string path = format_.meta_header.folder + "/";
				string name = GetFilePathString(format_.meta_header.name);
				string file_name = path + name + ".avatar.asset";
				avatar_setting.CreateAsset(file_name);
			} else {
				root_game_object_.AddComponent<Animation>();	// アニメーション追加
			}

			return root_game_object_;
		}
		
		/// <summary>
		/// メッシュを作成する時に参照するデータの纏め
		/// </summary>
		class MeshCreationInfo {
			public class Pack {
				public uint		material_index;	//マテリアル
				public uint[]	plane_indices;	//面
				public uint[]	vertices;		//頂点
			}
			public Pack[]					value;
			public uint[]					all_vertices;			//総頂点
			public Dictionary<uint, uint>	reassign_dictionary;	//頂点リアサインインデックス用辞書
		}
		
		/// <summary>
		/// メッシュを作成する為の情報を作成
		/// </summary>
		/// <returns>メッシュ作成情報</returns>
		MeshCreationInfo[] CreateMeshCreationInfo()
		{
			// 1メッシュで収まる場合でも-Multi()を使っても問題は起き無いが、
			// -Multi()では頂点数計測をマテリアル単位で行う関係上、頂点数が多く見積もられる(概算値)。
			// (1頂点を複数のマテリアルが参照している場合に参照している分だけ計上してしまう。)
			// 依って上限付近では本来1メッシュで収まる物が複数メッシュに分割されてしまう事が有るので注意。
			// 
			// -Multi()を使っても最終的には頂点数を最適化するので、
			// 1メッシュに収まってしまえば-Single()と同じ頂点数に為る(確定値)。
			// 
			// 単純に-Single()の方が解析が少ない分早い。

			MeshCreationInfo[] result;
			if (format_.vertex_list.vertex.Length < c_max_vertex_count_in_mesh) {
				//1メッシュで収まるなら
				result = CreateMeshCreationInfoSingle();
			} else {
				//1メッシュで収まらず、複数メッシュに分割するなら
				result = CreateMeshCreationInfoMulti();
			}
			return result;
		}

		/// <summary>
		/// メッシュを作成する為の情報を作成(単体メッシュ版)
		/// </summary>
		/// <returns>メッシュ作成情報</returns>
		MeshCreationInfo[] CreateMeshCreationInfoSingle()
		{
			MeshCreationInfo[] result = new[]{new MeshCreationInfo()};
			//全マテリアルを設定
			result[0].value = CreateMeshCreationInfoPacks();
			//全頂点を設定
			result[0].all_vertices = Enumerable.Range(0, format_.vertex_list.vertex.Length).Select(x=>(uint)x).ToArray();
			//頂点リアサインインデックス用辞書作成
			result[0].reassign_dictionary = new Dictionary<uint, uint>(result[0].all_vertices.Length);
			for (uint i = 0, i_max = (uint)result[0].all_vertices.Length; i < i_max; ++i) {
				result[0].reassign_dictionary[i] = i;
			}
			return result;
		}

		/// <summary>
		/// 全マテリアルをメッシュ作成情報のマテリアルパックとして返す
		/// </summary>
		/// <returns>メッシュ作成情報のマテリアルパック</returns>
		MeshCreationInfo.Pack[] CreateMeshCreationInfoPacks()
		{
			uint plane_start = 0;
			//マテリアル単位のMeshCreationInfo.Packを作成する
			return Enumerable.Range(0, format_.material_list.material.Length)
							.Select(x=>{
										MeshCreationInfo.Pack pack = new MeshCreationInfo.Pack();
										pack.material_index = (uint)x;
										uint plane_count = format_.material_list.material[x].face_vert_count;
										pack.plane_indices = format_.face_vertex_list.face_vert_index.Skip((int)plane_start)
																											.Take((int)plane_count)
																											.ToArray();
										pack.vertices = pack.plane_indices.Distinct() //重複削除
																			.ToArray();
										plane_start += plane_count;
										return pack;
									})
							.ToArray();
		}

		/// <summary>
		/// メッシュを作成する為の情報を作成(複数メッシュ版)
		/// </summary>
		/// <returns>メッシュ作成情報</returns>
		MeshCreationInfo[] CreateMeshCreationInfoMulti()
		{
			//マテリアル単位のMeshCreationInfo.Packを作成する
			MeshCreationInfo.Pack[] packs = CreateMeshCreationInfoPacks();
			//マテリアル細分化
			packs = SplitSubMesh(packs);
			//頂点数の多い順に並べる(メッシュ分割アルゴリズム上、後半に行く程頂点数が少ない方が敷き詰め効率が良い)
			System.Array.Sort(packs, (x,y)=>y.vertices.Length - x.vertices.Length);
			
			List<MeshCreationInfo> result = new List<MeshCreationInfo>();
			do {
				uint vertex_sum = 0;
				MeshCreationInfo info = new MeshCreationInfo();
				//マテリアルパック作成
				info.value = Enumerable.Range(0, packs.Length)
										.Where(x=>null!=packs[x]) //有効なマテリアルに絞る
										.Where(x=>{	//採用しても頂点数が限界を超えないなら
													vertex_sum += (uint)packs[x].vertices.Length;
													return vertex_sum < c_max_vertex_count_in_mesh;
												})
										.Select(x=>{	//マテリアルの採用と無効化
													var pack = packs[x];
													packs[x] = null;
													return pack;
												})
										.ToArray();
				//マテリアルインデックスに並べる(メッシュの選定が終わったので見易い様に並びを戻す)
				System.Array.Sort(info.value, (x,y)=>((x.material_index>y.material_index)? 1: (x.material_index<y.material_index)? -1: 0));
				//総頂点作成
				info.all_vertices = info.value.SelectMany(x=>x.vertices).Distinct().ToArray();
				System.Array.Sort(info.all_vertices);
				//頂点リアサインインデックス用辞書作成
				info.reassign_dictionary = new Dictionary<uint, uint>();
				uint reassign_index = 0;
				foreach (var i in info.all_vertices) {
					info.reassign_dictionary[i] = reassign_index++;
				}
				//戻り値に追加
				result.Add(info);
			} while (packs.Any(x=>null!=x)); //使用していないマテリアルが為るならループ
			return result.ToArray();
		}
		
		/// <summary>
		/// 1マテリアルの頂点数が1メッシュで表現出来ない場合に分割する
		/// </summary>
		/// <returns>メッシュ作成情報のマテリアルパック</returns>
		/// <param name='creation_infos'>メッシュ作成情報のマテリアルパック</param>
		MeshCreationInfo.Pack[] SplitSubMesh(MeshCreationInfo.Pack[] packs)
		{
			MeshCreationInfo.Pack[] result = packs;
			if (packs.Any(x=>c_max_vertex_count_in_mesh<=x.vertices.Length)) {
				//1メッシュに収まらないマテリアルが有るなら
				List<MeshCreationInfo.Pack> result_list = new List<MeshCreationInfo.Pack>();
				foreach (var pack in packs) {
					if (c_max_vertex_count_in_mesh <= pack.vertices.Length) {
						//1メッシュに収まらないなら
						//分離
						var split_pack = SplitSubMesh(pack);
						foreach (var i in split_pack) {
							result_list.Add(i);
						}
					} else {
						//1メッシュに収まるなら
						//素通し
						result_list.Add(pack);
					}
				}
				result = result_list.ToArray();
			}
			return result;
		}
		
		/// <summary>
		/// 1マテリアルの頂点数が1メッシュで表現出来ないので分割する
		/// </summary>
		/// <returns>メッシュ作成情報のマテリアルパック</returns>
		/// <param name='creation_infos'>メッシュ作成情報のマテリアルパック</param>
		List<MeshCreationInfo.Pack> SplitSubMesh(MeshCreationInfo.Pack pack)
		{
			List<MeshCreationInfo.Pack> result = new List<MeshCreationInfo.Pack>();
			//1メッシュに収まらないなら
			uint plane_end = (uint)pack.plane_indices.Length;
			uint plane_start = 0;
			while (plane_start < plane_end) {
				//まだ面が有るなら
				uint plane_count = 0;
				uint vertex_count = 0;
				while (true) {
					//現在の頂点数から考えると、余裕分の1/3迄の数の面は安定して入る
					//はみ出て欲しいから更に1面(3頂点)を足す
					plane_count += (c_max_vertex_count_in_mesh - vertex_count) / 3 * 3 + 3;
					vertex_count = (uint)pack.plane_indices.Skip((int)plane_start)	//面頂点インデックス取り出し(先頭)
															.Take((int)plane_count)	//面頂点インデックス取り出し(末尾)
															.Distinct()				//重複削除
															.Count();				//個数取得
					if (c_max_vertex_count_in_mesh <= vertex_count) {
						//1メッシュを超えているなら
						//此処でのメッシュ超えは必ずc_max_vertex_count_in_meshぎりぎりで有り、1面(3頂点)を1つ取れば収まる様になっている
						plane_count -= 3;
						break;
					}
					if (plane_end <= (plane_start + plane_count)) {
						//面の最後なら
						break;
					}
				}
				//分離分を戻り値の追加
				MeshCreationInfo.Pack result_pack = new MeshCreationInfo.Pack();;
				result_pack.material_index = pack.material_index;
				result_pack.plane_indices = pack.plane_indices.Skip((int)plane_start)	//面頂点インデックス取り出し(先頭)
																.Take((int)plane_count)	//面頂点インデックス取り出し(末尾)
																.ToArray();
				result_pack.vertices = result_pack.plane_indices.Distinct()	//重複削除
																.ToArray();
				result.Add(result_pack);
				//開始点を後ろに
				plane_start += plane_count;
			}
			return result;
		}
		
		/// <summary>
		/// メッシュ作成
		/// </summary>
		/// <returns>メッシュ</returns>
		/// <param name='creation_info'>メッシュ作成情報</param>
		Mesh[] CreateMesh(MeshCreationInfo[] creation_info)
		{
			Mesh[] result = new Mesh[creation_info.Length];
			for (int i = 0, i_max = creation_info.Length; i < i_max; ++i) {
				Mesh mesh = new Mesh();
				EntryAttributesForMesh(mesh, creation_info[i]);
				SetSubMesh(mesh, creation_info[i]);
				CreateAssetForMesh(mesh, i);
				result[i] = mesh;
			}
			return result;
		}
		
		/// <summary>
		/// メッシュに基本情報(頂点座標・法線・UV・ボーンウェイト)を登録する
		/// </summary>
		/// <param name='mesh'>対象メッシュ</param>
		/// <param name='creation_info'>メッシュ作成情報</param>
		void EntryAttributesForMesh(Mesh mesh, MeshCreationInfo creation_info)
		{
			mesh.vertices = creation_info.all_vertices.Select(x=>format_.vertex_list.vertex[x].pos * scale_).ToArray();
			mesh.normals = creation_info.all_vertices.Select(x=>format_.vertex_list.vertex[x].normal_vec).ToArray();
			mesh.uv = creation_info.all_vertices.Select(x=>format_.vertex_list.vertex[x].uv).ToArray();
			if (0 < format_.header.additionalUV) {
				//追加UVが1つ以上有れば
				//1つ目のみ登録
				mesh.uv2 = creation_info.all_vertices.Select(x=>new Vector2(format_.vertex_list.vertex[x].add_uv[0].x, format_.vertex_list.vertex[x].add_uv[0].y)).ToArray();
			}
			mesh.boneWeights = creation_info.all_vertices.Select(x=>ConvertBoneWeight(format_.vertex_list.vertex[x].bone_weight)).ToArray();
			mesh.colors = creation_info.all_vertices.Select(x=>new Color(0.0f, 0.0f, 0.0f, format_.vertex_list.vertex[x].edge_magnification * 0.25f)).ToArray(); //不透明度にエッジ倍率を0.25倍した情報を仕込む(0～8迄は表せる)
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
		/// <param name='creation_info'>メッシュ作成情報</param>
		void SetSubMesh(Mesh mesh, MeshCreationInfo creation_info)
		{
			// マテリアル対サブメッシュ
			// サブメッシュとはマテリアルに適用したい面頂点データのこと
			// 面ごとに設定するマテリアルはここ
			mesh.subMeshCount = creation_info.value.Length;
			for (int i = 0, i_max = creation_info.value.Length; i < i_max; ++i) {
				//format_.face_vertex_list.face_vert_indexを[start](含む)から[start+count](含まず)迄取り出し
				int[] indices = creation_info.value[i].plane_indices.Select(x=>(int)creation_info.reassign_dictionary[x]) //頂点リアサインインデックス変換
																	.ToArray();
				mesh.SetTriangles(indices, i);
			}
		}
		
		/// <summary>
		/// メッシュをProjectに登録する
		/// </summary>
		/// <param name='mesh'>対象メッシュ</param>
		/// <param name='index'>メッシュインデックス</param>
		void CreateAssetForMesh(Mesh mesh, int index)
		{
			string path = format_.meta_header.folder + "/Meshes/";
			if (!System.IO.Directory.Exists(path)) { 
				AssetDatabase.CreateFolder(format_.meta_header.folder, "Meshes");
			}
			
			string name = GetFilePathString(format_.meta_header.name);
			string file_name = path + index.ToString() + "_" + name + ".asset";
			AssetDatabase.CreateAsset(mesh, file_name);
		}
		
		/// <summary>
		/// マテリアル作成
		/// </summary>
		/// <returns>マテリアル</returns>
		/// <param name='creation_info'>メッシュ作成情報</param>
		Material[][] CreateMaterials(MeshCreationInfo[] creation_info)
		{
			// 適当なフォルダに投げる
			string path = format_.meta_header.folder + "/Materials/";
			if (!System.IO.Directory.Exists(path)) { 
				AssetDatabase.CreateFolder(format_.meta_header.folder, "Materials");
			}
			
			//全マテリアルを作成
			Material[] materials = EntryAttributesForMaterials();
			CreateAssetForMaterials(materials);

			//メッシュ単位へ振り分け
			Material[][] result = new Material[creation_info.Length][];
			for (int i = 0, i_max = creation_info.Length; i < i_max; ++i) {
				result[i] = creation_info[i].value.Select(x=>materials[x.material_index]).ToArray();
			}

			return result;
		}

		/// <summary>
		/// マテリアルに基本情報(シェーダー・カラー・テクスチャ)を登録する
		/// </summary>
		/// <returns>マテリアル</returns>
		Material[] EntryAttributesForMaterials()
		{
			//材質モーフが透過を要望するか
			bool[] is_transparent_by_material = IsTransparentByMaterial(); //材質
			bool[] is_transparent_by_material_morph = IsTransparentByMaterialMorph(); //材質モーフ
			bool[] is_transparent_by_texture_alpha = IsTransparentByTextureAlpha(); //テクスチャのアルファ値(UV考慮済み)
			
			return Enumerable.Range(0, format_.material_list.material.Length)
							.Select(x=>new {material_index = (uint)x
											, is_transparent = is_transparent_by_material[x] || is_transparent_by_material_morph[x] || is_transparent_by_texture_alpha[x]
											}
									)
							.Select(x=>ConvertMaterial(x.material_index, x.is_transparent))
							.ToArray();
		}
		
		/// <summary>
		/// 材質に依る透過確認
		/// </summary>
		/// <returns>透過かの配列(true:透過, false:不透明)</returns>
		bool[] IsTransparentByMaterial()
		{
			//拡散色とエッジ色の透過確認
			bool[] result = format_.material_list.material.Select(x=>(x.diffuse_color.a < 1.0f) || (x.edge_color.a < 1.0f))
															.ToArray();
			return result;
		}
		
		/// <summary>
		/// 材質モーフに依る透過確認
		/// </summary>
		/// <returns>透過かの配列(true:透過, false:不透明)</returns>
		bool[] IsTransparentByMaterialMorph()
		{
			bool[] result = Enumerable.Repeat(false, format_.material_list.material.Length)
										.ToArray();
			var transparent_material_indices = format_.morph_list.morph_data.Where(x=>PMXFormat.MorphData.MorphType.Material==x.morph_type) //材質モーフなら
																			.SelectMany(x=>x.morph_offset) //材質モーフオフセット取得
																			.Select(x=>(PMXFormat.MaterialMorphOffset)x) //材質モーフオフセットにキャスト
																			.Where(x=>(PMXFormat.MaterialMorphOffset.OffsetMethod.Mul==x.offset_method)&&((x.diffuse.a < 1.0f)||(x.edge_color.a < 1.0f))) //拡散色かエッジ色が透過に為るなら
																			.Select(x=>x.material_index) //マテリアルインデックス取得
																			.Distinct(); //重複除去
			foreach (uint material_index in transparent_material_indices) {
				//材質モーフに依って透過が要望されているなら
				//透過扱いにする
				if (material_index < (uint)format_.material_list.material.Length) {
					//単体モーフのマテリアルインデックスなら
					//対象マテリアルだけ透過扱い
					result[material_index] = true;
				} else {
					//全対象モーフのマテリアルインデックスなら
					//全て透過扱い
					result = Enumerable.Repeat(true, result.Length).ToArray();
					break;
				}
			}
			return result;
		}
		
		/// <summary>
		/// テクスチャのアルファ値に依る透過確認
		/// </summary>
		/// <returns>透過かの配列(true:透過, false:不透明)</returns>
		bool[] IsTransparentByTextureAlpha()
		{
			Texture2D[] textures = GetTextureList();
			Vector2[][] uvs = GetUvList();
			bool[] result = Enumerable.Range(0, format_.material_list.material.Length)
										.Select(x=>((null != textures[x])
													? IsTransparentByTextureAlphaWithUv(textures[x], uvs[x])
													: false
												))
										.ToArray();
			return result;
		}

		/// <summary>
		/// テクスチャの取得
		/// </summary>
		/// <returns>テクスチャ配列</returns>
		Texture2D[] GetTextureList()
		{
			string[] texture_path = format_.material_list.material.Select(x=>x.usually_texture_index) //材質が使用しているテクスチャインデックスを取得
																	.Select(x=>((x<format_.texture_list.texture_file.Length)? format_.texture_list.texture_file[x]: null)) //有効なテクスチャインデックスならパスの取得
																	.ToArray();
			alpha_readable_texture_ = new AlphaReadableTexture(texture_path
															, format_.meta_header.folder + "/"
															, format_.meta_header.folder + "/Materials/"
															);
			return alpha_readable_texture_.textures;
		}
		
		/// <summary>
		/// UVの取得
		/// </summary>
		/// <returns>UV配列</returns>
		/// <remarks>
		/// UVモーフにて改変される場合は未適応(0.0f)と全適応(1.0f)の2段階のみを扱い、中間適応は考慮しない。
		/// 複数のUVモーフが同一頂点に掛かる場合に多重適応すると単体では参照出来無い領域迄参照出来る様に為るが、これは考慮しない。
		/// 同様にグループモーフに依る1.0f超えも考慮しない。
		/// </remarks>
		Vector2[][] GetUvList()
		{
			uint[][] vertex_list = CreateMeshCreationInfoPacks().Select(x=>x.plane_indices).ToArray();
			
			Dictionary<uint, Vector4>[] uv_morphs = format_.morph_list.morph_data
															.Where(x=>PMXFormat.MorphData.MorphType.Uv==x.morph_type) //UVモーフなら
															.Select(x=>x.morph_offset.Select(y=>(PMXFormat.UVMorphOffset)y)
																					.ToDictionary(z=>z.vertex_index, z=>z.uv_offset) //頂点インデックスでディクショナリ化
																	) //UVモーフオフセット取得
															.ToArray();

			List<Vector2>[] result = vertex_list.Select(x=>x.Select(y=>format_.vertex_list.vertex[y].uv).ToList()).ToArray();
			
			//材質走査
			bool is_cancel = false;
			for (int material_index = 0, material_index_max = result.Length; material_index < material_index_max; ++material_index) {
				//UVモーフ走査
				for (int uv_morph_index = 0, uv_morph_index_max = uv_morphs.Length; uv_morph_index < uv_morph_index_max; ++uv_morph_index) {
					var uv_morph = uv_morphs[uv_morph_index];
					//ブログレスパー更新
					is_cancel = EditorUtility.DisplayCancelableProgressBar("Create UV Area Infomation"
																			, "Material:[" + material_index + "|" + material_index_max + "]"
																				+ format_.material_list.material[material_index].name
																				+ "\t"
																				+ "UV Morph:[" + uv_morph_index + "|" + uv_morph_index_max + "]"
																				+ format_.morph_list.morph_data.Where(x=>PMXFormat.MorphData.MorphType.Uv==x.morph_type).Skip(uv_morph_index).First().morph_name
																			, ((((float)uv_morph_index / (float)uv_morph_index_max) + (float)material_index) / (float)material_index_max)
																			);
					if (is_cancel) {
						break;
					}

					//先行UVモーフ対象確認(三角形構成を無視して全頂点をUVモーフ参照)
					var vertex_dictionary = vertex_list[material_index].Distinct().ToDictionary(x=>x, x=>true); //(UVモーフに設定されている頂点数依りも三角形構成頂点の方が多いと思うので、そちら側をlogNにする為に辞書作成)
					if (uv_morph.Keys.Any(x=>vertex_dictionary.ContainsKey(x))) {
						//UVモーフ対象なら
						//頂点走査(三角形構成頂点走査)
						for (int vertex_index = 0, vertex_index_max = vertex_list[material_index].Length; vertex_index < vertex_index_max; vertex_index+=3) {
							//三角形構成頂点インデックス取り出し
							uint[] tri_vertices = new []{vertex_list[material_index][vertex_index+0]
														, vertex_list[material_index][vertex_index+1]
														, vertex_list[material_index][vertex_index+2]
														};
							//UVモーフ対象確認
							if (tri_vertices.Any(x=>uv_morph.ContainsKey(x))) {
								//UVモーフ対象なら
								//適応したUV値を作成
								var tri_uv = tri_vertices.Select(x=>new{original_uv = format_.vertex_list.vertex[x].uv
																		, add_uv = ((uv_morph.ContainsKey(x))? uv_morph[x]: Vector4.zero)
																		}
																)
														.Select(x=>new Vector2(x.original_uv.x + x.add_uv.x, x.original_uv.y + x.add_uv.y));
								//追加
								result[material_index].AddRange(tri_uv);
							}
						}
					}
				}
				if (is_cancel) {
					break;
				}
			}
			EditorUtility.ClearProgressBar();

			return result.Select(x=>x.ToArray()).ToArray();
		}
		
		/// <summary>
		/// UV値を考慮した、テクスチャのアルファ値に依る透過確認
		/// </summary>
		/// <returns>透過か(true:透過, false:不透明)</returns>
		/// <param name="texture">テクスチャ</param>
		/// <param name="uvs">UV値(3つ単位で三角形を構成する)</param>
		static bool IsTransparentByTextureAlphaWithUv(Texture2D texture, Vector2[] uvs)
		{
			bool result = true;
			if (TextureFormat.Alpha8 == texture.format) {
				//ファイルがDDS以外なら(AlphaReadableTextureDirectoryImporterに依ってDDS以外はAlpha8に為る)
				//alphaIsTransparencyを確認する
				result = texture.alphaIsTransparency; //アルファ値を持たないなら透過フラグが立っていない
			}
			if (result) {
				//アルファ値を持つなら
				//詳細確認
				result = Enumerable.Range(0, uvs.Length / 3) //3つ単位で取り出す為の元インデックス
										.Select(x=>x*3) //3つ間隔に変換
										.Any(x=>IsTransparentByTextureAlphaWithUv(texture, uvs[x+0],uvs[x+1],uvs[x+2])); //三角形を透過確認、どれかが透過していたら透過とする
			}
			return result;
		}
		
		/// <summary>
		/// UV値を考慮した、テクスチャのアルファ値に依る透過確認
		/// </summary>
		/// <returns>透過か(true:透過, false:不透明)</returns>
		/// <param name="texture">テクスチャ</param>
		/// <param name="uv1">三角形頂点のUV値</param>
		/// <param name="uv2">三角形頂点のUV値</param>
		/// <param name="uv3">三角形頂点のUV値</param>
		/// <remarks>
		/// 理想ならば全テクセルを確認しなければならないが、
		/// 現在の実装では三角形を構成する各頂点のUV・重心・各辺の中心点の7点のテクセルしか確認していない
		/// </remarks>
		static bool IsTransparentByTextureAlphaWithUv(Texture2D texture, Vector2 uv1, Vector2 uv2, Vector2 uv3)
		{
			bool result = true; //透過
			do {
				//座標系が相違しているので補正
				uv1.Set(uv1.x, 1.0f - uv1.y - (1.0f / texture.height));
				uv2.Set(uv2.x, 1.0f - uv2.y - (1.0f / texture.height));
				uv3.Set(uv3.x, 1.0f - uv3.y - (1.0f / texture.height));
				
				const float c_threshold = 253.0f / 255.0f; //253程度迄は不透明として見逃す

				//頂点直下
				if (texture.GetPixelBilinear(uv1.x, uv1.y).a < c_threshold) {
					break;
				}
				if (texture.GetPixelBilinear(uv2.x, uv2.y).a < c_threshold) {
					break;
				}
				if (texture.GetPixelBilinear(uv3.x, uv3.y).a < c_threshold) {
					break;
				}

				//重心
				Vector2 center = new Vector2((uv1.x + uv2.x + uv3.x) / 3.0f, (uv1.y + uv2.y + uv3.y) / 3.0f);
				if (texture.GetPixelBilinear(center.x, center.y).a < c_threshold) {
					break;
				}

				//辺中央
				Vector2 uv12 = new Vector2((uv1.x + uv2.x) / 2.0f, (uv1.y + uv2.y) / 2.0f);
				if (texture.GetPixelBilinear(uv12.x, uv12.y).a < c_threshold) {
					break;
				}
				Vector2 uv23 = new Vector2((uv2.x + uv3.x) / 2.0f, (uv2.y + uv3.y) / 2.0f);
				if (texture.GetPixelBilinear(uv23.x, uv23.y).a < c_threshold) {
					break;
				}
				Vector2 uv31 = new Vector2((uv3.x + uv1.x) / 2.0f, (uv3.y + uv1.y) / 2.0f);
				if (texture.GetPixelBilinear(uv31.x, uv31.y).a < c_threshold) {
					break;
				}

				//此処迄来たら不透明
				result = false;
			} while(false);
			return result;
		}
		
		/// <summary>
		/// マテリアルをUnity用に変換する
		/// </summary>
		/// <returns>Unity用マテリアル</returns>
		/// <param name='material_index'>PMX用マテリアルインデックス</param>
		/// <param name='is_transparent'>透過か</param>
		Material ConvertMaterial(uint material_index, bool is_transparent)
		{
			PMXFormat.Material material = format_.material_list.material[material_index];

			//先にテクスチャ情報を検索
			Texture2D main_texture = null;
			if (material.usually_texture_index < format_.texture_list.texture_file.Length) {
				string texture_file_name = format_.texture_list.texture_file[material.usually_texture_index];
				string path = format_.meta_header.folder + "/" + texture_file_name;
				main_texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
			}
			
			//マテリアルに設定
			string shader_path = GetMmdShaderPath(material, main_texture, is_transparent);
			Material result = new Material(Shader.Find(shader_path));
		
			// シェーダに依って値が有ったり無かったりするが、設定してもエラーに為らない様なので全部設定
			result.SetColor("_Color", material.diffuse_color);
			result.SetColor("_AmbColor", material.ambient_color);
			result.SetFloat("_Opacity", material.diffuse_color.a);
			result.SetColor("_SpecularColor", material.specular_color);
			result.SetFloat("_Shininess", material.specularity);
			// エッジ
			const float c_default_scale = 0.085f; //0.085fの時にMMDと一致する様にしているので、それ以外なら補正
			result.SetFloat("_OutlineWidth", material.edge_size * scale_ / c_default_scale);
			result.SetColor("_OutlineColor", material.edge_color);
			//カスタムレンダーキュー
			{
				MMDEngine engine = root_game_object_.GetComponent<MMDEngine>();
				if (engine.enable_render_queue && IsTransparentMaterial(is_transparent)) {
					//カスタムレンダーキューが有効 かつ マテリアルが透過なら
					//マテリアル順に並べる
					result.renderQueue = engine.render_queue_value + (int)material_index;
				} else {
					//非透明なら
					result.renderQueue = -1;
				}
			}
			
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
		/// <param name='is_transparent'>透過か</param>
		string GetMmdShaderPath(PMXFormat.Material material, Texture2D texture, bool is_transparent) {
			string result = "MMD/";
			if (IsTransparentMaterial(is_transparent)) {
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
		/// <param name='is_transparent'>透過か</param>
		bool IsTransparentMaterial(bool is_transparent) {
			return is_transparent;
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
		/// <param name='materials'>対象マテリアル</param>
		void CreateAssetForMaterials(Material[] materials) {
			string path = format_.meta_header.folder + "/Materials/";

			for (int i = 0, i_max = materials.Length; i < i_max; ++i) {
				string name = GetFilePathString(format_.material_list.material[i].name);
				string file_name = path + i.ToString() + "_" + name + ".asset";
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
													game_object.transform.position = x.bone_position * scale_;
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
		/// <param name='mesh'>対象メッシュ</param>
		/// <param name='materials'>対象マテリアル</param>
		/// <param name='bones'>対象ボーン</param>
		/// <param name='renderers'>対象レンダラー</param>
		/// <param name='creation_info'>メッシュ作成情報</param>
		void CreateMorph(Mesh[] mesh, Material[][] materials, GameObject[] bones, SkinnedMeshRenderer[] renderers, MeshCreationInfo[] creation_info)
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
			CreateVertexMorph(morph_manager, morphs, creation_info);
			//UV・追加UVモーフ作成
			CreateUvMorph(morph_manager, morphs, creation_info);
			//材質モーフ作成
			CreateMaterialMorph(morph_manager, morphs, creation_info);
			//モーフ一覧設定(モーフコンポーネントの情報を拾う為、最後に設定する)
			morph_manager.morphs = morphs.Select(x=>x.GetComponent<MorphBase>()).ToArray();

			//メッシュ・マテリアル設定
			morph_manager.renderers = renderers;
			morph_manager.mesh = mesh;
			morph_manager.materials = materials;
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
																	result.position *= scale_;
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
														param.position = y.move_value * scale_;
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
		/// <param name='creation_info'>メッシュ作成情報</param>
		void CreateVertexMorph(MorphManager morph_manager, GameObject[] morphs, MeshCreationInfo[] creation_info)
		{
			//インデックスと元データの作成
			List<uint> original_indices = format_.morph_list.morph_data.Where(x=>(PMXFormat.MorphData.MorphType.Vertex == x.morph_type)) //該当モーフに絞る
																		.SelectMany(x=>x.morph_offset.Select(y=>((PMXFormat.VertexMorphOffset)y).vertex_index)) //インデックスの取り出しと連結
																		.Distinct() //重複したインデックスの削除
																		.ToList(); //ソートに向けて一旦リスト化
			original_indices.Sort(); //ソート
			int[] indices = original_indices.Select(x=>(int)x).ToArray();
			Vector3[] source = indices.Select(x=>format_.vertex_list.vertex[x].pos * scale_) //インデックスを用いて、元データをパック
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
			
			//メッシュ別インデックスの作成
			int invalid_vertex_index = format_.vertex_list.vertex.Length;
			MorphManager.VertexMorphPack.Meshes[] multi_indices = new MorphManager.VertexMorphPack.Meshes[creation_info.Length];
			for (int i = 0, i_max = creation_info.Length; i < i_max; ++i) {
				multi_indices[i] = new MorphManager.VertexMorphPack.Meshes();
				multi_indices[i].indices = new int[indices.Length];
				for (int k = 0, k_max = indices.Length; k < k_max; ++k) {
					if (creation_info[i].reassign_dictionary.ContainsKey((uint)indices[k])) {
						//このメッシュで有効なら
						multi_indices[i].indices[k] = (int)creation_info[i].reassign_dictionary[(uint)indices[k]];
					} else {
						//このメッシュでは無効なら
						multi_indices[i].indices[k] = invalid_vertex_index; //最大頂点数を設定(uint.MaxValueでは無いので注意)
					}
				}
			}

			//表情マネージャーにインデックス・元データ・スクリプトの設定
			morph_manager.vertex_morph = new MorphManager.VertexMorphPack(multi_indices, source, script);
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
			result.values = data.morph_offset.Select(x=>((PMXFormat.VertexMorphOffset)x).position_offset * scale_).ToArray();
			return result;
		}

		/// <summary>
		/// UV・追加UVモーフ作成
		/// </summary>
		/// <param name='morph_manager'>表情マネージャー</param>
		/// <param name='morphs'>モーフのゲームオブジェクト</param>
		/// <param name='creation_info'>メッシュ作成情報</param>
		void CreateUvMorph(MorphManager morph_manager, GameObject[] morphs, MeshCreationInfo[] creation_info)
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
				
				//メッシュ別インデックスの作成
				int invalid_vertex_index = format_.vertex_list.vertex.Length;
				MorphManager.UvMorphPack.Meshes[] multi_indices = new MorphManager.UvMorphPack.Meshes[creation_info.Length];
				for (int i = 0, i_max = creation_info.Length; i < i_max; ++i) {
					multi_indices[i] = new MorphManager.UvMorphPack.Meshes();
					multi_indices[i].indices = new int[indices.Length];
					for (int k = 0, k_max = indices.Length; k < k_max; ++k) {
						if (creation_info[i].reassign_dictionary.ContainsKey((uint)indices[k])) {
							//このメッシュで有効なら
							multi_indices[i].indices[k] = (int)creation_info[i].reassign_dictionary[(uint)indices[k]];
						} else {
							//このメッシュでは無効なら
							multi_indices[i].indices[k] = invalid_vertex_index; //最大頂点数を設定(uint.MaxValueでは無いので注意)
						}
					}
				}
	
				//表情マネージャーにインデックス・元データ・スクリプトの設定
				morph_manager.uv_morph[morph_type_index] = new MorphManager.UvMorphPack(multi_indices, source, script);
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
		/// <param name='creation_info'>メッシュ作成情報</param>
		void CreateMaterialMorph(MorphManager morph_manager, GameObject[] morphs, MeshCreationInfo[] creation_info)
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
			
			//材質リアサイン辞書の作成
			Dictionary<uint, uint>[] material_reassign_dictionary = new Dictionary<uint, uint>[creation_info.Length + 1];
			for (int i = 0, i_max = creation_info.Length; i < i_max; +++i) {
				material_reassign_dictionary[i] = new Dictionary<uint, uint>();
				for (uint k = 0, k_max = (uint)creation_info[i].value.Length; k < k_max; ++k) {
					material_reassign_dictionary[i][creation_info[i].value[k].material_index] = k;
				}
				if (-1 == indices.LastOrDefault()) {
					//indices の最後が -1(≒uint.MaxValue) なら
					//全材質対象が存在するので材質リアサイン辞書に追加
					material_reassign_dictionary[i][uint.MaxValue] = uint.MaxValue;
				}
			}

			//メッシュ別インデックスの作成
			int invalid_material_index = format_.material_list.material.Length;
			MorphManager.MaterialMorphPack.Meshes[] multi_indices = new MorphManager.MaterialMorphPack.Meshes[creation_info.Length];
			for (int i = 0, i_max = creation_info.Length; i < i_max; ++i) {
				multi_indices[i] = new MorphManager.MaterialMorphPack.Meshes();
				multi_indices[i].indices = new int[indices.Length];
				for (int k = 0, k_max = indices.Length; k < k_max; ++k) {
					if (material_reassign_dictionary[i].ContainsKey((uint)indices[k])) {
						//この材質で有効なら
						multi_indices[i].indices[k] = (int)material_reassign_dictionary[i][(uint)indices[k]];
					} else {
						//この材質では無効なら
						multi_indices[i].indices[k] = invalid_material_index; //最大材質数を設定(uint.MaxValueでは無いので注意)
					}
				}
			}

			//表情マネージャーにインデックス・元データ・スクリプトの設定
			morph_manager.material_morph = new MorphManager.MaterialMorphPack(multi_indices, source, script);
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
		/// <returns>レンダラー</returns>
		/// <param name='mesh'>対象メッシュ</param>
		/// <param name='materials'>設定するマテリアル</param>
		/// <param name='bones'>設定するボーン</param>
		SkinnedMeshRenderer[] BuildingBindpose(Mesh[] mesh, Material[][] materials, GameObject[] bones)
		{
			// メッシュルートを生成してルートの子供に付ける
			Transform mesh_root_transform = (new GameObject("Mesh")).transform;
			mesh_root_transform.parent = root_game_object_.transform;

			//モデルルート取得
			Transform model_root_transform = root_game_object_.transform.FindChild("Model");
			//ボーン共通データ
			Matrix4x4[] bindposes = bones.Select(x=>x.transform.worldToLocalMatrix).ToArray();
			Transform[] bones_transform = bones.Select(x=>x.transform).ToArray();
			
			//レンダー設定
			SkinnedMeshRenderer[] result = new SkinnedMeshRenderer[mesh.Length];
			for (int i = 0, i_max = mesh.Length; i < i_max; ++i) {
				Transform mesh_transform = (new GameObject("Mesh" + i.ToString())).transform;
				mesh_transform.parent = mesh_root_transform;
				SkinnedMeshRenderer smr = mesh_transform.gameObject.AddComponent<SkinnedMeshRenderer>();
				mesh[i].bindposes = bindposes;
				smr.sharedMesh = mesh[i];
				smr.bones = bones_transform;
				smr.materials = materials[i];
				smr.rootBone = model_root_transform;
				smr.receiveShadows = false; //影を受けない
				
				result[i] = smr;
			}
			return result;
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
			if (use_ik_) {
				//IKを使用するなら
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
			result.transform.position = rigidbody.collider_position * scale_;
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
			collider.radius = rigidbody.shape_size.x * scale_;
		}

		/// <summary>
		/// Box Colliderの設定
		/// </summary>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='obj'>Unity用剛体ゲームオブジェクト</param>
		void EntryBoxCollider(PMXFormat.Rigidbody rigidbody, GameObject obj)
		{
			BoxCollider collider = obj.AddComponent<BoxCollider>();
			collider.size = rigidbody.shape_size * 2.0f * scale_;
		}

		/// <summary>
		/// Capsule Colliderの設定
		/// </summary>
		/// <param name='rigidbody'>PMX用剛体データ</param>
		/// <param name='obj'>Unity用剛体ゲームオブジェクト</param>
		void EntryCapsuleCollider(PMXFormat.Rigidbody rigidbody, GameObject obj)
		{
			CapsuleCollider collider = obj.AddComponent<CapsuleCollider>();
			collider.radius = rigidbody.shape_size.x * scale_;
			collider.height = (rigidbody.shape_size.y + rigidbody.shape_size.x * 2.0f) * scale_;
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
			
			string name = GetFilePathString(rigidbody.name);
			string file_name = format_.meta_header.folder + "/Physics/" + index.ToString() + "_" + name + ".asset";
			AssetDatabase.CreateAsset(material, file_name);
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
				drive.positionSpring = joint.spring_position.x * scale_;
				conf.xDrive = drive;
			}
			if (joint.spring_position.y != 0.0f) {
				drive = new JointDrive();
				drive.positionSpring = joint.spring_position.y * scale_;
				conf.yDrive = drive;
			}
			if (joint.spring_position.z != 0.0f) {
				drive = new JointDrive();
				drive.positionSpring = joint.spring_position.z * scale_;
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

		const uint	c_max_vertex_count_in_mesh = 65535; //meshに含まれる最大頂点数(Unity3D的には65536迄入ると思われるが、ushort.MaxValueは特別な値として使うのでその分を除外)

		GameObject				root_game_object_;
		PMXFormat				format_;
		bool					use_ik_;
		float					scale_;
		AlphaReadableTexture	alpha_readable_texture_ = null;
	}
}
