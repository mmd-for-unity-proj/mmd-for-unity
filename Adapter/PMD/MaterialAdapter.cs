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
        public List<Material> Materials { get; set; }
        public List<Texture2D> Textures { get; set; }

        Texture2D[] toons;

        public MaterialAdapter()
        {
            toons = new Texture2D[11];

            toons[0] = null;
            for (int i = 1; i < 10; ++i)
            {
                toons[i] = Resources.Load<Texture2D>("Toon/toon0" + i.ToString() + ".bmp");
            }
            toons[10] = Resources.Load<Texture2D>("Toon/toon10.bmp");
        }

        public void Read(Shader shader, MMD.Format.PMDFormat format)
        {
            var source = format.Materials;

            Materials = new List<Material>(source.Count);
            Textures = new List<Texture2D>();

            for (int i = 0; i < source.Count; ++i)
                Materials.Add(AddMaterial(shader, source[i], format.Path));
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
                    material.SetInt("_SphereAdd", 0);    // 乗算
                    material.SetTexture("_SphereMap", textureRef);
                }
                else if (textures[i].Contains(".spa"))
                {
                    material.SetInt("_SphereAdd", 1);    // 加算
                    material.SetTexture("_SphereMap", textureRef);
                }
                else
                {
                    material.SetInt("_SphereAdd", 0);
                    material.SetTexture("_Texture", textureRef);
                }
            }
        }

        Material AddMaterial(Shader shader, MMD.Format.PMD.Material source, string path)
        {
            var material = new Material(shader);

            material.SetColor("_Diffuse", ToColor(source.diffuse));
            material.SetColor("_Ambient", ToColor(source.ambient));
            material.SetColor("_Specular", ToColor(source.specular));
            material.SetFloat("_Specularity", source.specularity);
            material.SetInt("_Edge Flag", source.edgeFlag);

            if (source.SelfShadow)
                material.SetFloat("_Transparency", 1.0f);
            else
                material.SetFloat("_Transparency", 1.0f - source.alpha);  // 不透明な度合いなので逆転させる

            material.SetInt("_SelfShadow", source.SelfShadow ? 1 : 0);
            material.SetTexture("_ToonTexture", toons[source.ToonIndex]);
            material.SetInt("_BackCull", source.BackFaceCulling ? 1 : 0);

            SetTexture(material, source, path);

            return material;
        }


    }
}
