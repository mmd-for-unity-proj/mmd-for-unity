using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using MMD.Format.PMD;
using MMD.Format.Common;
using MMD.Format;
using UnityEngine;

[TestFixture]
public class PMDAnimasaTest
{
    PMDFormat format = new PMDFormat();

    BinaryReader ReadFile()
    {
        return new BinaryReader(File.OpenRead(@"./Assets/Models/ANIMASA/初音ミクVer2.pmd"));
    }

    [Test]
    public void OpenPMD()
    {
        Assert.AreNotEqual(ReadFile(), null);
    }

    [Test]
    public void ReadHeader()
    {
        format.Read(ReadFile());
        Assert.AreEqual(format.Header.magic, "Pmd");
        Assert.AreEqual(format.Header.version, 1.0f);
        Assert.AreEqual(format.Header.modelName, @"初音ミク");
    }

    [Test]
    public void ReadVertex()
    {
        format.Read(ReadFile());
        Assert.AreEqual(format.Vertices.Count, 12354);
        Assert.AreEqual(format.Vertices[0].boneNumber[0], 66);
        Assert.AreEqual(format.Vertices[3578].boneNumber[0], 55);
        Assert.AreEqual(format.Vertices[12353].boneNumber[0], 3);
    }

    void TestFace(Face f, int a, int b, int c)
    {
        Assert.AreEqual(f[0], a);
        Assert.AreEqual(f[1], b);
        Assert.AreEqual(f[2], c);
    }

    [Test]
    public void ReadFace()
    {
        format.Read(ReadFile());
        Assert.AreEqual(format.Faces.Count, 22961);
        TestFace(format.Faces[0], 647, 648, 649);
        TestFace(format.Faces[1000], 3109, 3110, 3107);
        TestFace(format.Faces[10000], 10547, 11231, 11230);
        TestFace(format.Faces[22960], 11296, 11287, 11286);
    }

    [Test]
    public void ReadMaterial()
    {
        format.Read(ReadFile());
        Assert.AreEqual(format.Materials.Count, 17);
        Assert.AreEqual(format.Materials[0].assignedFaceConut, 2425);
        Assert.AreEqual(format.Materials[7].assignedFaceConut, 2810);
        Assert.AreEqual(format.Materials[8].assignedFaceConut, 36);
        Assert.AreEqual(format.Materials[16].assignedFaceConut, 312);
    }

    void TestBone(Bone b, int parent, string name)
    {
        Assert.AreEqual(b.parentBoneIndex, parent);
        Assert.AreEqual(b.name, name);
    }

    [Test]
    public void ReadBone()
    {
        format.Read(ReadFile());
        var bone = format.Bones;
        Assert.AreEqual(bone.Count, 140);
        TestBone(bone[0], 65535, "センター");
        TestBone(bone[10], 9, "腰飾り");
        TestBone(bone[100], 25, "左親指先");
        TestBone(bone[139], 50, "右腕捩3");
    }

    void TestMorph(Morph m, int count, string name)
    {
        Assert.AreEqual(m.vertexCount, count);
        Assert.AreEqual(m.name, name);
    }

    [Test]
    public void ReadMorph()
    {
        var morph = format.Read(ReadFile()).Morphs;
        Assert.AreEqual(morph.Count, 31);
        TestMorph(morph[1], 78, "真面目");
        TestMorph(morph[30], 45, "にやり");
    }

    [Test]
    public void ReadDisp()
    {
        var f = format.Read(ReadFile());
        var morph = f.MorphDisplays;
        Assert.AreEqual(morph.Count, 30);

        var window = f.BoneWindows;
        Assert.AreEqual(window.Count, 7);
    }

    [Test]
    public void ReadEnglish()
    {
        var english = format.Read(ReadFile()).Englishes;
        Assert.AreEqual(english.header.modelName, "Miku Hatsune");

        Assert.AreEqual(english.bones.Count, 140);
        Assert.AreEqual(english.bones[0], "center");
        Assert.AreEqual(english.bones[6], "necktie1");
        Assert.AreEqual(english.bones[92], "toe IK_R");

        Debug.Log("enmr:" + english.morphs.Count);
        for (int i = 0; i < english.morphs.Count; ++i)
            Debug.Log(english.morphs[i]);

        Debug.Log("enbw:" + english.boneWindows.Count.ToString());
        for (int i = 0; i < english.boneWindows.Count; ++i)
            Debug.Log(english.boneWindows[i]);
    }

    [Test]
    public void ReadToon()
    {
        var toon = format.Read(ReadFile()).ToonTextures;
        for (int i = 0; i < toon.Count; ++i)
            Assert.AreEqual(toon[i].Contains("toon"), true);
    }

    [Test]
    public void ReadRigidbody()
    {
        var rigidbody = format.Read(ReadFile()).Rigidbodies;
        Assert.AreEqual(rigidbody.Count, 45);
        Assert.AreEqual(rigidbody[0].name, "頭");
        Assert.AreEqual(rigidbody[44].name, "ネクタイ3");
    }

    [Test]
    public void ReadJoint()
    {
        var joint = format.Read(ReadFile()).Joints;
        Assert.AreEqual(joint.Count, 27);
        Assert.AreEqual(joint[0].name, "右髪1");
        Assert.AreEqual(joint[26].name, "左スカート前2");
    }
}