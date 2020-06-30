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
        private ModelColumnSeg modelColumnSeg;
        public LineBeamLTypeColumnAsv0(List<ModelBeamSeg> beamSegs, ModelColumnSeg modelColumnSeg,bool start, string dtlCalcPath)
            : base(beamSegs, modelColumnSeg, start,dtlCalcPath)
        {
            this.modelColumnSeg = modelColumnSeg;
        }

        public override void Calculate()
        {
           //ToDo
        }
    }
}
