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
    /// 弧梁和直墙
    /// </summary>
    class ArcBeamLineWallAsv0 : Asv0Calculation
    {
        private ModelWallSeg modelArcWallSeg;
        public ArcBeamLineWallAsv0(List<ModelBeamSeg> beamSegs, ModelArcWallSeg modelArcWallSeg, bool start, string dtlCalcPath)
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