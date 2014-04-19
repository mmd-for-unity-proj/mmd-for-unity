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
    BoneController controller;
    bool is_ik;
    Mode grab_mode = Mode.Rotate;

    Vector3 prev_mouse_pos;
    Vector3 moving_vector;
    Transform camera_t;

    void Start()
    {
        grab = new GameObject("GrabCircle");
        grab.transform.position = new Vector3(-1f, -1f, 0);
        grab.AddComponent<GrabCircle>();
        prev_mouse_pos = Input.mousePosition;
        camera_t = Camera.main.transform;
    }

    void Update()
    {
        moving_vector = Input.mousePosition - prev_mouse_pos;
        ChangeGrabMode(KeyCode.Space);
        Grabbing(ref moving_vector);
        prev_mouse_pos = Input.mousePosition;
    }

    void Grabbing(ref Vector3 moving_vector)
    {
        if (Input.GetMouseButton(0) && target != null && controller.operatable && controller.display_flag)
        {
            moving_vector.x = moving_vector.x / Screen.width;
            moving_vector.y = moving_vector.y / Screen.height;  // 相対的な座標に置き換える
            var distance = (target.transform.position - camera_t.position).magnitude;

            ToEachMode(ref moving_vector, distance);
        }
    }

    void ToEachMode(ref Vector3 moving_vector, float distance)
    {
        switch (grab_mode)
        {
            case Mode.Rotate:
                DoRotate(ref moving_vector, distance);
                break;

            case Mode.Translate:
                DoRotate(ref moving_vector, distance);
                break;
        }
    }

    void DoRotate(ref Vector3 moving_vector, float distance)
    {
        // distanceが近いほど回転量が大きくなる，遠いと小さくなる
        var moving_power = 1f / distance;
    }

    void DoTranslate(ref Vector3 moving_vector, float distance)
    {
        // distanceが近いほど移動量が小さくなる，遠いと大きくなる
        var moving_power = Mathf.Log10(distance);
    }

    void ChangeGrabMode(KeyCode kcode)
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
        controller = target.GetComponent<BoneController>();
    }

    /// <summary>
    /// 色を変える
    /// </summary>
    /// <param name="color"></param>
    void ChangeColor(Color color)
    {
        grab.guiTexture.color = color;
    }
}
