using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class MinimumReinforceRatioBModel : ValidateModel
    {
        /// <summary>
        /// 最小全截面配筋率限值
        /// </summary>
        public double Dblsespmin { get; set; }
        /// <summary>
        /// 是否为IV类场地较高建筑
        /// </summary>
        public bool IsFourClassHigherArchitecture { get; set; }
        /// <summary>
        /// 最小单侧配筋率限值
        /// </summary>
        public double Dblpsessmin { get; set; } = 0.2;

        public ColumnDataModel Cdm { get; set; }
        
        public string ConcreteStrength { get; set; }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ")))
            {
                return false;
            }
            return true;
        }
    }
}
