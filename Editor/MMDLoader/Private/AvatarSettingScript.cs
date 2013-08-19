#if (UNITY_4_0 || UNITY_4_1)

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

// アバターの設定を行うスクリプト
public class AvatarSettingScript 
{
    GameObject root_object;
    Animator animator;

    //Transform avt_root;
    //Transform avt_left_hand;
    //Transform avt_right_hand;
    //Transform avt_left_foot;
    //Transform avt_right_foot;
    //Transform avt_body;

    // root_objectはMMDEngineが入っているオブジェクト
    public AvatarSettingScript(GameObject root_object)
    {
        this.root_object = root_object;
    }

    public void SettingAvatar()
    {
        animator = root_object.AddComponent<Animator>();
        animator.InterruptMatchTarget();	// 強制的にAvatarのターゲットを設定

        SetIKs();

        //// 各主要部位のTransformを取得してくる
        //avt_root = FindChild("センター", root_object.transform);
        //avt_right_hand = FindChild("右手首", root_object.transform);
        //avt_left_hand = FindChild("左手首", root_object.transform);
        //avt_right_foot = FindChild("右足首", root_object.transform);
        //avt_left_foot = FindChild("左足首", root_object.transform);
        //avt_body = FindChild("上半身", root_object.transform);

        //Debug.Log(animator.IsControlled(avt_root));
        //Debug.Log(avt_right_hand);

        //// Animatorに設定
        //Match(avt_root,			AvatarTarget.Root);
        //Match(avt_right_hand,	AvatarTarget.RightHand);
        //Match(avt_left_hand,	AvatarTarget.LeftHand);
        //Match(avt_right_foot,	AvatarTarget.RightFoot);
        //Match(avt_left_foot,	AvatarTarget.LeftFoot);
        //Match(avt_body,			AvatarTarget.Body);
    }

    void SetIKs()
    {
        var left_foot_ik = FindChild("左足ＩＫ", root_object.transform);
        var right_foot_ik = FindChild("右足ＩＫ", root_object.transform);
        var left_hand_ik = FindChild("左手ＩＫ", root_object.transform);
        var right_hand_ik = FindChild("右手ＩＫ", root_object.transform);

        SetIK(left_foot_ik, AvatarIKGoal.LeftFoot);
        SetIK(right_foot_ik, AvatarIKGoal.RightFoot);
        SetIK(left_hand_ik, AvatarIKGoal.LeftHand);
        SetIK(right_hand_ik, AvatarIKGoal.RightHand);
    }

    void SetIK(Transform target, AvatarIKGoal ik_goal)
    {
        if (target != null)
        {
            animator.SetIKPosition(ik_goal, target.position);
            animator.SetIKRotation(ik_goal, target.rotation);
        }
    }

    // 特定の名前の子供を再帰的に調べて取得してくる
    Transform FindChild(string name, Transform target)
    {
        var find_result = target.FindChild(name);

        if (find_result == null)
        {
            for (int i = 0; i < target.childCount; i++)
            {
                find_result = FindChild(name, target.GetChild(i));
                if (find_result != null) return find_result;
            }
        }

        return find_result;
    }

    // AvatarTargetにマッチした部位の設定を行う
    void Match(Transform target, AvatarTarget type)
    {
        animator.MatchTarget(
            target.transform.position,
            target.transform.rotation,
            type,
            new MatchTargetWeightMask(Vector3.one, 1f),
            1);

        if (!animator.IsControlled(target))
            Debug.LogError("avatar is not controlled target: " + target.name);
        else
            Debug.Log("controlled: " + target.name);
    }
}

#endif