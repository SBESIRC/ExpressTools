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
using ThStructureCheck.Common.Service;

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
        private List<ModelColumnSegCompose> modelColumnSegComposes = new List<ModelColumnSegCompose>();
        private List<ModelBeamSegCompose> modelBeamSegComposes = new List<ModelBeamSegCompose>();
        private List<ModelWallSegCompose> modelWallSegComposes = new List<ModelWallSegCompose>();
        private List<List<int>> xyColumns = new List<List<int>>(); //按XY坐标排序 (记录ModelColumnSeg->JtID)
        private List<Tuple<int, Polyline>> columnEnts = new List<Tuple<int, Polyline>>();
        private List<Tuple<YjkEntityInfo, Polyline,Curve>> beamEnts = new List<Tuple<YjkEntityInfo, Polyline, Curve>>();
        private List<Tuple<BeamLink, List<Polyline>, List<Curve>>> splitSpanBeams = new List<Tuple<BeamLink, List<Polyline>, List<Curve>>>();
        private List<Tuple<YjkEntityInfo, Polyline>> wallEnts = new List<Tuple<YjkEntityInfo, Polyline>>();
        private YjkJointQuery modelJointQuery;
        private Document doc;
        private List<Entity> columnBeamEnts = new List<Entity>(); //储存Drag显示的物体
        private BuildFlrBeamLink buildFlrBeamLink;

        /// <summary>
        /// Yjk中Model库中梁段对应Cad中的几何图形
        /// </summary>
        public List<Tuple<YjkEntityInfo, Polyline, Curve>> BeamEnts => beamEnts;
        public List<Tuple<BeamLink, List<Polyline>, List<Curve>>> SplitSpanBeams => splitSpanBeams;
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
            this.modelJointQuery= new YjkJointQuery(this.dtlModelPath);
            this.modelColumnSegComposes = new YjkColumnQuery(this.dtlModelPath).GetModelColumnSegComposes(this.floorNo);
            this.modelBeamSegComposes = new YjkBeamQuery(this.dtlModelPath).GetModelBeamSegComposes(this.floorNo);
            this.modelWallSegComposes = new YjkWallQuery(this.dtlModelPath).GetModelWallSegComposes(this.floorNo);
            this.buildFlrBeamLink = new BuildFlrBeamLink(this.dtlModelPath, this.floorNo);
            this.buildFlrBeamLink.Build();
        }
        public void Draw()
        {
            Sort();
            DrawColumns();
            DrawBeams();
            DrawWalls();
            MoveToOrigin();
            Drag();
        }
        private void MoveToOrigin()
        {
            var firstColumn = GetModelColumnSeg(xyColumns[0][0]);
            Point3d basePt = new Point3d(firstColumn.Joint.X + firstColumn.ColumnSeg.EccX,
                firstColumn.Joint.Y + firstColumn.ColumnSeg.EccY, 0.0);
            Matrix3d moveMt = Matrix3d.Displacement(basePt.GetVectorTo(Point3d.Origin));
            //把基点移动到原点
            this.columnEnts.ForEach(o => o.Item2.TransformBy(moveMt));
            this.splitSpanBeams.ForEach(o => 
            {
                if(o.Item2!=null)
                {
                    o.Item2.ForEach(m => m.TransformBy(moveMt));
                }
                if(o.Item3 != null)
                {
                    o.Item3.ForEach(m => m.TransformBy(moveMt));
                }                
            });
            this.wallEnts.ForEach(o => o.Item2.TransformBy(moveMt));
            Matrix3d wcsToUcs = CadTool.UCS2WCS();
            this.columnEnts.ForEach(o => o.Item2.TransformBy(wcsToUcs));
            this.splitSpanBeams.ForEach(o =>
            {
                if (o.Item2 != null)
                {
                    o.Item2.ForEach(m => m.TransformBy(wcsToUcs));
                }
                if (o.Item3 != null)
                {
                    o.Item3.ForEach(m => m.TransformBy(wcsToUcs));
                }
            });
            this.wallEnts.ForEach(i => i.Item2.TransformBy(wcsToUcs));
        }
        private void Drag()
        {            
            this.columnEnts.ForEach(o => this.columnBeamEnts.Add(o.Item2));
            this.splitSpanBeams.ForEach(o => 
            {
                this.columnBeamEnts.AddRange(o.Item2);
                this.columnBeamEnts.AddRange(o.Item3);
            });
            this.wallEnts.ForEach(o => this.columnBeamEnts.Add(o.Item2));
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
            columnBeamEnts.ForEach(i=>CadTool.AddToBlockTable(i));
        }
        private Point3d GetSegJtPoint(YjkEntityInfo yjkEntity)
        {
            Point jointPt = new Point();
            if (yjkEntity is CalcJoint calcJoint)
            {
                jointPt = calcJoint.GetCoordinate();
            }
            else if(yjkEntity is ModelJoint modelJoint)
            {
                jointPt = modelJoint.GetCoordinate();
            }
            return CadTool.PointToPoint3d(jointPt);
        }
        private void DrawColumns()
        {
            for (int i = 0; i < xyColumns.Count; i++)
            {
                for (int j = 0; j < xyColumns[i].Count; j++)
                {
                    Polyline polyline = DrawColumn(GetModelColumnSeg(xyColumns[i][j]));
                    if(polyline!=null)
                    {
                        polyline.ColorIndex = 6;
                        this.columnEnts.Add(Tuple.Create<int, Polyline>(xyColumns[i][j], polyline));
                    }
                }
            }
        }
        private void DrawBeams()
        {
            for (int i=0;i< this.buildFlrBeamLink.MainBeamLinks.Count;i++)
            {
                List<Polyline> outLines = new List<Polyline>();
                List<Curve> centerLines = new List<Curve>();
                foreach(var beamSeg in this.buildFlrBeamLink.MainBeamLinks[i].Beams)
                {
                    var res = DrawLineBeam(beamSeg as ModelBeamSeg);
                    if (res != null)
                    {
                        if(i%2==0)
                        {
                            res.Item1.ColorIndex = 1;
                        }
                        else
                        {
                            res.Item1.ColorIndex = 2;
                        }
                        outLines.Add(res.Item1);
                        centerLines.Add(res.Item2);                        
                    }
                }
                this.splitSpanBeams.Add(Tuple.Create<BeamLink, List<Polyline>, List<Curve>>
                    (this.buildFlrBeamLink.MainBeamLinks[i], outLines, centerLines));
            }
            for (int i = 0; i < this.buildFlrBeamLink.SecondaryBeamLinks.Count; i++)
            {
                List<Polyline> outLines = new List<Polyline>();
                List<Curve> centerLines = new List<Curve>();
                foreach (var beamSeg in this.buildFlrBeamLink.SecondaryBeamLinks[i].Beams)
                {
                    var res = DrawLineBeam(beamSeg as ModelBeamSeg);
                    if (res != null)
                    {
                        if (i % 2 == 0)
                        {
                            res.Item1.ColorIndex = 3;
                        }
                        else
                        {
                            res.Item1.ColorIndex = 4;
                        }
                        outLines.Add(res.Item1);
                        centerLines.Add(res.Item2);
                    }
                }
                this.splitSpanBeams.Add(Tuple.Create<BeamLink, List<Polyline>, List<Curve>>
                    (this.buildFlrBeamLink.SecondaryBeamLinks[i], outLines, centerLines));
            }
        }
        private void DrawWalls()
        {
            foreach (var modelWallSegRecord in this.modelWallSegComposes)
            {
                Polyline wallFace = DrawWall(modelWallSegRecord);
                if (wallFace != null)
                {
                    wallFace.ColorIndex = 5;
                    wallEnts.Add(Tuple.Create<YjkEntityInfo, Polyline>(modelWallSegRecord.WallSeg, wallFace));
                }
            }
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
        private ModelColumnSegCompose GetModelColumnSeg(int jtID)
        {
           return this.modelColumnSegComposes.Where(i => i.ColumnSeg.JtID == jtID).Select(i=>i).First();
        }
        private ModelBeamSegCompose GetModelBeamSeg(ModelBeamSeg modelBeamSeg)
        {
            return this.modelBeamSegComposes.Where(o => o.BeamSeg.ID == modelBeamSeg.ID).First();
        }
        /// <summary>
        /// 绘制直梁
        /// </summary>
        /// <param name="modelBeamSegCompose"></param>
        /// <returns></returns>
        private Tuple<Polyline,Curve> DrawLineBeam(ModelBeamSeg modelBeamSeg)
        {
            Polyline polyline = null;
            Line line = null;
            ModelBeamSegCompose modelBeamSegCompose = GetModelBeamSeg(modelBeamSeg);
            ModelGrid grid = modelBeamSegCompose.BeamSeg.Grid;
            Point3d startPt = GetSegJtPoint(modelJointQuery.GetModelJoint(grid.Jt1ID));
            Point3d endPt = GetSegJtPoint(modelJointQuery.GetModelJoint(grid.Jt2ID));
            int ecc = modelBeamSegCompose.BeamSeg.Ecc;
            string spec = modelBeamSegCompose.BeamSect.Spec;
            if (string.IsNullOrEmpty(spec))
            {
                return null;
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
                        Vector3d offsetVec = GeometricCalculation.GetOffsetDirection(startPt, endPt);
                        Point3d sp = startPt + offsetVec.GetNormal().MultiplyBy(ecc);
                        Point3d ep = endPt + offsetVec.GetNormal().MultiplyBy(ecc);
                        line = new Line(sp, ep);
                        Point3d pt1 = sp + offsetVec.GetNormal().MultiplyBy(length / 2.0);
                        Point3d pt2 = ep + offsetVec.GetNormal().MultiplyBy(length / 2.0);
                        Point3d pt4 = sp - offsetVec.GetNormal().MultiplyBy(length / 2.0);
                        Point3d pt3 = ep - offsetVec.GetNormal().MultiplyBy(length / 2.0);
                        polyline.AddVertexAt(0, new Point2d(pt1.X, pt1.Y), 0.0, 0.0, 0.0);
                        polyline.AddVertexAt(1, new Point2d(pt2.X, pt2.Y), 0.0, 0.0, 0.0);
                        polyline.AddVertexAt(2, new Point2d(pt3.X, pt3.Y), 0.0, 0.0, 0.0);
                        polyline.AddVertexAt(3, new Point2d(pt4.X, pt4.Y), 0.0, 0.0, 0.0);
                        polyline.Closed = true;
                    }
                }
            }
            return Tuple.Create<Polyline, Curve>(polyline, line);
        }
        /// <summary>
        /// 绘制弧梁
        /// </summary>
        /// <param name="modelBeamSegCompose"></param>
        /// <returns></returns>
        private Tuple<Polyline, Curve> DrawArcBeam(ModelBeamSegCompose modelBeamSegCompose)
        {
            //ToDo Draw arc beam later
            Polyline polyline = null;
            Line line = null;            
            return Tuple.Create<Polyline, Curve>(polyline, line);
        }
        private Polyline DrawColumn(ModelColumnSegCompose modelColumnSegCompose)
        {
            Polyline polyline = null;
            switch(modelColumnSegCompose.ColumnSect.Kind)
            {
                case 1:
                    polyline = DrawRectangleColumn(modelColumnSegCompose);
                    break;
                case 28:
                    polyline = DrawLTypeColumn(modelColumnSegCompose);
                    break;
            }
            return polyline;
        }
        private Polyline DrawRectangleColumn(ModelColumnSegCompose modelColumnSegCompose)
        {
            Polyline polyline = null;
            Point3d pt = new Point3d(modelColumnSegCompose.Joint.X, modelColumnSegCompose.Joint.Y , 0.0);
            double rotateAng = Utils.AngToRad(modelColumnSegCompose.ColumnSeg.Rotation);
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

                            Matrix3d rotateMt = Matrix3d.Rotation(rotateAng, Vector3d.ZAxis, Point3d.Origin);
                            polyline.TransformBy(rotateMt);

                            Vector3d xRotateVec = Vector3d.XAxis.TransformBy(rotateMt);
                            Vector3d yRotateVec = Vector3d.YAxis.TransformBy(rotateMt);
                            Point3d originPt = Point3d.Origin + xRotateVec.GetNormal().MultiplyBy(modelColumnSegCompose.ColumnSeg.EccX);
                            originPt = originPt + yRotateVec.GetNormal().MultiplyBy(modelColumnSegCompose.ColumnSeg.EccY);

                            Matrix3d moveMt = Matrix3d.Displacement(Point3d.Origin.GetVectorTo(originPt));
                            polyline.TransformBy(moveMt);

                            moveMt = Matrix3d.Displacement(Point3d.Origin.GetVectorTo(pt));
                            polyline.TransformBy(moveMt);
                        }
                    }
                }
            }
            return polyline;
        }
        private Polyline DrawLTypeColumn(ModelColumnSegCompose modelColumnSegCompose)
        {
            Polyline polyline = null;
            Point3d pt = new Point3d(modelColumnSegCompose.Joint.X, modelColumnSegCompose.Joint.Y, 0.0);
            double rotateAng = Utils.AngToRad(modelColumnSegCompose.ColumnSeg.Rotation);
            string spec = modelColumnSegCompose.ColumnSect.Spec;
            if (string.IsNullOrEmpty(spec))
            {
                return polyline;
            }
            string[] specs = spec.Split('x');
            if(specs.Length != 4)
            {
                return polyline;
            }
            if (Utils.IsNumeric(specs[0]) && Utils.IsNumeric(specs[1]) &&
                Utils.IsNumeric(specs[2]) && Utils.IsNumeric(specs[3]))
            {
                double length =Convert.ToDouble(specs[0]);
                double width = Convert.ToDouble(specs[1]);
                length = Math.Abs(length);
                width = Math.Abs(width);
                if (length > 0 && width > 0)
                {
                    polyline = new Polyline();
                    polyline.AddVertexAt(0, new Point2d(length / 2.0, width / 2.0), 0.0, 0.0, 0.0);
                    polyline.AddVertexAt(1, new Point2d(-length / 2.0, width / 2.0), 0.0, 0.0, 0.0);
                    polyline.AddVertexAt(2, new Point2d(-length / 2.0, -width / 2.0), 0.0, 0.0, 0.0);
                    polyline.AddVertexAt(3, new Point2d(length / 2.0, -width / 2.0), 0.0, 0.0, 0.0);
                    polyline.Closed = true;

                    Matrix3d rotateMt = Matrix3d.Rotation(rotateAng, Vector3d.ZAxis, Point3d.Origin);
                    polyline.TransformBy(rotateMt);

                    Vector3d xRotateVec = Vector3d.XAxis.TransformBy(rotateMt);
                    Vector3d yRotateVec = Vector3d.YAxis.TransformBy(rotateMt);
                    Point3d originPt = Point3d.Origin + xRotateVec.GetNormal().MultiplyBy(modelColumnSegCompose.ColumnSeg.EccX);
                    originPt = originPt + yRotateVec.GetNormal().MultiplyBy(modelColumnSegCompose.ColumnSeg.EccY);

                    Matrix3d moveMt = Matrix3d.Displacement(Point3d.Origin.GetVectorTo(originPt));
                    polyline.TransformBy(moveMt);

                    moveMt = Matrix3d.Displacement(Point3d.Origin.GetVectorTo(pt));
                    polyline.TransformBy(moveMt);
                }
            }
            return polyline;
        }
        private Polyline DrawWall(ModelWallSegCompose wallSegCompose)
        {
            Polyline polyline = null;
            ModelGrid grid = wallSegCompose.WallSeg.Grid;
            Point3d startPt = GetSegJtPoint(modelJointQuery.GetModelJoint(grid.Jt1ID));
            Point3d endPt = GetSegJtPoint(modelJointQuery.GetModelJoint(grid.Jt2ID));
            int ecc = wallSegCompose.WallSeg.Ecc;
            double b = wallSegCompose.WallSect.B;
            if (b<=0.0)
            {
                return polyline;
            }
            polyline = new Polyline();
            Vector3d offsetVec = GeometricCalculation.GetOffsetDirection(startPt, endPt);
            Point3d sp = startPt + offsetVec.GetNormal().MultiplyBy(ecc);
            Point3d ep = endPt + offsetVec.GetNormal().MultiplyBy(ecc);
            Point3d pt1 = sp + offsetVec.GetNormal().MultiplyBy(b / 2.0);
            Point3d pt2 = ep + offsetVec.GetNormal().MultiplyBy(b / 2.0);
            Point3d pt4 = sp - offsetVec.GetNormal().MultiplyBy(b / 2.0);
            Point3d pt3 = ep - offsetVec.GetNormal().MultiplyBy(b / 2.0);
            polyline.AddVertexAt(0, new Point2d(pt1.X, pt1.Y), 0.0, 0.0, 0.0);
            polyline.AddVertexAt(1, new Point2d(pt2.X, pt2.Y), 0.0, 0.0, 0.0);
            polyline.AddVertexAt(2, new Point2d(pt3.X, pt3.Y), 0.0, 0.0, 0.0);
            polyline.AddVertexAt(3, new Point2d(pt4.X, pt4.Y), 0.0, 0.0, 0.0);
            polyline.Closed = true;
            return polyline;
        }
        public void Dispose()
        {
            this.modelBeamSegComposes.Clear();
            this.modelColumnSegComposes.Clear();
            this.modelWallSegComposes.Clear();
            this.columnBeamEnts.Clear();
        }
    }
}
