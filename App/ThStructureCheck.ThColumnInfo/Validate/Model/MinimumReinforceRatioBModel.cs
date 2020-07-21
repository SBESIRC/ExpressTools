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
        /// 对IV类场地上较高的高层建筑，最小配筋百分率应增加0.1
        /// </summary>
        public double P1
        {
            get
            {
                return GetP1();
            }
        }
        /// <summary>
        /// 采用335Mpa、400Mpa纵向受力钢筋时，应分别增加0.1和0.05
        /// </summary>
        public double P2
        {
            get
            {
                return GetP2();
            }
        }
        /// <summary>
        /// 当混凝土等级为C60以上时，增加0.1
        /// </summary>
        public double P3
        {
            get
            {
                return GetP3();
            }
        }
        /// <summary>
        /// 获取纵筋符号
        /// </summary>
        public string LongitudinalReinforceSign
        {
            get
            {
                return GetLongitudinalReinforceSign();
            }
        }
        /// <summary>
        /// 对IV类场地上较高的高层建筑，最小单侧配筋率限值应增加0.1
        /// </summary>
        public double DblpsessminIncrement
        {
            get
            {
                return GetDblpsessminIncrement();
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
        /// <summary>
        /// 混凝土强度
        /// </summary>
        public string ConcreteStrength { get; set; }
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
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "KZ", "ZHZ" }))
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
        /// <summary>
        /// 对IV类场地上较高的高层建筑，最小配筋百分率应增加0.1
        /// </summary>
        /// <returns></returns>
        private double GetP1()
        {
            double res = 0.0;
            if(this.IsFourClassHigherArchitecture)
            {
                res = 0.1;
            }
            return res;
        }
        /// <summary>
        /// 采用335Mpa、400Mpa纵向受力钢筋时，应分别增加0.1和0.05
        /// </summary>
        /// <returns></returns>
        private double GetP2()
        {
            double value = 0.0;
            if (string.IsNullOrEmpty(Cdm.Ctri.BEdgeSideMiddleReinforcement))
            {
                return value;
            }
            byte[] buffers = Encoding.UTF32.GetBytes(Cdm.Ctri.BEdgeSideMiddleReinforcement);
            var res1 = buffers.Where(i => i == 132);
            if ((res1 != null && res1.Count() > 0)|| 
                Cdm.Ctri.BEdgeSideMiddleReinforcement.IndexOf("%%132")>=0)
            {
                value = 0.05;
            }
            var res3 = buffers.Where(i => i == 131);
            if ((res3 != null && res3.Count() > 0) ||
                 Cdm.Ctri.BEdgeSideMiddleReinforcement.IndexOf("%%131") >= 0)
            {
                value = 0.1;
            }
            return value;
        }
        /// <summary>
        /// 当混凝土等级为C60以上时，增加0.1
        /// </summary>
        /// <returns></returns>
        private double GetP3()
        {
            double value = 0.0;
            if (!string.IsNullOrEmpty(this.ConcreteStrength))
            {
                List<double> values = ThColumnInfoUtils.GetDoubleValues(this.ConcreteStrength);
                if (values.Count > 0)
                {
                    if (values[0] > 60)
                    {
                        value = 0.1;
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public double GetDblpsessminIncrement()
        {
            if(this.IsFourClassHigherArchitecture)
            {
                return 0.1;
            }
            else
            {
                return 0.0;
            }
        }
        public string GetLongitudinalReinforceSign()
        {
            return ThSpecificationValidate.paraSetInfo.
                GetLongitudinalReinforcementSign(Cdm.Ctri.BEdgeSideMiddleReinforcement);
        }
    }
}
