using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Engine
{
    public class MMDLimitter : MonoBehaviour
    {
        public Vector3 MinLimitMotion;
        public Vector3 MaxLimitMotion;
        public Vector3 MinLimitAngular;
        public Vector3 MaxLimitAngular;

        Transform t;

        void Start()
        {
            t = transform;
        }

        void Check(ref float target, float min, float max)
        {
            if (target < min) target = min;
            else if (target > max) target = max;
        }

        void LateUpdate()
        {
            var localPos = t.localPosition;
            var localRot = t.localRotation;

            Check(ref localPos.x, MinLimitMotion.x, MaxLimitMotion.x);
            Check(ref localPos.y, MinLimitMotion.y, MaxLimitMotion.y);
            Check(ref localPos.z, MinLimitMotion.z, MaxLimitMotion.z);

            Check(ref localRot.x, MinLimitAngular.x, MaxLimitAngular.x);
            Check(ref localRot.y, MinLimitAngular.y, MaxLimitAngular.y);
            Check(ref localRot.z, MinLimitAngular.z, MaxLimitAngular.z);

            t.localPosition = localPos;
            t.localRotation = localRot;
        }
    }
}
