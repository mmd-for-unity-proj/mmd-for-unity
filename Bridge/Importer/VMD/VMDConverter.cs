using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using MMD.VMD;

namespace MMD
{
    public class VMDConverter
    {
        /// <summary>
        /// AnimationClipを作成する
        /// </summary>
        /// <param name='name'>内部形式データ</param>
        /// <param name='assign_pmd'>使用するPMDのGameObject</param>
        /// <param name='interpolationQuality'>補完曲線品質</param>
        public static AnimationClip CreateAnimationClip(VMDFormat format, GameObject assign_pmd, int interpolationQuality)
        {
            VMDConverter converter = new VMDConverter();
            return converter.CreateAnimationClip_(format, assign_pmd, interpolationQuality);
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        /// <remarks>
        /// ユーザーに依るインスタンス作成を禁止する
        /// </remarks>
        private VMDConverter() { }

        // クリップをアニメーションに登録する
        private AnimationClip CreateAnimationClip_(VMDFormat format, GameObject assign_pmd, int interpolationQuality)
        {
            //スケール設定
            scale_ = 1.0f;
            if (assign_pmd)
            {
                MMDEngine engine = assign_pmd.GetComponent<MMDEngine>();
                if (engine)
                {
                    scale_ = engine.scale;
                }
            }

            //Animation anim = assign_pmd.GetComponent<Animation>();

            // クリップの作成
            AnimationClip clip = new AnimationClip();
            clip.name = assign_pmd.name + "_" + format.name;

            Dictionary<string, string> bone_path = new Dictionary<string, string>();
            Dictionary<string, GameObject> gameobj = new Dictionary<string, GameObject>();
            GetGameObjects(gameobj, assign_pmd);		// 親ボーン下のGameObjectを取得
            FullSearchBonePath(assign_pmd.transform, bone_path);
            FullEntryBoneAnimation(format, clip, bone_path, gameobj, interpolationQuality);

            CreateKeysForSkin(format, clip);	// 表情の追加

            SetAnimationType(clip, assign_pmd); //アニメーションタイプの設定

            return clip;
        }

        // 補間曲線が線形補間と等価か
        public static bool IsLinear(byte[] interpolation, int type)
        {
            byte ax = interpolation[0 * 8 + type];
            byte ay = interpolation[0 * 8 + 4 + type];
            byte bx = interpolation[1 * 8 + type];
            byte by = interpolation[1 * 8 + 4 + type];
            return (ax == ay) && (bx == by);
        }

        // 補間曲線の近似のために追加するキーフレームを含めたキーフレーム数を取得する
        int GetKeyframeCount(List<VMDFormat.Motion> mlist, int type, int interpolationQuality)
        {
            int count = 0;
            for (int i = 0; i < mlist.Count; i++)
            {
                if (i > 0 && !IsLinear(mlist[i].interpolation, type))
                {
                    count += interpolationQuality;//Interpolation Keyframes
                }
                else
                {
                    count += 1;//Keyframe
                }
            }
            return count;
        }

        //キーフレームが1つの時、ダミーキーフレームを追加する
        void AddDummyKeyframe(ref Keyframe[] keyframes)
        {
            if (keyframes.Length == 1)
            {
                Keyframe[] newKeyframes = new Keyframe[2];
                newKeyframes[0] = keyframes[0];
                newKeyframes[1] = keyframes[0];
                newKeyframes[1].time += 0.001f / 60f;//1[ms]
                newKeyframes[0].outTangent = 0f;
                newKeyframes[1].inTangent = 0f;
                keyframes = newKeyframes;
            }
        }

        //移動の線形補間用tangentを求める 
        float GetLinearTangentForPosition(Keyframe from_keyframe, Keyframe to_keyframe)
        {
            return (to_keyframe.value - from_keyframe.value) / (to_keyframe.time - from_keyframe.time);
        }

        //-359～+359度の範囲を等価な0～359度へ変換する。
        float Mod360(float angle)
        {
            //剰余演算の代わりに加算にする
            return (angle < 0) ? (angle + 360f) : (angle);
        }

        //回転の線形補間用tangentを求める
        float GetLinearTangentForRotation(Keyframe from_keyframe, Keyframe to_keyframe)
        {
            float tv = Mod360(to_keyframe.value);
            float fv = Mod360(from_keyframe.value);
            float delta_value = Mod360(tv - fv);
            //180度を越える場合は逆回転
            if (delta_value < 180f)
            {
                return delta_value / (to_keyframe.time - from_keyframe.time);
            }
            else
            {
                return (delta_value - 360f) / (to_keyframe.time - from_keyframe.time);
            }
        }

        //アニメーションエディタでBothLinearを選択したときの値
        private const int TangentModeBothLinear = 21;

        //UnityのKeyframeに変換する（回転用）
        void ToKeyframesForRotation(QuaternionKeyframe[] custom_keys, ref Keyframe[] rx_keys, ref Keyframe[] ry_keys, ref Keyframe[] rz_keys)
        {
            rx_keys = new Keyframe[custom_keys.Length];
            ry_keys = new Keyframe[custom_keys.Length];
            rz_keys = new Keyframe[custom_keys.Length];
            for (int i = 0; i < custom_keys.Length; i++)
            {
                //オイラー角を取り出す
                Vector3 eulerAngles = custom_keys[i].value.eulerAngles;
                rx_keys[i] = new Keyframe(custom_keys[i].time, eulerAngles.x);
                ry_keys[i] = new Keyframe(custom_keys[i].time, eulerAngles.y);
                rz_keys[i] = new Keyframe(custom_keys[i].time, eulerAngles.z);
                //線形補間する
                rx_keys[i].tangentMode = TangentModeBothLinear;
                ry_keys[i].tangentMode = TangentModeBothLinear;
                rz_keys[i].tangentMode = TangentModeBothLinear;
                if (i > 0)
                {
                    float tx = GetLinearTangentForRotation(rx_keys[i - 1], rx_keys[i]);
                    float ty = GetLinearTangentForRotation(ry_keys[i - 1], ry_keys[i]);
                    float tz = GetLinearTangentForRotation(rz_keys[i - 1], rz_keys[i]);
                    rx_keys[i - 1].outTangent = tx;
                    ry_keys[i - 1].outTangent = ty;
                    rz_keys[i - 1].outTangent = tz;
                    rx_keys[i].inTangent = tx;
                    ry_keys[i].inTangent = ty;
                    rz_keys[i].inTangent = tz;
                }
            }
            AddDummyKeyframe(ref rx_keys);
            AddDummyKeyframe(ref ry_keys);
            AddDummyKeyframe(ref rz_keys);
        }

        void SetEditorCurveSingle(AnimationClip clip, string bone_path, System.Type type, string property_name, string element, AnimationCurve curve)
        {
#if UNITY_EDITOR
#if !UNITY_4_2
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(bone_path, typeof(Transform), property_name + element), curve);
#else
            AnimationUtility.SetEditorCurve(clip,bone_path,typeof(Transform),property_name + element,curve);
#endif
#else
            clip.SetCurve(bone_path, typeof(Transform), property_name + element, curve_x);
#endif
        }

