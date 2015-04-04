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
                public EnglishHeader header = new EnglishHeader();
                public EnglishBone bones = new EnglishBone();
                public EnglishMorph morphs = new EnglishMorph();
                public EnglishBoneWindow boneWindows = new EnglishBoneWindow();

                public void Read(System.IO.BinaryReader r, int boneCount, int morphCount, int boneWindowCount)
                {
                    header.Read(r);
                    bones.Read(r, boneCount);
                    morphs.Read(r, morphCount - 1);
                    boneWindows.Read(r, boneWindowCount);
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
                protected List<string> elements = new List<string>();
                public string this[int i] { get { return elements[i]; } set { elements[i] = value; } }
                public int Count { get { return elements.Count; } }

                public EnglishList(int byteSize)
                {
                    this.byteSize = byteSize;
                }

                public virtual void Read(System.IO.BinaryReader r, int count)
                {
                    elements = ReadStrings(r, count, byteSize);
                }
            }

            public class EnglishBone : EnglishList
            {
                public List<string> BoneNames { get { return elements; } set { elements = value; } }

                public EnglishBone() : base(20) { }
            }

            public class EnglishMorph : EnglishList
            {
                public List<string> MorphNames { get { return elements; } set { elements = value; } }

                public EnglishMorph() : base(20) { }

                public override void Read(System.IO.BinaryReader r, int count)
                {
                    elements = ReadStrings(r, count, byteSize);
                }
            }

            public class EnglishBoneWindow : EnglishList
            {
                public List<string> BoneWindows { get { return elements; } set { elements = value; } }

                public EnglishBoneWindow() : base(50) { }
            }
        }
    }
}