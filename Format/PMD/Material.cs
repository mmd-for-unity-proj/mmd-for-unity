using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MaterialList : ChunkList<Material, uint> { }

            public class Material : Chunk
            {
                public Vector3 diffuse;
                public float alpha;
                public float specularity;
                public Vector3 specular;
                public Vector3 mirror;
                public byte toonIndex;
                public byte edgeFlag;
                public uint assignedFaceConut;
                public string textureFileName;

                public override void Read(System.IO.BinaryReader r)
                {
                    diffuse = ReadVector3(r);
                    alpha = ReadFloat(r);
                    specularity = ReadFloat(r);
                    specular = ReadVector3(r);
                    mirror = ReadVector3(r);
                    toonIndex = ReadByte(r);
                    edgeFlag = ReadByte(r);
                    assignedFaceConut = ReadUInt(r);
                    textureFileName = ReadString(r, 20);
                }
            }
        }
    }
}