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
                public Vector3 position = new Vector3();
                public Vector3 rotation = new Vector3();
                public Vector3 constrainPosition1 = new Vector3();
                public Vector3 constrainPosition2 = new Vector3();
                public Vector3 constrainRotation1 = new Vector3();
                public Vector3 constrainRotation2 = new Vector3();
                public Vector3 springPositoin = new Vector3();
                public Vector3 springRotation = new Vector3();

                public override void Read(System.IO.BinaryReader r, float scale)
                {
                    name = ReadString(r, 20);
                    UnityEngine.Debug.Log(name);
                    rigidbodyA = ReadUInt(r);
                    rigidbodyB = ReadUInt(r);
                    position.Read(r, scale);
                    rotation.Read(r);
                    constrainPosition1.Read(r, scale);
                    constrainPosition2.Read(r, scale);
                    constrainRotation1.Read(r);
                    constrainRotation2.Read(r);
                    springPositoin.Read(r, scale);
                    springRotation.Read(r);
                }
            }
        }
    }
}