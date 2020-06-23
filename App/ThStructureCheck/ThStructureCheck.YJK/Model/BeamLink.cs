using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class BeamLink
    {
        public BeamStatus Status { get; set; }
        public List<YjkEntityInfo> Start { get; set; }
        public List<YjkEntityInfo> End { get; set; }
        public List<YjkEntityInfo> Beams { get; set; }
        /// <summary>
        /// 先默认用Yjk数据库连接，后续根据需要判断
        /// 从左到右
        /// </summary>
        private bool Forward
        {
            get
            {
                return forward;
            }
        }
        private BeamCalculationIndex beamCalculationIndex =new BeamCalculationIndex();
        public BeamCalculationIndex BeamCalcIndex
        {
            get
            {
                return beamCalculationIndex;
            }
        }
        private bool forward = true;
        public void GenerateBeamCalculationIndex 
            (string dtlCalcPath)
        {           
            List<BeamCalculationIndex> beamCalIndexes = new List<BeamCalculationIndex>();
            this.Beams.ForEach(i => beamCalIndexes.Add(new BeamCalculationIndex(i, dtlCalcPath)));
            
        }   
        private void CalculateAsv0()
        {
            if(this.Status!= BeamStatus.Primary)
            {
                return;
            }
            double startAsv0=CalculateStartAsv0();
            double endAsv0 = CalculateEndAsv0();
            this.beamCalculationIndex.Asv0 = Math.Max(startAsv0, endAsv0);
        }
        private double CalculateStartAsv0()
        {
            double asv0 = 0.0;
            if(this.Start.Count == 0)
            {
                return asv0;
            }
            if (this.Start[0] is ModelColumnSeg modelColumnSeg)
            {

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
