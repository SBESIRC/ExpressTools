using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThColumnInfo
{
    /// <summary>
    /// 提取二维表中附带图形标注说明的柱表信息
    /// </summary>
    public class GraphicStyleColumnDetailInfo:DataStyleColumnDetailInfo
    {
        protected List<List<TableCellInfo>> dataColumnCells = new List<List<TableCellInfo>>();
        protected List<List<TableCellInfo>> headColumnCells = new List<List<TableCellInfo>>();

        private double extendPointRatio = 0.5;
        public GraphicStyleColumnDetailInfo(Point3d tableLeftDownCornerPt, Point3d tableRightUpCornerPt)
            :base(tableLeftDownCornerPt, tableRightUpCornerPt)
        {           
        }
        public override void Extract()
        {
            Vector3d vec = this.tableLeftDownCornerPt.GetVectorTo(this.tableRightUpCornerPt);
            Point3d cenPt = this.tableLeftDownCornerPt+vec.GetNormal().MultiplyBy(this.extendPointRatio*vec.Length);
            try
            {
                List<string> tableLayerNames = base.GetTableLayerName();
                base.ColumnTableObjIds = GetColumnTableObjIds(tableLayerNames);
                base.GetColunmTableCurves();
                bool findRes = false;
                cenPt = FindInvalidCenPt(cenPt, out findRes);
                List<TableCellInfo> sameColumnCells = GetSameColumnCells(cenPt, null);
                sameColumnCells = sameColumnCells.OrderByDescending(i => ThColumnInfoUtils.GetMidPt(
                   ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[0]),
                   ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[2])).Y).ToList(); //对同一列的记录进行排序(从上到下)
                List<List<TableCellInfo>> tableCells = new List<List<TableCellInfo>>();
                int totalRow = sameColumnCells.Count;
                List<double> xValueList = new List<double>();
                foreach (TableCellInfo tableCellInfo in sameColumnCells)
                {
                    if (tableCellInfo.BoundaryPts.Count != 4)
                    {
                        continue;
                    }
                    double diagonalLine = tableCellInfo.BoundaryPts[0].DistanceTo(tableCellInfo.BoundaryPts[2]);
                    Point3d findCenPt = ThColumnInfoUtils.GetExtendPt(tableCellInfo.BoundaryPts[0], tableCellInfo.BoundaryPts[2], this.extendPointRatio * diagonalLine);
                    List<TableCellInfo> sameRowCells = GetSameRowCells(ThColumnInfoUtils.TransPtFromWcsToUcs(findCenPt), null, tableCellInfo.RowHeight);
                    sameRowCells = sameRowCells.OrderByDescending(i => ThColumnInfoUtils.GetMidPt(
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[0]),
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[2])).X).ToList(); //对同一行的记录进行排序
                    tableCells.Add(sameRowCells);
                    List<double> tempXvalues = sameRowCells.Select(i => ThColumnInfoUtils.GetMidPt(
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[0]),
                        ThColumnInfoUtils.TransPtFromWcsToUcs(i.BoundaryPts[2])).X).ToList();
                    for (int i = 0; i < tempXvalues.Count; i++)
                    {
                        List<double> tempValues = xValueList.Where(j => Math.Abs(j - tempXvalues[i]) <= 2.0).Select(j => j).ToList();
                        if (tempValues == null || tempValues.Count == 0)
                        {
                            xValueList.Add(tempXvalues[i]);
                        }
                    }
                    ThProgressBar.MeterProgress();
                }
                for (int i = 0; i < tableCells.Count; i++)
                {
                    tableCells[i].ForEach(j => j.Row = i);
                }
                int totalColumn = tableCells.OrderByDescending(i => i.Count).Select(i => i.Count).First();
                //提取数据单元格
                xValueList = xValueList.OrderBy(i => i).ToList();
                Dictionary<double, List<TableCellInfo>> columnHeightCells = new Dictionary<double, List<TableCellInfo>>(); //行转成列
                for (int m = 0; m < xValueList.Count; m++)
                {
                    List<TableCellInfo> columnCells = new List<TableCellInfo>();
                    for (int i = 0; i < tableCells.Count; i++)
                    {
                        TableCellInfo tableCellInfo = tableCells[i].Where(j => Math.Abs(ThColumnInfoUtils.GetMidPt(
                              ThColumnInfoUtils.TransPtFromWcsToUcs(j.BoundaryPts[0]),
                              ThColumnInfoUtils.TransPtFromWcsToUcs(j.BoundaryPts[2])).X - xValueList[m]) <= 2.0).Select(j => j).FirstOrDefault();
                        columnCells.Add(tableCellInfo);
                    }
                    columnCells.ForEach(i => i.Column = m);
                    columnHeightCells.Add(xValueList[m], columnCells);
                    ThProgressBar.MeterProgress();
                }
                this.dataColumnCells = columnHeightCells.Where(i => i.Value.Count == totalRow).Select(i => i.Value).ToList(); //具有相同列数的数据单元格
                this.headColumnCells = columnHeightCells.Where(i => i.Value.Count != totalRow).Select(i => i.Value).ToList();
                this.headColumnCells = this.headColumnCells.Select(i => GetNoRepeatedCells(i)).ToList();
                if (this.headColumnCells.Count > 0)
                {
                    Dictionary<int, double> rowHeightDic = new Dictionary<int, double>();
                    for (int i = 0; i < headColumnCells[0].Count; i++)
                    {
                        rowHeightDic.Add(i, headColumnCells[0][i].RowHeight);
                    }
                    Dictionary<int, double> columnWidthDic = new Dictionary<int, double>();
                    for (int i = 0; i < headColumnCells.Count; i++)
                    {
                        columnWidthDic.Add(i, headColumnCells[i][0].ColumnWidth);
                    }
                    for (int i = 0; i < headColumnCells.Count; i++)
                    {
                        for (int j = 0; j < headColumnCells[i].Count; j++)
                        {
                            TableCellInfo cellInfo = headColumnCells[i][j];
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
                    if (dataColumnCells.Count > 0)
                    {
                        this.headColumnCells.Add(dataColumnCells[0]);
                        this.dataColumnCells.RemoveAt(0);
                    }
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
            finally
            {
                Dispose();
            }
            GetHeadCellText(); //先把表头的数据拿到
            GetDataCellText(); //获取数据单元格的文本
        }
        /// <summary>
        ///  获取表头单元格的文本
        /// </summary>
        private void GetHeadCellText()
        {
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Text,MText") };
            SelectionFilter sf = new SelectionFilter(tvs);
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                string headCellText = "";
                for (int i = 0; i < this.headColumnCells.Count; i++)
                {
                    for (int j = 0; j < this.headColumnCells[i].Count; j++)
                    {
                        if (this.headColumnCells[i][j].BoundaryPts.Count == 0)
                        {
                            continue;
                        }
                        Point3dCollection ucsPts = new Point3dCollection();
                        foreach(Point3d pt in this.headColumnCells[i][j].BoundaryPts)
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
                            this.headColumnCells[i][j].Text = headCellText;
                        }
                        ThProgressBar.MeterProgress();
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 获取数据单元格的文本，并将提取的信息直接赋给ColumnTableRecordInf
        /// </summary>
        private void GetDataCellText()
        {
            Dictionary<string, int> keyWordDic = new Dictionary<string, int>();
            List<TableCellInfo> headAllCells = new List<TableCellInfo>();
            this.headColumnCells.ForEach(i => headAllCells.AddRange(i));
            List<string> keyWordList = new List<string>() { "截面", "编号", "柱号", "名称", "标高", "纵筋", "箍筋", "备注" };
            foreach (string keyWord in keyWordList)
            {
                TableCellInfo tci = headAllCells.Where(i => !string.IsNullOrEmpty(i.Text) && i.Text.Replace(" ", "").ToLower().
                Contains(keyWord)).Select(i => i).FirstOrDefault();
                if (tci == null)
                {
                    continue;
                }
                keyWordDic.Add(keyWord, tci.Row);
            }
            TypedValue[] tvs1 = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Text,MText") };
            TypedValue[] tvs2 = new TypedValue[] { new TypedValue((int)DxfCode.Start, "LWPOLYLINE,Polyline,DIMENSION,Text,MText") };
            SelectionFilter sf1 = new SelectionFilter(tvs1);
            SelectionFilter sf2 = new SelectionFilter(tvs2);
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                string cellText = "";
                for (int i = 0; i < this.dataColumnCells.Count; i++)
                {
                    ColumnTableRecordInfo coluTabRi = new ColumnTableRecordInfo();
                    List<Curve> polylineObjs = new List<Curve>();
                    for (int j = 0; j < this.dataColumnCells[i].Count; j++)
                    {
                        if (this.dataColumnCells[i][j].BoundaryPts.Count == 0)
                        {
                            continue;
                        }
                        List<string> keyWordStrs = keyWordDic.Where(m => m.Value == this.dataColumnCells[i][j].Row).Select(m => m.Key).ToList();
                        if (keyWordStrs == null || keyWordStrs.Count == 0)
                        {
                            continue;
                        }
                        List<Point3d> pts = new List<Point3d>();
                        foreach (Point3d pt in this.dataColumnCells[i][j].BoundaryPts)
                        {
                            pts.Add(ThColumnInfoUtils.TransPtFromWcsToUcs(pt));
                        }
                        foreach (string keyWordStr in keyWordStrs)
                        {
                            PromptSelectionResult psr;
                            cellText = "";
                            if (keyWordStr == "截面")
                            {
                                double minX = pts.OrderBy(k => k.X).First().X;
                                double minY = pts.OrderBy(k => k.Y).First().Y;
                                double maxX = pts.OrderByDescending(k => k.X).First().X;
                                double maxY = pts.OrderByDescending(k => k.Y).First().Y;
                                Point3d point1 = new Point3d(minX, minY, 0) + new Vector3d(5, 5, 0);
                                Point3d point3 = new Point3d(maxX, maxY, 0) + new Vector3d(-5, -5, 0);
                                Point3d point2 = new Point3d(point3.X, point1.Y, 0);
                                Point3d point4 = new Point3d(point1.X, point3.Y, 0);
                                Point3dCollection points = new Point3dCollection();
                                points.Add(point1);
                                points.Add(point2);
                                points.Add(point3);
                                points.Add(point4);
                                psr = ThColumnInfoUtils.SelectByPolyline(doc.Editor,
                           points, PolygonSelectionMode.Window, sf2);
                                if (psr.Status == PromptStatus.OK)
                                {
                                    List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                                    List<DBObject> dbObjs = objIds.Select(m => trans.GetObject(m, OpenMode.ForRead)).ToList();
                                    polylineObjs = dbObjs.Where(m => m is Polyline).Select(m => m as Curve).ToList();
                                    List<DBObject> dimensionObjs = dbObjs.Where(m => m is Dimension).Select(m => m).ToList();
                                    string spec = GetHoopReinforcementTypeNumberOneSpec(dimensionObjs);
                                    coluTabRi.Spec = spec; //规格
                                }
                            }
                            else //仅仅是文字
                            {
                                Point3dCollection ucsBoundaryPts = new Point3dCollection();
                                pts.ForEach(k => ucsBoundaryPts.Add(k));
                                psr = ThColumnInfoUtils.SelectByPolyline(doc.Editor,
                           ucsBoundaryPts, PolygonSelectionMode.Window, sf1);
                                if (psr.Status == PromptStatus.OK)
                                {
                                    List<ObjectId> textObjIds = psr.Value.GetObjectIds().ToList();
                                    List<DBObject> dbObjs = textObjIds.Select(m => trans.GetObject(m, OpenMode.ForRead)).ToList();
                                    if (dbObjs.Count > 1)
                                    {
                                        dbObjs = dbObjs.Where(m => m.Bounds != null && m.Bounds.HasValue).Select(m => m).ToList();
                                        dbObjs = dbObjs.OrderBy(m => ThColumnInfoUtils.GetMidPt(m.Bounds.Value.MinPoint, m.Bounds.Value.MaxPoint).X).ToList();
                                    }
                                    if (dbObjs[0] is DBText)
                                    {
                                        DBText dbText = dbObjs[0] as DBText;
                                        cellText = dbText.TextString;
                                    }
                                    else if (dbObjs[0] is MText)
                                    {
                                        MText mText = dbObjs[0] as MText;
                                        cellText = mText.Text;
                                    }
                                }
                            }
                            dataColumnCells[i][j].Text = cellText;
                            switch (keyWordStr)
                            {
                                case "编号":
                                case "柱号":
                                case "名称":
                                    coluTabRi.Code = dataColumnCells[i][j].Text;
                                    break;
                                case "标高":
                                    coluTabRi.Level = dataColumnCells[i][j].Text;
                                    break;
                                case "纵筋":
                                    coluTabRi.AllLongitudinalReinforcement = dataColumnCells[i][j].Text;
                                    break;
                                case "箍筋":
                                    coluTabRi.HoopReinforcement = dataColumnCells[i][j].Text;
                                    break;
                                case "备注":
                                    coluTabRi.Remark = dataColumnCells[i][j].Text;
                                    break;
                            }
                        }
                    }
                    Curve situMarkFrame = GetSituMarkFrame(polylineObjs);
                    BuildInSituMarkInf buildInSituMarkInf = new BuildInSituMarkInf(situMarkFrame);
                    buildInSituMarkInf.Ctri = coluTabRi;
                    buildInSituMarkInf.Build();
                    if(buildInSituMarkInf.Valid)
                    {
                        if (!string.IsNullOrEmpty(coluTabRi.Code))
                        {
                            coluTabRi.Code.Replace('，', ',');
                            string[] codeStrs = coluTabRi.Code.Split(',');
                            if (codeStrs.Length == 1)
                            {
                                this.coluTabRecordInfs.Add(coluTabRi);
                            }
                            else
                            {
                                foreach (string code in codeStrs)
                                {
                                    ColumnTableRecordInfo ctri = coluTabRi.Clone() as ColumnTableRecordInfo;
                                    ctri.Code = code.Trim();
                                    this.coluTabRecordInfs.Add(ctri);
                                }
                            }
                        }
                    }
                    ThProgressBar.MeterProgress();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 获取柱表中截面列 “角筋、纵筋、箍筋” 的外框
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        private Curve GetSituMarkFrame(List<Curve> curve)
        {
            Curve frameCurve=null;
            double maxArea = 0.0;
            int index = -1;
            for(int i=0;i< curve.Count;i++)
            {
                if(curve[i] is Polyline polyline)
                {
                    if(polyline.Closed && polyline.Area> maxArea)
                    {
                        maxArea = polyline.Area;
                        index = i;
                    }
                }
                else if (curve[i] is Polyline2d polyline2d)
                {
                    if (polyline2d.Closed && polyline2d.Area > maxArea)
                    {
                        maxArea = polyline2d.Area;
                        index = i;
                    }
                }
                else if (curve[i] is Polyline3d polyline3d)
                {
                    if (polyline3d.Closed && polyline3d.Area > maxArea)
                    {
                        maxArea = polyline3d.Area;
                        index = i;
                    }
                }
            }
            if(index>=0)
            {
                frameCurve = curve[index];
            }
            return frameCurve;
        }
        /// <summary>
        /// 获取角筋规格
        /// </summary>
        /// <param name="longitudinalReinforcementSpec"></param>
        /// <returns></returns>
        private string GetAngularReinforcementSpec(string longitudinalReinforcementSpec)
        {
            string angularReinforcementSpec = "";
            if (string.IsNullOrEmpty(longitudinalReinforcementSpec))
            {
                return angularReinforcementSpec;
            }
            string[] splitValues=longitudinalReinforcementSpec.Split('+');
            string splitStr = @"[0-9]+";
            List<string> values = new List<string>();
            MatchCollection matchCol = Regex.Matches(splitValues[0], splitStr);
            foreach(var item in matchCol)
            {
                values.Add(item.ToString());
            }
            if (values != null && values.Count > 0)
            {
                string suffix = splitValues[0].Substring(values[0].Length);
                int totalNum = Convert.ToInt32(values[0]);
                if (totalNum < 4)
                {
                    return angularReinforcementSpec;
                }
                angularReinforcementSpec = "4" + suffix;
            }
            return angularReinforcementSpec;
        }
        /// <summary>
        /// 获取B边Side和H边Side中部筋
        /// </summary>
        /// <param name="dbObjs"></param>
        /// <param name="bSide"></param>
        /// <param name="hSide"></param>
        private void GetBHSideHoopReinforceSpec(List<DBObject> dbObjs,out string bSide,out string hSide)
        {
            bSide = "";
            hSide = "";
            ColumnTableRecordInfo tempCTRI = new ColumnTableRecordInfo();
            foreach (DBObject dbObj in dbObjs)
            {
                if(dbObj is DBText)
                {
                    DBText dBText = dbObj as DBText;
                    if(tempCTRI.ValidateReinforcement(dBText.TextString))
                    {
                        if (Math.Abs(ThColumnInfoUtils.RadToAng(dBText.Rotation) - 90.0) <= 1.0 ||
                        Math.Abs(ThColumnInfoUtils.RadToAng(dBText.Rotation) - 270.0) <= 1.0)
                        {
                            if(string.IsNullOrEmpty(hSide))
                            {
                                hSide = dBText.TextString;
                            }                            
                        }
                        else if(Math.Abs(ThColumnInfoUtils.RadToAng(dBText.Rotation) - 0.0) <= 1.0 ||
                        Math.Abs(ThColumnInfoUtils.RadToAng(dBText.Rotation) - 180.0) <= 1.0)
                        {
                            if(string.IsNullOrEmpty(bSide))
                            {
                                bSide = dBText.TextString;
                            }                            
                        }
                    }
                }
                else if(dbObj is MText)
                {
                    MText mText = dbObj as MText;
                    if (new ColumnTableRecordInfo().ValidateReinforcement(mText.Text))
                    {
                        if (Math.Abs(ThColumnInfoUtils.RadToAng(mText.Rotation) - 90.0) <= 1.0 ||
                        Math.Abs(ThColumnInfoUtils.RadToAng(mText.Rotation) - 270.0) <= 1.0)
                        {
                            if (string.IsNullOrEmpty(hSide))
                            {
                                hSide = mText.Text;
                            }                           
                        }
                        else if (Math.Abs(ThColumnInfoUtils.RadToAng(mText.Rotation) - 0.0) <= 1.0 ||
                        Math.Abs(ThColumnInfoUtils.RadToAng(mText.Rotation) - 180.0) <= 1.0)
                        {
                            if (string.IsNullOrEmpty(bSide))
                            {
                                bSide = mText.Text;
                            }
                        }
                    }
                }
                if(!string.IsNullOrEmpty(bSide) && !string.IsNullOrEmpty(hSide))
                {
                    break;
                }
            }
        }
        /// <summary>
        /// 获取箍筋类型号1对应的柱子规格
        /// </summary>
        /// <returns></returns>
        private string GetHoopReinforcementTypeNumberOneSpec(List<DBObject> dimensionObjs)
        {
            string spec = "";
            string xSpec = "";
            string ySpec = "";
            string dimText = "";
            foreach(DBObject dbObj in dimensionObjs)
            {
                Vector3d vec = new Vector3d(1,1,0);
                dimText = "";
                if (dbObj is AlignedDimension)
                {
                    AlignedDimension ad = dbObj as AlignedDimension;
                    vec = ad.XLine1Point.GetVectorTo(ad.XLine2Point);
                    dimText= ad.Measurement.ToString();       
                }
                else if(dbObj is RotatedDimension)
                {
                    RotatedDimension rd = dbObj as RotatedDimension;
                    vec = rd.XLine1Point.GetVectorTo(rd.XLine2Point);
                    dimText = rd.Measurement.ToString();
                }
                if(string.IsNullOrEmpty(dimText) || !BaseFunction.IsNumeric(dimText))
                {
                    continue;
                }
                if(string.IsNullOrEmpty(xSpec) && vec.GetNormal().IsEqualTo(Vector3d.XAxis, new Tolerance(1e-1, 1e-1)))
                {
                    xSpec = dimText;
                }
                if (string.IsNullOrEmpty(ySpec) && vec.GetNormal().IsEqualTo(Vector3d.YAxis, new Tolerance(1e-1, 1e-1)))
                {
                    ySpec = dimText;
                }
                if(!string.IsNullOrEmpty(xSpec) && !string.IsNullOrEmpty(ySpec))
                {
                    break;
                }
            }
            if (!string.IsNullOrEmpty(xSpec) && !string.IsNullOrEmpty(ySpec))
            {
                spec = xSpec + " x " + ySpec;
            }           
            return spec;
        }
        /// <summary>
        /// 获取箍筋类型号1
        /// </summary>
        /// <param name="polylines"></param>
        /// <returns></returns>
        private string GetHoopReinforcementTypeNumberOne(List<DBObject> polylines)
        {
            string typeNumber = "";
            if (polylines.Count == 0)
            {
                return typeNumber;
            }
            List<double> xDirList = new List<double>();
            List<double> yDirList = new List<double>();
            for (int i = 0; i < polylines.Count; i++)
            {
                Polyline polyline = polylines[i] as Polyline;
                if(polyline==null)
                {
                    continue;
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
                    if(vec.Length<=50)
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
            Dictionary<double, int> xVecLengthDic = new Dictionary<double, int>();
            Dictionary<double, int> yVecLengthDic = new Dictionary<double, int>();
            xDirList = xDirList.OrderByDescending(i => i).ToList();
            yDirList = yDirList.OrderByDescending(i => i).ToList();
            List<double> tempXDirList = new List<double>();
            List<double> tempYDirList = new List<double>();
            if (xDirList.Count>0)
            {
                tempXDirList = xDirList.Distinct().Take(3).ToList();
            }
            if (yDirList.Count > 0)
            {
                tempYDirList = yDirList.Distinct().Take(3).ToList();
            }     
            foreach(double length in tempXDirList)
            {
                List<double> tempList = xDirList.Where(i => Math.Abs(i - length) <= 5.0).Select(i => i).ToList();
                xVecLengthDic.Add(length, tempList.Count);
            }
            foreach (double length in tempYDirList)
            {
                List<double> tempList = yDirList.Where(i => Math.Abs(i - length) <= 5.0).Select(i => i).ToList();
                yVecLengthDic.Add(length, tempList.Count);
            }            
            int xNum= xVecLengthDic.OrderByDescending(i => i.Value).Select(i=>i.Value).FirstOrDefault();
            int yNum = yVecLengthDic.OrderByDescending(i => i.Value).Select(i => i.Value).FirstOrDefault();
            typeNumber = "1（" + xNum.ToString() + " x " + yNum.ToString() + "）";
            return typeNumber;
        }
    }
}
