using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Engine
{
    public class MMDPhysics : MonoBehaviour
    {
        public int groupIndex;
        public Collider[] ignoreColliders;

        void Start()
        {
            var myCollider = GetComponent<Collider>();

            for (int i = 0; i < ignoreColliders.Length; ++i)
            {
                Physics.IgnoreCollision(myCollider, ignoreColliders[i]);
            }
        }
    }
}
