using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    /// <summary>
    /// 轴压比(轴压比)
    /// </summary>
    public class AxialCompressionRatioRule : IRule
    {
        private AxialCompressionRatioModel axialCompressionRatioModel;
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "（《砼规》11.4.16）";
        public AxialCompressionRatioRule(AxialCompressionRatioModel axialCompressionRatioModel)
        {
            this.axialCompressionRatioModel = axialCompressionRatioModel;
        }
        public void Validate()
        {
            if (axialCompressionRatioModel == null || !axialCompressionRatioModel.ValidateProperty())
            {
                return;
            }
            if(axialCompressionRatioModel.IsNonAntiseismic)
            {
                return;
            }
            if (axialCompressionRatioModel.AxialCompressionRatio > 
                axialCompressionRatioModel.AxialCompressionRatioLimited)
            {
                this.ValidateResults.Add("轴压比超限 ["+ axialCompressionRatioModel.AxialCompressionRatio+" > "+
                    axialCompressionRatioModel.AxialCompressionRatioLimited+"]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("轴压比满足要求"+this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            if (axialCompressionRatioModel.IsNonAntiseismic)
            {
                return steps;
            }
            steps.Add("类别：轴压比（轴压比）");
            steps.Add("条目编号：21， 强制性：宜，适用构件：KZ、ZHZ");
            steps.Add("适用功能：计算书校核，条文编号：砼规 砼规 11.4.16，条文页数：177");
            steps.Add("条文：一、二、三、四级抗震等级的各类结构的框架柱、框支柱，其轴压比不宜大予表11.4.16 规定的限值。对凹类场地上较高的高层建筑，柱轴压比限值应适当减小。");
            steps.Add("柱号 = " + this.axialCompressionRatioModel.Text);
            steps.Add("if (轴压比[" + axialCompressionRatioModel.AxialCompressionRatio + "] > 轴压比限值[" +
                axialCompressionRatioModel.AxialCompressionRatioLimited+"])");
            steps.Add("  {");
            steps.Add("    Err: 轴压比大于计算书限值（《砼规》11.4.16）");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("    Debugprint: 轴压比满足计算书限值（《砼规》11.4.16）");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
