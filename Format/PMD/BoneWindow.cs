using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class BoneWindowList : StringChunkList
            {
                public List<string> BoneWindows { get { return elements; } }

                public override void Read(System.IO.BinaryReader r)
                {
                    int count = ReadByte(r);
                    ReadStrings(r, elements, count, 50);
                }
            }
        }
    }
}