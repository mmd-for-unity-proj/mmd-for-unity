using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Text;

// Reference URL:
//	  http://blog.goo.ne.jp/torisu_tetosuki/e/209ad341d3ece2b1b4df24abf619d6e4
//	  http://mikudan.blog120.fc2.com/blog-entry-280.html

namespace MMD
{
    public interface IBinary
    {
        byte[] ToBytes();
    }

    public interface IInsertableList
    {
        void Insert(IBinary insert_object);
    }
}