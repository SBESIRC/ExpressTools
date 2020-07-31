using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThCADCore.NTS;
using Linq2Acad;
using AcHelper;
using ThColumnInfo.Service;

namespace ThColumnInfo
{
    /// <summary>
    /// 提取二维表中的柱表信息
    /// </summary>
    public class DataStyleColumnDetailInfo : ExtractBase
    {
        protected Vector3d baseVec = Vector3d.XAxis;
        //以下是Regular提取需要的参数
        protected int disKeepPointNum = 1; //长度保留小数点位数
        protected double searchBoundaryOffsetDis = 50.0;
        protected int findSingleCellNum = 3;
        private List<List<TableCellInfo>> dataRowCells = new List<List<TableCellInfo>>();
        private List<List<TableCellInfo>> headRowCells = new List<List<TableCellInfo>>();

        private List<ObjectId> columnTableObjIds = new List<ObjectId>();
        protected List<Curve> columnTableCurves = new List<Curve>(); //支持Line和Arc
        protected List<Curve> topoCurves = new List<Curve>(); //支持Line和Arc

        protected DBObjectCollection TableCells = new DBObjectCollection(); //根据columnTableCurve获取所有内部的单元格

        public List<ObjectId> ColumnTableObjIds
        {
            set
            {
                this.columnTableObjIds = value;
            }
            get
            {
                return columnTableObjIds;
            }
        }
        public DataStyleColumnDetailInfo(Point3d tableLeftDownCornerPt, Point3d tableRightUpCornerPt) :
            base(tableLeftDownCornerPt, tableRightUpCornerPt)
        {
        }
        public override void Extract()
        {
            Point3d cenPt = ThColumnInfoUtils.GetMidPt(this.tableLeftDownCornerPt, this.tableRightUpCornerPt);
            List<ObjectId> needHideObjIds = new List<ObjectId>();
            try
            {
                List<string> tableLayerNames = base.GetTableLayerName();
                ThProgressBar.MeterProgress();
                if (tableLayerNames != null && tableLayerNames.Count > 0)
                {
                    needHideObjIds = base.GetNeedHideObjIds(tableLayerNames,out this.columnTableObjIds);
                    ThColumnInfoUtils.ShowObjIds(needHideObjIds, false);
                }
                GetColunmTableCurves();
                bool findRes = false;
                cenPt = FindInvalidCenPt(cenPt, out findRes);
                List<TableCellInfo> sameRowCells = GetSameRowCells(cenPt, null);
                sameRowCells = sameRowCells.OrderBy(i => ThColumnInfoUtils.GetMidPt(
                    ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[0]),
                    ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[2])).X).ToList(); //对同一行的记录进行排序(从左到右)
                List<List<TableCellInfo>> tableCells = new List<List<TableCellInfo>>();
                int totalColumn = sameRowCells.Count;
                List<double> yValueList = new List<double>();
                foreach (TableCellInfo tableCellInfo in sameRowCells)
                {
                    if (tableCellInfo.BoundaryPts.Count != 4)
                    {
                        continue;
                    }
                    Point3d midPt = ThColumnInfoUtils.GetMidPt(
                        ThColumnInfoUtils.TransPtFromWcsToUcs(tableCellInfo.BoundaryPts[0]),
                        ThColumnInfoUtils.TransPtFromWcsToUcs(tableCellInfo.BoundaryPts[2]));
                    List<TableCellInfo> sameColumnCells = GetSameColumnCells(midPt, null, tableCellInfo.ColumnWidth);
                    sameColumnCells = sameColumnCells.OrderByDescending(i => ThColumnInfoUtils.GetMidPt(
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[0]),
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[2])).Y).ToList(); //对同一行的记录进行排序(从上到下)
                    tableCells.Add(sameColumnCells);
                    List<double> tempYvalues = sameColumnCells.Select(i => ThColumnInfoUtils.GetMidPt(
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[0]),
                         ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[2])).Y).ToList();
                    for (int i = 0; i < tempYvalues.Count; i++)
                    {
                        List<double> tempValues = yValueList.Where(j => Math.Abs(j - tempYvalues[i]) <= 2.0).Select(j => j).ToList();
                        if (tempValues == null || tempValues.Count == 0)
                        {
                            yValueList.Add(tempYvalues[i]);
                        }
                    }
                    ThProgressBar.MeterProgress();
                }
                for (int i = 0; i < tableCells.Count; i++)
                {
                    tableCells[i].ForEach(j => j.Column = i);
                }
                ThProgressBar.MeterProgress();
                int totalRow = tableCells.OrderByDescending(i => i.Count).Select(i => i.Count).First();
                //提取数据单元格
                yValueList = yValueList.OrderByDescending(i => i).ToList();
                Dictionary<double, List<TableCellInfo>> rowHeightCells = new Dictionary<double, List<TableCellInfo>>(); //列转成行

                for (int m = 0; m < yValueList.Count; m++)
                {
                    List<TableCellInfo> rowCells = new List<TableCellInfo>();
                    for (int i = 0; i < tableCells.Count; i++)
                    {
                        TableCellInfo tableCellInfo = tableCells[i].Where(j => Math.Abs(ThColumnInfoUtils.GetMidPt(
                            ThColumnInfoUtils.TransPtFromWcsToUcs(j.BoundaryPts[0]),
                             ThColumnInfoUtils.TransPtFromWcsToUcs(j.BoundaryPts[2])).Y - yValueList[m]) <= 2.0).Select(j => j).FirstOrDefault();
                        rowCells.Add(tableCellInfo);
                    }
                    rowCells.ForEach(i => i.Row = m);
                    rowHeightCells.Add(yValueList[m], rowCells);
                    ThProgressBar.MeterProgress();
                }
                this.dataRowCells = rowHeightCells.Where(i => i.Value.Count == totalColumn).Select(i => i.Value).ToList(); //具有相同列数的数据单元格
                this.headRowCells = rowHeightCells.Where(i => i.Value.Count != totalColumn).Select(i => i.Value).ToList();
                headRowCells = headRowCells.Select(i => GetNoRepeatedCells(i)).ToList();
                ThProgressBar.MeterProgress();
                if (headRowCells.Count > 0)
                {
                    Dictionary<int, double> columnWidthDic = new Dictionary<int, double>();
                    for (int i = 0; i < dataRowCells[0].Count; i++)
                    {
                        columnWidthDic.Add(i, dataRowCells[0][i].ColumnWidth);
                    }
                    Dictionary<int, double> rowHeightDic = new Dictionary<int, double>();
                    for (int i = 0; i < headRowCells.Count; i++)
                    {
                        rowHeightDic.Add(i, headRowCells[i][0].RowHeight);
                    }
                    for (int i = 0; i < headRowCells.Count; i++)
                    {
                        for (int j = 0; i < headRowCells[i].Count; j++)
                        {
                            TableCellInfo cellInfo = headRowCells[i][j];
                            int columnSpan = GetColumnSpan(cellInfo.Column, cellInfo.ColumnWidth, columnWidthDic);
                            int rowSpan = GetRowSpan(cellInfo.Row, cellInfo.RowHeight, rowHeightDic);
                            cellInfo.RowSpan = rowSpan;
                            cellInfo.ColumnSpan = columnSpan;
                        }
                        ThProgressBar.MeterProgress();
                    }
                }
                else
                {
                    if (dataRowCells.Count > 0)
                    {
                        headRowCells.Add(dataRowCells[0]);
                        dataRowCells.RemoveAt(0);
                    }
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
            finally
            {
                if (needHideObjIds != null && needHideObjIds.Count > 0)
                {
                    ThColumnInfoUtils.ShowObjIds(needHideObjIds, true);
                }
                Dispose();
            }
            GetCellText();
            TransferColumnInfo();
        }
        protected void Dispose()
        {
            this.columnTableCurves.ForEach(i => i.Dispose());
            this.columnTableCurves.Clear();
        }
        protected void GetColunmTableCurves()
        {
            Dispose();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                foreach (ObjectId objId in this.columnTableObjIds)
                {
                    if (trans.GetObject(objId, OpenMode.ForRead) is Curve curve)
                    {
                        Curve curveCopy = curve.Clone() as Curve;
                        if (curveCopy is Line)
                        {
                            this.columnTableCurves.Add(curveCopy);
                        }
                        else
                        {
                            DBObjectCollection dbObjs = new DBObjectCollection();
                            curveCopy.Explode(dbObjs);
                            foreach (DBObject dbObj in dbObjs)
                            {
                                if (dbObj is Line line)
                                {
                                    this.columnTableCurves.Add(line);
                                }
                            }
                        }
                    }
                    ThProgressBar.MeterProgress();
                }
                trans.Commit();
            }
            //this.topoCurves = CommonUtils.RemoveCollinearLines(this.columnTableCurves);
            //更新当前单元格集合
            UpdateTableCells();
        }
        /// <summary>
        /// 获取轮廓范围内所有Curve围成的单元格
        /// 根据this.columnTableCurves生成单元格
        /// </summary>
        /// <returns></returns>
        protected void UpdateTableCells()
        {
            this.TableCells.Dispose();
            this.TableCells = new DBObjectCollection();
            var objs = new DBObjectCollection();
            this.columnTableCurves.ForEach(i => objs.Add(i));
            DBObjectCollection newObjs = ThColumnInfoUtils.RemoveDuplicateCurves(objs);
            ExtendLineLength extendLineLength = new ExtendLineLength(newObjs);
            DBObjectCollection extendObjs = extendLineLength.Extend();
            using (var ov = new ThCADCoreNTSPrecisionReducer(10))
            {
                this.TableCells = extendObjs.Polygons();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt">Ucs Point3d</param>
        /// <returns></returns>
        protected List<Curve> TraceCells(Point3d pt)
        {
            List<Curve> cells = new List<Curve>();
            Point3d wcsPt = ThColumnInfoUtils.TransPtFromUcsToWcs(pt);
            foreach(DBObject dbObj in TableCells)
            {
                List<Point3d> pts = new List<Point3d>();
                if(dbObj is Polyline polyline)
                {
                    pts = ThColumnInfoUtils.GetPolylinePts(polyline);
                }
                else if (dbObj is Polyline2d polyline2d)
                {
                    pts = ThColumnInfoUtils.GetPolylinePts(polyline2d);
                }
                pts=FilterOverlapPoint(pts);
                if (pts.Count<4)
                {
                    continue;
                }
                Point3dCollection points = new Point3dCollection();
                pts.ForEach(i => points.Add(i));
                if(ThColumnInfoUtils.IsPointInPolyline(points, wcsPt))
                {
                    cells.Add(dbObj as Curve);
                }
            }
            return cells;
        }
        private List<Point3d> FilterOverlapPoint(List<Point3d> pts)
        {
            double tolerance = 1.0;
            List<Point3d> newPts = new List<Point3d>();
            while(pts.Count>0)
            {
                Point3d firstPt = pts[0];
                newPts.Add(firstPt);
                pts.RemoveAt(0);

                for(int i=0;i< pts.Count;i++)
                {
                    if(pts[i].DistanceTo(firstPt)<= tolerance)
                    {
                        pts.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return newPts;
        }
        protected DBObjectCollection ExtractTableOutline()
        {
            DBObjectCollection dbObjs = new DBObjectCollection();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                this.columnTableObjIds.ForEach(i => dbObjs.Add(trans.GetObject(i, OpenMode.ForRead)));
                trans.Commit();
            }
            return dbObjs.Outline();
        }
        /// <summary>
        /// 提取的单元格，获取文字
        /// </summary>
        private void GetCellText()
        {
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Text,MText") };
            SelectionFilter sf = new SelectionFilter(tvs);
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < this.dataRowCells.Count; i++)
                {
                    for (int j = 0; j < this.dataRowCells[i].Count; j++)
                    {
                        if (this.dataRowCells[i][j].BoundaryPts.Count == 0)
                        {
                            continue;
                        }
                        Point3dCollection ucsPts = new Point3dCollection();
                        foreach(Point3d pt in this.dataRowCells[i][j].BoundaryPts)
                        {
                            ucsPts.Add(ThColumnInfoUtils.TransPtFromWcsToUcs(pt));
                        }
                        PromptSelectionResult psr = ThColumnInfoUtils.SelectByPolyline(doc.Editor,
                            ucsPts, PolygonSelectionMode.Window, sf);
                        if (psr.Status == PromptStatus.OK)
                        {
                            List<ObjectId> textObjIds = psr.Value.GetObjectIds().ToList();
                            List<DBObject> dbObjs = textObjIds.Select(m => trans.GetObject(m, OpenMode.ForRead)).ToList();
                            if (dbObjs.Count > 1)
                            {
                                dbObjs = dbObjs.Where(m => m.Bounds != null && m.Bounds.HasValue).Select(m => m).ToList();
                                dbObjs = dbObjs.OrderBy(m => ThColumnInfoUtils.GetMidPt(
                                    ThColumnInfoUtils.GeometricExtentsImpl(m as Entity).MinPoint,
                                    ThColumnInfoUtils.GeometricExtentsImpl(m as Entity).MaxPoint).X).ToList();
                            }
                            if (dbObjs[0] is DBText)
                            {
                                DBText dbText = dbObjs[0] as DBText;
                                this.dataRowCells[i][j].Text = dbText.TextString;
                            }
                            else if (dbObjs[0] is MText)
                            {
                                MText mText = dbObjs[0] as MText;
                                this.dataRowCells[i][j].Text = mText.Text;
                            }
                        }
                        ThProgressBar.MeterProgress();
                    }
                }
                string headCellText = "";
                for (int i = 0; i < this.headRowCells.Count; i++)
                {
                    for (int j = 0; j < this.headRowCells[i].Count; j++)
                    {
                        if (this.headRowCells[i][j].BoundaryPts.Count == 0)
                        {
                            continue;
                        }
                        Point3dCollection ucsPts = new Point3dCollection();
                        foreach (Point3d pt in this.headRowCells[i][j].BoundaryPts)
                        {
                            ucsPts.Add(ThColumnInfoUtils.TransPtFromWcsToUcs(pt));
                        }
                        PromptSelectionResult psr = ThColumnInfoUtils.SelectByPolyline(doc.Editor,
                            ucsPts, PolygonSelectionMode.Window, sf);
                        if (psr.Status == PromptStatus.OK)
                        {
                            List<ObjectId> textObjIds = psr.Value.GetObjectIds().ToList();
                            List<DBObject> dbObjs = textObjIds.Select(m => trans.GetObject(m, OpenMode.ForRead)).ToList();
                            headCellText = "";
                            for (int m = 0; m < dbObjs.Count; m++)
                            {
                                if (dbObjs[m] is DBText)
                                {
                                    DBText dbText = dbObjs[m] as DBText;
                                    if (m == 0)
                                    {
                                        headCellText += dbText.TextString;
                                    }
                                    else
                                    {
                                        headCellText += " " + dbText.TextString;
                                    }
                                }
                                else if (dbObjs[m] is MText)
                                {
                                    MText mText = dbObjs[m] as MText;
                                    if (m == 0)
                                    {
                                        headCellText += mText.Text;
                                    }
                                    else
                                    {
                                        headCellText += " " + mText.Text;
                                    }
                                }
                            }
                            this.headRowCells[i][j].Text = headCellText;
                        }
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 获取二维表中柱子信息(从ThColumnTableDataExtract)
        /// </summary>
        /// <param name="dataRowCells"></param>
        /// <param name="headRowCells"></param>
        /// <returns></returns>
        private void TransferColumnInfo()
        {
            List<string> keyWordList = new List<string> { "柱号", "编号","名称", "标高", "bxh", "b×h", "全部纵筋", "角筋", "b边一侧", "h边一侧", "箍筋", "箍筋类型号", "备注" };
            Dictionary<string, int> keyWordDic = new Dictionary<string, int>();
            List<TableCellInfo> headAllCells = new List<TableCellInfo>();
            headRowCells.ForEach(i => headAllCells.AddRange(i));
            foreach (string keyWord in keyWordList)
            {
                TableCellInfo tci;
                if (keyWord == "箍筋")
                {
                    tci = headAllCells.Where(o =>
                    {
                        if (!string.IsNullOrEmpty(o.Text))
                        {
                            string text = o.Text.Replace(" ", "").ToLower();
                            if (text.Contains("箍筋") && !text.Contains("类型"))
                            {
                                return true;
                            }
                        }
                        return false;
                    }).Select(i => i).FirstOrDefault();
                }
                else if (keyWord == "箍筋类型号")
                {
                    tci = headAllCells.Where(o =>
                    {
                        if (!string.IsNullOrEmpty(o.Text))
                        {
                            string text = o.Text.Replace(" ", "").ToLower();
                            if (text.Contains("箍筋") && text.Contains("类型"))
                            {
                                return true;
                            }
                        }
                        return false;
                    }).Select(i => i).FirstOrDefault();
                }
                else
                {
                    tci = headAllCells.Where(i => !string.IsNullOrEmpty(i.Text) && i.Text.Replace(" ", "").ToLower().
                Contains(keyWord)).Select(i => i).FirstOrDefault();
                }
                if (tci == null)
                {
                    continue;
                }
                keyWordDic.Add(keyWord, tci.Column);
            }
            for (int i = 0; i < dataRowCells.Count; i++)
            {
                ColumnTableRecordInfo coluTabRi = new ColumnTableRecordInfo();
                for (int j = 0; j < dataRowCells[i].Count; j++)
                {
                    List<string> keyWordStrs = keyWordDic.Where(m => m.Value == dataRowCells[i][j].Column).Select(m => m.Key).ToList();
                    if (keyWordStrs == null || keyWordStrs.Count == 0)
                    {
                        continue;
                    }
                    foreach (string keyWordStr in keyWordStrs)
                    {
                        switch (keyWordStr)
                        {
                            case "编号":
                            case "柱号":
                            case "名称":
                                coluTabRi.Code = dataRowCells[i][j].Text;
                                break;
                            case "标高":
                                coluTabRi.Level = dataRowCells[i][j].Text;
                                break;
                            case "bxh":
                            case "b×h":
                                coluTabRi.Spec = dataRowCells[i][j].Text;
                                break;
                            case "全部纵筋":
                                coluTabRi.AllLongitudinalReinforcement = dataRowCells[i][j].Text;
                                break;
                            case "角筋":
                                coluTabRi.AngularReinforcement = dataRowCells[i][j].Text;
                                break;
                            case "b边一侧":
                                coluTabRi.BEdgeSideMiddleReinforcement = dataRowCells[i][j].Text;
                                break;
                            case "h边一侧":
                                coluTabRi.HEdgeSideMiddleReinforcement = dataRowCells[i][j].Text;
                                break;
                            case "箍筋":
                                coluTabRi.HoopReinforcement = dataRowCells[i][j].Text;
                                break;
                            case "箍筋类型号":
                                coluTabRi.HoopReinforcementTypeNumber = dataRowCells[i][j].Text;
                                break;
                            case "备注":
                                coluTabRi.Remark = dataRowCells[i][j].Text;
                                break;
                        }
                    }
                }
                ThProgressBar.MeterProgress();
                this.coluTabRecordInfs.Add(coluTabRi);
            }
        }
        protected int GetColumnSpan(int columnIndex, double columnWidth, Dictionary<int, double> columnWidthDic)
        {
            int columnSpan = 1;
            if (columnWidthDic.ContainsKey(columnIndex))
            {
                double width1 = columnWidthDic[columnIndex];
                if (Math.Abs(columnWidth - width1) <= 2.0)
                {
                    return columnSpan;
                }
                else
                {
                    List<double> widths = columnWidthDic.Where(i => i.Key > columnIndex).Select(i => i.Value).ToList();
                    bool isFind = false;
                    for (int i = 0; i < widths.Count; i++)
                    {
                        width1 += widths[i];
                        if (Math.Abs(width1 - columnWidth) < 2.0)
                        {
                            isFind = true;
                            columnSpan++;
                        }
                    }
                    if (isFind == false)
                    {
                        columnSpan = -1;
                    }
                }
            }      
            return columnSpan;
        }
        protected int GetRowSpan(int rowIndex, double rowHeight, Dictionary<int, double> rowHeightDic)
        {
            int rowSpan = 1;
            if (rowHeightDic.ContainsKey(rowIndex))
            {
                double height1 = rowHeightDic[rowIndex];
                if (Math.Abs(rowHeight - height1) <= 2.0)
                {
                    return rowSpan;
                }
                else
                {
                    List<double> heights = rowHeightDic.Where(i => i.Key > rowIndex).Select(i => i.Value).ToList();
                    bool isFind = false;
                    for (int i = 0; i < heights.Count; i++)
                    {
                        height1 += heights[i];
                        if (Math.Abs(height1 - rowHeight) < 2.0)
                        {
                            isFind = true;
                            rowSpan++;
                        }
                    }
                    if (isFind == false)
                    {
                        rowSpan = -1;
                    }
                }
            }
            return rowSpan;
        }
        protected List<TableCellInfo> GetNoRepeatedCells(List<TableCellInfo> rowCells)
        {
            List<TableCellInfo> noRepeatedCells = new List<TableCellInfo>();
            rowCells = rowCells.OrderBy(i => i.Column).ToList();
            while (rowCells.Count > 0)
            {
                noRepeatedCells.Add(rowCells[0]);
                Point3d cenPt = ThColumnInfoUtils.GetMidPt(rowCells[0].BoundaryPts[0], rowCells[0].BoundaryPts[2]);
                rowCells.RemoveAt(0);
                rowCells = rowCells.Where(i => ThColumnInfoUtils.GetMidPt(i.BoundaryPts[0], i.BoundaryPts[2]).DistanceTo(cenPt) > 2.0).Select(i => i).ToList();
            }
            return noRepeatedCells;
        }
        private double GetColumnCompareXValue(List<TableCellInfo> sameColumnCells)
        {
            List<Point3d> pts = sameColumnCells.Where(i => i.BoundaryPts.Count == 4).
                Select(i => ThColumnInfoUtils.GetMidPt(i.BoundaryPts[0], i.BoundaryPts[3])).ToList();
            List<double> xValues = pts.Select(i => i.X).ToList();
            xValues = xValues.Distinct().ToList();
            double xValue = 0.0;
            int num = 0;
            for (int i = 0; i < xValues.Count; i++)
            {
                List<Point3d> tempPts = pts.Where(j => Math.Abs(j.X - xValues[i]) <= 2.0).Select(j => j).ToList();
                if (tempPts.Count > num)
                {
                    num = tempPts.Count;
                    xValue = xValues[i];
                }
            }
            return xValue;
        }
        protected Point3d FindInvalidCenPt(Point3d originPt, out bool isFind)
        {
            Point3d pt = originPt;
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Line,LWPolyline,Text,MText") };
            SelectionFilter sf = new SelectionFilter(tvs);
            Point3d pt1 = originPt + new Vector3d(-5, -5, 0);
            Point3d pt3 = originPt + new Vector3d(5, 5, 0);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor, pt1, pt3, PolygonSelectionMode.Crossing, sf);
            ThProgressBar.MeterProgress();
            List<Curve> dbObjs = TraceCells(originPt);
            if (psr.Status == PromptStatus.OK || dbObjs ==null || dbObjs.Count==0) //传入的点有物体，且选不到边界
            {
                pt = FindInvalidCenPt(new Point3d(pt.X+ this.searchBoundaryOffsetDis,pt.Y+ this.searchBoundaryOffsetDis, pt.Z), out isFind);
                ThProgressBar.MeterProgress();
                if (isFind)
                {
                    return pt;
                }
                pt = FindInvalidCenPt(new Point3d(pt.X - this.searchBoundaryOffsetDis, pt.Y + this.searchBoundaryOffsetDis, pt.Z), out isFind);
                ThProgressBar.MeterProgress();
                if (isFind)
                {
                    return pt;
                }
                pt = FindInvalidCenPt(new Point3d(pt.X - this.searchBoundaryOffsetDis, pt.Y - this.searchBoundaryOffsetDis, pt.Z), out isFind);
                ThProgressBar.MeterProgress();
                if (isFind)
                {
                    return pt;
                }
                pt = FindInvalidCenPt(new Point3d(pt.X + this.searchBoundaryOffsetDis, pt.Y - this.searchBoundaryOffsetDis, pt.Z), out isFind);
                ThProgressBar.MeterProgress();
                if (isFind)
                {
                    return pt;
                }
            }
            else
            {
                isFind = true;
            }
            return pt;
        }
        /// <summary>
        /// 查找同一行的所有单元格
        /// </summary>
        /// <param name="cenPt">查找的中心点</param>
        /// <param name="twoDir">往哪个方向(null->两个方向,true->右,false->左)</param>
        /// <returns></returns>
        protected List<TableCellInfo> GetSameRowCells(Point3d cenPt,bool? towardRight,double rowHeight=0.0)
        {
            List<TableCellInfo> tableCellInfos = new List<TableCellInfo>();
            TableCellInfo tableCellInfo= GetSingleCell(cenPt, 0.0, rowHeight);
           
            if (tableCellInfo.BoundaryPts.Count==0)
            {
                return tableCellInfos;
            }
            tableCellInfos.Add(tableCellInfo);
            if(towardRight==null && rowHeight==0)
            {
                rowHeight = tableCellInfo.RowHeight;
            }

            Point3d currentCellCenPt = ThColumnInfoUtils.GetMidPt(tableCellInfo.BoundaryPts[0], tableCellInfo.BoundaryPts[2]);
            currentCellCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(currentCellCenPt);

            Point3d rightPt = currentCellCenPt + Vector3d.XAxis.MultiplyBy(tableCellInfo.ColumnWidth/2.0+this.searchBoundaryOffsetDis);
            Point3d leftPt = currentCellCenPt + Vector3d.XAxis.Negate().MultiplyBy(tableCellInfo.ColumnWidth / 2.0 + this.searchBoundaryOffsetDis);

            List<TableCellInfo> rightCells = new List<TableCellInfo>();
            List<TableCellInfo> leftCells = new List<TableCellInfo>();
            if (towardRight == true) //往右边方向走
            {
                rightCells = GetSameRowCells(rightPt, true, rowHeight);
            }
            else if (towardRight == false) //往左边方向走
            {
                leftCells = GetSameRowCells(leftPt, false, rowHeight);
            }
            else //往两个方向走
            {
                rightCells = GetSameRowCells(rightPt, true, rowHeight);
                leftCells = GetSameRowCells(leftPt, false, rowHeight);
            }
            if(rightCells!=null && rightCells.Count>0)
            {
                tableCellInfos.AddRange(rightCells);
            }
            if (leftCells != null && leftCells.Count > 0)
            {
                tableCellInfos.AddRange(leftCells);
            }
            return tableCellInfos;
        }
        protected List<TableCellInfo> GetSameColumnCells(Point3d cenPt, bool? towardUp,double columnWidth=0.0)
        {
            List<TableCellInfo> tableCellInfos = new List<TableCellInfo>();
            TableCellInfo tableCellInfo = GetSingleCell(cenPt, columnWidth,0.0);
            if (tableCellInfo.BoundaryPts.Count == 0)
            {
                return tableCellInfos;
            }
            if(towardUp==null && columnWidth==0)
            {
                columnWidth = tableCellInfo.ColumnWidth;
            }
            tableCellInfos.Add(tableCellInfo);
            Point3d currentCellCenPt = ThColumnInfoUtils.GetMidPt(tableCellInfo.BoundaryPts[0], tableCellInfo.BoundaryPts[2]);
            currentCellCenPt = ThColumnInfoUtils.TransPtFromWcsToUcs(currentCellCenPt);
            Point3d upPt = currentCellCenPt + Vector3d.YAxis.MultiplyBy(tableCellInfo.RowHeight / 2.0 + this.searchBoundaryOffsetDis);
            Point3d downPt = currentCellCenPt + Vector3d.YAxis.Negate().MultiplyBy(tableCellInfo.RowHeight / 2.0 + this.searchBoundaryOffsetDis);
            List<TableCellInfo> upCells = new List<TableCellInfo>();
            List<TableCellInfo> downCells = new List<TableCellInfo>();
            if (towardUp == true) //往上边方向走
            {
                upCells = GetSameColumnCells(upPt, true, columnWidth);
            }
            else if (towardUp == false) //往下边方向走
            {
                downCells = GetSameColumnCells(downPt, false, columnWidth);
            }
            else  //往两个方向走
            {
                upCells = GetSameColumnCells(upPt, true, columnWidth);
                downCells = GetSameColumnCells(downPt, false, columnWidth);
            }
            if (upCells != null && upCells.Count > 0)
            {
                tableCellInfos.AddRange(upCells);
            }
            if (downCells != null && downCells.Count > 0)
            {
                tableCellInfos.AddRange(downCells);
            }
            return tableCellInfos;
        }
        protected TableCellInfo GetSingleCell(Point3d cenPt,double columnWidth = 0.0,double rowHeight = 0.0, FindDir findDir = FindDir.None)
        {
            TableCellInfo tableCellInfo = new TableCellInfo();
            List<Curve> dbObjs = TopoService.TraceBoundary(doc.Editor, cenPt,false); 
            if (dbObjs==null || dbObjs.Count==0)
            {
                if(findDir != FindDir.None)
                {
                    for (int i = 1; i <= this.findSingleCellNum; i++)
                    {
                        switch (findDir)
                        {
                            case FindDir.Up:
                                cenPt += new Vector3d(0, this.searchBoundaryOffsetDis * i, 0);
                                break;
                            case FindDir.Down:
                                cenPt += new Vector3d(0, this.searchBoundaryOffsetDis * -i, 0);
                                break;
                            case FindDir.Left:
                                cenPt += new Vector3d(this.searchBoundaryOffsetDis * -i, 0, 0);
                                break;
                            case FindDir.Right:
                                cenPt += new Vector3d(this.searchBoundaryOffsetDis * i, 0, 0);
                                break;
                        }
                        dbObjs = TopoService.TraceBoundary(doc.Editor, cenPt, false);
                        if (dbObjs != null && dbObjs.Count > 0)
                        {
                            break;
                        }
                    }
                }
            }
            bool compareRes = true;
            if (dbObjs.Count > 0)
            {
                foreach (DBObject dbObj in dbObjs)
                {
                    Polyline polyline = dbObj as Polyline;
                    if (polyline == null || polyline.NumberOfVertices != 4)
                    {
                        continue;
                    }
                    Point3dCollection boundaryPts = new Point3dCollection();
                    for (int i = 0; i < polyline.NumberOfVertices; i++)
                    {
                        boundaryPts.Add(polyline.GetPoint3dAt(i));
                    }
                    double currentRowHeight =0.0;
                    double currentColumnWidth = 0.0;
                    if (boundaryPts[0].GetVectorTo(boundaryPts[1]).IsParallelTo(Vector3d.XAxis,ThCADCommon.Global_Tolerance))
                    {                        
                        currentColumnWidth = boundaryPts[0].DistanceTo(boundaryPts[1]);
                        currentRowHeight = boundaryPts[0].DistanceTo(boundaryPts[3]);
                    }
                    else if(boundaryPts[0].GetVectorTo(boundaryPts[1]).IsParallelTo(Vector3d.YAxis, ThCADCommon.Global_Tolerance))
                    {
                        currentColumnWidth = boundaryPts[0].DistanceTo(boundaryPts[3]);
                        currentRowHeight = boundaryPts[0].DistanceTo(boundaryPts[1]);
                    }
                    else //默认长的一边沿X方向
                    { 
                        if(boundaryPts[0].DistanceTo(boundaryPts[1])>= boundaryPts[0].DistanceTo(boundaryPts[3]))
                        {
                            currentColumnWidth = boundaryPts[0].DistanceTo(boundaryPts[1]);
                            currentRowHeight = boundaryPts[0].DistanceTo(boundaryPts[3]);
                        }
                        else
                        {
                            currentColumnWidth = boundaryPts[0].DistanceTo(boundaryPts[3]);
                            currentRowHeight = boundaryPts[0].DistanceTo(boundaryPts[1]);
                        }
                    }
                    currentColumnWidth = Math.Round(currentColumnWidth, this.disKeepPointNum);
                    currentRowHeight = Math.Round(currentRowHeight, this.disKeepPointNum);
                    if (columnWidth>0.0)
                    {
                        if(!(Math.Abs(currentColumnWidth- columnWidth)<=0.001))
                        {
                            compareRes = false;
                        }
                    }
                    if(rowHeight>0.0 && compareRes)
                    {
                        if (!(Math.Abs(currentRowHeight - rowHeight) <= 0.001))
                        {
                            compareRes = false;
                        }
                    }
                    if(compareRes)
                    {
                        tableCellInfo.BoundaryPts = boundaryPts;
                        tableCellInfo.RowHeight = currentRowHeight;
                        tableCellInfo.ColumnWidth = currentColumnWidth;
                        break;
                    }
                }
            }
            return tableCellInfo;
        }
        /// <summary>
        /// 通过一点获取其外框线
        /// </summary>
        /// <param name="cenPt">查找的种子点</param>
        /// <param name="columnTableFrameIds">柱表外框线</param>
        /// <returns></returns>
        protected TableCellInfo GetSingleCell(Point3d cenPt,double columnWidth = 0.0, double rowHeight = 0.0)
        {
            TableCellInfo tableCellInfo = new TableCellInfo();
            List<Curve> edgeCurves= TraceCells(cenPt);
            if(edgeCurves==null)
            {
                return tableCellInfo;
            }
            if(edgeCurves.Count>0)
            {
                bool compareRes = true;
                foreach (Curve curve in edgeCurves)
                {
                    Polyline polyline = curve as Polyline;
                    if (polyline == null || polyline.NumberOfVertices < 4)
                    {
                        continue;
                    }
                    Extents3d extents= ThColumnInfoUtils.GeometricExtentsImpl(polyline);
                    List<Point3d> ucsBoundaryPts = new List<Point3d>();
                    ucsBoundaryPts.Add(new Point3d(extents.MinPoint.X, extents.MinPoint.Y,0.0));
                    ucsBoundaryPts.Add(new Point3d(extents.MaxPoint.X, extents.MinPoint.Y,0.0));
                    ucsBoundaryPts.Add(new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y,0.0));
                    ucsBoundaryPts.Add(new Point3d(extents.MinPoint.X, extents.MaxPoint.Y,0.0));

                    double currentRowHeight = 0.0;
                    double currentColumnWidth = 0.0;
                    if (ucsBoundaryPts[0].GetVectorTo(ucsBoundaryPts[1]).IsParallelTo(Vector3d.XAxis, ThCADCommon.Global_Tolerance))
                    {
                        currentColumnWidth = ucsBoundaryPts[0].DistanceTo(ucsBoundaryPts[1]);
                        currentRowHeight = ucsBoundaryPts[0].DistanceTo(ucsBoundaryPts[3]);
                    }
                    currentColumnWidth = Math.Round(currentColumnWidth, this.disKeepPointNum);
                    currentRowHeight = Math.Round(currentRowHeight, this.disKeepPointNum);
                    if (columnWidth > 0.0)
                    {
                        if (!(Math.Abs(currentColumnWidth - columnWidth) <= 1.0))
                        {
                            compareRes = false;
                        }
                    }
                    if (rowHeight > 0.0 && compareRes)
                    {
                        if (!(Math.Abs(currentRowHeight - rowHeight) <= 1.0))
                        {
                            compareRes = false;
                        }
                    }
                    if (compareRes)
                    {
                        List<Point3d> boundaryPts = ThColumnInfoUtils.GetPolylinePts(polyline);
                        Point3dCollection boundaryCol = AdjustCellPts(boundaryPts);
                        tableCellInfo.BoundaryPts = boundaryCol;
                        tableCellInfo.RowHeight = currentRowHeight;
                        tableCellInfo.ColumnWidth = currentColumnWidth;
                        break;
                    }
                }
            }
            ThProgressBar.MeterProgress();
            return tableCellInfo;
        }
        private Point3dCollection AdjustCellPts(List<Point3d> pts)
        {
            Point3dCollection res = new Point3dCollection();
            List<Point3d> ptList = pts.Select(i=> ThColumnInfoUtils.TransPtFromWcsToUcs(i)).ToList();
            double minX=ptList.OrderBy(i => i.X).First().X;
            double minY = ptList.OrderBy(i => i.Y).First().Y;
            double minZ = ptList.OrderBy(i => i.Z).First().Z;

            double maxX = ptList.OrderByDescending(i => i.X).First().X;
            double maxY = ptList.OrderByDescending(i => i.Y).First().Y;
            double maxZ = ptList.OrderByDescending(i => i.Z).First().Z;

            res.Add(ThColumnInfoUtils.TransPtFromUcsToWcs(new Point3d(minX, minY, minZ)));
            res.Add(ThColumnInfoUtils.TransPtFromUcsToWcs(new Point3d(maxX, minY, minZ)));
            res.Add(ThColumnInfoUtils.TransPtFromUcsToWcs(new Point3d(maxX, maxY, minZ)));
            res.Add(ThColumnInfoUtils.TransPtFromUcsToWcs(new Point3d(minX, maxY, minZ)));

            return res;
        }
    }
    public enum FindDir
    {
        None,
        /// <summary>
        /// 上
        /// </summary>
        Up,
        /// <summary>
        /// 下
        /// </summary>
        Down,
        /// <summary>
        /// 右
        /// </summary>
        Right,
        /// <summary>
        /// 左
        /// </summary>
        Left
    }
}
