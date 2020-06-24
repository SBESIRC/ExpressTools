using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common.Model
{
    public class Coordinate
    {
        private double x;
        private double y;
        private double z;

        public Point3d Coord
        {
            get
            {
                return new Point3d(this.x,this.y,this.z);
            }
        }
        public Coordinate(double x,double y,double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Coordinate(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public Coordinate()
        {
        }
    }
}
