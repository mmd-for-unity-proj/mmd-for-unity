using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MMD
{
    namespace Format
    {
        public abstract class Chunk
        {
            public virtual void Read(BinaryReader r) { }
            public virtual void Write(BinaryWriter w) { }
        }
    }
}