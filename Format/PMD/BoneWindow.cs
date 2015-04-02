using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class BoneWindowList : ChunkStructList<string, byte> 
            {
                public override void Read(System.IO.BinaryReader r)
                {
                    int count = ReadCount(r);
                    ReadStrings(r, elements, count, 50);
                }
            }
        }
    }
}