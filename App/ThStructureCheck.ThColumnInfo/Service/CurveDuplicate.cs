using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Service
{
    public class CurveDuplicate
    {
        private Curve firstCurve;
        private Curve secondCurve;
        private bool isDuplicate = false;
        public bool IsDuplicate => isDuplicate;
        private Point3d firstSp;
        private Point3d firstEp;
        private Point3d secondSp;
        private Point3d secondEp;
        private double tolerance = 1.0;
        public CurveDuplicate(Curve firstCurve, Curve secondCurve)
        {
            this.firstCurve = firstCurve;
            this.secondCurve = secondCurve;
            firstSp = this.firstCurve.StartPoint;
            firstEp = this.firstCurve.EndPoint;
            secondSp = this.secondCurve.StartPoint;
            secondEp = this.secondCurve.EndPoint;
        }
        public void Judge()
        {
            if(Math.Abs(firstCurve.GetLength()- secondCurve.GetLength())>2* tolerance)
            {
                return;
            }
            bool portPointIsSame = false;
            if(firstSp.DistanceTo(secondSp)<= tolerance && firstEp.DistanceTo(secondEp) <= tolerance)
            {
                portPointIsSame = true;
            }
            if (firstSp.DistanceTo(secondEp) <= tolerance && firstEp.DistanceTo(secondSp) <= tolerance)
            {
                portPointIsSame = true;
            }
            if(portPointIsSame)
            {
                if (firstCurve is Line && secondCurve is Line)
                {
                    this.isDuplicate = true;
                }
                else if (firstCurve is Arc arc1 && secondCurve is Arc arc2)
                {
                    if(ThColumnInfoUtils.GetBulge(arc1) == ThColumnInfoUtils.GetBulge(arc2))
                    {
                        this.isDuplicate = true;
                    }
                }
            }
        }
    }
}
