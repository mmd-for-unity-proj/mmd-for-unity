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
                public Vector3 ambient = new Vector3();

                /// <summary>
                /// toon??.bmp
                /// 0 == 0xFF, 1 == 0x00, 10 == 0x09
                /// </summary>
                public byte toonIndex;

                public byte edgeFlag;
                public uint assignedFaceConut;
                public string textureFileName;

                /// <summary>
                /// toon??.bmpのインデックスに修正した値を返す
                /// </summary>
                public int ToonIndex { get { return toonIndex == 0xFF ? 0 : toonIndex - 1; } }

                /// <summary>
                /// セルフシャドウの有無
                /// </summary>
                public bool SelfShadow { get { return alpha == 0.98f; } }

                /// <summary>
                /// 両面描画フラグ
                /// </summary>
                public bool BackFaceCulling { get { return alpha == 1.0f; } }

                public override void Read(System.IO.BinaryReader r)
                {
                    diffuse.Read(r);
                    alpha = ReadFloat(r);
                    specularity = ReadFloat(r);
                    specular.Read(r);
                    ambient.Read(r);
                    toonIndex = ReadByte(r);
                    edgeFlag = ReadByte(r);
                    assignedFaceConut = ReadUInt(r) / 3;
                    textureFileName = ReadString(r, 20);
                }
            }
        }
    }
}