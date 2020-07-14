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
    class LineBeamLineBeamAsv0 : LineBeamRecColumnAsv0
    {
        private ModelLineBeamSeg modelLineBeamSeg;
        public LineBeamLineBeamAsv0(List<ModelBeamSeg> beamSegs, ModelLineBeamSeg modelLineBeamSeg,bool start,string dtlCalcPath)
            :base(beamSegs, modelLineBeamSeg.BuildGeometry(), start ,dtlCalcPath)
        {
            this.modelLineBeamSeg = modelLineBeamSeg;
            base.linkEnty = modelLineBeamSeg;
        }
        public override void Calculate()
        {
            base.Calculate();
        }
    }
}
