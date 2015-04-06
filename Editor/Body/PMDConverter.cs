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

        public PMDConverter(string path)
        {
            if (!path.ToLower().Contains(".pmd"))
                throw new System.ArgumentException("PMDファイル以外のファイルがロードされました");

            format = new MMD.Format.PMDFormat();
            format.Read(path);
        }

        GameObject CreateRoot()
        {
            var root = new GameObject(format.Header.modelName);
            var boneRoot = new GameObject("Bones");
            var rigidbodyRoot = new GameObject("Rigidbodies");

            boneRoot.transform.parent = root.transform;
            rigidbodyRoot.transform.parent = root.transform;

            return root;
        }

        public void Import(Shader shader, float scale)
        {
            var root = CreateRoot();
            var renderer = root.AddComponent<SkinnedMeshRenderer>();

            var builder = new MMD.Builder.PMD.ModelBuilder(renderer);
            builder.Read(format, shader, scale);

            // ここより下でアセットの登録を行う
            
            // マテリアルの生成/登録
            // 剛体の生成/登録
        }
    }
}
