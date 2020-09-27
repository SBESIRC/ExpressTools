using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Model.Wall;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelLineWallSeg : ModelWallSeg, IEntityInf
    {
        public IEntity BuildGeometry()
        {
            return BuildLineWallGeometry();
        }
        public double Length => GetBeamLength(); //ToDo
        private LineWallGeometry BuildLineWallGeometry()
        {
            YjkJointQuery yjkJointQuery = new YjkJointQuery(this.DbPath);
            ModelGrid modelGrid = this.Grid;
            ModelJoint startJoint = yjkJointQuery.GetModelJoint(modelGrid.Jt1ID);
            ModelJoint endJoint = yjkJointQuery.GetModelJoint(modelGrid.Jt2ID);
            return new LineWallGeometry()
            {
                StartPoint = new Coordinate(startJoint.X, startJoint.Y),
                EndPoint = new Coordinate(endJoint.X, endJoint.Y),
                Ecc = this.Ecc,
                B = this.WallSect.B,
                H = this.WallSect.H
            };
        }
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
}
