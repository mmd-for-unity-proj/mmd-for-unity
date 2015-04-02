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
                public float rotateDiminish;
                public float recoil;
                public float friction;
                public byte rigidbodyType;
            }
        }
    }
}