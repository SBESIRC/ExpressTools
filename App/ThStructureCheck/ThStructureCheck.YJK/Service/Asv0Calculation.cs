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
    public abstract class Asv0Calculation
    {
        protected string dtlCalcPath = "";
        private double asv0 = 0.0;
        public double Asv0 { get => this.asv0; set => this.asv0 = value; }
        /// <summary>
        /// 非加密区箍筋
        /// </summary>
        /// <param name="beamEntity"></param>
        /// <param name="linkEntity"></param>
        public Asv0Calculation(string dtlCalcPath)
        {
            this.dtlCalcPath = dtlCalcPath;
        }
        public abstract void Calculate(List<ModelBeamSeg> beamSegs);
    }
}
