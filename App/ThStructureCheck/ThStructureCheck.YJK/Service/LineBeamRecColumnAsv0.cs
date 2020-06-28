using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Service
{
    /// <summary>
    /// 直梁和方型柱
    /// </summary>
    class LineBeamRecColumnAsv0 : Asv0Calculation
    {
        private ModelBeamSeg modelBeamSeg;
        private ModelColumnSeg modelColumnSeg;
        public LineBeamRecColumnAsv0(ModelBeamSeg modelBeamSeg, ModelColumnSeg modelColumnSeg,string dtlCalcPath)
            :base(dtlCalcPath)
        {
            this.modelBeamSeg = modelBeamSeg;
            this.modelColumnSeg = modelColumnSeg;
        }

        public override void Calculate(List<ModelBeamSeg> modelBeamSegs)
        {
            IEntity beamGeo = this.modelBeamSeg.BuildGeometry();
            IEntity columnGeo = this.modelColumnSeg.BuildGeometry();
            double dis = GeometricCalculation.GetInsertBeamDis(columnGeo, beamGeo);
            int floorNo;
            int beamSegNo;          
            bool res = new YjkBeamQuery(modelBeamSeg.DbPath).GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSeg.ID, out floorNo, out beamSegNo);
            int beamCalcId = new YjkBeamQuery(base.dtlCalcPath).GetTblBeamSegIDFromDtlCalc(floorNo, beamSegNo);


        }
    }
}
