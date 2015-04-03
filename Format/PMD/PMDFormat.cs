using System.Collections;
using System.Collections.Generic;

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

                Header Header { get { return header; } }
                List<Vertex> Vertices { get { return vertices.Vertices; } }
                List<Face> Faces { get { return faces.Faces; } }
                List<Material> Materials { get { return materials.Materials; } }
                List<Bone> Bones { get { return bones.Bones; } }
                List<IK> IKs { get { return iks.IKs; } }
                List<Morph> Morphs { get { return morphs.Morphs; } }
                List<ushort> MorphDisplays { get { return morphDisplays.MorphDisplays; } }
                List<string> BoneWindows { get { return boneWindows.BoneWindows; } }
                List<BoneDisplay> BoneDisplays { get { return boneDisplays.BoneDisplays; } }
                English Englishes { get { return englishes; } }
                EnglishHeader EnglishHeader { get { return englishes.header; } }
                List<string> EnglishMorphes { get { return englishes.morphs.MorphNames; } }
                List<string> EnglishBones { get { return englishes.bones.BoneNames; } }
                List<string> EnglishBoneWindows { get { return englishes.boneWindows.BoneWindows; } }
                List<string> ToonTextures { get { return toonTextures.ToonTextures; } }
                List<Rigidbody> Rigidbodies { get { return rigidbodies.Rigidbodies; } }
                List<Joint> Joints { get { return joints.Joints; } }

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