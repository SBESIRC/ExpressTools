using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.PlottingServices;
using AcHelper;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using DotNetARX;
using System;
using Autodesk.AutoCAD.Colors;

namespace ThPlot
{
    public class RelatedData
    {
        public Polyline ImagePolyline { get; set; } // 打印的图片多段线
        public Polyline PptPolyline { get; set; }   // PPT多段线
        public List<DBText> PptTextLst { get; set; }         // PPT 说明性文档
        public DBText PageText { get; set; }        // PPT 页码说明

        public RelatedData(Polyline imagePolyline, Polyline pptPolyline = null, DBText pptText = null, DBText pageText = null)
        {
            this.ImagePolyline = imagePolyline;
            this.PptPolyline = pptPolyline;
            this.PptTextLst = new List<DBText>();
            if (pptText != null)
                this.PptTextLst.Add(pptText);
            this.PageText = pageText;
        }
    }

    // 二维窗口的最小点和最大点
    public class WindowData
    {
        public Point2d LeftBottomPoint { get; set; }
        public Point2d RightTopPoint { get; set; }
        public WindowData(Point2d leftPt, Point2d rightPt)
        {
            this.LeftBottomPoint = leftPt;
            this.RightTopPoint = rightPt;
        }

        public WindowData()
        {
            this.LeftBottomPoint = this.RightTopPoint = new Point2d();
        }
    }

    /// <summary>
    /// 页码和轮廓的数据节点
    /// </summary>
    public class PageWithProfile
    {
        public Polyline Profile { get; set; } // 图元对象
        public Point2d EntityPoint { get; set; } // 每个图元的参考点，这里取图元的左顶点
        public DBText PageText { get; set; }  // 每个图元页码

        public PageWithProfile(Polyline polyline, Point2d entityPoint, DBText dbText = null)
        {
            this.Profile = polyline;
            this.EntityPoint = entityPoint;
            this.PageText = dbText;
        }
    }

    public enum SelectDir
    {
        LEFT2RIGHTUP2DOWN = 0, // 左右，上下
        UP2DOWNLEFT2RIGHT  // 上下，左右
    }

    // 自定义点坐标，便于计算
    public class ThPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public ThPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// 网格类，对数据进行预处理，便于后面两种排序计算 (1 -> n的关系）
    /// </summary>
    public class GridBox
    {
        private List<PageWithProfile> m_pageWithProfiles = new List<PageWithProfile>(); // 网格内的数据结构
        public WindowData BoxWindowData { get; set; } // 细分的网格(子网格)

        public List<PageWithProfile> PageWithProfiles
        {
            get { return m_pageWithProfiles; }
        }

        public void AddPageWithProfile(PageWithProfile pageWithProfile)
        {
            m_pageWithProfiles.Add(pageWithProfile);
        }

        public GridBox(WindowData boxWindowData, PageWithProfile pageWithProfile = null)
        {
            this.BoxWindowData = boxWindowData;

            if (pageWithProfile != null)
                m_pageWithProfiles.Add(pageWithProfile);
        }
    }

    /// <summary>
    /// 用户打印PPT项目时的选择信息
    /// </summary>
    public class UserSelectData
    {
        public enum ImageQuality
        {
            IMAGELOWER = 0,
            IMAGEMEDIUM,
            IMAGEHIGHER
        }

        public string PPTLayer { get; set; }        // PPT 层
        public string ImageLayer { get; set; }      // 图片层
        public string PrintTextLayer { get; set; }  // 打印文字层
        public string PrintStyle { get; set; }      // 打印样式
        public string PrintOutPath { get; set; }    // 输出位置路径
        public ImageQuality ImageQua { get; set; }  // 输出图片质量
    }

    public class ThPlotData
    {
        private static readonly double A4PAGEPORTRAITRATIO = 297.0 / 210.0;
        private static readonly double A4PAGELANDSCAPERATIO = 210.0 / 297.0;


        // 绘图，用于测试点的位置
        public static void DrawPointWithTransaction(Point3d drawPoint)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Line lineX = new Line(drawPoint, drawPoint + new Vector3d(1, 0, 0) * 10000);
            Line lineY = new Line(drawPoint, drawPoint + new Vector3d(0, 1, 0) * 10000);

            // 添加到modelSpace中
            AcHelper.DocumentExtensions.AddEntity<Line>(doc, lineX);
            AcHelper.DocumentExtensions.AddEntity<Line>(doc, lineY);
        }

        // 测试用
        public static List<Line> Point2Lines(Point3d drawPoint)
        {
            Line lineX = new Line(drawPoint, drawPoint + new Vector3d(1, 0, 0) * 10000);
            Line lineY = new Line(drawPoint, drawPoint + new Vector3d(0, 1, 0) * 10000);
            return new List<Line>() { lineX, lineY };
        }

