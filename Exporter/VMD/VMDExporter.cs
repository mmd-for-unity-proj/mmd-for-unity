using UnityEngine;
using System.Collections;
using MMD.VMD;

public class VMDExporter
{
    VMDFormat format;

    public VMDExporter(VMDFormat format)
    {
        this.format = format;
    }

    public byte[] ExportMorph()
    {
        byte[] header = format.header.ToBytes();

        return null;
    }
}
