using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThColumnInfo.Model;

namespace ThColumnInfo
{
    public class ExtractBase
    {
        protected Point3d tableLeftDownCornerPt;
        protected Point3d tableRightUpCornerPt;
        protected double selectRangeLength = 0.0;
        protected double selectRangeHeight = 0.0;
        protected Document doc;
        protected List<ColumnTableRecordInfo> coluTabRecordInfs = new List<ColumnTableRecordInfo>();
        public ExtractBase(Point3d tableLeftDownCornerPt, Point3d tableRightUpCornerPt)
        {
            this.tableLeftDownCornerPt = tableLeftDownCornerPt;
            this.tableRightUpCornerPt = tableRightUpCornerPt;
            doc = ThColumnInfoUtils.GetMdiActiveDocument();
            ResetTableCornerPt();
            this.selectRangeLength = this.tableRightUpCornerPt.X - this.tableLeftDownCornerPt.X;
            this.selectRangeHeight = this.tableRightUpCornerPt.Y - this.tableLeftDownCornerPt.Y;
        }
        /// <summary>
        /// 提取的柱子信息
        /// </summary>
        public List<ColumnTableRecordInfo> ColuTabRecordInfs
        {
            get
            {
                return coluTabRecordInfs;
            }
        }
        /// <summary>
        /// 提取信息
        /// </summary>
        public virtual void Extract()
        {
        }
        private void ResetTableCornerPt()
        {
            double minX = Math.Min(this.tableLeftDownCornerPt.X, this.tableRightUpCornerPt.X);
            double minY = Math.Min(this.tableLeftDownCornerPt.Y, this.tableRightUpCornerPt.Y);
            double minZ = Math.Min(this.tableLeftDownCornerPt.Z, this.tableRightUpCornerPt.Z);
            double maxX = Math.Max(this.tableLeftDownCornerPt.X, this.tableRightUpCornerPt.X);
            double maxY = Math.Max(this.tableLeftDownCornerPt.Y, this.tableRightUpCornerPt.Y);
            double maxZ = Math.Max(this.tableLeftDownCornerPt.Z, this.tableRightUpCornerPt.Z);
            this.tableLeftDownCornerPt = new Point3d(minX, minY, minZ);
            this.tableRightUpCornerPt = new Point3d(maxX, maxY, maxZ);
        }
    }
}
