using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class RigidbodyList : ChunkList<Rigidbody, uint> { }

            public class Rigidbody : Chunk
            {
                public string name;
                public ushort boneIndex;
                public byte groupIndex;
                public ushort groupTarget;
                public byte shapeType;
                public Vector3 shape;
                public Vector3 position;
                public Vector3 rotation;
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
                    shape = ReadVector3(r);
                    position = ReadVector3(r);
                    rotation = ReadVector3(r);
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