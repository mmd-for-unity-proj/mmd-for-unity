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

        Color ToColor(MMD.Format.Common.Vector3 v)
        {
            return new Color(v.x, v.y, v.z);
        }

        Material AddMaterial(Shader shader, MMD.Format.PMD.Material source)
        {
            var material = new Material(shader);

            material.SetColor("Diffuse", ToColor(source.diffuse));
            material.SetColor("Ambient", ToColor(source.ambient));
            material.SetColor("Specular", ToColor(source.specular));
            material.SetFloat("Specularity", source.specularity);

            if (source.SelfShadow)
                material.SetFloat("Transparency", 1.0f);
            else
                material.SetFloat("Transparency", 1.0f - source.alpha);  // 不透明な度合いなので逆転させる

            material.SetInt("SelfShadow", source.SelfShadow ? 1 : 0);

            return material;
        }


    }
}
