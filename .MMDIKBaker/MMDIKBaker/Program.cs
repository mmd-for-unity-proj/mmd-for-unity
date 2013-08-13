using System;
using System.Collections.Generic;
using System.Text;
using MikuMikuDance.Model.Ver1;
using MikuMikuDance.Model;
using MikuMikuDance.Motion.Motion2;
using MikuMikuDance.Motion;
using MMDIKBakerLibrary;

namespace MMDIKBaker
{
    class Program
    {
        static void Main(string[] args)
        {
            //コンソールアプリ作りたいわけじゃないのでハードコーディングしとく
            string InputPMD = "miku.pmd";
            string InputVMD = "TrueMyHeart.vmd";
            string OutputVMD = "tmh_bake.vmd";
            MMDModel1 model = (MMDModel1)ModelManager.Read(InputPMD, MikuMikuDance.Model.CoordinateType.RightHandedCoordinate);
            MMDMotion2 motion = (MMDMotion2)MotionManager.Read(InputVMD, MikuMikuDance.Motion.CoordinateType.RightHandedCoordinate);
            motion = IKBaker.bake(motion, model);
            MotionManager.Write(OutputVMD, motion);
        }
    }
}
