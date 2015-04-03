using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using MMD.Format.PMD;

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
}