        // 测试用
        public static List<Line> Point2Lines(Point2d drawPoint)
        {
            var aimPoint = new Point3d(drawPoint.X, drawPoint.Y, 0);
            Line lineX = new Line(aimPoint, aimPoint + new Vector3d(1, 0, 0) * 10000);
            Line lineY = new Line(aimPoint, aimPoint + new Vector3d(0, 1, 0) * 10000);
            return new List<Line>() { lineX, lineY };
        }

        /// <summary>
        /// 返回值true 时，点在图形内
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="aimPoint"></param>
        /// <returns></returns>
        public static bool PointInnerEntity(Polyline polyline, Point3d aimPoint)
        {
            var lineSegLst = new List<LineSegment2d>();
            var aimPoint2d = new Point2d(aimPoint.X, aimPoint.Y);

            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                var lineSeg = polyline.GetLineSegment2dAt(i);
                lineSegLst.Add(lineSeg);
            }

            LineSegment2d horizontalLine = new LineSegment2d(aimPoint2d, aimPoint2d + new Vector2d(1, 0) * 100000000);
            List<Point2d> ptLst = new List<Point2d>();

            foreach (var line in lineSegLst)
            {
                var intersectPts = line.IntersectWith(horizontalLine);
                if (intersectPts != null)
                    ptLst.AddRange(intersectPts);
            }

            if (ptLst.Count % 2 == 1)
                return true;

