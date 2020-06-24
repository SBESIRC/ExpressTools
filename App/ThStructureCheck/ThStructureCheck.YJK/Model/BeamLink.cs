using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Query;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.YJK.Model
{
    public class BeamLink
    {
        public BeamStatus Status { get; set; }
        public List<YjkEntityInfo> Start { get; set; }
        public List<YjkEntityInfo> End { get; set; }
        public List<YjkEntityInfo> Beams { get; set; }
        ///// <summary>
        ///// 先默认用Yjk数据库连接，后续根据需要判断
        ///// 从左到右
        ///// </summary>
        //private bool Forward
        //{
        //    get
        //    {
        //        return forward;
        //    }
        //}
        //private bool forward = true;
        private string dtlCalcPath = "";
        public BeamCalculationIndex GenerateBeamCalculationIndex 
            (string dtlCalcPath)
        {
            BeamCalculationIndex beamCalculationIndex = new BeamCalculationIndex();
            this.dtlCalcPath = dtlCalcPath;
            List<BeamCalculationIndex> beamCalIndexes = new List<BeamCalculationIndex>();
            this.Beams.ForEach(i => beamCalIndexes.Add(new BeamCalculationIndex(i, dtlCalcPath)));
            double asv0=CalculateAsv0();
            return beamCalculationIndex;
        }
        /// <summary>
        /// 计算梁段非加密区数值
        /// </summary>
        private double CalculateAsv0()
        {
            double asv0 = 0.0;
            if(this.Status!= BeamStatus.Primary)
            {
                return asv0;
            }
            double startAsv0=CalculateStartAsv0();
            double endAsv0 = CalculateEndAsv0();
            return Math.Max(startAsv0, endAsv0);
        }
        private double CalculateStartAsv0()
        {
            double asv0 = 0.0;
            if(this.Start.Count == 0)
            {
                return asv0;
            }
            Asv0Calculation asv0Calculation;
            if (this.Start[0] is ModelColumnSeg modelColumnSeg)
            {
                asv0Calculation = new Asv0Calculation(this.Beams[0], this.Start[0],this.dtlCalcPath);
            }
            else if (this.Start[0] is ModelWallSeg modelWallSeg)
            {

            }
            
            return asv0;
        }
        private double CalculateEndAsv0()
        {
            double asv0 = 0.0;
            if (this.End.Count > 0)
            {
                if (this.End[0] is ModelColumnSeg modelColumnSeg)
                {

                }
                else if (this.End[0] is ModelWallSeg modelWallSeg)
                {

                }
            }
            return asv0;
        }
    }
    public enum BeamStatus
    {
        Primary,
        Secondary,
        Unknown
    }
}
