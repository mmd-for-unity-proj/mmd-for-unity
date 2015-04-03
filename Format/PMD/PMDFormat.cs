using System.Collections;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class PMDFormat
            {
                Header header = new Header();
                VertexList vertices = new VertexList();
                FaceList faces = new FaceList();
                MaterialList materials = new MaterialList();
                BoneList bones = new BoneList();
                IKList iks = new IKList();
                MorphList morphs = new MorphList();
                MorphDisplayList morphDisplays = new MorphDisplayList();
                BoneWindowList boneWindows = new BoneWindowList();
                BoneDisplayList boneDisplays = new BoneDisplayList();
                English englishes = new English();
                ToonTexture toonTextures = new ToonTexture();
                RigidbodyList rigidbodies = new RigidbodyList();
                JointList joints = new JointList();

                public void Read(System.IO.BinaryReader r)
                {
                    header.Read(r);
                    vertices.Read(r);
                    faces.Read(r);
                    materials.Read(r);
                    bones.Read(r);
                    iks.Read(r);
                    morphDisplays.Read(r);
                    boneWindows.Read(r);
                    boneDisplays.Read(r);
                    englishes.Read(r, bones.Bones.Count, morphs.Morphs.Count);
                    toonTextures.Read(r);
                    rigidbodies.Read(r);
                    joints.Read(r);
                }
            }
        }
    }
}