        void SetEditorCurve(AnimationClip clip, string bone_path, System.Type type, string property_name, AnimationCurve curve_x, AnimationCurve curve_y, AnimationCurve curve_z)
        {
            SetEditorCurveSingle(clip, bone_path, type, property_name, ".x", curve_x);
            SetEditorCurveSingle(clip, bone_path, type, property_name, ".y", curve_y);
            SetEditorCurveSingle(clip, bone_path, type, property_name, ".z", curve_z);
        }

        // あるボーンに含まれるキーフレを抽出
        // これは回転のみ
        void CreateKeysForRotation(VMDFormat format, AnimationClip clip, string current_bone, string bone_path, int interpolationQuality)
        {
            try
            {
                List<VMDFormat.Motion> mlist = format.motion_list.motion[current_bone];
                int keyframeCount = GetKeyframeCount(mlist, 3, interpolationQuality);

                QuaternionKeyframe[] r_keys = new QuaternionKeyframe[keyframeCount];
                QuaternionKeyframe r_prev_key = null;
                int ir = 0;
                for (int i = 0; i < mlist.Count; i++)
                {
                    const float tick_time = 1.0f / 30.0f;
                    float tick = mlist[i].frame_no * tick_time;

                    Quaternion rotation = mlist[i].rotation;
                    QuaternionKeyframe r_cur_key = new QuaternionKeyframe(tick, rotation);
                    QuaternionKeyframe.AddBezierKeyframes(mlist[i].interpolation, 3, r_prev_key, r_cur_key, interpolationQuality, ref r_keys, ref ir);
                    r_prev_key = r_cur_key;
                }

                Keyframe[] rx_keys = null;
                Keyframe[] ry_keys = null;
                Keyframe[] rz_keys = null;
                ToKeyframesForRotation(r_keys, ref rx_keys, ref ry_keys, ref rz_keys);
                AnimationCurve curve_x = new AnimationCurve(rx_keys);
                AnimationCurve curve_y = new AnimationCurve(ry_keys);
                AnimationCurve curve_z = new AnimationCurve(rz_keys);
                const string property_name = "localEulerAngles";
                
                // ここで回転オイラー角をセット（補間はクォータニオン）
                SetEditorCurve(clip, bone_path, typeof(Transform), property_name, curve_x, curve_y, curve_z);
            }
                
            catch (KeyNotFoundException)
            {
                //Debug.LogError("互換性のないボーンが読み込まれました:" + bone_path);
            }
        }

