using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ThBlockReferenceExtensions
    {
        public static void DecomposeBlockTransform(this Matrix3d blockTransform, 
            out Point3d insertPt,
            out double rotation,
            out Scale3d scale)
        {
            double[] dims = blockTransform.ToArray();
            double[] line1 = new[] { dims[0], dims[1], dims[2], dims[3] };
            double[] line2 = new[] { dims[4], dims[5], dims[6], dims[7] };
            double[] line3 = new[] { dims[8], dims[9], dims[10], dims[11] };
            double[] line4 = new[] { dims[12], dims[13], dims[14], dims[15] };

            insertPt = new Point3d(line1[3], line2[3], line3[3]);
            rotation = Math.Atan(line2[0] / line1[0]);
            scale = new Scale3d(
                line1[0] / Math.Cos(rotation),
                line2[1] / Math.Cos(rotation),
                line3[2]);
        }
    }
}