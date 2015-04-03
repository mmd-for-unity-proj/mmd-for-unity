using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class ToonTexture : StringChunkList
            {
                public override void Read(System.IO.BinaryReader r)
                {
                    ReadStrings(r, elements, 10, 100);
                }
            }
        }
    }
}