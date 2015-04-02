using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MorphList
            {
                public uint morphCount;
                public List<Morph> morphs;
            }

            public class Morph
            {
                public string name;
                public uint vertexCount;
                public byte morphType;
                public List<MorphVertex> vertices;
            }

            public class MorphVertex
            {
                public uint vertexIndex;
                public Vector3 offset;
            }
        }
    }
}