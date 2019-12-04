using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class AxialCompressionRatioRule : IRule
    {
        private AxialCompressionRatioModel axialCompressionRatioModel;
        public List<ValidateResult> ValidateResults { get; set; } = new List<ValidateResult>();
        public AxialCompressionRatioRule(AxialCompressionRatioModel axialCompressionRatioModel)
        {
            this.axialCompressionRatioModel = axialCompressionRatioModel;
        }
        public void Validate()
        {
            if (axialCompressionRatioModel == null)
            {
                return;
            }
            if (axialCompressionRatioModel.AxialCompressionRatio > 
                axialCompressionRatioModel.AxialCompressionRatioLimited)
            {
                this.ValidateResults.Add(ValidateResult.AxialCompressionRatioTransfinite);
            }
        }
    }
}
