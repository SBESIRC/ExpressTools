using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Service
{
    public class ExtendLineLength
    {
        private DBObjectCollection extendObjs = new DBObjectCollection();
        private double disRatio = 0.01;
        public ExtendLineLength(DBObjectCollection dbObjs)
        {
            this.extendObjs = dbObjs;
        }
        public DBObjectCollection Extend()
        {
            DBObjectCollection resCol = new DBObjectCollection();
            foreach (DBObject dbObj in extendObjs)
            {
                if(dbObj is Line line && line.Length>0.0)
                {
                    Point3d lineSp = line.StartPoint;
                    Point3d lineEp = line.EndPoint;
                    lineSp = ThColumnInfoUtils.GetExtendPt(lineSp, lineEp, -disRatio * line.Length);
                    lineEp = ThColumnInfoUtils.GetExtendPt(lineEp, lineSp, -disRatio * line.Length);
                    resCol.Add(new Line(lineSp, lineEp));
                }
                else
                {
                    resCol.Add(dbObj);
                }
            }
            return resCol;
        }
    }
}
