using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMD.Adapter.PMD
{
    public enum BoneType
    {
        Rotate = 0,
        RotateTransform = 1,
        IK = 2,
        Unknown = 3,
        InfluencedIK = 4,
        InfluencedRotation = 5,
        ConnectToIK = 6,
        Invisible = 7,
        Twist = 8,
        RotaryMotion = 9
    }
}
