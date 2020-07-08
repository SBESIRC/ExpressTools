using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Service;

namespace ThStructureCheck.Common.Model
{
    public abstract class WallGeometry
    {
        public Coordinate StartPoint { get; set; }
        public Coordinate EndPoint { get; set; }
        public double Ecc { get; set; }
        public double B { get; set; }
        public double H { get; set; }
    }
}
