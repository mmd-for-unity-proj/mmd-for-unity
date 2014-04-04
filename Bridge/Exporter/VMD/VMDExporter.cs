using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
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
        byte[] motion = BitConverter.GetBytes((uint)0);
        byte[] morph = format.skin_list.ToBytes();
        byte[] camera = BitConverter.GetBytes((uint)0);
        byte[] light = BitConverter.GetBytes((uint)0);
        byte[] shadow = BitConverter.GetBytes((uint)0);
        
        return MakeReturnArray(header, motion, morph, camera, light, shadow);
    }

    byte[] MakeReturnArray(byte[] header, byte[] motion, byte[] morph, byte[] camera, byte[] light, byte[] shadow)
    {
        List<byte> retarr = new List<byte>();
        retarr.AddRange(header);
        retarr.AddRange(motion);
        retarr.AddRange(morph);
        retarr.AddRange(camera);
        retarr.AddRange(light);
        retarr.AddRange(shadow);
        return retarr.ToArray();
    }
}
