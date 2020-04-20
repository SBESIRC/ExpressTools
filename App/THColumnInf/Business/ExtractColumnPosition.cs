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
using ThColumnInfo.ViewModel;

namespace ThColumnInfo
{
    /// <summary>
    /// 提取柱子信息及其中包括的原位标注信息（生成柱表）
    /// </summary>
    public class ExtractColumnPosition : IDataSource
    {
        private List<BlockReference> blkRefs = new List<BlockReference>();
        private Point3d rangePt1 = Point3d.Origin;
        private Point3d rangePt2 = Point3d.Origin;
        private Document doc;
        private List<List<Point3d>> allColumnBoundaryPts = new List<List<Point3d>>();
        private double columnOffsetRatio = 0.2;
        private double findBHSideMiddleTextOffsetDisRatio = 0.2; //查找范围由柱子长边和短边之和的一半再乘以这个比例，用于查找柱子B边和H边偏移的文字
        private double searchColumnPolylineDis = 5.0;
        private SelectionFilter polylineSf;
        private SelectionFilter textSf;
        private double textSize = 0.0;
        private List<ColumnTableRecordInfo> propertyHasProblemList = new List<ColumnTableRecordInfo>(); //记录有问题的柱表信息
        private List<string> layerList = new List<string>();
        private ThStandardSign thStandardSign;
        private ParameterSetInfo paraSetInfo;

        private bool extractColumnGeometry = false;

