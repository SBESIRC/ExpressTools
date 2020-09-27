using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Model.Beam;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// 直梁
    /// </summary>
    public class ModelLineBeamSeg: ModelBeamSeg,IEntityInf
    {
        public IEntity BuildGeometry()
        {
            return BuildLineBeamGeo();
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
        public bool IsCollinear(ModelLineBeamSeg otherBeamSeg)
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
    }
}
