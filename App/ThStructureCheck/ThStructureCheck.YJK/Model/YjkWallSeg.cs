using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelWallSeg : YjkEntityInfo,IEntityInf
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int GridID { get; set; }
        public int Ecc { get; set; }
        public int HDiff1 { get; set; }
        public int HDiff2 { get; set; }
        public int HDiffB { get; set; }
        public int Sloping { get; set; }
        public int EccDown { get; set; }
        public int Offset1 { get; set; }
        public int Offset2 { get; set; }
        public ModelGrid Grid
        {
            get
            {
                return new YjkGridQuery(this.DbPath).GetModelGrid(this.GridID);
            }
        }
        public ModelWallSect WallSect
        {
            get
            {
                return new YjkWallQuery(this.DbPath).GetModelWallSect(this.SectID);
            }
        }

        public IEntity BuildGeometry()
        {
            //ToDo 后续增加弧墙
            return BuildLineWallGeometry();
        }
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
    }
}
