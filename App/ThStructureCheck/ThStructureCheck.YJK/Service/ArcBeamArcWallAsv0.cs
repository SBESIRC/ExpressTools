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
    /// 弧梁和弧墙
    /// </summary>
    class ArcBeamArcWallAsv0 : Asv0Calculation
    {
        private ModelBeamSeg modelBeamSeg;
        private ModelWallSeg modelWallSeg;
        public ArcBeamArcWallAsv0(ModelBeamSeg modelBeamSeg, ModelWallSeg modelWallSeg,string dtlCalcPath)
            :base(dtlCalcPath)
        {
            this.modelBeamSeg = modelBeamSeg;
            this.modelWallSeg = modelWallSeg;
        }

        public override void Calculate(List<ModelBeamSeg> beamSegs,bool start)
        {
            //ToDo
        }
    }
}
