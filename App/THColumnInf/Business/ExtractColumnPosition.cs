using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThColumnInfo
{
    public enum ExtractColumnDetailInfoMode
    {
        None,
        /// <summary>
        /// 二维表
        /// </summary>
        Regular,
        /// <summary>
        /// 有附图
        /// </summary>
        Graphic,
        /// <summary>
        /// 原位
        /// </summary>
        InSitu
    }
    /// <summary>
    /// 提取柱子信息及其中包括的原位标注信息（生成柱表）
    /// </summary>
    public class ExtractColumnPosition : IDataSource
    {
        private List<BlockReference> blkRefs = new List<BlockReference>();
        private Point3d rangePt1 = Point3d.Origin;
        private Point3d rangePt2 = Point3d.Origin;
        private Document doc;
        private ExtractColumnDetailInfoMode extractColumnDetailInfoMode = ExtractColumnDetailInfoMode.None;
        private List<List<Point3d>> allColumnBoundaryPts = new List<List<Point3d>>();
        private double columnOffsetRatio = 0.1;
        private double findBHSideMiddleTextOffsetDisRatio = 0.2; //查找范围由柱子长边和短边之和的一半再乘以这个比例，用于查找柱子B边和H边偏移的文字
        private double searchColumnPolylineDis = 5.0;
        private SelectionFilter polylineSf;
        private SelectionFilter textSf;
        private double textSize = 0.0;

        public ExtractColumnPosition(Point3d rangePt1, Point3d rangePt2)
        {
            this.rangePt1 = rangePt1;
            this.rangePt2 = rangePt2;
            ResetRangePt();
            this.doc = ThColumnInfoUtils.GetMdiActiveDocument();
            TypedValue[] tvs1 = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline,LWPOLYLINE") }; //后期根据需要再追加搜索条件
            TypedValue[] tvs2 = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Text") }; //后期根据需要再追加搜索条件
            this.polylineSf = new SelectionFilter(tvs1);
            this.textSf = new SelectionFilter(tvs2);
            GetTextSize();
        }
        private void GetTextSize()
        {
            PromptSelectionResult psr = this.doc.Editor.SelectAll(this.textSf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> textObjIds = psr.Value.GetObjectIds().ToList();
                List<double> textSizes = textObjIds.Select(i => (ThColumnInfoDbUtils.GetEntity(this.doc.Database, i) as DBText).Height).ToList();
                List<double> diffs = textSizes.Distinct().ToList();
                Dictionary<double, int> textSizeDic = new Dictionary<double, int>();
                foreach (double textSizeV in diffs)
                {
                    List<double> tempList = textSizes.Where(i => i == textSizeV).Select(i => i).ToList();
                    textSizeDic.Add(textSizeV, tempList.Count);
                }
                this.textSize = textSizeDic.OrderByDescending(i => i.Value).Select(i => i.Key).FirstOrDefault();
            }
            else
            {
                object textSizeV = Application.GetSystemVariable("TextSize");
                if (textSizeV != null)
                {
                    if (textSizeV.GetType() == typeof(string))
                    {
                        string value = textSizeV as string;
                        this.textSize = Convert.ToDouble(value);
                    }
                    else if (textSizeV.GetType() == typeof(double))
                    {
                        this.textSize = (double)textSizeV;
                    }
                }
            }
        }
        /// <summary>
        /// 柱子位置信息
        /// </summary>
        public List<ColumnInf> ColumnInfs { get; set; } = new List<ColumnInf>();
        /// <summary>
        /// 柱表详细信息(用于柱号查找)
        /// </summary>
        public List<ColumnTableRecordInfo> ColumnTableRecordInfos { get; set; } = new List<ColumnTableRecordInfo>();

        public void Extract()
        {
            ExtractTableInfo(); //如果不是原位图纸，提取一下柱表信息
            this.blkRefs = GetXRefColumn(); //获取两点范围内找到的柱子
            if (this.blkRefs.Count == 0)
            {
                return;
            }
            this.allColumnBoundaryPts = new List<List<Point3d>>();
            using (Transaction trans = this.doc.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < blkRefs.Count; i++)
                {
                    List<List<Point3d>> columnPts = FindColumnPositions(blkRefs[i]);
                    allColumnBoundaryPts.AddRange(columnPts);
                }
                trans.Commit();
            }
            this.allColumnBoundaryPts = this.allColumnBoundaryPts.Where(i => IsColumnInCheckRange(i)).Select(i => i).ToList(); //找到所有柱的位置
            FindColumnInfo(); //查找柱子信息(包括原位标注的信息)
        }
        private void FindColumnInfo()
        {
            for (int i = 0; i < this.allColumnBoundaryPts.Count; i++)
            {
                ColumnInf columnInfo = new ColumnInf();
                Dictionary<Line, Point3d> searchLineDic = GetLeaderLine(this.allColumnBoundaryPts[i]);
                foreach (var lineItem in searchLineDic)
                {
                    GetColumnInf(columnInfo,lineItem.Key, lineItem.Value);
                    if (!string.IsNullOrEmpty(columnInfo.Code))
                    {
                        columnInfo.Points = this.allColumnBoundaryPts[i];
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(columnInfo.Code))
                {
                    this.ColumnInfs.Add(columnInfo);
                }
            }
        }
        /// <summary>
        /// 获取柱子的信息
        /// </summary>
        /// <param name="line">柱子伸出来的线</param>
        /// <param name="searchPt">用于查找的点</param>
        /// <returns></returns>
        private void GetColumnInf(ColumnInf columnInf,Line line, Point3d searchPt)
        {
            string columnCode = ""; 
            string antiSeismicGrade = ""; //抗震等级
            Point3d pt1 = new Point3d(searchPt.X - this.searchColumnPolylineDis, searchPt.Y - this.searchColumnPolylineDis, searchPt.Z);
            Point3d pt2 = new Point3d(searchPt.X + this.searchColumnPolylineDis, searchPt.Y + this.searchColumnPolylineDis, searchPt.Z);
            Point3d filterPt1 = pt1.TransformBy(this.doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d filterPt2 = pt2.TransformBy(this.doc.Editor.CurrentUserCoordinateSystem.Inverse());
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, filterPt1, filterPt2, 
                PolygonSelectionMode.Crossing, this.polylineSf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> polylineObjIds = psr.Value.GetObjectIds().ToList();
                List<Polyline> polylines = polylineObjIds.Select(i => ThColumnInfoDbUtils.GetEntity(this.doc.Database, i) as Polyline).ToList();
                polylines = polylines.OrderByDescending(i => i.Area).ToList();
                for (int i = 0; i < polylines.Count; i++)
                {
                    List<Point3d> polylinePts = new List<Point3d>();
                    for (int j = 0; j < polylines[i].NumberOfVertices; j++)
                    {
                        polylinePts.Add(polylines[i].GetPoint3dAt(j));
                    }
                    Dictionary<Line, Point3d> hooplingReinforceLeaderLines = GetLeaderLine(polylinePts);
                    if (hooplingReinforceLeaderLines.Count == 0)
                    {
                        continue;
                    }
                    List<Line> leaderLines = hooplingReinforceLeaderLines.Where(j => j.Key.ObjectId == line.ObjectId).Select(j => j.Key).ToList();
                    if (leaderLines != null && hooplingReinforceLeaderLines.Count > 1) //找到传入的Line,又找到另外一个线
                    {
                        foreach (var item in hooplingReinforceLeaderLines)
                        {
                            if (item.Key.ObjectId == line.ObjectId)
                            {
                                continue;
                            }
                            List<DBText> dBTexts = GetMarkTexts(item.Key, item.Value); //获取集中标注的文字，用于提取信息
                            if (dBTexts.Count == 0)
                            {
                                continue;
                            }
                            ColumnTableRecordInfo ctri = GetColumnInSituMarkInfs(polylines[i], dBTexts);
                            if (ctri.Validate())
                            {
                                columnCode = ctri.Code;
                                this.ColumnTableRecordInfos.Add(ctri);
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(columnCode))
                    {
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(columnCode) || string.IsNullOrEmpty(antiSeismicGrade))
            {
                List<DBText> dBTexts = GetMarkTexts(line, searchPt);
                for (int i = 0; i < dBTexts.Count; i++)
                {
                    if (string.IsNullOrEmpty(dBTexts[i].TextString))
                    {
                        continue;
                    }
                    if(string.IsNullOrEmpty(columnCode))
                    {
                        if (dBTexts[i].TextString.ToUpper().Contains("KZ")) //如果没有原位标注，则只查找柱号
                        {
                            columnCode = dBTexts[i].TextString;
                        }
                    }
                    if(string.IsNullOrEmpty(antiSeismicGrade))
                    {
                        if (dBTexts[i].TextString.ToUpper().Contains("抗震")) 
                        {
                            antiSeismicGrade = dBTexts[i].TextString;
                        }
                    }
                    if(!string.IsNullOrEmpty(columnCode) && 
                        !string.IsNullOrEmpty(antiSeismicGrade))
                    {
                        break;
                    }
                }
            }
            columnInf.Code = columnCode;
            columnInf.AntiSeismicGrade = antiSeismicGrade;
        }
        /// <summary>
        /// 获取一点旁边的文字(含柱号的文字，下方有5个左右)
        /// </summary>
        /// <param name="leaderLine">引线</param>
        /// <param name="pt">引线一端要搜索的点</param>
        /// <returns></returns>
        private List<DBText> GetMarkTexts(Line leaderLine,Point3d pt)
        {
            List<DBText> findTexts = new List<DBText>();
            Point3d pt1 = pt, pt2 = pt;
            if(leaderLine.EndPoint.IsEqualTo(pt,ThCADCommon.Global_Tolerance))
            {
                pt1 = pt;
                pt2 = ThColumnInfoUtils.GetExtendPt(leaderLine.EndPoint, leaderLine.StartPoint, -1.5 * this.textSize);
            }
            else
            {
                pt2 = ThColumnInfoUtils.GetExtendPt(leaderLine.StartPoint, leaderLine.EndPoint, -1.5 * this.textSize);
            }
            List<ObjectId> findDbTextIds = new List<ObjectId>();
            Point3d filterPt1 = pt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d filterPt2 = pt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, 
                filterPt1, filterPt2, PolygonSelectionMode.Crossing,this.textSf);
            if (psr.Status == PromptStatus.OK)
            {
                findDbTextIds=psr.Value.GetObjectIds().ToList();
            }
            List<DBText> dBTexts = findDbTextIds.Select(j => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
            List<DBText> findCodeRes = findCodeRes = dBTexts.Where(j => j.TextString.Contains("KZ")).Select(j => j).ToList();
            if(findCodeRes.Count==0)
            {
                List<Point3d> textPts = new List<Point3d>();
                textPts.AddRange(dBTexts.Select(i => i.Bounds.Value.MinPoint).ToList());
                textPts.AddRange(dBTexts.Select(i => i.Bounds.Value.MaxPoint).ToList());

                double minX = textPts.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
                double minY = textPts.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
                double minZ = textPts.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

                double maxX = textPts.OrderByDescending(i => i.X).Select(i => i.X).FirstOrDefault();
                double maxY = textPts.OrderByDescending(i => i.Y).Select(i => i.Y).FirstOrDefault();
                double maxZ = textPts.OrderByDescending(i => i.Z).Select(i => i.Z).FirstOrDefault();

                Point3d searchPt1 = new Point3d(minX, minY, minZ);
                Point3d searchPt2 = new Point3d(maxX, maxY + 5*this.textSize, minZ);
                Point3d filterPt3 = searchPt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
                Point3d filterPt4 = searchPt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
                psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, filterPt3, filterPt4, PolygonSelectionMode.Crossing, this.textSf);
                if(psr.Status==PromptStatus.OK)
                {
                    findDbTextIds= psr.Value.GetObjectIds().ToList();
                    dBTexts = findDbTextIds.Select(j => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
                    findCodeRes = findCodeRes = dBTexts.Where(j => j.TextString.Contains("KZ")).Select(j => j).ToList();
                }
                if(findCodeRes.Count==0)
                {
                    searchPt1 = new Point3d(maxX, maxY, minZ);
                    searchPt2 = new Point3d(minX, minY - 5 * this.textSize, minZ);
                    Point3d filterPt5 = searchPt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
                    Point3d filterPt6 = searchPt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
                    psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, filterPt5, filterPt6, PolygonSelectionMode.Crossing, this.textSf);
                    if(psr.Status==PromptStatus.OK)
                    {
                        findDbTextIds = psr.Value.GetObjectIds().ToList();
                        dBTexts = findDbTextIds.Select(j => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
                        findCodeRes = findCodeRes = dBTexts.Where(j => j.TextString.Contains("KZ")).Select(j => j).ToList();
                    }
                }
            }
            if (findCodeRes.Count == 0)
            {
                return findTexts;
            }
            DBText codeText = findCodeRes.OrderBy(j => Math.Abs(j.Position.X - pt.X)).First();
            pt1 = codeText.Bounds.Value.MinPoint;
            pt2 = new Point3d(codeText.Bounds.Value.MaxPoint.X, pt1.Y - 5 * this.textSize, pt1.Z);
            Point3d filterPt7 = pt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d filterPt8 = pt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, filterPt7, filterPt8, PolygonSelectionMode.Crossing, this.textSf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> newTextIds = psr.Value.GetObjectIds().ToList();
                newTextIds = newTextIds.Where(i => i != codeText.ObjectId).Select(i => i).ToList();
                List<DBText> newDbTexts = newTextIds.Select(j => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
                findTexts = newDbTexts.Where(j => Math.Abs(j.Position.X - codeText.Position.X) <= 5.0).Select(j => j).ToList();
                findTexts = findTexts.OrderByDescending(i => i.Position.Y).ToList();
                findTexts.Insert(0, codeText);
            }
            return findTexts;
        }
        /// <summary>
        /// 获取柱原位标注信息
        /// </summary>
        /// <param name="polyline">柱子外框</param>
        /// <param name="dBTexts">柱子外框引线</param>
        /// <returns></returns>
        private ColumnTableRecordInfo GetColumnInSituMarkInfs(Polyline polyline,List<DBText> dBTexts)
        {
            ColumnTableRecordInfo ctri = new ColumnTableRecordInfo();
            List<Point3d> boundaryPts = new List<Point3d>();
            for(int i=0;i<polyline.NumberOfVertices;i++)
            {
                boundaryPts.Add(polyline.GetPoint3dAt(i));
            }
            double minX = boundaryPts.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
            double minY = boundaryPts.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double minZ=  boundaryPts.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

            double maxX = boundaryPts.OrderByDescending(i => i.X).Select(i => i.X).FirstOrDefault();
            double maxY = boundaryPts.OrderByDescending(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double maxZ = boundaryPts.OrderByDescending(i => i.Z).Select(i => i.Z).FirstOrDefault();

            double offsetDis = (Math.Abs(maxX - minX) + Math.Abs(maxY - minY))/2.0;
            offsetDis *= this.findBHSideMiddleTextOffsetDisRatio;

            Point3d pt1 = new Point3d(minX- offsetDis, minY- offsetDis, minZ);
            Point3d pt2 = new Point3d(maxX + offsetDis, maxY + offsetDis, minZ);
            Point3d fiterPt1 = pt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d fiterPt2 = pt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor,
                           fiterPt1, fiterPt2, PolygonSelectionMode.Crossing, this.textSf);
            Dictionary<TextRotation, DBText> sideTextDic = new Dictionary<TextRotation, DBText>(); //存储文字
            if (psr.Status==PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                List<DBText> sideTexts = objIds.Select(i => ThColumnInfoDbUtils.GetEntity(this.doc.Database, i) as DBText).ToList();
                sideTextDic=  GetValidHoopReinforce(sideTexts, minX, minY, maxX, maxY); //查找柱子外框边的两个文字
            }
            Point3d fiterPt3 = new Point3d(minX, minY, minZ).TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d fiterPt4 = new Point3d(maxX, maxY, minZ).TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor,fiterPt3, fiterPt4, PolygonSelectionMode.Window, this.polylineSf);
            int bSideNum=0, hSideNum=0;
            string typeNubmer = "";
            if (psr.Status ==PromptStatus.OK)
            {
                List<ObjectId> polylineObjIds = psr.Value.GetObjectIds().ToList();
                List<DBObject> polylineObjs = polylineObjIds.Select(i => ThColumnInfoDbUtils.GetEntity(this.doc.Database,i) as DBObject).ToList();
                typeNubmer=GetHoopReinforcementTypeNumberOne(polylineObjs, out bSideNum, out hSideNum); //获取箍筋类型号
            }
            if(bSideNum==0 || hSideNum==0)
            {
                return ctri;
            }
            //以下是整理信息
            string angularReinforcement = "";
            string bEdgeSideMiddleReinforcement = "";
            string hEdgeSideMiddleReinforcement = "";
            string allLongitudinalReinforcement = "";
            string tempReinforcement = "";
            string antiSeismicGrade = "";
            for (int i = 0; i < dBTexts.Count; i++)
            {
                if (string.IsNullOrEmpty(dBTexts[i].TextString))
                {
                    continue;
                }
                if (ThColumnInfoUtils.IsColumnCode(dBTexts[i].TextString.ToUpper()))
                {
                    ctri.Code = dBTexts[i].TextString;
                }
                else if (dBTexts[i].TextString.ToUpper().Contains("x") || dBTexts[i].TextString.ToUpper().Contains("X")
                    || dBTexts[i].TextString.ToUpper().Contains("×") || dBTexts[i].TextString.ToUpper().Contains("×"))
                {
                    ctri.Spec = dBTexts[i].TextString;
                }
                else if(new ColumnTableRecordInfo().ValidateReinforcement(dBTexts[i].TextString))
                {
                    tempReinforcement = dBTexts[i].TextString;
                }
                else if (new ColumnTableRecordInfo().ValidateHoopReinforcement(dBTexts[i].TextString))
                {
                    ctri.HoopReinforcement = dBTexts[i].TextString;
                }
                else if(dBTexts[i].TextString.Contains("抗震"))
                {
                    if(string.IsNullOrEmpty(ctri.Remark))
                    {
                        antiSeismicGrade = dBTexts[i].TextString;
                    }
                }
            }
            ctri.HoopReinforcementTypeNumber = typeNubmer;
            List<int> reinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(tempReinforcement);
            int markedReinforceNum = -1;
            if (reinforceDatas.Count==2)
            {
                markedReinforceNum = reinforceDatas[0]; //获取12x22 ->12 x是分隔符
            }
            int bSideNumber = 0; //从集中标注文字中提取出来的数量
            int hSideNumber = 0; //从集中标注文字中提取出来的数量
            if (sideTextDic.ContainsKey(TextRotation.Horizontal))
            {
                string horStr = sideTextDic[TextRotation.Horizontal].TextString;
                List<int> bSideReinforceDatas= new ColumnTableRecordInfo().GetReinforceDatas(horStr);
                if(bSideReinforceDatas.Count==2)
                {
                    bSideNumber = bSideReinforceDatas[0];
                }              
                bEdgeSideMiddleReinforcement = bSideNumber * 2 + new ColumnTableRecordInfo().GetReinforceSuffix(horStr);
            }
            else if(sideTextDic.ContainsKey(TextRotation.Vertical))
            {
                string verStr = sideTextDic[TextRotation.Vertical].TextString;
                List<int> hSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(verStr);
                if (hSideReinforceDatas.Count == 2)
                {
                    hSideNumber = hSideReinforceDatas[0];
                }                
                hEdgeSideMiddleReinforcement = hSideNumber * 2 + new ColumnTableRecordInfo().GetReinforceSuffix(verStr);
            }
            else //没有标注任何文字
            {
                allLongitudinalReinforcement = tempReinforcement;
            }
            string tempReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(tempReinforcement);
            if(markedReinforceNum >= 4)
            {
                angularReinforcement=4+ tempReinforceSuffix; //算出角筋
            }
            if(string.IsNullOrEmpty(bEdgeSideMiddleReinforcement))
            {
                bEdgeSideMiddleReinforcement = bSideNum * 2 + tempReinforceSuffix; //b边中部筋
            }
            if (string.IsNullOrEmpty(hEdgeSideMiddleReinforcement))
            {
                hEdgeSideMiddleReinforcement = hSideNum * 2 + tempReinforceSuffix; //h边中部筋
            }
            ctri.BEdgeSideMiddleReinforcement = bEdgeSideMiddleReinforcement;
            ctri.HEdgeSideMiddleReinforcement = hEdgeSideMiddleReinforcement;
            ctri.AngularReinforcement = angularReinforcement;
            ctri.AllLongitudinalReinforcement = allLongitudinalReinforcement;
            return ctri;
        }
        private string GetHoopReinforcementTypeNumberOne(List<DBObject> polylines,out int bSideNum, out int hSideNum)
        {
            string typeNumber = "";
            bSideNum = 0;
            hSideNum = 0;
            if (polylines.Count == 0)
            {
                return typeNumber;
            }
            Dictionary<double, List<DBObject>> polylineAreaDic = new Dictionary<double, List<DBObject>>();
            List<double> xDirList = new List<double>();
            List<double> yDirList = new List<double>();
            for (int i = 0; i < polylines.Count; i++)
            {
                Polyline polyline = polylines[i] as Polyline;
                if (polyline == null)
                {
                    continue;
                }
                if(polyline.Area>0.0)
                {
                   List<double> existAreas= polylineAreaDic.Where(j => Math.Abs(j.Key - polyline.Area) <= 2.0).Select(j => j.Key).ToList();
                    if(existAreas==null || existAreas.Count==0)
                    {
                        polylineAreaDic.Add(polyline.Area, new List<DBObject>() { polylines[i] });
                    }
                    else
                    {
                        polylineAreaDic[existAreas[0]].Add(polylines[i]);
                    }
                }
                Point3dCollection pts = new Point3dCollection();
                polyline.GetStretchPoints(pts);
                List<Point2d> polyline2ds = new List<Point2d>();
                foreach (Point3d pt in pts)
                {
                    polyline2ds.Add(new Point2d(pt.X, pt.Y));
                }
                for (int j = 0; j < polyline2ds.Count - 1; j++)
                {
                    Point2d startPt = polyline2ds[j];
                    Point2d endPt = polyline2ds[j + 1];
                    Vector2d vec = startPt.GetVectorTo(endPt);
                    if (vec.Length <= 50) 
                    {
                        continue;
                    }
                    if (vec.GetNormal().IsParallelTo(Vector2d.XAxis, ThColumnInfoUtils.tolerance))
                    {
                        xDirList.Add(startPt.GetDistanceTo(endPt));
                    }
                    else if (vec.GetNormal().IsParallelTo(Vector2d.YAxis, ThColumnInfoUtils.tolerance))
                    {
                        yDirList.Add(startPt.GetDistanceTo(endPt));
                    }
                }
            }

            List<double> smallPolylineAreas= polylineAreaDic.OrderBy(i => i.Key).Select(i => i.Key).ToList();
            foreach(double polylineArea in smallPolylineAreas)
            {
                if(polylineAreaDic[polylineArea].Count>=4 && polylineAreaDic[polylineArea].Count%2==0)
                {
                    SplitInsidePolylines(polylineAreaDic[polylineArea], out bSideNum, out hSideNum);
                    break;
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
            typeNumber = "1（" + xNum.ToString() + " x " + yNum.ToString() + "）";
            return typeNumber;
        }
        private void SplitInsidePolylines(List<DBObject> polylineObjs,out int bsideNum,out int hSideNum)
        {
            bsideNum = 0;
            hSideNum = 0;
            List<Polyline> polylines = polylineObjs.Select(i => i as Polyline).ToList();
            List<Polyline> noRepeatedPolylines = new List<Polyline>();
            while (polylines.Count>0)
            {
                Polyline currentLine = polylines[0];
                noRepeatedPolylines.Add(currentLine);
                polylines=polylines.Where(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).DistanceTo
                (ThColumnInfoUtils.GetMidPt(currentLine.Bounds.Value.MinPoint, currentLine.Bounds.Value.MaxPoint)) > 5.0).Select(i=>i).ToList();
            }
            polylines = noRepeatedPolylines;
            List<double> xValues= polylines.Select(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).X).ToList();
            List<double> yValues = polylines.Select(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).Y).ToList();

            double minX = xValues.OrderBy(i => i).First();
            double minY = yValues.OrderBy(i => i).First();
            List<double> hSides = xValues.Where(i => Math.Abs(i - minX) <= 5.0).Select(i => i).ToList();
            List<double> bSides = yValues.Where(i => Math.Abs(i - minY) <= 5.0).Select(i => i).ToList();
            bsideNum = bSides.Count - 2;
            hSideNum = hSides.Count - 2;
        }
        private TextRotation GetDbTextRotation(DBText dBText)
        {
            double ang = ThColumnInfoUtils.RadToAng(dBText.Rotation);
            if(Math.Abs(ang-90)<=1.0 || Math.Abs(ang - 270) <= 1.0)
            {
                return TextRotation.Vertical;
            }
            else if(Math.Abs(ang - 0.0) <= 1.0 || Math.Abs(ang - 180) <= 1.0)
            {
                return TextRotation.Horizontal;
            }
            else
            {
                return TextRotation.Oblique;
            }
        }
        private Dictionary<TextRotation,DBText> GetValidHoopReinforce(List<DBText> dbTexts,double minX,double minY,double maxX,double maxY)
        {
            Dictionary<TextRotation, DBText> textDic = new Dictionary<TextRotation, DBText>();
            dbTexts = dbTexts.Where(i => !string.IsNullOrEmpty(i.TextString) && 
            new ColumnTableRecordInfo().ValidateReinforcement(i.TextString)).Select(i => i).ToList();
            //把方框以内的文字给去掉
            dbTexts = dbTexts.Where(i => !((i.Bounds.Value.MinPoint.X >= minX && i.Bounds.Value.MinPoint.X >= maxX) &&
                                (i.Bounds.Value.MaxPoint.X >= minX && i.Bounds.Value.MaxPoint.X >= maxX) &&
                                (i.Bounds.Value.MinPoint.Y >= minY && i.Bounds.Value.MinPoint.Y >= maxY) &&
                                (i.Bounds.Value.MaxPoint.Y >= minY && i.Bounds.Value.MaxPoint.Y >= maxY))).Select(i => i).ToList();
            List<DBText> xDirText = dbTexts.Where(i => GetDbTextRotation(i) == TextRotation.Horizontal).Select(i => i).ToList();
            List<DBText> yDirText = dbTexts.Where(i => GetDbTextRotation(i) == TextRotation.Vertical).Select(i => i).ToList();
            double middleX = (minX + maxX) / 2.0;
            double middleY = (minY + maxY) / 2.0;
            xDirText = xDirText.OrderBy(i => Math.Abs(i.Position.X - middleX)).Select(i => i).ToList();
            yDirText = yDirText.OrderBy(i => Math.Abs(i.Position.Y - middleY)).Select(i => i).ToList();
            if(xDirText!=null && xDirText.Count>0)
            {
                textDic.Add(TextRotation.Horizontal, xDirText[0]);
            }
            if (yDirText != null && yDirText.Count > 0)
            {
                textDic.Add(TextRotation.Vertical, yDirText[0]);
            }
            return textDic;
        }
        /// <summary>
        /// 获取标注引线
        /// </summary>
        /// <returns></returns>
        private Dictionary<Line, Point3d> GetLeaderLine(List<Point3d> boundaryPts)
        {
            Dictionary<Line, Point3d> linePtDic = new Dictionary<Line, Point3d>();
            double xMin = boundaryPts.OrderBy(i => i.X).Select(i=>i.X).First();
            double xMax = boundaryPts.OrderByDescending(i => i.X).Select(i => i.X).First();

            double yMin = boundaryPts.OrderBy(i => i.Y).Select(i => i.Y).First();
            double yMax = boundaryPts.OrderByDescending(i => i.Y).Select(i => i.Y).First();
            Point3d pt1 = new Point3d(xMin, yMin, 0.0);
            Point3d pt2 = new Point3d(xMax, yMax, 0.0);      
            double length = pt1.DistanceTo(pt2);
            pt1=ThColumnInfoUtils.GetExtendPt(pt1, pt2, this.columnOffsetRatio * length * -1.0);
            pt2 = ThColumnInfoUtils.GetExtendPt(pt2, pt1, this.columnOffsetRatio * length * -1.0);
            TypedValue[] tvs = new TypedValue[]
             {
                  new TypedValue((int)DxfCode.Start,"LINE") //后续如果需要，根据配置来设置过滤图层
             };
            Point3dCollection offsetRecPts = new Point3dCollection();
            offsetRecPts.Add(pt1);
            offsetRecPts.Add(new Point3d(pt2.X,pt1.Y,0.0));
            offsetRecPts.Add(pt2);
            offsetRecPts.Add(new Point3d(pt1.X, pt2.Y, 0.0));

            Point3d filterPt1 = pt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d filterPt2 = pt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, 
                filterPt1, filterPt2, PolygonSelectionMode.Crossing,sf); 
            List<Line> lines = new List<Line>();
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                lines = objIds.Select(i => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, i) as Line).ToList();
            }
            bool startInside = false;
            bool endInside = false;
            for (int i = 0; i < lines.Count; i++)
            {
                startInside = false;
                endInside = false;
                if (ThColumnInfoUtils.IsPointInPolyline(offsetRecPts, lines[i].StartPoint)) //线的起点在
                {
                    startInside = true;
                }
                if (ThColumnInfoUtils.IsPointInPolyline(offsetRecPts, lines[i].EndPoint)) //线的起点在
                {
                    endInside = true;
                }
                if(startInside && !endInside)
                {
                    linePtDic.Add(lines[i], lines[i].EndPoint);
                }
                else if(!startInside && endInside)
                {
                    linePtDic.Add(lines[i], lines[i].StartPoint);
                }
                else if(startInside && endInside)
                {
                    continue;
                }
                else if(!startInside && !endInside)
                {
                   Polyline recOffsetPolyline= ThColumnInfoUtils.CreatePolyline(offsetRecPts);
                    startInside = ThColumnInfoUtils.IsPointOnPolyline(recOffsetPolyline, lines[i].StartPoint); 
                    endInside = ThColumnInfoUtils.IsPointOnPolyline(recOffsetPolyline, lines[i].EndPoint);
                    if (startInside && !endInside) //起点在偏移的Rectangle上，终点不在
                    {
                        linePtDic.Add(lines[i], lines[i].EndPoint);
                    }
                    else if(!startInside && endInside) //终点在偏移的Rectangle上，起点不在
                    {
                        linePtDic.Add(lines[i], lines[i].StartPoint);
                    }
                }
            }
            return linePtDic;
        }
        /// <summary>
        /// 判断柱子在检查范围内
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private bool IsColumnInCheckRange(List<Point3d> pts)
        {
            bool isIn = false;
            for(int i=0;i<pts.Count;i++)
            {
                if((pts[i].X>this.rangePt1.X && pts[i].X < this.rangePt2.X) &&
                    (pts[i].Y > this.rangePt1.Y && pts[i].Y < this.rangePt2.Y))
                {
                    isIn = true;
                    break;
                }
            }
            return isIn;
        }
        private void ExtractTableInfo()
        {
            Point3d firstPt = Point3d.Origin;
            Point3d secondPt = Point3d.Origin;
            PromptPointOptions ppo1 = new PromptPointOptions("\n请选择表格外框线的左下角点[原位(I)]");
            ppo1.Keywords.Add("i");
            PromptPointResult ppr1 = this.doc.Editor.GetPoint(ppo1);
            if(ppr1.StringResult=="i" || ppr1.StringResult == "I")
            {
                this.extractColumnDetailInfoMode = ExtractColumnDetailInfoMode.InSitu;
                return;
            }
            if (ppr1.Status == PromptStatus.OK)
            {
                firstPt = ppr1.Value;
            }
            else
            {

                return;
            }
            PromptPointResult ppr2 = this.doc.Editor.GetCorner("\n请选择表格外框线的右上角点", ppr1.Value);
            if (ppr2.Status == PromptStatus.OK)
            {
                secondPt = ppr2.Value;
            }
            else
            {
                return;
            }
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Insert") };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, firstPt, secondPt, PolygonSelectionMode.Crossing, sf);
            ObjectId[] insertObjIds = new ObjectId[] { };
            if (psr.Status == PromptStatus.OK)
            {
                insertObjIds = psr.Value.GetObjectIds();
                ThColumnInfoUtils.ShowObjIds(insertObjIds, false); //把块隐藏掉
            }
            try
            {
                bool hasDimension = false, hasText = false, hasPolyline = false;
                bool hasLine = false;
                PromptSelectionResult psr1 = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, firstPt, secondPt, PolygonSelectionMode.Crossing);                
                if (psr1.Status==PromptStatus.OK)
                {
                    List<ObjectId> seleObjIds = psr1.Value.GetObjectIds().ToList();
                    using (Transaction trans = doc.TransactionManager.StartTransaction())
                    {
                        for (int i = 0; i < seleObjIds.Count; i++)
                        {
                            Entity ent = trans.GetObject(seleObjIds[i], OpenMode.ForRead) as Entity;
                            if (ent == null)
                            {
                                continue;
                            }
                            if (ent is Dimension)
                            {
                                if (!hasDimension)
                                {
                                    hasDimension = true;
                                }
                            }
                            if (ent is DBText || ent is MText)
                            {
                                if (!hasText)
                                {
                                    hasText = true;
                                }
                            }
                            if (ent is Polyline)
                            {
                                if (!hasPolyline)
                                {
                                    hasPolyline = true;
                                }
                            }
                            if (ent is Line)
                            {
                                if (!hasLine)
                                {
                                    hasLine = true;
                                }
                            }
                            if (hasDimension && hasText && hasPolyline)
                            {
                                this.extractColumnDetailInfoMode = ExtractColumnDetailInfoMode.Graphic;
                                break;
                            }
                        }
                        trans.Commit();
                    }   
                }
                if(this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.None)
                {
                    if(hasText && (hasLine || hasPolyline))
                    {
                        this.extractColumnDetailInfoMode = ExtractColumnDetailInfoMode.Regular;
                    }
                }
                if (this.extractColumnDetailInfoMode==ExtractColumnDetailInfoMode.Regular)
                {
                    DataStyleColumnDetailInfo dscdi = new DataStyleColumnDetailInfo(firstPt, secondPt);
                    dscdi.Extract();
                    this.ColumnTableRecordInfos = dscdi.ColuTabRecordInfs;
                }
                else if(this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.Graphic)
                {
                    GraphicStyleColumnDetailInfo gscdi = new GraphicStyleColumnDetailInfo(firstPt, secondPt);
                    gscdi.Extract();
                    this.ColumnTableRecordInfos = gscdi.ColuTabRecordInfs;
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
            finally
            {
                ThColumnInfoUtils.ShowObjIds(insertObjIds, true);
            }
        }
        private List<List<Point3d>> FindColumnPositions(BlockReference br)
        {
            List<List<Point3d>> points = new List<List<Point3d>>();
            if(br==null)
            {
                return points;
            }
            BlockTableRecord btr = doc.TransactionManager.TopTransaction.GetObject(br.BlockTableRecord, OpenMode.ForRead)as BlockTableRecord;
            foreach(var id in btr)
            {
                DBObject dbObj = doc.TransactionManager.TopTransaction.GetObject(id,OpenMode.ForRead);
                if (dbObj is BlockReference)
                {
                    List<List<Point3d>> subBrColumnPts = FindColumnPositions(dbObj as BlockReference);
                    if(subBrColumnPts!=null && subBrColumnPts.Count>0)
                    {
                        points.AddRange(subBrColumnPts);
                    }
                }
                else if(dbObj is Polyline)
                {
                    Polyline polyline = dbObj as Polyline;
                    if(polyline.Layer.ToUpper().Contains("S_COLU")) //根据图层名来搜索
                    {
                        List<Point3d> polylinePts = new List<Point3d>();
                        for(int i=0;i<polyline.NumberOfVertices;i++)
                        {
                            polylinePts.Add(polyline.GetPoint3dAt(i));
                        }
                        points.Add(polylinePts);
                    }
                }
            }
            for(int i=0;i< points.Count;i++)
            {
                List<Point3d> pts = points[i];
                for(int j=0;j<pts.Count;j++)
                {
                    pts[j] = pts[j].TransformBy(br.BlockTransform);
                }
                points[i] = pts;
            }
            return points;
        }
        private List<BlockReference> GetXRefColumn()
        {
            List<BlockReference> brs = new List<BlockReference>();
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Insert") };
            SelectionFilter sf = new SelectionFilter(tvs);
            Point3d cornerPt1 = this.rangePt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d cornerPt2 = this.rangePt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, cornerPt1,
                cornerPt2, PolygonSelectionMode.Crossing, sf);
            ObjectId[] insertObjIds = new ObjectId[] { };
            if (psr.Status == PromptStatus.OK)
            {
                insertObjIds = psr.Value.GetObjectIds();
                brs= insertObjIds.Select(i => ThColumnInfoDbUtils.GetEntity(this.doc.Database, i) as BlockReference).ToList();
            }
            return brs;
        }
        private void ResetRangePt()
        {
            double minX = Math.Min(this.rangePt1.X, this.rangePt2.X);
            double minY = Math.Min(this.rangePt1.Y, this.rangePt2.Y);
            double minZ = Math.Min(this.rangePt1.Z, this.rangePt2.Z);
            double maxX = Math.Max(this.rangePt1.X, this.rangePt2.X);
            double maxY = Math.Max(this.rangePt1.Y, this.rangePt2.Y);
            double maxZ = Math.Max(this.rangePt1.Z, this.rangePt2.Z);
            this.rangePt1 = new Point3d(minX, minY, minZ);
            this.rangePt2 = new Point3d(maxX, maxY, maxZ);
        }
    }
    public enum TextRotation
    {
        /// <summary>
        /// 水平
        /// </summary>
        Horizontal,
        /// <summary>
        /// 垂直
        /// </summary>
        Vertical,
        /// <summary>
        /// 倾斜的
        /// </summary>
        Oblique
    }
}
