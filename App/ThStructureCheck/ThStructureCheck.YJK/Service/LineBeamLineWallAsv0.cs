﻿using System;
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
        private ModelBeamSeg modelBeamSeg;
        private ModelWallSeg modelWallSeg;
        public LineBeamLineWallAsv0(ModelBeamSeg modelBeamSeg, ModelWallSeg modelWallSeg,string dtlCalcPath)
            :base(modelBeamSeg, modelWallSeg.BuildGeometry(), dtlCalcPath)
        {
            this.modelBeamSeg = modelBeamSeg;
            this.modelWallSeg = modelWallSeg;
        }

        public override void Calculate(List<ModelBeamSeg> beamSegs)
        {
            base.Calculate(beamSegs);
        }
    }
}
