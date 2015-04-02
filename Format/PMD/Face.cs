using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class FaceList
            {
                public uint faceCount;
                public List<Face> faces;
            }

            public class Face
            {
                public ushort[] vertexIndices;
            }
        }
    }
}