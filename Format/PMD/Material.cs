using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MaterialList
            {
                public uint materialCount;
                public List<Face> materials;
            }

            public class Material
            {
                Vector3 diffuse;
                float alpha;
                float specularity;
                Vector3 specular;
                Vector3 mirror;
                byte toonIndex;
                byte edgeFlag;
                uint assignedFaceConut;
                string textureFileName;
            }
        }
    }
}