using System.Collections;
using System.Collections.Generic;
using MMD.Format.Common;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class RigidbodyList : ChunkList<Rigidbody, uint> 
            {
                public List<Rigidbody> Rigidbodies { get { return elements; } }
            }

            public class Rigidbody : Chunk
            {
                public string name;
                public ushort boneIndex;
                public byte groupIndex;
                public ushort groupTarget;
                public byte shapeType;
                public Vector3 shape = new Vector3();
                public Vector3 position = new Vector3();
                public Vector3 rotation = new Vector3();
                public float weight;
                public float positionDiminish;
                public float rotationDiminish;
                public float recoil;
                public float friction;
                public byte rigidbodyType;

                public override void Read(System.IO.BinaryReader r)
                {
                    name = ReadString(r, 20);
                    boneIndex = ReadUShort(r);
                    groupIndex = ReadByte(r);
                    groupTarget = ReadUShort(r);
                    shapeType = ReadByte(r);
                    shape.Read(r);
                    position.Read(r);
                    rotation.Read(r);
                    weight = ReadFloat(r);
                    positionDiminish = ReadFloat(r);
                    rotationDiminish = ReadFloat(r);
                    recoil = ReadFloat(r);
                    friction = ReadFloat(r);
                    rigidbodyType = ReadByte(r);
                }
            }
        }
    }
}