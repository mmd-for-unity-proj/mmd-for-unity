using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

// アバターの設定を行うスクリプト
public class AvatarSettingScript 
{
	/// <summary>
	/// デフォルトコンストラクタ
	/// </summary>
	/// <remarks>
	/// ユーザーに依るインスタンス作成を禁止する
	/// </remarks>
	public AvatarSettingScript(GameObject root_game_object, GameObject[] bones) {
		root_game_object_ = root_game_object;
		bones_ = bones;
	}

	/// <summary>
	/// 汎用アバダーを設定する
	/// </summary>
	/// <returns>アニメーター</returns>
	public Animator SettingGenericAvatar() {
		//アバタールートトランスフォームの取得
		Transform avatar_root_transform = root_game_object_.transform.FindChild("Model");
		if (null == avatar_root_transform) {
			//ルートゲームオブジェクト直下にモデルルートオブジェクトが無い(PMDConverter)なら
			//ルートゲームオブジェクトをアバタールートオブジェクトとする
			avatar_root_transform = root_game_object_.transform;
		}
		
		//ジェネリックアバター作成
		string root_name = ((HasBone("全ての親"))? "全ての親": "");
		avatar_ = AvatarBuilder.BuildGenericAvatar(avatar_root_transform.gameObject, root_name);
		
		//アバターをアニメーターに設定
		animator_ = root_game_object_.AddComponent<Animator>();
		animator_.avatar = avatar_;
		
		return animator_;
	}

	/// <summary>
	/// 人型アバダーを設定する
	/// </summary>
	/// <returns>アニメーター</returns>
	/// <remarks>
	/// モデルに依ってボーン構成に差が有るが、MMD標準モデルとの一致を優先する
	/// </remarks>
	public Animator SettingHumanAvatar() {
		//生成済みのボーンをUnity推奨ポーズに設定
		SetRequirePose();
		
		//アバタールートトランスフォームの取得
		Transform avatar_root_transform = root_game_object_.transform.FindChild("Model");
		if (null == avatar_root_transform) {
			//ルートゲームオブジェクト直下にモデルルートオブジェクトが無い(PMDConverter)なら
			//ルートゲームオブジェクトをアバタールートオブジェクトとする
			avatar_root_transform = root_game_object_.transform;
		}
		
		//ヒューマノイドアバター作成
		HumanDescription description = new HumanDescription();
		description.human = CreateHumanBone();
		description.skeleton = CreateSkeletonBone();
		description.lowerArmTwist = 0.0f;
		description.upperArmTwist = 0.0f;
		description.upperLegTwist = 0.0f;
		description.lowerLegTwist = 0.0f;
		description.armStretch = 0.0f;
		description.legStretch = 0.0f;
		description.feetSpacing = 0.0f;
		avatar_ = AvatarBuilder.BuildHumanAvatar(avatar_root_transform.gameObject, description);
		
		//アバターをアニメーターに設定
		animator_ = root_game_object_.AddComponent<Animator>();
		animator_.avatar = avatar_;
		
		//IKの無効化
		DisableIk();

		return animator_;
	}

	/// アバターをProjectに登録する
	/// </summary>
	/// <param name='file_path'>ファイルパス</param>
	public void CreateAsset(string file_path)
	{
		if (avatar_) {
			AssetDatabase.CreateAsset(avatar_, file_path);
		} else {
			throw new System.InvalidOperationException();
		}
	}

	/// <summary>
	/// 生成済みのボーンをUnity推奨ポーズに設定
	/// </summary>
	void SetRequirePose()
	{
		foreach (Transform transform in bones_.Select(x=>x.transform)) {
			SetRequirePose(transform);
		}
	}

	/// <summary>
	/// 特定の名前を持つボーンを先代から選び出す
	/// </summary>
	/// <param name="transform">基点のボーン</param>
	/// <param name="name">対象のボーン名</param>
	/// <returns>対象の先代ボーン</returns>
	/// <remarks>
	/// 基点ボーンが探索名なら基点ボーンを返す。
	/// </remarks>
	static Transform FindTransformUpwards(Transform transform, string name)
	{
		while ((null != transform) && (transform.name != name)) {
			transform = transform.parent;
		}
		return transform;
	}