        //UnityのKeyframeに変換する（移動用）
        Keyframe[] ToKeyframesForLocation(FloatKeyframe[] custom_keys)
        {
            Keyframe[] keys = new Keyframe[custom_keys.Length];
            for (int i = 0; i < custom_keys.Length; i++)
            {
                keys[i] = new Keyframe(custom_keys[i].time, custom_keys[i].value);
                //線形補間する
                keys[i].tangentMode = TangentModeBothLinear;
                if (i > 0)
                {
                    float t = GetLinearTangentForPosition(keys[i - 1], keys[i]);
                    keys[i - 1].outTangent = t;
                    keys[i].inTangent = t;
                }
            }
            AddDummyKeyframe(ref keys);
            return keys;
        }

        // 移動のみの抽出
        void CreateKeysForLocation(VMDFormat format, AnimationClip clip, string current_bone, string bone_path, int interpolationQuality, GameObject current_obj = null)
        {
            try
            {
                Vector3 default_position = Vector3.zero;
                if (current_obj != null)
                    default_position = current_obj.transform.localPosition;

                List<VMDFormat.Motion> mlist = format.motion_list.motion[current_bone];

                int keyframeCountX = GetKeyframeCount(mlist, 0, interpolationQuality);
                int keyframeCountY = GetKeyframeCount(mlist, 1, interpolationQuality);
                int keyframeCountZ = GetKeyframeCount(mlist, 2, interpolationQuality);

                FloatKeyframe[] lx_keys = new FloatKeyframe[keyframeCountX];
                FloatKeyframe[] ly_keys = new FloatKeyframe[keyframeCountY];
                FloatKeyframe[] lz_keys = new FloatKeyframe[keyframeCountZ];

                FloatKeyframe lx_prev_key = null;
                FloatKeyframe ly_prev_key = null;
                FloatKeyframe lz_prev_key = null;
                int ix = 0;
                int iy = 0;
                int iz = 0;
                for (int i = 0; i < mlist.Count; i++)
                {
                    const float tick_time = 1.0f / 30.0f;

                    float tick = mlist[i].frame_no * tick_time;

                    FloatKeyframe lx_cur_key = new FloatKeyframe(tick, mlist[i].location.x * scale_ + default_position.x);
                    FloatKeyframe ly_cur_key = new FloatKeyframe(tick, mlist[i].location.y * scale_ + default_position.y);
                    FloatKeyframe lz_cur_key = new FloatKeyframe(tick, mlist[i].location.z * scale_ + default_position.z);

                    // 各軸別々に補間が付いてる
                    FloatKeyframe.AddBezierKeyframes(mlist[i].interpolation, 0, lx_prev_key, lx_cur_key, interpolationQuality, ref lx_keys, ref ix);
                    FloatKeyframe.AddBezierKeyframes(mlist[i].interpolation, 1, ly_prev_key, ly_cur_key, interpolationQuality, ref ly_keys, ref iy);
                    FloatKeyframe.AddBezierKeyframes(mlist[i].interpolation, 2, lz_prev_key, lz_cur_key, interpolationQuality, ref lz_keys, ref iz);

                    lx_prev_key = lx_cur_key;
                    ly_prev_key = ly_cur_key;
                    lz_prev_key = lz_cur_key;
                }

                // 回転ボーンの場合はデータが入ってないはず
                if (mlist.Count != 0)
                {
                    AnimationCurve curve_x = new AnimationCurve(ToKeyframesForLocation(lx_keys));
                    AnimationCurve curve_y = new AnimationCurve(ToKeyframesForLocation(ly_keys));
                    AnimationCurve curve_z = new AnimationCurve(ToKeyframesForLocation(lz_keys));
                    const string property_name = "m_LocalPosition";
                    SetEditorCurve(clip, bone_path, typeof(Transform), property_name, curve_x, curve_y, curve_z);
                }
            }
            catch (KeyNotFoundException)
            {
                //Debug.LogError("互換性のないボーンが読み込まれました:" + current_bone);
            }
        }

