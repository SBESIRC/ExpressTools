using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{   
    public class ColumnInf
    {
        /// <summary>
        /// 柱子编号
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 柱子对角点（左下角和右上角）
        /// </summary>
        public List<Point3d> Points { get; set; } = new List<Point3d>();
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade { get; set; } = "";

        public ErrorMsg Error { get; set; }
        public void Locate()
        {
            if(this.Points.Count==0)
            {
                return;
            }
            double minX = this.Points.OrderBy(i => i.X).First().X;
            double minY = this.Points.OrderBy(i => i.Y).First().Y;
            double maxX = this.Points.OrderByDescending(i => i.X).First().X;
            double maxY = this.Points.OrderByDescending(i => i.Y).First().Y;
            Point3d leftPt = new Point3d(minX, minY, 0);
            Point3d rightPt = new Point3d(maxX, maxY, 0);
            ThColumnInfoUtils.ZoomWin(ThColumnInfoUtils.GetMdiActiveDocument().Editor, leftPt, rightPt);
        }
        /// <summary>
        /// 关联到外框Id
        /// </summary>
        public ObjectId FrameId { get; set; } = ObjectId.Null;
        public string Text { get; set; } = "";
        /// <summary>
        /// 是否是原位标注
        /// </summary>
        public bool HasOrigin { get; set; } = false;
    }
    class ColumnInfCompare : IComparer<ColumnInf>
    {
        public int Compare(ColumnInf x, ColumnInf y)
        {
            int compareIndex= x.Code.CompareTo(y.Code);
            if(compareIndex == 0)
            {
                if(!string.IsNullOrEmpty(x.Text) && !string.IsNullOrEmpty(y.Text))
                {
                    compareIndex = x.Text.CompareTo(y.Text);
                }
            }
            return compareIndex;
        }        
    }
    class ColumnRelateInfCompare : IComparer<ColumnRelateInf>
    {
        public int Compare(ColumnRelateInf x, ColumnRelateInf y)
        {
            if(x.ModelColumnInfs.Count==0 || y.ModelColumnInfs.Count == 0)
            {
                return 0;
            }
            int copareIndex = x.ModelColumnInfs[0].Code.CompareTo(y.ModelColumnInfs[0].Code);
            if (copareIndex == 0)
            {
                if (!string.IsNullOrEmpty(x.ModelColumnInfs[0].Text) && !string.IsNullOrEmpty(y.ModelColumnInfs[0].Text))
                {
                    copareIndex = x.ModelColumnInfs[0].Text.CompareTo(y.ModelColumnInfs[0].Text);
                }
            }
            //小于0 x.Code 在y.Code前； 等于0  x.Code ，y.Code位置相同 ；大于0 x.Code 在y.Code后
            return copareIndex;
        }
    }
    class ColumnTableRecordInfoCompare : IComparer<ColumnTableRecordInfo>
    {
        public int Compare(ColumnTableRecordInfo x, ColumnTableRecordInfo y)
        {
            int copareIndex = x.Code.CompareTo(y.Code);
            return copareIndex;
        }
    }
    class ColumnCordCompare: IComparer<ColumnInf>
    {
        public int Compare(ColumnInf x, ColumnInf y)
        {
            Point3d firstPt = ThColumnInfoUtils.GetMidPt(x.Points[0], x.Points[2]);
            Point3d secondPt = ThColumnInfoUtils.GetMidPt(y.Points[0], y.Points[2]);
            firstPt = ThColumnInfoUtils.TransPtFromWcsToUcs(firstPt);
            secondPt = ThColumnInfoUtils.TransPtFromWcsToUcs(secondPt);
            if(firstPt.Y> secondPt.Y)
            {
                return -1;
            }
            else if(firstPt.Y < secondPt.Y)
            {
                return 1;
            }
            else 
            {
                if(firstPt.X< secondPt.X)
                {
                    return -1;
                }
                else if(firstPt.X > secondPt.X)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
    class ColumnPolylineCompare : IComparer<ObjectId>
    {
        public int Compare(ObjectId x, ObjectId y)
        {
            List<Point3d> xPts = ThColumnInfoUtils.GetPolylinePts(x);
            List<Point3d> yPts = ThColumnInfoUtils.GetPolylinePts(y);

            Point3d firstCenPt = ThColumnInfoUtils.GetMidPt(xPts[0], xPts[2]);
            Point3d secondCenPt = ThColumnInfoUtils.GetMidPt(yPts[0], yPts[2]);

            firstCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(firstCenPt);
            secondCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(secondCenPt);
            if (firstCenPt.Y > secondCenPt.Y)
            {
                return -1;
            }
            else if (firstCenPt.Y < secondCenPt.Y)
            {
                return 1;
            }
            else
            {
                if (firstCenPt.X < secondCenPt.X)
                {
                    return -1;
                }
                else if (firstCenPt.X > secondCenPt.X)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
