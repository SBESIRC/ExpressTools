using System.Collections.Generic;

namespace ThColumnInfo.Validate.Rules
{
    public interface IRule
    {
       List<string> ValidateResults { get; set; }
       List<string> CorrectResults { get; set; }
       void Validate();
       List<string> GetCalculationSteps();
    }
}
