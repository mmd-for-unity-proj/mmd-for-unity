using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GrabCircle : MonoBehaviour
{
    void Start()
    {
        const float size = 64f;
        gameObject.transform.localScale = new Vector3(0, 0, 1);
        gameObject.AddComponent<GUITexture>();
        gameObject.guiTexture.texture = Resources.Load("white_circle") as Texture2D;
        gameObject.guiTexture.color = new Color(1f, 0f, 0f, 0.25f);
        gameObject.guiTexture.pixelInset = new Rect(-size * 0.5f, -size * 0.5f, size, size);
    }

    void Update()
    {

    }
}
