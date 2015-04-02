using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class IKList : ChunkList<Face, uint> { }

            public class IK : Chunk
            {
                public ushort boneIndex;
                public ushort targetBoneIndex;
                public byte chainLength;
                public ushort iterations;
                public float controlWeight;
                public ushort[] childBoneIndices;
            }
        }
    }
}