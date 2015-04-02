using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class BoneList : ChunkList<Bone, ushort> { }

            public class Bone : Chunk
            {
                public string boneName;
                public ushort parentBoneIndex;
                public ushort tailBoneIndex;
                public byte boneType;
                public ushort ikBoneIndex;
                public Vector3 position;
            }
        }
    }
}