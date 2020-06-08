using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.ThBeamInfo.Model;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Query;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.ThBeamInfo.Service
{
    public class ThDrawBeam:IDisposable
    {
        private string dtlCalcPath = "";
        private string dtlModelPath = "";
        private double pointRange = 5.0;
        private int floorNo;
        private double rotateAngle = 0.0;
        private bool isGoOn = true;
        private List<BeamLink> beamLinks = new List<BeamLink>();
        private List<ModelColumnSegCompose> modelColumnSegComposes = new List<ModelColumnSegCompose>();
        private List<ModelBeamSegCompose> modelBeamSegCompose = new List<ModelBeamSegCompose>();

        private List<List<int>> xyColumns = new List<List<int>>(); //按XY坐标排序 (记录ModelColumnSeg->JtID)

        private List<Tuple<int, Polyline>> columnEnts = new List<Tuple<int, Polyline>>();
        private List<Tuple<CalcBeamSeg, Polyline>> beamEnts = new List<Tuple<CalcBeamSeg, Polyline>>();
        public List<BeamRelateInf> ColumnRelateInfs { get; set; } = new List<BeamRelateInf>();
        private YjkJointQuery yjkJointQuery;
        private Document doc;
        /// <summary>
        /// 是否继续
        /// </summary>
        public bool IsGoOn => this.isGoOn;
        /// <summary>
        /// 旋转角度
        /// </summary>
       
        public double RotateAngle=> this.rotateAngle;
        
        public ThDrawBeam(string dtlModelPath, string dtlCalcPath,int floorNo)
        {
            this.dtlModelPath = dtlModelPath;
            this.dtlCalcPath = dtlCalcPath;
            this.floorNo = floorNo;
            Init();
        }
        private void Init()
        {
            this.doc = CadTool.GetMdiActiveDocument();
            this.yjkJointQuery = new YjkJointQuery(this.dtlCalcPath);
            this.modelColumnSegComposes = new YjkColumnQuery(this.dtlModelPath).GetModelColumnSegComposes(this.floorNo);
            this.modelBeamSegCompose= new YjkBeamQuery(this.dtlModelPath).GetModelBeamSegComposes(this.floorNo);
            BuildBeamLink buildBeamLink = new BuildBeamLink(this.dtlCalcPath, this.floorNo);
            buildBeamLink.Build();
            this.beamLinks = buildBeamLink.BeamLinks;
        }
        public void Draw()
        {
            Sort();
            DrawCurrentFloorColumns();
            DrawCurrentFloorBeams();
            Drag();
        }
        private void Drag()
        {
            List<Polyline> columnBeamEnts = new List<Polyline>();
            this.columnEnts.ForEach(i =>
            {
                if (i.Item2 != null)
                {
                    columnBeamEnts.Add(i.Item2);
                }
            });
            this.beamEnts.ForEach(i =>
                {
                    if (i.Item2 != null)
                    {
                        columnBeamEnts.Add(i.Item2);
                    }
                });
            bool doMark = true;
            Point3d basePt = Point3d.Origin;
            Point3d moveTargetPt = basePt;
            ThBeamJig thBeamJig = new ThBeamJig(columnBeamEnts, basePt);
            while (doMark)
            {
                PromptResult jigRes = doc.Editor.Drag(thBeamJig);
                if (jigRes.Status == PromptStatus.OK)
                {
                    doMark = false;
                    this.isGoOn = true;
                    moveTargetPt = thBeamJig.Location;
                    break;
                }
                else if (jigRes.Status == PromptStatus.Keyword)
                {
                    if (thBeamJig.KeyWord == "R")
                    {
                        List<Entity> ents = columnBeamEnts.Select(i => i.Clone() as Entity).ToList();
                        thBeamJig.TransformEntities();
                        thBeamJig.TransformEntities(ents);
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
                        PromptPointResult ppr = doc.Editor.GetPoint(ppo);
                        if (ppr.Status == PromptStatus.OK)
                        {
                            basePt = ppr.Value;
                            thBeamJig.UpdateMbase(basePt);
                        }
                        if (ents != null && ents.Count > 0)
                        {
                            CadTool.EraseObjIds(ents.Select(i => i.ObjectId).ToArray());
                        }
                    }
                    else if (thBeamJig.KeyWord == "E")
                    {
                        doMark = false;
                        this.isGoOn = false;
                        break;
                    }
                }
                else if (jigRes.Status == PromptStatus.Cancel)
                {
                    doMark = false;
                    this.isGoOn = false;
                    break;
                }
            }
            if (this.isGoOn)
            {
                doMark = true;
                do
                {
                    thBeamJig.TransformEntities();
                    MoveRotateScaleJig rotateJig = new MoveRotateScaleJig(columnBeamEnts, moveTargetPt, JigWay.Rotate, 0, 1);
                    PromptResult jigRes = doc.Editor.Drag(rotateJig);
                    if (jigRes.Status == PromptStatus.OK)
                    {
                        doMark = false;
                        rotateJig.TranformEntities();
                        this.rotateAngle= Utils.RadToAng(rotateJig.RotateAngle);
                        break;
                    }
                    else if (jigRes.Status == PromptStatus.Cancel)
                    {
                        doMark = false;
                    }
                } while (doMark);
            }
            //必须赋值完后，方可释放上面的柱子实体
            this.beamEnts.ForEach(i => this.ColumnRelateInfs.Add(new BeamRelateInf()
            { DbBeamInf = i.Item1, InModelPts = CadTool.GetPolylinePts(i.Item2)}));
            columnBeamEnts.ForEach(i => i.Dispose());
        }
        private void DrawCurrentFloorBeams()
        {
            for(int i=0;i<this.beamLinks.Count;i++)
            {
                for (int j = 0; j < this.beamLinks[i].Beams.Count; j++)
                {
                    var calcBeamSeg = this.beamLinks[i].Beams[j];
                    Polyline polyline = DrawBeam(calcBeamSeg);
                    this.beamEnts.Add(Tuple.Create<CalcBeamSeg, Polyline>(calcBeamSeg, polyline));
                }
            }
        }
        private Point3d GetBeamJtPoint(CalcJoint calcJoint)
        {
            Point3d res = Point3d.Origin;
            var point = calcJoint.GetCoordinate();
            res = ThBeamUtils.PointToPoint3d(point);
            return res;
        }
        private void DrawCurrentFloorColumns()
        {
            for (int i = 0; i < xyColumns.Count; i++)
            {
                for (int j = 0; j < xyColumns[i].Count; j++)
                {
                    Polyline polyline = DrawColumn(GetModelColumnSeg(xyColumns[i][j]));
                    this.columnEnts.Add(Tuple.Create<int, Polyline>(xyColumns[i][j], polyline));
                }
            }
            var firstColumn = GetModelColumnSeg(xyColumns[0][0]);
            Point3d basePt = new Point3d(firstColumn.Joint.X + firstColumn.ColumnSeg.EccX,
                firstColumn.Joint.Y + firstColumn.ColumnSeg.EccY, 0.0);
            Matrix3d moveMt = Matrix3d.Displacement(basePt.GetVectorTo(Point3d.Origin));
            this.columnEnts.ForEach(i => i.Item2.TransformBy(moveMt)); //把基点移动到原点
            Matrix3d wcsToUcs = CadTool.UCS2WCS();
            this.columnEnts.ForEach(i => i.Item2.TransformBy(wcsToUcs));
        }
        private void Sort()
        {
            List<double> yValues = this.modelColumnSegComposes.Select(i => i.Joint.Y).ToList();
            yValues = yValues.Distinct().ToList();
            List<double> tempList = new List<double>();
            while (yValues.Count > 0)
            {
                List<double> exists = tempList.Where(i => Math.Abs(i - yValues[0]) 
                <= this.pointRange).Select(i => i).ToList();
                if (exists == null || exists.Count == 0)
                {
                    tempList.Add(yValues[0]);
                }
                yValues.RemoveAt(0);
            }
            yValues = tempList.OrderBy(i => i).ToList(); //Y坐标从小
            foreach (double yValue in yValues)
            {
                List<int> currentRowColumnInfs = this.modelColumnSegComposes.
                     Where(i => Math.Abs(i.Joint.Y - yValue) <= this.pointRange).Select(i => i.ColumnSeg.JtID).ToList();
                if (currentRowColumnInfs == null || currentRowColumnInfs.Count == 0)
                {
                    continue;
                }
                currentRowColumnInfs = currentRowColumnInfs.OrderBy(i => GetModelColumnSeg(i).Joint.X).ToList();
                this.xyColumns.Add(currentRowColumnInfs);
            }
        }
        /// <summary>
        /// 通过计算书中的梁段得到模型中的记录
        /// </summary>
        /// <param name="calcBeamSeg"></param>
        /// <returns></returns>
        private ModelBeamSegCompose GetModelBeamSegCompose(CalcBeamSeg calcBeamSeg)
        {
           return this.modelBeamSegCompose.Where(i => i.Floor.No_ == calcBeamSeg.FlrNo && i.BeamSeg.No_ == calcBeamSeg.MdlNo).First();
        }
        private ModelColumnSegCompose GetModelColumnSeg(int jtID)
        {
           return this.modelColumnSegComposes.Where(i => i.ColumnSeg.JtID == jtID).Select(i=>i).First();
        }
        private Polyline DrawBeam(CalcBeamSeg calcBeamSeg)
        {
            Polyline polyline=null;
            Point3d startPt = GetBeamJtPoint(yjkJointQuery.GetCalcJoint(calcBeamSeg.Jt1));
            Point3d endPt = GetBeamJtPoint(yjkJointQuery.GetCalcJoint(calcBeamSeg.Jt2));
            ModelBeamSegCompose modelBeamSegCompose = GetModelBeamSegCompose(calcBeamSeg);
            int ecc = modelBeamSegCompose.BeamSeg.Ecc;
            string spec = modelBeamSegCompose.BeamSect.Spec;
            if(string.IsNullOrEmpty(spec))
            {
                return polyline;
            }
            string[] specs = spec.Split('x');
            if (specs != null && specs.Length == 2)
            {
                if (Utils.IsNumeric(specs[0]) && Utils.IsNumeric(specs[1]))
                {
                    double length = Convert.ToDouble(specs[0]); //截面宽度
                    double width = Convert.ToDouble(specs[1]);  //截面高度
                    if (length > 0 && width > 0)
                    {
                        polyline = new Polyline();
                        Vector3d vec = endPt - startPt;
                        Vector3d perpendVec = vec.GetPerpendicularVector();
                        Point3d sp = startPt + perpendVec.GetNormal().MultiplyBy(ecc);
                        Point3d ep = endPt + perpendVec.GetNormal().MultiplyBy(ecc);
                        Point3d pt1 = sp + perpendVec.GetNormal().MultiplyBy(length / 2.0);
                        Point3d pt2 = ep + perpendVec.GetNormal().MultiplyBy(length / 2.0);
                        Point3d pt3 = sp - perpendVec.GetNormal().MultiplyBy(length / 2.0);
                        Point3d pt4 = ep - perpendVec.GetNormal().MultiplyBy(length / 2.0);
                        polyline.AddVertexAt(0, new Point2d(pt1.X, pt1.Y), 0.0, 0.0, 0.0);
                        polyline.AddVertexAt(1, new Point2d(pt2.X, pt2.Y), 0.0, 0.0, 0.0);
                        polyline.AddVertexAt(2, new Point2d(pt3.X, pt3.Y), 0.0, 0.0, 0.0);
                        polyline.AddVertexAt(3, new Point2d(pt4.X, pt4.Y), 0.0, 0.0, 0.0);
                        polyline.Closed = true;
                    }
                }
            }
            return polyline;
        }
        private Polyline DrawColumn(ModelColumnSegCompose modelColumnSegCompose)
        {
            Polyline polyline = null;
            Point3d pt = new Point3d(modelColumnSegCompose.Joint.X + modelColumnSegCompose.ColumnSeg.EccX,
                modelColumnSegCompose.Joint.Y + modelColumnSegCompose.ColumnSeg.EccY, 0.0);
            double rotateAng = Utils.AngToRad(modelColumnSegCompose.ColumnSeg.Rotation);
            Matrix3d rotateMt = Matrix3d.Rotation(rotateAng, Vector3d.ZAxis, Point3d.Origin);
            string spec = modelColumnSegCompose.ColumnSect.Spec;
            if (!string.IsNullOrEmpty(spec))
            {
                string[] specs = spec.Split('x');
                if (specs != null && specs.Length == 2)
                {
                    if (Utils.IsNumeric(specs[0]) && Utils.IsNumeric(specs[1]))
                    {
                        double length = Convert.ToDouble(specs[0]);
                        double width = Convert.ToDouble(specs[1]);
                        if (length > 0 && width > 0)
                        {
                            polyline = new Polyline();
                            polyline.AddVertexAt(0, new Point2d(length / 2.0, width / 2.0), 0.0, 0.0, 0.0);
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

        public void Dispose()
        {
            this.modelBeamSegCompose.Clear();
            this.modelColumnSegComposes.Clear();
        }
    }
}
