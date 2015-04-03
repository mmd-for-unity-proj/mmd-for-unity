using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class MorphDisplayList : StructChunkList<ushort, byte> 
            {
                public override void Read(System.IO.BinaryReader r)
                {
                    var count = ReadCount<byte>(r);
                    elements = new List<ushort>(count);
                    for (int i = 0; i < count; ++i)
                        elements.Add(ReadUShort(r));
                }
            }
        }
    }
}