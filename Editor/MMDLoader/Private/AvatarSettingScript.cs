#if UNITY_4_2

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
	/// アバダーを設定する
	/// </summary>
	/// <returns>アニメーター</returns>
	/// <param name='root_game_object'>ルートゲームオブジェクト</param>
	/// <param name='bones'>ボーンのゲームオブジェクト</param>
	/// <remarks>
	/// モデルに依ってボーン構成に差が有るが、MMD標準モデルとの一致を優先する
	/// </remarks>
	public Animator SettingAvatar() {
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
	/// 生成済みのボーンをUnity推奨ポーズに設定
	/// </summary>
	/// <param name='transform'>ボーンのトランスフォーム</param>
	void SetRequirePose(Transform transform)
	{
		switch (transform.name) {
		case "左肩":	//Tポーズにする為に腕を持ち上げる
			transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 9.0f);
			break;
		case "右肩":	//Tポーズにする為に腕を持ち上げる
			transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -9.0f);
			break;
		case "左腕":	//Tポーズにする為に腕を持ち上げる
			transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 27.0f);
			break;
		case "右腕":	//Tポーズにする為に腕を持ち上げる
			transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -27.0f);
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
		int count = bones_.Where(x=>x.name == name).Count();
		return 0 < count;
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
		return bones_.Select(x=>{
								SkeletonBone skeleton_bone = new SkeletonBone();
								skeleton_bone.name = x.name;
								Transform transform = x.transform;
								skeleton_bone.position = transform.localPosition;
								skeleton_bone.rotation = transform.localRotation;
								skeleton_bone.scale = transform.localScale;
								return skeleton_bone;
							}).ToArray();
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

#endif //UNITY_4_2
