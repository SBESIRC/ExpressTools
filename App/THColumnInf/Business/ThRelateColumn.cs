using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    /// <summary>
    /// 关联柱
    /// </summary>
    public class ThRelateColumn
    {
        private List<ColumnInf> localColumnInfs = new List<ColumnInf>();
        private List<ColumnRelateInf> columnRelateInfs = new List<ColumnRelateInf>();
        public List<ColumnRelateInf> ColumnRelateInfs
        {
            get
            {
                return this.columnRelateInfs;
            }
        }
        /// <summary>
        /// 剩下的
        /// </summary>
        public List<ColumnInf> RestColumnInfs
        {
            get
            {
                return localColumnInfs;
            }
        }
        public ThRelateColumn(List<ColumnInf> localColumnInfs, List<ColumnRelateInf> columnRelateInfs)
        {
            this.localColumnInfs = localColumnInfs.Select(i=>new ColumnInf()
            {
                Code =i.Code,
                Points =i.Points,
                AntiSeismicGrade =i.AntiSeismicGrade,
                Error=i.Error}
            ).ToList();
            this.columnRelateInfs = columnRelateInfs;
        }
        public void Relate()
        {
            if(this.localColumnInfs.Count==0 || this.columnRelateInfs.Count==0)
            {
                return;
            }
            for (int i = 0; i < this.columnRelateInfs.Count; i++)
            {
                if (this.columnRelateInfs[i].InModelPts == null || this.columnRelateInfs[i].InModelPts.Count == 0)
                {
                    continue;
                }
                List<ColumnInf> columnInfs = new List<ColumnInf>();
                for (int j=0;j< this.localColumnInfs.Count;j++)
                {
                    //bool isIntersect = CheckTwoPolylineIntersect(this.columnRelateInfs[i].InModelPts, this.localColumnInfs[j].Points);
                    bool isIntersect = ThColumnInfoUtils.JudgeTwoCurveIsOverLap(this.columnRelateInfs[i].InModelPts, this.localColumnInfs[j].Points);
                    if(isIntersect)
                    {
                        columnInfs.Add(this.localColumnInfs[j]);
                        this.localColumnInfs.RemoveAt(j);
                        j = j - 1;
                    }
                }            
                this.columnRelateInfs[i].ModelColumnInfs = columnInfs;
                //将柱子绘制到图元中并隐藏起来
                ThProgressBar.MeterProgress();
            }
        }
        private Polyline CreateZeroPolyline(List<Point3d> pts)
        {
            Polyline polyline = new Polyline();
            if(pts==null || pts.Count==0)
            {
                return polyline;
            }
            for (int i = 0; i < pts.Count; i++)
            {
                polyline.AddVertexAt(i, new Autodesk.AutoCAD.Geometry.Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
            polyline.Closed = true;
            return polyline;
        }
        /// <summary>
        /// 检查两个Polyline是否相交
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="pts2"></param>
        /// <returns></returns>
        private bool CheckTwoPolylineIntersect(List<Point3d> pts1, List<Point3d> pts2)
        {
            bool isOverLap = false;
            if(pts1==null || pts2==null || pts1.Count==0 || pts2.Count==0)
            {
                return isOverLap;
            }
            Polyline polyline1 = CreateZeroPolyline(pts1);
            Polyline polyline2 = CreateZeroPolyline(pts2);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if(intersectPts==null || intersectPts.Count==0)
            {
                Point2dCollection pts1Col = new Point2dCollection();
                Point2dCollection pts2Col = new Point2dCollection();
                pts1.ForEach(i => pts1Col.Add(new Point2d(i.X, i.Y)));
                pts2.ForEach(i => pts2Col.Add(new Point2d(i.X, i.Y)));
                if (ThColumnInfoUtils.IsPointInPolyline(pts2Col, new Point2d(pts1[0].X,pts1[0].Y)) ||
                    ThColumnInfoUtils.IsPointInPolyline(pts1Col, new Point2d(pts2[0].X, pts2[0].Y)))
                {
                    isOverLap = true;
                }
            }
            else
            {
                isOverLap = true;
            }
            return isOverLap;
        }

        public List<ObjectId> PrintJtID()
        {
            List<ObjectId> textIds = new List<ObjectId>();
            List<DBText> texts = new List<DBText>();
            for(int i=0;i<this.columnRelateInfs.Count;i++)
            {
                if(this.columnRelateInfs[i].DbColumnInf.JtID==0)
                {
                    continue;
                }
                Point3d textBasePt = ThColumnInfoUtils.GetMidPt(this.columnRelateInfs[i].InModelPts[0], 
                    this.columnRelateInfs[i].InModelPts[1]);
                DBText dBText = new DBText();
                dBText.TextString = this.columnRelateInfs[i].DbColumnInf.JtID.ToString();
                dBText.Position = textBasePt;
                texts.Add(dBText);
            }
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(doc.Database.BlockTableId,OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead) as BlockTableRecord;
                btr.UpgradeOpen();
                texts.ForEach(i=>btr.AppendEntity(i));
                texts.ForEach(i => trans.AddNewlyCreatedDBObject(i, true));
                btr.DowngradeOpen();
                trans.Commit();
            }
            texts.ForEach(i => textIds.Add(i.ObjectId));
            TypedValue tv = new TypedValue((int)DxfCode.ExtendedDataAsciiString, "*");
            texts.ForEach(i => ThColumnInfoUtils.AddXData(i.ObjectId,ThColumnInfoUtils.thColumnFrameRegAppName,new List<TypedValue>() { tv }));
            ObjectId layerId = BaseFunction.CreateColumnLayer();
            textIds.ForEach(i => ThColumnInfoUtils.SetLayer(i, layerId));
            return textIds;
        }
    }
}
