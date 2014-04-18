using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ModelGrabManager : MonoBehaviour
{
    enum Mode
    {
        Rotate,
        Translate
    }

    GameObject grab;
    GameObject target = null;
    bool is_ik;
    Mode mode = Mode.Rotate;

    Vector3 prev_mouse_pos;
    Vector3 moving_vector;

    void Start()
    {
        grab = new GameObject("GrabCircle");
        grab.transform.position = new Vector3(-1f, -1f, 0);
        grab.AddComponent<GrabCircle>();
        prev_mouse_pos = Input.mousePosition;
    }

    void Update()
    {
        moving_vector = Input.mousePosition - prev_mouse_pos;
        ChangeGrabMode(mode, KeyCode.Space);
        Grabbing(mode, target, ref moving_vector);
        prev_mouse_pos = Input.mousePosition;
    }

    void Grabbing(Mode mode, GameObject target, ref Vector3 moving_vector)
    {
        if (Input.GetMouseButton(0) && target != null)
        {
            ToEachMode(mode, target, ref moving_vector);
        }
    }

    void ToEachMode(Mode mode, GameObject target, ref Vector3 moving_vector)
    {
        switch (mode)
        {
            case Mode.Rotate:
                DoRotate(target, ref moving_vector);
                break;

            case Mode.Translate:
                DoRotate(target, ref moving_vector);
                break;
        }
    }

    void DoRotate(GameObject target, ref Vector3 moving_vector)
    {

    }

    void DoTranslate(GameObject target, ref Vector3 moving_vector)
    {

    }

    void ChangeGrabMode(Mode grab_mode, KeyCode kcode)
    {
        if (Input.GetKeyDown(kcode))
        {
            if (grab_mode == Mode.Rotate)
                grab_mode = Mode.Translate;
            else
                grab_mode = Mode.Rotate;
        }
    }

    /// <summary>
    /// 引数でボーンを指定し，円を表示させる
    /// </summary>
    /// <param name="target_bone"></param>
    void SelectedBoneAsViewCircle(GameObject target_bone)
    {
        var screen_point = Camera.main.ViewportToScreenPoint(target_bone.transform.position);
        grab.transform.position = screen_point;
        target = target_bone;
    }

    /// <summary>
    /// 色を返る
    /// </summary>
    /// <param name="color"></param>
    void ChangeColor(Color color)
    {
        grab.guiTexture.color = color;
    }
}
