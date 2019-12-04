using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class AllVerDirIronReinRatioBigThanFivePerRule:IRule
    {
        private AllVerDirIronReinRatioBigThanFivePerModel ruleModel;
        public AllVerDirIronReinRatioBigThanFivePerRule(AllVerDirIronReinRatioBigThanFivePerModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<ValidateResult> ValidateResults { get; set; } = new List<ValidateResult>();

        public void Validate()
        {
            if(ruleModel==null)
            {
                return;
            }
            if(ruleModel.B<=0 || ruleModel.H<=0)
            {
                return;
            }
            double allVerIronAreas = ruleModel.IntCBarCount * ruleModel.IntCBarDia +
                2 * (ruleModel.IntXBarCount * ruleModel.IntXBarDia + ruleModel.IntYBarCount * ruleModel.IntYBarDia);
            double reinforceRatio = (allVerIronAreas * 10e4) / (ruleModel.B * ruleModel.H);
            if(reinforceRatio>5)
            {
                ValidateResults.Add(ValidateResult.AllVerDirIronReinforceRatioBiggerThanFivePercent);
            }
        }
    }
}
