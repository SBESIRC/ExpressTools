using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThElectrical.Model.ThDraw
{
    public class ThCabinetDraw : ThDraw
    {
        public ThCabinetDraw(string name, ObjectId boundaryId, Point3d minPoint, Point3d maxPoint) : base(name, boundaryId, minPoint, maxPoint)
        {
        }
    }
}
