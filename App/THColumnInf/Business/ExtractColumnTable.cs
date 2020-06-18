using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using ThColumnInfo.ViewModel;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThColumnInfo
{
    public class ExtractColumnTable
    {
        private ExtractColumnDetailInfoMode extractColumnDetailInfoMode = ExtractColumnDetailInfoMode.None;
        /// <summary>
        /// 柱表图层
        /// </summary>
        private string columnTableLayer = "S_TABL";  //TAB 从参数设置来获取
        private Point3d leftDownPt; //柱表左下角点(来源于GetColumnTableCornerPts)
        private Point3d rightUpPt; //柱表右上角点(来源于GetColumnTableCornerPts)

        private Point3d signPt1; //图签范围点
        private Point3d signPt2; //图签范围点

        private Document doc;

        /// <summary>
        /// 柱表详细信息(用于柱号查找)
        /// </summary>
        public List<ColumnTableRecordInfo> ColumnTableRecordInfos { get; set; } = new List<ColumnTableRecordInfo>();
        private ParameterSetInfo paraSetInfo;
        public ExtractColumnTable(Point3d pt1,Point3d pt2, ParameterSetInfo paraSetInfo=null)
        {
            this.signPt1 = pt1;
            this.signPt2 = pt2;
            if(paraSetInfo==null)
            {
                ParameterSetVM parameterSetVM = new ParameterSetVM();
                this.paraSetInfo = parameterSetVM.ParaSetInfo;
            }
            else
            {
                this.paraSetInfo = paraSetInfo;
            }
            this.doc = ThColumnInfoUtils.GetMdiActiveDocument();
        }
        /// <summary>
        /// 获取柱表对角点
        /// </summary>
        /// <param name="columnTableLayerName"></param>
        /// <returns></returns>
        private bool GetColumnTableCornerPts(List<string> columnTableLayerNames)
        {
            bool isFind=false;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            string columnTableLayerName = string.Join(",", columnTableLayerNames.ToArray());
            TypedValue[] tvs = new TypedValue[]
            {
                //new TypedValue((int)DxfCode.Start,"Line,LWPolyline"),
                new TypedValue((int)DxfCode.LayerName,columnTableLayerName)
            };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr= ThColumnInfoUtils.SelectByRectangle(ed, this.signPt1, this.signPt2, PolygonSelectionMode.Crossing, sf);
            ThProgressBar.MeterProgress();
            if(psr.Status!=PromptStatus.OK)
            {
                return isFind;
            }
            List<ObjectId> objectIds= psr.Value.GetObjectIds().ToList();
            List<Point3d> pts = new List<Point3d>();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                List<Entity> ents = objectIds.Where(i => IsInSignRange(trans.GetObject(i, OpenMode.ForRead) as Entity)).
                    Select(i => trans.GetObject(i, OpenMode.ForRead) as Entity).ToList();
                foreach(Entity ent in ents)
                {
                    Extents3d extents = ThColumnInfoUtils.GeometricExtentsImpl(ent);
                    if(extents!=null)
                    {
                        pts.Add(extents.MinPoint);
                        pts.Add(extents.MaxPoint);
                    }
                }
                trans.Commit();
            }       
            if(pts.Count>2)
            {
                double minX = pts.OrderBy(i => i.X).First().X;
                double minY = pts.OrderBy(i => i.Y).First().Y;
                double minZ = pts.OrderBy(i => i.Z).First().Z;

                double maxX = pts.OrderByDescending(i => i.X).First().X;
                double maxY = pts.OrderByDescending(i => i.Y).First().Y;
                double maxZ = pts.OrderByDescending(i => i.Z).First().Z;

                double xLen = Math.Abs(maxX - minX);
                double yLen = Math.Abs(maxY - minY);
                double offset = Math.Min(xLen, yLen) * 0.1;

                this.leftDownPt = new Point3d(minX, minY, minZ) + new Vector3d(-offset, -offset, 0.0);
                this.rightUpPt = new Point3d(maxX, maxY, maxZ) + new Vector3d(offset, offset, 0.0);
                isFind = true;
            }
            ThProgressBar.MeterProgress();
            return isFind;
        }
        private bool IsInSignRange(Entity ent)
        {
            bool isIn = false;
            double minX = Math.Min(this.signPt1.X, this.signPt2.X);
            double maxX = Math.Max(this.signPt1.X, this.signPt2.X);
            double minY = Math.Min(this.signPt1.Y, this.signPt2.Y);
            double maxY = Math.Max(this.signPt1.Y, this.signPt2.Y);
            Extents3d entExtents = ThColumnInfoUtils.GeometricExtentsImpl(ent);
            if ((entExtents.MinPoint.X >= minX && entExtents.MinPoint.X <= maxX) &&
                (entExtents.MinPoint.Y >= minY && entExtents.MinPoint.Y <= maxY)
                )
            {
                isIn = true;
            }
            else if((entExtents.MaxPoint.X >= minX && entExtents.MaxPoint.X <= maxX) &&
                (entExtents.MaxPoint.Y >= minY && entExtents.MaxPoint.Y <= maxY))
            {
                isIn = true;
            }
            return isIn;
        }
        public void Extract()
        {
            this.columnTableLayer = this.paraSetInfo.ColumnTableLayer;
            List<string> totalLayers = ThColumnInfoUtils.GetLayerList();
            List<string> layers = new List<string>() { };
            if (string.IsNullOrEmpty(this.columnTableLayer))
            {
                layers.Add("S_TABL");
            }
            else
            {
                string[] splitStrs = this.columnTableLayer.Split(',');
                foreach (string str in splitStrs)
                {
                    if(str.IndexOf("*")==0 && str.Length>1)
                    {
                        string tailContent = str.Substring(1);
                        foreach(string layerItem in totalLayers)
                        {
                            int lastIndex = layerItem.LastIndexOf(tailContent);
                            if (lastIndex+tailContent.Length== layerItem.Length)
                            {
                                layers.Add(layerItem);
                            }
                        }
                    }
                    else
                    {
                        layers.Add(str);
                    }
                }
            }
            //查找图签里的
            bool isFind = GetColumnTableCornerPts(layers);            
            if (!isFind)
            {
                return;
            }
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Insert") };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor, this.leftDownPt,
                this.rightUpPt, PolygonSelectionMode.Crossing, sf);
            ThProgressBar.MeterProgress();
            ObjectId[] insertObjIds = new ObjectId[] { };
            if (psr.Status == PromptStatus.OK)
            {
                insertObjIds = psr.Value.GetObjectIds();
                ThColumnInfoUtils.ShowObjIds(insertObjIds, false); //把块隐藏掉
            }           
            ViewTableRecord currentView = this.doc.Editor.GetCurrentView();
            try
            {
                bool hasDimension = false, hasText = false, hasPolyline = false;
                bool hasLine = false;
                PromptSelectionResult psr1 = ThColumnInfoUtils.SelectByRectangle(this.doc.Editor,
                    this.leftDownPt, this.rightUpPt, PolygonSelectionMode.Crossing);
                ThProgressBar.MeterProgress();
                if (psr1.Status == PromptStatus.OK)
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
                ThProgressBar.MeterProgress();
                if (this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.None)
                {
                    if (hasText && (hasLine || hasPolyline))
                    {
                        this.extractColumnDetailInfoMode = ExtractColumnDetailInfoMode.Regular;
                    }
                }
                if(this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.Regular ||
                    this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.Graphic)
                {
                    double dis = this.leftDownPt.DistanceTo(this.rightUpPt)/3.0;
                    Point3d zoomFirstPt = ThColumnInfoUtils.GetExtendPt(this.leftDownPt, this.rightUpPt, -1.0*dis);
                    Point3d zoomSecondPt = ThColumnInfoUtils.GetExtendPt(this.rightUpPt, this.leftDownPt, -1.0*dis);
                    COMTool.ZoomWindow(ThColumnInfoUtils.TransPtFromUcsToWcs(zoomFirstPt),
                        ThColumnInfoUtils.TransPtFromUcsToWcs(zoomSecondPt));
                }
                
                if (this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.Regular)
                {
                    DataStyleColumnDetailInfo dscdi = new DataStyleColumnDetailInfo(this.leftDownPt, this.rightUpPt);
                    dscdi.Extract();
                    this.ColumnTableRecordInfos = dscdi.ColuTabRecordInfs;
                }
                else if (this.extractColumnDetailInfoMode == ExtractColumnDetailInfoMode.Graphic)
                {
                    GraphicStyleColumnDetailInfo gscdi = new GraphicStyleColumnDetailInfo(this.leftDownPt, this.rightUpPt);
                    gscdi.Extract();
                    this.ColumnTableRecordInfos = gscdi.ColuTabRecordInfs;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
            finally
            {
                this.doc.Editor.SetCurrentView(currentView);
                ThColumnInfoUtils.ShowObjIds(insertObjIds, true);
            }
        }
    }
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
}