	/// <summary>
	/// 親子関係を見てボーンを水平にする
	/// </summary>
	/// <param name="transform">対象のボーン</param>
	/// <returns>Z軸のみを回転させるQuaternion</returns>
	static Quaternion ResetHorizontalPose(Transform transform, Transform child_transform)
	{
		// ボーンの向きを取得
		var bone_vector = child_transform.position - transform.position;
		bone_vector.z = 0f;			// 平面化
		bone_vector.Normalize();

		// 平面化した正規化ベクトルと単位ベクトルを比較して，角度を取得する
		Vector3 normalized_vector = bone_vector.x >= 0 ? Vector3.right : Vector3.left;
		float cos_value = Vector3.Dot(bone_vector, normalized_vector);
		float theta = Mathf.Acos(cos_value) * Mathf.Rad2Deg;

		theta = bone_vector.x >= 0 ? -theta : theta;	// ボーンの向きによって回転方向が違う

		return Quaternion.Euler(0f, 0f, theta);
	}

	/// <summary>
	/// 腕全体を水平にする処理
	/// </summary>
	/// <param name="wrist">手首ボーン</param>
	/// <param name="hinge_name">ひじボーン名</param>
	/// <param name="arm_name">腕ボーン名</param>
	/// <param name="shoulder_name">肩ボーン名</param>
	static void StartResettingHorizontal(Transform wrist, string hinge_name, string arm_name, string shoulder_name)
	{
		var hinge		= FindTransformUpwards(wrist,	hinge_name);
		var arm			= FindTransformUpwards(hinge,	arm_name);
		var shoulder	= FindTransformUpwards(arm,		shoulder_name);
		shoulder.transform.localRotation = ResetHorizontalPose(shoulder, arm);
		arm.transform.localRotation		 = ResetHorizontalPose(arm,		 hinge);
		hinge.transform.localRotation	 = ResetHorizontalPose(hinge,	 wrist);
	}

