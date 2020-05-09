using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterBModel :ValidateModel
    {
        public ColumnDataModel Cdm { get; set; }
        public override bool ValidateProperty()
        {
            if (this.Code.Contains("LZ") || this.Code.Contains("KZ") || this.Code.Contains("ZHZ"))
            {
                return true;
            }
            return false;
        }
    }
}
