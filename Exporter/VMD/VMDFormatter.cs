using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMD.VMD;

class VMDFormatter
{
    GameObject mmd_object;
    VMDFormat format;

    public VMDFormatter(GameObject target)
    {
        mmd_object = target;
        format = new VMDFormat();
        format.motion_list = new VMDFormat.MotionList();
        format.skin_list = new VMDFormat.SkinList();
        format.camera_list = new VMDFormat.CameraList();
        format.light_list = new VMDFormat.LightList();
        format.self_shadow_list = new VMDFormat.SelfShadowList();
    }

    public VMDFormat InsertMorph(int insert_frame_no)
    {
        var expression = mmd_object.transform.FindChild("Expression");
        var expressions = new List<Transform>();
        for (int i = 0; i < expression.childCount; i++)
            expressions.Add(expression.GetChild(i));

        

        return format;
    }
}