using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class EnglishHeader : Chunk
            {
                public byte nameCompability;
                public string modelName;
                public string comment;

                public override void Read(System.IO.BinaryReader r)
                {
                    nameCompability = ReadByte(r);
                    modelName = ReadString(r, 20);
                    comment = ReadString(r, 256);
                }
            }

            public class EnglishBone : Chunk
            {
                public List<string> boneNames;

                public void Read(System.IO.BinaryReader r, int boneCount)
                {
                    ReadStrings(r, boneNames, boneCount, 20);
                }
            }

            public class EnglishMorph : Chunk
            {
                public List<string> morphNames;

                public void Read(System.IO.BinaryReader r, int morphCount)
                {
                    ReadStrings(r, morphNames, morphCount, 20);
                }
            }

            public class EnglishBoneWindow : Chunk
            {
                public List<string> boneWindows;

                public void Read(System.IO.BinaryReader r, int boneCount)
                {
                    ReadStrings(r, boneWindows, boneCount, 50);
                }
            }
        }
    }
}