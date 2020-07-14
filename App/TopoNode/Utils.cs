using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;
using DotNetARX;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcHelper;
using System.IO;
using Dreambuild.AutoCAD;
using System.Text.RegularExpressions;
using TopoNode.Progress;

namespace TopoNode
{
    /// <summary>
    /// 文字信息记录插入点和房间名字
    /// </summary>
    public class RoomTextNode
    {
        public Point3d textPoint;
        public String textString;

        public RoomTextNode(Point3d srcPt, String srcTextString)
        {
            textPoint = srcPt;
            textString = srcTextString;
        }
    }


    public class ConnectedNode
    {
        public ConnectedNode(List<LineSegment2d> relatedCurvesNode)
        {
            m_curNode = relatedCurvesNode;
        }

        public bool m_bUse = false;
        public List<LineSegment2d> m_curNode = null;
        public List<ConnectedNode> m_relatedConnectNodes = new List<ConnectedNode>();
    }

    /// <summary>
    /// 连通区域的外包框
    /// </summary>
    public class ConnectedBoxBound
    {
        private List<List<LineSegment2d>> m_curvesLst;
        private List<List<LineSegment2d>> m_outBounds = new List<List<LineSegment2d>>();
        public List<List<LineSegment2d>> OutBounds
        {
            get { return m_outBounds; }
        }

        private List<ConnectedNode> m_ConnectedNodes = new List<ConnectedNode>();
        public ConnectedBoxBound(List<List<LineSegment2d>> curvesLst)
        {
            m_curvesLst = curvesLst;
        }

        /// <summary>
        /// 生成相邻区域外包轮廓
        /// </summary>
        /// <param name="curvesLst"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeRelatedBoundary(List<List<LineSegment2d>> curvesLst)
        {
            var boxBound = new ConnectedBoxBound(curvesLst);
            var relatedCurves = boxBound.Do();
            boxBound.CalculateBounds(relatedCurves);
            return boxBound.OutBounds;
        }

        public static List<List<LineSegment2d>> MakeRelatedCurves(List<List<LineSegment2d>> curvesLst)
        {
            var boxBound = new ConnectedBoxBound(curvesLst);
            return boxBound.Do();
        }

        public List<List<LineSegment2d>> Do()
        {
            for (int i = 0; i < m_curvesLst.Count; i++)
            {
                m_ConnectedNodes.Add(new ConnectedNode(m_curvesLst[i]));
            }

            for (int i = 0; i < m_ConnectedNodes.Count; i++)
            {
                var curProfile = m_ConnectedNodes[i];
                for (int j = i + 1; j < m_ConnectedNodes.Count; j++)
                {
                    var nextProfile = m_ConnectedNodes[j];
                    if (IsAreaConnectArea(curProfile.m_curNode, nextProfile.m_curNode))
                    {
                        curProfile.m_relatedConnectNodes.Add(nextProfile);
                        nextProfile.m_relatedConnectNodes.Add(curProfile);
                    }
                }
            }

            var connectedAreaLst = new List<List<LineSegment2d>>();
            // 相邻区域收集
            for (int j = 0; j < m_ConnectedNodes.Count; j++)
            {
                if (m_ConnectedNodes[j].m_bUse)
                    continue;

                if (m_ConnectedNodes[j].m_relatedConnectNodes.Count == 0)
                {
                    m_ConnectedNodes[j].m_bUse = true;
                    connectedAreaLst.Add(m_ConnectedNodes[j].m_curNode);
                }
                else
                {
                    var loops = SearchFromOneArea(m_ConnectedNodes[j]);
                    connectedAreaLst.Add(loops);
                }
            }

            return connectedAreaLst;
        }

        /// <summary>
        /// 计算相邻区域的外包框
        /// </summary>
        /// <param name="connectedAreaLst"></param>
        public void CalculateBounds(List<List<LineSegment2d>> connectedAreaLst)
        {
            foreach (var connectCurves in connectedAreaLst)
            {
                // 经过外扩的
                var boundLines = GetBoundFromLine2ds(connectCurves);
                if (boundLines != null && boundLines.Count != 0)
                {
                    m_outBounds.Add(boundLines);
                }
            }
        }

        /// <summary>
        ///  计算外包边界
        /// </summary>
        /// <param name="line2ds"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromLine2ds(List<LineSegment2d> line2ds)
        {
            if (line2ds == null || line2ds.Count == 0)
                return null;

            var boundLines = new List<LineSegment2d>();
            var ptLst = new List<XY>();
            foreach (var line in line2ds)
            {
                var pxHead = line.StartPoint;
                var pxEnd = line.EndPoint;
                var ptS = new XY(pxHead.X, pxHead.Y);
                var ptE = new XY(pxEnd.X, pxEnd.Y);
                ptLst.Add(ptS);
                ptLst.Add(ptE);
            }

            var leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
            var rightTop = new XY(ptLst[0].X, ptLst[0].Y);
            for (int i = 1; i < ptLst.Count; i++)
            {
                if (leftBottom.X > ptLst[i].X)
                    leftBottom.X = ptLst[i].X;
                if (leftBottom.Y > ptLst[i].Y)
                    leftBottom.Y = ptLst[i].Y;

                if (rightTop.X < ptLst[i].X)
                    rightTop.X = ptLst[i].X;
                if (rightTop.Y < ptLst[i].Y)
                    rightTop.Y = ptLst[i].Y;
            }

            leftBottom = leftBottom - new XY(10, 10);
            rightTop = rightTop + new XY(10, 10);
            var pt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var pt3 = new Point2d(rightTop.X, rightTop.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        // 从一块区域开始搜索
        private List<LineSegment2d> SearchFromOneArea(ConnectedNode Searchprofile)
        {
            var line2ds = new List<LineSegment2d>();
            var InOutNodes = new List<ConnectedNode>();
            InOutNodes.Add(Searchprofile);
            while (InOutNodes.Count != 0)
            {
                var curProfile = InOutNodes.First();
                line2ds.AddRange(curProfile.m_curNode);
                curProfile.m_bUse = true;
                InOutNodes.RemoveAt(0);
                var childConnectNodes = curProfile.m_relatedConnectNodes;
                foreach (var connectedNode in childConnectNodes)
                {
                    if (!connectedNode.m_bUse && !InOutNodes.Contains(connectedNode))
                        InOutNodes.Add(connectedNode);
                }
            }

            return line2ds;
        }

        /// <summary>
        /// 重叠
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        private bool IsOverlap(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var ptFirS = lineFir.StartPoint;
            var ptFirE = lineFir.EndPoint;
            var ptSecS = lineSec.StartPoint;
            var ptSecE = lineSec.EndPoint;
            if (CommonUtils.IsPointOnSegment(ptFirS, lineSec) || CommonUtils.IsPointOnSegment(ptFirE, lineSec)
               || CommonUtils.IsPointOnSegment(ptSecS, lineFir) || CommonUtils.IsPointOnSegment(ptSecE, lineFir))
                return true;
            return false;
        }

        // 区域之间有连通的
        private bool IsAreaConnectArea(List<LineSegment2d> curvesFir, List<LineSegment2d> curvesSec)
        {
            foreach (var curveFir in curvesFir)
            {
                foreach (var curveSec in curvesSec)
                {
                    var intersectPts = curveFir.IntersectWith(curveSec);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        // intersect
                        return true;
                    }
                    else
                    {
                        // not intersect
                        if (IsOverlap(curveFir, curveSec))
                            return true;
                    }
                }
            }

            return false;
        }
    }

    public static class XRecordExtend
    {
        /// <summary>
        /// 添加扩展记录
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">扩展记录名称</param>
        /// <param name="values">扩展记录的内容</param>
        /// <returns>返回添加的扩展记录的Id</returns>
        public static ObjectId AddXrecord(this BlockReference blockRef, string searchKey, TypedValueList values)
        {
            // 判断对象是否已经拥有扩展字典,若无扩展字典，则
            if (blockRef.ExtensionDictionary.IsNull)
            {
                blockRef.UpgradeOpen(); // 切换对象为写的状态
                blockRef.CreateExtensionDictionary();// 为对象创建扩展字典
                blockRef.DowngradeOpen(); // 为了安全，将对象切换成读的状态
            }

            // 打开对象的扩展字典
            DBDictionary dict = (DBDictionary)blockRef.ExtensionDictionary.GetObject(OpenMode.ForRead);
            // 如果扩展字典中已包含指定的扩展记录对象，则返回
            if (dict.Contains(searchKey))
                return ObjectId.Null;

            Xrecord xrec = new Xrecord();// 为对象新建一个扩展记录
            xrec.Data = values;// 指定扩展记录的内容
            dict.UpgradeOpen(); // 将扩展字典切换成写的状态
            //在扩展字典中加入新建的扩展记录，并指定它的搜索关键字
            ObjectId idXrec = dict.SetAt(searchKey, xrec);
            blockRef.Database.TransactionManager.AddNewlyCreatedDBObject(xrec, true);
            dict.DowngradeOpen(); // 为了安全，将扩展字典切换成读的状态            
            return idXrec; // 返回添加的扩展记录的的Id
        }

        /// <summary>
        /// 获取扩展记录
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">扩展记录名称</param>
        /// <returns>返回扩展记录的内容</returns>
        public static TypedValueList GetXrecord(this BlockReference blockRef, string searchKey)
        {
            //获取对象的扩展字典
            ObjectId dictId = blockRef.ExtensionDictionary;

            if (dictId.IsNull)
                return null;//若对象没有扩展字典，则返回

            DBDictionary dict = (DBDictionary)dictId.GetObject(OpenMode.ForRead);
            //在扩展字典中搜索指定关键字的扩展记录，如果没找到则返回
            if (!dict.Contains(searchKey))
                return null;

            // 获取扩展记录的Id
            ObjectId xrecordId = dict.GetAt(searchKey);
            //打开扩展记录并获取扩展记录的内容
            Xrecord xrecord = (Xrecord)xrecordId.GetObject(OpenMode.ForRead);
            return xrecord.Data; // 返回扩展记录的内容
        }
    }

    class Utils
    {
        public static int INDEX = 0;
        /// <summary>
        /// 获取所有curves
        /// </summary>
        /// <returns></returns>
        public static List<Curve> GetAllCurves()
        {
            List<Curve> curves = null;
            using (var db = AcadDatabase.Active())
            {
                // curve 读取
                curves = db.ModelSpace.OfType<Curve>().ToList<Curve>();
            }

            return curves;
        }

        public static List<Entity> GetAllTexts(string roomLayer)
        {
            List<DBText> dbTexts = null;
            using (var db = AcadDatabase.Active())
            {
                dbTexts = db.ModelSpace.OfType<DBText>().Where(s => s.Layer.Contains(roomLayer)).ToList();
            }

            List<MText> mTexts = null;
            using (var db = AcadDatabase.Active())
            {
                mTexts = db.ModelSpace.OfType<MText>().Where(s => s.Layer.Contains(roomLayer)).ToList();
            }

            var entityTexts = new List<Entity>();
            if (dbTexts != null && dbTexts.Count != 0)
                entityTexts.AddRange(dbTexts);

            if (mTexts != null && mTexts.Count != 0)
                entityTexts.AddRange(mTexts);

            return entityTexts;
        }

        public static bool InValidRoomName(string textString)
        {
            if (textString.Contains("强电")
                   || textString.Contains("弱电")
                   || textString.Contains("排烟")
                   || textString.Contains("空调水管")
                   || textString.Contains("加压")
                   || textString.Contains("新风")
                   || textString.Contains("排风")
                   || textString.Contains("冷媒")
                   || textString.Contains("给水")
                   || textString.Contains("排水")
                   || textString.Contains("水管")
                   || textString.Contains("楼梯")
                   || textString.Contains("外廊")
                   || textString.Contains("水池")
                   || textString.Contains("泳池")
                   || textString.Contains("电信")
                   || textString.Contains("IT")
                   || textString.Contains("数据")
                   || textString.Contains("通讯")
                   || textString.Contains("消控")
                   || textString.Contains("消防控制")
                   || textString.Contains("绿化")
                   || textString.Contains("投影")
                   || textString.Contains("花坛")
                   || textString.Contains("电梯")
                   || textString.Contains("电井")
                   || textString.Contains("阳台")
                   || textString.Contains("露台")
                   || textString.Contains("水井")
                   || textString.Contains("台阶")
                   || textString.Contains("挡烟")
                   || textString.Contains("垂壁")
                   || textString.Contains("管井")
                   || textString.Contains("食梯")
                   || textString.Contains("天井")
                   || textString.Contains("上空")
                   || textString.Contains("进风")
                   || textString.Contains("雨棚")
                   || textString.Contains("雨篷")
                   || textString.Contains("雨蓬")
                   || textString.Contains("凸窗")
                   || textString.Contains("烟井")
                   || textString.Contains("燃气井")
                   || textString.Contains("客梯")
                   || textString.Contains("风井")
                   || textString.Contains("煤气井")
                   || textString.Contains("变电站")
                   || textString.Contains("变电间")
                   || textString.Contains("变电房")
                   || textString.Contains("变电室")
                   || textString.Contains("配电站")
                   || textString.Contains("配电间")
                   || textString.Contains("配电房")
                   || textString.Contains("配电室")
                   || textString.Contains("屋面")
                   || textString.Contains("屋顶")
                   || textString.Contains("休息平台")
                   || textString.Contains("休息木平台")
                   || textString.Contains("不上人")
                   || textString.Contains("不上人平台")
                   || textString.Contains("不上人木平台")
                   || textString.Contains("PT站")
                   || textString.Contains("PT间")
                   || textString.Contains("PT房")
                   || textString.Contains("KT站")
                   || textString.Contains("KT间")
                   || textString.Contains("KT房")
                   || textString.Contains("空腔")
                   || textString.Contains("消防电梯")
                   || textString.Contains("无障碍电梯")
                   || textString.Contains("无障碍梯")
                   || textString.Contains("室外机")
                   || textString.Contains("室外机平台")
                   || textString.Contains("室外平台")
                   || textString.Contains("露天")
                   || textString.Contains("排气")
                   || textString.Contains("风室")
                   || textString.Contains("景观池")
                   || textString.Contains("水池")
                   || textString.Contains("池塘")
                   || textString.Contains("池子")
                   || textString.Contains("水池")
                   || textString.Contains("花坛")
                   || textString.Contains("花盆")
                   || textString.Contains("井道")
                   || textString.Contains("集水坑")
                   || textString.Contains("排泥坑")
                   || textString.Contains("起坡线")
                   || textString.Contains("变坡线")
                   || textString.Contains("观光梯")
                   || textString.Contains("观光电梯")
                   || textString.Contains("载客电梯")
                   || textString.Contains("上客电梯")
                   || textString.Contains("入口")
                   || textString.Contains("出口")
                   || textString.Contains("扩散")
                   || textString.Contains("滤毒")
                   || textString.Contains("除尘")
                   || textString.Contains("集气")
                   || textString.Contains("防化值班")
                   || textString.Contains("找坡")
                   || textString.Contains("坡度")
                   || textString.Contains("排烟井")
                   || textString.Contains("货梯")
                   || textString.Contains("载货电梯")
                   || textString.Contains("载货梯")
                   || textString.Contains("水表")
                   || textString.Contains("无线覆盖机房")
                   || textString.Contains("无线机房")
                   || textString.Contains("移动进线机房")
                   || textString.Contains("移动机房")
                   || textString.Contains("联通进线机房")
                   || textString.Contains("联通机房")
                   || textString.Contains("控制值班室")
                   || textString.Contains("控制室")
                   || textString.Contains("花池")
                   || textString.Contains("庭院")
                   || textString.Contains("电间")
                   || textString.Contains("空调机位")
                   || textString.Contains("空调外机")
                   || textString.Contains("室外")
                   || textString.Contains("户外")
                   || (textString.Contains("上") && textString.Contains("级"))
                   || (textString.Contains("下") && textString.Contains("级"))
                   || textString.Contains("井")
                   || textString.Contains("太阳能")
                   || textString.Contains("PV")
                   || textString.Contains("空地")
                   || textString.Contains("庭院"))
                return true;

            return false;
        }

        public static bool ValidRoomName(string textString)
        {
            if (textString.Contains("电梯厅")
                || textString.Contains("客梯厅")
                || textString.Contains("货梯厅")
                || textString.Contains("新风机房")
                || textString.Contains("排烟机房")
                || textString.Contains("排风机房")
                || textString.Contains("排烟排风机房")
                || textString.Contains("补风机房")
                || textString.Contains("排风排烟机房")
                || textString.Contains("加压送风机房")
                || textString.Contains("风机房")
                || textString.Contains("风机机房")
                || textString.Contains("加压机房")
                || textString.Contains("换热机房")
                || textString.Contains("水泵房")
                || textString.Contains("水泵间")
                || textString.Contains("水泵机房")
                || textString.Contains("排烟机房")
                || textString.Contains("储藏")
                || (textString.Contains("水") && textString.Contains("井")))
            {
                return true;
            }
            else
                return false;
        }

        public static List<Point3d> GetRoomPoints(string roomLayer)
        {
            var textEntitys = GetAllTexts(roomLayer);
            var pts = new List<Point3d>();
            for (int i = 0; i < textEntitys.Count; i++)
            {
                var text = textEntitys[i];
                string textString;
                if (text is DBText)
                {
                    textString = (text as DBText).TextString.Trim();
                }
                else
                {
                    var mText = text as MText;
                    textString = mText.Text.Trim();
                }

                if (textString.IsNullOrEmpty())
                    continue;

                if (ValidRoomName(textString))
                {
                    pts.Add(text.Bounds.Value.CenterPoint());
                    continue;
                }

                if (textString.Length == 2 && textString.Contains("平台"))
                {
                    continue;
                }

                if (InValidRoomName(textString))
                    continue;

                if (textString.Length < 2)
                    continue;

                if (IsValidString(textString, "-"))
                {
                    if (text.Bounds.HasValue)
                        pts.Add(text.Bounds.Value.CenterPoint());
                }
            }

            return pts;
        }

        public static List<RoomTextNode> GetRoomTextNodes(string roomLayer)
        {
            var textEntitys = GetAllTexts(roomLayer);

            // 房间文字信息
            var roomTextNodes = new List<RoomTextNode>();
            for (int i = 0; i < textEntitys.Count; i++)
            {
                var text = textEntitys[i];
                string textString;
                if (text is DBText)
                {
                    textString = (text as DBText).TextString.Trim();
                }
                else
                {
                    var mText = text as MText;
                    textString = mText.Text.Trim();
                }

                if (textString.IsNullOrEmpty())
                    continue;

                if (ValidRoomName(textString))
                {
                    if (text.Bounds.HasValue)
                    {
                        var roomPt = text.Bounds.Value.CenterPoint();
                        roomTextNodes.Add(new RoomTextNode(roomPt, textString));
                    }
                    continue;
                }

                if (textString.Length == 2 && textString.Contains("平台"))
                {
                    continue;
                }

                if (InValidRoomName(textString))
                    continue;

                if (textString.Length < 2)
                    continue;

                if (IsValidString(textString, "-"))
                {
                    if (text.Bounds.HasValue)
                    {
                        var roomPt = text.Bounds.Value.CenterPoint();
                        roomTextNodes.Add(new RoomTextNode(roomPt, textString));
                    }
                }
            }

            return roomTextNodes;
        }

        private static bool IsValidString(string source, string aimTag)
        {
            var dic = new Dictionary<string, int>();

            // 汉字位置关系
            Regex r = new Regex(@"[\u4e00-\u9fa5]+");
            var characters = r.Matches(source);

            var characterRelations = new List<Tuple<string, int>>();
            foreach (Match character in characters)
            {
                var index = character.Index;
                string value = character.Value;
                characterRelations.Add(new Tuple<string, int>(value, index));
            }

            if (characterRelations.Count == 0)
                return false;

            // “-”位置关系
            Regex r1 = new Regex(@"[-]");
            var symbols = r1.Matches(source);

            var symbolRelations = new List<int>();
            foreach (Match symbol in symbols)
            {
                var index = symbol.Index;
                var value = symbol.Value;
                symbolRelations.Add(index);
            }

            // 没有汉字包含“-” 不算
            if (symbolRelations.Count != 0 && characterRelations.Count == 0)
                return false;

            if (symbolRelations.Count > 2)
            {
                for (int i = 1; i < symbolRelations.Count; i++)
                {
                    var before = symbolRelations[i - 1];
                    var cur = symbolRelations[i];
                    if (cur - before > 1)
                        return false;
                }
            }

            return true;
        }

        public static Polyline Pts2Polyline(List<Point3d> points)
        {
            if (points.Count < 3)
                return null;
            var poly = new Polyline();
            poly.Closed = true;
            for (int i = 0; i < points.Count; i++)
            {
                var curPt = points[i];
                poly.AddVertexAt(i, new Point2d(curPt.X, curPt.Y), 0, 0, 0);
            }
            return poly;
        }

        /// <summary>
        /// 插入喷淋图块
        /// </summary>
        /// <param name="insertPts"></param>
        public static void InsertSprayBlock(List<Point3d> insertPts, SprayType type)
        {
            Utils.CreateLayer("W-FRPT-SPRL", Color.FromRgb(191, 255, 0));

            string propName = "上喷";
            if (type == SprayType.SPRAYDOWN)
                propName = "下喷";
            using (var db = AcadDatabase.Active())
            {
                var filePath = Path.Combine(ThCADCommon.SupportPath(), "SprayBlockUp.dwg");
                db.Database.ImportBlocksFromDwg(filePath);
                foreach (var insertPoint in insertPts)
                {
                    var blockId = db.ModelSpace.ObjectId.InsertBlockReference("W-FRPT-SPRL", "喷头", insertPoint, new Scale3d(1, 1, 1), 0);
                    var props = blockId.GetDynProperties();
                    foreach (DynamicBlockReferenceProperty prop in props)
                    {
                        // 如果动态属性的名称与输入的名称相同
                        prop.Value = propName;
                    }
                }
            }
        }

        /// <summary>
        /// 延长curve的长度
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="length"></param>
        public static Curve ExtendCurve(Curve curve, double length)
        {
            try
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var startPoint = line.StartPoint;
                    var endPoint = line.EndPoint;
                    var dir = line.GetFirstDerivative(endPoint).GetNormal();
                    var ptHead = startPoint - dir * length;
                    var ptTail = endPoint + dir * length;


                    var extendLine = new Line(ptHead, ptTail);
                    extendLine.Layer = curve.Layer;
                    return extendLine;
                }
                //else if (curve is Arc)
                //{
                //    var arc = curve as Arc;
                //    var radius = arc.Radius;
                //    var startParam = arc.StartParam;
                //    var endParam = arc.EndParam;
                //    var param = Math.Asin(length * 0.5 / radius) * 2;
                //    arc.Extend(startParam - param);
                //    arc.Extend(endParam + param);
                //}
            }
            catch
            {

            }
            return curve;
        }

