using System.Collections;
using System.Collections.Generic;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class English : Chunk
            {
                public EnglishHeader header;
                public EnglishBone bones;
                public EnglishMorph morphs;
                public EnglishBoneWindow boneWindows;

                public void Read(System.IO.BinaryReader r, int boneCount, int morphCount)
                {
                    header.Read(r);
                    bones.Read(r, boneCount);
                    morphs.Read(r, morphCount - 1);
                    boneWindows.Read(r, boneCount);
                }
            }

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

            public class EnglishList : Chunk
            {
                protected int byteSize;
                protected List<string> elements;
                public string this[int i] { get { return elements[i]; } set { elements[i] = value; } }

                public EnglishList(int byteSize)
                {
                    this.byteSize = byteSize;
                }

                public void Read(System.IO.BinaryReader r, int count)
                {
                    ReadStrings(r, elements, count, byteSize);
                }
            }

            public class EnglishBone : EnglishList
            {
                public List<string> BoneNames { get { return elements; } }

                public EnglishBone() : base(20) { }
            }

            public class EnglishMorph : EnglishList
            {
                public List<string> MorphNames { get { return elements; } }

                public EnglishMorph() : base(20) { }
            }

            public class EnglishBoneWindow : EnglishList
            {
                public List<string> BoneWindows { get; private set; }

                public EnglishBoneWindow() : base(50) { }
            }
        }
    }
}