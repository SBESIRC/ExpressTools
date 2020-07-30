using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioCRule : IRule
    {
        private VolumeReinforceRatioCModel vrrc = null;
        public VolumeReinforceRatioCRule(VolumeReinforceRatioCModel volumeRRC)
        {
            this.vrrc = volumeRRC;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double calVolumnReinforceRatio = 0.0;

        public void Validate()
        {
            if (this.vrrc == null || !this.vrrc.ValidateProperty())
            {
                return;
            }
            this.calVolumnReinforceRatio = this.vrrc.Cdm.GetVolumeStirrupRatio(this.vrrc.ProtectLayerThickness);
            if (calVolumnReinforceRatio < this.vrrc.VolumnReinforceRatioLimited)
            {
                this.ValidateResults.Add("加密区箍筋体积配箍率小于计算书结果 ["+ 
                    calVolumnReinforceRatio + " < " + this.vrrc.VolumnReinforceRatioLimited+"]");
            }
            else
            {
                this.CorrectResults.Add("加密区箍筋体积配箍率满足计算书结果");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率C    （箍筋）");
            steps.Add("条目编号：518， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：配筋规则，条文页数：-");
            steps.Add("条文：实配钢筋应满足计算值");

            steps.Add("柱号 = " + this.vrrc.Text);
            steps.Add("intStirrupDia= " + (int)this.vrrc.Cdm.IntStirrupDia);
            steps.Add("intStirrupDiaArea= " + this.vrrc.Cdm.IntStirrupDiaArea);
            steps.Add("cover= " + this.vrrc.ProtectLayerThickness + "//保护层厚度");
            steps.Add(this.vrrc.Cdm.GetVolumeStirrupRatioCalculation(this.vrrc.ProtectLayerThickness) + this.calVolumnReinforceRatio);

            steps.Add("if (体积配箍率计算[" + this.calVolumnReinforceRatio + "] < 体积配筋率限值[" +
                this.vrrc.VolumnReinforceRatioLimited + "])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋体积配箍率小于计算书结果");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 加密区箍筋体积配箍率满足计算书结果");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
