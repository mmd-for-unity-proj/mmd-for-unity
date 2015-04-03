using System.Collections;

namespace MMD
{
    namespace Format
    {
        namespace Common
        {
            public class Vector2 : Chunk
            {
                public float x, y;

                public override void Read(System.IO.BinaryReader r)
                {
                    x = ReadFloat(r);
                    y = ReadFloat(r);
                }
            }
        }
    }
}