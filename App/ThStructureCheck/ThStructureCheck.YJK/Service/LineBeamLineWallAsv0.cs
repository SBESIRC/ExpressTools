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
        private ModelLineWallSeg modelLineWallSeg;
        public LineBeamLineWallAsv0(List<ModelBeamSeg> beamSegs, ModelLineWallSeg modelLineWallSeg,bool start,string dtlCalcPath)
            :base(beamSegs, modelLineWallSeg.BuildGeometry(), start ,dtlCalcPath)
        {
            this.modelLineWallSeg = modelLineWallSeg;
            base.linkEnty = modelLineWallSeg;
        }
        public override void Calculate()
        {
            base.Calculate();
        }
    }
}
