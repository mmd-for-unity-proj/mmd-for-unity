using System.Collections;
using System.Collections.Generic;
using MMD.Format.Common;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class JointList : ChunkList<Joint, uint> 
            {
                public List<Joint> Joints { get { return elements; } }
            }

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
                    position.Read(r);
                    rotation.Read(r);
                    constrainPosition1.Read(r);
                    constrainPosition2.Read(r);
                    constrainRotation1.Read(r);
                    constrainRotation2.Read(r);
                    springPositoin.Read(r);
                    springRotation.Read(r);
                }
            }
        }
    }
}