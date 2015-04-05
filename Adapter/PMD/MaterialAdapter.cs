using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class MaterialAdapter
    {
        public List<Material> Materials { get; set; }
        public List<Texture> Textures { get; set; }

        public MaterialAdapter(Shader shader, List<MMD.Format.PMD.Material> materials)
        {
            Materials = new List<Material>(materials.Count);
            Textures = new List<Texture>();

            for (int i = 0; i < materials.Count; ++i)
                Materials.Add(AddMaterial(shader, materials[i]));
        }

        Material AddMaterial(Shader shader, MMD.Format.PMD.Material source)
        {
            var material = new Material(shader);

            return material;
        }


    }
}