        public static void ExtendCurveWithTransaction(Curve curve, double length)
        {
            try
            {
                using (var db = AcadDatabase.Active())
                {
                    if (curve is Line)
                    {
                        var line = curve as Line;
                        var startPoint = line.StartPoint;
                        var endPoint = line.EndPoint;
                        var dir = line.GetFirstDerivative(startPoint);
                        var ptHead = startPoint - dir.GetNormal() * length;
                        var ptTail = endPoint + dir.GetNormal() * length;

                        var openLine = db.Element<Line>(line.Id, true);
                        openLine.Extend(true, ptHead);
                        openLine.Extend(false, ptTail);
                        openLine.DowngradeOpen();
                    }
                    //else if (curve is Arc)
                    //{
                    //    var arc = curve as Arc;
                    //    var radius = arc.Radius;
                    //    var startParam = arc.StartParam;
                    //    var endParam = arc.EndParam;
                    //    var param = Math.Asin(length * 0.5 / radius) * 2;
                    //    arc.Extend(startParam - param);
                    //    arc.Extend(endParam + param);
                    //}
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 是否自相交
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static bool IsSelfIntersected(List<Point3d> pts)
        {
            if (pts.Count < 3)
                return false;

            var lines = new List<Line>();
            for (int i = 0; i < pts.Count; i++)
            {
                var curPt = pts[i];
                var nextPt = pts[(i + 1) % pts.Count];
                lines.Add(new Line(curPt, nextPt));
            }

            for (int j = 0; j < lines.Count; j++)
            {
                var curLine = lines[j];
                for (int k = 0; k < lines.Count; k++)
                {
                    if (j == k)
                        continue;

                    var nextLine = lines[k];
                    if (CurveIsIntersectCurve(curLine, nextLine, pts))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 两段curve是否相交
        /// </summary>
        /// <param name="curveFirst"></param>
        /// <param name="curveSec"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static bool CurveIsIntersectCurve(Curve curveFirst, Curve curveSec, List<Point3d> pts)
        {
            if (!CommonUtils.IntersectValid(curveFirst, curveSec))
                return false;

            var ptLst = new Point3dCollection();
            curveFirst.IntersectWith(curveSec, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
            if (ptLst.Count != 0)
            {
                foreach (Point3d pt in ptLst)
                {
                    if (pts.Contains(pt))
                        continue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 对房间面积框线标准化
        /// </summary>
        /// <param name="polylines"></param>
        /// <returns></returns>
        public static List<Polyline> NormalizePolylines(List<Polyline> polylines)
        {
            if (polylines == null || polylines.Count == 0)
                return null;

            var resPolylines = new List<Polyline>();

            foreach (var poly in polylines)
            {
                resPolylines.Add(NormalizePolyline(poly));
            }

            return resPolylines;
        }

        /// <summary>
        /// 删除无效的房间框线
        /// </summary>
        /// <param name="srcPolylines"></param>
        /// <param name="wallCurves"></param>
        /// <returns></returns>
        public static List<Polyline> EraseInvalidPolylines(List<Polyline> srcPolylines, List<Curve> wallCurves)
        {
            if (srcPolylines == null || srcPolylines.Count == 0)
                return null;

            var resPolys = new List<Polyline>();
            var ptMids = new List<Point3d>();
            foreach (var wallCurve in wallCurves)
            {
                var ptMid = wallCurve.GetPointAtParameter((wallCurve.StartParam + wallCurve.EndParam) * 0.5);
                ptMids.Add(ptMid);
            }

            foreach (var poly in srcPolylines)
            {
                if (PolylineIsValid(poly, ptMids))
                    resPolys.Add(poly);
            }

            return resPolys;
        }

        public static bool PolylineIsValid(Polyline poly, List<Point3d> ptLst)
        {
            var curves = TopoUtils.Polyline2Curves(poly);

            foreach (var pt in ptLst)
            {
                if (CommonUtils.PtOnCurves(pt, curves))
                    continue;

                if (CommonUtils.PtInLoop(poly, new Point2d(pt.X, pt.Y)))
                    return false;
            }

            return true;
        }

        public static Polyline NormalizePolyline(Polyline pol)
        {
            var srcPts = pol.Vertices();
            var colPts = new List<Point2d>();
            colPts.Add(new Point2d(srcPts[0].X, srcPts[0].Y));
            for (int i = 1; i < srcPts.Count; i++)
            {
                var curPt = srcPts[i];
                var lastPt = colPts.Last();
                var xGap = Math.Abs(curPt.X - lastPt.X);
                var yGap = Math.Abs(curPt.Y - lastPt.Y);
                var aimPt = Point2d.Origin;
                if (yGap < xGap)
                {
                    aimPt = new Point2d(curPt.X, lastPt.Y);
                }
                else
                {
                    aimPt = new Point2d(lastPt.X, curPt.Y);
                }

                if (i == srcPts.Count - 1)
                {
                    var firstPt = colPts.First();
                    var tx = Math.Abs(firstPt.X - aimPt.X);
                    var ty = Math.Abs(firstPt.Y - aimPt.Y);

                    // todo 细化具体场景
                    if (tx < ty)
                    {
                        aimPt = new Point2d(firstPt.X, aimPt.Y);
                    }
                    else
                    {
                        aimPt = new Point2d(aimPt.X, firstPt.Y);
                    }
                }

                colPts.Add(aimPt);
            }
            var nPoly = new Polyline();
            nPoly.Closed = true;

            for (int i = 0; i < colPts.Count; i++)
            {
                nPoly.AddVertexAt(i, colPts[i], 0, 0, 0);
            }
            return nPoly;
        }

        public static void CollectPLines(PromptSelectionResult result, List<Polyline> polylines)
        {
            using (var db = AcadDatabase.Active())
            {
                foreach (var objId in result.Value.GetObjectIds())
                {
                    var pLine = db.Element<Polyline>(objId);
                    var clonePoly = pLine.Clone() as Polyline;
                    clonePoly.Closed = true;
                    polylines.Add(clonePoly);
                }
            }
        }

        public static void CollectEntityPLine(PromptEntityResult result, List<ObjectId> hasIds, List<Polyline> polylines)
        {
            using (var db = AcadDatabase.Active())
            {
                var aimId = result.ObjectId;
                foreach (var id in hasIds)
                {
                    if (id.Equals(aimId))
                        return;
                }

                hasIds.Add(aimId);
                var pLine = db.Element<Polyline>(aimId);
                var clonePoly = pLine.Clone() as Polyline;
                clonePoly.Closed = true;
                polylines.Add(clonePoly);
            }
        }

        public static List<Polyline> GetEntitys(string message, Type allowedType, bool exactMatch = true)
        {
            var resPolylines = new List<Polyline>();
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var opt = new PromptEntityOptions(message)
            {
                AllowObjectOnLockedLayer = false,
            };

            opt.SetRejectMessage("允许选择类型 " + allowedType.Name);
            opt.AddAllowedClass(allowedType, exactMatch);
            List<ObjectId> hasIds = new List<ObjectId>();

            do
            {
                PromptEntityResult result = ed.GetEntity(opt);
                if (result.Status == PromptStatus.OK)
                    Utils.CollectEntityPLine(result, hasIds, resPolylines);
                else
                    break;
            } while (true);

            return resPolylines;
        }

        /// <summary>
        /// 延长指定长度
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="length"></param>
        public static List<Curve> ExtendCurves(List<Curve> curves, double length)
        {
            if (curves == null || curves.Count == 0)
                return null;
            var resCurves = new List<Curve>();
            foreach (var curve in curves)
            {
                var resCurve = ExtendCurve(curve, length);
                resCurves.Add(resCurve);
            }

            return resCurves;
        }

        public static void ExtendCurvesWithTransaction(List<Curve> curves, double length)
        {
            foreach (var curve in curves)
            {
                ExtendCurveWithTransaction(curve, length);
            }
        }
        /// <summary>
        /// 删除无效的，即很窄的轮廓
        /// </summary>
        /// <param name="beamLoops"></param>
        /// <param name="minSprayOffset"></param>
        /// <returns></returns>
        public static List<Curve> EraseInvalidLoops(List<Curve> beamLoops, double minSprayOffset)
        {
            // 梁轮廓内部数据提取
            double offset = 0;
            if (minSprayOffset > 310)
                offset = minSprayOffset;
            else
                offset = 310;
            var validLoops = new List<Curve>();
            for (int i = 0; i < beamLoops.Count; i++)
            {
                var poly = beamLoops[i] as Polyline;

                var lines = CommonUtils.Polyline2dLines(poly);
                if (CommonUtils.CalcuLoopArea(lines) < 0)
                    poly.ReverseCurve();

                var db = poly.GetOffsetCurves(-offset);
                for (int j = 0; j < db.Count; j++)
                {
                    validLoops.Add(db[j] as Polyline);
                }
            }

            return validLoops;
        }

        /// <summary>
        /// 墙的内部可布置梁轮廓
        /// </summary>
        /// <param name="allCurves"></param>
        /// <param name="roomPolyline"></param>
        /// <returns></returns>
        public static List<Point3d> MakeInnerProfiles(List<Curve> allCurves, Polyline roomPolyline, PlaceData userData)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            // 生成相关梁柱数据
            var roomCurves = TopoUtils.Polyline2Curves(roomPolyline);
            var curves = TopoUtils.TesslateCurve(allCurves);

            var offsetPolylines = new List<Curve>();
            var line2ds = CommonUtils.Polyline2dLines(roomPolyline);
            if (CommonUtils.CalcuLoopArea(line2ds) < 0)
            {
                using (var db = AcadDatabase.Active())
                {
                    roomPolyline.ReverseCurve();
                }
            }
            var offsetRoomPolylines = new List<Curve>();
            var dbRoom = roomPolyline.GetOffsetCurves(-userData.minWallGap);
            for (int i = 0; i < dbRoom.Count; i++)
                offsetRoomPolylines.Add(dbRoom[i] as Polyline);
            if (offsetRoomPolylines.Count == 0)
                return null;

            var offsetWallCurves = TopoUtils.Polyline2Curves(offsetRoomPolylines.First() as Polyline);
            var relatedCurves = GetValidBeams(curves, offsetWallCurves);
            // Utils.DrawProfile(offsetWallCurves, "offsetWallCurves");
            //Utils.DrawProfile(relatedCurves, "relatedCurves");
            if (relatedCurves == null || relatedCurves.Count == 0)
                return null;
            // 偏移数据
            // Utils.DrawProfile(relatedCurves, "relatedCurves");


            // 获取房间框线偏移后的数据
            if (offsetRoomPolylines.Count != 0 && relatedCurves.Count != 0)
            {
                relatedCurves.AddRange(offsetWallCurves);
            }
            //Utils.DrawProfile(relatedCurves, "relatedCurves");
            var beamLoops = TopoUtils.MakeSrcProfilesNoTes(relatedCurves);
            if (beamLoops == null || beamLoops.Count == 0)
                return null;
            //Utils.DrawProfile(beamLoops, "beamLoops");
            var validLoops = EraseInvalidLoops(beamLoops, userData.minBeamGap);

            if (validLoops == null || validLoops.Count == 0)
                return null;
            //Utils.DrawProfile(validLoops, "validLoops");
            // 偏移后的墙，和没有偏移后的梁组成的环进行计算
            var insertPtS = PlaceSpray.CalcuInsertPos(TopoUtils.Polyline2Curves(roomPolyline), validLoops, userData);

            return insertPtS;
        }

        /// <summary>
        /// // 偏移后的墙，和没有偏移后的梁组成的环进行计算
        /// </summary>
        public class PlaceSpray
        {
            private List<Curve> m_roomSrcCurves; // 房间的原始边界线
            private List<List<Curve>> m_beamLoops; // 梁轮廓
            private double m_minBeamGapOffset; // 喷淋距墙的最小位置

            private double m_maxWallOffset; // 喷淋距墙的最大偏移值
            private double m_maxWallOffsetLeft; // 喷淋距墙的左边最大偏移值
            private double m_maxWallOffsetRight; // 喷淋距墙的右边最大偏移值
            private double m_maxWallOffsetBottom; // 喷淋距墙的下边最大偏移值
            private double m_maxWallOffsetTop; // 喷淋距墙的上边最大偏移值
            private double m_minWallOffsetTop; // 喷淋距墙的上边最小偏移值
            public bool m_bEnd = false; // 程序是否结束标记

            private double m_minWallOffset; // 喷淋距墙的最小偏移值
            private double m_maxSprayGap; // 喷淋之间的最大间距
            private double m_minSprayGap; // 喷淋之间的最小间距
            private Line m_splitLeftEdge;
            private Line m_splitBottomEdge;

            private double m_offsetAdd = 43.52; // 水平或者垂直方向上面增加的向量偏移值

            // 原始房间包围盒的上下左右边
            private Line m_srcRoomRight;
            private Line m_srcRoomTop;
            private Line m_srcRoomLeft;
            private Line m_srcRoomBottom;

            private List<Line> m_hLines; // 水平切割线
            private List<Line> m_vLines; // 垂直切割线

            private List<Line> boundCurves; // 边界轮廓

            public bool IsLinesInvalid()
            {
                if (m_hLines == null || m_hLines.Count == 0)
                    return true;
                if (m_vLines == null || m_vLines.Count == 0)
                    return true;
                return false;
            }

            public PlaceSpray(List<Curve> srcRoomCurves, List<List<Curve>> beamLoops, PlaceData userData)
            {
                m_roomSrcCurves = srcRoomCurves;
                m_beamLoops = beamLoops;
                m_minBeamGapOffset = userData.minBeamGap;
                m_maxSprayGap = userData.maxSprayGap;

                m_minWallOffset = userData.minWallGap;
                //var value = (userData.maxWallGap + m_minWallOffset + m_minBeamGapOffset) * 0.5;
                //m_maxWallOffset = (int)((value / 100 + 1) * 100);
                m_maxWallOffset = userData.maxWallGap;

                m_minSprayGap = userData.minSprayGap;
            }

            /// <summary>
            /// 计算四周距墙的最大偏移值
            /// </summary>
            private void Calculate4MaxWallOffset()
            {
                // 下边距墙的最大偏移值
                m_maxWallOffsetBottom = CalculateBottomMaxWallOffset(m_srcRoomBottom.StartPoint.X);
                if (m_bEnd)
                    return;

                // 上边距墙的最大偏移值
                m_maxWallOffsetTop = CalculateTopMaxWallOffset(m_srcRoomTop.StartPoint.X);
                m_minWallOffsetTop = CalculateTopMinWallOffset(m_srcRoomTop.StartPoint.X);
                if (m_bEnd)
                    return;

                // 左边距墙的最大偏移值
                m_maxWallOffsetLeft = CalculateLeftMaxWallOffset(m_srcRoomLeft.StartPoint.Y);
                if (m_bEnd)
                    return;

                // 右边距墙的最大偏移值
                m_maxWallOffsetRight = CalculateRightMaxWallOffset(m_srcRoomRight.StartPoint.Y);
            }

            private double CalculateRightMaxWallOffset(double yValue)
            {
                var srcRoomRightX = m_srcRoomRight.StartPoint.X;
                var rightMaxX = srcRoomRightX - m_minWallOffset;
                var rightMinX = srcRoomRightX - m_maxWallOffset;
                var yGap = m_srcRoomTop.StartPoint.Y - m_srcRoomBottom.StartPoint.Y;

                var yS = yValue - yGap;
                var yE = yValue + yGap;
                var xValue = rightMinX;

                while (true)
                {
                    if (xValue > rightMaxX)
                    {
                        m_bEnd = true;
                        return m_maxWallOffset;
                    }

                    var ptS = new Point3d(xValue, yS, 0);
                    var ptE = new Point3d(xValue, yE, 0);

                    Line rightLine = new Line(ptS, ptE);
                    if (LineWithLoops(rightLine, m_beamLoops))
                    {
                        var offset = Math.Abs(xValue - srcRoomRightX);
                        return ((int)offset);
                    }
                    else
                    {
                        xValue += 50;
                    }
                }
            }

            private double CalculateLeftMaxWallOffset(double yValue)
            {
                var srcRoomLeftX = m_srcRoomLeft.StartPoint.X;
                var leftMaxX = srcRoomLeftX + m_maxWallOffset;
                var leftMinX = srcRoomLeftX + m_minWallOffset;

                var yGap = m_srcRoomTop.StartPoint.Y - m_srcRoomBottom.StartPoint.Y;
                var yS = yValue - yGap;
                var yE = yValue + yGap;
                var xValue = leftMaxX;

                while (true)
                {
                    if (xValue < leftMinX)
                    {
                        m_bEnd = true;
                        return m_maxWallOffset;
                    }

                    var ptS = new Point3d(xValue, yS, 0);
                    var ptE = new Point3d(xValue, yE, 0);

                    Line leftLine = new Line(ptS, ptE);
                    if (LineWithLoops(leftLine, m_beamLoops))
                    {
                        var offset = Math.Abs(xValue - srcRoomLeftX);
                        return ((int)offset);
                    }
                    else
                    {
                        xValue -= 50;
                    }
                }
            }

            private double CalculateTopMinWallOffset(double xValue)
            {
                var srcRoomTopY = m_srcRoomTop.StartPoint.Y;
                var topMinY = srcRoomTopY - m_maxWallOffset;
                var topMaxY = srcRoomTopY - m_minWallOffset;

                var xGap = m_srcRoomRight.StartPoint.X - m_srcRoomLeft.StartPoint.X;
                var xS = xValue - xGap;
                var xE = xValue + xGap;
                var yValue = topMaxY;

                while (true)
                {
                    if (yValue < topMinY)
                    {
                        m_bEnd = true;
                        return m_maxWallOffset;
                    }

                    var ptS = new Point3d(xS, yValue, 0);
                    var ptE = new Point3d(xE, yValue, 0);

                    Line topLine = new Line(ptS, ptE);
                    if (LineWithLoops(topLine, m_beamLoops))
                    {
                        //foreach (var loop in m_beamLoops)
                        //{
                        //    var curves = new List<Curve>();
                        //    curves.AddRange(loop);
                        //    curves.Add(topLine);
                        //    DrawProfile(curves, "beam");
                        //}
                        var offset = Math.Abs(yValue - srcRoomTopY);
                        return ((int)offset);
                    }
                    else
                    {
                        yValue -= 50;
                    }
                }
            }

            private double CalculateTopMaxWallOffset(double xValue)
            {
                var srcRoomTopY = m_srcRoomTop.StartPoint.Y;
                var topMinY = srcRoomTopY - m_maxWallOffset;
                var topMaxY = srcRoomTopY - m_minWallOffset;

                var xGap = m_srcRoomRight.StartPoint.X - m_srcRoomLeft.StartPoint.X;
                var xS = xValue - xGap;
                var xE = xValue + xGap;
                var yValue = topMinY;

                while (true)
                {
                    if (yValue > topMaxY)
                    {
                        m_bEnd = true;
                        return m_maxWallOffset;
                    }

                    var ptS = new Point3d(xS, yValue, 0);
                    var ptE = new Point3d(xE, yValue, 0);

                    Line topLine = new Line(ptS, ptE);
                    if (LineWithLoops(topLine, m_beamLoops))
                    {
                        //foreach (var loop in m_beamLoops)
                        //{
                        //    var curves = new List<Curve>();
                        //    curves.AddRange(loop);
                        //    curves.Add(topLine);
                        //    DrawProfile(curves, "beam");
                        //}
                        var offset = Math.Abs(yValue - srcRoomTopY);
                        return ((int)offset);
                    }
                    else
                    {
                        yValue += 50;
                    }
                }
            }

            // 底边最大y值
            private double CalculateBottomMaxWallOffset(double xValue)
            {
                var srcRoomBotttomY = m_srcRoomBottom.StartPoint.Y;
                var bottomMaxY = srcRoomBotttomY + m_maxWallOffset;
                var bottomMinY = srcRoomBotttomY + m_minWallOffset;

                var xGap = m_srcRoomRight.StartPoint.X - m_srcRoomLeft.StartPoint.X;
                var xS = xValue - xGap;
                var xE = xValue + xGap;
                var yValue = bottomMaxY;

                while (true)
                {
                    if (yValue < bottomMinY)
                    {
                        m_bEnd = true;
                        return m_maxWallOffset;
                    }

                    var ptS = new Point3d(xS, yValue, 0);
                    var ptE = new Point3d(xE, yValue, 0);

                    Line bottomLine = new Line(ptS, ptE);
                    if (LineWithLoops(bottomLine, m_beamLoops))
                    {
                        var offset = Math.Abs(yValue - srcRoomBotttomY);
                        return ((int)offset);
                    }
                    else
                    {
                        yValue -= 50;
                    }
                }
            }

            /// <summary>
            /// 计算喷淋插入点位置
            /// </summary>
            /// <param name="srcRoomCurves"></param>
            /// <param name="loops"></param>
            /// <param name="userData"></param>
            /// <returns></returns>
            public static List<Point3d> CalcuInsertPos(List<Curve> srcRoomCurves, List<Curve> loops, PlaceData userData)
            {
                var beamLoops = new List<List<Curve>>();
                foreach (var loop in loops)
                {
                    if (loop is Polyline)
                        beamLoops.Add(TopoUtils.Polyline2Curves(loop as Polyline));
                }

                var placeSpray = new PlaceSpray(srcRoomCurves, beamLoops, userData);
                placeSpray.CalStartEdges(); // 开始边
                placeSpray.Calculate4MaxWallOffset(); // 计算四周最大墙的偏移值
                if (placeSpray.m_bEnd)
                    return null;
                placeSpray.CalcuVHLines(); // 水平，垂直线，并考虑是否平移至最下边和最左边
                if (placeSpray.IsLinesInvalid())
                    return null;
                placeSpray.CutFromMaxSprayOffset(); // 两边截断处理

                var ptsLst = placeSpray.DoPlace(); // 放置插入点计算
                return ptsLst;
            }

            private bool LineWithLoops(Line line, List<List<Curve>> loops)
            {
                foreach (var curLoop in loops)
                {
                    var pts = LineWithLoop(line, curLoop);
                    if (pts.Count > 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 计算开始边
            /// </summary>
            private void CalStartEdges()
            {
                // 原始房间的外接矩形框
                var srcRoomEdgeCal = new EdgeCalcu(m_roomSrcCurves);
                srcRoomEdgeCal.Do();

                // 房间外包框曲线
                boundCurves = srcRoomEdgeCal.BoundCurves;

                // 原始分界边的右边和上边
                m_srcRoomRight = srcRoomEdgeCal.SrcRightEdge;
                m_srcRoomTop = srcRoomEdgeCal.SrcTopEdge;

                m_srcRoomLeft = srcRoomEdgeCal.SrcLeftEdge;
                m_srcRoomBottom = srcRoomEdgeCal.SrcBottomEdge;

                // 所有梁轮廓的边界值计算
                var beamCurves = new List<Curve>();
                foreach (var curves in m_beamLoops)
                {
                    beamCurves.AddRange(curves);
                }
                var beamLoopEdgeCal = new EdgeCalcu(beamCurves);
                beamLoopEdgeCal.Do();
                var srcLeftEdge = beamLoopEdgeCal.SrcLeftEdge;
                var vecHori = new Vector3d(m_offsetAdd, 0, 0);
                var leftPtS = srcLeftEdge.StartPoint;
                var leftPtE = srcLeftEdge.EndPoint;
                m_splitLeftEdge = new Line(leftPtS + vecHori, leftPtE + vecHori);

                var srcBottomEdge = beamLoopEdgeCal.SrcBottomEdge;
                var vecVer = new Vector3d(0, m_offsetAdd, 0);
                var bottomPtS = srcBottomEdge.StartPoint;
                var bottomPtE = srcBottomEdge.EndPoint;
                m_splitBottomEdge = new Line(bottomPtS + vecVer, bottomPtE + vecVer);

                //var lineCurves = new List<Curve>();
                //lineCurves.Add(m_splitBottomEdge);
                //Utils.DrawProfile(lineCurves, "lineCurve");
            }


            private List<Point3d> LineWithLoop(Curve InterCurve, List<Curve> curLoop)
            {
                var ptLst = new List<Point3d>();
                var InterCurveS = InterCurve.StartPoint;
                var InterCurveE = InterCurve.EndPoint;
                for (int j = 0; j < curLoop.Count; j++)
                {
                    var curve = curLoop[j];

                    if (!CommonUtils.IntersectValid(InterCurve, curve))
                        continue;

                    var curveS = curve.StartPoint;
                    var curveE = curve.EndPoint;

                    var tmpPtLst = new Point3dCollection();
                    InterCurve.IntersectWith(curve, Autodesk.AutoCAD.DatabaseServices.Intersect.OnBothOperands, tmpPtLst, new System.IntPtr(0), new System.IntPtr(0));
                    if (tmpPtLst.Count != 0 && tmpPtLst.Count < 3)
                    {
                        foreach (Point3d pt in tmpPtLst)
                        {
                            bool bInLst = false;
                            for (int m = 0; m < ptLst.Count; m++)
                            {
                                var curPt = ptLst[m];
                                if (CommonUtils.Point3dIsEqualPoint3d(pt, curPt))
                                {
                                    bInLst = true;
                                    break;
                                }
                            }

                            if (!bInLst)
                                ptLst.Add(pt);
                        }
                    }
                }

                return ptLst;
            }
            /// <summary>
            /// 计算水平可插入的交线
            /// </summary>
            /// <param name="line"></param>
            /// <param name="loops"></param>
            /// <returns></returns>
            private List<Line> CalculateHoriLinesWithLoops(Line line, List<List<Curve>> loops)
            {
                var ptLst = new List<Point3d>();
                var lines = new List<Line>();

                //foreach (var drawCurves in loops)
                //{
                //    Utils.DrawProfile(drawCurves, "drawCurves");
                //}

                //var lineCurves = new List<Curve>();
                //lineCurves.Add(line);
                //Utils.DrawProfile(lineCurves, "lineCurves");
                //return null;
                for (int i = 0; i < loops.Count; i++)
                {
                    ptLst.Clear();
                    var curLoop = loops[i];
                    ptLst = LineWithLoop(line, curLoop);

                    // 跟每个环的交线加入到水平线中去
                    if (ptLst.Count > 1)
                    {
                        // 尖点处理
                        ptLst = ptLst.OrderBy(p => p.X).ToList();

                        for (int index = 0; index < ptLst.Count - 1; index++)
                        {
                            var curPt = ptLst[index];
                            var nextPt = ptLst[index + 1];
                            var midPt = new Point3d((curPt.X + nextPt.X) * 0.5, (curPt.Y + nextPt.Y) * 0.5, 0);

                            if (CommonUtils.PtInLoop(curLoop, midPt))
                                lines.Add(new Line(curPt, nextPt));
                        }
                    }
                }

                lines = lines.OrderBy(p => p.StartPoint.X).ToList();
                return lines;
            }

            /// <summary>
            /// 计算垂直方向的线段集
            /// </summary>
            /// <param name="line"></param>
            /// <param name="loops"></param>
            /// <returns></returns>
            private List<Line> CalculateVerLinesWithLoops(Line line, List<List<Curve>> loops)
            {
                var ptLst = new List<Point3d>();
                var lines = new List<Line>();

                for (int i = 0; i < loops.Count; i++)
                {
                    ptLst.Clear();
                    var curLoop = loops[i];
                    ptLst = LineWithLoop(line, curLoop);

                    // 跟每个环的交线加入到垂直线中去
                    if (ptLst.Count > 1)
                    {
                        // 尖点处理
                        ptLst = ptLst.OrderBy(p => p.Y).ToList();

                        for (int index = 0; index < ptLst.Count - 1; index++)
                        {
                            var curPt = ptLst[index];
                            var nextPt = ptLst[index + 1];
                            var midPt = new Point3d((curPt.X + nextPt.X) * 0.5, (curPt.Y + nextPt.Y) * 0.5, 0);

                            if (CommonUtils.PtInLoop(curLoop, midPt))
                                lines.Add(new Line(curPt, nextPt));
                        }
                    }
                }

                lines = lines.OrderBy(p => p.StartPoint.Y).ToList();
                return lines;
            }

            private void CalcuVHLines()
            {
                double YValue = m_maxSprayGap;
                // 计算水平分割边

                //var drawCurves = new List<Curve>();
                //foreach (var curves in m_beamLoops)
                //{
                //    drawCurves.AddRange(curves);
                //}
                ////Utils.DrawProfile(drawCurves, "horizon");
                m_hLines = CalculateHoriLinesWithLoops(m_splitBottomEdge, m_beamLoops);
                //if (m_hLines == null)
                //    return;
                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //return;
                // 计算垂直分割边
                Line vSplitLine = null;
                var yExtendValue = m_srcRoomTop.StartPoint.Y - m_srcRoomBottom.StartPoint.Y;

                // 
                for (int i = 0; i < m_hLines.Count; i++)
                {
                    var ptS = m_hLines[i].StartPoint;
                    var ptE = m_hLines[i].EndPoint;
                    var vecAdd = (ptE - ptS).MultiplyBy(0.25);

                    var ptAim = ptS + vecAdd;
                    var vec = new Vector3d(0, yExtendValue, 0);
                    var vLine = new Line(ptAim - vec, ptAim + vec);

                    var roomPtLst = LineWithLoop(vLine, m_roomSrcCurves);
                    roomPtLst = roomPtLst.OrderBy(pt => (pt.Y)).ToList();

                    var length = (roomPtLst.First() - roomPtLst.Last()).Length;

                    // 没有大的空洞出现
                    if (CommonUtils.IsAlmostNearZero(Math.Abs(length - m_srcRoomLeft.Length), 600))
                    {
                        vSplitLine = vLine;
                        break;
                    }
                }

                if (vSplitLine == null)
                    return;

                if (vSplitLine != null)
                {
                    var hVec = new Vector3d(m_offsetAdd, 0, 0);
                    var vSplitS = vSplitLine.StartPoint;
                    var vSplitE = vSplitLine.EndPoint;
                    var vOffLine = new Line(vSplitS + hVec, vSplitE + hVec);
                    //Utils.DrawProfile(new List<Curve> { vOffLine }, "voff");
                    m_vLines = CalculateVerLinesWithLoops(vOffLine, m_beamLoops);
                }
                // 判断水平边可能是否为缺少边
                var xExtendValue = m_srcRoomRight.StartPoint.X - m_srcRoomLeft.StartPoint.X;
                var splitHoriS = m_splitBottomEdge.StartPoint;
                var splitHoriE = m_splitBottomEdge.EndPoint;
                var vecHori = new Vector3d(xExtendValue, 0, 0);
                var splitHoriLine = new Line(splitHoriS - vecHori, splitHoriE + vecHori);
                var horiPtLst = LineWithLoop(splitHoriLine, m_roomSrcCurves);
                Line hSplitLine = null;
                if (horiPtLst.Count == 2)
                {
                    var length = (horiPtLst.First() - horiPtLst.Last()).Length;
                    if (!CommonUtils.IsAlmostNearZero(Math.Abs(length - m_srcRoomBottom.Length), 600))
                    {
                        // 左下角有空洞的情形
                        for (int j = 0; j < m_vLines.Count; j++)
                        {
                            var ptS = m_vLines[j].StartPoint;
                            var vec = new Vector3d(xExtendValue, 0, 0);
                            var horiLine = new Line(ptS - vec, ptS + vec);
                            var hRoomPtLst = LineWithLoop(horiLine, m_roomSrcCurves);
                            if (hRoomPtLst.Count == 2)
                            {
                                var hLength = (hRoomPtLst.First() - hRoomPtLst.Last()).Length;
                                if (CommonUtils.IsAlmostNearZero(Math.Abs(hLength - m_srcRoomBottom.Length), 600))
                                {
                                    hSplitLine = horiLine;
                                    m_hLines = CalculateHoriLinesWithLoops(hSplitLine, m_beamLoops);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (m_hLines.Count == 0 || m_vLines.Count == 0)
                    return;
                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
                //return;
                // 处理至最左边和最下边的位置
                MoveVHLines();
            }

            /// <summary>
            /// 处理至最左边和最下边的位置
            /// </summary>
            private void MoveVHLines()
            {
                var tmpHLines = new List<Line>();
                var curHY = m_hLines.First().StartPoint.Y;
                // 不等 则移动到下面
                if (!CommonUtils.IsAlmostNearZero(Math.Abs(curHY - m_srcRoomBottom.StartPoint.Y - m_minBeamGapOffset - m_offsetAdd), 600))
                {
                    var vecV = new Vector3d(0, curHY - m_minBeamGapOffset - m_offsetAdd, 0);
                    foreach (var line in m_hLines)
                    {
                        var hPtS = line.StartPoint;
                        var hPtE = line.EndPoint;
                        tmpHLines.Add(new Line(hPtS - vecV, hPtE - vecV));
                    }
                    m_hLines = tmpHLines;
                }

                // 不等 则移动到左边
                var tmpVLines = new List<Line>();
                var curVX = m_vLines.First().StartPoint.X;
                if (!CommonUtils.IsAlmostNearZero(Math.Abs(curVX - m_srcRoomLeft.StartPoint.X - m_minBeamGapOffset - m_offsetAdd), 600))
                {
                    var vecH = new Vector3d(curVX - m_minBeamGapOffset - m_offsetAdd, 0, 0);
                    foreach (var line in m_vLines)
                    {
                        var vPtS = line.StartPoint;
                        var vPtE = line.EndPoint;
                        tmpVLines.Add(new Line(vPtS - vecH, vPtE - vecH));
                    }

                    m_vLines = tmpVLines;
                }

                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
            }

            /// <summary>
            /// 计算水平裁剪
            /// </summary>
            private void CutFromMaxSprayOffsetHLines()
            {
                var leftMaxX = m_srcRoomLeft.StartPoint.X + m_maxWallOffsetLeft;
                var rightMaxX = m_srcRoomRight.StartPoint.X - m_maxWallOffsetRight;

                var tmpHLines = new List<Line>();
                if ((m_srcRoomRight.StartPoint.X - m_srcRoomLeft.StartPoint.X) < (m_maxWallOffsetLeft + m_maxWallOffsetRight))
                {
                    var firstLine = m_hLines.First();
                    var lastLine = m_hLines.Last();
                    var ptFir = firstLine.StartPoint;
                    var ptEnd = lastLine.EndPoint;
                    var midPt = new Point3d((ptFir.X + ptEnd.X) * 0.5, ptFir.Y, 0);
                    var sVLine = new Line(midPt, midPt + new Vector3d(1, 0, 0) * 2);
                    tmpHLines.Add(sVLine);
                    m_hLines.Clear();
                    m_hLines = tmpHLines;
                    return;
                }

                for (int i = 0; i < m_hLines.Count; i++)
                {
                    // 当前
                    var curHLine = m_hLines[i];
                    var curPtS = curHLine.StartPoint;
                    var curPtE = curHLine.EndPoint;
                    if (m_hLines.Count == 1)
                    {
                        // 四种情况处理
                        if (curPtS.X > leftMaxX && curPtE.X < rightMaxX)
                        {
                            var nPtS = new Point3d(leftMaxX, curPtS.Y, 0);
                            var nPtE = curPtE;
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.X > leftMaxX && curPtE.X > rightMaxX)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(rightMaxX, curPtE.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.X < leftMaxX && curPtE.X > rightMaxX)
                        {
                            var nPtS = new Point3d(leftMaxX, curPtS.Y, 0);
                            var nPtE = new Point3d(rightMaxX, curPtE.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else
                        {
                            tmpHLines.Add(curHLine);
                        }
                    }
                    else if (i < m_hLines.Count - 1)
                    {
                        // 至倒数第二段的情形
                        // 下一条
                        var nextHLine = m_hLines[i + 1];
                        var nextPtS = nextHLine.StartPoint;
                        var nextPtE = nextHLine.EndPoint;
                        if (curPtS.X < leftMaxX && curPtE.X > leftMaxX)
                        {
                            // 当前与左边相交 收集
                            var nPtS = new Point3d(leftMaxX, curPtS.Y, 0);
                            tmpHLines.Add(new Line(nPtS, curPtE));
                        }
                        else if (curPtE.X < leftMaxX && nextPtS.X > leftMaxX)
                        {
                            // 当前是被包含的最后一个线段 收集
                            var nPtS = curPtE;
                            var nPtE = curPtE + new Vector3d(10, 0, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtS.X > leftMaxX && curPtE.X < rightMaxX)
                        {
                            // 当前段在左右两条边的中间 收集
                            tmpHLines.Add(curHLine);
                        }
                        if (curPtS.X < rightMaxX && curPtE.X > rightMaxX)
                        {
                            // 当前段与尾段相交 收集
                            var nPtS = curPtS;
                            var nPtE = new Point3d(rightMaxX, curPtS.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtE.X < rightMaxX && nextPtS.X > rightMaxX)
                        {
                            // 收集next段
                            var nPtS = nextPtS;
                            var nPtE = nextPtS + new Vector3d(2, 0, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE)); // 最后一段检查长度，小于10的只要放置一个点
                        }
                    }
                    else if (i == m_hLines.Count - 1)
                    {
                        // 最后一段的情形
                        if (curPtS.X < rightMaxX && curPtE.X > rightMaxX)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(rightMaxX, curPtE.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtE.X < rightMaxX)
                        {
                            tmpHLines.Add(curHLine);
                        }
                    }
                }

                m_hLines.Clear();
                m_hLines = tmpHLines;
            }

            /// <summary>
            /// 计算垂直裁剪
            /// </summary>
            private void CutFromMaxSprayOffsetVLines()
            {
                var bottomMaxY = m_srcRoomBottom.StartPoint.Y + m_maxWallOffsetBottom;
                var topMaxY = m_srcRoomTop.StartPoint.Y - m_maxWallOffsetTop;
                var tmpVLines = new List<Line>();

                if ((m_srcRoomTop.StartPoint.Y - m_srcRoomBottom.StartPoint.Y) < (m_maxWallOffsetBottom + m_maxWallOffsetTop))
                {
                    var firstLine = m_vLines.First();
                    var lastLine = m_vLines.Last();
                    var ptFir = firstLine.StartPoint;
                    var ptEnd = lastLine.EndPoint;
                    var midPt = new Point3d(ptFir.X, (int)((ptFir.Y + ptEnd.Y) * 0.5), 0);
                    var sVLine = new Line(midPt, midPt + new Vector3d(0, 1, 0) * 2);
                    tmpVLines.Add(sVLine);
                    m_vLines.Clear();
                    m_vLines = tmpVLines;
                    return;
                }


                for (int i = 0; i < m_vLines.Count; i++)
                {
                    // 当前
                    var curVLine = m_vLines[i];
                    var curPtS = curVLine.StartPoint;
                    var curPtE = curVLine.EndPoint;
                    if (m_vLines.Count == 1)
                    {
                        // 四种情况处理
                        if (curPtS.Y > bottomMaxY && curPtE.Y < topMaxY)
                        {
                            var nPtS = new Point3d(curPtS.X, bottomMaxY, 0);
                            var nPtE = curPtE;
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.Y > bottomMaxY && curPtE.Y > topMaxY)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(curPtE.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.Y < bottomMaxY && curPtE.Y > topMaxY)
                        {
                            var nPtS = new Point3d(curPtS.X, bottomMaxY, 0);
                            var nPtE = new Point3d(curPtE.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else
                        {
                            tmpVLines.Add(curVLine);
                        }
                    }
                    else if (i < m_vLines.Count - 1)
                    {
                        // 至倒数第二段的情形
                        // 下一条
                        var nextVLine = m_vLines[i + 1];
                        var nextPtS = nextVLine.StartPoint;
                        var nextPtE = nextVLine.EndPoint;
                        if (curPtS.Y < bottomMaxY && curPtE.Y > bottomMaxY)
                        {
                            // 当前与左边相交 收集
                            var nPtS = new Point3d(curPtS.X, bottomMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, curPtE));
                        }
                        else if (curPtE.Y < bottomMaxY && nextPtS.Y > bottomMaxY)
                        {
                            // 当前是被包含的最后一个线段 收集
                            var nPtS = curPtE;
                            var nPtE = curPtE + new Vector3d(0, 10, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtS.Y > bottomMaxY && curPtE.Y < topMaxY)
                        {
                            // 当前段在左右两条边的中间 收集
                            tmpVLines.Add(curVLine);
                        }
                        if (curPtS.Y < topMaxY && curPtE.Y > topMaxY)
                        {
                            // 当前段与尾段相交 收集
                            var nPtS = curPtS;
                            var nPtE = new Point3d(curPtS.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtE.Y < topMaxY && nextPtS.Y > topMaxY)
                        {
                            // 收集next段
                            var nPtS = nextPtS;
                            var nPtE = nextPtS + new Vector3d(0, 2, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE)); // 最后一段检查长度，小于10的只要放置一个点
                        }
                    }
                    else if (i == m_vLines.Count - 1)
                    {
                        // 最后一段的情形
                        if (curPtS.Y < topMaxY && curPtE.Y > topMaxY)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(curPtS.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtE.Y <= topMaxY)
                        {
                            tmpVLines.Add(curVLine);
                        }

                        if (CommonUtils.IsAlmostNearZero(curPtS.Y - topMaxY, 1E-8) && curPtE.Y > topMaxY)
                        {
                            var nPtS = curPtS;
                            var nPtE = nPtS + new Vector3d(0, 2, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                    }
                }

                m_vLines.Clear();
                m_vLines = tmpVLines;
                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
            }

            /// <summary>
            /// 四周切掉最大距墙值
            /// </summary>
            private void CutFromMaxSprayOffset()
            {
                CutFromMaxSprayOffsetHLines();
                CutFromMaxSprayOffsetVLines();
                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
            }

            /// <summary>
            /// 计算水平第一行
            /// </summary>
            /// <returns></returns>
            private List<Point3d> CalcuHPoints()
            {
                var hPtLst = new List<Point3d>();
                for (int i = 0; i < m_hLines.Count; i++)
                {
                    var curHLine = m_hLines[i];
                    var firPtS = curHLine.StartPoint;
                    var firPtE = curHLine.EndPoint;
                    if (i == m_hLines.Count - 1)
                    {
                        // 最后一段
                        if (curHLine.Length < 10)
                            hPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        else
                        {
                            var xWid = curHLine.Length;
                            var ratio = xWid / m_maxSprayGap;
                            int nCount = (int)ratio;
                            // 多出一点点的情形，不是少于的情形
                            if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                                nCount -= 1;
                            if (nCount == 0)
                                hPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                            else
                            {
                                nCount++;
                                var midStep = (int)(xWid / nCount);
                                var gapAdd = (midStep / 50) * 50;
                                for (int j = 0; j < nCount; j++)
                                {
                                    var curX = firPtS.X + j * gapAdd;
                                    hPtLst.Add(new Point3d(curX, firPtS.Y, 0));
                                }
                            }
                            hPtLst.Add(new Point3d(firPtE.X, firPtE.Y, 0));
                        }
                    }
                    else
                    {
                        // 非最后一段
                        var nextLine = m_hLines[i + 1];
                        var nextPtS = nextLine.StartPoint;
                        var xWid = nextPtS.X - firPtS.X;
                        var ratio = xWid / m_maxSprayGap;
                        int nCount = (int)ratio;
                        // 多出一点点的情形，不是少于的情形
                        if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                            nCount -= 1;
                        if (nCount == 0)
                            hPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        else
                        {
                            nCount++;
                            var midStep = (int)(xWid / nCount);
                            var gapAdd = (midStep / 50) * 50;
                            for (int j = 0; j < nCount; j++)
                            {
                                var curX = firPtS.X + j * gapAdd;
                                hPtLst.Add(new Point3d(curX, firPtS.Y, 0));
                            }
                        }
                    }
                }

                if (hPtLst.Count < 3)
                    return hPtLst;

                // 调整每一行数据，尽量平均
                var resPtLst = new List<Point3d>();
                resPtLst.Add(hPtLst.First());
                for (int i = 1; i < hPtLst.Count - 1; i++)
                {
                    var beforePt = hPtLst[i - 1];
                    var nextPt = hPtLst[i + 1];
                    var curSpt = hPtLst[i];
                    var midPt = new Point3d((beforePt.X + nextPt.X) * 0.5, (beforePt.Y + nextPt.Y) * 0.5, 0);
                    bool bPointIn = false;
                    foreach (var line in m_hLines)
                    {
                        var ptSX = line.StartPoint.X;
                        var ptEX = line.EndPoint.X;
                        if (midPt.X >= ptSX && midPt.X <= ptEX)
                        {
                            bPointIn = true;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        hPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                        resPtLst.Add(curSpt);
                }

                resPtLst.Add(hPtLst.Last());
                // 水平坐标点规整 距离规整到50
                resPtLst = NormalizeHPoints(resPtLst, m_hLines);

                // 两端距离控制以及喷淋最小间隔距离控制
                resPtLst = NormalizeHSprayMinPoints(resPtLst, m_hLines);
                return resPtLst;
            }

            /// <summary>
            /// 归一化水平点
            /// </summary>
            /// <param name="hPtLst"></param>
            /// <param name="range"></param>
            /// <returns></returns>
            private List<Point3d> NormalizeHPoints(List<Point3d> hPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(hPtLst.First());
                for (int i = 1; i < hPtLst.Count - 1; i++)
                {
                    var beforePt = hPtLst[i - 1];
                    var curSpt = hPtLst[i];

                    // 当前点的左右两个点
                    int nRatio = (int)((curSpt.X - beforePt.X) / 50.0);
                    int nNext = nRatio + 1;

                    var ratioPoint = new Point3d(beforePt.X + nRatio * 50, curSpt.Y, 0);
                    var nextPt = new Point3d(beforePt.X + nNext * 50, curSpt.Y, 0);
                    Point3d midPt = Point3d.Origin;

                    bool bPointIn = false;
                    foreach (var line in m_hLines)
                    {
                        var ptSX = line.StartPoint.X;
                        var ptEX = line.EndPoint.X;

                        if ((ratioPoint.X >= ptSX && ratioPoint.X <= ptEX))
                        {
                            bPointIn = true;
                            midPt = ratioPoint;
                            break;
                        }
                        else if ((nextPt.X >= ptSX && nextPt.X <= ptEX))
                        {
                            bPointIn = true;
                            midPt = nextPt;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        hPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }

                resPtLst.Add(hPtLst.Last());
                return resPtLst;
            }

            /// <summary>
            /// 垂直方向，从上到下开始规范化
            /// </summary>
            /// <param name="vPtLst"></param>
            /// <param name="rangeLines"></param>
            /// <returns></returns>
            private List<Point3d> NormalizeVSprayMinPointsFromUp(List<Point3d> vPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(vPtLst.Last());
                vPtLst.Remove(vPtLst.Last());
                for (int i = vPtLst.Count - 1; i >= 0; i--)
                {
                    var beforePt = resPtLst.Last();
                    var curSpt = vPtLst[i];

                    var yGap = Math.Abs(curSpt.Y - beforePt.Y);
                    if (yGap >= m_minSprayGap)
                    {
                        resPtLst.Add(curSpt);
                        continue;
                    }

                    var yPos = beforePt.Y - ((m_minSprayGap / 50) + 1) * 50;
                    var yNewGap = Math.Abs(yPos - beforePt.Y);
                    // 最后一段距离调整
                    if (i == 0)
                    {
                        var maxAdd = m_maxWallOffsetTop - m_minWallOffset;
                        while (true)
                        {
                            if (YRangeVLines(yPos, rangeLines))
                                break;
                            if (Math.Abs(yPos - beforePt.Y) >= m_maxSprayGap || Math.Abs(yPos - beforePt.Y) >= maxAdd)
                            {
                                yPos += 50;
                                break;
                            }
                            yPos -= 50;
                        }
                    }
                    else
                    {
                        // 调整距离前一个喷淋的位置
                        while (true)
                        {
                            if (YRangeVLines(yPos, rangeLines))
                                break;
                            if (Math.Abs(yPos - beforePt.Y) >= m_maxSprayGap)
                            {
                                yPos += 50;
                                break;
                            }
                            yPos -= 50;
                        }
                    }

                    var aimPt = new Point3d(curSpt.X, yPos, 0);
                    resPtLst.Add(aimPt);
                }

                return resPtLst;
            }

            private List<Point3d> NormalizeVSprayMinPoints(List<Point3d> vPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(vPtLst.First());
                for (int i = 1; i < vPtLst.Count; i++)
                {
                    var beforePt = resPtLst.Last();
                    var curSpt = vPtLst[i];
                    //if (i == vPtLst.Count - 1)
                    //{
                    //    resPtLst.Add(curSpt);
                    //    continue;
                    //}
                    var yGap = curSpt.Y - beforePt.Y;
                    if (yGap >= m_minSprayGap)
                    {
                        resPtLst.Add(curSpt);
                        continue;
                    }

                    var yPos = ((m_minSprayGap / 50) + 1) * 50 + beforePt.Y;
                    var yNewGap = yPos - beforePt.Y;
                    // 最后一段距离调整
                    if (i == vPtLst.Count - 1)
                    {
                        var maxAdd = m_maxWallOffsetTop - m_minWallOffset;
                        while (true)
                        {
                            if (YRangeVLines(yPos, rangeLines))
                                break;
                            if (yNewGap >= m_maxSprayGap || yNewGap >= maxAdd)
                            {
                                yPos -= 50;
                                break;
                            }
                            yPos += 50;
                        }
                    }
                    else
                    {
                        // 调整距离前一个喷淋的位置
                        while (true)
                        {
                            if (YRangeVLines(yPos, rangeLines))
                                break;
                            if (yNewGap >= m_maxSprayGap)
                            {
                                yPos -= 50;
                                break;
                            }
                            yPos += 50;
                        }
                    }

                    var aimPt = new Point3d(curSpt.X, yPos, 0);
                    resPtLst.Add(aimPt);
                }

                return resPtLst;
            }

            private List<Point3d> NormalizeHSprayMinPoints(List<Point3d> hPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(hPtLst.First());
                for (int i = 1; i < hPtLst.Count; i++)
                {
                    var beforePt = resPtLst.Last();
                    var curSpt = hPtLst[i];
                    var xGap = curSpt.X - beforePt.X;
                    if (xGap >= m_minSprayGap)
                    {
                        resPtLst.Add(curSpt);
                        continue;
                    }

                    var xPos = ((m_minSprayGap / 50) + 1) * 50 + beforePt.X;
                    // 最后一段距离调整
                    if (i == hPtLst.Count - 1)
                    {
                        var maxAdd = m_maxWallOffsetRight - m_minWallOffset;
                        while (true)
                        {
                            if (XRangeHLines(xPos, rangeLines))
                                break;
                            if ((xPos - beforePt.X) >= m_maxSprayGap || (xPos - beforePt.X) >= maxAdd)
                            {
                                xPos -= 50;
                                break;
                            }
                            xPos += 50;
                        }
                    }
                    else
                    {
                        // 调整距离前一个喷淋的位置
                        while (true)
                        {
                            if (XRangeHLines(xPos, rangeLines))
                                break;
                            if ((xPos - beforePt.X) >= m_maxSprayGap)
                            {
                                xPos -= 50;
                                break;
                            }
                            xPos += 50;
                        }
                    }

                    var aimPt = new Point3d(xPos, curSpt.Y, 0);
                    resPtLst.Add(aimPt);
                }

                return resPtLst;
            }

            private bool YRangeVLines(double yPos, List<Line> rangeVLines)
            {
                foreach (var line in rangeVLines)
                {
                    var ptSY = line.StartPoint.Y;
                    var ptEY = line.EndPoint.Y;

                    if ((yPos >= ptSY && yPos <= ptEY))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool XRangeHLines(double xPos, List<Line> rangeHLines)
            {
                foreach (var line in rangeHLines)
                {
                    var ptSX = line.StartPoint.X;
                    var ptEX = line.EndPoint.X;

                    if ((xPos >= ptSX && xPos <= ptEX))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool PtRangeHLines(Point3d pt, List<Line> rangeHLines)
            {
                foreach (var line in rangeHLines)
                {
                    var ptSX = line.StartPoint.X;
                    var ptEX = line.EndPoint.X;

                    if ((pt.X >= ptSX && pt.X <= ptEX))
                    {
                        return true;
                    }
                }

                return false;
            }

            private List<Point3d> NormalizeVPoints(List<Point3d> vPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(vPtLst.First());
                for (int i = 1; i < vPtLst.Count - 1; i++)
                {
                    var beforePt = vPtLst[i - 1];
                    var curSpt = vPtLst[i];

                    // 当前点的上下两个点
                    int nRatio = (int)((curSpt.Y - beforePt.Y) / 50.0);
                    int nNext = nRatio + 1;

                    var ratioPoint = new Point3d(beforePt.X, beforePt.Y + nRatio * 50, 0);
                    var nextPt = new Point3d(beforePt.X, beforePt.Y + nNext * 50, 0);
                    Point3d midPt = Point3d.Origin;

                    bool bPointIn = false;
                    foreach (var line in m_vLines)
                    {
                        var ptSY = line.StartPoint.Y;
                        var ptEY = line.EndPoint.Y;

                        if ((ratioPoint.Y >= ptSY && ratioPoint.Y <= ptEY))
                        {
                            bPointIn = true;
                            midPt = ratioPoint;
                            break;
                        }
                        else if ((nextPt.Y >= ptSY && nextPt.Y <= ptEY))
                        {
                            bPointIn = true;
                            midPt = nextPt;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        vPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }

                resPtLst.Add(vPtLst.Last());
                return resPtLst;
            }

            /// <summary>
            /// 计算垂直的点
            /// </summary>
            /// <returns></returns>
            private List<Point3d> CalcuVPointsOP()
            {
                var vPtLst = new List<Point3d>();
                for (int i = 0; i < m_vLines.Count; i++)
                {
                    var curVLine = m_vLines[i];
                    var firPtS = curVLine.StartPoint;
                    var firPtE = curVLine.EndPoint;
                    if (i == m_vLines.Count - 1)
                    {
                        // 最后一段
                        if (curVLine.Length < 10)
                        {
                            vPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        }
                        else
                        {
                            var yWid = curVLine.Length;
                            var ratio = yWid / m_maxSprayGap;
                            int nCount = (int)ratio;

                            // 多出一点点的情形，不是少于的情形
                            if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                                nCount -= 1;

                            if (nCount == 0)
                            {
                                vPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                            }
                            else
                            {
                                nCount++;
                                var midStep = (int)(yWid / nCount);
                                var gapAdd = (midStep / 50) * 50;

                                for (int j = 0; j < nCount; j++)
                                {
                                    var curY = firPtS.Y + j * gapAdd;
                                    vPtLst.Add(new Point3d(firPtS.X, curY, 0));
                                }
                            }

                            vPtLst.Add(new Point3d(firPtE.X, firPtE.Y, 0));
                        }
                    }
                    else
                    {
                        // 非最后一段
                        var nextLine = m_vLines[i + 1];
                        var nextPtS = nextLine.StartPoint;
                        var yWid = nextPtS.Y - firPtS.Y;
                        var ratio = yWid / m_maxSprayGap;
                        int nCount = (int)ratio;

                        // 多出一点点的情形，不是少于的情形
                        if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                            nCount -= 1;

                        if (nCount == 0)
                        {
                            vPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        }
                        else
                        {
                            nCount++;
                            var midStep = (int)(yWid / nCount);
                            var gapAdd = (midStep / 50) * 50;

                            for (int j = 0; j < nCount; j++)
                            {
                                var curY = firPtS.Y + j * gapAdd;
                                vPtLst.Add(new Point3d(firPtS.X, curY, 0));
                            }
                        }
                    }
                }

                if (vPtLst.Count < 3)
                    return vPtLst;

                var resPtLst = new List<Point3d>();
                resPtLst.Add(vPtLst.First());
                for (int i = 1; i < vPtLst.Count - 1; i++)
                {
                    var beforePt = vPtLst[i - 1];
                    var nextPt = vPtLst[i + 1];
                    var curSpt = vPtLst[i];
                    var midPt = new Point3d((beforePt.X + nextPt.X) * 0.5, (beforePt.Y + nextPt.Y) * 0.5, 0);
                    bool bPointIn = false;
                    foreach (var line in m_vLines)
                    {
                        if (CommonUtils.IsPointOnLine(midPt, line))
                        {
                            bPointIn = true;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        vPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }
                resPtLst.Add(vPtLst.Last());
                // 垂直方向规整
                resPtLst = NormalizeVPoints(resPtLst, m_vLines);

                // 两端距离控制以及喷淋最小间隔距离控制
                resPtLst = NormalizeVSprayMinPoints(resPtLst, m_vLines);
                return resPtLst;
            }

            /// <summary>
            /// y表示两个点之间的间距值
            /// </summary>
            /// <param name="yValue"></param>
            /// <param name="curPt"></param>
            /// <returns></returns>
            private double CalculateVGapFromMin(double yValue, Point3d curPt)
            {
                var curYvalue = yValue;
                while (true)
                {
                    if (curYvalue > m_maxSprayGap)
                    {
                        m_bEnd = true;
                        return m_minSprayGap;
                    }

                    var yPoint = curPt + new Vector3d(0, 1, 0) * curYvalue;
                    var yPos = yPoint.Y;
                    if (YRangeVLines(yPos, m_vLines))
                        return curYvalue;
                    else
                        curYvalue += 50;
                }
            }

            private double CalculateVGapFromMax(double yValue, Point3d curPt)
            {
                var curYvalue = yValue;
                while (true)
                {
                    if (curYvalue < m_minSprayGap)
                    {
                        m_bEnd = true;
                        return m_minSprayGap;
                    }

                    var yPoint = curPt + new Vector3d(0, 1, 0) * curYvalue;
                    var yPos = yPoint.Y;
                    if (YRangeVLines(yPos, m_vLines))
                        return curYvalue;
                    else
                        curYvalue -= 50;
                }
            }

            /// <summary>
            /// 计算垂直的点
            /// </summary>
            /// <returns></returns>
            private List<Point3d> CalcuVPoints()
            {
                var vPtLst = new List<Point3d>();
                var lastLine = m_vLines.Last();
                if (m_vLines.Count == 1)
                {

                    if (lastLine.Length < 10)
                    {
                        vPtLst.Add(lastLine.StartPoint);
                        return vPtLst;
                    }
                }

                var firstPt = m_vLines.First().StartPoint;
                var endPt = lastLine.EndPoint;

                if (lastLine.Length < 10)
                    endPt = lastLine.StartPoint;
                vPtLst.Add(firstPt);
                var vec = new Vector3d(0, 1, 0);
                bool bReviseCheck = false;
                while (true)
                {
                    var tailPoint = vPtLst.Last();
                    var yGapFromMax = (m_maxSprayGap / 50) * 50; // 差值小于一个50
                    var curPt = tailPoint + vec * yGapFromMax;
                    if (curPt.Y > endPt.Y)
                    {
                        var yTailGap = Math.Abs(tailPoint.Y - endPt.Y);
                        if (yTailGap > m_minSprayGap)
                        {
                            vPtLst.Add(endPt);
                            break;
                        }
                        else
                        {
                            var yTailGapAdd = yTailGap + (m_maxWallOffsetTop - m_minWallOffsetTop);
                            if (yTailGapAdd < m_minSprayGap)
                                bReviseCheck = true;
                            var tmpPt = tailPoint + vec * yTailGapAdd;
                            vPtLst.Add(tmpPt);
                            break;
                        }
                    }
                    else
                    {
                        yGapFromMax = CalculateVGapFromMax(yGapFromMax, tailPoint);
                        var tmpPt = tailPoint + vec * yGapFromMax;
                        vPtLst.Add(tmpPt);
                    }
                }

                if (bReviseCheck && vPtLst.Count > 2)
                {
                    vPtLst = NormalizeVSprayMinPointsFromUp(vPtLst, m_vLines); ;
                }

                return vPtLst;
            }

            /// <summary>
            /// 开始布置
            /// </summary>
            private List<Point3d> DoPlace()
            {
                var hPtLst = CalcuHPoints(); // 水平分割点
                var vPtLst = CalcuVPoints(); // 垂直分割点
                var dbCol = new DBObjectCollection();

                var curPtY = hPtLst.First().Y;
                var ptsLst = new List<List<Point3d>>();

                foreach (var vPt in vPtLst)
                {
                    var vY = vPt.Y;
                    var moveValue = vY - curPtY;
                    var movePtLst = new List<Point3d>();
                    if (CommonUtils.IsAlmostNearZero(Math.Abs(moveValue), 10))
                    {
                        ptsLst.Add(hPtLst);
                    }
                    else
                    {
                        foreach (var hPt in hPtLst)
                        {
                            var movePt = hPt + new Vector3d(0, moveValue, 0);
                            movePtLst.Add(movePt);
                        }
                        ptsLst.Add(movePtLst);
                    }
                }

                // 有效的放置点
                var validPtsLst = ErasePoints(m_roomSrcCurves, ptsLst);
                //var validPtsLst = ptsLst;
                //foreach (var drawPts in validPtsLst)
                //    foreach (var drawPt in drawPts)
                //        Utils.DrawPreviewPoint(new DBObjectCollection(), drawPt);
                var insertPts = new List<Point3d>();
                if (validPtsLst != null && validPtsLst.Count != 0)
                {
                    foreach (var pts in validPtsLst)
                    {
                        insertPts.AddRange(pts);
                    }
                }
                return insertPts;
            }

            /// <summary>
            /// 删除无效点
            /// </summary>
            /// <param name="srcCurves"></param>
            /// <param name="ptsLst"></param>
            /// <returns></returns>
            private List<List<Point3d>> ErasePoints(List<Curve> srcCurves, List<List<Point3d>> ptsLst)
            {
                var validPtsLst = new List<List<Point3d>>();

                foreach (var pts in ptsLst)
                {
                    var validPtLst = new List<Point3d>();
                    foreach (var pt in pts)
                    {
                        if (CommonUtils.PtInLoop(srcCurves, pt))
                            validPtLst.Add(pt);
                    }

                    validPtsLst.Add(validPtLst);
                }

                return validPtsLst;
            }
        }

        /// <summary>
        /// 计算多段线的外接矩形框下的四条边，垂直开始边和水平开始边
        /// </summary>
        public class EdgeCalcu
        {
            private List<Curve> m_srcCurves;

            // Src表示是最小矩形盒构成的四条边
            public Line SrcRightEdge;
            public Line SrcTopEdge;

            public Line SrcLeftEdge;
            public Line SrcBottomEdge;

            public List<Line> BoundCurves;
            public EdgeCalcu(List<Curve> curves)
            {
                m_srcCurves = curves;
            }

            public void Do()
            {
                var box = new BoundBoxPlane(m_srcCurves);

                // 房间的右边
                SrcRightEdge = box.CalculateRightEdge();
                // 房间的上边
                SrcTopEdge = box.CalculateTopEdge();

                // 房间的左边
                SrcLeftEdge = box.CalculateLeftEdge();

                // 房间的下边
                SrcBottomEdge = box.CalculateBottomEdge();
                // 房间的边界原始curves
                BoundCurves = box.CalcuBoundCurves();
            }
        }


        public static List<Curve> GetValidBeams(List<Curve> beams, List<Curve> roomCurves)
        {
            if (beams == null || beams.Count == 0 || roomCurves == null || roomCurves.Count == 0)
                return null;

            var relatedCurves = new List<Curve>();
            var intersectCurves = new List<Curve>();
            foreach (var beam in beams)
            {
                if (LoopContainCurve(roomCurves, beam))
                    relatedCurves.Add(beam);
                else if (CurveIntersectWithLoop(beam, roomCurves))
                {
                    intersectCurves.Add(beam);
                }
            }

            // 裁剪掉房间框线外的梁曲线
            var newCurves = ScatterCurves.MakeScatterCurves(intersectCurves, roomCurves);
            if (newCurves != null)
            {
                foreach (var curve in newCurves)
                {
                    var ptMid = curve.GetPointAtParameter((curve.StartParam + curve.EndParam) * 0.5);
                    //Utils.DrawPreviewPoint(new DBObjectCollection(), ptMid);
                    if (CommonUtils.PtOnCurves(ptMid, roomCurves))
                        continue;

                    if (CommonUtils.PtInLoop(roomCurves, ptMid))
                        relatedCurves.Add(curve);
                }
            }

            //Utils.DrawProfile(relatedCurves, "relatedCurves");
            //Utils.DrawProfile(roomCurves, "roomCurves", Color.FromRgb(0, 255, 0));
            return relatedCurves;
        }


        public static bool CurveIntersectWithLoop(Curve srcCurve, List<Curve> loop)
        {
            foreach (var curve in loop)
            {
                if (!CommonUtils.IntersectValid(srcCurve, curve))
                    continue;

                var ptLst = new Point3dCollection();
                srcCurve.IntersectWith(curve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                if (ptLst.Count != 0)
                {
                    return true;
                }
            }

            return false;
        }


        public static bool CurveIntersectWithLoop(Curve srcCurve, List<Curve> loop, out Point3d intersectPt, out Curve intersectCurve)
        {
            foreach (var curve in loop)
            {
                if (!CommonUtils.IntersectValid(srcCurve, curve))
                    continue;

                var ptLst = new Point3dCollection();
                srcCurve.IntersectWith(curve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                if (ptLst.Count != 0)
                {
                    intersectPt = ptLst[0];
                    intersectCurve = curve;
                    return true;
                }
            }

            intersectPt = Point3d.Origin;
            intersectCurve = null;
            return false;
        }

        /// <summary>
        /// 包含断点在曲线上的情形
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static bool LoopContainCurve(List<Curve> loop, Curve curve)
        {
            bool bStartPtOn = CommonUtils.PtInLoop(loop, curve.StartPoint) || CommonUtils.PtOnCurves(curve.StartPoint, loop, 1e-3);
            bool bEndPtOn = CommonUtils.PtInLoop(loop, curve.EndPoint) || CommonUtils.PtOnCurves(curve.EndPoint, loop, 1e-3);
            if (bStartPtOn && bEndPtOn)
                return true;
            else
                return false;
        }

        public static List<Curve> Line2d2Curve(List<LineSegment2d> line2ds)
        {
            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;
                var pt3S = new Point3d(ptS.X, ptS.Y, 0);
                var pt3E = new Point3d(ptE.X, ptE.Y, 0);
                var line = new Line(pt3S, pt3E);
                curves.Add(line);
            }

            return curves;
        }

        //public static void CreateGroup(List<List<TopoEdge>> topoEdges, string groupName, string showName)
        //{

        //    Document doc = Application.DocumentManager.MdiActiveDocument;
        //    Database db = doc.Database;
        //    Editor ed = doc.Editor;
        //    Utils.CreateLayer(showName, Color.FromRgb(255, 0, 0));

        //    using (Transaction tr = db.TransactionManager.StartTransaction())
        //    {
        //        // Get the group dictionary from the drawing
        //        DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForRead);

        //        foreach (var loop in topoEdges)
        //        {
        //            var loopcurves = new List<Curve>();
        //            foreach (var edge in loop)
        //            {
        //                loopcurves.Add(edge.SrcCurve);
        //            }

        //            //Random random = new Random();
        //            //int value = random.Next(100000000);
        //            Utils.INDEX++;
        //            string grpName = groupName + Utils.INDEX;
        //            //do
        //            //{
        //            //    inputGrpName = groupName + value;
        //            //    try
        //            //    {
        //            //        if (gd.Contains(inputGrpName))
        //            //            ed.WriteMessage("\nA group with this name already exists.");
        //            //        else
        //            //            grpName = inputGrpName;
        //            //    }
        //            //    catch
        //            //    {
        //            //        ed.WriteMessage("\nInvalid group name.");
        //            //    }
        //            //} while (grpName == "");

        //            // Create our new group...
        //            Group grp = new Group("Profile group", true);
        //            var color = Color.FromRgb(255, 255, 0);
        //            grp.SetColor(color);
        //            // Add the new group to the dictionary
        //            gd.UpgradeOpen();
        //            ObjectId grpId = gd.SetAt(grpName, grp);
        //            tr.AddNewlyCreatedDBObject(grp, true);
        //            // Open the model-space
        //            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        //            BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        //            // Add some lines to the group to form a square
        //            // (the entities belong to the model-space)

        //            ObjectIdCollection ids = new ObjectIdCollection();
        //            foreach (Entity ent in loopcurves)
        //            {
        //                ent.Layer = showName;
        //                ObjectId id = ms.AppendEntity(ent);
        //                ids.Add(id);
        //                tr.AddNewlyCreatedDBObject(ent, true);
        //            }

        //            grp.InsertAt(0, ids);
        //        }

        //        tr.Commit();
        //    }
        //}

        public static void DrawPreviewPoint(DBObjectCollection objCol, Point3d pt, double length = 500)
        {
            var dbCollection = new DBObjectCollection();
            var half = length * 0.5;
            var curveFirStart = pt + new Vector3d(1, 1, 0).GetNormal() * half;
            var curveFirEnd = curveFirStart - new Vector3d(1, 1, 0).GetNormal() * length;
            var curFir = new Line(curveFirStart, curveFirEnd);
            objCol.Add(curFir);

            var curveSecStart = pt + new Vector3d(-1, 1, 0).GetNormal() * half;
            var curveSecEnd = curveSecStart - new Vector3d(-1, 1, 0).GetNormal() * length;
            var curveSec = new Line(curveSecStart, curveSecEnd);
            objCol.Add(curveSec);
            IntegerCollection intCol = new IntegerCollection();
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                tm.AddTransient(curFir, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
                tm.AddTransient(curveSec, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
            }
        }

        public static void DrawPreviewPoint(Point3d pt, string layerName, double length = 500)
        {
            var dbCollection = new DBObjectCollection();
            var half = length * 0.5;
            var curveFirStart = pt + new Vector3d(1, 1, 0).GetNormal() * half;
            var curveFirEnd = curveFirStart - new Vector3d(1, 1, 0).GetNormal() * length;
            var curFir = new Line(curveFirStart, curveFirEnd);

            var curveSecStart = pt + new Vector3d(-1, 1, 0).GetNormal() * half;
            var curveSecEnd = curveSecStart - new Vector3d(-1, 1, 0).GetNormal() * length;
            var curveSec = new Line(curveSecStart, curveSecEnd);
            CreateLayer(layerName, Color.FromRgb(255, 0, 0));
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                var id1 = acad.ModelSpace.Add(curFir);
                acad.ModelSpace.Element(id1, true).Layer = layerName;
                var id2 = acad.ModelSpace.Add(curveSec);
                acad.ModelSpace.Element(id2, true).Layer = layerName;

            }
        }

        public static void ErasePreviewPoint(DBObjectCollection dbCol)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                foreach (DBObject obj in dbCol)
                {
                    tm.EraseTransient(obj, new IntegerCollection());
                    obj.Dispose();
                }
            }
        }

        /// <summary>
        /// 创建新的图层
        /// </summary>
        /// <param name="allLayers"></param>
        /// <param name="aimLayer"></param>
        public static void CreateLayer(string aimLayer, Color color)
        {
            LayerTableRecord layerRecord = null;
            using (var db = AcadDatabase.Active())
            {
                foreach (var layer in db.Layers)
                {
                    if (layer.Name.Equals(aimLayer))
                    {
                        layerRecord = db.Layers.Element(aimLayer);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(aimLayer);
                    layerRecord.Color = color;
                    layerRecord.IsPlottable = false;
                }
            }
        }

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="curvesLst"></param>
        public static void ShowCurves(List<List<Curve>> curvesLst)
        {
            var polylines = Convert2Polyline(curvesLst);
            DisplayBoundaryProfile(polylines, "polyline");
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DisplayBoundaryProfile(List<Polyline> boundaryPolylines, string boundaryLayerName)
        {
            if (boundaryPolylines == null || boundaryPolylines.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                CreateLayer(boundaryLayerName, Color.FromRgb(255, 0, 0));

                foreach (var polyline in boundaryPolylines)
                {
                    var objectPolylineId = db.ModelSpace.Add(polyline);
                    db.ModelSpace.Element(objectPolylineId, true).Layer = boundaryLayerName;
                }
            }
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DrawProfile(List<Curve> curves, string LayerName, Color color = null)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                if (color == null)
                    CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                else
                    CreateLayer(LayerName, color);

                foreach (var curve in curves)
                {
                    var clone = curve.Clone() as Curve;
                    clone.Layer = LayerName;
                    db.ModelSpace.Add(clone);
                }
            }
        }

        public static void DrawLineSegments(List<LineSegment2d> line2ds, string layerName, Color color = null)
        {
            if (line2ds == null || line2ds.Count == 0)
                return;

            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;

                var ptS3d = new Point3d(ptS.X, ptS.Y, 0);
                var ptE3d = new Point3d(ptE.X, ptE.Y, 0);
                curves.Add(new Line(ptS3d, ptE3d));
            }

            Utils.DrawProfile(curves, layerName, color);
        }

        public static void DrawTextProfile(List<Curve> curves, List<string> layerNames, Color color = null)
        {
            if (curves == null || curves.Count == 0 || layerNames == null || layerNames.Count == 0)
                return;

            if (curves.Count != layerNames.Count)
                return;

            using (var db = AcadDatabase.Active())
            {
                for (int i = 0; i < curves.Count; i++)
                {
                    var curve = curves[i];
                    var LayerName = layerNames[i];

                    var dbtext = new DBText();
                    dbtext.Height = 50;
                    var midPt = curve.GetPointAtParameter(0.5 * (curve.StartParam + curve.EndParam));
                    dbtext.Justify = AttachmentPoint.MiddleCenter;
                    // 设置字体样式
                    var textId = GetIdFromSymbolTable();
                    if (textId != ObjectId.Null)
                        dbtext.TextStyleId = textId;

                    dbtext.TextString = LayerName;
                    dbtext.AlignmentPoint = midPt;

                    var dbId = db.ModelSpace.Add(dbtext);
                    db.ModelSpace.Element(dbId, true).Layer = LayerName;
                }
            }
        }

        public static void DrawTexts(List<DBText> texts, string layerName)
        {
            if (texts == null || texts.Count == 0)
                return;

            CreateLayer(layerName, Color.FromRgb(255, 0, 0));

            using (var db = AcadDatabase.Active())
            {
                for (int i = 0; i < texts.Count; i++)
                {
                    var curve = texts[i];

                    var dbtext = new DBText();
                    dbtext.Height = 50;
                    dbtext.Justify = AttachmentPoint.MiddleCenter;
                    // 设置字体样式
                    var textId = GetIdFromSymbolTable();
                    if (textId != ObjectId.Null)
                        dbtext.TextStyleId = textId;

                    var dbId = db.ModelSpace.Add(dbtext);
                    db.ModelSpace.Element(dbId, true).Layer = layerName;
                }
            }
        }

        public static void DrawText(string textString, Point3d pos)
        {
            CreateLayer(textString, Color.FromRgb(255, 0, 0));

            using (var db = AcadDatabase.Active())
            {
                var dbtext = new DBText();
                dbtext.Height = 50;
                dbtext.Justify = AttachmentPoint.MiddleCenter;

                dbtext.AlignmentPoint = pos;
                dbtext.TextString = textString;

                // 设置字体样式
                var textId = GetIdFromSymbolTable();
                if (textId != ObjectId.Null)
                    dbtext.TextStyleId = textId;

                var dbId = db.ModelSpace.Add(dbtext);
                db.ModelSpace.Element(dbId, true).Layer = textString;
            }
        }

        public static void DrawProfile(List<TopoEdge> topoEdges, string LayerName, Color color = null)
        {
            if (topoEdges == null || topoEdges.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                if (color == null)
                    CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                else
                    CreateLayer(LayerName, color);

                foreach (var topoEdge in topoEdges)
                {
                    var objectCurveId = db.ModelSpace.Add(topoEdge.SrcCurve.Clone() as Curve);
                    db.ModelSpace.Element(objectCurveId, true).Layer = LayerName;
                }
            }
        }

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

        public static void DrawProfileAndText(List<Curve> curves, Color color = null)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                foreach (var curve in curves)
                {
                    var LayerName = curve.Layer;
                    if (color == null)
                        CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                    else
                        CreateLayer(LayerName, color);

                    var objectCurveId = db.ModelSpace.Add(curve.Clone() as Curve);
                    db.ModelSpace.Element(objectCurveId, true).Layer = LayerName;
                    var dbtext = new DBText();
                    dbtext.Height = 50;
                    var midParam = 0.5 * (curve.StartParam + curve.EndParam);
                    if (CommonUtils.IsAlmostNearZero(midParam))
                        continue;

                    var midPt = curve.GetPointAtParameter(midParam);
                    dbtext.Justify = AttachmentPoint.MiddleCenter;
                    // 设置字体样式
                    var textId = GetIdFromSymbolTable();
                    if (textId != ObjectId.Null)
                        dbtext.TextStyleId = textId;

                    dbtext.TextString = LayerName;
                    dbtext.AlignmentPoint = midPt;
                    var dbId = db.ModelSpace.Add(dbtext);
                    db.ModelSpace.Element(dbId, true).Layer = LayerName;
                }
            }
        }

        public static void AddProfile(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {

                foreach (var curve in curves)
                {
                    var objectCurveId = db.ModelSpace.Add(curve);
                }
            }
        }

        public static void AddProfile(List<Curve> curves, string LayerName, Color color = null)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                if (color == null)
                    CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                else
                    CreateLayer(LayerName, color);

                foreach (var curve in curves)
                {
                    var objectCurveId = db.ModelSpace.Add(curve);
                    db.ModelSpace.Element(objectCurveId, true).Layer = LayerName;
                }
            }
        }

        public static List<string> GetLayersFromCurves(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var layerNames = new List<string>();

            foreach (var curve in curves)
            {
                layerNames.Add(curve.Layer);
            }
            return layerNames;
        }

        public static void EraseProfile(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                foreach (var curve in curves)
                {
                    var cur = db.Element<Curve>(curve.Id, true);
                    cur.Visible = false;
                    cur.Erase();
                }
            }
        }
        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<TopoEdge> edges, string layerName = null)
        {
            if (edges == null || edges.Count == 0)
                return;
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (layerName != null)
                    CreateLayer(layerName, Color.FromRgb(255, 0, 0));

                foreach (var edge in edges)
                {
                    var ptS = edge.Start;
                    var ptE = edge.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 255);
                    if (layerName != null)
                        line.Layer = layerName;
                    // 添加到modelSpace中
                    AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
                }
            }
        }

        /// <summary>
        /// 格式转换
        /// </summary>
        /// <param name="curvesLst"></param>
        /// <returns></returns>
        public static List<Polyline> Convert2Polyline(List<List<Curve>> curvesLst)
        {
            if (curvesLst == null || curvesLst.Count == 0)
                return null;

            var polylineLst = new List<Polyline>();
            foreach (var curves in curvesLst)
            {
                var polyline = new Polyline();
                polyline.Closed = true;

                for (int i = 0; i < curves.Count; i++)
                {
                    var point = curves[i].StartPoint;
                    polyline.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
                }

                polylineLst.Add(polyline);
            }

            return polylineLst;
        }

        /// <summary>
        /// 获取block中的所有曲线数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Curve> GetCurvesFromBlock(BlockReference block, bool bSetLayer = true)
        {
            DBObjectCollection collection = new DBObjectCollection();
            try
            {
                block.Explode(collection);
            }
            catch (Exception e)
            {
                Utils.DrawText(e.Message, block.Position);
                return null;
            }

            var blockCurves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {
                foreach (var obj in collection)
                {
                    if (obj is Curve)
                    {
                        var curve = obj as Curve;
                        if (bSetLayer)
                            curve.Layer = block.Layer;
                        blockCurves.Add(curve);
                    }
                    else if (obj is BlockReference)
                    {
                        var blockReference = obj as BlockReference;
                        if (bSetLayer)
                            blockReference.Layer = block.Layer;
                        var childCurves = GetCurvesFromBlock(blockReference, bSetLayer);
                        if (childCurves.Count != 0)
                            blockCurves.AddRange(childCurves);
                    }
                }
            }

            return blockCurves;
        }

        public static List<Curve> GetCurvesFromAEWINDBlock(BlockReference block, bool bSetLayer = true)
        {
            DBObjectCollection collection = new DBObjectCollection();
            try
            {
                // AE-WIND 中的BLOCK 中的curves 层数 必须是1， 剔除当前图纸中的无效wind图块
                var blockInnerLayer = new List<string>();
                block.Explode(collection);
                int arcCount = 0;
                foreach (Entity obj in collection)
                {
                    if (obj is Arc)
                        arcCount++;
                    //if (!blockInnerLayer.Contains(obj.Layer))
                    //    blockInnerLayer.Add(obj.Layer);
                    //if (blockInnerLayer.Count > 1)
                    //    return null;
                }
                if (arcCount == 0)
                    return null;
            }
            catch (Exception e)
            {
                Utils.DrawText(e.Message, block.Position);
                return null;
            }

            var blockCurves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {
                foreach (var obj in collection)
                {
                    if (obj is Curve)
                    {
                        var curve = obj as Curve;
                        if (bSetLayer)
                            curve.Layer = block.Layer;
                        blockCurves.Add(curve);
                    }
                    else if (obj is BlockReference)
                    {
                        var blockReference = obj as BlockReference;
                        if (bSetLayer)
                            blockReference.Layer = block.Layer;
                        var childCurves = GetCurvesFromBlock(blockReference, bSetLayer);
                        if (childCurves.Count != 0)
                            blockCurves.AddRange(childCurves);
                    }
                }
            }

            return blockCurves;
        }

        /// <summary>
        /// 获取矩形框截取后的curves
        /// </summary>
        /// <param name="allCurves"></param>
        /// <param name="rectLines"></param>
        /// <returns></returns>
        public static List<Curve> GetValidCurvesFromSelectArea(List<Curve> allCurves, List<LineSegment2d> rectLines)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            var validCurves = new List<Curve>();
            foreach (var srcCurve in allCurves)
            {
                if (IsValidCurve(srcCurve, rectLines))
                    validCurves.Add(srcCurve);
            }

            return validCurves;
        }

        public static List<Point3d> GetValidFPointsFromSelectPLine(List<Point3d> point3s, Polyline poly)
        {
            var resPts = new List<Point3d>();
            var obcurves = poly.GetOffsetCurves(500);
            //Utils.DrawProfile(new List<Curve>() { poly }, "poly");
            if (obcurves.Count == 0)
                obcurves.Add(poly);
            var lineSegments = new List<LineSegment2d>();
            foreach (Polyline curve in obcurves)
            {
                var lines = Polyline2Lines(curve);
                if (lines != null && lines.Count != 0)
                    lineSegments.AddRange(lines);
            }

            foreach (var pt in point3s)
            {
                if (CommonUtils.PtInLoop(lineSegments, pt.toPoint2d()))
                    resPts.Add(pt);
            }

            return resPts;
        }

        public static List<RoomTextNode> GetValidFRoomNodeFromSelectPLine(List<RoomTextNode> roomTextNodes, Polyline poly)
        {
            var resRoomNodes = new List<RoomTextNode>();
            var obcurves = poly.GetOffsetCurves(500);
            //Utils.DrawProfile(new List<Curve>() { poly }, "poly");
            if (obcurves.Count == 0)
                obcurves.Add(poly);
            var lineSegments = new List<LineSegment2d>();
            foreach (Polyline curve in obcurves)
            {
                var lines = Polyline2Lines(curve);
                if (lines != null && lines.Count != 0)
                    lineSegments.AddRange(lines);
            }

            foreach (var roomNode in roomTextNodes)
            {
                if (CommonUtils.PtInLoop(lineSegments, roomNode.textPoint.toPoint2d()))
                    resRoomNodes.Add(roomNode);
            }

            return resRoomNodes;
        }

        public static List<Curve> GetValidCurvesFromSelectPLine(List<Curve> allCurves, Polyline poly)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            var obcurves = poly.GetOffsetCurves(500);
            if (obcurves.Count == 0)
                obcurves.Add(poly);
            var lineSegments = new List<LineSegment2d>();
            CreateLayer("FLine", Color.FromRgb(0, 0, 255));
            foreach (var curve in obcurves)
            {
                if (curve is Polyline polyline)
                {
                    var lines = Polyline2Lines(polyline);
                    if (lines != null && lines.Count != 0)
                        lineSegments.AddRange(lines);
                }
            }

            var validCurves = new List<Curve>();
            //var curveNodes = TopoUtils.Polyline2Curves(poly);
            //foreach (var node in curveNodes)
            //{
            //    node.Layer = "FLine";
            //}

            //if (curveNodes != null && curveNodes.Count != 0)
            //    validCurves.AddRange(curveNodes);

            foreach (var srcCurve in allCurves)
            {
                if (IsValidCurve(srcCurve, lineSegments))
                    validCurves.Add(srcCurve);
            }

            return validCurves;
        }

        public static List<Curve> GetValidCurvesFromSelectPLineNoSelf(List<Curve> allCurves, Polyline poly)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            var obcurves = poly.GetOffsetCurves(500);
            if (obcurves.Count == 0)
                obcurves.Add(poly);
            var lineSegments = new List<LineSegment2d>();

            foreach (var curve in obcurves)
            {
                if (curve is Polyline polyline)
                {
                    var lines = Polyline2Lines(polyline);
                    if (lines != null && lines.Count != 0)
                        lineSegments.AddRange(lines);
                }
            }

            var validCurves = new List<Curve>();
            foreach (var srcCurve in allCurves)
            {
                if (IsValidCurve(srcCurve, lineSegments))
                    validCurves.Add(srcCurve);
            }

            return validCurves;
        }

        public static List<List<LineSegment2d>> GetValidBoundsFromSelectPLine(List<List<LineSegment2d>> allBounds, Polyline poly)
        {
            if (allBounds == null || allBounds.Count == 0)
                return null;

            var obcurves = poly.GetOffsetCurves(500);
            if (obcurves.Count == 0)
                obcurves.Add(poly);
            var lineSegments = new List<LineSegment2d>();
            foreach (Polyline curve in obcurves)
            {
                var lines = Polyline2Lines(curve);
                if (lines != null && lines.Count != 0)
                    lineSegments.AddRange(lines);
            }

            // 计算有效的门窗边界
            var resBounds = new List<List<LineSegment2d>>();
            foreach (var bound in allBounds)
            {
                if (IsValidBound(bound, lineSegments))
                    resBounds.Add(bound);
            }

            return resBounds;
        }

        public static bool IsValidBound(List<LineSegment2d> bound, List<LineSegment2d> profileLines)
        {
            if (bound == null && bound.Count == 0)
                return false;

            if (IsLinesIntersectWithLines(bound, profileLines) || CommonUtils.OutLoopContainsInnerLoop(profileLines, bound))
            {
                return true;
            }

            return false;
        }

        public static bool IsValidCurve(Curve curve, List<LineSegment2d> rectLines)
        {
            var bound = curve.Bounds.Value;
            if (bound == null)
                return false;

            var srcLine2ds = GetRectFromBound(bound);
            if (IsLinesIntersectWithLines(srcLine2ds, rectLines) || CommonUtils.OutLoopContainsInnerLoop(rectLines, srcLine2ds))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取指定图层中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<Curve> GetAllCurvesFromLayerNames(List<string> layerNames)
        {
            var curves = new List<Curve>();
            var curveIds = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {

                // curve 读取
                var res = db.ModelSpace.OfType<Curve>().Where(p =>
                {
                    if (!p.Visible)
                        return false;

                    var layerTrue = false;
                    var curLayer = p.Layer;
                    foreach (var layerName in layerNames)
                    {
                        if (curLayer == layerName)
                        {
                            layerTrue = true;
                            break;
                        }
                    }

                    if (layerTrue)
                    {
                        var layerTableRecord = db.Element<LayerTableRecord>(p.LayerId);
                        if (layerTableRecord.IsFrozen || layerTableRecord.IsOff)
                            return false;
                        else
                            return true;
                    }

                    return false;

                }).ToList<Curve>();

                foreach (var curve in res)
                {
                    if (curve is Polyline2d polyline2d)
                    {
                        var pline = new Polyline();
                        pline.ConvertFrom(polyline2d, false);
                        curves.Add(pline);
                    }
                    else
                    {
                        curves.Add(curve.GetTransformedCopy(Matrix3d.Identity) as Curve);
                    }
                }
            }

            return curves;
        }

        /// <summary>
        /// 是否是有效的curve
        /// </summary>
        /// <param name="curveLayer"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool ValidLayer(string curveLayer, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (curveLayer == layerName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否是有效的块
        /// </summary>
        /// <param name="block"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool ValidBlock(BlockReference block, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (block.Layer == layerName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 数据打撒成直线段
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> TesslateCurve2Lines(List<Curve> curves)
        {
            var lines = new List<LineSegment2d>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var ptS = line.StartPoint;
                    var ptE = line.EndPoint;
                    var lineSegment2d = new LineSegment2d(new Point2d(ptS.X, ptS.Y), new Point2d(ptE.X, ptE.Y));
                    lines.Add(lineSegment2d);
                }
                else if (curve is Arc)
                {
                    var arc = curve as Arc;
                    var polyline = arc.Spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Circle)
                {
                    var circle = curve as Circle;
                    var spline = circle.Spline;
                    var polyline = spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Ellipse)
                {
                    var ellipse = curve as Ellipse;
                    var polyline = ellipse.Spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Polyline)
                {
                    var lineNodes = Polyline2Lines(curve as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Spline)
                {
                    var polyline = (curve as Spline).ToPolyline();
                    if (polyline is Polyline)
                    {
                        var lineNodes = Polyline2Lines(polyline as Polyline);
                        if (lineNodes != null)
                            lines.AddRange(lineNodes);
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 多段线转换为直线
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetLineFromPolyline(Polyline polyline)
        {
            if (polyline == null)
                return null;

            var lines = new List<LineSegment2d>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(i);
                        lines.Add(line2d);
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    var bulge = polyline.GetBulgeAt(j);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(j);
                        lines.Add(line2d);
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 多段线转换为直线
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static List<LineSegment2d> Polyline2Lines(Polyline polyline)
        {
            if (polyline == null)
                return null;

            var lines = new List<LineSegment2d>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(i);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var type = polyline.GetSegmentType(i);
                        if (type == SegmentType.Arc)
                        {
                            var arc3d = polyline.GetArcSegmentAt(i);
                            var normal = arc3d.Normal;
                            var axisZ = Vector3d.ZAxis;
                            var arc = new Arc();
                            if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                                arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                            else
                                arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                            var pline = arc.Spline.ToPolyline();
                            var lineNodes = Polyline2Lines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    var bulge = polyline.GetBulgeAt(j);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(j);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var type = polyline.GetSegmentType(j);
                        if (type == SegmentType.Arc)
                        {
                            var arc3d = polyline.GetArcSegmentAt(j);
                            var normal = arc3d.Normal;
                            var axisZ = Vector3d.ZAxis;
                            var arc = new Arc();
                            if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                                arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                            else
                                arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                            var pline = arc.Spline.ToPolyline();
                            var lineNodes = Polyline2Lines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }

            return lines;
        }

        /// <summary>
        ///  计算curves的外包边界
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromCurves(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            // 线数据处理
            var ptLst = new List<XY>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var ptS = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                    var ptE = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                    ptLst.Add(new XY(line.StartPoint.X, line.StartPoint.Y));
                    ptLst.Add(new XY(line.EndPoint.X, line.EndPoint.Y));
                }
                else
                {
                    var minPt = curve.Bounds.Value.MinPoint;
                    var maxPt = curve.Bounds.Value.MaxPoint;
                    ptLst.Add(new XY(minPt.X, minPt.Y));
                    ptLst.Add(new XY(maxPt.X, maxPt.Y));
                }
            }

            var leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
            var rightTop = new XY(ptLst[0].X, ptLst[0].Y);
            for (int i = 1; i < ptLst.Count; i++)
            {
                if (leftBottom.X > ptLst[i].X)
                    leftBottom.X = ptLst[i].X;
                if (leftBottom.Y > ptLst[i].Y)
                    leftBottom.Y = ptLst[i].Y;

                if (rightTop.X < ptLst[i].X)
                    rightTop.X = ptLst[i].X;
                if (rightTop.Y < ptLst[i].Y)
                    rightTop.Y = ptLst[i].Y;
            }

            var boundLines = new List<LineSegment2d>();
            var pt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var pt3 = new Point2d(rightTop.X, rightTop.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        /// <summary>
        ///  计算curves的外包边界
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromCurvesExtend(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            // 线数据处理
            var ptLst = new List<XY>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var ptS = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                    var ptE = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                    ptLst.Add(new XY(line.StartPoint.X, line.StartPoint.Y));
                    ptLst.Add(new XY(line.EndPoint.X, line.EndPoint.Y));
                }
                else
                {
                    var minPt = curve.Bounds.Value.MinPoint;
                    var maxPt = curve.Bounds.Value.MaxPoint;
                    ptLst.Add(new XY(minPt.X, minPt.Y));
                    ptLst.Add(new XY(maxPt.X, maxPt.Y));
                }
            }

            var leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
            var rightTop = new XY(ptLst[0].X, ptLst[0].Y);
            for (int i = 1; i < ptLst.Count; i++)
            {
                if (leftBottom.X > ptLst[i].X)
                    leftBottom.X = ptLst[i].X;
                if (leftBottom.Y > ptLst[i].Y)
                    leftBottom.Y = ptLst[i].Y;

                if (rightTop.X < ptLst[i].X)
                    rightTop.X = ptLst[i].X;
                if (rightTop.Y < ptLst[i].Y)
                    rightTop.Y = ptLst[i].Y;
            }

            var extendValue = 2;
            var boundLines = new List<LineSegment2d>();
            var pt1 = new Point2d(leftBottom.X - extendValue, leftBottom.Y - extendValue);
            var pt3 = new Point2d(rightTop.X + extendValue, rightTop.Y + extendValue);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        /// <summary>
        /// 获取指定图层中连通区域的包围盒边界处理
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromLayerBlocksAndCurves(List<string> layerNames)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                // 块轮廓
                var blocks = db.ModelSpace.OfType<BlockReference>();

                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var layerTableRecord = db.Element<LayerTableRecord>(block.LayerId);
                        if (layerTableRecord.IsFrozen)
                            continue;

                        var blockCurves = GetCurvesFromBlock(block);
                        var lines = GetBoundFromCurves(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

                        blockBounds.Add(lines);
                    }
                }

                var layerCurves = db.ModelSpace.OfType<Curve>().Where(l =>
                {
                    var layerTrue = false;
                    var curLayer = l.Layer;
                    foreach (var layerName in layerNames)
                    {
                        if (curLayer == layerName)
                        {
                            layerTrue = true;
                            break;
                        }
                    }

                    if (layerTrue)
                    {
                        var layerTableRecord = db.Element<LayerTableRecord>(l.LayerId);
                        if (layerTableRecord.IsFrozen)
                            return false;
                        else
                            return true;
                    }

                    return false;
                }).ToList<Curve>();

                // 线数据处理
                if (layerCurves != null && layerCurves.Count != 0)
                {
                    foreach (var curve in layerCurves)
                    {
                        if (curve is Line)
                        {
                            var line = curve as Line;
                            var ptS = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                            var ptE = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                            blockBounds.Add(new List<LineSegment2d>() { new LineSegment2d(ptS, ptE) });
                        }
                        else
                        {
                            var curveBounds = Utils.GetRectFromBound(curve.Bounds.Value);
                            blockBounds.Add(curveBounds);
                        }
                    }
                }
            }

            // 生成相邻框的数据，框边缘是外扩的， 上面的框边缘都是原始的，不经过外扩的数据
            var connectBounds = ConnectedBoxBound.MakeRelatedBoundary(blockBounds);

            return connectBounds;
        }

        /// <summary>
        /// 获取wind图层中的curves
        /// </summary>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static List<Curve> GetWindDOORCurves(List<string> layerNames)
        {
            List<Curve> layerCurves = null;
            using (var db = AcadDatabase.Active())
            {
                layerCurves = db.ModelSpace.OfType<Curve>().Where(l =>
                {
                    var layerTrue = false;
                    var curLayer = l.Layer;
                    foreach (var layerName in layerNames)
                    {
                        if (curLayer == layerName)
                        {
                            layerTrue = true;
                            break;
                        }
                    }

                    if (layerTrue)
                    {
                        var layerTableRecord = db.Element<LayerTableRecord>(l.LayerId);
                        if (layerTableRecord.IsFrozen)
                            return false;
                        else
                            return true;
                    }

                    return false;
                }).ToList<Curve>();
            }

            return layerCurves;
        }

        /// <summary>
        /// 获取指定图层中连通区域的包围盒边界处理，WIND图层做特殊处理，WIND图层中非块数据做先做墙处理，后面再改过来
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromWINDLayerCurves(List<string> layerNames)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                // 块轮廓
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var layerTableRecord = db.Element<LayerTableRecord>(block.LayerId);
                        if (layerTableRecord.IsFrozen)
                            continue;

                        var blockCurves = GetCurvesFromAEWINDBlock(block);
                        var lines = GetBoundFromCurves(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

                        blockBounds.Add(lines);
                    }
                }
            }

            // 生成相邻框的数据，框边缘是外扩的， 上面的框边缘都是原始的，不经过外扩的数据
            // 不同的wind块的边框需要计算合并区域
            var connectBounds = ConnectedBoxBound.MakeRelatedBoundary(blockBounds);
            return connectBounds;
        }

        public static List<List<LineSegment2d>> GetBoundsFromDOORLayerCurves(List<string> layerNames)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                // 块轮廓
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var layerTableRecord = db.Element<LayerTableRecord>(block.LayerId);
                        if (layerTableRecord.IsFrozen)
                            continue;

                        var blockCurves = GetCurvesFromBlock(block);
                        var lines = GetBoundFromCurvesExtend(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

                        blockBounds.Add(lines);
                    }
                }
            }

            //foreach (var curves in blockBounds)
            //{
            //    Utils.DrawLineSegments(curves, "curves");
            //}

            // 这里的门的块的区域不计算不同门之间的合并区域
            return blockBounds;
        }

        /// <summary>
        /// 线是否与曲线集相交
        /// </summary>
        /// <param name="srcLines"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool IsLinesIntersectWithLine(List<LineSegment2d> srcLines, LineSegment2d line)
        {
            foreach (var srcline in srcLines)
            {
                var intersectPts = srcline.IntersectWith(line);
                if (intersectPts != null && intersectPts.Count() != 0)
                    return true;
            }

            return false;
        }


        // 直线处理
        public static List<LineSegment2d> BoundRelatedBoundLines(List<LineSegment2d> doorBound, List<Curve> wallCurves)
        {
            var relatedLines = new List<LineSegment2d>();
            foreach (var curve in wallCurves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var line2d = new LineSegment2d(new Point2d(line.StartPoint.X, line.StartPoint.Y), new Point2d(line.EndPoint.X, line.EndPoint.Y));
                    if (IsLinesIntersectWithLine(doorBound, line2d) || (CommonUtils.PointInnerEntity(doorBound, line2d.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line2d.EndPoint)))
                    {
                        relatedLines.Add(line2d);
                    }
                }
            }

            return relatedLines;
        }

        public static List<Curve> BoundRelatedBoundCurves(List<LineSegment2d> doorBound, List<Curve> wallCurves)
        {
            var relatedCurves = new List<Curve>();
            foreach (var curve in wallCurves)
            {
                if (curve is Line line)
                {
                    var line2d = new LineSegment2d(new Point2d(line.StartPoint.X, line.StartPoint.Y), new Point2d(line.EndPoint.X, line.EndPoint.Y));
                    if (IsLinesIntersectWithLine(doorBound, line2d) || (CommonUtils.PointInnerEntity(doorBound, line2d.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line2d.EndPoint)))
                    {
                        relatedCurves.Add(line);
                    }
                }
                else if (curve is Arc arc)
                {
                    var arcS = arc.StartPoint;
                    var arcE = arc.EndPoint;
                    var arcMid = arc.GetPointAtParameter(0.5 * (arc.StartParam + arc.EndParam));
                    var arc2s = new Point2d(arcS.X, arcS.Y);
                    var arc2mid = new Point2d(arcMid.X, arcMid.Y);
                    var arc2e = new Point2d(arcE.X, arcE.Y);
                    CircularArc2d ar = new CircularArc2d(arc2s, arc2mid, arc2e);

                    if (IsLinesIntersectWithArc(doorBound, ar)
                        || (CommonUtils.PointInnerEntity(doorBound, arc2s) && CommonUtils.PointInnerEntity(doorBound, arc2mid) && CommonUtils.PointInnerEntity(doorBound, arc2e)))
                    {
                        relatedCurves.Add(arc);
                    }
                }
            }

            return relatedCurves;
        }

        public static bool IsLinesIntersectWithArc(List<LineSegment2d> srcLines, CircularArc2d arc2d)
        {
            foreach (var srcline in srcLines)
            {
                var intersectPts = arc2d.IntersectWith(srcline);
                if (intersectPts != null && intersectPts.Count() != 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 共线处理
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        public static bool IsCollinearLines(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
            {
                var ptFirS = lineFir.StartPoint;
                var ptSecS = lineSec.StartPoint;
                var vector = ptFirS - ptSecS;
                var angle3 = lineFir.Direction.GetAngleTo(vector);
                var angle4 = lineFir.Direction.GetAngleTo(vector.Negate());

                // 共线
                if (CommonUtils.IsAlmostNearZero(Math.Abs(angle3), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle4), 1e-5))
                {
                    // 共线直线之间的距离不能太小
                    var ptFirE = lineFir.EndPoint;
                    var ptSecE = lineSec.EndPoint;
                    if (CommonUtils.IsPointOnLine(ptSecS.toPoint3d(), new Line(ptFirS.toPoint3d(), ptFirE.toPoint3d()), 1e-4)
                        || CommonUtils.IsPointOnLine(ptSecE.toPoint3d(), new Line(ptFirS.toPoint3d(), ptFirE.toPoint3d()), 1e-4))
                    {
                        return false;
                    }

                    if (ptSecS.GetDistanceTo(ptFirS) > 500 && ptSecS.GetDistanceTo(ptFirE) > 500 && ptSecE.GetDistanceTo(ptFirS) > 500 && ptSecE.GetDistanceTo(ptFirE) > 500)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否有平行共线的数据，如果没有，则进行相交处理
        /// </summary>
        /// <param name="relatedDatas"></param>
        /// <returns> 没有平行共线的处理则返回true</returns>
        public static bool IsNotHaveCollinearDatas(List<CollinearData> relatedDatas)
        {
            foreach (var relatedData in relatedDatas)
            {
                // 只要有false则说明有共线的处理
                if (!relatedData.Valid)
                    return false;
            }

            return true;
        }

        //共线数据生成
        public static List<Curve> InsertConnectCurves(List<LineSegment2d> line2ds, List<LineSegment2d> profileBounds, string layer = null)
        {
            var relatedDatas = new List<CollinearData>();
            foreach (var line in line2ds)
            {
                relatedDatas.Add(new CollinearData(line));
            }

            for (int i = 0; i < relatedDatas.Count; i++)
            {
                if (!relatedDatas[i].Valid)
                    continue;

                for (int j = i + 1; j < relatedDatas.Count; j++)
                {
                    if (!relatedDatas[j].Valid)
                        continue;

                    //共线
                    if (IsCollinearLines(relatedDatas[i].Line, relatedDatas[j].Line))
                    {
                        var lineFir = relatedDatas[i].Line;
                        var ptFirS = lineFir.StartPoint;
                        var ptFirE = lineFir.EndPoint;

                        var lineSec = relatedDatas[j].Line;
                        var ptSecS = lineSec.StartPoint;
                        var ptSecE = lineSec.EndPoint;


                        relatedDatas[i].RelatedCollinearLines.Add(((LineSegment2d)relatedDatas[j].Line.Clone()));
                        relatedDatas[j].Valid = false;
                    }
                }
            }

            // 如果没有平行线，则做相交处理
            if (IsNotHaveCollinearDatas(relatedDatas))
            {
                var innerLinesLst = new List<List<LineSegment2d>>();
                relatedDatas.ForEach(p =>
                {
                    innerLinesLst.Add(new List<LineSegment2d>() { p.Line });
                });

                var relatedCurvesLst = ConnectedBoxBound.MakeRelatedCurves(innerLinesLst);
                if (relatedCurvesLst.Count > 1)
                {
                    var insertCurves = new List<Curve>();
                    // 一一匹配
                    var nCount = relatedCurvesLst.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        var curvesFir = relatedCurvesLst[i];
                        for (int j = i + 1; j < nCount; j++)
                        {
                            var curvesSec = relatedCurvesLst[j];
                            var tmpInsertCurves = ConnectLinesWithsLines(curvesFir, curvesSec, layer);
                            if (tmpInsertCurves != null && tmpInsertCurves.Count != 0)
                                insertCurves.AddRange(tmpInsertCurves);
                        }
                    }

                    // 没有相交的直线则进行普通连接线
                    if (insertCurves.Count == 0)
                    {
                        for (int i = 0; i < nCount; i++)
                        {
                            var curvesFir = relatedCurvesLst[i];

                            for (int j = i + 1; j < nCount; j++)
                            {
                                var curvesSec = relatedCurvesLst[j];
                                var tmpInsertCurves = ConnectLinesWithsLinesWithPoints(curvesFir, curvesSec, profileBounds, layer);
                                if (tmpInsertCurves != null && tmpInsertCurves.Count != 0)
                                    insertCurves.AddRange(tmpInsertCurves);
                            }
                        }
                    }

                    return insertCurves;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // 有共线则生成共线数据
                var curves = new List<Curve>();
                foreach (var data in relatedDatas)
                {
                    if (data.Valid)
                    {
                        var line = MakeLineFromCollinears(data);
                        if (line != null)
                        {
                            if (layer != null)
                                line.Layer = layer;
                            curves.Add(line);
                        }
                    }
                }

                return curves;
            }
        }

        public static ObjectIdCollection PreProcessCurDwg3(List<string> validLayers, Extents3d extents)
        {
            var objs = new ObjectIdCollection();
            double progressPos = 1;
            // 本图纸数据块处理
            using (var db = AcadDatabase.Active())
            {
                var blockRefs = db.CurrentSpace.OfType<BlockReference>().Where(p => p.Visible).ToList();
                if (blockRefs.Count == 0)
                {
                    return objs;
                }
                var incre = 390.0 / blockRefs.Count;
                foreach (var blockReference in blockRefs)
                {
                    var layerId = blockReference.LayerId;
                    if (layerId == null || !layerId.IsValid)
                        continue;

                    LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(layerId);
                    if (layerTableRecord.IsOff && layerTableRecord.IsFrozen)
                        continue;

                    var blockId = blockReference.BlockTableRecord;
                    var blockRefRecord = db.Element<BlockTableRecord>(blockId);
                    if (blockRefRecord.IsFromExternalReference)
                        continue;

                    progressPos += incre;
                    Progress.Progress.SetValue((int)progressPos);

                    List<Entity> entityLst = null;
                    entityLst = GetEntityFromBlock2(blockReference);
                    var entities = new List<Entity>();
                    if (entityLst != null && entityLst.Count > 0)
                    {
                        if (entityLst.Count == 1)
                        {
                            var entity = entityLst.First();
                            if (!(entity is BlockReference))
                            {
                                if (!entity.Equals(blockReference)
                                    && IsValidLayer(entity, validLayers)
                                    && IsInValidExtents(entity, extents)
                                    && !entity.IsErased
                                    && !(entity is Hatch)
                                    && !(entity is AttributeDefinition))
                                {
                                    if (entity is Region region)
                                    {
                                        var dbObjects = new DBObjectCollection();
                                        region.Explode(dbObjects);
                                        foreach (var obj in dbObjects)
                                        {
                                            if (obj is Entity entityRe)
                                            {
                                                entities.Add(entityRe);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        entities.Add(entity);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var entity in entityLst)
                            {
                                // 除了块，其余的可以用图层确定是否收集，块的图层内部数据也可能符合要求
                                if (!entity.Equals(blockReference)
                                    && IsValidLayer(entity, validLayers)
                                    && IsInValidExtents(entity, extents)
                                    && !entity.IsErased
                                    && !(entity is Hatch)
                                    && !(entity is AttributeDefinition))
                                {
                                    if (entity is Region region)
                                    {
                                        var dbObjects = new DBObjectCollection();
                                        region.Explode(dbObjects);
                                        foreach (var obj in dbObjects)
                                        {
                                            if (obj is Entity entityRe)
                                            {
                                                entities.Add(entityRe);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        entities.Add(entity);
                                    }
                                }
                            }
                        }
                    }

                    // 将目标图元添加到图纸中
                    foreach (Entity entity in entities)
                    {
                        objs.Add(db.ModelSpace.Add(entity));
                    }

                    // 释放不用的图元对象
                    if (entityLst != null)
                    {
                        foreach (Entity entity in entityLst.Where(o => !entities.Contains(o)))
                        {
                            entity.Dispose();
                        }
                    }
                }
            }

            return objs;
        }

        public static ObjectIdCollection PreProcessCurDwg2(List<string> validLayers)
        {
            var objs = new ObjectIdCollection();
            double progressPos = 1;
            // 本图纸数据块处理
            using (var db = AcadDatabase.Active())
            {
                var blockRefs = db.CurrentSpace.OfType<BlockReference>().Where(p => p.Visible).ToList();
                if (blockRefs.Count == 0)
                {
                    return objs;
                }
                var incre = 390.0 / blockRefs.Count;
                foreach (var blockReference in blockRefs)
                {
                    var layerId = blockReference.LayerId;
                    if (layerId == null || !layerId.IsValid)
                        continue;

                    LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(layerId);
                    if (layerTableRecord.IsOff && layerTableRecord.IsFrozen)
                        continue;

                    var blockId = blockReference.BlockTableRecord;
                    var blockRefRecord = db.Element<BlockTableRecord>(blockId);
                    if (blockRefRecord.IsFromExternalReference)
                        continue;

                    progressPos += incre;
                    Progress.Progress.SetValue((int)progressPos);

                    List<Entity> entityLst = null;
                    entityLst = GetEntityFromBlock2(blockReference);
                    var entities = new List<Entity>();
                    if (entityLst != null && entityLst.Count > 0)
                    {
                        if (entityLst.Count == 1)
                        {
                            var entity = entityLst.First();
                            if (!(entity is BlockReference))
                            {
                                if (!entity.Equals(blockReference)
                                    && IsValidLayer(entity, validLayers)
                                    && !entity.IsErased
                                    && !(entity is Hatch)
                                    && !(entity is AttributeDefinition)
                                    && !(entity is Wipeout))
                                {
                                    if (entity is Region || entity is Mline)
                                    {
                                        var dbObjects = new DBObjectCollection();
                                        entity.Explode(dbObjects);
                                        foreach (var obj in dbObjects)
                                        {
                                            if (obj is Entity entityRe)
                                            {
                                                entities.Add(entityRe);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        entities.Add(entity);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var entity in entityLst)
                            {
                                // 除了块，其余的可以用图层确定是否收集，块的图层内部数据也可能符合要求
                                if (!entity.Equals(blockReference)
                                    && IsValidLayer(entity, validLayers)
                                    && !entity.IsErased
                                    && !(entity is Hatch)
                                    && !(entity is AttributeDefinition)
                                    && !(entity is Wipeout))
                                {
                                    if (entity is Region || entity is Mline)
                                    {
                                        var dbObjects = new DBObjectCollection();
                                        entity.Explode(dbObjects);
                                        foreach (var obj in dbObjects)
                                        {
                                            if (obj is Entity entityRe)
                                            {
                                                entities.Add(entityRe);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        entities.Add(entity);
                                    }
                                }
                            }
                        }
                    }

                    // 将目标图元添加到图纸中
                    foreach (Entity entity in entities)
                    {
                        try
                        {
                            objs.Add(db.ModelSpace.Add(entity));
                        }
                        catch (Exception e)
                        { }
                    }

                    // 释放不用的图元对象
                    if (entityLst != null)
                    {
                        foreach (Entity entity in entityLst.Where(o => !entities.Contains(o)))
                        {
                            entity.Dispose();
                        }
                    }
                }
            }

            return objs;
        }

        public static List<Entity> PreProcessCurDwg(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            double progressPos = 5;
            // 本图纸数据块处理
            using (var db = AcadDatabase.Active())
            {
                var blockRefs = db.CurrentSpace.OfType<BlockReference>().Where(p => p.Visible).ToList();
                var incre = 15 / blockRefs.Count;
                foreach (var blockReference in blockRefs)
                {
                    var layerId = blockReference.LayerId;
                    if (layerId == null || !layerId.IsValid)
                        continue;

                    LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(layerId);
                    if (layerTableRecord.IsOff)
                        continue;

                    var blockId = blockReference.BlockTableRecord;
                    var blockRefRecord = db.Element<BlockTableRecord>(blockId);
                    if (blockRefRecord.IsFromExternalReference)
                        continue;
                    progressPos += 0.4;
                    progressPos += incre;
                    Progress.Progress.SetValue((int)progressPos);
                    var entityLst = GetEntityFromBlock(blockReference);
                    if (entityLst != null && entityLst.Count > 1)
                    {
                        foreach (var entity in entityLst)
                        {
                            // 除了块，其余的可以用图层确定是否收集，块的图层内部数据也可能符合要求
                            if (!entity.Equals(blockReference) && IsValidLayer(entity, validLayers))
                            {
                                db.CurrentSpace.Add(entity);
                                resEntityLst.Add(entity);
                            }
                        }
                    }
                }
            }

            return resEntityLst;
        }

        public static bool IsValidLayer(Entity entity, List<string> validLayers)
        {
            if (validLayers.Count == 0)
                return false;
            if (entity is BlockReference)
                return true;

            foreach (var layer in validLayers)
            {
                if (entity.Layer.Contains(layer))
                    return true;
            }
            return false;
        }

        public static bool IsInValidExtents(Entity entity, Extents3d extents)
        {
            try
            {
                Plane XYPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
                Matrix3d matrix = Matrix3d.Projection(XYPlane, XYPlane.Normal);
                Extents2d extents2d = new Extents2d(
                    extents.MinPoint.TransformBy(matrix).ToPoint2d(),
                    extents.MaxPoint.TransformBy(matrix).ToPoint2d());
                return extents2d.IsPointIn(entity.GeometricExtents.MinPoint.TransformBy(matrix).ToPoint2d())
                    && extents2d.IsPointIn(entity.GeometricExtents.MaxPoint.TransformBy(matrix).ToPoint2d());
            }
            catch
            {
                return true;
            }
        }

        public static bool IsLinesIntersectWithLines(List<LineSegment2d> srcLines, List<LineSegment2d> rectLines)
        {
            foreach (var rectLine in rectLines)
            {
                foreach (var line in srcLines)
                {
                    var intersectPts = line.IntersectWith(rectLine);
                    if (intersectPts != null && intersectPts.Count() != 0)
                        return true;
                }
            }

            return false;
        }

        public static void PostProcess(ObjectIdCollection objs)
        {
            using (var db = AcadDatabase.Active())
            {
                foreach (ObjectId obj in objs)
                {
                    try
                    {
                        var entity = db.Element<Entity>(obj, true);
                        var entityLayer = db.Element<LayerTableRecord>(entity.LayerId);
                        if (entityLayer.IsLocked)
                            continue;
                        entity.Erase();
                    }
                    catch (Exception e)
                    { }
                }
            }
            Active.Database.ReclaimMemoryFromErasedObjects(objs);
        }

        public static bool IsValidBlockReference(BlockReference block, List<LineSegment2d> rectLines)
        {
            var blockCurves = GetCurvesFromBlock(block, false);
            var lines = GetBoundFromCurves(blockCurves);
            if (lines == null || lines.Count == 0)
                return false;

            if (CommonUtils.OutLoopContainsInnerLoop(rectLines, lines) || IsLinesIntersectWithLines(lines, rectLines))
                return true;

            return false;
        }

        /// <summary>
        /// 从block单个数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Entity> GetEntityFromBlock(BlockReference block)
        {
            if (block == null || !block.Visible)
                return null;

            var entityLst = new List<Entity>();
            var blockReferences = new List<BlockReference>();
            blockReferences.Add(block);
            while (blockReferences.Count > 0)
            {
                var curBlock = blockReferences.First();
                blockReferences.RemoveAt(0);

                if (curBlock.Visible)
                {
                    var entitysInBlock = FromSingleBlock(curBlock, ref blockReferences);
                    if (entitysInBlock != null && entitysInBlock.Count != 0)
                    {
                        entityLst.AddRange(entitysInBlock);
                    }
                }
            }

            return entityLst;
        }

        /// <summary>
        /// 从block单个数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Entity> GetEntityFromBlock2(BlockReference block)
        {
            if (block == null || !block.Visible)
                return null;

            var entityLst = new List<Entity>();
            var blockReferences = new List<BlockReference>();
            blockReferences.Add(block);

            while (blockReferences.Count > 0)
            {
                var curBlock = blockReferences.First();
                blockReferences.RemoveAt(0);

                if (curBlock.Visible)
                {
                    var entitysInBlock = FromSingleBlock2(curBlock, ref blockReferences);
                    if (entitysInBlock != null && entitysInBlock.Count != 0)
                    {
                        entityLst.AddRange(entitysInBlock);
                    }
                }
            }

            return entityLst;
        }

        /// <summary>
        /// 是否是其他标准图层
        /// </summary>
        /// <param name="srcLayerName"></param>
        /// <returns></returns>
        public static bool IsOtherStandardLayer(string srcLayerName)
        {
            if (srcLayerName.Contains("AE-EQPM")
                || srcLayerName.Contains("AE-ABOV")
                || srcLayerName.Contains("AE-FLOR")
                || srcLayerName.Contains("AE-STAR")
                || srcLayerName.Contains("AE-ROOF")
                || srcLayerName.Contains("AE-HOLE")
                || srcLayerName.Contains("AE-ELEV")
                || srcLayerName.Contains("AE-PATN")
                || srcLayerName.Contains("AD-AXIS")
                || srcLayerName.Contains("AD-ARCH")
                || srcLayerName.Contains("AD-DIMS")
                || srcLayerName.Contains("AE-SUFC")
                || srcLayerName.Contains("AD-NAME")
                || srcLayerName.Contains("AD-NUMB")
                || srcLayerName.Contains("AD-AREA")
                || srcLayerName.Contains("AD-INDX")
                || srcLayerName.Contains("AD-SIGN")
                || srcLayerName.Contains("AD-LEVL")
                || srcLayerName.Contains("AD-NOTE")
                || srcLayerName.Contains("AD-POST")
                || srcLayerName.Contains("AE-PIPE")
                || srcLayerName.Contains("S_BEAM")
                || srcLayerName.Contains("S_FLOR")
                || srcLayerName.Contains("S_STAR")
                || srcLayerName.Contains("S_HOLE")
                || srcLayerName.Contains("S_PILE")
                || srcLayerName.Contains("S_AXIS")
                || srcLayerName.Contains("S_PLAN")
                || srcLayerName.Contains("S_BASE")
                || srcLayerName.Contains("S_LEVL")
                || srcLayerName.Contains("S_BURY")
                || srcLayerName.Contains("S_STEL")
                || srcLayerName.Contains("S_DETL")
                || srcLayerName.Contains("S_PSPR")
                || srcLayerName.Contains("S_OBSV")
                || srcLayerName.Contains("S_INDX")
                || srcLayerName.Contains("S_CONS")
                || srcLayerName.Contains("S_TABL")
                || srcLayerName.Contains("S_HELP")
                || srcLayerName.Contains("C-SHET")
                || srcLayerName.Contains("H-DUCT")
                || srcLayerName.Contains("H-DAPP")
                || srcLayerName.Contains("H-DIMS")
                || srcLayerName.Contains("H-DUAL")
                || srcLayerName.Contains("H-FIRE")
                || srcLayerName.Contains("H-PIPE")
                || srcLayerName.Contains("H-VALV")
                || srcLayerName.Contains("H-PAPP")
                || srcLayerName.Contains("H-EQUP")
                || srcLayerName.Contains("H-DIMS")
                || srcLayerName.Contains("H-AI")
                || srcLayerName.Contains("H-BUSH")
                || srcLayerName.Contains("H-HOLE")
                || srcLayerName.Contains("H-BASE")
                || srcLayerName.Contains("H-HOBU")
                || srcLayerName.Contains("H-HOLE")
                || srcLayerName.Contains("D-DUCT")
                || srcLayerName.Contains("D-PIPE")
                || srcLayerName.Contains("D-EQUP")
                || srcLayerName.Contains("D-AI")
                || srcLayerName.Contains("D-BUSH")
                || srcLayerName.Contains("D-HOLE")
                || srcLayerName.Contains("D-BASE")
                || srcLayerName.Contains("D-HOBU")
                || srcLayerName.Contains("D-DIMS")
                || srcLayerName.Contains("C-SHET")
                || srcLayerName.Contains("E-UNIV")
                || srcLayerName.Contains("E-REQU")
                || srcLayerName.Contains("E-POWR")
                || srcLayerName.Contains("E-GRND")
                || srcLayerName.Contains("E-THUN")
                || srcLayerName.Contains("E-LITE")
                || srcLayerName.Contains("E-TELE")
                || srcLayerName.Contains("E-CTRL")
                || srcLayerName.Contains("E-FAS")
                || srcLayerName.Contains("E-EFPS")
                || srcLayerName.Contains("E-GCS")
                || srcLayerName.Contains("E-CATV")
                || srcLayerName.Contains("E-IAS")
                || srcLayerName.Contains("E-VSCS")
                || srcLayerName.Contains("E-PLMS")
                || srcLayerName.Contains("E-BRST")
                || srcLayerName.Contains("E-MBF")
                || srcLayerName.Contains("E-GTS")
                || srcLayerName.Contains("E-BAS")
                || srcLayerName.Contains("E-MOBL")
                || srcLayerName.Contains("SP_OST")
                || srcLayerName.Contains("S_PLAN")
                || srcLayerName.Contains("S_AXIS")
                || srcLayerName.Contains("S_HELP")
                || srcLayerName.Contains("S_DETL")
                || srcLayerName.Contains("S_LEVL")
                || srcLayerName.Contains("S_BURY"))
                return true;

            return false;
        }

        /// <summary>
        /// 是否是封闭空间图层列表
        /// </summary>
        /// <param name="srcLayerName"></param>
        /// <returns></returns>
        public static bool IsValidLoopLayer(string srcLayerName)
        {
            if (IsValidShowLayer(srcLayerName, "AE-WALL")
                || IsValidShowLayer(srcLayerName, "COLU")
                || IsValidShowLayer(srcLayerName, "AD-NAME-ROOM")
                || IsValidShowLayer(srcLayerName, "AE-STRU")
                || IsValidShowLayer(srcLayerName, "AE-HDWR")
                //|| IsValidShowLayer(srcLayerName, "AE-FLOR")
                || IsValidShowLayer(srcLayerName, "S_WALL")
                || IsValidShowLayer(srcLayerName, "S_WALL_DETL")
                || IsValidShowLayer(srcLayerName, "S_BRIK")
                || IsValidShowLayer(srcLayerName, "AE-FNSH")
                || IsValidShowLayer(srcLayerName, "AE-DOOR-INSD")
                || IsValidShowLayer(srcLayerName, "AE-WIND")
                || IsValidShowLayer(srcLayerName, "-框")
                || IsValidShowLayer(srcLayerName, "AE-SUFC"))
            {
                return true;
            }

            return false;
        }

        public static void AddRegion(Entity srcEntity, string layerName, ref List<Entity> entityLst)
        {
            var dbObjects = new DBObjectCollection();
            srcEntity.Explode(dbObjects);
            foreach (var innerObj in dbObjects)
            {
                if (innerObj is Entity entityRe)
                {
                    entityRe.Layer = layerName;
                    entityLst.Add(entityRe);
                }
            }
        }

        /// <summary>
        /// 判断单个能否炸开，以及返回的数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="childReferences"></param>
        /// <returns></returns>
        public static List<Entity> FromSingleBlock2(BlockReference block, ref List<BlockReference> childReferences)
        {
            if (block == null)
                return null;

            string name;
            var nameMaps = new List<string>();
            try
            {
                using (var db = AcadDatabase.Active())
                {
                    name = block.GetEffectiveName(db.Database.TransactionManager.TopTransaction); // 块的名字

                    if (name.Contains("Window resuce mark"))
                        return null;

                    BlockTableRecord btr = db.Database.TransactionManager.TopTransaction.GetObject(block.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    foreach (ObjectId ociId in btr)
                    {
                        DBObject dbObj = db.Database.TransactionManager.TopTransaction.GetObject(ociId, OpenMode.ForRead);
                        if (dbObj is BlockReference)
                        {
                            var newBr = dbObj as BlockReference;
                            ObjectId blkRecdId = ObjectId.Null;
                            if (newBr.IsDynamicBlock)
                            {
                                blkRecdId = newBr.DynamicBlockTableRecord;
                                BlockTableRecord dynBlockRecord = db.Database.TransactionManager.TopTransaction.GetObject(blkRecdId, OpenMode.ForRead) as BlockTableRecord;
                                if (dynBlockRecord.Name.Contains("door"))
                                {
                                    nameMaps.Add(newBr.Name);
                                }
                            }
                        }
                    }


                    // 解析数据
                    var entityLst = new List<Entity>();
                    var dbCollection = new DBObjectCollection();
                    block.Explode(dbCollection);
                    var blockLayer = block.Layer;
                    if (IsHasBlockReference(dbCollection))
                    {
                        // 内部包含块
                        foreach (var obj in dbCollection)
                        {
                            if (obj is Region || obj is Mline)
                            {
                                var reEntity = obj as Entity;
                                AddRegion(reEntity, blockLayer, ref entityLst);
                                continue;
                            }

                            bool bContinue = false;
                            if (obj is Entity objEntity)
                            {
                                LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(objEntity.LayerId);
                                if (layerTableRecord.IsFrozen && !IsContainsZeroLayer(objEntity.Layer))
                                    continue;

                                if (objEntity is BlockReference)
                                {
                                    var childBlock = obj as BlockReference;
                                    foreach (var keyName in nameMaps)
                                    {
                                        if (keyName.Equals(childBlock.Name))
                                        {
                                            entityLst.Add(childBlock);
                                            bContinue = true;
                                            break;
                                        }
                                    }

                                    if (bContinue)
                                        continue;

                                    if (childBlock.Visible)
                                    {
                                        var childBlockLayer = childBlock.Layer;
                                        if (IsValidLoopLayer(childBlockLayer))
                                            childReferences.Add(childBlock);
                                        else if (!IsOtherStandardLayer(childBlockLayer))
                                        {
                                            childBlock.Layer = blockLayer;
                                            childReferences.Add(childBlock);
                                        }
                                        else if (IsOtherStandardLayer(childBlockLayer))
                                        {
                                            childReferences.Add(childBlock);
                                        }
                                    }
                                }
                                else
                                {
                                    var entity = obj as Entity;
                                    if (entity.Visible)
                                    {
                                        if (layerTableRecord.IsOff && !IsContainsZeroLayer(entity.Layer))
                                            continue;

                                        var entityLayer = entity.Layer;
                                        if (IsValidLoopLayer(entityLayer))
                                            entityLst.Add(entity);
                                        else if (!IsOtherStandardLayer(entityLayer))
                                        {
                                            entity.Layer = blockLayer;
                                            entityLst.Add(entity);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (IsCanExplode(dbCollection, blockLayer))
                    {
                        // 内部不包含块且曲线所在的图层名包含3个以上
                        foreach (var obj in dbCollection)
                        {
                            if (obj is Region || obj is Mline)
                            {
                                var reEntity = obj as Entity;
                                AddRegion(reEntity, blockLayer, ref entityLst);
                            }
                            else if (obj is Entity entity)
                            {
                                LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(entity.LayerId);
                                if ((layerTableRecord.IsFrozen || layerTableRecord.IsOff) && !IsContainsZeroLayer(entity.Layer))
                                    continue;

                                if (entity.Visible)
                                {
                                    var entityLayer = entity.Layer;
                                    if (IsValidLoopLayer(entityLayer))
                                        entityLst.Add(entity);
                                    else if (!IsOtherStandardLayer(entityLayer))
                                    {
                                        entity.Layer = blockLayer;
                                        entityLst.Add(entity);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // 内部不包含块且曲线所在的图层名小于3个图层
                        // 此时这个块不被炸开作为一个整体。
                        if (block.Visible)
                            entityLst.Add(block);
                    }

                    // 释放不需要的图元
                    foreach (Entity obj in dbCollection)
                    {
                        if (childReferences.Contains(obj))
                        {
                            continue;
                        }
                        if (entityLst.Contains(obj))
                        {
                            continue;
                        }
                        obj.Dispose();
                    }

                    // 返回需要的图元
                    return entityLst;
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool IsContainsZeroLayer(string layerName)
        {
            var trimName = layerName.Trim();
            if (trimName.Length == 1 && trimName.Contains("0"))
                return true;

            return false;
        }

        /// <summary>
        /// 判断单个能否炸开，以及返回的数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="childReferences"></param>
        /// <returns></returns>
        public static List<Entity> FromSingleBlock(BlockReference block, ref List<BlockReference> childReferences)
        {
            if (block == null)
                return null;

            var entityLst = new List<Entity>();
            try
            {
                var dbCollection = new DBObjectCollection();
                block.Explode(dbCollection);

                if (IsHasBlockReference(dbCollection)) // 内部包含块
                {
                    foreach (var obj in dbCollection)
                    {
                        if (obj is Entity)
                        {
                            if (obj is BlockReference)
                            {
                                var childBlock = obj as BlockReference;
                                if (childBlock.Visible)
                                    childReferences.Add(childBlock);
                            }
                            else
                            {
                                var entity = obj as Entity;
                                if (entity.Visible)
                                    entityLst.Add(entity);
                            }
                        }
                    }
                }
                else if (IsCanExplode(dbCollection)) // 内部不包含块且曲线所在的图层名包含3个以上
                {
                    foreach (var obj in dbCollection)
                    {
                        if (obj is Entity)
                        {
                            var entity = obj as Entity;
                            if (entity.Visible)
                                entityLst.Add(entity);
                        }
                    }
                }
                else // 内部不包含块且曲线所在的图层名小于3个图层
                {
                    // 此时这个块不被炸开作为一个整体。
                    if (block.Visible)
                        entityLst.Add(block);
                }
            }
            catch
            {
                return null;
            }

            return entityLst;
        }

        /// <summary>
        /// 是否含有块
        /// </summary>
        /// <param name="dbCollection"></param>
        /// <returns></returns>
        public static bool IsHasBlockReference(DBObjectCollection dbCollection)
        {
            if (dbCollection == null || dbCollection.Count == 0)
                return false;

            foreach (var obj in dbCollection)
            {
                if (obj is BlockReference)
                {
                    var curBlock = obj as BlockReference;
                    if (curBlock.Visible)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 一个块中是否有多3个图层
        /// </summary>
        /// <param name="dbCollection"></param>
        /// <returns></returns>
        public static bool IsCanExplode(DBObjectCollection dbCollection, string blockLayer = null)
        {
            var layerNames = new List<string>();
            int ArcCounts = 0;
            bool bInnerValid = false;
            foreach (var obj in dbCollection)
            {
                if (obj is Curve)
                {
                    var curve = obj as Curve;
                    if (curve.Visible)
                    {
                        var curveLayer = curve.Layer;
                        if (!bInnerValid && (curveLayer.Contains("AE-DOOR-INSD")
                            || curveLayer.Contains("AE-WIND")))
                            bInnerValid = true;

                        var layerName = curve.Layer;
                        if (!layerNames.Contains(layerName))
                        {
                            layerNames.Add(layerName);
                            //if (layerNames.Count > 1)
                            //    return true;
                        }

                        if (obj is Arc || obj is Ellipse)
                        {
                            ArcCounts++;
                        }
                    }
                }
                else if (obj is DBText text)
                {
                    if (obj is AttributeDefinition)
                        continue;

                    if (text.Visible && text.Layer.Contains("AD-NAME-ROOM"))
                        return true;
                }
            }

            if (ArcCounts < 1)
                return true;

            // 门里含有0或者文字等多个图层DEFPOINTS
            if (layerNames.Count > 3)
                return true;

            if (!blockLayer.Contains("AE-WIND") && !blockLayer.Contains("AE-DOOR-INSD")
                && bInnerValid)
                return true;

            return false;
        }

        public static ObjectIdCollection PreProcessXREF3(List<string> validLayers, Extents3d extents)
        {
            var objs = new ObjectIdCollection();
            // 外部参照
            double progressPos = 400.0;
            using (var db = AcadDatabase.Active())
            {
                var refs = db.XRefs;
                if (refs.Count() == 0)
                    return objs;

                var incre = 500.0 / refs.Count();

                foreach (var xblock in refs)
                {
                    if (xblock.Block.XrefStatus == XrefStatus.Resolved)
                    {
                        ObjectIdCollection idCollection = new ObjectIdCollection();
                        BlockTableRecord blockTableRecord = xblock.Block;
                        List<BlockReference> blockReferences = blockTableRecord.GetAllBlockReferences(true, true).ToList();
                        for (int i = 0; i < blockReferences.Count(); i++)
                        {
                            var blockReference = blockReferences[i];
                            progressPos += incre;
                            var layerId = blockReference.LayerId;
                            if (layerId == null || !layerId.IsValid)
                                continue;

                            LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(blockReference.LayerId);
                            if (layerTableRecord.IsOff && layerTableRecord.IsFrozen)
                                continue;

                            Progress.Progress.SetValue((int)progressPos);
                            var entities = new List<Entity>();
                            var entityLst = GetEntityFromBlock2(blockReference);
                            if (entityLst != null && entityLst.Count != 0)
                            {
                                foreach (var entity in entityLst)
                                {
                                    if (!entity.Equals(blockReference))
                                    {
                                        if (entity is BlockReference block)
                                        {
                                            if (block.Layer.Contains("AE-DOOR-INSD") || block.Layer.Contains("AE-WIND"))
                                            {
                                                if (IsInValidExtents(block, extents))
                                                {
                                                    entities.Add(entity);
                                                }
                                            }
                                        }
                                        else if (IsValidLayer(entity, validLayers)
                                            && IsInValidExtents(entity, extents)
                                            && !entity.IsErased
                                            && !(entity is Hatch) && !(entity is AttributeDefinition))
                                        {
                                            entities.Add(entity);
                                        }
                                    }
                                }
                            }
                            // 将目标图元添加到图纸中
                            foreach (Entity entity in entities)
                            {
                                objs.Add(db.ModelSpace.Add(entity));
                            }

                            // 释放不用的图元对象
                            if (entityLst != null)
                            {
                                foreach (Entity entity in entityLst.Where(o => !entities.Contains(o)))
                                {
                                    entity.Dispose();
                                }
                            }
                        }
                    }
                }
            }

            return objs;
        }

        public static ObjectIdCollection PreProcessXREF2(List<string> validLayers)
        {
            var objs = new ObjectIdCollection();
            // 外部参照
            double progressPos = 400.0;
            using (var db = AcadDatabase.Active())
            {
                var refs = db.XRefs;
                if (refs.Count() == 0)
                    return objs;

                var incre = 500.0 / refs.Count();

                foreach (var xblock in refs)
                {
                    if (xblock.Block.XrefStatus == XrefStatus.Resolved)
                    {
                        ObjectIdCollection idCollection = new ObjectIdCollection();
                        BlockTableRecord blockTableRecord = xblock.Block;
                        List<BlockReference> blockReferences = blockTableRecord.GetAllBlockReferences(true, true).ToList();
                        for (int i = 0; i < blockReferences.Count(); i++)
                        {
                            var blockReference = blockReferences[i];
                            progressPos += incre;
                            var layerId = blockReference.LayerId;
                            if (layerId == null || !layerId.IsValid)
                                continue;

                            LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(blockReference.LayerId);
                            if (layerTableRecord.IsOff && layerTableRecord.IsFrozen)
                                continue;

                            Progress.Progress.SetValue((int)progressPos);
                            var entities = new List<Entity>();
                            var entityLst = GetEntityFromBlock2(blockReference);
                            if (entityLst != null && entityLst.Count != 0)
                            {
                                foreach (var entity in entityLst)
                                {
                                    if (!entity.Equals(blockReference))
                                    {
                                        if (entity is BlockReference block)
                                        {
                                            if (block.Layer.Contains("AE-DOOR-INSD") || block.Layer.Contains("AE-WIND"))
                                            {
                                                entities.Add(entity);
                                            }
                                        }
                                        else if (IsValidLayer(entity, validLayers)
                                            && !entity.IsErased
                                            && !(entity is Hatch) && !(entity is AttributeDefinition) && !(entity is Wipeout))
                                        {
                                            if (entity is Region || entity is Mline)
                                            {
                                                var dbObjects = new DBObjectCollection();
                                                entity.Explode(dbObjects);
                                                foreach (var obj in dbObjects)
                                                {
                                                    if (obj is Entity entityRe)
                                                    {
                                                        entities.Add(entityRe);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                entities.Add(entity);
                                            }
                                        }
                                    }
                                }
                            }
                            // 将目标图元添加到图纸中
                            foreach (Entity entity in entities)
                            {
                                try
                                {
                                    var id = db.ModelSpace.Add(entity);
                                    objs.Add(id);
                                }
                                catch (Exception e)
                                {
                                }
                            }

                            // 释放不用的图元对象
                            if (entityLst != null)
                            {
                                foreach (Entity entity in entityLst.Where(o => !entities.Contains(o)))
                                {
                                    entity.Dispose();
                                }
                            }
                        }
                    }
                }
            }

            return objs;
        }

        public static List<Entity> PreProcessXREF(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            // 外部参照
            double progressPos = 25;
            using (var db = AcadDatabase.Active())
            {
                var refs = db.XRefs;
                var incre = 15.0 / refs.Count();

                foreach (var xblock in refs)
                {
                    if (xblock.Block.XrefStatus == XrefStatus.Resolved)
                    {
                        ObjectIdCollection idCollection = new ObjectIdCollection();
                        BlockTableRecord blockTableRecord = xblock.Block;
                        List<BlockReference> blockReferences = blockTableRecord.GetAllBlockReferences(true, true).ToList();
                        for (int i = 0; i < blockReferences.Count(); i++)
                        {
                            var blockReference = blockReferences[i];
                            progressPos += 0.4;
                            progressPos += incre;
                            Progress.Progress.SetValue((int)progressPos);
                            var entityLst = GetEntityFromBlock(blockReference);
                            if (entityLst != null && entityLst.Count != 0)
                            {
                                foreach (var entity in entityLst)
                                {
                                    if (!entity.Equals(blockReference))
                                    {
                                        try
                                        {
                                            if (entity is DBText || entity is MText)
                                            {
                                                continue;
                                            }
                                            if (entity is BlockReference)
                                            {
                                                // 块里面的数据可能是有效单元， 剔除文字的影响
                                                var reference = entity as BlockReference;
                                                var dbCollection = new DBObjectCollection();
                                                reference.Explode(dbCollection);
                                                foreach (var part in dbCollection)
                                                {
                                                    if (part is DBText || part is MText)
                                                    {
                                                        continue;
                                                    }
                                                    else if (part is Entity)
                                                    {
                                                        var partEntity = part as Entity;
                                                        partEntity.Layer = reference.Layer;
                                                        db.CurrentSpace.Add(partEntity);
                                                        resEntityLst.Add(partEntity);
                                                    }
                                                }
                                            }
                                            else if (IsValidLayer(entity, validLayers))
                                            {
                                                db.CurrentSpace.Add(entity);
                                                resEntityLst.Add(entity);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resEntityLst;
        }

        /// <summary>
        /// 图纸预处理
        /// </summary>
        public static List<Entity> PreProcess(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            var curEntityLst = PreProcessCurDwg(validLayers);

            if (curEntityLst.Count != 0)
                resEntityLst.AddRange(curEntityLst);

            var xRefEntityLst = PreProcessXREF(validLayers);
            if (xRefEntityLst.Count != 0)
                resEntityLst.AddRange(xRefEntityLst);
            return resEntityLst;
        }

        /// <summary>
        /// 图纸预处理
        /// </summary>
        public static ObjectIdCollection PreProcess2(List<string> validLayers)
        {
            var objs = new ObjectIdCollection();
            foreach (ObjectId obj in PreProcessCurDwg2(validLayers))
            {
                objs.Add(obj);
            }
            foreach (ObjectId obj in PreProcessXREF2(validLayers))
            {
                objs.Add(obj);
            }

            foreach (ObjectId obj in PreProcessRegion(validLayers))
            {
                objs.Add(obj);
            }
            return objs;
        }

        public static ObjectIdCollection PreProcessRegion(List<string> validLayers)
        {
            var objs = new ObjectIdCollection();
            using (var db = AcadDatabase.Active())
            {
                var regions = db.ModelSpace.OfType<Region>().Where(p =>
               {
                   foreach (var layer in validLayers)
                   {
                       if (layer.Contains(p.Layer))
                           return true;
                   }

                   return false;
               }).ToList();

                foreach (var region in regions)
                {
                    var dbObjects = new DBObjectCollection();
                    region.Explode(dbObjects);
                    foreach (var obj in dbObjects)
                    {
                        if (obj is Entity entityRe)
                            objs.Add(db.ModelSpace.Add(entityRe));
                    }
                }
            }

            return objs;
        }

        /// <summary>
        /// 图纸预处理3
        /// </summary>
        /// <param name="validLayers"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        public static ObjectIdCollection PreProcess3(List<string> validLayers, Extents3d extents)
        {
            var objs = new ObjectIdCollection();
            foreach (ObjectId obj in PreProcessCurDwg3(validLayers, extents))
            {
                objs.Add(obj);
            }
            foreach (ObjectId obj in PreProcessXREF3(validLayers, extents))
            {
                objs.Add(obj);
            }
            return objs;
        }

        /// <summary>
        /// 由共线数据集生成新的直线
        /// </summary>
        /// <param name="collinearLines"></param>
        /// <returns></returns>
        public static Line MakeLineFromCollinears(CollinearData collinearData)
        {
            if (collinearData.RelatedCollinearLines == null || collinearData.RelatedCollinearLines.Count == 0)
                return null;

            var ptLst = new List<Point2d>();
            foreach (var line2d in collinearData.RelatedCollinearLines)
            {
                ptLst.Add(line2d.StartPoint);
                ptLst.Add(line2d.EndPoint);
            }

            var curLine = collinearData.Line;
            ptLst.Add(curLine.StartPoint);
            ptLst.Add(curLine.EndPoint);

            Tolerance tolerance = new Tolerance(1e-4, 0);
            //跟Y轴平行
            if (collinearData.Line.Direction.IsParallelTo(new Vector2d(0, 1), tolerance))
            {
                ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
            }
            else
            {
                ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
            }

            Point2d ptS;
            Point2d ptE;
            if (ptLst.Count > 3)
            {
                ptS = ptLst[1];
                ptE = ptLst[2];
            }
            else
            {
                ptS = ptLst.First();
                ptE = ptLst.Last();
            }

            var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
            return line;
        }

        public static bool IsParallel(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
                return true;
            return false;
        }

        public static LineSegment2d ExtendLine(LineSegment2d line, double length = 1000000)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            ptS = ptS - line.Direction * length;
            ptE = ptE + line.Direction * length;
            return new LineSegment2d(ptS, ptE);
        }

        /// <summary>
        /// 一块区域和另一块区域的连接处理
        /// </summary>
        /// <param name="line2dsFir"></param>
        /// <param name="line2dsSec"></param>
        /// <returns></returns>
        public static List<Curve> ConnectLinesWithsLinesWithPoints(List<LineSegment2d> line2dsFir, List<LineSegment2d> line2dsSec, List<LineSegment2d> profileBounds, string layerName = null)
        {
            var ptFirLst = new List<Point2d>();
            var ptSecLst = new List<Point2d>();
            foreach (var curveFir in line2dsFir)
            {
                var ptS = curveFir.StartPoint;
                var ptE = curveFir.EndPoint;
                if (!ptFirLst.Contains(ptS) && CommonUtils.PtInLoop(profileBounds, ptS))
                    ptFirLst.Add(ptS);

                if (!ptFirLst.Contains(ptE) && CommonUtils.PtInLoop(profileBounds, ptE))
                    ptFirLst.Add(ptE);
            }

            foreach (var curveSec in line2dsSec)
            {
                var ptS = curveSec.StartPoint;
                var ptE = curveSec.EndPoint;
                if (!ptSecLst.Contains(ptS) && CommonUtils.PtInLoop(profileBounds, ptS))
                    ptSecLst.Add(ptS);

                if (!ptSecLst.Contains(ptE) && CommonUtils.PtInLoop(profileBounds, ptE))
                    ptSecLst.Add(ptE);
            }

            //var dbObjs = new DBObjectCollection();
            //foreach (var pt in ptFirLst)
            //    Utils.DrawPreviewPoint(dbObjs, pt.toPoint3d());

            //foreach (var pt in ptSecLst)
            //    Utils.DrawPreviewPoint(dbObjs, pt.toPoint3d());

            var line2ds = new List<LineSegment2d>();

            foreach (var ptOne in ptFirLst)
            {
                foreach (var ptTwo in ptSecLst)
                {
                    var line2d = new LineSegment2d(ptOne, ptTwo);
                    if (line2d.Length > 500)
                        line2ds.Add(line2d);
                }
            }

            if (line2ds.Count == 0)
                return null;

            line2ds = line2ds.OrderBy(s => s.Length).ToList();
            //var curves = new List<Curve>();
            //foreach (var line2d in line2ds)
            //{
            //    var aimS = line2d.StartPoint.toPoint3d();
            //    var aimE = line2d.EndPoint.toPoint3d();
            //    Line line = new Line(aimS, aimE);
            //    if (layerName != null)
            //        line.Layer = layerName;
            //    curves.Add(line);
            //}

            var aimLine = line2ds.First();

            var aimS = aimLine.StartPoint.toPoint3d();
            var aimE = aimLine.EndPoint.toPoint3d();
            Line line = new Line(aimS, aimE);
            if (layerName != null)
                line.Layer = layerName;
            var curves = new List<Curve>() { line };
            return curves;
        }

        /// <summary>
        /// 一块区域和另一块区域的连接处理
        /// </summary>
        /// <param name="line2dsFir"></param>
        /// <param name="line2dsSec"></param>
        /// <returns></returns>
        public static List<Curve> ConnectLinesWithsLines(List<LineSegment2d> line2dsFir, List<LineSegment2d> line2dsSec, string layerName = null)
        {
            //DrawCurves(line2dsFir);
            //DrawCurves(line2dsSec);
            var curves = new List<Curve>();
            foreach (var firLine in line2dsFir)
            {
                foreach (var secLine in line2dsSec)
                {
                    if (IsParallel(firLine, secLine))
                        continue;

                    // 非平行
                    var firLineExtend = ExtendLine(firLine);
                    var secLineExtend = ExtendLine(secLine);
                    var intersectPoints = firLineExtend.IntersectWith(secLineExtend);

                    if (intersectPoints != null && intersectPoints.Count() == 1)
                    {
                        Point2d pt = intersectPoints.First();
                        var ptFirHead = firLine.StartPoint;
                        var ptFirEnd = firLine.EndPoint;
                        var ptSecHead = secLine.StartPoint;
                        var ptSecEnd = secLine.EndPoint;
                        Line line = null;
                        if (CommonUtils.IsPointOnSegment(pt, firLine, 1e-1))
                        {
                            if (pt.GetDistanceTo(ptSecHead) < pt.GetDistanceTo(ptSecEnd))
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptSecHead.X, ptSecHead.Y, 0));
                            else
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptSecEnd.X, ptSecEnd.Y, 0));
                        }
                        else if (CommonUtils.IsPointOnSegment(pt, secLine, 1e-1))
                        {
                            if (pt.GetDistanceTo(ptFirHead) < pt.GetDistanceTo(ptFirEnd))
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptFirHead.X, ptFirHead.Y, 0));
                            else
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptFirEnd.X, ptFirEnd.Y, 0));
                        }

                        if (line != null)
                        {
                            //ExtendCurve(line, 200);
                            if (layerName != null)
                                line.Layer = layerName;

                            // 剔除重复的增加线
                            if (!IsInValidLine(curves, line))
                                curves.Add(line);
                        }
                    }

                }
            }

            return curves;
        }

        public static bool IsInValidLine(List<Curve> curves, Line line)
        {
            if (curves.Count == 0)
                return false;

            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var addLine = curve as Line;
                    if (CommonUtils.IsAlmostNearZero(addLine.Length - line.Length, 1))
                    {
                        var ptAddS = addLine.StartPoint;
                        var ptAddE = addLine.EndPoint;
                        var ptS = line.StartPoint;
                        var ptE = line.EndPoint;
                        if ((CommonUtils.Point3dIsEqualPoint3d(ptAddS, ptS, 0.1) && CommonUtils.Point3dIsEqualPoint3d(ptAddE, ptE, 0.1))
                            || (CommonUtils.Point3dIsEqualPoint3d(ptAddS, ptE, 0.1) && CommonUtils.Point3dIsEqualPoint3d(ptAddE, ptS, 0.1)))
                            return true;
                    }
                }
            }

            return false;
        }

        public static List<Line> ArcPts2Lines(List<Point3d> arcPts, string layer)
        {
            if (arcPts == null || arcPts.Count == 0)
                return null;

            var lines = new List<Line>();
            for (int i = 0; i < arcPts.Count; i++)
            {
                var curPt = arcPts[i];
                for (int j = i + 1; j < arcPts.Count; j++)
                {
                    var nextPt = arcPts[j];
                    try
                    {
                        var line = new Line(curPt, nextPt);
                        line.Layer = layer;
                        lines.Add(line);
                    }
                    catch
                    { }
                }
            }
            return lines;
        }

        /// <summary>
        /// 门和墙的曲线连接
        /// </summary>
        /// <param name="doorBounds"></param>
        /// <param name="wallCurves"></param>
        public static List<Curve> InsertDoorRelatedCurveDatas(List<List<LineSegment2d>> doorBounds, List<Curve> wallCurves, string layer)
        {
            if ((doorBounds == null || doorBounds.Count == 0) || (wallCurves == null || wallCurves.Count == 0))
                return null;

            var curves = new List<Curve>();
            var lines = new List<LineSegment2d>();
            foreach (var doorBound in doorBounds)
            {
                // 相关数据内部或者相交
                var relatedCurves = BoundRelatedBoundCurves(doorBound, wallCurves);
                if (relatedCurves == null || relatedCurves.Count == 0)
                    continue;

                relatedCurves = CommonUtils.RemoveCollinearLines(relatedCurves);
                //Utils.DrawLineSegments(doorBound, "doorBound");
                // 根据相关数据进行连接处理
                // 收尾相连且共线的线段进行合并直线处理
                var arcCurves = new List<Curve>();
                lines.Clear();

                var arcPts = new List<Point3d>(); // 存储圆弧的点
                int arcCount = 0; // 
                foreach (var curve in relatedCurves)
                {
                    if (curve.Layer.Contains("colu") && ((curve is Arc) || curve is Circle))
                        continue;

                    if (curve is Line line)
                    {
                        Point3d ptS = line.StartPoint;
                        Point3d ptE = line.EndPoint;
                        lines.Add(new LineSegment2d(ptS.toPoint2d(), ptE.toPoint2d()));
                    }
                    else if (curve is Arc arc)
                    {
                        arcCount++;
                        var ptLst = new List<Point3d>();
                        var arcS3d = arc.StartPoint;
                        var arcS = arcS3d.toPoint2d();
                        if (CommonUtils.PointInnerEntity(doorBound, arcS))
                            ptLst.Add(arcS3d);
                        var arcE3d = arc.EndPoint;
                        var arcE = arcE3d.toPoint2d();
                        if (CommonUtils.PointInnerEntity(doorBound, arcE))
                            ptLst.Add(arcE3d);
                        arcPts.AddRange(ptLst);
                        foreach (var aimPt in ptLst)
                        {
                            var vec = arc.GetFirstDerivative(aimPt).GetNormal();
                            var lineS = (aimPt - vec * 0.5).toPoint2d();
                            var lineE = (aimPt + vec * 0.5).toPoint2d();
                            lines.Add(new LineSegment2d(lineS, aimPt.toPoint2d()));
                            lines.Add(new LineSegment2d(lineE, aimPt.toPoint2d()));
                        }
                        //var polyline = arc.Spline.ToPolyline();
                        //var lineNodes = Polyline2Lines(polyline as Polyline);
                        //if (lineNodes != null && lineNodes.Count != 0)
                        //{
                        //    foreach (var line2d in lineNodes)
                        //    {
                        //        if (IsLinesIntersectWithLine(doorBound, line2d) || (CommonUtils.PointInnerEntity(doorBound, line2d.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line2d.EndPoint)))
                        //        {
                        //            lines.Add(line2d);
                        //        }
                        //    }
                        //}
                        //var arcS = arc.StartPoint.toPoint2d();
                        //var arcE = arc.EndPoint.toPoint2d();
                        //var arcMid = arc.GetPointAtParameter(0.5 * (arc.StartParam + arc.EndParam)).toPoint2d();
                        ////var midParam = arc.GetParameterAtPoint(arcMid.toPoint3d());

                        //var arcSM = arc.GetPointAtDist(arc.Length * 0.25).toPoint2d();
                        //var arcME = arc.GetPointAtDist(arc.Length * 0.75).toPoint2d();
                        //var line1 = new LineSegment2d(arcS, arcSM);
                        //var line2 = new LineSegment2d(arcSM, arcMid);
                        //var line3 = new LineSegment2d(arcMid, arcME);
                        //var line4 = new LineSegment2d(arcME, arcE);
                        //if (IsLinesIntersectWithLine(doorBound, line1) || (CommonUtils.PointInnerEntity(doorBound, line1.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line1.EndPoint)))
                        //{
                        //    lines.Add(line1);
                        //}
                        //if (IsLinesIntersectWithLine(doorBound, line2) || (CommonUtils.PointInnerEntity(doorBound, line2.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line2.EndPoint)))
                        //{
                        //    lines.Add(line2);
                        //}

                        //if (IsLinesIntersectWithLine(doorBound, line3) || (CommonUtils.PointInnerEntity(doorBound, line3.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line3.EndPoint)))
                        //{
                        //    lines.Add(line3);
                        //}
                        //if (IsLinesIntersectWithLine(doorBound, line4) || (CommonUtils.PointInnerEntity(doorBound, line4.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line4.EndPoint)))
                        //{
                        //    lines.Add(line4);
                        //}
                    }
                }

                //Utils.DrawLineSegments(lines, "lines");
                //Utils.DrawProfile(lines, "lines");
                if (arcCount != relatedCurves.Count())
                {
                    var combineCurves = CombineCollinearLines(lines);
                    if (combineCurves == null || combineCurves.Count == 0)
                        continue;
                    var insertCurves = InsertConnectCurves(combineCurves, doorBound, layer);
                    if (insertCurves != null && insertCurves.Count != 0)
                    {
                        var maxLength = CalMaxLength(doorBound);
                        foreach (var insertCurve in insertCurves)
                        {
                            if (insertCurve.GetLength() < maxLength)
                                curves.Add(insertCurve);
                        }
                    }
                }
                else if (arcPts.Count != 0)
                {
                    // 两端都是圆弧的情形处理
                    var tempLines = ArcPts2Lines(arcPts, layer);
                    if (tempLines != null && tempLines.Count != 0)
                        curves.AddRange(tempLines);
                }
            }

            return curves;
        }

        public static double CalMaxLength(List<LineSegment2d> line2ds)
        {
            if (line2ds == null || line2ds.Count < 2)
                return 0;

            var lengths = line2ds.Select(p => p.Length).ToList();
            lengths.Sort();
            var maxLength = lengths.First() + lengths.Last();
            return maxLength;
        }

        public class CollinearData
        {
            public LineSegment2d Line
            {
                get;
                set;
            }

            public bool Valid
            {
                get;
                set;
            }

            public List<LineSegment2d> RelatedCollinearLines
            {
                get;
                set;
            }

            public CollinearData(LineSegment2d line)
            {
                Line = line;
                Valid = true;
                RelatedCollinearLines = new List<LineSegment2d>();
            }

        }

        class PathNearSearch
        {
            class PathNode
            {
                public PathNode(LineSegment2d line)
                {
                    m_curLine = line;
                }

                public bool m_bUse = false;
                public LineSegment2d m_curLine = null;
                public List<PathNode> m_relatedConnectNodes = new List<PathNode>();
            }

            private List<LineSegment2d> m_lines;
            private List<LineSegment2d> m_outLines = new List<LineSegment2d>();
            public List<LineSegment2d> OutLines
            {
                get { return m_outLines; }
            }

            private List<PathNode> m_ConnectedNodes = new List<PathNode>();
            public PathNearSearch(List<LineSegment2d> lines)
            {
                m_lines = lines;
            }

            public static List<LineSegment2d> MakeMergeLines(List<LineSegment2d> lines)
            {
                var pathSearch = new PathNearSearch(lines);
                pathSearch.Do();
                return pathSearch.OutLines;
            }

            public void Do()
            {
                for (int i = 0; i < m_lines.Count; i++)
                {
                    m_ConnectedNodes.Add(new PathNode(m_lines[i]));
                }

                for (int i = 0; i < m_ConnectedNodes.Count; i++)
                {
                    var curProfile = m_ConnectedNodes[i];
                    for (int j = i + 1; j < m_ConnectedNodes.Count; j++)
                    {
                        var nextProfile = m_ConnectedNodes[j];
                        if (IsLineNear(curProfile.m_curLine, nextProfile.m_curLine))
                        {
                            curProfile.m_relatedConnectNodes.Add(nextProfile);
                            nextProfile.m_relatedConnectNodes.Add(curProfile);
                        }
                    }
                }

                // 相邻区域收集
                for (int j = 0; j < m_ConnectedNodes.Count; j++)
                {
                    if (m_ConnectedNodes[j].m_bUse)
                        continue;

                    if (m_ConnectedNodes[j].m_relatedConnectNodes.Count == 0)
                    {
                        m_ConnectedNodes[j].m_bUse = true;
                        m_outLines.Add(m_ConnectedNodes[j].m_curLine);
                    }
                    else
                    {
                        var nearLines = SearchFromOneLine(m_ConnectedNodes[j]);
                        var mergeLine = MergeLines(nearLines);
                        if (mergeLine != null)
                            m_outLines.Add(mergeLine);
                    }
                }
            }

            private LineSegment2d MergeLines(List<LineSegment2d> lines)
            {
                if (lines == null || lines.Count == 0)
                    return null;
                var ptLst = new List<Point2d>();
                foreach (var line in lines)
                {
                    ptLst.Add(line.StartPoint);
                    ptLst.Add(line.EndPoint);
                }

                //跟Y轴平行
                if (lines[0].Direction.IsParallelTo(new Vector2d(0, 1)))
                {
                    ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
                }
                else
                {
                    ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
                }

                Point2d ptS = ptLst.First();
                Point2d ptE = ptLst.Last();
                var outLine = new LineSegment2d(ptS, ptE);
                return outLine;
            }

            // 从一块区域开始搜索
            private List<LineSegment2d> SearchFromOneLine(PathNode SearchLine)
            {
                var line2ds = new List<LineSegment2d>();
                var InOutNodes = new List<PathNode>();
                InOutNodes.Add(SearchLine);
                while (InOutNodes.Count != 0)
                {
                    var curProfile = InOutNodes.First();
                    line2ds.Add(curProfile.m_curLine);
                    curProfile.m_bUse = true;
                    InOutNodes.RemoveAt(0);
                    var childConnectNodes = curProfile.m_relatedConnectNodes;
                    foreach (var connectedNode in childConnectNodes)
                    {
                        if (!connectedNode.m_bUse && !InOutNodes.Contains(connectedNode))
                            InOutNodes.Add(connectedNode);
                    }
                }

                return line2ds;
            }

            // 线与线之间是连接的
            private bool IsLineNear(LineSegment2d lineFir, LineSegment2d lineSec)
            {
                var ptFirHead = lineFir.StartPoint;
                var ptFirTail = lineFir.EndPoint;
                var ptSecHead = lineSec.StartPoint;
                var ptSecTail = lineSec.EndPoint;

                if (CommonUtils.IsAlmostNearZero(ptFirHead.GetDistanceTo(ptSecHead), 1e-2) || CommonUtils.IsAlmostNearZero(ptFirHead.GetDistanceTo(ptSecTail), 1e-2)
                    || CommonUtils.IsAlmostNearZero(ptFirTail.GetDistanceTo(ptSecHead), 1e-2) || CommonUtils.IsAlmostNearZero(ptFirTail.GetDistanceTo(ptSecTail), 1e-2))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///  合并相连的直线
        /// </summary>
        class CombineLine
        {
            public List<LineSegment2d> CombineLines
            {
                get;
                set;
            }

            public List<LineSegment2d> OutLines
            {
                get;
                set;
            }

            public CombineLine(List<LineSegment2d> srcLines)
            {
                CombineLines = srcLines;
                OutLines = new List<LineSegment2d>();
            }

            public static List<LineSegment2d> MakeCombineLines(List<LineSegment2d> srcLines)
            {
                var combine = new CombineLine(srcLines);
                combine.Do();
                return combine.OutLines;
            }

            private bool IsCollinearLines(LineSegment2d lineFir, LineSegment2d lineSec)
            {
                var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
                var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
                if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
                {
                    var ptFirS = lineFir.StartPoint;
                    var ptSecS = lineSec.StartPoint;
                    var vector = ptFirS - ptSecS;
                    var angle3 = lineFir.Direction.GetAngleTo(vector);
                    var angle4 = lineFir.Direction.GetAngleTo(vector.Negate());
                    if (CommonUtils.IsAlmostNearZero(Math.Abs(angle3), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle4), 1e-5))
                    {
                        return true;
                    }

                }

                return false;
            }

            public void Do()
            {
                var relatedDatas = new List<CollinearData>();
                foreach (var line in CombineLines)
                {
                    relatedDatas.Add(new CollinearData(line));
                }

                for (int i = 0; i < relatedDatas.Count; i++)
                {
                    if (!relatedDatas[i].Valid)
                        continue;

                    for (int j = i + 1; j < relatedDatas.Count; j++)
                    {
                        if (!relatedDatas[j].Valid)
                            continue;

                        //共线
                        if (IsCollinearLines(relatedDatas[i].Line, relatedDatas[j].Line))
                        {
                            relatedDatas[i].RelatedCollinearLines.Add(((LineSegment2d)relatedDatas[j].Line.Clone()));
                            relatedDatas[j].Valid = false;
                        }
                    }
                }

                // 有共线则生成共线数据
                foreach (var data in relatedDatas)
                {
                    if (data.Valid)
                    {
                        if (data.RelatedCollinearLines.Count == 0)
                        {
                            OutLines.Add(data.Line);
                        }
                        else
                        {
                            var lines = CombineData(data);
                            if (lines.Count != 0)
                                OutLines.AddRange(lines);
                        }
                    }
                }
            }

            /// <summary>
            /// 合并相邻的数据
            /// </summary>
            /// <param name="CollinearData"></param>
            /// <returns></returns>
            private List<LineSegment2d> CombineData(CollinearData data)
            {
                var lines = new List<LineSegment2d>();
                lines.Add(data.Line);
                lines.AddRange(data.RelatedCollinearLines);
                var outLines = PathNearSearch.MakeMergeLines(lines);
                return outLines;
            }
        }

        /// <summary>
        /// 合并直线处理
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<LineSegment2d> CombineCollinearLines(List<LineSegment2d> lines)
        {
            if (lines == null || lines.Count == 0)
                return null;

            var combines = CombineLine.MakeCombineLines(lines);
            return combines;
        }

        /// <summary>
        /// 可扩展的矩形范围， vector 为空值时， 则是原始的范围
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetRectFromBound(Extents3d bound, Vector3d? vector = null)
        {
            Point3d minPoint;
            Point3d maxPoint;
            if (vector != null)
            {
                minPoint = bound.MinPoint - vector.Value;
                maxPoint = bound.MaxPoint + vector.Value;
            }
            else
            {
                minPoint = bound.MinPoint;
                maxPoint = bound.MaxPoint;
            }

            var recPt1 = new Point2d(minPoint.X, minPoint.Y);
            var recPt2 = new Point2d(maxPoint.X, minPoint.Y);
            var recPt3 = new Point2d(maxPoint.X, maxPoint.Y);
            var recPt4 = new Point2d(minPoint.X, maxPoint.Y);
            var lines = new List<LineSegment2d>();
            if (!CommonUtils.IsAlmostNearZero(recPt1.GetDistanceTo(recPt2), 1e-4))
                lines.Add(new LineSegment2d(recPt1, recPt2));
            if (!CommonUtils.IsAlmostNearZero(recPt2.GetDistanceTo(recPt3), 1e-4))
                lines.Add(new LineSegment2d(recPt2, recPt3));
            if (!CommonUtils.IsAlmostNearZero(recPt3.GetDistanceTo(recPt4), 1e-4))
                lines.Add(new LineSegment2d(recPt3, recPt4));
            if (!CommonUtils.IsAlmostNearZero(recPt4.GetDistanceTo(recPt1), 1e-4))
                lines.Add(new LineSegment2d(recPt4, recPt1));
            return lines;
        }

        private static bool IsValidShowLayer(LayerTableRecord layer, string layerName)
        {
            if (layer.Name.Contains(layerName)
                //&& !layer.Name.Contains("HATCH")
                //&& !layer.Name.Contains("HACH")
                && !layer.Name.Contains("OTHE")
                && !layer.Name.Contains("CAP")
                && !layer.Name.Contains("TEXT")
                && !layer.Name.Contains("DIMS")
                && !layer.Name.Contains("OTHE")
                && !layer.Name.Contains("DETL")
                && !layer.Name.Contains("TPTN"))
                return true;

            return false;
        }

        public static bool IsValidShowLayer(string srcLayerName, string aimLayerName)
        {
            if (srcLayerName.Contains(aimLayerName)
                //&& !srcLayerName.Contains("HATCH")
                && !srcLayerName.Contains("OTHE")
                && !srcLayerName.Contains("CAP")
                && !srcLayerName.Contains("TEXT")
                && !srcLayerName.Contains("DIMS")
                && !srcLayerName.Contains("DETL"))
                return true;

            return false;
        }

        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static List<string> ShowThLayers(out List<string> wallLayers, out List<string> doorLayers, out List<string> windLayers, out List<string> allValidLayers, out List<string> beamLayers, out List<string> columnLayers)
        {
            var allCurveLayers = new List<string>();
            wallLayers = new List<string>();
            doorLayers = new List<string>();
            windLayers = new List<string>();
            allValidLayers = new List<string>();
            beamLayers = new List<string>();
            columnLayers = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                foreach (var layer in layers)
                {
                    if (IsValidShowLayer(layer, "AE-WALL")
                        || IsValidShowLayer(layer, "COLU")
                        || IsValidShowLayer(layer, "AD-NAME-ROOM")
                        || IsValidShowLayer(layer, "STRU")
                        || IsValidShowLayer(layer, "AE-HDWR")
                        //|| IsValidShowLayer(layer, "AE-FLOR")
                        || IsValidShowLayer(layer, "S_WALL")
                        || IsValidShowLayer(layer, "S_WALL_DETL")
                        || IsValidShowLayer(layer, "S_BRIK")
                        || IsValidShowLayer(layer, "AE-FNSH"))
                    {
                        allCurveLayers.Add(layer.Name);
                        allValidLayers.Add(layer.Name);
                    }

                    if (layer.Name.Contains("AE-WALL") || layer.Name.Contains("AE-HDWR"))
                        wallLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-DOOR-INSD"))
                        doorLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-WIND"))
                        windLayers.Add(layer.Name);

                    if (layer.Name.Contains("BEAM"))
                        beamLayers.Add(layer.Name);

                    if (layer.Name.Contains("COLU"))
                        columnLayers.Add(layer.Name);
                }

                allValidLayers.Add("AE-DOOR-INSD");
                allValidLayers.Add("AE-WIND");
            }

            return allCurveLayers;
        }
    }
}
