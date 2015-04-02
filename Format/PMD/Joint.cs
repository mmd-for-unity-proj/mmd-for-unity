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
                public Vector3 Rotation;
                public Vector3 constrainPosition1;
                public Vector3 constrainPosition2;
                public Vector3 constrainRotation1;
                public Vector3 constrainRotation2;
                public Vector3 springPositoin;
                public Vector3 springRotation;
            }
        }
    }
}