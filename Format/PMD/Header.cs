using UnityEngine;
using System.Collections;

namespace MMD
{
    namespace Format
    {
        namespace PMD
        {
            public class Header
            {
                public string Magic { get; set; }
                public float Version { get; set; }
                public string ModelName { get; set; }
                public string Comment { get; set; }
            }
        }
    }
}