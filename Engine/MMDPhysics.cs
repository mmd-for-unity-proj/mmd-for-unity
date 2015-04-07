using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Engine
{
    public class MMDPhysics : MonoBehaviour
    {
        public Collider[] ignoreRigidbodies;

        void Start()
        {
            var myCollider = GetComponent<Collider>();

            for (int i = 0; i < ignoreRigidbodies.Length; ++i)
            {
                Physics.IgnoreCollision(myCollider, ignoreRigidbodies[i]);
            }
        }
    }
}
