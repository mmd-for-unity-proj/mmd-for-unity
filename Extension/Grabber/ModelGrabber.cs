using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ModelGrabber : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    void CreateGrabObject()
    {
        var grab = new GameObject("grab");
        grab.AddComponent<MeshRenderer>();
        var shader = Shader.Find("");
    }
}
