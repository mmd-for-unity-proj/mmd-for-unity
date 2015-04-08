using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Body.Argument
{
    /// <summary>
    /// 引数をパックするクラス
    /// </summary>
    public class PMDArgument
    {
        public string path;
        public float scale;
        public Shader shader;

        public PMDArgument(string path, float scale, Shader shader)
        {
            this.path = path;
            this.scale = scale;
            this.shader = shader;
        }
    }
}
