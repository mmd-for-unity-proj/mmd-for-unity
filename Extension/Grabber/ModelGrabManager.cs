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
        ChangeGrabMode(mode, KeyCode.Space);
        Grabbing(mode, target, ref moving_vector);
        prev_mouse_pos = Input.mousePosition;
    }

    void Grabbing(Mode mode, GameObject target, ref Vector3 moving_vector)
    {
        if (Input.GetMouseButton(0) && target != null)
        {
            moving_vector.x = moving_vector.x / Screen.width;
            moving_vector.y = moving_vector.y / Screen.height;  // 相対的な座標に置き換える
            var distance = (target.transform.position - camera_t.position).magnitude;

            ToEachMode(mode, target, ref moving_vector, distance);
        }
    }

    void ToEachMode(Mode mode, GameObject target, ref Vector3 moving_vector, float distance)
    {
        switch (mode)
        {
            case Mode.Rotate:
                DoRotate(target, ref moving_vector, distance);
                break;

            case Mode.Translate:
                DoRotate(target, ref moving_vector, distance);
                break;
        }
    }

    void DoRotate(GameObject target, ref Vector3 moving_vector, float distance)
    {
        // distanceが近いほど回転量が大きくなる，遠いと小さくなる
        var moving_power = 1f / distance;
    }

    void DoTranslate(GameObject target, ref Vector3 moving_vector, float distance)
    {
        // distanceが近いほど移動量が小さくなる，遠いと大きくなる
        var moving_power = Mathf.Log10(distance);
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
