using System.Collections.Generic;
using ThColumnInfo.ViewModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using System.Linq;
using System;
using ThColumnInfo.Validate;
using Autodesk.AutoCAD.EditorInput;

namespace ThColumnInfo
{
    public class PlantCalDataToDraw
    {
        private CalculationInfo calculationInfo = null;
        private ThDrawColumns thDrawColumns = null;
        private List<List<Point3d>> thColumnPoints = new List<List<Point3d>>();
        public static double offsetDisScale = 3.0;
        public static double lineWidth = 200;
        private string thColumnRegAppName = "ThColumnCalculation";
        private ExtractYjkColumnInfo yjkCalculateDb = null;
        private ExtractYjkColumnInfo yjkModelDb = null;
        private Document document = null;
        private ParameterSetInfo paraSetInfo = new ParameterSetInfo();
        private string yjkData1KeyName = "YjkData1";
        private string yjkData2KeyName = "YjkData2";
        private string customKeyName = "CustomData";
        private string yjkColumnKeyName = "YjkColumnData";

        private List<ObjectId> columnFrameIds = new List<ObjectId>(); //正柱子外围的框(包括关联和没关联到计算书中的柱子)
        private List<ObjectId> unrelatedFrameIds = new List<ObjectId>(); //计算书中的柱子没有关联到本地图纸的柱子
        private List<ObjectId> exceptionFrameIds = new List<ObjectId>(); //计算书中柱子关联很多柱子

        private List<ObjectId> showFrameTextIds = new List<ObjectId>();
        private ThStandardSign thStandardSign;

        /// <summary>
        /// 用户构件属性定义在实体中的Key值
        /// </summary>
        public string CustomKeyName
        {
            get
            {
                return customKeyName;
            }
        }

