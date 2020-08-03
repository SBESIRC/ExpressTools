using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThColumnInfo.ViewModel;

namespace ThColumnInfo
{
    /// <summary>
    /// 动态绘制柱子
    /// </summary>
    public class ThDrawColumns
    {
        private List<DrawColumnInf> drawColumnInfs = new List<DrawColumnInf>();
        private double pointRange = 5.0;
        private List<List<DrawColumnInf>> xyColumns = new List<List<DrawColumnInf>>(); //按XY坐标排序
        private List<Polyline> columnEnts = new List<Polyline>();
        private List<KeyValuePair<DrawColumnInf, Polyline>> keyValuePairs = new List<KeyValuePair<DrawColumnInf, Polyline>>();
        private Document doc;
        private bool isGoOn = true;
        public bool IsGoOn
        {
            get
            {
                return isGoOn;
            }
        }
        /// <summary>
        /// 获取数据库信息在模型中摆放的位置
        /// </summary>
        public List<ColumnRelateInf> ColumnRelateInfs { get; set; } = new List<ColumnRelateInf>();
        private CalculationInfo calculateInfo = null;
        
        public ThDrawColumns(List<DrawColumnInf> drawColumnInfs, CalculationInfo calculateInfo)
        {
            this.drawColumnInfs = drawColumnInfs;
            this.calculateInfo = calculateInfo;
            doc = ThColumnInfoUtils.GetMdiActiveDocument();
        }
        public void Draw()
        {
            Sort(); //排序
            DrawAllColumn(); //绘制要Jig的柱子
            Drag();            
        }
        private void DrawAllColumn()
        {
            for (int i = 0; i < xyColumns.Count;i++)
            {
                for (int j = 0; j < xyColumns[i].Count; j++)
                {
                    Polyline polyline = DrawColumn(xyColumns[i][j]);
                    keyValuePairs.Add(new KeyValuePair<DrawColumnInf, Polyline>(xyColumns[i][j], polyline));
                    this.columnEnts.Add(polyline);
                }
            }
            Point3d basePt = new Point3d(xyColumns[0][0].X+ xyColumns[0][0].EccX,
                xyColumns[0][0].Y + xyColumns[0][0].EccY,0.0);
            Matrix3d moveMt = Matrix3d.Displacement(basePt.GetVectorTo(Point3d.Origin));
            this.columnEnts.ForEach(i=>i.TransformBy(moveMt)); //把基点移动到原点

            Matrix3d wcsToUcs= ThColumnInfoUtils.UCS2WCS();
            this.columnEnts.ForEach(i => i.TransformBy(wcsToUcs));

        }
        private List<Point3d> GetPolylinePts(Polyline polyline)
        {
            List<Point3d> pts = new List<Point3d>();
            for(int i=0;i< polyline.NumberOfVertices;i++)
            {
                pts.Add(polyline.GetPoint3dAt(i));
            }
            return pts;
        }
        private Polyline DrawColumn(DrawColumnInf drawColumnInf)
        {
            Polyline polyline = new Polyline();
            Point3d pt = new Point3d(drawColumnInf.X+ drawColumnInf.EccX, drawColumnInf.Y + drawColumnInf.EccY, 0.0);
            double rotateAng = ThColumnInfoUtils.AngToRad(drawColumnInf.Rotation);
            Matrix3d rotateMt = Matrix3d.Rotation(rotateAng, Vector3d.ZAxis, Point3d.Origin);
            string spec = drawColumnInf.Spec;
            if(!string.IsNullOrEmpty(drawColumnInf.Spec))
            {
               string[] specs= drawColumnInf.Spec.Split('x');
                if(specs!=null && specs.Length==2)
                {
                    if(BaseFunction.IsNumeric(specs[0]) && BaseFunction.IsNumeric(specs[1]))
                    {
                        double length = Convert.ToDouble(specs[0]);
                        double width = Convert.ToDouble(specs[1]);
                        if(length>0 && width>0)
                        {
                            polyline.AddVertexAt(0,new Point2d(length / 2.0, width / 2.0), 0.0, 0.0, 0.0);
                            polyline.AddVertexAt(1, new Point2d(-length / 2.0, width / 2.0), 0.0, 0.0, 0.0);
                            polyline.AddVertexAt(2, new Point2d(-length / 2.0, -width / 2.0), 0.0, 0.0, 0.0);
                            polyline.AddVertexAt(3, new Point2d(length / 2.0, -width / 2.0), 0.0, 0.0, 0.0);
                            polyline.Closed = true;
                            polyline.TransformBy(rotateMt);
                            Matrix3d moveMt = Matrix3d.Displacement(Point3d.Origin.GetVectorTo(pt));
                            polyline.TransformBy(moveMt);
                        }
                    }
                }
            }
            return polyline;
        }
        private void Sort()
        {
            List<double> yValues = this.drawColumnInfs.Select(i => i.Y).ToList();
            yValues = yValues.Distinct().ToList();
            List<double> tempList = new List<double>();
            while (yValues.Count > 0)
            {
                List<double> exists = tempList.Where(i => Math.Abs(i - yValues[0]) <= this.pointRange).Select(i => i).ToList();
                if (exists == null || exists.Count ==0)
                {
                    tempList.Add(yValues[0]);
                }
                yValues.RemoveAt(0);
            }
            yValues = tempList.OrderBy(i=>i).ToList(); //Y坐标从小
            foreach(double yValue in yValues)
            {
               List<DrawColumnInf> currentRowColumnInfs= this.drawColumnInfs.
                    Where(i => Math.Abs(i.Y - yValue) <= this.pointRange).Select(i => i).ToList();
               if(currentRowColumnInfs==null || currentRowColumnInfs.Count==0)
                {
                    continue;
                }
                currentRowColumnInfs=currentRowColumnInfs.OrderBy(i => i.X).ToList();
                this.xyColumns.Add(currentRowColumnInfs);
            }
        }
        private void Drag()
        {
            if(this.columnEnts==null || this.columnEnts.Count==0)
            {
                return;
            }
            bool doMark = true;
            Point3d basePt = Point3d.Origin;
            double modelAng = this.calculateInfo.Angle;
            modelAng = ThColumnInfoUtils.AngToRad(modelAng);
            if (modelAng != 0.0)
            {
                Matrix3d rotateMt = Matrix3d.Rotation(modelAng, 
                    this.doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis, basePt);
                this.columnEnts.ForEach(i => i.TransformBy(rotateMt));
            }
            Point3d moveTargetPt= basePt;
            ThColumnJig thColumnJig = new ThColumnJig(this.columnEnts, basePt);
            while (doMark)
            {
                PromptResult jigRes = doc.Editor.Drag(thColumnJig);
                if (jigRes.Status == PromptStatus.OK)
                {
                    doMark = false;
                    this.isGoOn = true;
                    moveTargetPt = thColumnJig.Location;
                    break;
                }
                else if(jigRes.Status == PromptStatus.Keyword)
                {
                    if(thColumnJig.KeyWord=="R")
                    {
                        List<Entity> ents = this.columnEnts.Select(i => i.Clone() as Entity).ToList();
                        thColumnJig.TransformEntities();
                        thColumnJig.TransformEntities(ents);
                        using (Transaction trans = doc.TransactionManager.StartTransaction())
                        {
                            BlockTable bt = trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                            BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                            btr.UpgradeOpen();
                            ents.ForEach(i => btr.AppendEntity(i));
                            ents.ForEach(i => trans.AddNewlyCreatedDBObject(i, true));
                            btr.DowngradeOpen();
                            trans.Commit();
                        }
                        PromptPointOptions ppo = new PromptPointOptions("\n请选择重置的基点");
                        ppo.AllowArbitraryInput = false;
                        ppo.AllowNone = false;
                        PromptPointResult ppr= doc.Editor.GetPoint(ppo);
                        if(ppr.Status==PromptStatus.OK)
                        {
                            basePt = ppr.Value;
                            thColumnJig.UpdateMbase(basePt);
                        }
                        if(ents!=null && ents.Count>0)
                        {
                            ThColumnInfoUtils.EraseObjIds(ents.Select(i => i.ObjectId).ToArray());
                        }
                    }
                    else if(thColumnJig.KeyWord == "E")
                    {
                        doMark = false;
                        this.isGoOn = false;
                        break;
                    }
                }
                else if(jigRes.Status == PromptStatus.Cancel)
                {
                    doMark = false;
                    this.isGoOn = false;
                    break;
                }
            }
            if(this.isGoOn)
            {
                doMark = true;
                do
                {
                    thColumnJig.TransformEntities();
                    MoveRotateScaleJig rotateJig = new MoveRotateScaleJig(this.columnEnts, moveTargetPt, JigWay.Rotate, 0, 1);
                    PromptResult jigRes = doc.Editor.Drag(rotateJig);
                    if (jigRes.Status == PromptStatus.OK)
                    {
                        doMark = false;
                        rotateJig.TranformEntities();
                        this.calculateInfo.Angle = BaseFunction.RadToAng(rotateJig.RotateAngle);
                        break;
                    }
                    else if (jigRes.Status == PromptStatus.Cancel)
                    {
                        doMark = false;
                        this.isGoOn = false;
                        break;
                    }
                    else if(jigRes.Status == PromptStatus.Other)
                    {
                        doMark = false;
                        break;
                    }
                } while (doMark);
            }
            if(this.isGoOn)
            {
                //必须赋值完后，方可释放上面的柱子实体
                keyValuePairs.ForEach(i => this.ColumnRelateInfs.Add(new ColumnRelateInf()
                { DbColumnInf = i.Key, InModelPts = GetPolylinePts(i.Value) }));
            }
            //ThColumnInfoUtils.EraseObjIds(columnObjIds.ToArray());//释放已绘制的柱子
            this.columnEnts.ForEach(i => i.Dispose());
        }
    }
}