            return false;
        }

        /// <summary>
        /// 计算点是否在窗口内部
        /// </summary>
        /// <param name="windowData"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool PointInnerWindowData(WindowData windowData, Point2d point)
        {
            var leftBottomPoint = windowData.LeftBottomPoint;
            var rightTopPoint = windowData.RightTopPoint;
            var nHeight = rightTopPoint.Y - leftBottomPoint.Y;
            var rightBottomPoint = new Point2d(rightTopPoint.X, rightTopPoint.Y - nHeight);
            var leftTopPoint = new Point2d(leftBottomPoint.X, leftBottomPoint.Y + nHeight);

            // 构造轮廓边界
            var lines = new List<LineSegment2d>();
            var line1 = new LineSegment2d(leftBottomPoint, rightBottomPoint);
            var line2 = new LineSegment2d(rightBottomPoint, rightTopPoint);
            var line3 = new LineSegment2d(rightTopPoint, leftTopPoint);
            var line4 = new LineSegment2d(leftTopPoint, leftBottomPoint);
            var lineSegLst = new List<LineSegment2d>();
            lineSegLst.Add(line1);
            lineSegLst.Add(line2);
            lineSegLst.Add(line3);
            lineSegLst.Add(line4);

            LineSegment2d horizontalLine = new LineSegment2d(point, point + new Vector2d(1, 0) * 10000000000);
            List<Point2d> ptLst = new List<Point2d>();

            foreach (var line in lineSegLst)
            {
                var intersectPts = line.IntersectWith(horizontalLine);
                if (intersectPts != null)
                    ptLst.AddRange(intersectPts);
            }

            if (ptLst.Count % 2 == 1)
                return true;

            return false;
        }

        /// <summary>
        /// 根据排版的方向规则给轮廓框线进行编号
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static List<PageWithProfile> CalculateProfileWithPages(List<Polyline> profiles, SelectDir dir, ref int pageIndex)
        {
            if (profiles.Count == 0)
                return null;

            // 页码对应数据，以及所有左顶点的外包轮廓
            var entityWindowData = new WindowData();
            var srcPageWithProfiles = CalculateEntityDatas(profiles, ref entityWindowData);

            // 网格划分数据（预处理）轮廓需要进行外扩处理（为了包含边界顶点处理）
            var gridBoxsLst = CalculateGridBoxs(srcPageWithProfiles, OffsetRectBox(entityWindowData), dir);

            return CalculateProfileWithPagesWithGridBoxs(gridBoxsLst, ref pageIndex);
        }

        /// <summary>
        /// 使用box中的数据，以及排版方向选择 生成最终的顺序
        /// </summary>
        /// <param name="boxsLst"> boxsLst这个网格二维表格的排列顺序是从左到右，从上到下的</param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static List<PageWithProfile> CalculateProfileWithPagesWithGridBoxs(List<List<GridBox>> boxsLst, ref int indexPage)
        {
            var pageWithProfiles = new List<PageWithProfile>();
            foreach (var gridBoxs in boxsLst)
            {
                foreach (var gridBox in gridBoxs)
                {
                    if (gridBox.PageWithProfiles.Count != 0)
                    {
                        // 进入到每个子网格中进行页码匹配计算
                        var pageRes = CalculatePagesWithProfileWithUnitGridBoxHorizontal(gridBox, ref indexPage);
                        pageWithProfiles.AddRange(pageRes);
                    }
                }
            }

            return pageWithProfiles;
        }

        /// <summary>
        /// 从左到右进行单个网格页匹配
        /// </summary>
        /// <param name="unitBox"></param>
        /// <param name="nPageCount"></param>
        /// <returns></returns>
        public static List<PageWithProfile> CalculatePagesWithProfileWithUnitGridBoxHorizontal(GridBox unitBox, ref int nPageCount)
        {
            var profileLst = unitBox.PageWithProfiles;
            var leftX = unitBox.BoxWindowData.LeftBottomPoint.X; // 网格左边的X值
            profileLst.Sort((s1, s2) => { return s1.EntityPoint.X.CompareTo(s2.EntityPoint.X); });
            for (int i = 0; i < profileLst.Count; i++)
            {
                ++nPageCount;
                DBText dbText = new DBText();
                dbText.TextString = nPageCount.ToString();
                profileLst[i].PageText = dbText;
            }
            return profileLst;
        }

        /// <summary>
        /// 根据nLength的值确定是外扩还是内缩， 如果是正值，则外扩，如果是负值，则内缩
        /// </summary>
        /// <param name="srcWindowData"></param>
        /// <param name="nLength"></param>
        /// <returns></returns>
        public static WindowData OffsetRectBox(WindowData srcWindowData, double nLength = 50)
        {
            var leftBottomPoint = srcWindowData.LeftBottomPoint;
            var rightTopPoint = srcWindowData.RightTopPoint;

            var offsetLeftBottom = new Point2d(leftBottomPoint.X - nLength, leftBottomPoint.Y - nLength);
            var offsetRightTop = new Point2d(rightTopPoint.X + nLength, rightTopPoint.Y + nLength);
            return new WindowData(offsetLeftBottom, offsetRightTop);
        }

        /// <summary>
        /// 将外包矩形框进行网格划分，然后再向这些细分得网格中插入数据（可能一个网格中有多个数据，
        /// 可能一个网格中没有数据），后面再对网格结构进行处理
        /// </summary>
        /// <param name="pageWithProfiles"></param>
        /// <param name="totalBoxWindowData"></param>
        /// <param name="gridLength"> 子网格宽度默认为长度100mm</param>
        /// <returns></returns>
        public static List<List<GridBox>> CalculateGridBoxs(List<PageWithProfile> pageWithProfiles, WindowData totalBoxWindowData, SelectDir dir, double gridLength = 5000)
        {
            var leftBottomPoint = totalBoxWindowData.LeftBottomPoint;
            var rightTopPoint = totalBoxWindowData.RightTopPoint;
            var nWidth = rightTopPoint.X - leftBottomPoint.X;
            var nHeight = rightTopPoint.Y - leftBottomPoint.Y;

            // +1 处理是为了取整处理， 这个时候的实际每个box的长度值一般是小于gridLength
            int nHorizontalCount = (int)(nWidth / gridLength) + 1;
            int nVerticalCount = (int)(nHeight / gridLength) + 1;
            double nUnitHorizontalLength = nWidth / nHorizontalCount;
            double nUnitVerticalLength = nHeight / nVerticalCount;

            // 1->构造GridBoxs
            var gridBoxsLst = new List<List<GridBox>>();
            var leftTopPoint = new Point2d(leftBottomPoint.X, leftBottomPoint.Y + nHeight);

            if (dir == SelectDir.UP2DOWNLEFT2RIGHT)
            {
                for (int i = 0; i < nHorizontalCount; i++)
                {
                    var gridBoxs = new List<GridBox>();

                    for (int j = 0; j < nVerticalCount; j++)
                    {
                        var offsetLeftTopPointX = i * nUnitHorizontalLength;
                        var offsetLeftTopPointY = j * nUnitVerticalLength;
                        var curLeftTopPoint = new Point2d(leftTopPoint.X + offsetLeftTopPointX, leftTopPoint.Y - offsetLeftTopPointY);
                        var curRightBottomPoint = new Point2d(curLeftTopPoint.X + nUnitHorizontalLength, curLeftTopPoint.Y - nUnitVerticalLength);
                        WindowData boxData = ConvertToWindowData(curLeftTopPoint, curRightBottomPoint, nUnitVerticalLength);
                        var box = new GridBox(boxData);
                        gridBoxs.Add(box);
                    }

                    gridBoxsLst.Add(gridBoxs);
                }
            }
            else
            {
                for (int i = 0; i < nVerticalCount; i++)
                {
                    var gridBoxs = new List<GridBox>();

                    for (int j = 0; j < nHorizontalCount; j++)
                    {
                        var offsetLeftTopPointX = j * nUnitHorizontalLength;
                        var offsetLeftTopPointY = i * nUnitVerticalLength;
                        var curLeftTopPoint = new Point2d(leftTopPoint.X + offsetLeftTopPointX, leftTopPoint.Y - offsetLeftTopPointY);
                        var curRightBottomPoint = new Point2d(curLeftTopPoint.X + nUnitHorizontalLength, curLeftTopPoint.Y - nUnitVerticalLength);
                        WindowData boxData = ConvertToWindowData(curLeftTopPoint, curRightBottomPoint, nUnitVerticalLength);
                        var box = new GridBox(boxData);
                        gridBoxs.Add(box);
                    }

                    gridBoxsLst.Add(gridBoxs);
                }
            }


            // 2-> 向GridBoxs中插入PageWithProfile对象
            // 判断每一个轮廓页和box的位置包含关系
            foreach (var pageWithProfile in pageWithProfiles)
            {
                GridBoxsAddPageProfile(gridBoxsLst, pageWithProfile);
            }

            return gridBoxsLst;
        }

        /// <summary>
        /// 插入页
        /// </summary>
        /// <param name="gridBoxsLst"></param>
        /// <param name="pageWithProfile"></param>
        public static void GridBoxsAddPageProfile(List<List<GridBox>> gridBoxsLst, PageWithProfile pageWithProfile)
        {
            foreach (var boxs in gridBoxsLst)
            {
                foreach (var box in boxs)
                {
                    if (GridBoxContainsProfile(box, pageWithProfile.EntityPoint))
                    {
                        box.AddPageWithProfile(pageWithProfile);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 计算box是否包含左顶点
        /// </summary>
        /// <param name="gridBox"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static bool GridBoxContainsProfile(GridBox gridBox, Point2d entityPoint)
        {
            var windowData = gridBox.BoxWindowData;

            if (PointInnerWindowData(windowData, entityPoint))
                return true;

            return false;
        }

        /// <summary>
        /// 坐标点的转化
        /// </summary>
        /// <param name="leftTopPoint"></param>
        /// <param name="rightBottomPoint"></param>
        /// <param name="nUnitWidth"></param>
        /// <param name="nUnitHeight"></param>
        /// <returns></returns>
        public static WindowData ConvertToWindowData(Point2d leftTopPoint, Point2d rightBottomPoint, double nUnitHeight)
        {
            var leftBottomPoint = new Point2d(leftTopPoint.X, leftTopPoint.Y - nUnitHeight);
            var rightTopPoint = new Point2d(rightBottomPoint.X, rightBottomPoint.Y + nUnitHeight);
            return new WindowData(leftBottomPoint, rightTopPoint);
        }

        /// <summary>
        /// 构造图元的计算结构，计算图元的外包左顶点
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="windowData"> 返回矩形窗口</param>
        /// <returns></returns>
        public static List<PageWithProfile> CalculateEntityDatas(List<Polyline> profiles, ref WindowData windowData)
        {
            if (profiles.Count == 0)
                return null;

            var pageWithProfiles = new List<PageWithProfile>();
            var points = new List<Point2d>();
            foreach (var profile in profiles)
            {
                var leftTopPoint = CalculateLeftTopPoint(profile);
                points.Add(leftTopPoint);
                var curPageWithProfile = new PageWithProfile(profile, leftTopPoint);
                pageWithProfiles.Add(curPageWithProfile);
            }

            // 计算矩形框数据
            windowData = GetEntityPointsWindow(points);
            return pageWithProfiles;
        }

        /// <summary>
        /// 计算图元的左顶点
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static Point2d CalculateLeftTopPoint(Polyline profile)
        {
            var points = GetPolylinePoints(profile);
            var windowData = GetEntityPointsWindow(points);
            return CalculateLeftPointFromWindow(windowData);
        }

        /// <summary>
        /// 计算窗口左顶点
        /// </summary>
        /// <param name="windowData"></param>
        /// <returns></returns>
        public static Point2d CalculateLeftPointFromWindow(WindowData windowData)
        {
            var leftBottomPoint = windowData.LeftBottomPoint;
            var rightTopPoint = windowData.RightTopPoint;
            var height = rightTopPoint.Y - leftBottomPoint.Y;
            return new Point2d(leftBottomPoint.X, leftBottomPoint.Y + height);
        }

        /// <summary>
        /// 计算文本相对于轮廓左顶角的位置，用于向PPT中插入文本
        /// </summary>
        /// <param name="text"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static Point2d PptTextPosRelatedToProfilePos(DBText text, Polyline profile)
        {
            var ptLst = ThPlotData.GetPolylinePoints(profile);
            var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
            var height = windowData.RightTopPoint.Y - windowData.LeftBottomPoint.Y;
            var leftTop = new Point2d(windowData.LeftBottomPoint.X, windowData.LeftBottomPoint.Y + height);
            var textPos = new Point2d(text.Position.X, text.Position.Y);
            return new Point2d(Math.Abs(textPos.X - leftTop.X), Math.Abs(textPos.Y - leftTop.Y));
        }

        /// <summary>
        /// 计算位置相当于轮廓的位置比例
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="profile"></param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetPosRelatedRation(DBText text, Polyline profile, ref double xRation, ref double yRation)
        {
            var ptLst = ThPlotData.GetPolylinePoints(profile);
            var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
            var height = windowData.RightTopPoint.Y - windowData.LeftBottomPoint.Y;
            var width = windowData.RightTopPoint.X - windowData.LeftBottomPoint.X;
            var leftTop = new Point2d(windowData.LeftBottomPoint.X, windowData.LeftBottomPoint.Y + height);
            var textPos = new Point2d(text.Position.X, text.Position.Y);
            var textPos2OrigionX = Math.Abs(textPos.X - leftTop.X);
            var textPos2OrigionY = Math.Abs(textPos.Y - leftTop.Y);
            xRation = textPos2OrigionX / width;
            yRation = textPos2OrigionY / height;
        }

        /// <summary>
        /// 计算Polyline的宽边跟高边 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="lineWidth"></param>
        /// <param name="lineHeight"></param>
        /// <returns></returns>
        public static WindowData CalculateProfileTwoEdge(Polyline profile, ref double lineWidthLength, ref double lineHeightLength)
        {
            var pt2dLst = ThPlotData.GetPolylinePoints(profile);

            ThPoint minPoint = new ThPoint(pt2dLst[0].X, pt2dLst[0].Y);
            ThPoint maxPoint = new ThPoint(pt2dLst[0].X, pt2dLst[0].Y);

            // 计算最小点和最大点
            for (int j = 1; j < pt2dLst.Count; j++)
            {
                if (pt2dLst[j].X < minPoint.X)
                    minPoint.X = pt2dLst[j].X;
                if (pt2dLst[j].Y < minPoint.Y)
                    minPoint.Y = pt2dLst[j].Y;

                if (pt2dLst[j].X > maxPoint.X)
                    maxPoint.X = pt2dLst[j].X;
                if (pt2dLst[j].Y > maxPoint.Y)
                    maxPoint.Y = pt2dLst[j].Y;
            }

            // 创建两条直线根据斜率确定宽线 和高线
            Point2d ptFir = new Point2d(minPoint.X, minPoint.Y);
            Point2d ptSec = new Point2d(maxPoint.X, minPoint.Y);
            Point2d ptThird = new Point2d(maxPoint.X, maxPoint.Y);
            Line2d lineFir = new Line2d(ptFir, ptSec);
            Line2d lineSec = new Line2d(ptSec, ptThird);

            double lineFirAngle = lineFir.Direction.GetAngleTo(new Vector2d(1, 0));
            double lineSecAngle = lineSec.Direction.GetAngleTo(new Vector2d(1, 0));
            if (lineFirAngle < lineSecAngle)
            {
                lineWidthLength = (ptFir - ptSec).Length;
                lineHeightLength = (ptSec - ptThird).Length;
            }
            else
            {
                lineHeightLength = (ptFir - ptSec).Length;
                lineWidthLength = (ptSec - ptThird).Length;
            }

            return new WindowData(new Point2d(minPoint.X, minPoint.Y), new Point2d(maxPoint.X, maxPoint.Y));
        }

        /// <summary>
        /// 计算Page的相对位置关系，考虑到留白处理的情形
        /// </summary>
        /// <param name="imageProfile"></param>
        /// <param name="pptProfile"></param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetImagePosRelatedRation(Polyline imageProfile, Polyline pptProfile, ref double xRation, ref double yRation)
        {
            var a4PaperRation = CalculatePaperSizeInfo(imageProfile);
            double lineWidth = 0;
            double lineHeight = 0;
            var window = CalculateProfileTwoEdge(imageProfile, ref lineWidth, ref lineHeight);
            var standardHeight = a4PaperRation * lineWidth;
            var standardWidth = lineHeight / a4PaperRation;

            var windowHeight = window.RightTopPoint.Y - window.LeftBottomPoint.Y;
            // 原始框的左顶点
            var topLeftPos = new Point2d(window.LeftBottomPoint.X, window.LeftBottomPoint.Y + windowHeight);

            // PPt轮廓的左顶点
            var pptPoints = GetPolylinePoints(pptProfile);
            var pptWindow = GetEntityPointsWindow(pptPoints);
            var pptWindowHeight = pptWindow.RightTopPoint.Y - pptWindow.LeftBottomPoint.Y;
            var pptWindowWidth = pptWindow.RightTopPoint.X - pptWindow.LeftBottomPoint.X;
            var pptTopLeftPos = new Point2d(pptWindow.LeftBottomPoint.X, pptWindow.LeftBottomPoint.Y + pptWindowHeight);
            // 计算生成纸张后考虑留白的轮廓的左顶点
            if (standardWidth > lineWidth)
            {
                topLeftPos += (new Vector2d(1, 0) * (standardWidth - lineWidth) * 0.5);
            }
            else if (standardHeight > lineHeight)
            {
                topLeftPos += (new Vector2d(0, 1) * (standardHeight - lineHeight) * 0.5);
            }

            var horizontalX = Math.Abs(topLeftPos.X - pptTopLeftPos.X);
            var verticalY = Math.Abs(topLeftPos.Y - pptTopLeftPos.Y);
            xRation = horizontalX / pptWindowWidth;
            yRation = verticalY / pptWindowHeight;
        }

        /// <summary>
        /// 计算Page的相对位置关系，考虑到留白处理的情形 以及旋转角度
        /// </summary>
        /// <param name="imageProfile"></param>
        /// <param name="pptProfile"></param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetImagePosRelatedRationWithAngle(RelatedData relatedData,  ref double xRation, ref double yRation)
        {
            var pptText = relatedData.PptTextLst;
            Polyline imageProfile = relatedData.ImagePolyline;
            Polyline pptProfile = relatedData.PptPolyline;

            // 尺寸计算
            var a4PaperRation = CalculatePaperSizeInfo(imageProfile);
            double lineWidth = 0;
            double lineHeight = 0;
            var window = CalculateProfileTwoEdge(imageProfile, ref lineWidth, ref lineHeight);
            var standardHeight = a4PaperRation * lineWidth;
            var standardWidth = lineHeight / a4PaperRation;

            var windowHeight = window.RightTopPoint.Y - window.LeftBottomPoint.Y;
            // 原始框的左顶点
            var topLeftPos = new Point2d(window.LeftBottomPoint.X, window.LeftBottomPoint.Y + windowHeight);

            // PPt轮廓的左顶点
            var pptPoints = GetPolylinePoints(pptProfile);
            var pptWindow = GetEntityPointsWindow(pptPoints);
            var pptWindowHeight = pptWindow.RightTopPoint.Y - pptWindow.LeftBottomPoint.Y;
            var pptWindowWidth = pptWindow.RightTopPoint.X - pptWindow.LeftBottomPoint.X;
            var pptTopLeftPos = new Point2d(pptWindow.LeftBottomPoint.X, pptWindow.LeftBottomPoint.Y + pptWindowHeight);

            // 计算生成纸张后考虑留白的轮廓的左顶点
            if (standardWidth > lineWidth)
            {
                topLeftPos += (new Vector2d(1, 0) * (standardWidth - lineWidth) * 0.5);
            }
            else if (standardHeight > lineHeight)
            {
                topLeftPos += (new Vector2d(0, 1) * (standardHeight - lineHeight) * 0.5);
            }

            var horizontalX = Math.Abs(topLeftPos.X - pptTopLeftPos.X);
            var verticalY = Math.Abs(topLeftPos.Y - pptTopLeftPos.Y);
            xRation = horizontalX / pptWindowWidth;
            yRation = verticalY / pptWindowHeight;
        }

        /// <summary>
        /// 计算当前纸张的尺寸
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static double CalculatePaperSizeInfo(Polyline profile)
        {
            var ptLst = GetPolylinePoints(profile);
            var windowData = GetEntityPointsWindow(ptLst);
            double width = 0;
            double height = 0;
            ThPlotUtil.GetWindowDataWidthAndHeight(windowData, ref width, ref height);
            if (width < height)
            {
                return A4PAGEPORTRAITRATIO;
            }

            return A4PAGELANDSCAPERATIO;
        }

        /// <summary>
        /// 获取图层中多段线的Id对象
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<ObjectId> GetObjectIdFromLayer(string layerName)
        {
            var result = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<Polyline>().Where(p => p.Layer == layerName);
                foreach (var pline in res)
                {
                    result.Add(pline.ObjectId);
                }
                return result;
            }
        }

        /// <summary>
        /// 获取layer名字
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLayerNames()
        {
            var layerNames = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers; ;
                foreach (var layer in layers)
                {
                    layerNames.Add(layer.Name);
                }
            }

            return layerNames;
        }

        // 获取图层中文本的Id对象
        public static List<ObjectId> GetTextIdFromLayer(string layerName)
        {
            var result = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<DBText>().Where(p => p.Layer == layerName);
                foreach (var text in res)
                {
                    result.Add(text.ObjectId);
                }
                return result;
            }
        }

        // 只计算Z轴为0的外包矩形框
        public static WindowData GetEntityPointsWindow(Point3dCollection points)
        {
            if (points.Count == 0)
                return null;

            List<ThPoint> pt2dLst = new List<ThPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                //// UCS坐标到DCS坐标转化
                //Point3d ptDcs = UCSTools.TranslateCoordinates(points[i], UCSTools.CoordSystem.UCS, UCSTools.CoordSystem.DCS);
                pt2dLst.Add(new ThPoint(points[i].X, points[i].Y));
            }

            ThPoint minPoint = new ThPoint(pt2dLst[0].X, pt2dLst[0].Y);
            ThPoint maxPoint = new ThPoint(pt2dLst[0].X, pt2dLst[0].Y);

            // 计算最小点和最大点
            for (int j = 1; j < pt2dLst.Count; j++)
            {
                if (pt2dLst[j].X < minPoint.X)
                    minPoint.X = pt2dLst[j].X;
                if (pt2dLst[j].Y < minPoint.Y)
                    minPoint.Y = pt2dLst[j].Y;

                if (pt2dLst[j].X > maxPoint.X)
                    maxPoint.X = pt2dLst[j].X;
                if (pt2dLst[j].Y > maxPoint.Y)
                    maxPoint.Y = pt2dLst[j].Y;
            }

            return new WindowData(new Point2d(minPoint.X, minPoint.Y), new Point2d(maxPoint.X, maxPoint.Y));
        }

        /// <summary>
        /// 计算二维坐标的窗口信息，外包矩形
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static WindowData GetEntityPointsWindow(List<Point2d> points)
        {
            if (points.Count == 0)
                return null;

            List<ThPoint> pt2dLst = new List<ThPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                pt2dLst.Add(new ThPoint(points[i].X, points[i].Y));
            }

            ThPoint minPoint = new ThPoint(pt2dLst[0].X, pt2dLst[0].Y);
            ThPoint maxPoint = new ThPoint(pt2dLst[0].X, pt2dLst[0].Y);

            // 计算最小点和最大点
            for (int j = 1; j < pt2dLst.Count; j++)
            {
                if (pt2dLst[j].X < minPoint.X)
                    minPoint.X = pt2dLst[j].X;
                if (pt2dLst[j].Y < minPoint.Y)
                    minPoint.Y = pt2dLst[j].Y;

                if (pt2dLst[j].X > maxPoint.X)
                    maxPoint.X = pt2dLst[j].X;
                if (pt2dLst[j].Y > maxPoint.Y)
                    maxPoint.Y = pt2dLst[j].Y;
            }

            return new WindowData(new Point2d(minPoint.X, minPoint.Y), new Point2d(maxPoint.X, maxPoint.Y));
        }

        // 获取多段线的顶点坐标
        public static Point3dCollection GetPolylinePoints(Polyline polyline)
        {
            Point3dCollection ptLst = new Point3dCollection();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                ptLst.Add(polyline.GetPoint3dAt(i));
            }

            return ptLst;
        }

        // 获取多段线的顶点坐标
        public static List<Point3d> GetPolylinePointLst(Polyline polyline)
        {
            List<Point3d> ptLst = new List<Point3d>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                ptLst.Add(polyline.GetPoint3dAt(i));
            }

            return ptLst;
        }

        /// <summary>
        /// 根据PPT轮廓计算需要插入的DBText
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public static DBText InsertPageText(Polyline profile, int page)
        {
            var pageText = new DBText();
            var ptLst = GetPolylinePoints(profile);
            var windowData = GetEntityPointsWindow(ptLst);
            var leftBottomPos = windowData.LeftBottomPoint;
            var rightTopPos = windowData.RightTopPoint;
            var rightBottomPos = new Point3d(rightTopPos.X, leftBottomPos.Y, 0);
            pageText.TextString = page.ToString();

            // 根据现有PPT框线的一个比例计算字符的高度
            var heightRatio = 800.0 / 27000;
            var profileHeight = rightTopPos.Y - leftBottomPos.Y;
            pageText.Height = heightRatio * profileHeight;
            pageText.Position = new Point3d(rightBottomPos.X - 2500, rightBottomPos.Y + 1000, 0);
            return pageText;
        }

        /// <summary>
        /// 向polyline轮廓的左下角插入页码
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="dbText">页码</param>
        /// <returns></returns>
        public static DBText SetPageTextToProfileCorner(Polyline profile, DBText dbText)
        {
            var ptLst = GetPolylinePoints(profile);
            var windowData = GetEntityPointsWindow(ptLst);
            var leftBottomPos = windowData.LeftBottomPoint;
            var rightTopPos = windowData.RightTopPoint;
            var rightBottomPos = new Point3d(rightTopPos.X, leftBottomPos.Y, 0);

            // 根据现有PPT框线的一个比例计算字符的高度
            var heightRatio = 800.0 / 27000;
            var profileHeight = rightTopPos.Y - leftBottomPos.Y;
            dbText.Height = heightRatio * profileHeight;
            dbText.Position = new Point3d(rightBottomPos.X - 2500 - dbText.Height, rightBottomPos.Y + 1000 + dbText.Height, 0);
            return dbText;
        }

        /// <summary>
        /// 两个多段线相交
        /// </summary>
        /// <param name="relatedData"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static bool BoundaryIntersectWithBound(RelatedData relatedData, Polyline polyline)
        {
            Point3dCollection ptLst = new Point3dCollection();
            var curImagePolyline = relatedData.ImagePolyline;
            curImagePolyline.IntersectWith(polyline, Intersect.OnBothOperands, ptLst, new System.IntPtr(0), new System.IntPtr(0));

            if (ptLst.Count != 0)
            {
                relatedData.PptPolyline = polyline;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 前者在后者图元的内部 返回true
        /// </summary>
        /// <param name="relatedData"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static bool EntityContainsEntity(RelatedData relatedData, Polyline polyline)
        {
            var curImagePolyline = relatedData.ImagePolyline;
            var ptLst = GetPolylinePointLst(curImagePolyline);

            foreach (var pt in ptLst)
            {
                if (!PointInnerEntity(polyline, pt))
                {
                    return false;
                }
            }

            relatedData.PptPolyline = polyline;
            return true;
        }

        /// <summary>
        /// 生成数据的相关关系，Image框， PPT框，ppt文字 页码文字
        /// </summary>
        /// <returns></returns>
        public static List<RelatedData> MakeRelatedData(UserSelectData userData)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            // 获取不同图层的数据
            var imageIdLst = GetObjectIdFromLayer(userData.ImageLayer);
            var pptIdLst = GetObjectIdFromLayer(userData.PPTLayer);
            var pptTextIdLst = GetTextIdFromLayer(userData.PrintTextLayer);
            var pageTextIdLst = GetTextIdFromLayer(userData.PPTLayer);
            var imagePolylineLst = new List<Polyline>();
            var pptPolylineLst = new List<Polyline>();
            var pptTextLst = new List<DBText>();
            var pageTextLst = new List<DBText>();
            var pptRelatedData = new List<RelatedData>();

            using (Transaction dataGetTrans = db.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var imageId in imageIdLst)
                {
                    Polyline imagePolyline = (Polyline)dataGetTrans.GetObject(imageId, OpenMode.ForRead);
                    imagePolylineLst.Add((Polyline)imagePolyline.Clone());
                }

                if (imagePolylineLst.Count == 0)
                    return null;

                imagePolylineLst.ForEach(p => pptRelatedData.Add(new RelatedData(p)));

                // PPT多段线
                foreach (var pptId in pptIdLst)
                {
                    Polyline pptPolyline = (Polyline)dataGetTrans.GetObject(pptId, OpenMode.ForRead);
                    pptPolylineLst.Add((Polyline)pptPolyline.Clone());
                }

                if (pptPolylineLst.Count == 0)
                    return null;

                // PPT文本
                foreach (var textId in pptTextIdLst)
                {
                    DBText dbText = (DBText)dataGetTrans.GetObject(textId, OpenMode.ForRead);
                    pptTextLst.Add((DBText)dbText.Clone());
                }

                // 页码文本
                foreach (var pageTextId in pageTextIdLst)
                {
                    DBText dbPageText = (DBText)dataGetTrans.GetObject(pageTextId, OpenMode.ForRead);
                    pageTextLst.Add((DBText)dbPageText.Clone());
                }

                // polyline 线框文本信息等匹配
                foreach (var relatedData in pptRelatedData)
                {
                    var curImagePolyline = relatedData.ImagePolyline;
                    foreach (var pptPolyline in pptPolylineLst)
                    {
                        if (BoundaryIntersectWithBound(relatedData, pptPolyline) || EntityContainsEntity(relatedData, pptPolyline))
                        {
                            break;
                        }
                    }
                }

                foreach (var relatedData in pptRelatedData)
                {
                    var curPptPolyline = relatedData.PptPolyline;
                    if (curPptPolyline == null)
                        continue;

                    // ppt文本
                    foreach (var pptText in pptTextLst)
                    {
                        if (PointInnerEntity(curPptPolyline, pptText.Position))
                        {
                            relatedData.PptTextLst.Add(pptText);
                        }
                    }

                    // 页码文本
                    foreach (var pageText in pageTextLst)
                    {
                        if (PointInnerEntity(curPptPolyline, pageText.Position))
                        {
                            relatedData.PageText = pageText;
                            break;
                        }
                    }
                }

                // 页码从大到小排序
                var validRelatedData = pptRelatedData.Where(s1 => s1.PageText != null).ToList();
                validRelatedData.Sort((s1, s2) => { return Double.Parse(s1.PageText.TextString).CompareTo(Double.Parse(s2.PageText.TextString)); });
                validRelatedData.Reverse();
                return validRelatedData;
            }
        }
    }
}
