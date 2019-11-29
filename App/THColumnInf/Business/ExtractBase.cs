using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThColumnInfo.Model;

namespace ThColumnInfo
{
    public class ExtractBase
    {
        protected Point3d tableLeftDownCornerPt;
        protected Point3d tableRightUpCornerPt;
        protected double selectRangeLength = 0.0;
        protected double selectRangeHeight = 0.0;
        protected Document doc;
        protected List<ColumnTableRecordInfo> coluTabRecordInfs = new List<ColumnTableRecordInfo>();
        public ExtractBase(Point3d tableLeftDownCornerPt, Point3d tableRightUpCornerPt)
        {
            this.tableLeftDownCornerPt = tableLeftDownCornerPt;
            this.tableRightUpCornerPt = tableRightUpCornerPt;
            doc = ThColumnInfoUtils.GetMdiActiveDocument();
            ResetTableCornerPt();
            this.selectRangeLength = this.tableRightUpCornerPt.X - this.tableLeftDownCornerPt.X;
            this.selectRangeHeight = this.tableRightUpCornerPt.Y - this.tableLeftDownCornerPt.Y;
        }
        /// <summary>
        /// 提取的柱子信息
        /// </summary>
        public List<ColumnTableRecordInfo> ColuTabRecordInfs
        {
            get
            {
                return coluTabRecordInfs;
            }
        }
        /// <summary>
        /// 提取信息
        /// </summary>
        public virtual void Extract()
        {            
        }
        private void ResetTableCornerPt()
        {
            double minX = Math.Min(this.tableLeftDownCornerPt.X, this.tableRightUpCornerPt.X);
            double minY = Math.Min(this.tableLeftDownCornerPt.Y, this.tableRightUpCornerPt.Y);
            double minZ = Math.Min(this.tableLeftDownCornerPt.Z, this.tableRightUpCornerPt.Z);
            double maxX = Math.Max(this.tableLeftDownCornerPt.X, this.tableRightUpCornerPt.X);
            double maxY = Math.Max(this.tableLeftDownCornerPt.Y, this.tableRightUpCornerPt.Y);
            double maxZ = Math.Max(this.tableLeftDownCornerPt.Z, this.tableRightUpCornerPt.Z);
            this.tableLeftDownCornerPt = new Point3d(minX, minY, minZ);
            this.tableRightUpCornerPt = new Point3d(maxX, maxY, maxZ);
        }
        protected List<string> GetTableLayerName()
        {
            List<string> tableLayerNames= new List<string>();
            TypedValue[] tvs = new TypedValue[] {new TypedValue((int)DxfCode.Start, "Line,LWPolyline") };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr= ThColumnInfoUtils.SelectByRectangle(doc.Editor, 
                this.tableLeftDownCornerPt, this.tableRightUpCornerPt, PolygonSelectionMode.Crossing, sf);
            if(psr.Status==PromptStatus.OK)
            {
                List<ObjectId> selObjIds = psr.Value.GetObjectIds().ToList();
                List<Point3d> points = new List<Point3d>();
                using (Transaction trans=doc.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < selObjIds.Count; i++)
                    {
                        Entity ent = trans.GetObject(selObjIds[i], OpenMode.ForRead) as Entity;
                        if(ent ==null )
                        {
                            continue;
                        }
                        if(ent is Line)
                        {
                            Line line = ent as Line;
                            points.Add(line.StartPoint);
                            points.Add(line.EndPoint);
                        }
                        else if(ent is Polyline)
                        {
                            Polyline polyline = ent as Polyline;
                            for(int j=0;j< polyline.NumberOfVertices;j++)
                            {
                                points.Add(polyline.GetPoint3dAt(j));
                            }
                        }
                    }
                    trans.Commit();
                }
                if(points.Count>=2)
                {
                    double minX = points.OrderBy(i => i.X).FirstOrDefault().X;
                    double minY = points.OrderBy(i => i.Y).FirstOrDefault().Y;
                    double minZ = points.OrderBy(i => i.Z).FirstOrDefault().Z;
                    double maxX = points.OrderByDescending(i => i.X).FirstOrDefault().X;
                    double maxY = points.OrderByDescending(i => i.Y).FirstOrDefault().Y;
                    double maxZ = points.OrderByDescending(i => i.Z).FirstOrDefault().Z;
                    Point3d pt1 = new Point3d(minX,minY,minZ);
                    Point3d pt2 = new Point3d(maxX, minY, minZ);
                    Point3d pt3 = new Point3d(maxX, maxY, minZ);
                    Point3d pt4 = new Point3d(minX, maxY, minZ);
                    tableLayerNames.AddRange(GetLayerNames(pt1));
                    tableLayerNames.AddRange(GetLayerNames(pt2));
                    tableLayerNames.AddRange(GetLayerNames(pt3));
                    tableLayerNames.AddRange(GetLayerNames(pt4)); 
                }              
            }
            tableLayerNames = tableLayerNames.Distinct().ToList();
            return tableLayerNames;
        }
        protected List<ObjectId> GetNeedHideObjIds(List<string> layerNames)
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            PromptSelectionResult psr = doc.Editor.SelectAll();
            List<ObjectId> keepObjIds = new List<ObjectId>();
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> selObjIds = psr.Value.GetObjectIds().ToList();
                List<Point3d> points = new List<Point3d>();
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < selObjIds.Count; i++)
                    {
                        Entity ent = trans.GetObject(selObjIds[i], OpenMode.ForRead) as Entity;
                        if (ent == null)
                        {
                            continue;
                        }
                        if(layerNames.IndexOf(ent.Layer)<0)
                        {
                            objectIds.Add(selObjIds[i]);
                        }
                        else
                        {
                            keepObjIds.Add(selObjIds[i]);
                        }
                    }
                    for(int i=0;i< keepObjIds.Count;i++)
                    {
                        Entity ent = trans.GetObject(keepObjIds[i], OpenMode.ForRead) as Entity;
                        if(ent==null)
                        {
                            continue;
                        }
                        if(!(ent is Line || ent is Polyline))
                        {
                            objectIds.Add(keepObjIds[i]);
                            continue;
                        }
                        if(ent.Bounds==null || !ent.Bounds.HasValue)
                        {
                            continue;
                        }
                        Point3d minPt = ent.Bounds.Value.MinPoint;
                        Point3d maxPt = ent.Bounds.Value.MaxPoint;
                        if((minPt.X>=this.tableLeftDownCornerPt.X && minPt.X<=this.tableRightUpCornerPt.X) &&
                            (minPt.Y >= this.tableLeftDownCornerPt.Y && minPt.Y <= this.tableRightUpCornerPt.Y)
                            )
                        {
                            continue;
                        }
                        else if((maxPt.X >= this.tableLeftDownCornerPt.X && maxPt.X <= this.tableRightUpCornerPt.X) &&
                            (maxPt.Y >= this.tableLeftDownCornerPt.Y && maxPt.Y <= this.tableRightUpCornerPt.Y))
                        {
                            continue;
                        }
                        else
                        {
                            objectIds.Add(keepObjIds[i]);
                        }
                    }
                    trans.Commit();
                }
            }
            return objectIds;
        }
        private List<string> GetLayerNames(Point3d pt)
        {
            List<string> layerNames = new List<string>();
            Point3d pt1 = pt + new Vector3d(-5,-5,0);
            Point3d pt3 = pt + new Vector3d(5, 5, 0);
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Line,LWPolyline") };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor,
                pt1, pt3, PolygonSelectionMode.Crossing, sf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> selObjIds = psr.Value.GetObjectIds().ToList();
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < selObjIds.Count; i++)
                    {
                        Entity ent = trans.GetObject(selObjIds[i], OpenMode.ForRead) as Entity;
                        if (ent == null)
                        {
                            continue;
                        }
                        layerNames.Add(ent.Layer);
                    }
                    trans.Commit();
                }
            }
            return layerNames;
        }
    }
}
