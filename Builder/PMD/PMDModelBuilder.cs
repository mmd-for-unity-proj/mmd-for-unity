using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MMD.Adapter.PMD;
using UnityEngine;

namespace MMD.Builder.PMD
{
    public class PMDModelBuilder
    {
        Mesh mesh = new Mesh();

        void Read(MMD.Format.PMDFormat format)
        {
            mesh.vertices = PMDModelAdapter.Vertices(format.Vertices);
            mesh.uv = PMDModelAdapter.UVs(format.Vertices);
            mesh.normals = PMDModelAdapter.Nolmals(format.Vertices);
            mesh.SetTriangles(PMDModelAdapter.Triangles(format.Faces), 0);
        }
    }
}
