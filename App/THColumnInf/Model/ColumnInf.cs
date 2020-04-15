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
            List<string> xCodeStrs = BaseFunction.SplitCode(x.Code);
            List<string> yCodeStrs = BaseFunction.SplitCode(y.Code);
            int copareIndex = 0;

            if(xCodeStrs.Count==2 && yCodeStrs.Count==2)
            {
                copareIndex = xCodeStrs[0].CompareTo(yCodeStrs[0]);
                if (copareIndex==0)
                {
                    if(Convert.ToDouble(xCodeStrs[1])< Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = -1;
                    }
                    else if(Convert.ToDouble(xCodeStrs[1]) > Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = 1;
                    }
                }
            }
            else if(xCodeStrs.Count == 2)
            {
                copareIndex = -1;
            }
            else if(yCodeStrs.Count == 2)
            {
                copareIndex = 1;
            }
            else
            {
                copareIndex = x.Code.CompareTo(y.Code);
            }
            //小于0 x.Code 在y.Code前； 等于0  x.Code ，y.Code位置相同 ；大于0 x.Code 在y.Code后
            return copareIndex;
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
            string firstCode = x.ModelColumnInfs[0].Code;
            string secondCode = y.ModelColumnInfs[0].Code;
            List<string> xCodeStrs = BaseFunction.SplitCode(firstCode);
            List<string> yCodeStrs = BaseFunction.SplitCode(secondCode);
            int copareIndex = 0;
            if (xCodeStrs.Count == 2 && yCodeStrs.Count == 2)
            {
                copareIndex = xCodeStrs[0].CompareTo(yCodeStrs[0]);
                if (copareIndex == 0)
                {
                    if (Convert.ToDouble(xCodeStrs[1]) < Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = -1;
                    }
                    else if (Convert.ToDouble(xCodeStrs[1]) > Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = 1;
                    }
                }
            }
            else if (xCodeStrs.Count == 2)
            {
                copareIndex = -1;
            }
            else if (yCodeStrs.Count == 2)
            {
                copareIndex = 1;
            }
            else
            {
                copareIndex = firstCode.CompareTo(secondCode);
            }
            //小于0 x.Code 在y.Code前； 等于0  x.Code ，y.Code位置相同 ；大于0 x.Code 在y.Code后
            return copareIndex;
        }
    }
    class ColumnTableRecordInfoCompare : IComparer<ColumnTableRecordInfo>
    {
        public int Compare(ColumnTableRecordInfo x, ColumnTableRecordInfo y)
        {
            List<string> xCodeStrs = BaseFunction.SplitCode(x.Code);
            List<string> yCodeStrs = BaseFunction.SplitCode(y.Code);
            int copareIndex = 0;

            if (xCodeStrs.Count == 2 && yCodeStrs.Count == 2)
            {
                copareIndex = xCodeStrs[0].CompareTo(yCodeStrs[0]);
                if (copareIndex == 0)
                {
                    if (Convert.ToDouble(xCodeStrs[1]) < Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = -1;
                    }
                    else if (Convert.ToDouble(xCodeStrs[1]) > Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = 1;
                    }
                }
            }
            else if (xCodeStrs.Count == 2)
            {
                copareIndex = -1;
            }
            else if (yCodeStrs.Count == 2)
            {
                copareIndex = 1;
            }
            else
            {
                copareIndex = x.Code.CompareTo(y.Code);
            }
            //小于0 x.Code 在y.Code前； 等于0  x.Code ，y.Code位置相同 ；大于0 x.Code 在y.Code后
            return copareIndex;
        }
    }
}
