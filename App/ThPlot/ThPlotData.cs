﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Linq2Acad;
using DotNetARX;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThPlot
{
    public class RelatedData
    {
        public Polyline ImagePolyline { get; set; } // 打印的图片多段线
        public Polyline PptPolyline { get; set; }   // PPT多段线
        public List<DBText> PptTextLst { get; set; } // PPT 说明性文档
        public List<MText> PptMtextLst { get; set; } // PPT 多行说明性文档
        public DBText PageText { get; set; }        // PPT 页码说明

        public RelatedData(Polyline imagePolyline, Polyline pptPolyline = null, DBText pptText = null, MText pptMtext = null, DBText pageText = null)
        {
            this.ImagePolyline = imagePolyline;
            this.PptPolyline = pptPolyline;
            this.PptTextLst = new List<DBText>();
            this.PptMtextLst = new List<MText>();
            if (pptText != null)
                this.PptTextLst.Add(pptText);
            if (pptMtext != null)
                this.PptMtextLst.Add(pptMtext);
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
        // DPI设置
        public enum ImageQuality
        {
            IMAGEUNKOWN = -1,
            IMAGELOWER = 0,
            IMAGEMEDIUM,
            IMAGEHIGHER
        }

        // 选择方式
        public enum SelectWay
        {
            SELECTUNKNOWN = -1,
            SINGLESELECT = 0,
            RECTSELECTLEFTRIGHT,
            RECTSELECTUPDOWN
        }

        public UserSelectData()
        {
            PPTLayer = null;
            ImageLayer = null;
            PrintTextLayer = null;
            PrintStyle = null;
            PrintOutPath = null;
            InsertTemplateFile = false;
        }

        public string PPTLayer { get; set; }        // PPT 层
        public string ImageLayer { get; set; }      // 图片层
        public string PrintTextLayer { get; set; }  // 打印文字层
        public string PrintStyle { get; set; }      // 打印样式
        public string PrintOutPath { get; set; }    // 输出位置路径
        public ImageQuality ImageQua { get; set; }  // 输出图片质量
        public bool InsertTemplateFile { get; set; } // 插入模板文件

        public SelectWay SelectStyle { get; set; } // 选择框选方式
    }

    public class ThPlotData
    {
        private static readonly double A2PAPERLANDSCAPERATIO = 594.00 / 420.00;

        public static readonly double PI = 3.1415926535898;
        public static readonly double PPTWIDTH = 1010;
        public static readonly double PPTHEIGHT = 755;
        public static readonly string PAGELAYER = "天华页码";

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

        //  绘图，用于测试点的位置
        public static List<Line> Point2Lines(Point3d drawPoint)
        {
            Line lineX = new Line(drawPoint, drawPoint + new Vector3d(1, 0, 0) * 10000);
            Line lineY = new Line(drawPoint, drawPoint + new Vector3d(0, 1, 0) * 10000);
            return new List<Line>() { lineX, lineY };
        }

        //  绘图，用于测试点的位置
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

        public static void InsertPageWithProfiles(List<PageWithProfile> pageWithProfiles, string layerName)
        {
            // 计算最小PPT框高度
            var minHeight = CalculateMinHeight(pageWithProfiles);
            var doc = Application.DocumentManager.MdiActiveDocument;
            var database = doc.Database;

            using (var db = AcadDatabase.Active())
            {
                foreach (var pageWithProfile in pageWithProfiles)
                {
                    var dbText = ThPlotData.SetPageTextToProfileCorner(pageWithProfile.Profile, pageWithProfile.PageText);
                    dbText.ColorIndex = 256;
                    dbText.Height = minHeight / 15;
                    var objectId = db.ModelSpace.Add(dbText);
                    db.ModelSpace.Element(objectId, true).Layer = layerName;
                    using (var tr = database.TransactionManager.StartTransaction())
                    {
                        database.TransactionManager.QueueForGraphicsFlush();
                    }
                }
            }
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
        public static void GetTextPosRelatedRation(DBText text, Polyline profile, ref double xRation, ref double yRation)
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
        /// 计算Polyline的水平边和垂直高边 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="lineWidthLength"> 这个是水平的长度值</param>
        /// <param name="lineHeightLength"> 这个是垂直的高度值</param>
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
        public static void GetImagePosRelatedRation(Polyline imageProfile, Polyline pptProfile, ref double xRationPos, ref double yRationPos)
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
                topLeftPos -= (new Vector2d(1, 0) * (standardWidth - lineWidth) * 0.5);
            }
            else if (standardHeight > lineHeight)
            {
                topLeftPos += (new Vector2d(0, 1) * (standardHeight - lineHeight) * 0.5);
            }

            var horizontalX = (topLeftPos.X - pptTopLeftPos.X);
            var verticalY = -(topLeftPos.Y - pptTopLeftPos.Y);
            var xRatio = horizontalX / pptWindowWidth;
            var yRatio = verticalY / pptWindowHeight;
            xRationPos = xRatio * ThPlotData.PPTWIDTH;
            yRationPos = yRatio * ThPlotData.PPTHEIGHT;
        }

        /// <summary>
        /// 计算Page的相对位置关系，考虑到留白处理的情形 以及旋转角度
        /// </summary>
        /// <param name="imageProfile"></param>
        /// <param name="pptProfile"></param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetImagePosRelatedRationWithAngle(RelatedData relatedData, ref double xRatioPos, ref double yRatioPos, ref double pictureRotateWidth, ref double pictureRotateHeight)
        {
            var pptTextAngle = relatedData.PptTextLst.First().Rotation / ThPlotData.PI * 180;
            Polyline imageProfile = relatedData.ImagePolyline;
            Polyline pptProfile = relatedData.PptPolyline;

            // 计算不同情形下的距离PPT左顶点的计算方式 standardWidth 都是水平方向的长度， standardHeight是纵向的高度
            if (ThPlotUtil.DoubleEqualValue(pptTextAngle, 90, 5))
                CalculateAngle90(imageProfile, pptProfile, ref xRatioPos, ref yRatioPos, ref pictureRotateWidth, ref pictureRotateHeight);
            else if (ThPlotUtil.DoubleEqualValue(pptTextAngle, 180, 5))
                CalculateAngle180(imageProfile, pptProfile, ref xRatioPos, ref yRatioPos, ref pictureRotateWidth, ref pictureRotateHeight);
            else if (ThPlotUtil.DoubleEqualValue(pptTextAngle, 270, 5))
                CalculateAngle270(imageProfile, pptProfile, ref xRatioPos, ref yRatioPos, ref pictureRotateWidth, ref pictureRotateHeight);
        }

        /// <summary>
        /// 计算Image的左下顶点的位置比例
        /// </summary>
        /// <param name="imageProfile"></param>
        /// <param name="pptProfile"></param>
        /// <param name="xRatio"></param>
        /// <param name="yRatio"></param>
        public static void CalculateAngle90(Polyline imageProfile, Polyline pptProfile, ref double xRatioPos, ref double yRatioPos, ref double pictureRotateWidth, ref double pictureRotateHeight)
        {
            var a4PaperRation = CalculatePaperSizeInfo(imageProfile);
            double horizontalValue = 0; // 水平值
            double verticalValue = 0; // 竖向值
            var imageWindow = CalculateProfileTwoEdge(imageProfile, ref horizontalValue, ref verticalValue);
            var standardVerticalValue = a4PaperRation * horizontalValue;
            var standardHorizontalValue = verticalValue / a4PaperRation;

            // 计算imageWndow的左下顶点 和ppt框的左下顶点
            var ptLst = GetPolylinePoints(pptProfile);
            var pptWindow = GetEntityPointsWindow(ptLst);
            var pptWindowVertical = pptWindow.RightTopPoint.Y - pptWindow.LeftBottomPoint.Y;
            var pptWindowHorizontal = pptWindow.RightTopPoint.X - pptWindow.LeftBottomPoint.X;
            var pptLeftBottomPoint = pptWindow.LeftBottomPoint;

            // 计算生成纸张后考虑留白的轮廓的新的左下顶点
            var imageLeftBottomPoint = imageWindow.LeftBottomPoint;
            if (standardHorizontalValue > horizontalValue)
            {
                imageLeftBottomPoint -= (new Vector2d(1, 0) * (standardHorizontalValue - horizontalValue) * 0.5);
            }
            else if (standardVerticalValue > verticalValue)
            {
                imageLeftBottomPoint -= (new Vector2d(0, 1) * (standardVerticalValue - verticalValue) * 0.5);
            }

            var yGapValue = imageLeftBottomPoint.Y - pptLeftBottomPoint.Y;
            var xGapValue = imageLeftBottomPoint.X - pptLeftBottomPoint.X;
            // 左下顶点POS
            var xRatio = yGapValue / pptWindowVertical;
            var yRatio = xGapValue / pptWindowHorizontal;
            var xRatioPosTopLeft = xRatio * ThPlotData.PPTWIDTH;
            var yRatioPosTopLeft = yRatio * ThPlotData.PPTHEIGHT;

            // picture的在PPT中的的宽值和高值
            pictureRotateWidth = standardHorizontalValue / pptWindowHorizontal * ThPlotData.PPTHEIGHT;
            pictureRotateHeight = standardVerticalValue / pptWindowVertical * ThPlotData.PPTWIDTH;
            var imageLenthAdd = pictureRotateHeight * 0.5;
            var imageHeightAdd = pictureRotateWidth * 0.5;
            var centerPosX = xRatioPosTopLeft + imageLenthAdd;
            var centerPosY = yRatioPosTopLeft + imageHeightAdd;
            xRatioPos = centerPosX - imageHeightAdd;
            yRatioPos = centerPosY - imageLenthAdd;
        }

        /// <summary>
        /// 计算Image的右下顶点的位置比例， 旋转180的情形
        /// </summary>
        /// <param name="imageProfile"></param>
        /// <param name="pptProfile"></param>
        /// <param name="xRatio"></param>
        /// <param name="yRatio"></param>
        public static void CalculateAngle180(Polyline imageProfile, Polyline pptProfile, ref double xRatioPos, ref double yRatioPos, ref double pictureRotateWidth, ref double pictureRotateHeight)
        {
            var a4PaperRation = CalculatePaperSizeInfo(imageProfile);
            double horizontalValue = 0; // 水平值
            double verticalValue = 0; // 竖向值
            var imageWindow = CalculateProfileTwoEdge(imageProfile, ref horizontalValue, ref verticalValue);
            var standardVerticalValue = a4PaperRation * horizontalValue;
            var standardHorizontalValue = verticalValue / a4PaperRation;

            // 计算imageWndow的右下顶点 和ppt框的右下顶点
            var ptLst = GetPolylinePoints(pptProfile);
            var pptWindow = GetEntityPointsWindow(ptLst);
            var pptWindowVertical = pptWindow.RightTopPoint.Y - pptWindow.LeftBottomPoint.Y;
            var pptWindowHorizontal = pptWindow.RightTopPoint.X - pptWindow.LeftBottomPoint.X;
            var pptLeftBottomPoint = pptWindow.LeftBottomPoint;
            var pptRightBottomPoint = new Point2d(pptLeftBottomPoint.X + pptWindowHorizontal, pptLeftBottomPoint.Y);

            // 计算生成纸张后考虑留白的轮廓的新的右下顶点
            var imageLeftBottomPoint = imageWindow.LeftBottomPoint;
            var imageRightBottomPoint = new Point2d(imageLeftBottomPoint.X + horizontalValue, imageLeftBottomPoint.Y);

            if (standardHorizontalValue > horizontalValue)
            {
                imageRightBottomPoint += (new Vector2d(1, 0) * (standardHorizontalValue - horizontalValue) * 0.5);
            }
            else if (standardVerticalValue > verticalValue)
            {
                imageRightBottomPoint -= (new Vector2d(0, 1) * (standardVerticalValue - verticalValue) * 0.5);
            }

            var yGapValue = Math.Abs(imageRightBottomPoint.Y - pptRightBottomPoint.Y);
            var xGapValue = Math.Abs(imageRightBottomPoint.X - pptRightBottomPoint.X);
            // 右下顶点POS
            var xRatio = xGapValue / pptWindowHorizontal;
            var yRatio = yGapValue / pptWindowVertical;

            var xRatioPosTopLeft = xRatio * ThPlotData.PPTWIDTH;
            var yRatioPosTopLeft = yRatio * ThPlotData.PPTHEIGHT;

            // picture的在PPT中的的宽值和高值
            pictureRotateWidth = standardHorizontalValue / pptWindowHorizontal * ThPlotData.PPTWIDTH;
            pictureRotateHeight = standardVerticalValue / pptWindowVertical * ThPlotData.PPTHEIGHT;
            var imageLenthAdd = pictureRotateWidth;
            var imageHeightAdd = pictureRotateHeight;
            xRatioPos = xRatioPosTopLeft;
            yRatioPos = yRatioPosTopLeft;
        }

        /// <summary>
        /// 计算Image的左上顶点的位置比例
        /// </summary>
        /// <param name="imageProfile"></param>
        /// <param name="pptProfile"></param>
        /// <param name="xRatio"></param>
        /// <param name="yRatio"></param>
        public static void CalculateAngle270(Polyline imageProfile, Polyline pptProfile, ref double xRatioPos, ref double yRatioPos, ref double pictureRotateWidth, ref double pictureRotateHeight)
        {
            var a4PaperRation = CalculatePaperSizeInfo(imageProfile);
            double horizontalValue = 0; // 水平值
            double verticalValue = 0; // 竖向值
            var imageWindow = CalculateProfileTwoEdge(imageProfile, ref horizontalValue, ref verticalValue);
            var standardVerticalValue = a4PaperRation * horizontalValue;
            var standardHorizontalValue = verticalValue / a4PaperRation;

            // 计算imageWndow的右上顶点 和ppt框的右上顶点
            var ptLst = GetPolylinePoints(pptProfile);
            var pptWindow = GetEntityPointsWindow(ptLst);
            var pptWindowVertical = pptWindow.RightTopPoint.Y - pptWindow.LeftBottomPoint.Y;
            var pptWindowHorizontal = pptWindow.RightTopPoint.X - pptWindow.LeftBottomPoint.X;
            var pptRightTopPoint = pptWindow.RightTopPoint;

            // 计算生成纸张后考虑留白的轮廓的新的右上顶点
            var imageRightTopPoint = imageWindow.RightTopPoint;
            if (standardHorizontalValue > horizontalValue)
            {
                imageRightTopPoint += (new Vector2d(1, 0) * (standardHorizontalValue - horizontalValue) * 0.5);
            }
            else if (standardVerticalValue > verticalValue)
            {
                imageRightTopPoint -= (new Vector2d(0, 1) * (standardVerticalValue - verticalValue) * 0.5);
            }

            var yGapValue = Math.Abs(imageRightTopPoint.Y - pptRightTopPoint.Y);
            var xGapValue = Math.Abs(imageRightTopPoint.X - pptRightTopPoint.X);
            // 左下顶点POS
            var xRatio = yGapValue / pptWindowVertical;
            var yRatio = xGapValue / pptWindowHorizontal;
            var xRatioPosTopLeft = xRatio * ThPlotData.PPTWIDTH;
            var yRatioPosTopLeft = yRatio * ThPlotData.PPTHEIGHT;

            // picture的在PPT中的的宽值和高值
            pictureRotateWidth = standardHorizontalValue / pptWindowHorizontal * ThPlotData.PPTHEIGHT;
            pictureRotateHeight = standardVerticalValue / pptWindowVertical * ThPlotData.PPTWIDTH;
            var imageLenthAdd = pictureRotateHeight * 0.5;
            var imageHeightAdd = pictureRotateWidth * 0.5;
            var centerPosX = xRatioPosTopLeft + imageLenthAdd;
            var centerPosY = yRatioPosTopLeft + imageHeightAdd;
            xRatioPos = centerPosX - imageHeightAdd;
            yRatioPos = centerPosY - imageLenthAdd;
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
            return (width < height) ? A2PAPERLANDSCAPERATIO : (1.00 / A2PAPERLANDSCAPERATIO);
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

        // 获取图层中多行文本的Id对象
        public static List<ObjectId> GetMTextIdFromLayer(string layerName)
        {
            var result = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<MText>().Where(p => p.Layer == layerName);
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
            var heightRatio = 2000.0 / 27000;
            var profileHeight = rightTopPos.Y - leftBottomPos.Y;
            dbText.Height = heightRatio * profileHeight;
            dbText.Justify = AttachmentPoint.BottomRight;
            // 设置字体样式
            var textId = GetIdFromSymbolTable();
            if (textId != ObjectId.Null)
                dbText.TextStyleId = textId;

            dbText.AlignmentPoint = new Point3d(rightBottomPos.X - 300, rightBottomPos.Y + 300, 0);
            return dbText;
        }

        /// <summary>
        /// 获取字体
        /// </summary>
        /// <returns></returns>
        public static ObjectId GetIdFromSymbolTable()
        {
            Database dbH = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = dbH.TransactionManager.StartTransaction())
            {
                TextStyleTable textTableStyle = (TextStyleTable)trans.GetObject(dbH.TextStyleTableId, OpenMode.ForWrite);

                if (textTableStyle.Has("黑体"))
                {
                    ObjectId idres = textTableStyle["黑体"];
                    if (!idres.IsErased)
                        return idres;
                }
            }

            return ObjectId.Null;
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
        /// 前者在后者图元的内部 返回true
        /// </summary>
        /// <param name="relatedData"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static bool EntityContainsEntity(Polyline pptPolyline, RelatedData relatedData)
        {
            var curImagePolyline = relatedData.ImagePolyline;
            var ptLst = GetPolylinePointLst(pptPolyline);

            foreach (var pt in ptLst)
            {
                if (!PointInnerEntity(curImagePolyline, pt))
                {
                    return false;
                }
            }

            relatedData.PptPolyline = pptPolyline;
            return true;
        }

        /// <summary>
        /// 计算打印顺序
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="srcRelatedDatas"></param>
        /// <returns></returns>
        public static List<RelatedData> ReCalculatePrintSequence(UserSelectData userData, List<RelatedData> srcRelatedDatas)
        {
            if (userData.SelectStyle == UserSelectData.SelectWay.SELECTUNKNOWN || userData.SelectStyle == UserSelectData.SelectWay.SINGLESELECT)
                return srcRelatedDatas;

            SelectDir dir = SelectDir.UP2DOWNLEFT2RIGHT;
            if (userData.SelectStyle == UserSelectData.SelectWay.RECTSELECTLEFTRIGHT)
                dir = SelectDir.LEFT2RIGHTUP2DOWN;


            var polylines = new List<Polyline>();
            foreach (var pptData in srcRelatedDatas)
            {
                polylines.Add(pptData.PptPolyline);
            }

            int pageCount = 0;
            // 匹配重置页码
            var pageWithProfiles = ThPlotData.CalculateProfileWithPages(polylines, dir, ref pageCount);
            foreach (var pptData in srcRelatedDatas)
            {
                for (int i = 0; i < pageWithProfiles.Count; i++)
                {
                    if (IsSamePolyline(pptData, pageWithProfiles[i]))
                    {
                        pptData.PageText.TextString = (i + 1).ToString();
                        break;
                    }
                }
            }

            // 删除原页码
            using (var db = AcadDatabase.Active())
            {
                db.ModelSpace
                  .OfType<DBText>()
                  .UpgradeOpen()
                  .ForEach(br =>
                  {
                      if (br.Layer == ThPlotData.PAGELAYER)
                          br.Erase();
                  });
            }

            // 插入页码
            ThPlotData.InsertPageWithProfiles(pageWithProfiles, ThPlotData.PAGELAYER);

            srcRelatedDatas.Sort((s1, s2) =>
            {
                var num1 = int.Parse(s1.PageText.TextString);
                var num2 = int.Parse(s2.PageText.TextString);
                return num1.CompareTo(num2);
            });

            srcRelatedDatas.Reverse();
            return srcRelatedDatas;
        }

        /// <summary>
        /// 计算多段线的最小高度
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public static double CalculateMinHeight(List<PageWithProfile> profiles)
        {
            if (profiles == null || profiles.Count == 0)
                return 0;

            var heights = new List<double>();
            double lineWidth = 800;
            double lineHeight = 800;
            profiles.ForEach(p =>
            {
                if (p.Profile != null)
                {
                    ThPlotData.CalculateProfileTwoEdge(p.Profile, ref lineWidth, ref lineHeight);
                    heights.Add(lineHeight);
                }

            });

            var minValue = heights.Min();
            return minValue;
        }

        /// <summary>
        /// 是否是同一个多段线
        /// </summary>
        /// <param name="relatedData"></param>
        /// <param name="pageWithProfile"></param>
        /// <returns></returns>
        public static bool IsSamePolyline(RelatedData relatedData, PageWithProfile pageWithProfile)
        {
            var polylineFir = relatedData.PptPolyline;
            var ptLstFir = GetPolylinePoints(polylineFir);
            var firWindowData = GetEntityPointsWindow(ptLstFir);
            var firLeftBottomPos = firWindowData.LeftBottomPoint;

            var polylineSec = pageWithProfile.Profile;
            var ptLstSec = GetPolylinePoints(polylineSec);
            var secWindowData = GetEntityPointsWindow(ptLstSec);
            var secLeftBottomPos = secWindowData.LeftBottomPoint;

            if (firLeftBottomPos.GetDistanceTo(secLeftBottomPos) < 100)
                return true;

            return false;
        }

        /// <summary>
        /// 删除已经选择的页码记录
        /// </summary>
        public static void ErasePageLayerEntity()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            var pageTextIdLst = GetTextIdFromLayer(ThPlotData.PAGELAYER);
            if (pageTextIdLst == null || pageTextIdLst.Count == 0)
                return;

            using (Transaction dataGetTrans = db.TransactionManager.StartTransaction())
            {
                foreach (var pageTextId in pageTextIdLst)
                {
                    DBText dbPageText = (DBText)dataGetTrans.GetObject(pageTextId, OpenMode.ForWrite);
                    //dbPageText.UpgradeOpen();
                    dbPageText.Erase();
                }

                dataGetTrans.Commit();
            }
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
            var pptMTextIdLst = GetMTextIdFromLayer(userData.PrintTextLayer);
            var pageTextIdLst = GetTextIdFromLayer(ThPlotData.PAGELAYER);
            var imagePolylineLst = new List<Polyline>();
            var pptPolylineLst = new List<Polyline>();
            var pptTextLst = new List<DBText>();
            var pptMTextLst = new List<MText>();
            var pageTextLst = new List<DBText>();
            var pptRelatedData = new List<RelatedData>();

            using (Transaction dataGetTrans = db.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var imageId in imageIdLst)
                {
                    Polyline imagePolyline = (Polyline)dataGetTrans.GetObject(imageId, OpenMode.ForRead);
                    if (imagePolyline.Closed)
                        imagePolylineLst.Add((Polyline)imagePolyline.Clone());
                }

                if (imagePolylineLst.Count == 0)
                    return null;

                imagePolylineLst.ForEach(p => pptRelatedData.Add(new RelatedData(p)));

                // PPT多段线
                foreach (var pptId in pptIdLst)
                {
                    Polyline pptPolyline = (Polyline)dataGetTrans.GetObject(pptId, OpenMode.ForRead);
                    if (pptPolyline.Closed)
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

                foreach (var textId in pptMTextIdLst)
                {
                    MText dbText = (MText)dataGetTrans.GetObject(textId, OpenMode.ForRead);
                    pptMTextLst.Add((MText)dbText.Clone());
                }

                if (pptTextLst.Count == 0 && pptMTextLst.Count == 0)
                    return null;

                // 页码文本
                foreach (var pageTextId in pageTextIdLst)
                {
                    DBText dbPageText = (DBText)dataGetTrans.GetObject(pageTextId, OpenMode.ForRead);
                    var pageStr = dbPageText.TextString;
                    int nPage = 0;
                    if (Int32.TryParse(pageStr, out nPage))
                        pageTextLst.Add((DBText)dbPageText.Clone());
                }

                if (pageTextLst.Count == 0)
                    return null;

                // polyline 线框文本信息等匹配
                foreach (var relatedData in pptRelatedData)
                {
                    var curImagePolyline = relatedData.ImagePolyline;
                    foreach (var pptPolyline in pptPolylineLst)
                    {
                        if (BoundaryIntersectWithBound(relatedData, pptPolyline) || EntityContainsEntity(relatedData, pptPolyline)
                            || EntityContainsEntity(pptPolyline, relatedData))
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


                    // ppt多行文本
                    foreach (var pptMtext in pptMTextLst)
                    {
                        if (PointInnerEntity(curPptPolyline, pptMtext.Location))
                            relatedData.PptMtextLst.Add(pptMtext);
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

                // 多行文本转化为单行文本
                Convert2DBTexts(pptRelatedData);
                // 页码从大到小排序
                var validRelatedData = pptRelatedData.Where(s1 => s1.PageText != null).ToList();
                validRelatedData.Sort((s1, s2) => { return Double.Parse(s1.PageText.TextString).CompareTo(Double.Parse(s2.PageText.TextString)); });
                validRelatedData.Reverse();
                return validRelatedData;
            }
        }

        /// <summary>
        /// 多行文本转化为单行文本
        /// </summary>
        /// <param name="pptRelatedDatas"></param>
        public static void ConvertMTexts2DBText(RelatedData relatedData)
        {
            foreach (var mtext in relatedData.PptMtextLst)
            {
                var dbText = new DBText();
                var contents = mtext.Contents;

                string[] texts = contents.Split('\\');
                string stext = string.Empty;
                foreach (var text in texts)
                {
                    stext += text;
                }
                dbText.TextString = stext;
                relatedData.PptTextLst.Add(dbText);
                dbText.Position = mtext.Location;
            }

            relatedData.PptMtextLst.Clear();
        }

        public static void ConvertMTexts2DBText90(RelatedData relatedData)
        {
            foreach (var mtext in relatedData.PptMtextLst)
            {
                var contents = mtext.Contents;

                var curTextPos = mtext.Location;
                string[] texts = contents.Split('\\');

                for (int i = 0; i < texts.Count(); i++)
                {
                    var text = texts[i];
                    var dbText = new DBText();
                    curTextPos += new Vector3d(mtext.Width * i, 0, 0);
                    dbText.Position = curTextPos;
                    dbText.TextString = text;
                    relatedData.PptTextLst.Add(dbText);
                }
            }

            relatedData.PptMtextLst.Clear();
        }

        public static void ConvertMTexts2DBText180(RelatedData relatedData)
        {
            foreach (var mtext in relatedData.PptMtextLst)
            {
                var contents = mtext.Contents;

                var curTextPos = mtext.Location;
                string[] texts = contents.Split('\\');

                for (int i = 0; i < texts.Count(); i++)
                {
                    var text = texts[i];
                    var dbText = new DBText();
                    curTextPos += new Vector3d(0, mtext.TextHeight * i, 0);
                    dbText.Position = curTextPos;
                    dbText.TextString = text;
                    relatedData.PptTextLst.Add(dbText);
                }
            }

            relatedData.PptMtextLst.Clear();
        }

        public static void ConvertMTexts2DBText270(RelatedData relatedData)
        {
            foreach (var mtext in relatedData.PptMtextLst)
            {
                var contents = mtext.Contents;

                var curTextPos = mtext.Location;
                string[] texts = contents.Split('\\');

                for (int i = 0; i < texts.Count(); i++)
                {
                    var text = texts[i];
                    var dbText = new DBText();
                    curTextPos += new Vector3d(-mtext.Width, 0, 0);
                    dbText.Position = curTextPos;
                    dbText.TextString = text;
                    relatedData.PptTextLst.Add(dbText);
                }
            }

            relatedData.PptMtextLst.Clear();
        }


        public static void ConvertMTexts2DBText0(RelatedData relatedData)
        {
            foreach (var mtext in relatedData.PptMtextLst)
            {
                var contents = mtext.Contents;

                var curTextPos = mtext.Location;
                string[] texts = contents.Split('\\');

                for (int i = 0; i < texts.Count(); i++)
                {
                    var text = texts[i];
                    var dbText = new DBText();
                    curTextPos += new Vector3d(0, -mtext.TextHeight * i, 0);
                    dbText.Position = curTextPos;
                    dbText.TextString = text;
                    relatedData.PptTextLst.Add(dbText);
                }
            }

            relatedData.PptMtextLst.Clear();
        }


        public static void Convert2DBTexts(List<RelatedData> pptRelatedDatas)
        {
            foreach (var relatedData in pptRelatedDatas)
            {
                if (relatedData.PptMtextLst != null && relatedData.PptMtextLst.Count != 0)
                {
                    var textAngle = relatedData.PptMtextLst.First().Rotation;
                    if (ThPlotUtil.DoubleEqualValue(textAngle, 0, 5) || ThPlotUtil.DoubleEqualValue(textAngle, 360, 5))
                    {
                        ConvertMTexts2DBText0(relatedData);
                    }
                    else if (ThPlotUtil.DoubleEqualValue(textAngle, 90, 5))
                    {
                        ConvertMTexts2DBText90(relatedData);
                    }
                    else if (ThPlotUtil.DoubleEqualValue(textAngle, 180, 5))
                    {
                        ConvertMTexts2DBText180(relatedData);
                    }
                    else if (ThPlotUtil.DoubleEqualValue(textAngle, 270, 0))
                    {
                        ConvertMTexts2DBText270(relatedData);
                    }
                    else
                    {
                        ConvertMTexts2DBText(relatedData);
                    }
                }
            }
        }

        /// <summary>
        /// 插入块并拆分块
        /// </summary>
        public static void insertTemplateFile()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                var editor = doc.Editor;
                PromptPointOptions ppoLeftBottom = new PromptPointOptions("选择示例插入点");
                ppoLeftBottom.AllowNone = false;
                PromptPointResult ppr = editor.GetPoint(ppoLeftBottom);

                if (ppr.Status != PromptStatus.OK)
                    return;

                Point3d insertPoint = ppr.Value;

                using (var db = AcadDatabase.Active())
                {
                    var filePath = Path.Combine(ThCADCommon.SupportPath(), "图层框线示例.dwg");
                    db.Database.ImportBlocksFromDwg(filePath);
                    var insertobjId = db.ModelSpace.ObjectId.InsertBlockReference("0", "图层框线示例", insertPoint, new Scale3d(1, 1, 1), 0);
                    using (Transaction dataGetTrans = db.Database.TransactionManager.StartTransaction())
                    {
                        BlockReference blockReference = (BlockReference)dataGetTrans.GetObject(insertobjId, OpenMode.ForWrite);
                        DBObjectCollection collection = new DBObjectCollection();
                        blockReference.Explode(collection);
                        blockReference.Erase(true);
                        BlockTableRecord btr = (BlockTableRecord)dataGetTrans.GetObject(db.Database.CurrentSpaceId, OpenMode.ForWrite);
                        foreach (var obj in collection)
                        {
                            if (obj is Entity)
                            {
                                btr.AppendEntity(obj as Entity);
                                dataGetTrans.AddNewlyCreatedDBObject(obj as Entity, true);
                            }
                        }

                        ThPlotDialog.bTemplateFileUse = true;
                        dataGetTrans.Commit();
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 显示页码
        /// </summary>
        /// <param name="dbCollection"></param>
        public static void ShowPageText(DBObjectCollection dbCollection, Polyline polyline, DBText srcText, ObjectId textStyleId)
        {
            var dbText = new DBText();
            Line line1 = null;
            Line line2 = null;
            IntegerCollection intCol = new IntegerCollection();

            dbText.TextString = srcText.TextString;
            double lineWidth = 0;
            double lineHeight = 0;
            var windowData = CalculateProfileTwoEdge(polyline, ref lineWidth, ref lineHeight);
            double centerX = 0;
            double centerY = 0;
            var pt1 = windowData.LeftBottomPoint;
            var pt3 = windowData.RightTopPoint;
            centerX = 0.5 * (pt1.X + pt3.X);
            centerY = 0.5 * (pt1.Y + pt3.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);
            line1 = new Line(new Point3d(pt1.X, pt1.Y, 0), new Point3d(pt3.X, pt3.Y, 0));
            line1.Color = Color.FromRgb(255, 0, 0);
            line2 = new Line(new Point3d(pt2.X, pt2.Y, 0), new Point3d(pt4.X, pt4.Y, 0));
            line2.Color = Color.FromRgb(255, 0, 0);
            var minLength = (lineWidth < lineHeight) ? lineWidth : lineHeight;

            dbText.Height = 0.5 * minLength;
            dbText.Color = Color.FromRgb(255, 0, 0);
            var textwidth = dbText.WidthFactor * dbText.Height * 0.5;
            dbText.Position = new Point3d(centerX - textwidth, centerY - 0.5 *dbText.Height, 0);
            if (textStyleId != ObjectId.Null)
                dbText.TextStyleId = textStyleId;
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                tm.AddTransient(dbText, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
                tm.AddTransient(line1, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
                tm.AddTransient(line2, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
            }

            dbCollection.Add(dbText);
            dbCollection.Add(line1);
            dbCollection.Add(line2);
        }
    }
}
