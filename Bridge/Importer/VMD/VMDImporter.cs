using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD
{
    namespace VMD
    {
        public class VMDImporter
        {
            /// <summary>
            /// VMDファイル形式のバイナリデータをインポートする
            /// </summary>
            /// <param name="pmd_object">PMD/PMXから変換されたGameObject</param>
            /// <param name="data">VMDファイルのバイナリ</param>
            /// <param name="clip_name">AnimationClipに付けたいモーション名</param>
            public static void Import(GameObject pmd_object, byte[] data, string clip_name)
            {
                var format = VMDFormatFactory.Import(data);
                var clip = VMDConverter.CreateAnimationClip(format, pmd_object, 1);
                var animation = pmd_object.GetComponent<Animation>();
                if (animation != null)
                    animation.AddClip(clip, clip_name);
                else
                    Debug.Log("Failed to import " + clip_name + " for " + pmd_object.name + ".");
            }
        }
    }
}
