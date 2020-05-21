using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupFullHeightEncryptionModel : ValidateModel
    {
        /// <summary>
        /// 剪跨比
        /// </summary>
        public double Jkb { get; set; }
        public ColumnDataModel Cdm { get; set; }
        public override bool ValidateProperty()
        {
            if (this.Code.Contains("KZ") || this.Code.Contains("ZHZ"))
            {
                return true;
            }
            return false;
        }
    }
}
