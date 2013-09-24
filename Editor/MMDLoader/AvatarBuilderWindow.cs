#define MFU_AVATART_SAVE_FILE

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AvatarBuilderWindow : EditorWindow {
	
	/// <summary>
	/// メニュー
	/// </summary>
	[MenuItem("Plugins/MMD Loader/Avatar Builder")]
	static void OnMenuClick() {
		AvatarBuilderWindow window = EditorWindow.GetWindow<AvatarBuilderWindow>(true, "Avatar Builder");
		window.Show();
	}
	
	/// <summary>
	/// コンストラクタ
	/// </summary>
	AvatarBuilderWindow() {
		SetNamepackOfBasic();
	}
	
	/// <summary>
	/// GUI描画
	/// </summary>
	void OnGUI() {
		root_obj_ = (GameObject)EditorGUILayout.ObjectField("RootObject", root_obj_, typeof(GameObject), true);
		avatar_name_ = EditorGUILayout.TextField("Name", avatar_name_);
		
		if (GUILayout.Button("Build")) {
			BuildAvatar();
		}
		
#if MFU_AVATART_SAVE_FILE
		avatar_ = (Avatar)EditorGUILayout.ObjectField("Avatar", avatar_, typeof(Avatar), false);
		if (GUILayout.Button("SaveAvatar")) {
			Avatar avatar = (Avatar)Instantiate(avatar_);
			AssetDatabase.CreateAsset(avatar, avatar_name_);
		}
#endif

		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Basic", EditorStyles.miniButtonLeft)) {
				SetNamepackOfBasic();
			}
			
			if (GUILayout.Button("Unity Asset", EditorStyles.miniButtonMid)) {
				SetNamepackOfUnityAsset();
			}
			
			if (GUILayout.Button("Mmd", EditorStyles.miniButtonRight)) {
				SetNamepackOfMmd();
			}
		}
		EditorGUILayout.EndHorizontal();
		
		display_head_ = EditorGUILayout.Foldout(display_head_, "Head");
		if (display_head_) {
			name_["Hips"]		= EditorGUILayout.TextField("Hips◆",		name_["Hips"]		);	//ヒップ◆
			name_["Spine"]		= EditorGUILayout.TextField("Spine◆",		name_["Spine"]		);	//背骨◆
			name_["Chest"]		= EditorGUILayout.TextField("Chest",		name_["Chest"]		);	//胸
			name_["Neck"]		= EditorGUILayout.TextField("Neck",		name_["Neck"]		);	//首
			name_["Head"]		= EditorGUILayout.TextField("Head◆",		name_["Head"]		);	//頭◆
			name_["LeftEye"]	= EditorGUILayout.TextField("LeftEye",	name_["LeftEye"]	);	//左目
			name_["RightEye"]	= EditorGUILayout.TextField("RightEye",	name_["RightEye"]	);	//右目
			name_["Jaw"]		= EditorGUILayout.TextField("Jaw",		name_["Jaw"]		);	//あご
		}

		display_leg_ = EditorGUILayout.Foldout(display_leg_, "Leg");
		if (display_leg_) {
			name_["LeftUpperLeg"]	= EditorGUILayout.TextField("LeftUpperLeg◆",	name_["LeftUpperLeg"]	);	//左脚上部◆
			name_["RightUpperLeg"]	= EditorGUILayout.TextField("RightUpperLeg◆",	name_["RightUpperLeg"]	);	//右脚上部◆
			name_["LeftLowerLeg"]	= EditorGUILayout.TextField("LeftLowerLeg◆",	name_["LeftLowerLeg"]	);	//左脚◆
			name_["RightLowerLeg"]	= EditorGUILayout.TextField("RightLowerLeg◆",	name_["RightLowerLeg"]	);	//右脚◆
			name_["LeftFoot"]		= EditorGUILayout.TextField("LeftFoot◆",		name_["LeftFoot"]		);	//左足◆
			name_["RightFoot"]		= EditorGUILayout.TextField("RightFoot◆",		name_["RightFoot"]		);	//右足◆
			name_["LeftToes"]		= EditorGUILayout.TextField("LeftToes",		name_["LeftToes"]		);	//左足指
			name_["RightToes"]		= EditorGUILayout.TextField("RightToes",		name_["RightToes"]		);	//右足指
		}
		
		display_arm_ = EditorGUILayout.Foldout(display_arm_, "Arm");
		if (display_arm_) {
			name_["LeftShoulder"]	= EditorGUILayout.TextField("LeftShoulder",	name_["LeftShoulder"]	);	//左肩
			name_["RightShoulder"]	= EditorGUILayout.TextField("RightShoulder",	name_["RightShoulder"]	);	//右肩
			name_["LeftUpperArm"]	= EditorGUILayout.TextField("LeftUpperArm◆",	name_["LeftUpperArm"]	);	//左腕上部◆
			name_["RightUpperArm"]	= EditorGUILayout.TextField("RightUpperArm◆",	name_["RightUpperArm"]	);	//右腕上部◆
			name_["LeftLowerArm"]	= EditorGUILayout.TextField("LeftLowerArm◆",	name_["LeftLowerArm"]	);	//左腕◆
			name_["RightLowerArm"]	= EditorGUILayout.TextField("RightLowerArm◆",	name_["RightLowerArm"]	);	//右腕◆
			name_["LeftHand"]		= EditorGUILayout.TextField("LeftHand◆",		name_["LeftHand"]		);	//左手◆
			name_["RightHand"]		= EditorGUILayout.TextField("RightHand◆",		name_["RightHand"]		);	//右手◆
		}

		display_left_finger_ = EditorGUILayout.Foldout(display_left_finger_, "LeftFinger");
		if (display_left_finger_) {
			name_["Left Thumb Proximal"]		= EditorGUILayout.TextField("Left Thumb Proximal",			name_["Left Thumb Proximal"]		);	//左親指(付根)
			name_["Left Thumb Intermediate"]	= EditorGUILayout.TextField("Left Thumb Intermediate",		name_["Left Thumb Intermediate"]	);	//左親指(間接)
			name_["Left Thumb Distal"]			= EditorGUILayout.TextField("Left Thumb Distal",			name_["Left Thumb Distal"]			);	//左親指(先)
			name_["Left Index Proximal"]		= EditorGUILayout.TextField("Left Index Proximal",			name_["Left Index Proximal"]		);	//左人差し指(付根)
			name_["Left Index Intermediate"]	= EditorGUILayout.TextField("Left Index Intermediate",		name_["Left Index Intermediate"]	);	//左人差し指(間接)
			name_["Left Index Distal"]			= EditorGUILayout.TextField("Left Index Distal",			name_["Left Index Distal"]			);	//左人差し指(先)
			name_["Left Middle Proximal"]		= EditorGUILayout.TextField("Left Middle Proximal",			name_["Left Middle Proximal"]		);	//左中指(付根)
			name_["Left Middle Intermediate"]	= EditorGUILayout.TextField("Left Middle Intermediate",		name_["Left Middle Intermediate"]	);	//左中指(間接)
			name_["Left Middle Distal"]			= EditorGUILayout.TextField("Left Middle Distal",			name_["Left Middle Distal"]			);	//左中指(先)
			name_["Left Ring Proximal"]			= EditorGUILayout.TextField("Left Ring Proximal",			name_["Left Ring Proximal"]			);	//左薬指(付根)
			name_["Left Ring Intermediate"]		= EditorGUILayout.TextField("Left Ring Intermediate",		name_["Left Ring Intermediate"]		);	//左薬指(間接)
			name_["Left Ring Distal"]			= EditorGUILayout.TextField("Left Ring Distal",				name_["Left Ring Distal"]			);	//左薬指(先)
			name_["Left Little Proximal"]		= EditorGUILayout.TextField("Left Little Proximal",			name_["Left Little Proximal"]		);	//左小指(付根)
			name_["Left Little Intermediate"]	= EditorGUILayout.TextField("Left Little Intermediate",		name_["Left Little Intermediate"]	);	//左小指(間接)
			name_["Left Little Distal"]			= EditorGUILayout.TextField("Left Little Distal",			name_["Left Little Distal"]			);	//左小指(先)
		}
		display_right_finger_ = EditorGUILayout.Foldout(display_right_finger_, "RightFinger");
		if (display_right_finger_) {
			name_["Right Thumb Proximal"]		= EditorGUILayout.TextField("Right Thumb Proximal",			name_["Right Thumb Proximal"]		);	//右親指(付根)
			name_["Right Thumb Intermediate"]	= EditorGUILayout.TextField("Right Thumb Intermediate",		name_["Right Thumb Intermediate"]	);	//右親指(間接)
			name_["Right Thumb Distal"]			= EditorGUILayout.TextField("Right Thumb Distal",			name_["Right Thumb Distal"]			);	//右親指(先)
			name_["Right Index Proximal"]		= EditorGUILayout.TextField("Right Index Proximal",			name_["Right Index Proximal"]		);	//右人差し指(付根)
			name_["Right Index Intermediate"]	= EditorGUILayout.TextField("Right Index Intermediate",		name_["Right Index Intermediate"]	);	//右人差し指(間接)
			name_["Right Index Distal"]			= EditorGUILayout.TextField("Right Index Distal",			name_["Right Index Distal"]			);	//右人差し指(先)
			name_["Right Middle Proximal"]		= EditorGUILayout.TextField("Right Middle Proximal",		name_["Right Middle Proximal"]		);	//右中指(付根)
			name_["Right Middle Intermediate"]	= EditorGUILayout.TextField("Right Middle Intermediate",	name_["Right Middle Intermediate"]	);	//右中指(間接)
			name_["Right Middle Distal"]		= EditorGUILayout.TextField("Right Middle Distal",			name_["Right Middle Distal"]		);	//右中指(先)
			name_["Right Ring Proximal"]		= EditorGUILayout.TextField("Right Ring Proximal",			name_["Right Ring Proximal"]		);	//右薬指(付根)
			name_["Right Ring Intermediate"]	= EditorGUILayout.TextField("Right Ring Intermediate",		name_["Right Ring Intermediate"]	);	//右薬指(間接)
			name_["Right Ring Distal"]			= EditorGUILayout.TextField("Right Ring Distal",			name_["Right Ring Distal"]			);	//右薬指(先)
			name_["Right Little Proximal"]		= EditorGUILayout.TextField("Right Little Proximal",		name_["Right Little Proximal"]		);	//右小指(付根)
			name_["Right Little Intermediate"]	= EditorGUILayout.TextField("Right Little Intermediate",	name_["Right Little Intermediate"]	);	//右小指(間接)
			name_["Right Little Distal"]		= EditorGUILayout.TextField("Right Little Distal",			name_["Right Little Distal"]		);	//右小指(先)
		}
	}
	
	private void SetNamepackOfBasic() {
		name_["Hips"]		 = "Hips";
		name_["Spine"]		 = "Spine";
		name_["Chest"]		 = "Chest";
		name_["Neck"]		 = "Neck";
		name_["Head"]		 = "Head";
		name_["LeftEye"]	 = "LeftEye";
		name_["RightEye"]	 = "RightEye";
		name_["Jaw"]		 = "Jaw";
		
		name_["LeftUpperLeg"]	 = "LeftUpperLeg";
		name_["RightUpperLeg"]	 = "RightUpperLeg";
		name_["LeftLowerLeg"]	 = "LeftLowerLeg";
		name_["RightLowerLeg"]	 = "RightLowerLeg";
		name_["LeftFoot"]		 = "LeftFoot";
		name_["RightFoot"]		 = "RightFoot";
		name_["LeftToes"]		 = "LeftToes";
		name_["RightToes"]		 = "RightToes";
				
		name_["LeftShoulder"]	 = "LeftShoulder";
		name_["RightShoulder"]	 = "RightShoulder";
		name_["LeftUpperArm"]	 = "LeftUpperArm";
		name_["RightUpperArm"]	 = "RightUpperArm";
		name_["LeftLowerArm"]	 = "LeftLowerArm";
		name_["RightLowerArm"]	 = "RightLowerArm";
		name_["LeftHand"]		 = "LeftHand";
		name_["RightHand"]		 = "RightHand";
		
		name_["Left Thumb Proximal"]		 = "Left Thumb Proximal";
		name_["Left Thumb Intermediate"]	 = "Left Thumb Intermediate";
		name_["Left Thumb Distal"]			 = "Left Thumb Distal";
		name_["Left Index Proximal"]		 = "Left Index Proximal";
		name_["Left Index Intermediate"]	 = "Left Index Intermediate";
		name_["Left Index Distal"]			 = "Left Index Distal";
		name_["Left Middle Proximal"]		 = "Left Middle Proximal";
		name_["Left Middle Intermediate"]	 = "Left Middle Intermediate";
		name_["Left Middle Distal"]			 = "Left Middle Distal";
		name_["Left Ring Proximal"]			 = "Left Ring Proximal";
		name_["Left Ring Intermediate"]		 = "Left Ring Intermediate";
		name_["Left Ring Distal"]			 = "Left Ring Distal";
		name_["Left Little Proximal"]		 = "Left Little Proximal";
		name_["Left Little Intermediate"]	 = "Left Little Intermediate";
		name_["Left Little Distal"]			 = "Left Little Distal";
		name_["Right Thumb Proximal"]		 = "Right Thumb Proximal";
		name_["Right Thumb Intermediate"]	 = "Right Thumb Intermediate";
		name_["Right Thumb Distal"]			 = "Right Thumb Distal";
		name_["Right Index Proximal"]		 = "Right Index Proximal";
		name_["Right Index Intermediate"]	 = "Right Index Intermediate";
		name_["Right Index Distal"]			 = "Right Index Distal";
		name_["Right Middle Proximal"]		 = "Right Middle Proximal";
		name_["Right Middle Intermediate"]	 = "Right Middle Intermediate";
		name_["Right Middle Distal"]		 = "Right Middle Distal";
		name_["Right Ring Proximal"]		 = "Right Ring Proximal";
		name_["Right Ring Intermediate"]	 = "Right Ring Intermediate";
		name_["Right Ring Distal"]			 = "Right Ring Distal";
		name_["Right Little Proximal"]		 = "Right Little Proximal";
		name_["Right Little Intermediate"]	 = "Right Little Intermediate";
		name_["Right Little Distal"]		 = "Right Little Distal";
	}
	
	private void SetNamepackOfUnityAsset() {
		name_["Hips"]		 = "Hips";
		name_["Spine"]		 = "Spine";
		name_["Chest"]		 = "Chest";
		name_["Neck"]		 = "Neck";
		name_["Head"]		 = "Head";
		name_["LeftEye"]	 = "LeftEye";
		name_["RightEye"]	 = "RightEye";
		name_["Jaw"]		 = "Jaw";
		
		name_["LeftUpperLeg"]	 = "LeftUpLeg";
		name_["RightUpperLeg"]	 = "RightUpLeg";
		name_["LeftLowerLeg"]	 = "LeftLeg";
		name_["RightLowerLeg"]	 = "RightLeg";
		name_["LeftFoot"]		 = "LeftFoot";
		name_["RightFoot"]		 = "RightFoot";
		name_["LeftToes"]		 = "LeftToes";
		name_["RightToes"]		 = "RightToes";
				
		name_["LeftShoulder"]	 = "LeftShoulder";
		name_["RightShoulder"]	 = "RightShoulder";
		name_["LeftUpperArm"]	 = "LeftArm";
		name_["RightUpperArm"]	 = "RightArm";
		name_["LeftLowerArm"]	 = "LeftForeArm";
		name_["RightLowerArm"]	 = "RightForeArm";
		name_["LeftHand"]		 = "LeftHand";
		name_["RightHand"]		 = "RightHand";
		
		name_["Left Thumb Proximal"]		 = "LeftHandThumb1";
		name_["Left Thumb Intermediate"]	 = "LeftHandThumb2";
		name_["Left Thumb Distal"]			 = "LeftHandThumb3";
		name_["Left Index Proximal"]		 = "LeftHandIndex1";
		name_["Left Index Intermediate"]	 = "LeftHandIndex2";
		name_["Left Index Distal"]			 = "LeftHandIndex3";
		name_["Left Middle Proximal"]		 = "LeftHandMiddle1";
		name_["Left Middle Intermediate"]	 = "LeftHandMiddle2";
		name_["Left Middle Distal"]			 = "LeftHandMiddle3";
		name_["Left Ring Proximal"]			 = "LeftHandRing1";
		name_["Left Ring Intermediate"]		 = "LeftHandRing2";
		name_["Left Ring Distal"]			 = "LeftHandRing3";
		name_["Left Little Proximal"]		 = "LeftHandPinky1";
		name_["Left Little Intermediate"]	 = "LeftHandPinky2";
		name_["Left Little Distal"]			 = "LeftHandPinky3";
		name_["Right Thumb Proximal"]		 = "RightHandThumb1";
		name_["Right Thumb Intermediate"]	 = "RightHandThumb2";
		name_["Right Thumb Distal"]			 = "RightHandThumb3";
		name_["Right Index Proximal"]		 = "RightHandIndex1";
		name_["Right Index Intermediate"]	 = "RightHandIndex2";
		name_["Right Index Distal"]			 = "RightHandIndex3";
		name_["Right Middle Proximal"]		 = "RightHandMiddle1";
		name_["Right Middle Intermediate"]	 = "RightHandMiddle2";
		name_["Right Middle Distal"]		 = "RightHandMiddle3";
		name_["Right Ring Proximal"]		 = "RightHandRing1";
		name_["Right Ring Intermediate"]	 = "RightHandRing2";
		name_["Right Ring Distal"]			 = "RightHandRing3";
		name_["Right Little Proximal"]		 = "RightHandPinky1";
		name_["Right Little Intermediate"]	 = "RightHandPinky2";
		name_["Right Little Distal"]		 = "RightHandPinky3";
	}
	
	private void SetNamepackOfMmd() {
		name_["Hips"]		 = "腰";
		name_["Spine"]		 = "上半身";
		name_["Chest"]		 = "胸";
		name_["Neck"]		 = "首";
		name_["Head"]		 = "頭";
		name_["LeftEye"]	 = "左目";
		name_["RightEye"]	 = "右目";
		name_["Jaw"]		 = "あご";
		
		name_["LeftUpperLeg"]	 = "左足";
		name_["RightUpperLeg"]	 = "右足";
		name_["LeftLowerLeg"]	 = "左ひざ";
		name_["RightLowerLeg"]	 = "右ひざ";
		name_["LeftFoot"]		 = "左足首";
		name_["RightFoot"]		 = "右足首";
		name_["LeftToes"]		 = "左つま先";
		name_["RightToes"]		 = "右つま先";
		
		name_["LeftShoulder"]	 = "左肩";
		name_["RightShoulder"]	 = "右肩";
		name_["LeftUpperArm"]	 = "左腕";
		name_["RightUpperArm"]	 = "右腕";
		name_["LeftLowerArm"]	 = "左ひじ";
		name_["RightLowerArm"]	 = "右ひじ";
		name_["LeftHand"]		 = "左手首";
		name_["RightHand"]		 = "右手首";
		
		name_["Left Thumb Proximal"]		 = "左親指１";
		name_["Left Thumb Intermediate"]	 = "左親指２";
		name_["Left Thumb Distal"]			 = "左親指先";
		name_["Left Index Proximal"]		 = "左人指１";
		name_["Left Index Intermediate"]	 = "左人指２";
		name_["Left Index Distal"]			 = "左人指先";
		name_["Left Middle Proximal"]		 = "左中指１";
		name_["Left Middle Intermediate"]	 = "左中指２";
		name_["Left Middle Distal"]			 = "左中指先";
		name_["Left Ring Proximal"]			 = "左薬指１";
		name_["Left Ring Intermediate"]		 = "左薬指２";
		name_["Left Ring Distal"]			 = "左薬指先";
		name_["Left Little Proximal"]		 = "左小指１";
		name_["Left Little Intermediate"]	 = "左小指２";
		name_["Left Little Distal"]			 = "左小指先";
		name_["Right Thumb Proximal"]		 = "右親指１";
		name_["Right Thumb Intermediate"]	 = "右親指２";
		name_["Right Thumb Distal"]			 = "右親指先";
		name_["Right Index Proximal"]		 = "右人指１";
		name_["Right Index Intermediate"]	 = "右人指２";
		name_["Right Index Distal"]			 = "右人指先";
		name_["Right Middle Proximal"]		 = "右中指１";
		name_["Right Middle Intermediate"]	 = "右中指２";
		name_["Right Middle Distal"]		 = "右中指先";
		name_["Right Ring Proximal"]		 = "右薬指１";
		name_["Right Ring Intermediate"]	 = "右薬指２";
		name_["Right Ring Distal"]			 = "右薬指先";
		name_["Right Little Proximal"]		 = "右小指１";
		name_["Right Little Intermediate"]	 = "右小指２";
		name_["Right Little Distal"]		 = "右小指先";
	}
	
	private void BuildAvatar() {
		//ヒューマノイドアバター作成
		HumanDescription description = new HumanDescription();
		description.human = name_.Where(x=>!string.IsNullOrEmpty(x.Value))
								.Select(x=>{
											HumanBone human_bone = new HumanBone();
											human_bone.humanName = x.Key;
											human_bone.boneName = x.Value;
											human_bone.limit.useDefaultValues = true;
											return human_bone;
										})
								.ToArray();
		description.skeleton = GetSelfAndChirdrenTransform(root_obj_.transform)
									.Select(x=>{
												SkeletonBone skeleton_bone = new SkeletonBone();
												skeleton_bone.name = x.name;
												skeleton_bone.position = x.localPosition;
												skeleton_bone.rotation = x.localRotation;
												skeleton_bone.scale = x.localScale;
												return skeleton_bone;
											})
									.ToArray();
		description.upperArmTwist = 0.5f;
		description.lowerArmTwist = 0.5f;
		description.upperLegTwist = 0.5f;
		description.lowerLegTwist = 0.5f;
		description.armStretch = 0.05f;
		description.legStretch = 0.05f;
		description.feetSpacing = 0.0f;

		Avatar avatar = AvatarBuilder.BuildHumanAvatar(root_obj_, description);
		AssetDatabase.CreateAsset(avatar, avatar_name_);
	}
	
	static private Transform[] GetSelfAndChirdrenTransform(Transform transform) {
		List<Transform> result = new List<Transform>();
		result.Add(transform);
		if (0 < transform.childCount) {
			result.AddRange(Enumerable.Range(0, transform.childCount)
										.Select(x=>transform.GetChild(x))
										.SelectMany(x=>GetSelfAndChirdrenTransform(x))
							);
		}
		return result.ToArray();
	}

	GameObject					root_obj_		= null;
	string						avatar_name_	= "Assets/avatar.asset";
	Dictionary<string, string>	name_			= new Dictionary<string, string>(54);
	bool						display_head_			= true;
	bool						display_leg_			= true;
	bool						display_arm_			= true;
	bool						display_left_finger_	= false;
	bool						display_right_finger_	= false;

#if MFU_AVATART_SAVE_FILE
	Avatar						avatar_			= null;
#endif
}
