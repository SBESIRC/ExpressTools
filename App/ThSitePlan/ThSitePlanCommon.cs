using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThSitePlan
{
    public class ThSitePlanCommon
    {
        public static readonly string LAYER_FRAME = "P-AI-Frame";
        public static readonly Tolerance global_tolerance = new Tolerance(10e-10, 10e-10);
    }
}