        void CreateKeysForSkin(VMDFormat format, AnimationClip clip)
        {
            const float tick_time = 1f / 30f;

            // 全ての表情に打たれているキーフレームを探索
            List<VMD.VMDFormat.SkinData> s;

            foreach (var skin in format.skin_list.skin)
            {
                s = skin.Value;
                Keyframe[] keyframe = new Keyframe[skin.Value.Count];

                // キーフレームの登録を行う
                for (int i = 0; i < skin.Value.Count; i++)
                {
                    keyframe[i] = new Keyframe(s[i].frame_no * tick_time, s[i].weight);
                    //線形補間する
                    keyframe[i].tangentMode = TangentModeBothLinear;
                    if (i > 0)
                    {
                        float t = GetLinearTangentForPosition(keyframe[i - 1], keyframe[i]);
                        keyframe[i - 1].outTangent = t;
                        keyframe[i].inTangent = t;
                    }
                }
                AddDummyKeyframe(ref keyframe);

                // Z軸移動にキーフレームを打つ
                AnimationCurve curve = new AnimationCurve(keyframe);
                const string property_name = "m_LocalPosition";
                string skin_path = "Expression/" + skin.Key;
                SetEditorCurveSingle(clip, skin_path, typeof(Transform), property_name, ".z", curve);
            }
        }

        // ボーンのパスを取得する
        string GetBonePath(Transform transform)
        {
            string buf;
            if (transform.parent == null)
                return transform.name;
            else
                buf = GetBonePath(transform.parent);
            return buf + "/" + transform.name;
        }

        // ボーンの子供を再帰的に走査
        void FullSearchBonePath(Transform transform, Dictionary<string, string> dic)
        {
            int count = transform.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform t = transform.GetChild(i);
                FullSearchBonePath(t, dic);
            }

            // オブジェクト名が足されてしまうので抜く
            string buf = "";
            string[] spl = GetBonePath(transform).Split('/');
            for (int i = 1; i < spl.Length - 1; i++)
                buf += spl[i] + "/";
            buf += spl[spl.Length - 1];

            try
            {
                dic.Add(transform.name, buf);
            }
            catch (System.ArgumentException arg)
            {
                Debug.Log(arg.Message);
                Debug.Log("An element with the same key already exists in the dictionary. -> " + transform.name);
            }

            // dicには全てのボーンの名前, ボーンのパス名が入る
        }

        // 無駄なカーブを登録してるけどどうするか
        void FullEntryBoneAnimation(VMDFormat format, AnimationClip clip, Dictionary<string, string> dic, Dictionary<string, GameObject> obj, int interpolationQuality)
        {
            foreach (KeyValuePair<string, string> p in dic)	// keyはtransformの名前, valueはパス
            {
                // 互いに名前の一致する場合にRigidbodyが存在するか調べたい
                GameObject current_obj = null;
                if (obj.ContainsKey(p.Key))
                {
                    current_obj = obj[p.Key];

                    // Rigidbodyがある場合はキーフレの登録を無視する
                    var rigid = current_obj.GetComponent<Rigidbody>();
                    if (rigid != null && !rigid.isKinematic)
                    {
                        continue;
                    }
                }

                // キーフレの登録
                CreateKeysForLocation(format, clip, p.Key, p.Value, interpolationQuality, current_obj);
                CreateKeysForRotation(format, clip, p.Key, p.Value, interpolationQuality);
            }
        }

        // とりあえず再帰的に全てのゲームオブジェクトを取得する
        void GetGameObjects(Dictionary<string, GameObject> obj, GameObject assign_pmd)
        {
            for (int i = 0; i < assign_pmd.transform.childCount; i++)
            {
                var transf = assign_pmd.transform.GetChild(i);
                try
                {
                    obj.Add(transf.name, transf.gameObject);
                }
                catch (System.ArgumentException arg)
                {
                    Debug.Log(arg.Message);
                    Debug.Log("An element with the same key already exists in the dictionary. -> " + transf.name);
                }

                if (transf == null) continue;		// ストッパー
                GetGameObjects(obj, transf.gameObject);
            }
        }

        /// <summary>
        /// アニメーションタイプの設定
        /// </summary>
        /// <param name="clip">設定するアニメーションクリップ.</param>
        /// <param name="engine">設定の為に参照するAnimatorを持つゲームオブジェクト</param>
        static void SetAnimationType(AnimationClip clip, GameObject game_object)
        {
#if UNITY_EDITOR
            ModelImporterAnimationType animation_type;
            Animator animator = game_object.GetComponent<Animator>();
            if (null == animator)
            {
                animation_type = ModelImporterAnimationType.Legacy;
            }
            else if ((null == animator.avatar) && animator.avatar.isHuman)
            {
                animation_type = ModelImporterAnimationType.Human;
            }
            else
            {
                animation_type = ModelImporterAnimationType.Generic;
            }
            AnimationUtility.SetAnimationType(clip, animation_type);
#endif
        }

        private float scale_ = 1.0f;
    }
}
