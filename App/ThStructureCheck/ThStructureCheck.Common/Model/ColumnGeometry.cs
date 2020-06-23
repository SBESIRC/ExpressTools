using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common.Model
{
    public class ColumnGeometry
    {
        public Coordinate Origin { get; set; }
        public double EccX { get; set; }
        public double EccY { get; set; }
        public double Rotation { get; set; }
        public double B { get; set; }
    }
}
