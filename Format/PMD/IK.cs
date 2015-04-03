using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class IKList : ChunkList<IK, uint> 
            {
                public List<IK> IKs { get { return elements; } }
            }

            public class IK : Chunk
            {
                public ushort boneIndex;
                public ushort targetBoneIndex;
                public byte chainLength;
                public ushort iterations;
                public float controlWeight;
                public List<ushort> childBoneIndices;

                public override void Read(System.IO.BinaryReader r)
                {
                    boneIndex = ReadUShort(r);
                    targetBoneIndex = ReadUShort(r);
                    chainLength = ReadByte(r);
                    iterations = ReadUShort(r);
                    controlWeight = ReadFloat(r);
                    childBoneIndices = new List<ushort>(chainLength);
                    for (uint i = 0; i < chainLength; ++i)
                        childBoneIndices.Add(ReadUShort(r));
                }
            }
        }
    }
}