using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class EnglishHeader : Chunk
            {
                public byte nameCompability;
                public string modelName;
                public string comment;
            }

            public class EnglishBone : Chunk
            {
                public List<string> boneNames;
            }

            public class EnglishMorph : Chunk
            {
                public List<string> morphNames;
            }

            public class EnglishBoneWindow : Chunk
            {
                public List<string> boneWindows;
            }
        }
    }
}