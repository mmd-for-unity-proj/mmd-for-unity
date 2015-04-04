using System.Collections;
using System.Collections.Generic;
using MMD.Format.PMD;

namespace MMD
{
    namespace Format
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

            public Header Header { get { return header; } }
            public List<Vertex> Vertices { get { return vertices.Vertices; } }
            public List<Face> Faces { get { return faces.Faces; } }
            public List<Material> Materials { get { return materials.Materials; } }
            public List<Bone> Bones { get { return bones.Bones; } }
            public List<IK> IKs { get { return iks.IKs; } }
            public List<Morph> Morphs { get { return morphs.Morphs; } }
            public List<ushort> MorphDisplays { get { return morphDisplays.MorphDisplays; } }
            public List<string> BoneWindows { get { return boneWindows.BoneWindows; } }
            public List<BoneDisplay> BoneDisplays { get { return boneDisplays.BoneDisplays; } }
            public English Englishes { get { return englishes; } }
            public EnglishHeader EnglishHeader { get { return englishes.header; } }
            public List<string> EnglishMorphes { get { return englishes.morphs.MorphNames; } }
            public List<string> EnglishBones { get { return englishes.bones.BoneNames; } }
            public List<string> EnglishBoneWindows { get { return englishes.boneWindows.BoneWindows; } }
            public List<string> ToonTextures { get { return toonTextures.ToonTextures; } }
            public List<Rigidbody> Rigidbodies { get { return rigidbodies.Rigidbodies; } }
            public List<Joint> Joints { get { return joints.Joints; } }

            public PMDFormat Read(System.IO.BinaryReader r)
            {
                header.Read(r);
                vertices.Read(r);
                faces.Read(r);
                materials.Read(r);
                bones.Read(r);
                iks.Read(r);
                morphs.Read(r);
                morphDisplays.Read(r);
                boneWindows.Read(r);
                boneDisplays.Read(r);
                englishes.Read(r, bones.Bones.Count, morphs.Morphs.Count);
                toonTextures.Read(r);
                rigidbodies.Read(r);
                joints.Read(r);
                return this;
            }
        }
    }
}