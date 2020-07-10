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
        private string dtlCalcPath = "";
        private BeamCalculationIndex beamCalIndex;
        public BeamCalculationIndex BeamCalIndex => beamCalIndex;
        /// <summary>
        /// 生成梁段的配筋信息
        /// </summary>
        /// <param name="dtlCalcPath"></param>
        public void GenerateBeamCalculationIndex 
            (string dtlCalcPath)
        {
            this.beamCalIndex = new BeamCalculationIndex();
            this.dtlCalcPath = dtlCalcPath;
            List<BeamCalculationIndex> beamCalIndexes = new List<BeamCalculationIndex>();
            this.Beams.ForEach(i => beamCalIndexes.Add(new BeamCalculationIndex(i, dtlCalcPath)));
            if(beamCalIndexes.Count==1)
            {
                this.beamCalIndex = beamCalIndexes[0];
            }
            else
            {
                foreach(var calculateIndex in beamCalIndexes)
                {
                    if(calculateIndex.Asv> this.beamCalIndex.Asv)
                    {
                        this.beamCalIndex.Asv = calculateIndex.Asv;
                    }
                    if (calculateIndex.Asv0 > this.beamCalIndex.Asv0)
                    {
                        this.beamCalIndex.Asv0 = calculateIndex.Asv0;
                    }
                    if (calculateIndex.LeftAsu> this.beamCalIndex.LeftAsu)
                    {
                        this.beamCalIndex.LeftAsu = calculateIndex.LeftAsu;
                    }
                    if(calculateIndex.RightAsu> this.beamCalIndex.RightAsu)
                    {
                        this.beamCalIndex.RightAsu = calculateIndex.RightAsu;
                    }
                    if(calculateIndex.Ast1> this.beamCalIndex.Ast1)
                    {
                        this.beamCalIndex.Ast1 = calculateIndex.Ast1;
                    }
                    if (calculateIndex.Ast > this.beamCalIndex.Ast)
                    {
                        this.beamCalIndex.Ast = calculateIndex.Ast;
                    }
                    if (calculateIndex.Asd > this.beamCalIndex.Asd)
                    {
                        this.beamCalIndex.Asd = calculateIndex.Asd;
                    }                    
                }
                this.beamCalIndex.Spec = beamCalIndexes[0].Spec;
            }
        }
    }
    public enum BeamStatus
    {
        /// <summary>
        /// 主梁
        /// </summary>
        Primary,
        /// <summary>
        /// 次梁
        /// </summary>
        Secondary,
        /// <summary>
        /// 半主梁
        /// </summary>
        Half,
        /// <summary>
        /// 悬挑梁
        /// </summary>
        Cantilever,
        Unknown
    }
}
