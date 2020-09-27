using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common.Service
{
    public class CloseCurveOverlap
    {
        private Curve first;
        private Curve second;
        private Extents3d firstExt;
        private Extents3d secondExt;
        private bool isOverlap = false;
        /// <summary>
        /// 是否重叠
        /// </summary>
        public bool IsOverlap => isOverlap;

        public CloseCurveOverlap(Curve first, Curve second)
        {
            this.first = first;
            this.second = second;
            firstExt = first.GeometricExtents;
            secondExt = first.GeometricExtents;
        }
        public void Check()
        {
            bool boundingIntersect = BoundingIntersect();
            if(boundingIntersect)
            {
                this.isOverlap = CadTool.JudgeTwoCurveIsOverLap(this.first, this.second);
            }
        }
        private bool BoundingIntersect()
        {
            bool res = false;
            if(firstExt.MinPoint.X>secondExt.MinPoint.X &&
                firstExt.MinPoint.X < secondExt.MaxPoint.X)
            {
                if(firstExt.MinPoint.Y> secondExt.MinPoint.Y &&
                    firstExt.MinPoint.Y < secondExt.MaxPoint.Y)
                {
                    return true;
                }
            }
            if(secondExt.MinPoint.X> firstExt.MinPoint.X &&
                secondExt.MinPoint.X< firstExt.MaxPoint.X)
            {
                if (secondExt.MinPoint.Y > firstExt.MinPoint.Y &&
                       secondExt.MinPoint.Y < firstExt.MaxPoint.Y)
                {
                    return true;
                }
            }
            return res;
        }
    }
}
