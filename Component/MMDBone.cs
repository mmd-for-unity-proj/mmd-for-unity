using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MMD.Adapter.PMD;

namespace MMD.Component
{
    public class PMDBone : UnityEngine.Component
    {
        public GameObject TailBone { get; set; }
        public BoneType BoneType { get; set; }
        public GameObject InfluencedIKBone { get; set; }
    }
}
