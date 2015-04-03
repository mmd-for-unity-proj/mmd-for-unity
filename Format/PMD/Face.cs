using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class FaceList : ChunkList<Face, uint> { }

            public class Face : Chunk
            {
                public ushort[] vertexIndices;

                public override void Read(System.IO.BinaryReader r)
                {
                    vertexIndices = new ushort[3];
                    for (int i = 0; i < 3; ++i)
                        vertexIndices[i] = ReadUShort(r);
                }
            }
        }
    }
}