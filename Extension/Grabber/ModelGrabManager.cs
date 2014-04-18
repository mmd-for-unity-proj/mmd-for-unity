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
    bool is_ik;
    Mode mode = Mode.Rotate;

    void Start()
    {
        grab = new GameObject("GrabCircle");
        grab.transform.position = new Vector3(-1f, -1f, 0);
        grab.AddComponent<GrabCircle>();
    }

    void Update()
    {
        ChangeGrabMode(mode, KeyCode.Space);
        Grabbing(mode);
    }

    void Grabbing(Mode mode)
    {
        switch (mode)
        {
            case Mode.Rotate:
                break;

            case Mode.Translate:
                break;
        }
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

    void ViewCircle(GameObject target_bone)
    {
        var screen_point = Camera.main.ViewportToScreenPoint(target_bone.transform.position);
        grab.transform.position = screen_point;
    }
}
