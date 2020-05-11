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
        public double Dblsespmin
        {
            get
            {
                return GetDblsespmin();
            }
        }
        /// <summary>
        /// 是否为IV类场地较高建筑
        /// </summary>
        public bool IsFourClassHigherArchitecture { get; set; }
        /// <summary>
        /// 最小单侧配筋率限值
        /// </summary>
        public double Dblpsessmin { get; set; } = 0.2;

        public ColumnDataModel Cdm { get; set; }
        /// <summary>
        /// 混凝土强度
        /// </summary>
        public string ConcreteStrength { get; set; }
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade { get; set; }
        /// <summary>
        /// 结构类型
        /// </summary>
        public string StructureType { get; set; }
        /// <summary>
        /// 是否角柱
        /// </summary>
        public bool IsCornerColumn { get; set; }
        /// <summary>
        /// 柱子类型
        /// </summary>
        public string ColumnType
        {
            get
            {
                return GetColumnType();
            }
        }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ")))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取柱子类型
        /// </summary>
        /// <returns></returns>
        private string GetColumnType()
        {
            //柱子类型
            string columnType = "";
            if (this.IsCornerColumn)
            {
                columnType = "角柱";
            }
            else if (this.Code.ToUpper().Contains("ZHZ"))
            {
                columnType = "框支柱";
            }
            else
            {
                columnType = "中柱"; //不是角柱或框支柱的都认为是中柱和边柱
            }
            return columnType;
        }
        /// <summary>
        /// 最小全截面配筋率限值
        /// </summary>
        /// <returns></returns>
        private double GetDblsespmin()
        {
            double dblsespmin = ThValidate.GetIronMinimumReinforcementPercent(
                this.AntiSeismicGrade, this.ColumnType, this.StructureType);
            return dblsespmin;
        }
    }
}
