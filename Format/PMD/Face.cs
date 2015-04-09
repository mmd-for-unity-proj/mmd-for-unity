using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class FaceList : ChunkList<Face, uint> 
            {
                public List<Face> Faces { get { return elements; } }

                public override void Read(System.IO.BinaryReader r)
                {
                    var count = ReadCount<uint>(r) / 3;
                    elements = new List<Face>(count);

                    for (int i = 0; i < count; ++i)
                    {
                        var elem = new Face();
                        elem.Read(r);
                        elements.Add(elem);
                    }
                }
            }

            public class Face : Chunk
            {
                public ushort[] vertexIndices;

                public ushort this[int i] { get { return vertexIndices[i]; } set { vertexIndices[i] = value; } }

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