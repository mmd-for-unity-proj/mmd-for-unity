using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using MMD.Format.PMD;
using MMD.Format.Common;

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
}