using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class IKList
            {
                public uint ikCount;
                public List<IK> iks;
            }

            public class IK
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