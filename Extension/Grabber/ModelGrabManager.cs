using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ModelGrabManager : MonoBehaviour
{
    GameObject grab;

    bool is_ik;

    enum Mode
    {
        Rotate,
        Translate
    }
    Mode mode = Mode.Rotate;

    void Start()
    {
        grab = new GameObject("GrabCircle");
        grab.transform.position = new Vector3(-1f, -1f, 0);
        grab.AddComponent<GrabCircle>();
    }

    void Update()
    {
        ChangeGrabMode(mode);
    }

    void ChangeGrabMode(Mode mode)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (mode == Mode.Rotate)
                mode = Mode.Translate;
            else
                mode = Mode.Rotate;
        }
    }

    void ViewCircle(GameObject target_bone)
    {
        var screen_point = Camera.main.ViewportToScreenPoint(target_bone.transform.position);
        grab.transform.position = screen_point;
    }
}
