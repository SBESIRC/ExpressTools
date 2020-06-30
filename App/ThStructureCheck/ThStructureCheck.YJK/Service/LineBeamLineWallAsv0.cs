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
    /// 直梁和直墙
    /// </summary>
    class LineBeamLineWallAsv0 : LineBeamRecColumnAsv0
    {
        private ModelWallSeg modelWallSeg;
        public LineBeamLineWallAsv0(List<ModelBeamSeg> beamSegs, ModelWallSeg modelWallSeg,bool start,string dtlCalcPath)
            :base(beamSegs, modelWallSeg.BuildGeometry(), start ,dtlCalcPath)
        {
            this.modelWallSeg = modelWallSeg;
            base.linkEnty = modelWallSeg;
        }
        public override void Calculate()
        {
            base.Calculate();
        }
    }
}
