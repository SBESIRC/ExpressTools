using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Service
{
    /// <summary>
    /// 直梁和L型柱
    /// </summary>
    class LineBeamLTypeColumnAsv0 :Asv0Calculation
    {
        private ModelLTypeColumnSeg modelLTypeColumnSeg;
        public LineBeamLTypeColumnAsv0(List<ModelBeamSeg> beamSegs, ModelLTypeColumnSeg modelLTypeColumnSeg,bool start, string dtlCalcPath)
            : base(beamSegs, modelLTypeColumnSeg, start,dtlCalcPath)
        {
            this.modelLTypeColumnSeg = modelLTypeColumnSeg;
        }

        public override void Calculate()
        {
           //ToDo
        }
    }
}
