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
                public List<string> ToonTextures { get { return elements; } }

                public override void Read(System.IO.BinaryReader r)
                {
                    elements = ReadStrings(r, 10, 100);
                }
            }
        }
    }
}