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
    /// 弧梁和方形柱
    /// </summary>
    class ArcBeamRecColumnAsv0 : Asv0Calculation
    {
        private ModelBeamSeg modelBeamSeg;
        private ModelColumnSeg modelColumnSeg;
        public ArcBeamRecColumnAsv0(ModelBeamSeg modelBeamSeg, ModelColumnSeg modelColumnSeg,string dtlCalcPath)
            :base(dtlCalcPath)
        {
            this.modelBeamSeg = modelBeamSeg;
            this.modelColumnSeg = modelColumnSeg;
        }
        public override void Calculate(List<ModelBeamSeg> beamSegs)
        {
            //ToDo
        }
    }
}
