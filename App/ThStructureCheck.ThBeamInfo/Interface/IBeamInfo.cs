using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.ThBeamInfo.Interface
{
    public interface IBeamInfo
    {
        List<Point3d> Points { get; set; }
        void Locate();
    }
}
