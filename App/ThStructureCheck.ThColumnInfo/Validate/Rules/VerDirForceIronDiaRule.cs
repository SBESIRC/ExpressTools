using System;
using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    /// <summary>
    /// 纵向钢筋直径最小值
    /// </summary>
    public class VerDirForceIronDiaRule : IRule
    {
        private VerDirForceIronModel verDirForceIronModel;
        private string rule = "（《砼规》9.3.1-1）";
        public VerDirForceIronDiaRule(VerDirForceIronModel verDirForceIronModel)
        {
            this.verDirForceIronModel = verDirForceIronModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if (verDirForceIronModel == null || !verDirForceIronModel.ValidateProperty())
            {
                return;
            }
            //纵向受力钢筋直径不宜小于12mm
            double minValue = Math.Min(verDirForceIronModel.Cdm.IntXBarDia, verDirForceIronModel.Cdm.IntYBarDia);
            minValue = Math.Min(minValue, verDirForceIronModel.Cdm.IntCBarDia);
            if(minValue<12)
            {
                ValidateResults.Add("纵向受力钢筋直径小于12mm ["+ minValue+" < 12]，"+this.rule);
            }
            else
            {
                CorrectResults.Add("纵向受力钢筋直径不小于12mm" + this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            double intBardiamin = Math.Min(verDirForceIronModel.Cdm.IntXBarDia,
                verDirForceIronModel.Cdm.IntYBarDia);
            intBardiamin = Math.Min(verDirForceIronModel.Cdm.IntCBarDia, intBardiamin);
            steps.Add("类别：纵向钢筋直径最小值（侧面纵筋）");
            steps.Add("条目编号：41， 强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.1-1，条文页数：P123");
            steps.Add("条文：纵向受力钢筋直径不宜小于12mm");
            steps.Add("柱号 = " + this.verDirForceIronModel.Text);
            steps.Add("intBardiamin=Math.Min(IntCBarDia[" + verDirForceIronModel.Cdm.IntCBarDia + "],IntXBarDia[" + 
                verDirForceIronModel.Cdm.IntXBarDia +"],IntYBarDia[" + verDirForceIronModel.Cdm.IntYBarDia + "]) =" + intBardiamin);
            steps.Add("if (intBardiamin[" + intBardiamin + "] < 12)");
            steps.Add("  {");
            steps.Add("     Err: 纵向受力钢筋直径小于12mm（《砼规》9.3.1-1）");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint: 纵向受力钢筋直径不小于12mm（《砼规》9.3.1-1）");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
