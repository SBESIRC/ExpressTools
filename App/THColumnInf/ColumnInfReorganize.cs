using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

namespace THColumnInfo
{
    public class ColumnInfReorganize
    {
        private Polyline2d _polyline = null; //传入的柱子
        private SearchFields _searchFields = new SearchFields(); //需要查找物体的层名

        private List<Polyline2d> gujinPolylines = new List<Polyline2d>(); //柱子内的箍筋多段线
        private Dictionary<Line, Point3d> markLeaderLines = new Dictionary<Line, Point3d>(); //柱子旁引线标注的线

        private Point3d minPt; //柱子左下角点
        private Point3d maxPt; //柱子右上角点

        /// <summary>
        /// 柱子信息收集是否成功
        /// </summary>
        public bool CollectIsSuccess { get; set; }

        /// <summary>
        /// 柱子搜索的信息
        /// </summary>
        public ColumnInf ColumnInfs { get; set; }

        public ColumnInfReorganize(Polyline2d polyline, SearchFields searchFields)
        {
            this._polyline = polyline;
            this._searchFields = searchFields;
            this.ColumnInfs = new ColumnInf();
            this.ColumnInfs.CurrentHandle = polyline.Handle.ToString();
        }
        public void Collect()
        {
            List<Point2d> pts = GetPoint2Ds(_polyline);
            if (pts.Count > 1)
            {
                double minX = pts.OrderBy(i => i.X).First().X;
                double minY = pts.OrderBy(i => i.Y).First().Y;
                double maxX = pts.OrderByDescending(i => i.X).First().X;
                double maxY = pts.OrderByDescending(i => i.Y).First().Y;
                minPt = new Point3d(minX, minY, 0);
                maxPt = new Point3d(maxX, maxY, 0);
            }
            this.gujinPolylines = GetColumnInsidePolylines();
            this.markLeaderLines = GetLeader();
            GetColumnJiZhongMarkInfs();
            GetColumnYuanWeiMarkInfs();
            //如果没有柱原位标注，则表示所有
            if (string.IsNullOrEmpty(this.ColumnInfs.XIronSpec) && string.IsNullOrEmpty(this.ColumnInfs.YIronSpec))
            {
                this.ColumnInfs.IronSpec = this.ColumnInfs.CornerIronSpec;
                this.ColumnInfs.CornerIronSpec = "";
            }
            GetGuJinXYDirBarNum();
        }
        /// <summary>
        /// 获取柱子内部箍筋X,Y方向长边长度
        /// </summary>
        private void GetGuJinXYDirBarNum()
        {
            if (this.gujinPolylines.Count == 0)
            {
                return;
            }
            List<double> xDirList = new List<double>();
            List<double> yDirList = new List<double>();
            for (int i = 0; i < this.gujinPolylines.Count; i++)
            {
                Polyline2d polyline = this.gujinPolylines[i];
                foreach(var item in polyline)
                {
                    //ThColumnInfDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, item);
                }
                Point3dCollection pts = new Point3dCollection();
                polyline.GetStretchPoints(pts);
                List<Point2d> polyline2ds = new List<Point2d>(); 
                foreach(Point3d pt in pts)
                {
                    polyline2ds.Add(new Point2d(pt.X,pt.Y));
                }
                for (int j = 0; j < polyline2ds.Count - 1; j++)
                {
                    Point2d startPt = polyline2ds[j];
                    Point2d endPt = polyline2ds[j+1];
                    Vector2d vec = startPt.GetVectorTo(endPt);
                    if (vec.IsCodirectionalTo(Vector2d.XAxis) || vec.Negate().IsCodirectionalTo(Vector2d.XAxis))
                    {
                        xDirList.Add(startPt.GetDistanceTo(endPt));
                    }
                    else if (vec.IsCodirectionalTo(Vector2d.YAxis) || vec.Negate().IsCodirectionalTo(Vector2d.YAxis))
                    {
                        yDirList.Add(startPt.GetDistanceTo(endPt));
                    }
                }
            }
            xDirList = xDirList.OrderByDescending(i => i).ToList();
            yDirList = yDirList.OrderByDescending(i => i).ToList();

            double length = xDirList[0];
            xDirList = xDirList.Where(i => Math.Abs(i - length)<=5.0).Select(i => i).ToList();
            length = yDirList[0];
            yDirList = yDirList.Where(i => Math.Abs(i - length)<=5.0).Select(i => i).ToList();
            this.ColumnInfs.XIronNum = xDirList.Count; //XIronNum
            this.ColumnInfs.YIronNum = yDirList.Count; //YIronNum
        }
        /// <summary>
        /// 获取柱原位标注信息
        /// </summary>
        private void GetColumnYuanWeiMarkInfs()
        {
            TypedValue[] tvs = new TypedValue[]
              {
                  new TypedValue((int)DxfCode.LayerName,this._searchFields.ZhuYuanWeiMarkLayerName),
                  new TypedValue((int)DxfCode.Start,"TEXT")
               };

            SelectionFilter sf = new SelectionFilter(tvs);
            Point3d pt1 = new Point3d(minPt.X - this._searchFields.ZhuYuanWeiMarkTextSize * 1.5, minPt.Y - this._searchFields.ZhuYuanWeiMarkTextSize * 1.5, minPt.Z);
            Point3d pt2 = new Point3d(maxPt.X + this._searchFields.ZhuYuanWeiMarkTextSize * 1.5, maxPt.Y + this._searchFields.ZhuYuanWeiMarkTextSize * 1.5, maxPt.Z);

            PromptSelectionResult psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingWindow(pt1, pt2, sf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                List<DBText> dbTexts = objIds.Select(i => ThColumnInfDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, i) as DBText).ToList();
                if (dbTexts.Count == 2)
                {
                    DBText firstText = dbTexts[0];
                    DBText secondText = dbTexts[1];
                    double angle = BaseFunction.RadToAng(firstText.Rotation);
                    angle = angle % 180.0;
                    if (Math.Abs(angle - 0.0) < 1.0 || Math.Abs(angle - 180.0) < 1.0)
                    {
                        this.ColumnInfs.XIronSpec = BaseFunction.TransferSpecialChar(firstText.TextString);
                        this.ColumnInfs.YIronSpec = BaseFunction.TransferSpecialChar(secondText.TextString);
                    }
                    else
                    {
                        this.ColumnInfs.XIronSpec = BaseFunction.TransferSpecialChar(secondText.TextString);
                        this.ColumnInfs.YIronSpec = BaseFunction.TransferSpecialChar(firstText.TextString);
                    }
                }
                else if (dbTexts.Count == 1)
                {
                    double angle = BaseFunction.RadToAng(dbTexts[0].Rotation);
                    if (Math.Abs(angle - 0.0) < 1.0 || Math.Abs(angle - 180.0) < 1.0)
                    {
                        this.ColumnInfs.XIronSpec = BaseFunction.TransferSpecialChar(dbTexts[0].TextString);
                    }
                    else if (Math.Abs(angle - 90.0) < 1.0 || Math.Abs(angle - 270) < 1.0)
                    {
                        this.ColumnInfs.YIronSpec = BaseFunction.TransferSpecialChar(dbTexts[0].TextString);
                    }
                }
            }
        }
        /// <summary>
        /// 获取柱集中标注信息
        /// </summary>
        private void GetColumnJiZhongMarkInfs()
        {
            List<DBText> dBTexts = new List<DBText>();
            foreach (var item in this.markLeaderLines)
            {
                int i = 0, x = 0;
                while (i < 2)
                {
                    if(i == 0)
                    {
                        x = 1;
                    }
                    else
                    {
                        x = -1;
                    }
                    dBTexts = GetTexts(item.Value, this._searchFields.ZhuJiZhongMarkTextSize, item.Key.Length * x);
                    if (dBTexts.Count > 0)
                    {
                        if (dBTexts.Count > 1)
                        {
                            dBTexts = dBTexts.OrderBy(j => j.Position.Y).ToList();
                            if (dBTexts.Count == 4)
                            {
                                this.ColumnInfs.Code = dBTexts[dBTexts.Count - 1].TextString; //柱子编号
                                this.ColumnInfs.Spec = dBTexts[dBTexts.Count - 2].TextString;  //柱子规格
                                this.ColumnInfs.CornerIronSpec =BaseFunction.TransferSpecialChar(dBTexts[dBTexts.Count - 3].TextString); //四角钢筋规格
                                this.ColumnInfs.NeiborGuJinHeightSpec = BaseFunction.TransferSpecialChar(dBTexts[dBTexts.Count - 4].TextString);
                            }
                        }
                        else if (dBTexts.Count == 1)
                        {
                            this.ColumnInfs.Code = dBTexts[0].TextString;
                        }
                        break;
                    }
                    i++;
                }
                if (dBTexts.Count > 0)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// 获取一点旁边的文字
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="textSize"></param>
        /// <param name="lineLength"></param>
        /// <param name="textNum"></param>
        /// <returns></returns>
        private List<DBText> GetTexts(Point3d pt, double textSize, double lineLength,int textNum=6)
        {
            List<DBText> findTexts = new List<DBText>();            
            Point3d ptUp = pt, ptDown = pt;
            Point3d minPt = Point3d.Origin,maxPt = Point3d.Origin;
            List <ObjectId> findDbTextIds = new List<ObjectId>();
            TypedValue[] tvs = new TypedValue[]
                   {
                    new TypedValue((int)DxfCode.LayerName,_searchFields.ZhuJiZhongMarkLayerName),
                    new TypedValue((int)DxfCode.Start,"TEXT")
                   };
            SelectionFilter sf = new SelectionFilter(tvs);
            ptUp = new Point3d(pt.X + lineLength, pt.Y + textSize * textNum, pt.Z);
            ptDown = new Point3d(pt.X + lineLength, pt.Y - textSize * textNum, pt.Z);
            minPt = new Point3d(Math.Min(pt.X, ptUp.X), Math.Min(pt.Y, ptUp.Y), pt.Z);
            maxPt = new Point3d(Math.Max(pt.X, ptUp.X), Math.Max(pt.Y, ptUp.Y), pt.Z);

            PromptSelectionResult psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingWindow(minPt, maxPt, sf);
            if (psr.Status == PromptStatus.OK)
            {
                findDbTextIds.AddRange(psr.Value.GetObjectIds().ToList());
            }
            minPt = new Point3d(Math.Min(pt.X, ptDown.X), Math.Min(pt.Y, ptDown.Y), pt.Z);
            maxPt = new Point3d(Math.Max(pt.X, ptDown.X), Math.Max(pt.Y, ptDown.Y), pt.Z);
            psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingWindow(minPt, maxPt, sf);
            if (psr.Status == PromptStatus.OK)
            {
                findDbTextIds.AddRange(psr.Value.GetObjectIds().ToList());
            }

            List<DBText> dBTexts = findDbTextIds.Select(j => ThColumnInfDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
            List<DBText> findCodeRes = dBTexts.Where(j => j.TextString.Contains("KZ")).Select(j => j).ToList();
            if(findCodeRes.Count==0)
            {
                return findTexts;
            }
            DBText codeText = findCodeRes.OrderBy(j => Math.Abs(j.Position.X - pt.X)).First();
            double textLength = codeText.GeometricExtents.MaxPoint.X - codeText.GeometricExtents.MinPoint.X;

            minPt = new Point3d(codeText.GeometricExtents.MinPoint.X, codeText.GeometricExtents.MaxPoint.Y- textNum * textSize,pt.Z);
            maxPt = new Point3d(codeText.GeometricExtents.MinPoint.X+textLength, codeText.GeometricExtents.MaxPoint.Y, pt.Z);
            PromptSelectionResult psr1 = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingWindow(minPt, maxPt, sf);
            if (psr1.Status == PromptStatus.OK)
            {
                List<ObjectId> newTextIds= psr1.Value.GetObjectIds().ToList();
                List<DBText> newDbTexts = newTextIds.Select(j => ThColumnInfDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
                findTexts = newDbTexts.Where(j => Math.Abs(j.Position.X - codeText.Position.X) <= 5.0).Select(j => j).ToList();
            }
            return findTexts;
        }
        /// <summary>
        /// 获取柱子内的箍筋数量
        /// </summary>
        /// <returns></returns>
        private List<Polyline2d> GetColumnInsidePolylines()
        {
            List<Polyline2d> polylines = new List<Polyline2d>();
            TypedValue[] tvs = new TypedValue[]
              {
                  new TypedValue((int)DxfCode.LayerName,this._searchFields.ZhuGuJingLayerName),
                  new TypedValue((int)DxfCode.Start,"POLYLINE")
               };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingWindow(minPt, maxPt, sf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                polylines = objIds.Select(i =>  ThColumnInfDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database,i) as Polyline2d).ToList();
            }
            return polylines;
        }
        /// <summary>
        /// 获取多段线的点集合
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        private List<Point2d> GetPoint2Ds(Polyline2d polyline)
        {
            List<Point2d> point2ds = new List<Point2d>();
            Point3dCollection pts = new Point3dCollection();
            polyline.GetStretchPoints(pts);
            foreach(Point3d pt in pts)
            {
                point2ds.Add(new Point2d(pt.X, pt.Y));
            }
            return point2ds;
        }
        /// <summary>
        /// 获取标注引线
        /// </summary>
        /// <returns></returns>
        private Dictionary<Line, Point3d> GetLeader()
        {
            Dictionary<Line, Point3d> linePtDic = new Dictionary<Line, Point3d>();
            double length = new Point3d(minPt.X, minPt.Y, 0).DistanceTo(new Point3d(maxPt.X, maxPt.Y, 0));
            Point3d pt1 = new Point3d(minPt.X - 0.1 * length, minPt.Y - 0.1 * length, 0.0);
            Point3d pt2 = new Point3d(maxPt.X + 0.1 * length, maxPt.Y + 0.1 * length, 0.0);

            TypedValue[] tvs = new TypedValue[]
              {
                  new TypedValue((int)DxfCode.LayerName,this._searchFields.ZhuMarkLeaderLayerName),
                  new TypedValue((int)DxfCode.Start,"LINE")
               };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingWindow(pt1, pt2, sf);
            List<Line> lines = new List<Line>();
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                lines = objIds.Select(i => ThColumnInfDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database,i) as Line).ToList();
            }
            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];
                Point3dCollection pts = new Point3dCollection();
                line.IntersectWith(this._polyline, Intersect.ExtendThis, pts, IntPtr.Zero, IntPtr.Zero);

                Point3d midPt = BaseFunction.GetMidPt(line.StartPoint,line.EndPoint);
                Point3d interspt=Point3d.Origin;
                if (pts.Count == 1)
                {
                    interspt = pts[0];
                }
                else if (pts.Count > 1)
                {
                    double minDis = double.MaxValue;
                    foreach(Point3d pt in pts)
                    {
                        if (midPt.DistanceTo(pt)< minDis)
                        {
                            interspt = pt;
                            minDis = midPt.DistanceTo(pt);
                        }
                    }
                }
                else
                {
                    continue;
                }
                if(line.StartPoint.DistanceTo(interspt)< line.EndPoint.DistanceTo(interspt))
                {
                    linePtDic.Add(line, line.EndPoint);
                }
                else
                {
                    linePtDic.Add(line, line.StartPoint);
                }
            }
            return linePtDic;
        }
    }
}
    