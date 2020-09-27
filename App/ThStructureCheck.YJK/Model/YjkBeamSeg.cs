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
    public class ModelBeamSeg : YjkEntityInfo
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int GridID { get; set; }
        public int Ecc { get; set; }
        public int HDiff1 { get; set; }
        public int HDiff2 { get; set; }
        public double Rotation { get; set; }

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
        public static ModelBeamSeg TransModelBeamSeg(string dtlModelPath, int sectID)
        {
            //ToDo
            ModelBeamSeg modelBeamSeg = new ModelBeamSeg()
            {
                SectID = sectID,
                DbPath= dtlModelPath
            };
            int kind = modelBeamSeg.BeamSect.Kind;
            //根据Kind来判断
            return new ModelLineBeamSeg();
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
