using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class BoneDisplayList : ChunkList<BoneDisplay, uint> { }

            public class BoneDisplay : Chunk
            {
                public ushort boneIndex;
                public byte windowIndex;
            }
        }
    }
}