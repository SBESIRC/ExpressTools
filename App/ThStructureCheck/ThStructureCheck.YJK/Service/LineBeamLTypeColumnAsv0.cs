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
        private ModelBeamSeg modelBeamSeg;
        private ModelColumnSeg modelColumnSeg;
        public LineBeamLTypeColumnAsv0(ModelBeamSeg modelBeamSeg, ModelColumnSeg modelColumnSeg, string dtlCalcPath)
            : base(dtlCalcPath)
        {
            this.modelBeamSeg = modelBeamSeg;
            this.modelColumnSeg = modelColumnSeg;
        }

        public override void Calculate(List<ModelBeamSeg> beamSegs,bool start)
        {
           //ToDo
        }
    }
}
