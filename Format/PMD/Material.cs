using System.Collections;
using System.Collections.Generic;
using MMD.Format.Common;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MaterialList : ChunkList<Material, uint> 
            {
                public List<Material> Materials { get { return elements; } }
            }

            public class Material : Chunk
            {
                public Vector3 diffuse = new Vector3();
                public float alpha;
                public float specularity;
                public Vector3 specular = new Vector3();
                public Vector3 mirror = new Vector3();
                public byte toonIndex;
                public byte edgeFlag;
                public uint assignedFaceConut;
                public string textureFileName;

                public override void Read(System.IO.BinaryReader r)
                {
                    diffuse.Read(r);
                    alpha = ReadFloat(r);
                    specularity = ReadFloat(r);
                    specular.Read(r);
                    mirror.Read(r);
                    toonIndex = ReadByte(r);
                    edgeFlag = ReadByte(r);
                    assignedFaceConut = ReadUInt(r) / 3;
                    textureFileName = ReadString(r, 20);
                }
            }
        }
    }
}