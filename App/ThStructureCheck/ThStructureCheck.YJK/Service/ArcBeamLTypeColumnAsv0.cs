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
    /// 弧梁和L形柱
    /// </summary>
    class ArcBeamLTypeColumnAsv0 : Asv0Calculation
    {
        private ModelColumnSeg modelColumnSeg;
        public ArcBeamLTypeColumnAsv0(List<ModelBeamSeg> beamSegs, ModelColumnSeg modelColumnSeg, bool start, string dtlCalcPath)
            :base(beamSegs, modelColumnSeg, start,dtlCalcPath)
        {
            this.modelColumnSeg = modelColumnSeg;
        }
        public override void Calculate()
        {
            //ToDo
        }
    }
}
