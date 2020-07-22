using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class StirrupMaximumSpacingJModel : ValidateModel
    {
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string Antiseismic { get; set; } = "";
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> {"KZ", "ZHZ" }))
            {
                return false;
            }
            return false;
        }
    }
}
