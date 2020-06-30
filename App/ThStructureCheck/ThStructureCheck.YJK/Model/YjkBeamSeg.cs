using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.YJK.Model
{
    public class ModelBeamSeg : YjkEntityInfo,IEntityInf
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int GridID { get; set; }
        public int Ecc { get; set; }
        public int HDiff1 { get; set; }
        public int HDiff2 { get; set; }
        public double Rotation { get; set; }
        public bool IsCollinear(ModelBeamSeg otherBeamSeg)
        {
            bool result = false;
            YjkJointQuery yjkJointQuery = new YjkJointQuery(otherBeamSeg.DbPath);
            ModelGrid thisGrid = new YjkGridQuery(otherBeamSeg.DbPath).GetModelGrid(this.GridID);
            ModelGrid otherGrid = new YjkGridQuery(otherBeamSeg.DbPath).GetModelGrid(otherBeamSeg.GridID);
            Point thisStartPt = yjkJointQuery.GetModelJoint(thisGrid.Jt1ID).GetCoordinate();
            Point thisEndPt = yjkJointQuery.GetModelJoint(thisGrid.Jt2ID).GetCoordinate();
            Point otherStartPt = yjkJointQuery.GetModelJoint(otherGrid.Jt1ID).GetCoordinate();
            Point otherEndPt = yjkJointQuery.GetModelJoint(otherGrid.Jt2ID).GetCoordinate();
            if (MathLogic.ThreePointsCollinear(thisStartPt, thisEndPt, otherStartPt) &&
                MathLogic.ThreePointsCollinear(thisStartPt, thisEndPt, otherEndPt))
            {
                result = true;
            }
            return result;
        }
        public ModelGrid Grid
        {
            get
            {
                return new YjkGridQuery(this.DbPath).GetModelGrid(this.GridID);
            }
        }
        public ModelBeamSect BeamSect
        {
            get
            {
                return new YjkBeamQuery(this.DbPath).GetModelBeamSect(this.SectID);
            }
        }
        public IEntity BuildGeometry()
        {
            //ToDo 后续若增加弧梁
            return BuildLineBeamGeo();
        }
        private LineBeamGeometry BuildLineBeamGeo()
        {
            YjkJointQuery yjkJointQuery = new YjkJointQuery(this.DbPath);
            ModelGrid modelGrid = this.Grid;
            ModelJoint startJoint = yjkJointQuery.GetModelJoint(modelGrid.Jt1ID);
            ModelJoint endJoint = yjkJointQuery.GetModelJoint(modelGrid.Jt2ID);
            List<double> datas = Utils.GetDoubleDatas(this.BeamSect.Spec);
            if (datas.Count == 0)
            {
                datas.Add(0.0);
                datas.Add(0.0);
            }
            else if (datas.Count == 1)
            {
                datas.Add(0.0);
            }
            return new LineBeamGeometry()
            {
                StartPoint = new Coordinate(startJoint.X, startJoint.Y),
                EndPoint = new Coordinate(endJoint.X, endJoint.Y),
                Ecc = this.Ecc,               
                Rotation = this.Rotation,
                B = datas[0],
                H = datas[1]
            };
        }
        public double Length => GetBeamLength();
        private double GetBeamLength()
        {
            ModelGrid modelGrid = this.Grid;
            YjkJointQuery yjkJointQuery = new YjkJointQuery(this.DbPath);
            ModelJoint startJoint = yjkJointQuery.GetModelJoint(modelGrid.Jt1ID);
            ModelJoint endJoint = yjkJointQuery.GetModelJoint(modelGrid.Jt2ID);
            Point startPoint = startJoint.GetCoordinate();
            Point endPoint = endJoint.GetCoordinate();
            Coordinate startCoord = new Coordinate(startPoint.X, startPoint.Y);
            Coordinate endCoord = new Coordinate(endPoint.X, endPoint.Y);
            return startCoord.DistanceTo(endCoord);
        }
    }
    public class CalcBeamSeg : YjkEntityInfo
    {
        public int TowNo { get; set; }
        public int FlrNo { get; set; }
        public int MdlFlr { get; set; }
        public int MdlNo { get; set; }
        public int Jt1 { get; set; }
        public int Jt2 { get; set; }

        public bool IsCollinear(CalcBeamSeg otherBeamSeg)
        {
            bool result = false;
            YjkJointQuery yjkJointQuery = new YjkJointQuery(base.DbPath);
            Point thisStartPt  = yjkJointQuery.GetCalcJoint(this.Jt1).GetCoordinate();
            Point thisEndPt = yjkJointQuery.GetCalcJoint(this.Jt2).GetCoordinate();
            Point otherStartPt = yjkJointQuery.GetCalcJoint(otherBeamSeg.Jt1).GetCoordinate();
            Point otherEndPt = yjkJointQuery.GetCalcJoint(otherBeamSeg.Jt2).GetCoordinate();
            thisStartPt.ResetZ();
            thisEndPt.ResetZ();
            otherStartPt.ResetZ();
            otherEndPt.ResetZ();
            if (MathLogic.ThreePointsCollinear(thisStartPt, thisEndPt, otherStartPt) &&
                MathLogic.ThreePointsCollinear(thisStartPt, thisEndPt, otherEndPt))
            {
                result = true;
            }
            return result;
        }
    }
}
