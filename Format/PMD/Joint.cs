using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class JointList : ChunkList<Joint, uint> { }

            public class Joint : Chunk
            {
                public string name;
                public uint rigidbodyA;
                public uint rigidbodyB;
                public Vector3 position;
                public Vector3 rotation;
                public Vector3 constrainPosition1;
                public Vector3 constrainPosition2;
                public Vector3 constrainRotation1;
                public Vector3 constrainRotation2;
                public Vector3 springPositoin;
                public Vector3 springRotation;

                public override void Read(System.IO.BinaryReader r)
                {
                    name = ReadString(r, 20);
                    rigidbodyA = ReadUInt(r);
                    rigidbodyB = ReadUInt(r);
                    position = ReadVector3(r);
                    rotation = ReadVector3(r);
                    constrainPosition1 = ReadVector3(r);
                    constrainPosition2 = ReadVector3(r);
                    constrainRotation1 = ReadVector3(r);
                    constrainRotation2 = ReadVector3(r);
                    springPositoin = ReadVector3(r);
                    springRotation = ReadVector3(r);
                }
            }
        }
    }
}