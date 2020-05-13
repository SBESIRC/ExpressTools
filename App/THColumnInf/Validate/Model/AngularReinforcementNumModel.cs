
namespace ThColumnInfo.Validate
{
    public class AngularReinforcementNumModel : ValidateModel
    {
        public int AngularReinforcementNum { get; set; }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ") || this.Code.Contains("LZ")))
            {
                return false;
            }
            if (AngularReinforcementNum<=0)
            {
                return false;
            }
            return true;
        }
    }
}
