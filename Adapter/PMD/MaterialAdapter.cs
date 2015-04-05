using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;

namespace MMD.Adapter.PMD
{
    public class MaterialAdapter
    {
        public Material[] Materials { get; set; }
        public List<Texture2D> Textures { get; set; }

        public MaterialAdapter(Shader shader, MMD.Format.PMDFormat format)
        {
            var source = format.Materials;

            Materials = new Material[source.Count];
            Textures = new List<Texture2D>();

            for (int i = 0; i < source.Count; ++i)
                Materials[i] = AddMaterial(shader, source[i], format.Path);
        }

        Color ToColor(MMD.Format.Common.Vector3 v)
        {
            return new Color(v.x, v.y, v.z);
        }

        string[] AddTexture(MMD.Format.PMD.Material source, string path)
        {
            var directory = Path.GetDirectoryName(path);
            var textures = source.textureFileName.Split('*');

            for (int i = 0; i < textures.Length; ++i)
            {
                var texturePath = directory + "/" + textures[i];
                Textures.Add(Resources.Load(texturePath) as Texture2D);
            }

            return textures;
        }

        void SetTexture(Material material, MMD.Format.PMD.Material source, string path)
        {
            var textures = AddTexture(source, path);
            for (int i = 0; i < textures.Length; ++i)
            {
                var textureRef = Textures[Textures.Count - textures.Length + i];

                if (textures[i].Contains(".sph"))
                {
                    material.SetInt("SphereAdd", 0);    // 乗算
                    material.SetTexture("SphereMap", textureRef);
                }
                else if (textures[i].Contains(".spa"))
                {
                    material.SetInt("SphereAdd", 1);    // 加算
                    material.SetTexture("SphereMap", textureRef);
                }
                else
                {
                    material.SetInt("SphereAdd", 0);
                    material.SetTexture("Texture", textureRef);
                }
            }
        }

        Material AddMaterial(Shader shader, MMD.Format.PMD.Material source, string path)
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

            SetTexture(material, source, path);

            return material;
        }


    }
}
