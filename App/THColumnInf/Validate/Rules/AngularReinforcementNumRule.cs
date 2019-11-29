using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class AngularReinforcementNumRule : IRule
    {
        private AngularReinforcementNumModel ruleModel;
        public AngularReinforcementNumRule(AngularReinforcementNumModel ruleModel)
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
            if(ruleModel.AngularReinforcementNum%4!=0)
            {
                ValidateResults.Add(ValidateResult.AngularReinforcementNumFourTimes);
            }
        }
    }
}
