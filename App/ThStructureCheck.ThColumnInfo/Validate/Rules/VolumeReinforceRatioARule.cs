using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioARule : IRule
    {
        private VolumeReinforceRatioAModel vrra = null;
        private string rule = "（《砼规》11.4.17-2）";
        public VolumeReinforceRatioARule(VolumeReinforceRatioAModel volumeRRA)
        {
            this.vrra = volumeRRA;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double calVolumnReinforceRatio = 0.0;
        public void Validate()
        {
            if (this.vrra == null || !this.vrra.ValidateProperty())
            {
                return;
            }
            this.calVolumnReinforceRatio = this.vrra.Cdm.GetVolumeStirrupRatio(this.vrra.ProtectLayerThickness);

            if (calVolumnReinforceRatio < this.vrra.VolumnReinforceRatioLimited)
            {
                this.ValidateResults.Add("加密区箍筋体积配箍率不足 [" + calVolumnReinforceRatio + 
                    " < " + this.vrra.VolumnReinforceRatioLimited+"]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("加密区箍筋体积配箍率满足计算要求" + this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率A（箍筋）");
            steps.Add("条目编号：46， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：11.4.17-2，条文页数：P179");
            steps.Add("条文：对一、二、兰、四级抗震等级的柱，其箍筋加密区的箍筋体积配筋率分别不应小于0.8% 、" +
                "0. 6% 、0. 4%和0.4%;框支柱宜采用复合螺旋箍或井宇复合箍，其最小配箍特征值应按表11. 4. 17 中的数值增加0.02 采用，且体积配筋率不应小于1. 5%;");
            steps.Add("柱号 = " + this.vrra.Text);
            steps.Add("intXStirrupCount= " + (int)this.vrra.Cdm.IntXStirrupCount);
            steps.Add("intYStirrupCount= " + (int)this.vrra.Cdm.IntYStirrupCount);
            steps.Add("intStirrupDia= " + (int)this.vrra.Cdm.IntStirrupDia);
            steps.Add("intStirrupDiaArea= " + this.vrra.Cdm.IntStirrupDiaArea);
            steps.Add("cover = " + this.vrra.ProtectLayerThickness + "//保护层厚度");
            steps.Add("抗震等级 = " + this.vrra.AntiSeismicGrade);
            steps.Add(this.vrra.Cdm.GetVolumeStirrupRatioCalculation(this.vrra.ProtectLayerThickness) + this.calVolumnReinforceRatio);
            steps.Add("if (体积配箍率计算[" + calVolumnReinforceRatio + "] < 体积配筋率限值["+ this.vrra.VolumnReinforceRatioLimited+"])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋体积配箍率不足"+this.rule);
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 加密区箍筋体积配箍率满足计算要求" + this.rule);
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