        /// <summary>
        /// 通过两点提取对角范围内的柱子信息
        /// </summary>
        /// <param name="rangePt1">左下点 WCS Point</param>
        /// <param name="rangePt2">右上点 WCS Point</param>
        public ExtractColumnPosition(Point3d rangePt1, Point3d rangePt2, ThStandardSign thStandardSign)
        {
            this.rangePt1 = rangePt1;
            this.rangePt2 = rangePt2;
            this.thStandardSign = thStandardSign;
            ParameterSetVM parameterSetVM = new ParameterSetVM();
            this.paraSetInfo = parameterSetVM.ParaSetInfo;
            ResetRangePt();
            List<Point3d> ptList = new List<Point3d>();
            ptList.Distinct().ToList();
            Init();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="thStandardSign"></param>
        /// <param name="extractColumnGeometry"></param>
        public ExtractColumnPosition(ThStandardSign thStandardSign,bool extractColumnGeometry=false)
        {
            this.extractColumnGeometry = extractColumnGeometry;
            thStandardSign.SignExtractColumnInfo = this;
            this.thStandardSign = thStandardSign;
            ParameterSetVM parameterSetVM = new ParameterSetVM();
            this.paraSetInfo = parameterSetVM.ParaSetInfo;
            BlockReferenceGeometryExtents3d brge = new BlockReferenceGeometryExtents3d(thStandardSign.Br);
            brge.GeometryExtents3dBestFit();
            this.rangePt1 = brge.GeometryExtents3d.Value.MinPoint;
            this.rangePt2 = brge.GeometryExtents3d.Value.MaxPoint;
            Init();
        }
        private void Init()
        {
            this.doc = ThColumnInfoUtils.GetMdiActiveDocument();
            TypedValue[] tvs1 = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline,LWPOLYLINE") }; //后期根据需要再追加搜索条件
            TypedValue[] tvs2 = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Text") }; //后期根据需要再追加搜索条件
            this.polylineSf = new SelectionFilter(tvs1);
            this.textSf = new SelectionFilter(tvs2);
            GetTextSize();
            List<string> totalLayers = ThColumnInfoUtils.GetLayerList();
            string columnLayer = this.paraSetInfo.ColumnLayer;
            if(string.IsNullOrEmpty(columnLayer))
            {
                columnLayer = "*S_Colu";
            }
            columnLayer = columnLayer.ToUpper();
            columnLayer = columnLayer.Replace('，', ',');
            string[] splitLayers = columnLayer.Split(',');
            foreach(string splitLayer in splitLayers)
            {
                if (splitLayer.IndexOf("*") == 0 && splitLayer.Length > 1)
                {
                    string tailContent = splitLayer.Substring(1);
                    foreach (string layerItem in totalLayers)
                    {
                        int lastIndex = layerItem.ToUpper().LastIndexOf(tailContent);
                        if (lastIndex < 0)
                        {
                            continue;
                        }
                        if (lastIndex + tailContent.Length == layerItem.Length)
                        {
                            this.layerList.Add(layerItem);
                        }
                    }
                }
                else 
                {
                    List<string> tempLayers = totalLayers.Where(i => i.ToUpper() == splitLayer).Select(i => i).ToList();
                    if (tempLayers != null && tempLayers.Count > 0)
                    {
                        this.layerList.AddRange(tempLayers);
                    }
                }
            }
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
            ViewTableRecord view = doc.Editor.GetCurrentView();
            try
            {
                ThColumnInfoUtils.ZoomWindow(doc.Editor, this.rangePt1, this.rangePt2);
                if(!extractColumnGeometry)
                {
                    //提取柱表
                    ExtractColumnTable extractColumnTable = new ExtractColumnTable(this.rangePt1, this.rangePt2, this.paraSetInfo); //如果不是原位图纸，提取一下柱表信息
                    extractColumnTable.Extract();
                    this.ColumnTableRecordInfos = extractColumnTable.ColumnTableRecordInfos;
                }
                this.allColumnBoundaryPts = GetRangeColumnPoints();
                ThProgressBar.MeterProgress();
                FindColumnInfo(); //查找柱子信息(包括原位标注的信息)
                CheckColumnInfo();
                ThProgressBar.MeterProgress();
                this.ColumnTableRecordInfos.Sort(new ColumnTableRecordInfoCompare());
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
            finally
            {
                doc.Editor.SetCurrentView(view);
            }
        }
        public List<List<Point3d>> GetRangeColumnPoints()
        {
            List<List<Point3d>> pts = new List<List<Point3d>>();
            try
            {
                ThColumnInfoUtils.ZoomWindow(doc.Editor, this.rangePt1, this.rangePt2);
                this.blkRefs = GetXRefColumn(); //获取两点范围内找到的柱子
                if (this.blkRefs.Count == 0)
                {
                    return pts;
                }
                using (Transaction trans = this.doc.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < blkRefs.Count; i++)
                    {
                        List<List<Point3d>> columnPts = FindColumnPositions(blkRefs[i]);
                        pts.AddRange(columnPts);
                    }
                    trans.Commit();
                }
                pts = pts.Where(i => IsColumnInCheckRange(i)).Select(i => i).ToList(); //找到所有柱的位置
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }            
            return pts;
        }
        private void CheckColumnInfo()
        {
            if (this.ColumnInfs == null || this.ColumnInfs.Count==0)
            {
                return;
            }
            if(this.ColumnTableRecordInfos==null || this.ColumnTableRecordInfos.Count==0)
            {
                return;
            }
            propertyHasProblemList = this.ColumnTableRecordInfos.Where(i => !i.ValidateEmpty()).Select(i => i).ToList();
            for (int i = 0; i < this.ColumnInfs.Count; i++)
            {
                if (string.IsNullOrEmpty(this.ColumnInfs[i].Code))
                {
                    this.ColumnInfs[i].Error = ErrorMsg.CodeEmpty;
                    continue;
                }
                else
                {
                    this.ColumnInfs[i].Error = ErrorMsg.OK;
                    List<ColumnTableRecordInfo> ctris = this.ColumnTableRecordInfos.
                         Where(j => j.Code == this.ColumnInfs[i].Code).Select(j => j).ToList();
                    if (ctris == null || ctris.Count == 0)
                    {
                        this.ColumnInfs[i].Error = ErrorMsg.CodeEmpty;
                        continue;
                    }
                    else
                    {
                        List<ColumnTableRecordInfo> problemCtris = propertyHasProblemList.Where(
                            j => j.Code == this.ColumnInfs[i].Code).Select(j => j).ToList();
                        if (problemCtris != null && problemCtris.Count > 0)
                        {
                            this.ColumnInfs[i].Error = ErrorMsg.InfNotCompleted;
                        }
                    }
                }
            }
        }
        private void FindColumnInfo()
        {
            for (int i = 0; i < this.allColumnBoundaryPts.Count; i++)
            {
                if(this.allColumnBoundaryPts[i].Count<2)
                {
                    continue;
                }
                ColumnInf columnInfo = new ColumnInf();
                columnInfo.Points = this.allColumnBoundaryPts[i];
                if(!extractColumnGeometry)
                {
                    Dictionary<Curve, Point3d> searchLineDic = GetLeaderLine(this.allColumnBoundaryPts[i]);
                    foreach (var lineItem in searchLineDic)
                    {
                        GetColumnInf(columnInfo, lineItem.Key, lineItem.Value);
                        if (!string.IsNullOrEmpty(columnInfo.Code))
                        {
                            columnInfo.Points = this.allColumnBoundaryPts[i];
                            break;
                        }
                        ThProgressBar.MeterProgress();
                    }
                    if (string.IsNullOrEmpty(columnInfo.Code) && columnInfo.Points.Count == 0)
                    {
                        continue;
                    }
                }
                this.ColumnInfs.Add(columnInfo);
                ThProgressBar.MeterProgress();
            }
        }
        /// <summary>
        /// 获取柱子的信息
        /// </summary>
        /// <param name="line">柱子伸出来的线</param>
        /// <param name="searchPt">用于查找的点</param>
        /// <returns></returns>
        private void GetColumnInf(ColumnInf columnInf,Curve line, Point3d searchPt)
        {
            string columnCode = ""; 
            string antiSeismicGrade = ""; //抗震等级
            Point3d pt1 = new Point3d(searchPt.X - this.searchColumnPolylineDis, searchPt.Y - this.searchColumnPolylineDis, searchPt.Z);
            Point3d pt2 = new Point3d(searchPt.X + this.searchColumnPolylineDis, searchPt.Y + this.searchColumnPolylineDis, searchPt.Z);
            Point3d filterPt1 = pt1.TransformBy(this.doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d filterPt2 = pt2.TransformBy(this.doc.Editor.CurrentUserCoordinateSystem.Inverse());
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, filterPt1, filterPt2, 
                PolygonSelectionMode.Crossing, this.polylineSf);
            ThProgressBar.MeterProgress();
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> polylineObjIds = psr.Value.GetObjectIds().ToList();
                List<Curve> polylines = polylineObjIds.Where(i=> 
                {
                    Curve curve = ThColumnInfoDbUtils.GetEntity(this.doc.Database, i) as Curve;
                    if (curve is Polyline || curve is Polyline2d)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }).Select(i => ThColumnInfoDbUtils.GetEntity(
                        this.doc.Database, i) as Curve).ToList();
                polylines = polylines.OrderByDescending(i => i.Area).ToList();
                for (int i = 0; i < polylines.Count; i++)
                {
                    List<Point3d> polylinePts = ThColumnInfoUtils.GetPolylinePts(polylines[i]);                    
                    Dictionary<Curve, Point3d> hooplingReinforceLeaderLines = GetLeaderLine(polylinePts);
                    if (hooplingReinforceLeaderLines.Count == 0)
                    {
                        continue;
                    }
                    List<Curve> leaderLines = hooplingReinforceLeaderLines.Where(j => j.Key.ObjectId == line.ObjectId).Select(j => j.Key).ToList();
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
                                columnInf.HasOrigin = true;
                                columnCode = ctri.Code;
                                this.ColumnTableRecordInfos.Add(ctri);
                                break;
                            }
                            ThProgressBar.MeterProgress();
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
                        if (BaseFunction.IsColumnCode(dBTexts[i].TextString)) //如果没有原位标注，则只查找柱号
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
                ThProgressBar.MeterProgress();
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
        private List<DBText> GetMarkTexts(Curve leaderLine,Point3d pt)
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
            List<DBText> findCodeRes = findCodeRes = dBTexts.Where(j => BaseFunction.IsColumnCode(j.TextString)).Select(j => j).ToList();
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
                    findCodeRes = findCodeRes = dBTexts.Where(j => BaseFunction.IsColumnCode(j.TextString)).Select(j => j).ToList();
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
                        findCodeRes = findCodeRes = dBTexts.Where(j => BaseFunction.IsColumnCode(j.TextString)).Select(j => j).ToList();
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
                //findTexts = newDbTexts.Where(j => Math.Abs(j.Position.X - codeText.Position.X) <= 5.0).Select(j => j).ToList();
                findTexts = newDbTexts.OrderByDescending(i => i.Position.Y).ToList();
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
        private ColumnTableRecordInfo GetColumnInSituMarkInfs(Curve polyline,List<DBText> dBTexts)
        {
            ColumnTableRecordInfo ctri = new ColumnTableRecordInfo();
            List<Point3d> boundaryPts = ThColumnInfoUtils.GetPolylinePts(polyline);
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
            string tempReinforcement = "";
            string antiSeismicGrade = "";
            for (int i = 0; i < dBTexts.Count; i++)
            {
                if (string.IsNullOrEmpty(dBTexts[i].TextString))
                {
                    continue;
                }      
                if (BaseFunction.IsColumnCode(dBTexts[i].TextString.ToUpper()))
                {
                    ctri.Code = dBTexts[i].TextString;
                }
                else if (dBTexts[i].TextString.ToUpper().Contains("x") || dBTexts[i].TextString.ToUpper().Contains("X")
                    || dBTexts[i].TextString.ToUpper().Contains("×") || dBTexts[i].TextString.ToUpper().Contains("×"))
                {
                    ctri.Spec = dBTexts[i].TextString;
                }
                else if(new ColumnTableRecordInfo().ValidateReinforcement(dBTexts[i].TextString) ||
                    new ColumnTableRecordInfo().ValidateReinforcement(HandleReinfoceContent(dBTexts[i].TextString)))
                {
                    string textStr = HandleReinfoceContent(dBTexts[i].TextString);
                    tempReinforcement = textStr;
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
                bEdgeSideMiddleReinforcement = bSideNumber + new ColumnTableRecordInfo().GetReinforceSuffix(horStr);
            }
            if(sideTextDic.ContainsKey(TextRotation.Vertical))
            {
                string verStr = sideTextDic[TextRotation.Vertical].TextString;
                List<int> hSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(verStr);
                if (hSideReinforceDatas.Count == 2)
                {
                    hSideNumber = hSideReinforceDatas[0];
                }                
                hEdgeSideMiddleReinforcement = hSideNumber + new ColumnTableRecordInfo().GetReinforceSuffix(verStr);
            }
            List<string> results = GetReinforcement(tempReinforcement, bSideNum, hSideNum);
            angularReinforcement = results[0]; //角筋
            if (string.IsNullOrEmpty(bEdgeSideMiddleReinforcement))
            {
                bEdgeSideMiddleReinforcement = results[1]; //b边中部筋
            }
            if (string.IsNullOrEmpty(hEdgeSideMiddleReinforcement))
            {
                hEdgeSideMiddleReinforcement = results[2]; //h边中部筋
            }
            ctri.BEdgeSideMiddleReinforcement = bEdgeSideMiddleReinforcement;
            ctri.HEdgeSideMiddleReinforcement = hEdgeSideMiddleReinforcement;
            ctri.AngularReinforcement = angularReinforcement;
            ctri.AllLongitudinalReinforcement = GetAllLongitudinalReinforcement(ctri);
            return ctri;
        }
        private string GetAllLongitudinalReinforcement(ColumnTableRecordInfo ctri)
        {
            string allLongitudinalReinforcement = "";
            if(string.IsNullOrEmpty(ctri.AngularReinforcement) || string.IsNullOrEmpty(ctri.BEdgeSideMiddleReinforcement) ||
                 string.IsNullOrEmpty(ctri.HEdgeSideMiddleReinforcement) )
            {
                return allLongitudinalReinforcement;
            }
            int cornerNumber=0,bSideNumber=0, hSideNumber=0;
            List<int> cornerReinforceDatas = ctri.GetReinforceDatas(ctri.AngularReinforcement);
            List<int> bSideReinforceDatas = ctri.GetReinforceDatas(ctri.BEdgeSideMiddleReinforcement);
            List<int> hSideReinforceDatas = ctri.GetReinforceDatas(ctri.HEdgeSideMiddleReinforcement);
            if (cornerReinforceDatas.Count == 2)
            {
                cornerNumber = cornerReinforceDatas[0];
            }
            if (bSideReinforceDatas.Count == 2)
            {
                bSideNumber = bSideReinforceDatas[0];
            }
            if (hSideReinforceDatas.Count == 2)
            {
                hSideNumber = hSideReinforceDatas[0];
            }
            string cornerSuffix= ctri.GetReinforceSuffix(ctri.AngularReinforcement);
            string bSideSuffix = ctri.GetReinforceSuffix(ctri.BEdgeSideMiddleReinforcement);
            string hSideSuffix = ctri.GetReinforceSuffix(ctri.HEdgeSideMiddleReinforcement);
            if(!string.IsNullOrEmpty(cornerSuffix) && !string.IsNullOrEmpty(bSideSuffix) && !string.IsNullOrEmpty(hSideSuffix))
            {
                if(cornerSuffix== bSideSuffix && bSideSuffix== hSideSuffix)
                {
                    if(cornerNumber>0 && bSideNumber > 0 && hSideNumber > 0)
                    {
                        allLongitudinalReinforcement = (cornerNumber + bSideNumber * 2 + hSideNumber * 2) + cornerSuffix;
                    }
                }
            }
            return allLongitudinalReinforcement;
        }
        private List<string> GetReinforcement(string allLongitudinalReinforcement,int bSideNum,int hSideNum)
        {
            List<string> results = new List<string>();
            string cornerReinforce = "";
            string bsideReinforce = "";
            string hsideReinforce = "";
            string[] strs = allLongitudinalReinforcement.Split('+');
            if(strs.Length==1)
            {
                string tempReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(allLongitudinalReinforcement);
                cornerReinforce = "4" + tempReinforceSuffix;
                bsideReinforce = bSideNum + tempReinforceSuffix;
                hsideReinforce= hSideNum+ tempReinforceSuffix;
            }
            else if(strs.Length == 2)
            {
                string cornerReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[0]);
                cornerReinforce = "4" + cornerReinforceSuffix;
                string sideReinforceSuffix= new ColumnTableRecordInfo().GetReinforceSuffix(strs[1]);
                bsideReinforce = bSideNum + sideReinforceSuffix;
                hsideReinforce = hSideNum + sideReinforceSuffix;
            }
            else if(strs.Length == 3)
            {
                string cornerReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[0]);
                cornerReinforce = "4" + cornerReinforceSuffix;
                string bSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[1]);
                bsideReinforce = bSideNum + bSideReinforceSuffix;
                string hSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[2]);
                hsideReinforce = hSideNum + hSideReinforceSuffix;
            }
            results.Add(cornerReinforce);
            results.Add(bsideReinforce);
            results.Add(hsideReinforce);
            return results;
        }
        private string HandleReinfoceContent(string reinforceContent)
        {
            string res = reinforceContent;
            string[] strs = reinforceContent.Split('+');
            List<string> contents = new List<string>();
            foreach (string str in strs)
            {
                byte[] buffers = Encoding.UTF32.GetBytes(str);
                int lastIndex = -1;
                for (int i = 0; i < buffers.Length; i++)
                {
                    if (buffers[i] >=48 && buffers[i] <= 57)
                    {
                        lastIndex = i;
                    }
                }
                byte[] newBuffers=new byte[lastIndex+4];
                if (lastIndex>0)
                {
                    for (int i = 0; i < lastIndex+4; i++)
                    {
                        newBuffers[i]= buffers[i];
                    }
                }
                string newContent= Encoding.UTF32.GetString(newBuffers);
                contents.Add(newContent);
            }
            if(strs.Length>1)
            {
                res=string.Join("+", contents.ToArray());           
            }
            else
            {
                res = contents[0];
            }
            return res;
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
            double area = 0.0;
            for (int i = 0; i < polylines.Count; i++)
            {               
                if(polylines[i] is Polyline polyline)
                {
                    area = polyline.Area;
                }
                else if(polylines[i] is Polyline2d polyline2d)
                {
                    area = polyline2d.Area;
                }
                else if(polylines[i] is Polyline3d polyline3d)
                {
                    area = polyline3d.Area;
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
                    polylineAreaDic.Add(area, new List<DBObject>() { polylines[i] });
                }
                else
                {
                    polylineAreaDic[existAreas[0]].Add(polylines[i]);
                }
                List<Point3d> pts = ThColumnInfoUtils.GetPolylinePts(polylines[i] as Curve);
                List<Point2d> polyline2ds = new List<Point2d>();
                pts.ForEach(j => polyline2ds.Add(new Point2d(j.X, j.Y)));

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
        /// <summary>
        /// 分割框内部
        /// </summary>
        /// <param name="polylineObjs"></param>
        /// <param name="bsideNum"></param>
        /// <param name="hSideNum"></param>
        private void SplitInsidePolylines(List<DBObject> polylineObjs,out int bsideNum,out int hSideNum)
        {
            bsideNum = 0;
            hSideNum = 0;
            List<Curve> polylines = new List<Curve>();
            for (int i=0;i< polylineObjs.Count;i++)
            {
                if(polylineObjs[i] is Polyline || polylineObjs[i] is Polyline2d || polylineObjs[i] is Polyline3d ||
                    polylineObjs[i] is Line)
                {
                    polylines.Add(polylineObjs[i] as Curve);
                }
            }
            List<Curve> noRepeatedPolylines = new List<Curve>();
            while (polylines.Count>0)
            {
                Curve currentLine = polylines[0];
                noRepeatedPolylines.Add(currentLine);
                polylines=polylines.Where(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).DistanceTo
                (ThColumnInfoUtils.GetMidPt(currentLine.Bounds.Value.MinPoint, currentLine.Bounds.Value.MaxPoint)) > 5.0).Select(i=>i).ToList();
            }
            polylines = noRepeatedPolylines;
            List<double> xValues= polylines.Select(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).X).ToList();
            List<double> yValues = polylines.Select(i => ThColumnInfoUtils.GetMidPt(i.Bounds.Value.MinPoint, i.Bounds.Value.MaxPoint).Y).ToList();

            double minX = xValues.OrderBy(i => i).First();
            double minY = yValues.OrderBy(i => i).First();
            List<double> hSides = xValues.Where(i => Math.Abs(i - minX) <= 10.0).Select(i => i).ToList();
            List<double> bSides = yValues.Where(i => Math.Abs(i - minY) <= 10.0).Select(i => i).ToList();
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
        private Dictionary<Curve, Point3d> GetLeaderLine(List<Point3d> boundaryPts)
        {
            Dictionary<Curve, Point3d> linePtDic = new Dictionary<Curve, Point3d>();
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
                  new TypedValue((int)DxfCode.Start,"LINE,LWPOLYLINE") //后续如果需要，根据配置来设置过滤图层
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
            ThProgressBar.MeterProgress();
            List<Curve> lines = new List<Curve>();
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                lines = objIds.Where(i=> 
                {
                    Entity ent = ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, i);
                    if(ent is Line || ent is Polyline || ent is Polyline2d || ent is Polyline3d)
                    {
                        return true;
                    }
                    return false;
                }).Select(i => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, i) as Curve).ToList();
            }
            ThProgressBar.MeterProgress();
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
                ThProgressBar.MeterProgress();
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
            List<Point3d> cornerPts = ThColumnInfoUtils.GetRetanglePts(pts);
            Point3d minPt = cornerPts[0];
            Point3d maxPt = cornerPts[1];
            if((minPt.X> this.rangePt1.X && minPt.X < this.rangePt2.X) &&
                (minPt.Y > this.rangePt1.Y && minPt.Y < this.rangePt2.Y)
                )
            {
                return true;
            }
            if ((maxPt.X > this.rangePt1.X && maxPt.X < this.rangePt2.X) &&
                (maxPt.Y > this.rangePt1.Y && maxPt.Y < this.rangePt2.Y)
                )
            {
                return true;
            }
            return isIn;
        }
        private List<List<Point3d>> FindColumnPositions(BlockReference br)
        {
            List<List<Point3d>> points = new List<List<Point3d>>();
            if(br==null)
            {
                return points;
            }
            BlockTableRecord btr = doc.TransactionManager.TopTransaction.GetObject(br.BlockTableRecord, OpenMode.ForRead)as BlockTableRecord;
            List<Curve> curves = new List<Curve>();
            foreach (var id in btr)
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
                    int count=this.layerList.Where(i => i.ToUpper() == polyline.Layer.ToUpper()).Select(i => i).Count();
                    if (count>0) //根据图层名来搜索(从参数设置中来获取)
                    {
                        List<Point3d> polylinePts = new List<Point3d>();
                        for(int i=0;i<polyline.NumberOfVertices;i++)
                        {
                            polylinePts.Add(polyline.GetPoint3dAt(i));
                        }
                        points.Add(polylinePts);
                    }
                }
                else if(dbObj is Line)
                {
                    curves.Add(dbObj as Curve);
                }
            }
            List<List<Point3d>> loopPoints = GetLoopPoints(curves);
            if(loopPoints.Count>0)
            {
                points.AddRange(loopPoints);
            }
            for (int i=0;i< points.Count;i++)
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
        private List<List<Point3d>> GetLoopPoints(List<Curve> curves)
        {
            List<List<Point3d>> loopPoints = new List<List<Point3d>>();
            while (curves.Count > 0)
            {
                List<Point3d> loopPts = new List<Point3d>();
                loopPts.Add(curves[0].StartPoint);
                Point3d findPt = curves[0].EndPoint;
                curves.RemoveAt(0);
                for (int i = 0; i < curves.Count; i++)
                {
                    if (findPt.DistanceTo(curves[i].StartPoint) <= 5.0)
                    {
                        loopPts.Add(curves[i].StartPoint);
                        findPt = curves[i].EndPoint;
                        curves.RemoveAt(i);
                        i = -1;
                    }
                    else if (findPt.DistanceTo(curves[i].EndPoint) <= 5.0)
                    {
                        loopPts.Add(curves[i].EndPoint);
                        findPt = curves[i].StartPoint;
                        curves.RemoveAt(i);
                        i = -1;
                    }
                }
                if(loopPts.Count>2)
                {
                    if(findPt.DistanceTo(loopPts[0])<=5.0)
                    {
                        loopPoints.Add(loopPts);
                    }
                }
            }
            return loopPoints;
        }
        private List<List<Curve>> GetCurveLoops(List<Curve> curves)
        {
            List<List<Curve>> totalLoopCurves = new List<List<Curve>>();
            while (curves.Count>0)
            {
                List<Curve> loopCurves = new List<Curve>();
                loopCurves.Add(loopCurves[0]);
                curves.RemoveAt(0);
                for(int i=0;i< curves.Count;i++)
                {
                    if(curves[i].StartPoint.DistanceTo(loopCurves[loopCurves.Count-1].StartPoint)<=5.0 ||
                            curves[i].StartPoint.DistanceTo(loopCurves[loopCurves.Count - 1].EndPoint) <= 5.0)
                    {
                        loopCurves.Add(curves[i]);
                        curves.RemoveAt(i);
                        i = -1;
                    }
                    else if(curves[i].EndPoint.DistanceTo(loopCurves[loopCurves.Count - 1].StartPoint) <= 5.0 ||
                            curves[i].EndPoint.DistanceTo(loopCurves[loopCurves.Count - 1].EndPoint) <= 5.0)
                    {
                        loopCurves.Add(curves[i]);
                        curves.RemoveAt(i);
                        i = -1;
                    }
                }
                if(loopCurves.Count>=3)
                {
                    totalLoopCurves.Add(loopCurves);
                }
            }
            return totalLoopCurves;
        }
        private List<BlockReference> GetXRefColumn()
        {
            List<BlockReference> brs = new List<BlockReference>();
            Point3d selectPt1 = this.rangePt1.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d selectPt2 = this.rangePt2.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Insert") };
            SelectionFilter sf = new SelectionFilter(tvs);       
            PromptSelectionResult psr = this.doc.Editor.SelectCrossingWindow(selectPt1,selectPt2, sf);
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
        /// <summary>
        /// 打印识别后的柱框
        /// </summary>
        public void PrintColumnFrame()
        {
            for(int i=0;i<this.ColumnInfs.Count;i++)
            {
                ObjectId frameId = ThColumnInfoUtils.DrawOffsetColumn(
                            this.ColumnInfs[i].Points, PlantCalDataToDraw.offsetDisScale, true, PlantCalDataToDraw.lineWidth);
                System.Drawing.Color sysColor= System.Drawing.Color.White;
                Autodesk.AutoCAD.Colors.Color acadColor;
                switch (this.ColumnInfs[i].Error)
                {
                    case ErrorMsg.OK:
                        sysColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.Related);
                        break;
                    case ErrorMsg.InfNotCompleted:
                        sysColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.ParameterNotFull);
                        break;
                    case ErrorMsg.CodeEmpty:
                        sysColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.ColumnLost);
                        break;
                }
                acadColor = ThColumnInfoUtils.SystemColorToAcadColor(sysColor);
                ThColumnInfoUtils.ChangeColor(frameId, acadColor.ColorIndex);
                this.ColumnInfs[i].FrameId = frameId;
            }
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