	/// <summary>
	/// 生成済みのボーンをUnity推奨ポーズに設定
	/// </summary>
	/// <param name='transform'>ボーンのトランスフォーム</param>
	void SetRequirePose(Transform transform)
	{
		switch (transform.name) {
		case "左手首":	//Tポーズにする為に腕を持ち上げる
			StartResettingHorizontal(transform, "左ひじ", "左腕", "左肩");
			break;
		case "右手首":	//Tポーズにする為に腕を持ち上げる
			StartResettingHorizontal(transform, "右ひじ", "右腕", "右肩");
			break;
		case "腰": goto case "センター";
		case "センター":
			if (HasBone("腰") ^ ("センター" == transform.name)) {
				//腰ボーンを持っていて、現在の入力が腰ボーンなら もしくは 腰ボーンを持っておらず、現在の入力がセンターボーンなら
				//後ろを向いているので向き直す
				transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
				transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, -transform.localPosition.z);
			}
			break;
		default:
			if (root_game_object_.transform.FindChild("Physics") == transform.parent) {
				//物理演算ルートなら
				//後ろを向いているので向き直す
				transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
				transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, -transform.localPosition.z);
			}
			break;
		}
	}
	
	/// <summary>
	/// ボーン存在確認
	/// </summary>
	/// <returns>true:ボーンが存在する, false:ボーンが存在しない</returns>
	/// <param name='name'>ボーン名</param>
	bool HasBone(string name) {
		return bones_.Any(x=>x.name == name);
	}

	/// <summary>
	/// MMD用ヒューマノイドボーン作成
	/// </summary>
	/// <returns>ヒューマノイドボーン</returns>
	HumanBone[] CreateHumanBone()
	{
		System.Func<string, string, HumanBone> CreateHint = (key, value)=>{
																			HumanBone human_bone = new HumanBone();
																			human_bone.humanName = key;
																			human_bone.boneName = value;
																			human_bone.limit.useDefaultValues = true;
																			return human_bone;
																		};
		
		List<HumanBone> bone_name = new List<HumanBone>();

		//◆必須
		//△標準逸脱モデル対策
		
		//顔系
		//ヒューマノイドボーンは"ヒップ→背骨"のボーンを要求するが、MMDは"上半身→下半身"と接続方向が逆なのでこれらを割り当てる事は出来ない
		if (HasBone("腰")) {
			bone_name.Add(CreateHint("Hips",	"腰"		));	//ヒップ◆△
		} else {
			bone_name.Add(CreateHint("Hips",	"センター"	));	//ヒップ◆
		}
		bone_name.Add(CreateHint("Spine",		"上半身"	));	//背骨◆
		if (HasBone("胸")) {
			bone_name.Add(CreateHint("Chest",	"胸"		));	//胸△
		} else {
			bone_name.Add(CreateHint("Chest",	"上半身2"	));	//胸
		}
		bone_name.Add(CreateHint("Neck",		"首"		));	//首
		bone_name.Add(CreateHint("Head",		"頭"		));	//頭◆
		bone_name.Add(CreateHint("LeftEye",		"左目"		));	//左目
		bone_name.Add(CreateHint("RightEye",	"右目"		));	//右目
		if (HasBone("あご")) {
			bone_name.Add(CreateHint("Jaw",		"あご"		));	//あご△
		} else if (HasBone("顎")) {
			bone_name.Add(CreateHint("Jaw",		"顎"		));	//あご△
		}
		
		//足系
		bone_name.Add(CreateHint("LeftUpperLeg",	"左足"		));	//左脚上部◆
		bone_name.Add(CreateHint("RightUpperLeg",	"右足"		));	//右脚上部◆
		bone_name.Add(CreateHint("LeftLowerLeg",	"左ひざ"	));	//左脚◆
		bone_name.Add(CreateHint("RightLowerLeg",	"右ひざ"	));	//右脚◆
		bone_name.Add(CreateHint("LeftFoot",		"左足首"	));	//左足◆
		bone_name.Add(CreateHint("RightFoot",		"右足首"	));	//右足◆
		bone_name.Add(CreateHint("LeftToes",		"左つま先"	));	//左足指
		bone_name.Add(CreateHint("RightToes",		"右つま先"	));	//右足指
		
		//手系
		bone_name.Add(CreateHint("LeftShoulder",	"左肩"		));	//左肩
		bone_name.Add(CreateHint("RightShoulder",	"右肩"		));	//右肩
		bone_name.Add(CreateHint("LeftUpperArm",	"左腕"		));	//左腕上部◆
		bone_name.Add(CreateHint("RightUpperArm",	"右腕"		));	//右腕上部◆
		bone_name.Add(CreateHint("LeftLowerArm",	"左ひじ"	));	//左腕◆
		bone_name.Add(CreateHint("RightLowerArm",	"右ひじ"	));	//右腕◆
		bone_name.Add(CreateHint("LeftHand",		"左手首"	));	//左手◆
		bone_name.Add(CreateHint("RightHand",		"右手首"	));	//右手◆
		
		//指系
		bone_name.Add(CreateHint("Left Thumb Proximal",			"左親指１"		));	//左親指(付根)
		bone_name.Add(CreateHint("Left Thumb Intermediate",		"左親指２"		));	//左親指(間接)
		bone_name.Add(CreateHint("Left Thumb Distal",			"左親指先"		));	//左親指(先)
		bone_name.Add(CreateHint("Left Index Proximal",			"左人指１"		));	//左人差し指(付根)
		bone_name.Add(CreateHint("Left Index Intermediate",		"左人指２"		));	//左人差し指(間接)
		if (HasBone("左人指先")) {
			bone_name.Add(CreateHint("Left Index Distal",		"左人指先"		));	//左人差し指(先)△
		} else {
			bone_name.Add(CreateHint("Left Index Distal",		"左人差指先"	));	//左人差し指(先)
		}
		bone_name.Add(CreateHint("Left Middle Proximal",		"左中指１"		));	//左中指(付根)
		bone_name.Add(CreateHint("Left Middle Intermediate",	"左中指２"		));	//左中指(間接)
		bone_name.Add(CreateHint("Left Middle Distal",			"左中指先"		));	//左中指(先)
		bone_name.Add(CreateHint("Left Ring Proximal",			"左薬指１"		));	//左薬指(付根)
		bone_name.Add(CreateHint("Left Ring Intermediate",		"左薬指２"		));	//左薬指(間接)
		bone_name.Add(CreateHint("Left Ring Distal",			"左薬指先"		));	//左薬指(先)
		bone_name.Add(CreateHint("Left Little Proximal",		"左小指１"		));	//左小指(付根)
		bone_name.Add(CreateHint("Left Little Intermediate",	"左小指２"		));	//左小指(間接)
		bone_name.Add(CreateHint("Left Little Distal",			"左小指先"		));	//左小指(先)
		bone_name.Add(CreateHint("Right Thumb Proximal",		"右親指１"		));	//右親指(付根)
		bone_name.Add(CreateHint("Right Thumb Intermediate",	"右親指２"		));	//右親指(間接)
		bone_name.Add(CreateHint("Right Thumb Distal",			"右親指先"		));	//右親指(先)
		bone_name.Add(CreateHint("Right Index Proximal",		"右人指１"		));	//右人差し指(付根)
		bone_name.Add(CreateHint("Right Index Intermediate",	"右人指２"		));	//右人差し指(間接)
		if (HasBone("右人指先")) {
			bone_name.Add(CreateHint("Right Index Distal",		"右人指先"		));	//右人差し指(先)△
		} else {
			bone_name.Add(CreateHint("Right Index Distal",		"右人差指先"	));	//右人差し指(先)
		}
		bone_name.Add(CreateHint("Right Middle Proximal",		"右中指１"		));	//右中指(付根)
		bone_name.Add(CreateHint("Right Middle Intermediate",	"右中指２"		));	//右中指(間接)
		bone_name.Add(CreateHint("Right Middle Distal",			"右中指先"		));	//右中指(先)
		bone_name.Add(CreateHint("Right Ring Proximal",			"右薬指１"		));	//右薬指(付根)
		bone_name.Add(CreateHint("Right Ring Intermediate",		"右薬指２"		));	//右薬指(間接)
		bone_name.Add(CreateHint("Right Ring Distal",			"右薬指先"		));	//右薬指(先)
		bone_name.Add(CreateHint("Right Little Proximal",		"右小指１"		));	//右小指(付根)
		bone_name.Add(CreateHint("Right Little Intermediate",	"右小指２"		));	//右小指(間接)
		bone_name.Add(CreateHint("Right Little Distal",			"右小指先"		));	//右小指(先)
			
		return bone_name.Where(x=>!string.IsNullOrEmpty(x.boneName)).ToArray();
	}

	/// <summary>
	/// MMD用スケルトンボーン作成
	/// </summary>
	/// <returns>スケルトンボーン</returns>
	SkeletonBone[] CreateSkeletonBone()
	{
		IEnumerable<GameObject> bones_enumerator = bones_;

		//Hipsボーンの親ボーン迄SkeletonBoneに入れる必要が有るので、確認と追加
		string hips_bone_name = ((HasBone("腰"))? "腰": "センター");
		Transform hips_parent_bone = bones_.Where(x=>x.name == hips_bone_name).Select(x=>x.transform.parent).FirstOrDefault();
		if (null != hips_parent_bone) {
			//Hipsボーンの親ボーンが有るなら
			//Hipsボーンの親ボーンがbones_に含まれているか確認する
			if (!HasBone(hips_parent_bone.name)) {
				//Hipsボーンの親ボーンがbones_に無いなら
				//追加(Hipsボーン依りも前に追加しないといけないので注意)
				bones_enumerator = Enumerable.Repeat(hips_parent_bone.gameObject, 1)
											.Concat(bones_enumerator);
			}
		}

		var result = bones_enumerator.Select(x=>{
												SkeletonBone skeleton_bone = new SkeletonBone();
												skeleton_bone.name = x.name;
												Transform transform = x.transform;
												skeleton_bone.position = transform.localPosition;
												skeleton_bone.rotation = transform.localRotation;
												skeleton_bone.scale = transform.localScale;
												return skeleton_bone;
											});
		return result.ToArray();
	}

	/// <summary>
	/// IKの無効化
	/// </summary>
	void DisableIk()
	{
		foreach(CCDIKSolver ik_solver in root_game_object_.GetComponent<MMDEngine>().ik_list) {
			ik_solver.enabled = false;
		}
	}

	GameObject		root_game_object_ = null;
	GameObject[]	bones_ = null;
	Avatar			avatar_ = null;
	Animator		animator_ = null;
}
