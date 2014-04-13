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
            public static void Import(GameObject pmd_object, byte[] data)
            {
                var format = VMDFormatFactory.Import(data);
                VMDConverter.CreateAnimationClip(format, pmd_object, 1);
            }
        }
    }
}
