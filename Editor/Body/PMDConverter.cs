using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace MMD.Body.Converter
{
    public class PMDConverter
    {
        MMD.Format.PMDFormat format;
        MMD.Body.Argument.PMDArgument argument;

        string directory;
        string filename;

        public PMDConverter(MMD.Body.Argument.PMDArgument argument)
        {
            if (!argument.path.ToLower().Contains(".pmd"))
                throw new System.ArgumentException("PMDファイル以外のファイルがロードされました");

            format = new MMD.Format.PMDFormat();
            format.Read(argument.path, argument.scale);

            directory = System.IO.Path.GetDirectoryName(argument.path);
            filename = System.IO.Path.GetFileNameWithoutExtension(argument.path);

            this.argument = argument;
        }

        GameObject CreateRoot()
        {
            var root = new GameObject(format.Header.modelName);

            return root;
        }

        void EntryMesh(Mesh mesh)
        {
            AssetDatabase.CreateAsset(mesh, directory + "/" + filename + ".asset");
        }

        void ExistsAsCreateDirectory(string dirname)
        {
            if (!System.IO.Directory.Exists(directory + "/" + dirname))
                AssetDatabase.CreateFolder(directory, dirname);
        }

        void EntryMaterials(Material[] materials)
        {
            ExistsAsCreateDirectory("Materials");

            for (int i = 0; i < materials.Length; ++i)
            {
                AssetDatabase.CreateAsset(materials[i], directory + "/Materials/材質" + (i + 1).ToString() + ".mat");
            }
        }

        void EntryPhysicMaterials(List<MMD.Builder.PMD.Physics> materials)
        {
            ExistsAsCreateDirectory("Physics");

            for (int i = 0; i < materials.Count; ++i)
            {
                AssetDatabase.CreateAsset(materials[i].material, directory + "/Physics/" + materials[i].name + ".physicMaterial");
            }
        }

        void EntryPrefab(GameObject root)
        {
            //AssetDatabase.CreateAsset(root, directory + "/" + filename + ".prefab");
            PrefabUtility.CreatePrefab(directory + "/" + filename + ".prefab", root);
        }

        public void Import()
        {
            var root = CreateRoot();

            var renderer = root.AddComponent<SkinnedMeshRenderer>();
            var builder = new MMD.Builder.PMD.ModelBuilder(renderer);
            builder.Read(format, argument.shader);

            // ここより下でアセットの登録を行う
            EntryMesh(builder.Mesh);
            EntryMaterials(builder.Materials);
            EntryPhysicMaterials(builder.Physics);

            builder.RootBone.transform.parent = root.transform;
            
            // マテリアルを設定
            builder.Renderer.sharedMaterials = builder.Materials;

            // 登録したアセットを読み込む
            renderer.sharedMesh = AssetDatabase.LoadAssetAtPath(directory + "/" + filename + ".asset", typeof(Mesh)) as Mesh;

            // 剛体をルートと接続
            //builder.RigidbodyRoot.transform.parent = root.transform;

            EntryPrefab(root);
        }
    }
}