        /// <summary>
        /// 图纸上的柱子和计算书上的柱子正常关联
        /// </summary>
        public List<ObjectId> ColumnFrameIds
        {
            get { return columnFrameIds; }
        }
        /// <summary>
        /// 图纸上的柱子和计算书上的柱子无关联
        /// </summary>
        public List<ObjectId> UnrelatedFrameIds
        {
            get { return unrelatedFrameIds; }
        }
        /// <summary>
        /// 计算书中的柱子关联很多柱子
        /// </summary>
        public List<ObjectId> ExceptionFrameIds
        {
            get { return exceptionFrameIds; }
        }
        /// <summary>
        /// 计算书信息
        /// </summary>
        public CalculationInfo CalInfo
        {
            get { return calculationInfo; }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="calculationInfo"></param>
        public PlantCalDataToDraw(CalculationInfo calculationInfo, ThStandardSign thStandardSign)
        {
            this.calculationInfo = calculationInfo;
            this.thStandardSign = thStandardSign;
            yjkCalculateDb = new ExtractYjkColumnInfo(calculationInfo.GetDtlCalcFullPath());
            yjkModelDb = new ExtractYjkColumnInfo(calculationInfo.GetDtlmodelFullPath());
            Init();
        }
        public PlantCalDataToDraw(ThStandardSign thStandardSign)
        {
            this.thStandardSign = thStandardSign;
            Init();
        }
        private bool loadParameterSet = true;
        public PlantCalDataToDraw(bool loadParameterSet = true)
        {
            this.loadParameterSet = loadParameterSet;
            Init();
        }
        private void Init()
        {
            document = ThColumnInfoUtils.GetMdiActiveDocument();
            if (this.loadParameterSet)
            {
                //参数设置
                ParameterSetVM psVM = new ParameterSetVM();
                this.paraSetInfo = psVM.ParaSetInfo;
            }
        }
        public bool Plant()
        {
            bool result = true;
            try
            {
                if (string.IsNullOrEmpty(this.calculationInfo.YjkPath) ||
                this.calculationInfo.SelectLayers.Count == 0)
                {
                    return false;
                }
                //获取计算书路径对应的自然层的信息
                CalculationInfoVM calculationInfoVM = new CalculationInfoVM(calculationInfo);
                List<FloorInfo> floorInfs = calculationInfoVM.LoadYjkDbInfo();
                ThProgressBar.MeterProgress();
                //用户选择的自然层
                string dtlModelPath = calculationInfo.GetDtlmodelFullPath();
                List<string> selFloors = calculationInfoVM.GetSelectFloors();
                ThProgressBar.MeterProgress();
                List<FloorInfo> selectFloorInfs = new List<FloorInfo>();
                for (int i = 0; i < floorInfs.Count; i++)
                {
                    if (selFloors.IndexOf(floorInfs[i].Name) >= 0)
                    {
                        selectFloorInfs.Add(floorInfs[i]);
                    }
                }
                //提取柱子信息
                IDatabaseDataSource dbDataSource = new ExtractYjkColumnInfo(dtlModelPath);
                dbDataSource.Extract(selectFloorInfs[0].No);
                ThProgressBar.MeterProgress();
                //让用户指定柱子的位置
                this.thDrawColumns = new ThDrawColumns(dbDataSource.ColumnInfs, this.calculationInfo);
                thDrawColumns.Draw();
                ThProgressBar.MeterProgress();
                if (this.thDrawColumns.IsGoOn)
                {
                    if(this.thStandardSign==null)
                    {
                        this.showFrameTextIds = ShowFrameTextIds(this.thStandardSign.SignExtractColumnInfo);
                    }
                    else
                    {
                        this.showFrameTextIds = ShowFrameTextIds();
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "Plant");
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 埋入数据
        /// </summary>
        public void Embed()
        {
            //提取埋入的柱子范围内对应的本地图纸柱子,(校核命令可提取柱子数量)
            if (this.thStandardSign.SignExtractColumnInfo.ColumnInfs.Count == 0)
            {
                return;
            }
            this.unrelatedFrameIds.Clear();
            this.exceptionFrameIds.Clear();
            this.columnFrameIds.Clear();
            //获取数据信息完整的柱子与计算书中的柱子比对
            List<ColumnInf> columnInfs = this.thStandardSign.SignExtractColumnInfo.ColumnInfs.Where(i => i.Error == ErrorMsg.OK).Select(i => i).ToList();

            //关联计算书的柱子和本地图纸的柱子
            ThRelateColumn thRelateColumn = new ThRelateColumn(columnInfs, thDrawColumns.ColumnRelateInfs);
            thRelateColumn.Relate();

            //绘制正常或异常的柱子外框
            for (int i = 0; i < thRelateColumn.ColumnRelateInfs.Count; i++)
            {
                if (thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs != null &&
                    thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs.Count == 1)
                {
                    ObjectId objectId = thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs[0].FrameId;
                    //图纸中的柱子关联到计算书中的柱子
                    if(objectId== ObjectId.Null || objectId.IsErased || objectId.IsValid==false)
                    {
                        objectId = ThColumnInfoUtils.DrawOffsetColumn(thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs[0].Points,
                        offsetDisScale, true, lineWidth);
                    }
                    ChangeColor(objectId, FrameColor.Related);
                    if (objectId != ObjectId.Null)
                    {
                        this.columnFrameIds.Add(objectId);
                    }
                    DrawCalculation(thRelateColumn.ColumnRelateInfs[i]);
                }
                else if (thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs == null ||
                    thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs.Count == 0)
                {
                    //计算书中有柱子，图纸中没有找到柱子
                    ObjectId objectId = ThColumnInfoUtils.DrawOffsetColumn(thRelateColumn.ColumnRelateInfs[i].InModelPts,
                        offsetDisScale,true,lineWidth);
                    ChangeColor(objectId, FrameColor.DwgNotCalHas);
                    if (objectId != ObjectId.Null)
                    {
                        this.unrelatedFrameIds.Add(objectId);
                    }
                }
                else if (thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs.Count > 1)
                {
                    //计算书中的柱子关联很多个内框内的柱子
                    ObjectId objectId = ThColumnInfoUtils.DrawOffsetColumn(
                        thRelateColumn.ColumnRelateInfs[i].InModelPts,offsetDisScale,true,lineWidth);
                    this.exceptionFrameIds.Add(objectId);
                }
                ThProgressBar.MeterProgress();
            }
            //内框内正确柱子，还剩下没有关联到计算书中的柱子
            for (int i = 0; i < thRelateColumn.RestColumnInfs.Count; i++)
            {
                ObjectId objectId = thRelateColumn.RestColumnInfs[i].FrameId;
                //图纸中的柱子没有关联到计算书中的柱子
                if (objectId == ObjectId.Null || objectId.IsErased || objectId.IsValid == false)
                {
                    objectId = objectId = ThColumnInfoUtils.DrawOffsetColumn(
                    thRelateColumn.RestColumnInfs[i].Points, offsetDisScale, true, lineWidth);
                }
                ChangeColor(objectId, FrameColor.DwgHasCalNot);
                if (objectId != ObjectId.Null)
                {
                    this.columnFrameIds.Add(objectId);
                }
                ThProgressBar.MeterProgress();
            }
        }
        /// <summary>
        /// 埋入数据
        /// </summary>
        public void DrawColumnOriginFrame()
        {
            //提取埋入的柱子范围内对应的本地图纸柱子,(校核命令可提取柱子数量)
            if (this.thStandardSign == null || 
                this.thStandardSign.SignExtractColumnInfo==null ||
                this.thStandardSign.SignExtractColumnInfo.ColumnInfs.Count == 0)
            {
                return;
            }
            //获取数据信息完整的柱子与计算书中的柱子比对
            List<ColumnInf> columnInfs = this.thStandardSign.SignExtractColumnInfo.ColumnInfs.Where(
                i => i.Points.Count > 2).Select(i => i).ToList();
            //绘制正常或异常的柱子外框
            for (int i = 0; i < columnInfs.Count; i++)
            {
                if (columnInfs[i].Points.Count>0)
                {
                    //图纸中的柱子关联到计算书中的柱子
                    EmbedColumnCustom(columnInfs[i]);
                }
                ThProgressBar.MeterProgress();
            }
        }
        /// <summary>
        /// 删除计算书导入后产生的柱框线和文字
        /// </summary>
        public void EraseFrameTextIds(bool eraseText=true)
        {
            try
            {
                List<ObjectId> eraseObjIds = new List<ObjectId>();
                using (Transaction trans = document.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in this.showFrameTextIds)
                    {
                        if (id == ObjectId.Null || id.IsErased || !id.IsValid)
                        {
                            continue;
                        }
                        if (trans.GetObject(id, OpenMode.ForRead) is DBText)
                        {
                            if (!eraseText)
                            {
                                continue;
                            }
                        }
                        eraseObjIds.Add(id);
                    }
                    trans.Commit();
                }
                ThColumnInfoUtils.EraseObjIds(eraseObjIds.ToArray());
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "EraseFrameTextIds");
            }
        }
        public void ShowFrameTextIds(bool showFrame,bool showText,bool isShow=false)
        {
            try
            {
                List<ObjectId> filterIds = new List<ObjectId>();

                Document doc = Application.DocumentManager.MdiActiveDocument;
                using (Transaction trans=doc.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objId in this.showFrameTextIds)
                    {
                        DBObject dBObj = trans.GetObject(objId,OpenMode.ForRead);
                        if(showFrame)
                        {
                            if(dBObj is Polyline)
                            {
                                filterIds.Add(objId);
                            }
                        }
                        if (showText)
                        {
                            if (dBObj is DBText)
                            {
                                filterIds.Add(objId);
                            }
                        }
                    }
                    trans.Commit();
                }
                ThColumnInfoUtils.ShowObjIds(filterIds.ToArray(), isShow);
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "EraseFrameTextIds");
            }
        }
        /// <summary>
        /// 埋入数据
        /// </summary>
        private List<ObjectId> ShowFrameTextIds(ExtractColumnPosition extractColumnPosition=null)
        {
            List<ObjectId> frameTextIds = new List<ObjectId>();
            if (this.thStandardSign == null)
            {
                return frameTextIds;
            }
            if(extractColumnPosition==null)
            {
                extractColumnPosition = new ExtractColumnPosition(thStandardSign);
            }
            extractColumnPosition.Extract();
            //获取数据信息完整的柱子与计算书中的柱子比对
            List<ColumnInf> columnInfs = extractColumnPosition.ColumnInfs.Where(i => i.Points.Count>2).Select(i => i).ToList();
            //绘制正常或异常的柱子外框
            for (int i = 0; i < columnInfs.Count; i++)
            {
                if (columnInfs[i].Points.Count > 0)
                {
                    //图纸中的柱子关联到计算书中的柱子
                    EmbedColumnCustom(columnInfs[i]);
                    ThProgressBar.MeterProgress();
                }
            }

            //关联计算书的柱子和本地图纸的柱子
            ThRelateColumn thRelateColumn = new ThRelateColumn(columnInfs, this.thDrawColumns.ColumnRelateInfs);
            thRelateColumn.Relate();
            frameTextIds.AddRange(thRelateColumn.PrintJtID()); //打印柱号

            //绘制正常或异常的柱子外框
            for (int i = 0; i < thRelateColumn.ColumnRelateInfs.Count; i++)
            {
                if (thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs != null &&
                    thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs.Count == 1)
                {
                    //图纸中的柱子关联到计算书中的柱子
                    ObjectId objectId = ThColumnInfoUtils.DrawOffsetColumn(
                        thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs[0].Points, offsetDisScale, true,lineWidth);
                    ChangeColor(objectId, FrameColor.Related);
                    if (objectId != ObjectId.Null)
                    {
                        frameTextIds.Add(objectId);
                    }
                    DrawCalculation(thRelateColumn.ColumnRelateInfs[i]);                    
                }
                else if (thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs == null ||
                    thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs.Count == 0)
                {
                    //计算书中有柱子，图纸中没有找到柱子
                    ObjectId objectId = ThColumnInfoUtils.DrawOffsetColumn(
                        thRelateColumn.ColumnRelateInfs[i].InModelPts, offsetDisScale, true, lineWidth);
                    ChangeColor(objectId, FrameColor.DwgNotCalHas);
                    if (objectId != ObjectId.Null)
                    {
                        frameTextIds.Add(objectId);
                    }
                }
                else if (thRelateColumn.ColumnRelateInfs[i].ModelColumnInfs.Count > 1)
                {
                    //计算书中的柱子关联很多个内框内的柱子
                    ObjectId objectId = ThColumnInfoUtils.DrawOffsetColumn(
                        thRelateColumn.ColumnRelateInfs[i].InModelPts, offsetDisScale, true, lineWidth);
                    frameTextIds.Add(objectId);
                }
                ThProgressBar.MeterProgress();
            }
            //内框内正确柱子，还剩下没有关联到计算书中的柱子
            for (int i = 0; i < thRelateColumn.RestColumnInfs.Count; i++)
            {
                if(thRelateColumn.RestColumnInfs[i].Points.Count==0)
                {
                    continue;
                }
                //图纸中的柱子没有关联到计算书中的柱子
                ObjectId objectId = ThColumnInfoUtils.DrawOffsetColumn(
                    thRelateColumn.RestColumnInfs[i].Points, offsetDisScale, true, lineWidth);
                ChangeColor(objectId, FrameColor.DwgHasCalNot);
                if (objectId != ObjectId.Null)
                {
                    frameTextIds.Add(objectId);
                }
                ThProgressBar.MeterProgress();
            }
            return frameTextIds;
        }
        /// <summary>
        /// 关联计算书中正确的柱子
        /// </summary>
        /// <returns></returns>
        public void RelateCalulationColumn(ColumnInf columnInf)
        {
            if(columnInf.Points.Count==0)
            {
                return;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Point3dCollection pts = new Point3dCollection();
                columnInf.Points.ForEach(i => pts.Add(i));
                Polyline polyline = ThColumnInfoUtils.CreatePolyline(pts);
                foreach (ObjectId calColumnId in this.columnFrameIds)
                {
                    if (calColumnId == ObjectId.Null || calColumnId.IsErased || calColumnId.IsValid==false)
                    {
                        continue;
                    }
                    Curve columnFrame = trans.GetObject(calColumnId, OpenMode.ForRead) as Curve;
                    bool isOverLap= ThColumnInfoUtils.JudgeTwoCurveIsOverLap(polyline, columnFrame);
                    if(isOverLap)
                    {
                        columnInf.FrameId = calColumnId;
                        break;
                    }
                }
                polyline.Dispose();
                pts.Dispose();
                trans.Commit();
            }
        }
        public static System.Drawing.Color GetFrameSystemColor(FrameColor frameColor)
        {
            System.Drawing.Color color = System.Drawing.Color.Green;
            switch (frameColor)
            {
                case FrameColor.ColumnLost:
                    color = System.Drawing.Color.FromArgb(255, 0, 0);
                    break;
                case FrameColor.DwgHasCalNot:
                    color = System.Drawing.Color.FromArgb(0, 255, 255);
                    break;
                case FrameColor.DwgNotCalHas:
                    color = System.Drawing.Color.FromArgb(245, 154, 35);
                    break;
                case FrameColor.ParameterNotFull:
                    color = System.Drawing.Color.FromArgb(194, 128, 255);
                    break;
                case FrameColor.Related:
                    color = System.Drawing.Color.FromArgb(0, 255, 0);
                    break;
            }
            return color;
        }
        public static void ChangeColor(ObjectId objId, FrameColor frameColor)
        {
            if(frameColor== FrameColor.None)
            {
                return;
            }
            if(objId==ObjectId.Null)
            {
                return;
            }
            System.Drawing.Color sysColor = GetFrameSystemColor(frameColor);
            Autodesk.AutoCAD.Colors.Color acadColor= ThColumnInfoUtils.SystemColorToAcadColor(sysColor);
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                ent.UpgradeOpen();
                ent.ColorIndex = acadColor.ColorIndex;
                trans.Commit();
            }
        }
        /// <summary>
        /// 获取柱外框颜色类型
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public FrameColor GetFrameIdColorType(ObjectId frameId)
        {
            FrameColor frameColor = FrameColor.None;
            if (frameId == ObjectId.Null)
            {
                return frameColor;
            }
            List<FrameColor> frameColors = new List<FrameColor> { FrameColor.ColumnLost,FrameColor.DwgHasCalNot,FrameColor.DwgNotCalHas,
            FrameColor.ParameterNotFull,FrameColor.Related};
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(frameId, OpenMode.ForRead) as Entity;
                foreach (FrameColor fc in frameColors)
                {
                    System.Drawing.Color sysColor = GetFrameSystemColor(fc);
                    Autodesk.AutoCAD.Colors.Color acadColor = ThColumnInfoUtils.SystemColorToAcadColor(sysColor);
                    if (ent.ColorIndex == acadColor.ColorIndex)
                    {
                        frameColor = fc;
                        break;
                    }
                }
                trans.Commit();
            }
            return frameColor;
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
            if (pts1 == null || pts2 == null || pts1.Count == 0 || pts2.Count == 0)
            {
                return isOverLap;
            }
            Polyline polyline1 = CreateZeroPolyline(pts1);
            Polyline polyline2 = CreateZeroPolyline(pts2);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0)
            {
                Point2dCollection pts1Col = new Point2dCollection();
                Point2dCollection pts2Col = new Point2dCollection();
                pts1.ForEach(i => pts1Col.Add(new Point2d(i.X, i.Y)));
                pts2.ForEach(i => pts2Col.Add(new Point2d(i.X, i.Y)));
                if (ThColumnInfoUtils.IsPointInPolyline(pts2Col, new Point2d(pts1[0].X, pts1[0].Y)) ||
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
        private Polyline CreateZeroPolyline(List<Point3d> pts)
        {
            Polyline polyline = new Polyline();
            if (pts == null || pts.Count == 0)
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
        /// 将计算书埋入到图纸中
        /// </summary>
        private void DrawCalculation(ColumnRelateInf columnRelateInf)
        {
            //后续根据原则继续调整
            if(columnRelateInf.ModelColumnInfs.Count!=1)
            {
                return;
            }
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            ObjectId polylineId =GetUnVisibleColumn(columnRelateInf.ModelColumnInfs[0].Points);
            if(polylineId== ObjectId.Null)
            {
                Point3dCollection pts = new Point3dCollection();
                columnRelateInf.ModelColumnInfs[0].Points.ForEach(j => pts.Add(j));
                Polyline polyline = ThColumnInfoUtils.CreatePolyline(pts);
                polylineId = ThColumnInfoUtils.AddToBlockTable(polyline, true);
                List<TypedValue> tvs = new List<TypedValue>();
                tvs.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, "*"));
                ThColumnInfoUtils.AddXData(polylineId, this.thColumnRegAppName, tvs);
            }
            bool success = false;
            ColumnCustomData columnCustomData=ReadEmbededColumnCustomData(polylineId,out success); //埋入的构件属性定义数据
            YjkColumnDataInfo yjkColumnDataInfo = GetYjkDataInfo(columnRelateInf); //Yjk中的数据
            AddExtensionDictionary(polylineId, yjkColumnDataInfo, columnCustomData,columnRelateInf.DbColumnInf);
        }
        /// <summary>
        /// 埋入用户自定义数据
        /// </summary>
        /// <param name="columnInf"></param>
        /// <param name="columnCustomData"></param>
        public void EmbedColumnCustom(ColumnInf columnInf)
        {
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            ObjectId polylineId = GetUnVisibleColumn(columnInf.Points);
            if (polylineId == ObjectId.Null)
            {
                Point3dCollection pts = new Point3dCollection();
                columnInf.Points.ForEach(j => pts.Add(j));
                Polyline polyline = ThColumnInfoUtils.CreatePolyline(pts);
                polylineId = ThColumnInfoUtils.AddToBlockTable(polyline, true);
                List<TypedValue> tvs = new List<TypedValue>();
                tvs.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, "*"));
                ThColumnInfoUtils.AddXData(polylineId, this.thColumnRegAppName, tvs);
            }
            ThColumnInfoUtils.ShowObjId(polylineId, false);
            bool success = false;
            ColumnCustomData columnCustomData = ReadEmbededColumnCustomData(polylineId, out success); //埋入的构件属性定义数据
            WriteEmbededColumnCustomData(polylineId, columnCustomData);
        }
        public ObjectId GetUnVisibleColumn(List<Point3d> columnPts)
        {
            ObjectId columnObjId = ObjectId.Null;
            try
            {
                TypedValue[] tvs = new TypedValue[]
                {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName,this.thColumnRegAppName),
                new TypedValue((int)DxfCode.Start,"LWPOLYLINE")
                };
                double minX = columnPts.OrderBy(i => i.X).First().X;
                double minY = columnPts.OrderBy(i => i.Y).First().Y;
                double maxX = columnPts.OrderByDescending(i => i.X).First().X;
                double maxY = columnPts.OrderByDescending(i => i.Y).First().Y;
                Editor ed = this.document.Editor;
                SelectionFilter sf = new SelectionFilter(tvs);
                PromptSelectionResult psr = ed.SelectAll(sf);
                List<Polyline> polylines = new List<Polyline>();
                if (psr.Status == PromptStatus.OK)
                {
                    using (Transaction trans = this.document.TransactionManager.StartTransaction())
                    {
                        ObjectId[] selectObjIds = psr.Value.GetObjectIds();
                        foreach (ObjectId objId in selectObjIds)
                        {
                            Polyline polyline = trans.GetObject(objId, OpenMode.ForRead) as Polyline;
                            if (polyline.NumberOfVertices != 4)
                            {
                                continue;
                            }
                            if (((polyline.GeometricExtents.MinPoint.X >= minX && polyline.GeometricExtents.MinPoint.X <= maxX) &&
                                (polyline.GeometricExtents.MinPoint.Y >= minY && polyline.GeometricExtents.MinPoint.Y <= maxY)) ||
                                ((polyline.GeometricExtents.MaxPoint.X >= minX && polyline.GeometricExtents.MaxPoint.X <= maxX) &&
                                (polyline.GeometricExtents.MaxPoint.Y >= minY && polyline.GeometricExtents.MaxPoint.Y <= maxY))
                                )
                            {
                                polylines.Add(polyline);
                            }
                        }
                        if (polylines.Count == 0)
                        {
                            return columnObjId;
                        }
                        Polyline originPolyline = new Polyline();
                        for (int i = 0; i < columnPts.Count; i++)
                        {
                            Point2d pt = new Point2d(columnPts[i].X, columnPts[i].Y);
                            originPolyline.AddVertexAt(i, pt, 0, 0, 0);
                        }
                        originPolyline.Closed = true;
                        DBObjectCollection dbObjCo11 = new DBObjectCollection();
                        dbObjCo11.Add(originPolyline);
                        Region originRegion = (Region)Region.CreateFromCurves(dbObjCo11)[0];
                        List<Tuple<Region, Polyline>> intersectRegions = new List<Tuple<Region, Polyline>>();
                        foreach (Polyline polyline in polylines)
                        {
                            DBObjectCollection dBObjectCol = new DBObjectCollection() { polyline };
                            Region region = Region.CreateFromCurves(dBObjectCol)[0] as Region;
                            if (region == null)
                            {
                                continue;
                            }
                            Region originRegionCopy = originRegion.Clone() as Region;
                            originRegionCopy.BooleanOperation(BooleanOperationType.BoolIntersect, region);
                            intersectRegions.Add(new Tuple<Region, Polyline>(originRegionCopy, polyline));
                        }
                        columnObjId = intersectRegions.OrderBy(i => Math.Abs(i.Item1.Area - originRegion.Area)).First().Item2.ObjectId;
                        originRegion.Dispose();
                        intersectRegions.ForEach(i => i.Item1.Dispose());
                        trans.Commit();
                    }
                }
                else
                {
                    return columnObjId;
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "PlantCalDataToDraw->GetUnVisibleColumn");
            }
            return columnObjId;
        }
        private YjkColumnDataInfo GetYjkDataInfo(ColumnRelateInf columnRelateInf)
        {
            YjkColumnDataInfo yjkColumnDataInfo = new YjkColumnDataInfo();
            double jkb = 0.0;
            //获取jtId对应的自然层编号和柱编号（dtlModel）
            int tblFloorNo, tblColSegNo;
            yjkModelDb.GetTblFloorTblColSegNoFromDtlmodel(columnRelateInf.DbColumnInf.JtID, out tblFloorNo, out tblColSegNo);

            //获取dtlCalc库中的柱子ID
            int columnId = yjkCalculateDb.GetTblColSegIDFromDtlCalc(tblFloorNo, tblColSegNo);

            //获取剪跨比(dtlCalc)
            bool resJkb = yjkCalculateDb.GetShearSpanRatio(columnId, out jkb);
            yjkColumnDataInfo.Jkb = jkb;

            //获取轴压比和轴压比限值(dtlCalc)
            double axialCompressionRatio;
            double axialCompressionRatioLimited;
            bool resCompressionRatio = yjkCalculateDb.GetAxialCompressionRatio(columnId,
                out axialCompressionRatio, out axialCompressionRatioLimited);
            yjkColumnDataInfo.AxialCompressionRatio = axialCompressionRatio;
            yjkColumnDataInfo.AxialCompressionRatioLimited = axialCompressionRatioLimited;

            //获取角筋直径限值(dtlCalc)
            double arDiaLimited = 0.0;
            bool resArDiaLimited = yjkCalculateDb.GetAngularReinforcementDiaLimited(columnId, out arDiaLimited);
            yjkColumnDataInfo.ArDiaLimited = arDiaLimited;

            //获取抗震等级(dtlCalc)
            double antiSeismicGradeParaValue = yjkCalculateDb.GetAntiSeismicGradeInCalculation(columnId);
            if (antiSeismicGradeParaValue != 0.0)
            {
                yjkColumnDataInfo.AntiSeismicGrade = ThValidate.GetAntiSeismicGrade(antiSeismicGradeParaValue);
            }

            //获取保护层厚度
            double protectThickness = 0.0;
            bool findRes = yjkCalculateDb.GetProtectLayerThickInTblColSegPara(
                columnId, out protectThickness);
            if (!findRes)
            {
                findRes = yjkModelDb.GetProtectLayerThickInTblStdFlrPara(
                    columnRelateInf.DbColumnInf.StdFlrID, out protectThickness);
                if (!findRes)
                {
                    findRes = yjkModelDb.GetProtectLayerThickInTblProjectPara(out protectThickness);
                }
            }
            yjkColumnDataInfo.ProtectThickness = protectThickness;

            //判断是否为角柱
            yjkColumnDataInfo.IsCorner = yjkCalculateDb.CheckCornerColumn(columnId);

            double structureParaValue = yjkModelDb.GetStructureTypeInModel();
            yjkColumnDataInfo.StructureType = ThValidate.GetStructureType(structureParaValue);

            //获取配筋面积限值
            List<double> values = yjkCalculateDb.GetDblXYAsCal(columnId);
            yjkColumnDataInfo.DblXAsCal = values[0];
            yjkColumnDataInfo.DblYAsCal = values[1];

            //是否底层
            yjkColumnDataInfo.IsGroundFloor = CheckIsGroundFloor();

            //获取设防烈度
            double fortiCation = 0.0;
            bool res2 = yjkModelDb.GetFortificationIntensity(out fortiCation);
            yjkColumnDataInfo.FortiCation = fortiCation;

            //获取体积配筋率限值
            double volumeReinforceLimitedValue = 0.0;
            bool resVRLV = yjkCalculateDb.GetVolumeReinforceLimitedValue(
                columnId, out volumeReinforceLimitedValue);
            yjkColumnDataInfo.VolumeReinforceLimitedValue = volumeReinforceLimitedValue;

            //获取配筋面积限值
            double dblStirrupAsCal = yjkCalculateDb.GetDblStirrupAsCalLimited(columnId);
            double dblStirrupAsCal0 = yjkCalculateDb.GetDblStirrupAsCal0Limited(columnId);

            yjkColumnDataInfo.DblStirrupAsCal = dblStirrupAsCal;
            yjkColumnDataInfo.DblStirrupAsCal0 = dblStirrupAsCal0;

            //假定箍筋间距
            double intStirrupSpacingCal = 0.0;
            bool resIntStirrupSpacingCal = yjkModelDb.GetIntStirrupSpacingCal(out intStirrupSpacingCal);
            yjkColumnDataInfo.IntStirrupSpacingCal = intStirrupSpacingCal;

            return yjkColumnDataInfo;
        }
        /// <summary>
        /// 如果计算导入有"1F" ，则为底层
        /// </summary>
        /// <returns></returns>
        private bool CheckIsGroundFloor()
        {
            bool isGroundFloor = false;
            if(thStandardSign.SignPlantCalData==null)
            {
                return isGroundFloor;
            }
            CalculationInfoVM calculationInfoVM = new CalculationInfoVM(thStandardSign.SignPlantCalData.CalInfo);
            List<string> naturalFloors = calculationInfoVM.GetSelectFloors();

            var includeFirstGroundRes = naturalFloors.Where(i => i.ToUpper() == "1F").Select(i => i);
            if (includeFirstGroundRes != null && includeFirstGroundRes.Count() > 0)
            {
                isGroundFloor = true;
            }
            return isGroundFloor;
        }
        private void AddExtensionDictionary(ObjectId entityId, YjkColumnDataInfo yjkColumnDataInfo,ColumnCustomData columnCustomData, DrawColumnInf drawColumnInf)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            if (entityId == ObjectId.Null)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject dbObj = tr.GetObject(entityId, OpenMode.ForRead);
                ObjectId extId = dbObj.ExtensionDictionary;
                if (extId == ObjectId.Null)
                {
                    dbObj.UpgradeOpen();
                    dbObj.CreateExtensionDictionary();
                    extId = dbObj.ExtensionDictionary;
                }
                //now we will have extId...
                DBDictionary dbExt =
                        (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);

                //写入从Yjk中获取的数据
                int maxWriteNumber = 0;
                bool jkGroup1HasValue = false;
                bool jkGroup2HasValue = false;
                int yjkGroup1WriteNumber = 0;
                int yjkGroup2WriteNumber = 0;
                if (dbExt.Contains(this.yjkData1KeyName))
                {
                    dbExt.UpgradeOpen();
                    jkGroup1HasValue = true;
                    ObjectId yjkGroupObjId= dbExt.GetAt(this.yjkData1KeyName);
                    Xrecord xrecord = (Xrecord)tr.GetObject(yjkGroupObjId, OpenMode.ForRead);
                    object groupValue= xrecord.Data.AsArray()[0].Value;
                    YjkColumnDataInfo yjkColumnDataInfo1 = new YjkColumnDataInfo();
                    yjkColumnDataInfo1 = BaseFunction.getObjectByJson(groupValue.ToString(), yjkColumnDataInfo1) as YjkColumnDataInfo;
                    yjkGroup1WriteNumber = yjkColumnDataInfo1.WriteNumber;
                    if (yjkGroup1WriteNumber > maxWriteNumber)
                    {
                        maxWriteNumber = yjkColumnDataInfo1.WriteNumber;
                    }                    
                }
                if (dbExt.Contains(this.yjkData2KeyName))
                {
                    dbExt.UpgradeOpen();
                    jkGroup2HasValue = true;
                    ObjectId yjkGroupObjId = dbExt.GetAt(this.yjkData2KeyName);
                    Xrecord xrecord = (Xrecord)tr.GetObject(yjkGroupObjId, OpenMode.ForRead);
                    object groupValue = xrecord.Data.AsArray()[0].Value;
                    YjkColumnDataInfo yjkColumnDataInfo2 = new YjkColumnDataInfo();
                    yjkColumnDataInfo2 = BaseFunction.getObjectByJson(groupValue.ToString(), yjkColumnDataInfo2) as YjkColumnDataInfo;
                    yjkGroup2WriteNumber = yjkColumnDataInfo2.WriteNumber;
                    if (yjkGroup2WriteNumber > maxWriteNumber)
                    {
                        maxWriteNumber = yjkColumnDataInfo2.WriteNumber;
                    }
                }
                yjkColumnDataInfo.WriteNumber= maxWriteNumber+1;
                string yjkData=BaseFunction.getJsonByObject(yjkColumnDataInfo);
                string customerData= BaseFunction.getJsonByObject(columnCustomData);
                string yjkColumnData= BaseFunction.getJsonByObject(drawColumnInf);
                string dicKey = "";
                if(!jkGroup1HasValue)
                {
                    dicKey = this.yjkData1KeyName;
                }
                else if(jkGroup1HasValue && !jkGroup2HasValue)
                {
                    dicKey = this.yjkData2KeyName;
                }
                else if(jkGroup1HasValue && jkGroup2HasValue)
                {
                    if(yjkGroup1WriteNumber< yjkGroup2WriteNumber)
                    {
                        dicKey = this.yjkData1KeyName;
                    }
                    else if(yjkGroup1WriteNumber> yjkGroup2WriteNumber)
                    {
                        dicKey = this.yjkData2KeyName;
                    }
                }
                Xrecord yjkXRec = new Xrecord();
                ResultBuffer yjkRb = new ResultBuffer();
                yjkRb.Add(new TypedValue(
                          (int)DxfCode.ExtendedDataAsciiString, yjkData));
                //set the data
                yjkXRec.Data = yjkRb;
                if (!dbExt.Contains(dicKey))
                {
                    dbExt.UpgradeOpen();
                    dbExt.SetAt(dicKey, yjkXRec);
                    tr.AddNewlyCreatedDBObject(yjkXRec, true);
                }        
                else
                {
                    dbExt.UpgradeOpen();
                    DBObject oldXRecord= tr.GetObject(dbExt.GetAt(dicKey),OpenMode.ForWrite);
                    oldXRecord.Erase();
                    dbExt[dicKey] = yjkXRec;
                    tr.AddNewlyCreatedDBObject(yjkXRec, true);
                }

                //写入用户自定义数据
                Xrecord customXRec = new Xrecord();
                ResultBuffer customRb = new ResultBuffer();
                customRb.Add(new TypedValue(
                          (int)DxfCode.ExtendedDataAsciiString, customerData));
                //set the data
                customXRec.Data = customRb;
                if (!dbExt.Contains(this.customKeyName))
                {
                    dbExt.UpgradeOpen();
                    dbExt.SetAt(this.customKeyName, customXRec);
                    tr.AddNewlyCreatedDBObject(customXRec, true);
                }
                else
                {
                    dbExt.UpgradeOpen();
                    DBObject oldXRecord = tr.GetObject(dbExt.GetAt(this.customKeyName), OpenMode.ForWrite);
                    oldXRecord.Erase();
                    dbExt[this.customKeyName] = customXRec;
                    tr.AddNewlyCreatedDBObject(customXRec, true);
                }

                //写入Yjk柱子中的信息
                Xrecord yjkColumnXRec = new Xrecord();
                ResultBuffer yjkColumnRb = new ResultBuffer();
                yjkColumnRb.Add(new TypedValue(
                          (int)DxfCode.ExtendedDataAsciiString, yjkColumnData));
                //set the data
                yjkColumnXRec.Data = yjkColumnRb;
                if (!dbExt.Contains(this.yjkColumnKeyName)) 
                {
                    dbExt.UpgradeOpen();
                    dbExt.SetAt(this.yjkColumnKeyName, yjkColumnXRec);
                    tr.AddNewlyCreatedDBObject(yjkColumnXRec, true);
                }
                else
                {
                    dbExt.UpgradeOpen();
                    DBObject oldXRecord = tr.GetObject(dbExt.GetAt(this.yjkColumnKeyName), OpenMode.ForWrite);
                    oldXRecord.Erase();
                    dbExt[this.yjkColumnKeyName] = yjkColumnXRec;
                    tr.AddNewlyCreatedDBObject(yjkColumnXRec, true);
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 获取最后一次MaxNumber大的信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="yjkColumnDataInfo"></param>
        /// <param name="columnCustomData"></param>
        private void GetExtensionDictionary(ObjectId entityId, YjkColumnDataInfo yjkColumnDataInfo, 
            ColumnCustomData columnCustomData,DrawColumnInf drawColumnInf)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            if (entityId == ObjectId.Null)
                return;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject dbObj = tr.GetObject(entityId, OpenMode.ForRead);
                ObjectId extId = dbObj.ExtensionDictionary;
                if (extId == ObjectId.Null)
                {
                    dbObj.UpgradeOpen();
                    dbObj.CreateExtensionDictionary();
                    extId = dbObj.ExtensionDictionary;
                }
                //now we will have extId...
                DBDictionary dbExt =
                        (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);

                //写入从Yjk中获取的数据
                int yjkGroup1WriteNumber = 0;
                int yjkGroup2WriteNumber = 0;
                YjkColumnDataInfo yjkColumnDataInfo1 = new YjkColumnDataInfo();
                YjkColumnDataInfo yjkColumnDataInfo2 = new YjkColumnDataInfo();
                if (dbExt.Contains(this.yjkData1KeyName))
                {
                    dbExt.UpgradeOpen();
                    ObjectId yjkGroupObjId = dbExt.GetAt(this.yjkData1KeyName);
                    Xrecord xrecord = (Xrecord)tr.GetObject(yjkGroupObjId, OpenMode.ForRead);
                    object groupValue = xrecord.Data.AsArray()[0].Value;
                    yjkColumnDataInfo1 = BaseFunction.getObjectByJson(groupValue.ToString(), yjkColumnDataInfo1) as YjkColumnDataInfo;
                    yjkGroup1WriteNumber = yjkColumnDataInfo1.WriteNumber;
                    dbExt.DowngradeOpen();
                }
                if (dbExt.Contains(this.yjkData2KeyName))
                {
                    dbExt.UpgradeOpen();
                    ObjectId yjkGroupObjId = dbExt.GetAt(this.yjkData2KeyName);
                    Xrecord xrecord = (Xrecord)tr.GetObject(yjkGroupObjId, OpenMode.ForRead);
                    object groupValue = xrecord.Data.AsArray()[0].Value;                    
                    yjkColumnDataInfo2 = BaseFunction.getObjectByJson(groupValue.ToString(), yjkColumnDataInfo2) as YjkColumnDataInfo;
                    yjkGroup2WriteNumber = yjkColumnDataInfo2.WriteNumber;
                    dbExt.DowngradeOpen();
                }
                if(yjkGroup1WriteNumber> yjkGroup2WriteNumber)
                {
                    yjkColumnDataInfo = yjkColumnDataInfo1;
                }
                else
                {
                    yjkColumnDataInfo = yjkColumnDataInfo2;
                }
                if(dbExt.Contains(this.customKeyName))
                {
                    dbExt.UpgradeOpen();
                    ObjectId customObjId = dbExt.GetAt(this.customKeyName);
                    Xrecord xrecord = (Xrecord)tr.GetObject(customObjId, OpenMode.ForRead);
                    object groupValue = xrecord.Data.AsArray()[0].Value;
                    columnCustomData = BaseFunction.getObjectByJson(groupValue.ToString(), columnCustomData) as ColumnCustomData;
                    dbExt.DowngradeOpen();
                }
                if (dbExt.Contains(this.yjkColumnKeyName))
                {
                    dbExt.UpgradeOpen();
                    ObjectId yjkColumnObjId = dbExt.GetAt(this.yjkColumnKeyName);
                    Xrecord xrecord = (Xrecord)tr.GetObject(yjkColumnObjId, OpenMode.ForRead);
                    object groupValue = xrecord.Data.AsArray()[0].Value;
                    drawColumnInf = BaseFunction.getObjectByJson(groupValue.ToString(), drawColumnInf) as DrawColumnInf;
                    dbExt.DowngradeOpen();
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 删除柱子外框
        /// </summary>
        public void ClearFrameIds()
        {
            try
            {
                ThColumnInfoUtils.EraseObjIds(this.columnFrameIds.ToArray());
                ThColumnInfoUtils.EraseObjIds(this.unrelatedFrameIds.ToArray());
                ThColumnInfoUtils.EraseObjIds(this.exceptionFrameIds.ToArray());
                this.columnFrameIds.Clear();
                this.unrelatedFrameIds.Clear();
                this.exceptionFrameIds.Clear();
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ClearFrameIds");
            }
        }
        /// <summary>
        /// 获取图签内柱子的关联信息
        /// </summary>
        /// <param name="thStandardSign"></param>
        /// <returns></returns>
        public List<ColumnRelateInf> GetColumnRelateInfs(ThStandardSign thStandardSign)
        {
            List<ColumnRelateInf> columnRelateInfs = new List<ColumnRelateInf>();
            if (thStandardSign.SignExtractColumnInfo == null ||
                thStandardSign.SignExtractColumnInfo.ColumnInfs.Count == 0)
            {
                return columnRelateInfs;
            }
            for (int i = 0; i < thStandardSign.SignExtractColumnInfo.ColumnInfs.Count; i++)
            {
                ObjectId polylineId = GetUnVisibleColumn(thStandardSign.SignExtractColumnInfo.ColumnInfs[i].Points);
                if (polylineId == ObjectId.Null)
                {
                    continue;
                }
                YjkColumnDataInfo yjkColumnDataInfo = new YjkColumnDataInfo();
                ColumnCustomData columnCustomData = new ColumnCustomData();
                DrawColumnInf drawColumnInf = new DrawColumnInf();
                GetExtensionDictionary(polylineId, yjkColumnDataInfo, columnCustomData, drawColumnInf);
                ColumnRelateInf columnRelateInf = new ColumnRelateInf()
                {
                    DbColumnInf = drawColumnInf,
                    CustomData = columnCustomData,
                    ModelColumnInfs = new List<ColumnInf> { thStandardSign.SignExtractColumnInfo.ColumnInfs[i] },
                    InModelPts = thStandardSign.SignExtractColumnInfo.ColumnInfs[i].Points, //初始化为柱子的点,后期如果需要把Yjk柱子中的点存入
                    YjkColumnData = yjkColumnDataInfo
                };
                columnRelateInfs.Add(columnRelateInf);
                ThProgressBar.MeterProgress();
            }
            return columnRelateInfs;
        }

        private List<ObjectId> embededColumnIds = new List<ObjectId>();
        public List<ObjectId> EmbededColumnIds
        {
            get
            {
                return embededColumnIds;
            }            
        }
        /// <summary>
        /// 获取埋入的柱子集合
        /// </summary>
        /// <returns></returns>
        public void GetEmbededColumnIds()
        {
            this.embededColumnIds = new List<ObjectId>();
            try
            {
                TypedValue[] tvs = new TypedValue[]
                {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName,this.thColumnRegAppName),
                new TypedValue((int)DxfCode.Start,"LWPOLYLINE")
                };
                Editor ed = this.document.Editor;
                SelectionFilter sf = new SelectionFilter(tvs);
                PromptSelectionResult psr = ed.SelectAll(sf);
                if (psr.Status == PromptStatus.OK)
                {
                    this.embededColumnIds = psr.Value.GetObjectIds().ToList();
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetEmbededColumnIds");
            }
        }
        /// <summary>
        /// 从埋入的柱子读取自定义数据
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        public ColumnCustomData ReadEmbededColumnCustomData(ObjectId objectId,out bool success)
        {
            ColumnCustomData columnCustomData=new ColumnCustomData();
            success = false;
            if (objectId == ObjectId.Null)
                return columnCustomData;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dbObj = tr.GetObject(objectId, OpenMode.ForRead);
                    ObjectId extId = dbObj.ExtensionDictionary;
                    if (extId == ObjectId.Null)
                    {
                        dbObj.UpgradeOpen();
                        dbObj.CreateExtensionDictionary();
                        extId = dbObj.ExtensionDictionary;
                    }
                    //now we will have extId...
                    DBDictionary dbExt =
                            (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);
                    if (dbExt.Contains(this.customKeyName))
                    {
                        dbExt.UpgradeOpen();
                        ObjectId customObjId = dbExt.GetAt(this.customKeyName);
                        Xrecord xrecord = (Xrecord)tr.GetObject(customObjId, OpenMode.ForRead);
                        object groupValue = xrecord.Data.AsArray()[0].Value;
                        columnCustomData = BaseFunction.getObjectByJson(groupValue.ToString(), columnCustomData) as ColumnCustomData;
                        dbExt.DowngradeOpen();
                    }
                }
                catch(Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "ReadEmbededColumnCustomData");
                }
                tr.Commit();
            }
            return columnCustomData;
        }
        /// <summary>
        /// 往埋入的柱子写入自定义柱数据
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="columnCustomData"></param>
        public void WriteEmbededColumnCustomData(ObjectId objectId, ColumnCustomData columnCustomData)
        {
            if (objectId == ObjectId.Null)
                return;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dbObj = tr.GetObject(objectId, OpenMode.ForRead);
                    ObjectId extId = dbObj.ExtensionDictionary;
                    if (extId == ObjectId.Null)
                    {
                        dbObj.UpgradeOpen();
                        dbObj.CreateExtensionDictionary();
                        extId = dbObj.ExtensionDictionary;
                    }
                    //now we will have extId...
                    DBDictionary dbExt =
                            (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);
                    string customerData = BaseFunction.getJsonByObject(columnCustomData);
                    //写入用户自定义数据
                    Xrecord customXRec = new Xrecord();
                    ResultBuffer customRb = new ResultBuffer();
                    customRb.Add(new TypedValue(
                              (int)DxfCode.ExtendedDataAsciiString, customerData));
                    //set the data
                    customXRec.Data = customRb;
                    if (!dbExt.Contains(this.customKeyName))
                    {
                        dbExt.UpgradeOpen();
                        dbExt.SetAt(this.customKeyName, customXRec);
                        tr.AddNewlyCreatedDBObject(customXRec, true);
                    }
                    else
                    {
                        dbExt.UpgradeOpen();
                        DBObject oldXRecord = tr.GetObject(dbExt.GetAt(this.customKeyName), OpenMode.ForWrite);
                        oldXRecord.Erase();
                        dbExt[this.customKeyName] = customXRec;
                        tr.AddNewlyCreatedDBObject(customXRec, true);
                    }
                }
                catch(Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "WriteEmbededColumnCustomData");
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 清空传入的柱子自定义数据
        /// </summary>
        /// <param name="objectIds"></param>
        public void ClearEmbededColumnCustomData(List<ObjectId> objectIds)
        {
            if (objectIds == null || objectIds.Count==0)
                return;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    for(int i=0;i< objectIds.Count;i++)
                    {
                        DBObject dbObj = tr.GetObject(objectIds[i], OpenMode.ForRead);
                        ObjectId extId = dbObj.ExtensionDictionary;
                        if (extId == ObjectId.Null)
                        {
                            dbObj.UpgradeOpen();
                            dbObj.CreateExtensionDictionary();
                            extId = dbObj.ExtensionDictionary;
                        }
                        //now we will have extId...
                        DBDictionary dbExt =
                                (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);
                        //写入用户自定义数据
                        Xrecord customXRec = new Xrecord();
                        //set the data
                        customXRec.Data = null;
                        if (!dbExt.Contains(this.customKeyName))
                        {
                            dbExt.UpgradeOpen();
                            dbExt.SetAt(this.customKeyName, customXRec);
                            tr.AddNewlyCreatedDBObject(customXRec, true);
                        }
                        else
                        {
                            dbExt.UpgradeOpen();
                            DBObject oldXRecord = tr.GetObject(dbExt.GetAt(this.customKeyName), OpenMode.ForWrite);
                            oldXRecord.Erase();
                            dbExt[this.customKeyName] = customXRec;
                            tr.AddNewlyCreatedDBObject(customXRec, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "ClearEmbededColumnCustomData");
                }
                tr.Commit();
            }
        }
    }
    /// <summary>
    /// 柱子外框颜色
    /// </summary>
    public enum FrameColor
    {
        /// <summary>
        /// 关联
        /// </summary>
        Related,
        /// <summary>
        /// 本地图纸有，计算书没有
        /// </summary>
        DwgHasCalNot,
        /// <summary>
        /// 本地图纸没有，计算书有
        /// </summary>
        DwgNotCalHas,
        /// <summary>
        /// 柱平法缺失
        /// </summary>
        ColumnLost,
        /// <summary>
        /// 平法参数识别不全
        /// </summary>
        ParameterNotFull,
        None
    }
}
