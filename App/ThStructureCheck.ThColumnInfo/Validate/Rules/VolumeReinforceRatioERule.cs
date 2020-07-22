using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioERule : IRule
    {
        private VolumeReinforceRatioEModel vrre = null;
        private string rule = "（《砼规》 11.6.8）";
        public VolumeReinforceRatioERule(VolumeReinforceRatioEModel volumeRRE)
        {
            this.vrre = volumeRRE;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double calVolumnReinforceRatio = 0.0;

        public void Validate()
        {
            if (this.vrre == null || this.vrre.ValidateProperty() == false)
            {
                return;
            }
            this.calVolumnReinforceRatio = GetVolumeStirrupRatio();
            if (!this.vrre.AntiSeismicGrade.Contains("特"))
            {
                if (this.vrre.AntiSeismicGrade.Contains("一"))
                {
                    if(this.calVolumnReinforceRatio<0.006)
                    {
                        this.ValidateResults.Add("体积配箍率不宜小于0.6% " + this.rule);
                    }
                }
                if (this.vrre.AntiSeismicGrade.Contains("二"))
                {
                    if (this.calVolumnReinforceRatio < 0.005)
                    {
                        this.ValidateResults.Add("体积配箍率不宜小于0.5% " + this.rule);
                    }
                }
                if (this.vrre.AntiSeismicGrade.Contains("三"))
                {
                    if (this.calVolumnReinforceRatio < 0.004)
                    {
                        this.ValidateResults.Add("体积配箍率不宜小于0.4% " + this.rule);
                    }
                }
            }
        }
        private double GetVolumeStirrupRatio()
        {
            var coluJoinCore = this.vrre.ColuJoinCoreData;
            if(coluJoinCore==null)
            {
                return 0.0;
            }
            return this.vrre.Cdm.GetCoreVolumeStirrupRatio(coluJoinCore, this.vrre.ProtectLayerThickness);
        }
        private string GetVolumeStirrupRatioCalculation()
        {
            var coluJoinCore = this.vrre.ColuJoinCoreData;
            if (coluJoinCore == null)
            {
                return "";
            }
            return this.vrre.Cdm.GetCoreVolumeStirrupRatioCalculation(coluJoinCore, this.vrre.ProtectLayerThickness);
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：节点核心区体积配箍率 （箍筋）");
            steps.Add("强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("条文：实配钢筋应满足计算值");
            steps.Add("柱号 = " + this.vrre.Text);
            steps.Add("cover= " + this.vrre.ProtectLayerThickness + "//保护层厚度");
            steps.Add(GetVolumeStirrupRatioCalculation() + this.calVolumnReinforceRatio);
            steps.Add("if (抗震等级[" + this.vrre.AntiSeismicGrade + "].Contains(\"一级\"))");
            steps.Add("  {");
            steps.Add("    if(核芯区体积配箍率["+ this.calVolumnReinforceRatio+"] < 0.006)");
            steps.Add("      {");
            steps.Add("         Err: 体积配箍率不宜小于0.6% "+this.rule);
            steps.Add("      }");
            steps.Add("  }");

            steps.Add("if (抗震等级[" + this.vrre.AntiSeismicGrade + "].Contains(\"二级\"))");
            steps.Add("  {");
            steps.Add("    if(核芯区体积配箍率[" + this.calVolumnReinforceRatio + "] < 0.005)");
            steps.Add("      {");
            steps.Add("         Err: 体积配箍率不宜小于0.5% " + this.rule);
            steps.Add("      }");
            steps.Add("  }");

            steps.Add("if (抗震等级[" + this.vrre.AntiSeismicGrade + "].Contains(\"三级\"))");
            steps.Add("  {");
            steps.Add("    if(核芯区体积配箍率[" + this.calVolumnReinforceRatio + "] < 0.004)");
            steps.Add("      {");
            steps.Add("         Err: 体积配箍率不宜小于0.4% " + this.rule);
            steps.Add("      }");
            steps.Add("  }");
            return steps;
        }
    }
}
