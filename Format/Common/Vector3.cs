using System.Collections;

namespace MMD
{
    namespace Format
    {
        namespace Common
        {
            public class Vector3 : Chunk
            {
                public float x, y, z;

                public Vector3() { }

                public Vector3(float x, float y, float z)
                {
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }

                public override void Read(System.IO.BinaryReader r)
                {
                    x = ReadFloat(r);
                    y = ReadFloat(r);
                    z = ReadFloat(r);
                }

                public override void Read(System.IO.BinaryReader r, float scale)
                {
                    x = ReadFloat(r) * scale;
                    y = ReadFloat(r) * scale;
                    z = ReadFloat(r) * scale;
                }

                public void Add(Vector3 v)
                {
                    x += v.x;
                    y += v.y;
                    z += v.z;
                }

                public void Sub(Vector3 v)
                {
                    x -= v.x;
                    y -= v.y;
                    z -= v.z;
                }
            }
        }
    }
}