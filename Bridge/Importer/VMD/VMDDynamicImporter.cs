using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD
{
    namespace VMD
    {
        public class VMDDynamicImporter
        {
            public static void Import(GameObject pmd_object, byte[] data, string clip_name)
            {
                var format = VMDFormatFactory.Import(data);
                var clip = VMDConverter.CreateAnimationClip(format, pmd_object, 1);
                var animation = pmd_object.GetComponent<Animation>();
                animation.AddClip(clip, clip_name);
            }
        }
    }
}
