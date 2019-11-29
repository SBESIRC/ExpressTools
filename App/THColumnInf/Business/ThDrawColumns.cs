using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        public ThDrawColumns(List<DrawColumnInf> drawColumnInfs)
        {
            this.drawColumnInfs = drawColumnInfs;
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
            Matrix3d alignMt= Matrix3d.AlignCoordinateSystem(
                Point3d.Origin, 
                doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis,
                doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Yaxis,
                doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis,
                Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);
            this.columnEnts.ForEach(i => i.TransformBy(alignMt));
        }
        private List<Point3d> GetPolylinePts(Polyline polyline)
        {
            List<Point3d> pts = new List<Point3d>();
            for(int i=0;i< polyline.NumberOfVertices;i++)
            {

                pts.Add(polyline.GetPoint3dAt(i).TransformBy(doc.Editor.CurrentUserCoordinateSystem));
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
                    if(ThColumnInfoUtils.IsNumeric(specs[0]) && ThColumnInfoUtils.IsNumeric(specs[1]))
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
            double modelAng = 0.0;
            while (doMark)
            {
                ThColumnJig thColumnJig = new ThColumnJig(this.columnEnts, basePt);
                PromptResult jigRes = doc.Editor.Drag(thColumnJig);
                if (jigRes.Status == PromptStatus.OK)
                {
                    doMark = false;
                    this.isGoOn = true;
                    break;
                }
                else if(jigRes.Status == PromptStatus.Keyword)
                {
                    if(thColumnJig.KeyWord=="R")
                    {
                        List<Entity> ents = this.columnEnts.Select(i => i.Clone() as Entity).ToList();
                        ents.ForEach(i => i.TransformBy(doc.Editor.CurrentUserCoordinateSystem));
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
                        }
                        if(ents!=null && ents.Count>0)
                        {
                            ThColumnInfoUtils.EraseObjIds(ents.Select(i => i.ObjectId).ToArray());
                        }
                    }
                    else if(thColumnJig.KeyWord == "A")
                    {
                        basePt = thColumnJig.Position;
                        PromptAngleOptions pao = new PromptAngleOptions("\n输入模型的角度");
                        pao.AllowArbitraryInput = true;
                        pao.AllowZero = true;
                        pao.AllowNone = false;                        
                        PromptDoubleResult pdr = doc.Editor.GetAngle(pao);
                        if (pdr.Status == PromptStatus.OK)
                        {
                            double tempAng = pdr.Value;
                            if (tempAng != modelAng)
                            {
                                if(modelAng!=0.0)
                                {
                                    //先旋转到0度
                                    Matrix3d rotateMt1 = Matrix3d.Rotation(-1.0 * modelAng, Vector3d.ZAxis, basePt);
                                    this.columnEnts.ForEach(i => i.TransformBy(rotateMt1));
                                }                               
                                Matrix3d rotateMt2 = Matrix3d.Rotation(tempAng, Vector3d.ZAxis, basePt);
                                this.columnEnts.ForEach(i => i.TransformBy(rotateMt2));
                                modelAng = tempAng;
                            }
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
            //必须赋值完后，方可释放上面的柱子实体
            keyValuePairs.ForEach(i => this.ColumnRelateInfs.Add(new ColumnRelateInf()
            { DbColumnInf = i.Key, InModelPts = GetPolylinePts(i.Value)}));
            this.columnEnts.ForEach(i => i.Dispose()); //释放已绘制的柱子
        }
    }
}
