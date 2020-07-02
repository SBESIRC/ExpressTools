using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.Common.Model.Column
{
    public abstract class ColumnGeometry
    {
        public Coordinate Origin { get; set; }
        public double EccX { get; set; }
        public double EccY { get; set; }
        public double Rotation { get; set; }
        public double B { get; set; }
        public double H { get; set; }        
    }
}
