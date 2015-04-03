using System.Collections;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class Header : Chunk
            {
                public string magic;
                public float version;
                public string modelName;
                public string comment;

                public override void Read(System.IO.BinaryReader r)
                {
                    magic = ReadString(r, 3);
                    version = ReadFloat(r);
                    modelName = ReadString(r, 20);
                    comment = ReadString(r, 256);
                }
            }
        }
    }
}