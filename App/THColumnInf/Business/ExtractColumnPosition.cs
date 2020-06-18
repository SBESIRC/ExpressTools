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
       
        private double searchColumnPolylineDis = 5.0;
        private SelectionFilter polylineSf;
        private SelectionFilter textSf;
        private double textSize = 0.0;
        
        private List<string> layerList = new List<string>();
        private ThStandardSign thStandardSign;
        private ParameterSetInfo paraSetInfo;
        private bool extractColumnPos = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thStandardSign"></param>
        /// <param name="extractColumnGeometry"></param>
        public ExtractColumnPosition(ThStandardSign thStandardSign,bool extractColumnPos=false)
        {
            this.extractColumnPos = extractColumnPos;
            thStandardSign.SignExtractColumnInfo = this;
            this.thStandardSign = thStandardSign;
            ParameterSetVM parameterSetVM = new ParameterSetVM();
            this.paraSetInfo = parameterSetVM.ParaSetInfo;
            Update();
            Init();
        }
        public void Update()
        {
            var extents = ThColumnInfoUtils.GeometricExtentsImpl(thStandardSign.Br);
            if (extents != null)
            {
                this.rangePt1 = extents.MinPoint;
                this.rangePt2 = extents.MaxPoint;
            }
            else
            {
                BlockReferenceGeometryExtents3d brge = new BlockReferenceGeometryExtents3d(thStandardSign.Br);
                brge.GeometryExtents3dBestFit();
                this.rangePt1 = brge.GeometryExtents3d.Value.MinPoint;
                this.rangePt2 = brge.GeometryExtents3d.Value.MaxPoint;
            }
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
        /// <summary>
        /// 无效的柱表
        /// </summary>
        public List<ColumnTableRecordInfo> InvalidCtris { get; set; } = new List<ColumnTableRecordInfo>();
        private bool importCalInfo = false;

        public void Extract(bool importCalInfo =false)
        {
            this.importCalInfo = importCalInfo;
            ViewTableRecord view = doc.Editor.GetCurrentView();
            try
            {
                ParameterSetVM parameterSetVM = new ParameterSetVM();
                this.paraSetInfo = parameterSetVM.ParaSetInfo;
                ClearColumnInfs();
                COMTool.ZoomWindow(ThColumnInfoUtils.TransPtFromUcsToWcs(this.rangePt1)
                    , ThColumnInfoUtils.TransPtFromUcsToWcs(this.rangePt2));
                //提取柱表
                ExtractColumnTable extractColumnTable = new ExtractColumnTable(this.rangePt1, this.rangePt2, this.paraSetInfo); //如果不是原位图纸，提取一下柱表信息
                extractColumnTable.Extract();
                this.ColumnTableRecordInfos = extractColumnTable.ColumnTableRecordInfos;
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
        public void ClearColumnInfs()
        {
            if(this.ColumnInfs.Count==0)
            {
                return;
            }
            List<ObjectId> frameIds = this.ColumnInfs.Select(i => i.FrameId).ToList();
            ThColumnInfoUtils.EraseObjIds(frameIds.ToArray());
            this.ColumnInfs.Clear();
        }
        public List<List<Point3d>> GetRangeColumnPoints()
        {
            List<List<Point3d>> pts = new List<List<Point3d>>();
            try
            {
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
            this.ColumnTableRecordInfos.ForEach(i => i.Handle());
            //如果导入计算书，就不要用柱表来判断
            InvalidCtris.AddRange(this.ColumnTableRecordInfos.Where(i => !i.Validate()).Select(i => i).ToList());
            for (int i = 0; i < this.ColumnInfs.Count; i++)
            {
                if (string.IsNullOrEmpty(this.ColumnInfs[i].Code))
                {
                    this.ColumnInfs[i].Error = ErrorMsg.CodeEmpty;
                    continue;
                }
                if(this.importCalInfo)
                {
                    //如果是导入计算书，只要有柱号，就是可以参与匹配的柱子
                    this.ColumnInfs[i].Error = ErrorMsg.OK;
                    continue;
                }
                int errorCount=InvalidCtris.Where(j => j.Code == this.ColumnInfs[i].Code).Select(j => j).Count();
                if(errorCount>0)
                {
                    this.ColumnInfs[i].Error = ErrorMsg.InfNotCompleted;
                    continue;
                }
                int correctCount = this.ColumnTableRecordInfos.
                    Where(j => j.Code == this.ColumnInfs[i].Code).Select(j => j).ToList().Count();
                if(correctCount>0)
                {
                    this.ColumnInfs[i].Error = ErrorMsg.OK;
                }
                else
                {
                    this.ColumnInfs[i].Error = ErrorMsg.InfNotCompleted;
                }
            }
        }
        private void FindColumnInfo()
        {
            for (int i = 0; i < this.allColumnBoundaryPts.Count; i++)
            {
                if (this.allColumnBoundaryPts[i].Count < 2)
                {
                    continue;
                }
                ColumnInf columnInfo = new ColumnInf();
                columnInfo.Points = this.allColumnBoundaryPts[i];
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
                this.ColumnInfs.Add(columnInfo);
                ThProgressBar.MeterProgress();
            }
        }
        /// <summary>
        /// 获取柱子的信息
        /// </summary>
        /// <param name="line">柱子伸出来的线</param>
        /// <param name="searchPt">用于查找的点 Wcs Point3d</param>
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
                            BuildInSituMarkInf buildInSituMarkInf = new BuildInSituMarkInf(polylines[i], dBTexts);
                            buildInSituMarkInf.Build();
                            columnCode = buildInSituMarkInf.Ctri.Code;
                            if (buildInSituMarkInf.Ctri.Validate())
                            {
                                columnInf.HasOrigin = true;
                                this.ColumnTableRecordInfos.Add(buildInSituMarkInf.Ctri);
                                break;
                            }
                            else
                            {
                                InvalidCtris.Add(buildInSituMarkInf.Ctri);
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
        /// <param name="pt">引线一端要搜索的点 Wcs Point3d</param>
        /// <returns></returns>
        private List<DBText> GetMarkTexts(Curve leaderLine,Point3d pt)
        {
            List<DBText> findTexts = new List<DBText>();
            double searchRecLength = 1.5 * this.textSize;
            Point3d recCenPt = pt.TransformBy(doc.Editor.CurrentUserCoordinateSystem.Inverse());
            Point3d recPt1 = recCenPt + new Vector3d(searchRecLength / 2.0, searchRecLength / 2.0, 0.0);
            Point3d recPt2 = recCenPt + new Vector3d(-searchRecLength / 2.0, searchRecLength / 2.0, 0.0);
            Point3d recPt3 = recCenPt + new Vector3d(-searchRecLength / 2.0, -searchRecLength / 2.0, 0.0);
            Point3d recPt4 = recCenPt + new Vector3d(searchRecLength / 2.0, -searchRecLength / 2.0, 0.0);
            Point3dCollection recPts = new Point3dCollection();
            recPts.Add(recPt1);
            recPts.Add(recPt2);
            recPts.Add(recPt3);
            recPts.Add(recPt4);
            List<ObjectId> findDbTextIds = new List<ObjectId>();
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByPolyline(this.doc.Editor,
                recPts, PolygonSelectionMode.Crossing,this.textSf);
            if (psr.Status == PromptStatus.OK)
            {
                findDbTextIds=psr.Value.GetObjectIds().ToList();
            }
            List<DBText> dBTexts = findDbTextIds.Select(j => ThColumnInfoDbUtils.GetEntity(Application.DocumentManager.MdiActiveDocument.Database, j) as DBText).ToList();
            List<DBText> findCodeRes = findCodeRes = dBTexts.Where(j => BaseFunction.IsColumnCode(j.TextString)).Select(j => j).ToList();
            if(findCodeRes.Count==0)
            {
                List<Point3d> textPts = new List<Point3d>();
                foreach(DBText dbText in dBTexts)
                {
                    Extents3d extents = ThColumnInfoUtils.GeometricExtentsImpl(dbText);
                    if(extents!=null)
                    {
                        textPts.Add(extents.MinPoint);
                        textPts.Add(extents.MaxPoint);                       
                    }
                }
                double minX = textPts.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
                double minY = textPts.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
                double minZ = textPts.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

                double maxX = textPts.OrderByDescending(i => i.X).Select(i => i.X).FirstOrDefault();
                double maxY = textPts.OrderByDescending(i => i.Y).Select(i => i.Y).FirstOrDefault();
                double maxZ = textPts.OrderByDescending(i => i.Z).Select(i => i.Z).FirstOrDefault();

                Point3d searchPt1 = new Point3d(minX, minY, minZ);
                Point3d searchPt2 = new Point3d(maxX, maxY + 5*this.textSize, minZ);                
                psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, searchPt1, searchPt2, PolygonSelectionMode.Crossing, this.textSf);
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
                    psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, searchPt1, searchPt2, PolygonSelectionMode.Crossing, this.textSf);
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
            Extents3d codeTextExtents = ThColumnInfoUtils.GeometricExtentsImpl(codeText);
            Point3d minPt = codeTextExtents.MinPoint;
            Point3d maxPt = codeTextExtents.MaxPoint;
            maxPt = new Point3d(maxPt.X, minPt.Y - 5 * this.textSize, minPt.Z);           
            psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, minPt, maxPt, PolygonSelectionMode.Crossing, this.textSf);
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
        /// 获取标注引线
        /// </summary>
        /// <returns></returns>
        private Dictionary<Curve, Point3d> GetLeaderLine(List<Point3d> boundaryPts)
        {
            //返回Curve,Wcs Point3d
            Dictionary<Curve, Point3d> linePtDic = new Dictionary<Curve, Point3d>();
            List<Point3d> ucsBoundaryPts = boundaryPts.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(i)).ToList();
            double xMin = ucsBoundaryPts.OrderBy(i => i.X).Select(i=>i.X).First();
            double xMax = ucsBoundaryPts.OrderByDescending(i => i.X).Select(i => i.X).First();

            double yMin = ucsBoundaryPts.OrderBy(i => i.Y).Select(i => i.Y).First();
            double yMax = ucsBoundaryPts.OrderByDescending(i => i.Y).Select(i => i.Y).First();
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

            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor,
                pt1, pt2, PolygonSelectionMode.Crossing,sf);
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
                if (ThColumnInfoUtils.IsPointInPolyline(offsetRecPts, ThColumnInfoUtils.TransPtFromWcsToUcs(lines[i].StartPoint))) //线的起点在
                {
                    startInside = true;
                }
                if (ThColumnInfoUtils.IsPointInPolyline(offsetRecPts, ThColumnInfoUtils.TransPtFromWcsToUcs(lines[i].EndPoint))) //线的起点在
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
                    Point3dCollection pts = new Point3dCollection();
                    foreach(Point3d pt in offsetRecPts)
                    {
                        pts.Add(ThColumnInfoUtils.TransPtFromUcsToWcs(pt));
                    }
                    Polyline recOffsetPolyline= ThColumnInfoUtils.CreatePolyline(pts);
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
            List<Point3d> ucsPts = pts.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(i)).ToList();
            List<Point3d> cornerPts = ThColumnInfoUtils.GetRetanglePts(ucsPts);
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
            BlockTableRecord btr = doc.TransactionManager.TopTransaction.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
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
                else if(dbObj is Polyline polyline)
                {                   
                    int count=this.layerList.Where(i => i.ToUpper() == polyline.Layer.ToUpper()).Select(i => i).Count();
                    if (count > 0) //根据图层名来搜索(从参数设置中来获取)
                    {
                        List<Point3d> polylinePts = new List<Point3d>();                        
                        for (int i = 0; i < polyline.NumberOfVertices; i++)
                        {
                            polylinePts.Add(polyline.GetPoint3dAt(i));
                        }
                        if (polylinePts[0].DistanceTo(polylinePts[1]) > 2000)
                        {
                            Point3d debugPt = ThColumnInfoUtils.TransPtFromWcsToUcs(polyline.GetPoint3dAt(0));
                        }
                        points.Add(polylinePts);
                    }
                }
                else if(dbObj is Polyline2d polyline2d)
                {
                    int count = this.layerList.Where(i => i.ToUpper() == polyline2d.Layer.ToUpper()).Select(i => i).Count();
                    if (count > 0) //根据图层名来搜索(从参数设置中来获取)
                    {
                        List<Point3d> polylinePts = new List<Point3d>();
                        foreach (var vertex in polyline2d)
                        {
                            ObjectId vertexId = (ObjectId)vertex;
                            Vertex2d vertex2d= doc.TransactionManager.TopTransaction.GetObject(vertexId, OpenMode.ForRead) as Vertex2d;
                            polylinePts.Add(polyline2d.VertexPosition(vertex2d));
                        }
                        points.Add(polylinePts);
                    }
                   
                }
                else if(dbObj is Line line)
                {
                    int count = this.layerList.Where(i => i.ToUpper() == line.Layer.ToUpper()).Select(i => i).Count();
                    if(count>0)
                    {
                        curves.Add(dbObj as Curve);
                    }
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
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Insert") };
            SelectionFilter sf = new SelectionFilter(tvs);       
            PromptSelectionResult psr = this.doc.Editor.SelectCrossingWindow(this.rangePt1, this.rangePt2, sf);
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
        public void PrintColumnFrame(bool show = true)
        {
            for (int i = 0; i < this.ColumnInfs.Count; i++)
            {
                ObjectId frameId = ThColumnInfoUtils.DrawOffsetColumn(
                            this.ColumnInfs[i].Points, PlantCalDataToDraw.offsetDisScale, true, PlantCalDataToDraw.lineWidth);
                System.Drawing.Color sysColor = System.Drawing.Color.White;
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
                ThColumnInfoUtils.EraseObjIds(this.ColumnInfs[i].FrameId);
                this.ColumnInfs[i].FrameId = frameId;
            }
            if (!show)
            {
                List<ObjectId> hideObjIds = this.ColumnInfs.Select(i => i.FrameId).ToList();
                ThColumnInfoUtils.ShowObjIds(hideObjIds.ToArray(), show);
            }
        }
        /// <summary>
        /// 打印识别后的柱框
        /// </summary>
        public void PrintErrorColumnFrame(bool show = true)
        {
            for (int i = 0; i < this.ColumnInfs.Count; i++)
            {
                if(this.ColumnInfs[i].Error== ErrorMsg.OK)
                {
                    continue;
                }
                ObjectId frameId = ThColumnInfoUtils.DrawOffsetColumn(
                            this.ColumnInfs[i].Points, PlantCalDataToDraw.offsetDisScale, true, PlantCalDataToDraw.lineWidth);
                System.Drawing.Color sysColor = System.Drawing.Color.White;
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
            if (!show)
            {
                List<ObjectId> hideObjIds = this.ColumnInfs.Select(i => i.FrameId).ToList();
                ThColumnInfoUtils.ShowObjIds(hideObjIds.ToArray(), show);
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
