using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    /// <summary>
    /// 识别角筋、b边纵筋、h边纵筋的数量
    /// </summary>
    public class DistinguishCBH
    {
        private int cornerNum = 0;
        private int bEdgeNum = 0;
        private int hEdgeNum = 0;
        /// <summary>
        /// 角筋数量
        /// </summary>
        public int CornerNum {
            get
            {
                return cornerNum;
            }
        }
        /// <summary>
        /// B边纵筋数量
        /// </summary>
        /// 
        public int BEdgeNum
        {
            get
            {
                return bEdgeNum;
            }
        }
        /// <summary>
        /// H边纵筋数量
        /// </summary>
        public int HEdgeNum
        {
            get
            {
                return hEdgeNum;
            }
        } 

        /// <summary>
        /// 获取总数
        /// </summary>
        /// <returns></returns>
        public int TotalNum
        {
            get
            {
                return this.cornerNum * 4 + this.bEdgeNum * 2 + this.hEdgeNum * 2;
            }
        }

        /// <summary>
        /// 类型代号
        /// </summary>
        public string TypeNumber { get; set; } = "";

        private Curve columnFrame; //方框

        private List<Curve> curves = new List<Curve>();  //方框里包括的所有曲线
        private List<Curve> restCurves = new List<Curve>(); //方框里除掉小曲线，剩下的曲线
        private List<Curve> smallCurves = new List<Curve>();  //小Curve,用于分析角筋数量,b边数量
        private Document doc;

        private double offsetDis = 10.0;
        private double searchRatio = 1.0 / 3.0;
        private Point3d leftDownPt;
        private Point3d rightUpPt;
        private Curve originCurve; //左下角Curve

        public DistinguishCBH(Curve columnFrame)
        {
            this.columnFrame = columnFrame;
            this.doc = ThColumnInfoUtils.GetMdiActiveDocument();
        }
        public void Distinguish()
        {
            //获取方框里所有符合条件的曲线
            GetPolylines();
            if (curves.Count == 0)
            {
                return;
            }
            //获取方框里所有小的圆圈
            GetSmallPolylines();
            this.restCurves=this.curves.Where(i => this.smallCurves.IndexOf(i) < 0).Select(i => i).ToList();
            //分析角筋数量
            AnalyzeCornerNumber();
            //分析b、h边纵筋数量
            AnalyzeBHEdgeNumber();
            //分析肢数
            AnalyzeTypeNumber();
        }
        private void GetFrameCornerPts()
        {
            List<Point3d> pts = ThColumnInfoUtils.GetPolylinePts(this.columnFrame);
            pts.ForEach(i => ThColumnInfoUtils.TransPtFromWcsToUcs(i));
            double minX = pts.OrderBy(i => i.X).First().X;
            double minY = pts.OrderBy(i => i.Y).First().Y;
            double maxX = pts.OrderByDescending(i => i.X).First().X;
            double maxY = pts.OrderByDescending(i => i.Y).First().Y;
            this.leftDownPt = new Point3d(minX, minY, 0.0);
            this.rightUpPt = new Point3d(maxX, maxY, 0.0);
        }
        /// <summary>
        /// 获取方框里所有符合条件的曲线
        /// </summary>
        private void GetPolylines()
        {
            if (this.columnFrame == null)
            {
                return;
            }
            List<Point3d> boundaryPts = ThColumnInfoUtils.GetPolylinePts(this.columnFrame);
            boundaryPts=boundaryPts.Select(i=>ThColumnInfoUtils.TransPtFromWcsToUcs(i)).ToList();
            double minX = boundaryPts.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
            double minY = boundaryPts.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double minZ = boundaryPts.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

            double maxX = boundaryPts.OrderByDescending(i => i.X).Select(i => i.X).FirstOrDefault();
            double maxY = boundaryPts.OrderByDescending(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double maxZ = boundaryPts.OrderByDescending(i => i.Z).Select(i => i.Z).FirstOrDefault();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Point3d pt1 = new Point3d(minX, minY, minZ);
            Point3d pt2 = new Point3d(maxX, maxY, minZ);
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline,LWPOLYLINE") }; //后期根据需要再追加搜索条件
            SelectionFilter polylineSf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor, pt1, pt2, PolygonSelectionMode.Window, polylineSf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> polylineObjIds = psr.Value.GetObjectIds().ToList();
                curves = polylineObjIds.Select(i => ThColumnInfoDbUtils.GetEntity(doc.Database, i) as Curve).ToList();
            }
        }
        /// <summary>
        /// 获取所有小的圆圈
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
                else if (this.curves[i] is Circle circle)
                {
                    area = circle.Area;
                }
                else if (this.curves[i] is Ellipse ellipse)
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
            this.smallCurves = polylineAreaDic.OrderBy(i => i.Key).Where(i => i.Value.Count > 4).Select(i => i.Value).First();
        }
        private void AnalyzeCornerNumber()
        {
            if (this.smallCurves.Count == 0)
            {
                return;
            }
            List<Curve> leftDownCorner = GetLeftDownCorner();
            List<Curve> rightDownCorner = GetRightDownCorner();
            List<Curve> rightUpCorner = GetRightUpCorner();
            List<Curve> leftUpCorner = GetLeftUpCorner();
            this.cornerNum=AnalyzeMostNumber(new List<int> { leftDownCorner.Count, rightDownCorner.Count, rightUpCorner.Count, leftUpCorner.Count });
            RemoveCorner(leftDownCorner);
            RemoveCorner(rightDownCorner);
            RemoveCorner(rightUpCorner);
            RemoveCorner(leftUpCorner);
        }
        private void AnalyzeBHEdgeNumber()
        {
            Point3d originCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(this.originCurve));

            var xDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y - originCurveCenPt.Y) <= this.offsetDis).Select(i => i).ToList();

            var yDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X - originCurveCenPt.X) <= this.offsetDis).Select(i => i).ToList();         
            foreach(Curve xCurve in xDirCurves)
            {
                Point3d xCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xCurve));
                var yLineCurves = this.smallCurves.Where(i => Math.Abs(
                ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X - xCurveCenPt.X) <= this.offsetDis).Select(i => i).ToList();
                if(yLineCurves!=null && yLineCurves.Count()==2)
                {
                    this.bEdgeNum += 1;
                }
            }
            foreach (Curve yCurve in yDirCurves)
            {
                Point3d yCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yCurve));
                var xLineCurves = this.smallCurves.Where(i => Math.Abs(
                ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y - yCurveCenPt.Y) <= this.offsetDis).Select(i => i).ToList();
                if (xLineCurves != null && xLineCurves.Count() == 2)
                {
                    this.hEdgeNum += 1;
                }
            }
        }
        private void RemoveCorner(List<Curve> corners)
        {
            for(int i=1;i<= corners.Count;i++)
            {
                if(i<= this.CornerNum)
                {
                    this.smallCurves.Remove(corners[i-1]);
                }
            }
        }
        private int AnalyzeMostNumber(List<int> numbers)
        {
            List<int> distinceValues = numbers.Distinct().ToList();
            Dictionary<int, int> numberCount = new Dictionary<int, int>();
            foreach(int value in distinceValues)
            {
                numberCount.Add(value, numbers.Where(i => i == value).Select(i => i).ToList().Count());
            }
            return numberCount.OrderByDescending(i => i.Value).First().Key;
        }
        private double GetOriginCurveLength()
        {
            double length = 0.0;
            if(this.originCurve is Polyline polyline)
            {
                length = polyline.Length;
            }
            else if(this.originCurve is Circle circle)
            {
                length = circle.Circumference;
            }
            else
            {
                length = this.originCurve.GetLength();
            }
            return length;
        }
        /// <summary>
        /// 分析箍筋类型号 1(4 x 4)
        /// </summary>
        private void AnalyzeTypeNumber()
        {
            double originCurveLength = GetOriginCurveLength();
            Dictionary<double, List<DBObject>> polylineAreaDic = new Dictionary<double, List<DBObject>>();
            List<double> xDirList = new List<double>();
            List<double> yDirList = new List<double>();
            for (int i = 0; i < this.restCurves.Count; i++)
            {
                List<Point3d> pts = ThColumnInfoUtils.GetPolylinePts(this.restCurves[i] as Curve);
                for (int j = 0; j < pts.Count - 1; j++)
                {
                    Point3d startPt =ThColumnInfoUtils.TransPtFromWcsToUcs(pts[j]);
                    Point3d endPt = ThColumnInfoUtils.TransPtFromWcsToUcs(pts[j+1]);
                    startPt = new Point3d(startPt.X, startPt.Y,0.0);
                    endPt = new Point3d(endPt.X, endPt.Y, 0.0);
                    Vector3d vec = startPt.GetVectorTo(endPt);
                    if (vec.Length <= originCurveLength)
                    {
                        continue;
                    }
                    if (vec.GetNormal().IsParallelTo(Vector3d.XAxis, ThColumnInfoUtils.tolerance))
                    {
                        xDirList.Add(startPt.DistanceTo(endPt));
                    }
                    else if (vec.GetNormal().IsParallelTo(Vector3d.YAxis, ThColumnInfoUtils.tolerance))
                    {
                        yDirList.Add(startPt.DistanceTo(endPt));
                    }
                }
            }
            Dictionary<double, int> xVecLengthDic = new Dictionary<double, int>();
            Dictionary<double, int> yVecLengthDic = new Dictionary<double, int>();
            xDirList = xDirList.OrderByDescending(i => i).ToList();
            yDirList = yDirList.OrderByDescending(i => i).ToList();
            List<double> tempXDirList = new List<double>();
            List<double> tempYDirList = new List<double>();
            if (xDirList.Count > 0)
            {
                tempXDirList = xDirList.Distinct().Take(3).ToList();
            }
            if (yDirList.Count > 0)
            {
                tempYDirList = yDirList.Distinct().Take(3).ToList();
            }
            foreach (double length in tempXDirList)
            {
                List<double> tempList = xDirList.Where(i => Math.Abs(i - length) <= 5.0).Select(i => i).ToList();
                xVecLengthDic.Add(length, tempList.Count);
            }
            foreach (double length in tempYDirList)
            {
                List<double> tempList = yDirList.Where(i => Math.Abs(i - length) <= 5.0).Select(i => i).ToList();
                yVecLengthDic.Add(length, tempList.Count);
            }
            int xNum = xVecLengthDic.OrderByDescending(i => i.Value).Select(i => i.Value).FirstOrDefault();
            int yNum = yVecLengthDic.OrderByDescending(i => i.Value).Select(i => i.Value).FirstOrDefault();
            this.TypeNumber = "1（" + xNum.ToString() + " x " + yNum.ToString() + "）";
        }
        /// <summary>
        /// 左下角点Corner
        /// </summary>
        private List<Curve> GetLeftDownCorner()
        {
            List<Curve> cornerCurves = new List<Curve>();
            Curve firstCurve = GetLeftDownCurve();
            cornerCurves.Add(firstCurve);
            this.originCurve= firstCurve;
            Point3d firstCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(firstCurve));

            var xDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y - firstCurveCenPt.Y) <= this.offsetDis).Select(i => i).ToList();
            xDirCurves = xDirCurves.OrderBy(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();

            var yDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X - firstCurveCenPt.X) <= this.offsetDis).Select(i => i).ToList();
            yDirCurves = yDirCurves.OrderBy(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();

            if (xDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[2]));
                double firstThirdDis = Math.Abs(thirdCurveCenPt.X - firstCurveCenPt.X);
                double firstSecondDis = Math.Abs(secondCurveCenPt.X - firstCurveCenPt.X);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(xDirCurves[1]);
                }
            }
            if (yDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[2]));
                double firstThirdDis = Math.Abs(thirdCurveCenPt.Y - firstCurveCenPt.Y);
                double firstSecondDis = Math.Abs(secondCurveCenPt.Y - firstCurveCenPt.Y);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(yDirCurves[1]);
                }
            }
            return cornerCurves;
        }
        private Curve GetLeftDownCurve()
        {
            Curve curve = null;
            List<double> yValues = this.smallCurves.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();
            yValues = yValues.OrderBy(i => i).ToList();
            foreach (double yValue in yValues)
            {
               List<Curve> fiterCurves= this.smallCurves.Where(i => Math.Abs(ThColumnInfoUtils.TransPtFromWcsToUcs(
                    GetBoundingBoxCenter(i)).Y - yValue) <= this.offsetDis).Select(i => i).ToList();
                if(fiterCurves.Count<=1)
                {
                    continue;
                }
                fiterCurves=fiterCurves.OrderBy(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();
                curve = fiterCurves.First();
                break;
            }
            return curve;
        }
        /// <summary>
        /// 右下角点Corner
        /// </summary>
        private List<Curve> GetRightDownCorner()
        {
            List<Curve> cornerCurves = new List<Curve>();
            Curve firstCurve = GetRightDownCurve();
            cornerCurves.Add(firstCurve);
            Point3d firstCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(firstCurve));

            var xDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y - firstCurveCenPt.Y) <= this.offsetDis).Select(i => i).ToList();
            xDirCurves = xDirCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();

            var yDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X - firstCurveCenPt.X) <= this.offsetDis).Select(i => i).ToList();
            yDirCurves = yDirCurves.OrderBy(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();

            if (xDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[2]));
                double firstThirdDis = Math.Abs(firstCurveCenPt.X - thirdCurveCenPt.X);
                double firstSecondDis = Math.Abs(firstCurveCenPt.X - secondCurveCenPt.X);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(xDirCurves[1]);
                }
            }
            if (yDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[2]));
                double firstThirdDis = Math.Abs(thirdCurveCenPt.Y - firstCurveCenPt.Y);
                double firstSecondDis = Math.Abs(secondCurveCenPt.Y - firstCurveCenPt.Y);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(yDirCurves[1]);
                }
            }
            return cornerCurves;
        }
        private Curve GetRightDownCurve()
        {
            Curve curve = null;
            List<double> yValues = this.smallCurves.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();
            yValues = yValues.OrderBy(i => i).ToList();
            foreach (double yValue in yValues)
            {
                List<Curve> fiterCurves = this.smallCurves.Where(i => Math.Abs(ThColumnInfoUtils.TransPtFromWcsToUcs(
                      GetBoundingBoxCenter(i)).Y - yValue) <= this.offsetDis).Select(i => i).ToList();
                if (fiterCurves.Count <= 1)
                {
                    continue;
                }
                fiterCurves = fiterCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();
                curve = fiterCurves.First();
                break;
            }
            return curve;
        }
        /// <summary>
        /// 右上角点Corner
        /// </summary>
        private List<Curve> GetRightUpCorner()
        {
            List<Curve> cornerCurves = new List<Curve>();
            Curve firstCurve = GetRightUpCurve();
            cornerCurves.Add(firstCurve);
            Point3d firstCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(firstCurve));

            var xDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y - firstCurveCenPt.Y) <= this.offsetDis).Select(i => i).ToList();
            xDirCurves = xDirCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();

            var yDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X - firstCurveCenPt.X) <= this.offsetDis).Select(i => i).ToList();
            yDirCurves = yDirCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();

            if (xDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[2]));
                double firstThirdDis = Math.Abs(firstCurveCenPt.X-thirdCurveCenPt.X);
                double firstSecondDis = Math.Abs(firstCurveCenPt.X-secondCurveCenPt.X);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(xDirCurves[1]);
                }
            }
            if (yDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[2]));
                double firstThirdDis = Math.Abs(firstCurveCenPt.Y-thirdCurveCenPt.Y);
                double firstSecondDis = Math.Abs(firstCurveCenPt.Y- secondCurveCenPt.Y);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(yDirCurves[1]);
                }
            }
            return cornerCurves;
        }
        private Curve GetRightUpCurve()
        {
            Curve curve = null;
            List<double> xValues = this.smallCurves.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();
            xValues = xValues.OrderByDescending(i => i).ToList();
            foreach (double xValue in xValues)
            {
                List<Curve> fiterCurves = this.smallCurves.Where(i => Math.Abs(ThColumnInfoUtils.TransPtFromWcsToUcs(
                      GetBoundingBoxCenter(i)).X - xValue) <= this.offsetDis).Select(i => i).ToList();
                if (fiterCurves.Count <= 1)
                {
                    continue;
                }
                fiterCurves = fiterCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();
                curve = fiterCurves.First();
                break;
            }
            return curve;
        }
        /// <summary>
        /// 左上角点Corner
        /// </summary>
        private List<Curve> GetLeftUpCorner()
        {
            List<Curve> cornerCurves = new List<Curve>();
            Curve firstCurve = GetLeftUpCurve();
            cornerCurves.Add(firstCurve);
            Point3d firstCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(firstCurve));
            var xDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y - firstCurveCenPt.Y) <= this.offsetDis).Select(i => i).ToList();
            xDirCurves = xDirCurves.OrderBy(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();

            var yDirCurves = this.smallCurves.Where(i => Math.Abs(
                 ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X - firstCurveCenPt.X) <= this.offsetDis).Select(i => i).ToList();
            yDirCurves = yDirCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();

            if (xDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(xDirCurves[2]));
                double firstThirdDis = Math.Abs(thirdCurveCenPt.X-firstCurveCenPt.X);
                double firstSecondDis = Math.Abs(secondCurveCenPt.X- firstCurveCenPt.X);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(xDirCurves[1]);
                }
            }
            if (yDirCurves.Count > 2)
            {
                Point3d secondCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[1]));
                Point3d thirdCurveCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(yDirCurves[2]));
                double firstThirdDis = Math.Abs(firstCurveCenPt.Y - thirdCurveCenPt.Y);
                double firstSecondDis = Math.Abs(firstCurveCenPt.Y - secondCurveCenPt.Y);
                if (firstSecondDis <= (firstThirdDis * this.searchRatio))
                {
                    cornerCurves.Add(yDirCurves[1]);
                }
            }
            return cornerCurves;
        }
        private Curve GetLeftUpCurve()
        {
            Curve curve = null;
            List<double> xValues = this.smallCurves.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).X).ToList();
            xValues = xValues.OrderBy(i => i).ToList();
            foreach (double xValue in xValues)
            {
                List<Curve> fiterCurves = this.smallCurves.Where(i => Math.Abs(ThColumnInfoUtils.TransPtFromWcsToUcs(
                      GetBoundingBoxCenter(i)).X - xValue) <= this.offsetDis).Select(i => i).ToList();
                if (fiterCurves.Count <= 1)
                {
                    continue;
                }
                fiterCurves = fiterCurves.OrderByDescending(i => ThColumnInfoUtils.TransPtFromWcsToUcs(GetBoundingBoxCenter(i)).Y).ToList();
                curve = fiterCurves.First();
                break;
            }
            return curve;
        }
        private Point3d GetBoundingBoxCenter(Curve curve)
        {
            Point3d minPt = curve.Bounds.Value.MinPoint;
            Point3d maxPt = curve.Bounds.Value.MaxPoint;
            Point3d cenPt = ThColumnInfoUtils.GetMidPt(minPt, maxPt);
            return new Point3d(cenPt.X, cenPt.Y,0.0);
        }       
    }
}
