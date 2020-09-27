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
    /// 直梁和弧墙
    /// </summary>
    class LineBeamArcWallAsv0 : Asv0Calculation
    {
        private ModelArcWallSeg modelArcWallSeg;
        public LineBeamArcWallAsv0(List<ModelBeamSeg> beamSegs, ModelArcWallSeg modelArcWallSeg, bool start,string dtlCalcPath)
            :base(beamSegs, modelArcWallSeg, start,dtlCalcPath)
        {
            this.modelArcWallSeg = modelArcWallSeg;
        }

        public override void Calculate()
        {
            //ToDo
        }
    }
}
