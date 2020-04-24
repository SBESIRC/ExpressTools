using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Business
{
    /// <summary>
    /// 识别角筋、b边纵筋、h边纵筋的数量
    /// </summary>
    public class DistinguishCBH
    {
        /// <summary>
        /// 角筋数量
        /// </summary>
        public int CornerNum { get; set; }
        /// <summary>
        /// B边纵筋数量
        /// </summary>
        public int BEdgeNum { get; set; }
        /// <summary>
        /// H边纵筋数量
        /// </summary>
        public int HEdgeNum { get; set; }
        /// <summary>
        /// 类型代号
        /// </summary>
        public string TypeNumber { get; set; } = "";

        private Curve columnFrame; //方框

        private List<Curve> curves = new List<Curve>();  //方框里包括的所有Curves

        private List<Curve> smallCurves = new List<Curve>();  //小Curve,用于分析角筋数量,b边数量

        public DistinguishCBH(Curve columnFrame)
        {
            this.columnFrame = columnFrame;
            GetPolylines();
        }
        public void Distinguish()
        {          
            if (curves.Count == 0)
            {
                return;
            }
            //获取方框里所有小的圆圈
            GetSmallPolylines();

        }
        private void GetPolylines()
        {
            if (this.columnFrame == null)
            {
                return;
            }
            List<Point3d> boundaryPts = ThColumnInfoUtils.GetPolylinePts(this.columnFrame);
            double minX = boundaryPts.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
            double minY = boundaryPts.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double minZ = boundaryPts.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

            double maxX = boundaryPts.OrderByDescending(i => i.X).Select(i => i.X).FirstOrDefault();
            double maxY = boundaryPts.OrderByDescending(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double maxZ = boundaryPts.OrderByDescending(i => i.Z).Select(i => i.Z).FirstOrDefault();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Point3d pt1 = new Point3d(minX, minY, minZ);
            Point3d pt2 = new Point3d(maxX, maxY, minZ);
            Point3d fiterPt1 = pt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d fiterPt2 = pt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());

            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline,LWPOLYLINE") }; //后期根据需要再追加搜索条件
            SelectionFilter polylineSf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor, fiterPt1, fiterPt2, PolygonSelectionMode.Window, polylineSf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> polylineObjIds = psr.Value.GetObjectIds().ToList();
                curves = polylineObjIds.Select(i => ThColumnInfoDbUtils.GetEntity(doc.Database, i) as Curve).ToList();
            }
        }
        /// <summary>
        /// 获取所有小的
        /// </summary>
        private void GetSmallPolylines()
        {
            Dictionary<double, List<Curve>> polylineAreaDic = new Dictionary<double, List<Curve>>();
            double area = 0.0;
            for (int i = 0; i < this.curves.Count; i++)
            {
                if (this.curves[i] is Polyline polyline)
                {
                    area = polyline.Area;
                }
                else if (this.curves[i] is Polyline2d polyline2d)
                {
                    area = polyline2d.Area;
                }
                else if (this.curves[i] is Polyline3d polyline3d)
                {
                    area = polyline3d.Area;
                }
                else if(this.curves[i] is Circle circle)
                {
                    area = circle.Area;
                }
                else if(this.curves[i] is Ellipse ellipse)
                {
                    area = ellipse.Area;
                }
                else
                {
                    area = 0.0;
                }
                if (area == 0.0)
                {
                    continue;
                }
                List<double> existAreas = polylineAreaDic.Where(j => Math.Abs(j.Key - area) <= 2.0).Select(j => j.Key).ToList();
                if (existAreas == null || existAreas.Count == 0)
                {
                    polylineAreaDic.Add(area, new List<Curve>() { this.curves[i] });
                }
                else
                {
                    polylineAreaDic[existAreas[0]].Add(this.curves[i]);
                }
            }
            List<double> smallPolylineAreas = polylineAreaDic.OrderBy(i => i.Key).Select(i => i.Key).ToList();
            this.smallCurves= polylineAreaDic.OrderBy(i => i.Key).Where(i => i.Value.Count > 4).Select(i => i.Value).First();
        }
        private void AnalyzeCornerBHEdgeNumber()
        {
            if(this.smallCurves.Count==0)
            {
                return;
            }
            //Todo

        }
        private double offsetDis = 5.0;
        /// <summary>
        /// 左下角点Corner
        /// </summary>
        private void LeftDownCorner()
        {
            var res = from item in this.smallCurves
                      orderby GetBoundingBoxCenter(item).X ascending, GetBoundingBoxCenter(item).Y ascending
                      select item;
            Curve leftDownCurve = res.First();
            Point3d cenPt = GetBoundingBoxCenter(leftDownCurve);
            List<Curve> xCurves = this.smallCurves.Where(i => Math.Abs(GetBoundingBoxCenter(i).Y - cenPt.Y) <= this.offsetDis).Select(i => i).ToList();
            //List<Curve> xCurves = this.smallCurves.Where(i => Math.Abs(GetBoundingBoxCenter(i).Y - cenPt.Y) <= this.offsetDis).Select(i => i).ToList();
            //Todo
        }
        /// <summary>
        /// 右下角点Corner
        /// </summary>
        private void RightDownCorner()
        {
            var res = from item in this.smallCurves
                      orderby GetBoundingBoxCenter(item).X ascending, GetBoundingBoxCenter(item).Y ascending
                      select item;
            Curve leftDownCurve = res.First();
            //Todo
        }
        /// <summary>
        /// 右上角点Corner
        /// </summary>
        private void RightUpCorner()
        {
            var res = from item in this.smallCurves
                      orderby GetBoundingBoxCenter(item).X ascending, GetBoundingBoxCenter(item).Y ascending
                      select item;
            Curve leftDownCurve = res.First();
            //Todo
        }
        /// <summary>
        /// 左上角点Corner
        /// </summary>
        private void LeftUpCorner()
        {
            var res = from item in this.smallCurves
                      orderby GetBoundingBoxCenter(item).X ascending, GetBoundingBoxCenter(item).Y ascending
                      select item;
            Curve leftDownCurve = res.First();
            //Todo
        }
        private Point3d GetBoundingBoxCenter(Curve curve)
        {
            Point3d minPt = curve.Bounds.Value.MinPoint;
            Point3d maxPt = curve.Bounds.Value.MaxPoint;
            Point3d cenPt = ThColumnInfoUtils.GetMidPt(minPt, maxPt);
            return new Point3d(cenPt.X, cenPt.Y,0.0);
        }       
        /// <summary>
        /// 分割框内部
        /// </summary>
        /// <param name="polylineObjs"></param>
        /// <param name="bsideNum"></param>
        /// <param name="hSideNum"></param>
        private void SplitInsidePolylines(List<DBObject> polylineObjs)
        {
            List<Curve> polylines = new List<Curve>();
            for (int i = 0; i < polylineObjs.Count; i++)
            {
                if (polylineObjs[i] is Polyline || polylineObjs[i] is Polyline2d || polylineObjs[i] is Polyline3d ||
                    polylineObjs[i] is Line)
                {
                    polylines.Add(polylineObjs[i] as Curve);
                }
            }
            List<Curve> noRepeatedPolylines = new List<Curve>();
            while (polylines.Count > 0)
            {
                Curve currentLine = polylines[0];
                noRepeatedPolylines.Add(currentLine);
                polylines = polylines.Where(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).DistanceTo
                  (ThColumnInfoUtils.GetMidPt(currentLine.Bounds.Value.MinPoint, currentLine.Bounds.Value.MaxPoint)) > 5.0).Select(i => i).ToList();
            }
            polylines = noRepeatedPolylines;
            List<double> xValues = polylines.Select(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).X).ToList();
            List<double> yValues = polylines.Select(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).Y).ToList();

            double minX = xValues.OrderBy(i => i).First();
            double minY = yValues.OrderBy(i => i).First();
            List<double> hSides = xValues.Where(i => Math.Abs(i - minX) <= 10.0).Select(i => i).ToList();
            List<double> bSides = yValues.Where(i => Math.Abs(i - minY) <= 10.0).Select(i => i).ToList();
            //bsideNum = bSides.Count - 2;
            //hSideNum = hSides.Count - 2;
        }
    }
}
