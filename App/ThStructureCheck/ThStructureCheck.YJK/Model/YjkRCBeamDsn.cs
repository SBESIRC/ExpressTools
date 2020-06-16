using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class CalcRCBeamDsn : YjkEntityInfo
    {
        public string AsTop { get; set; }
        public string AsBtm { get; set; }
        public string Asv { get; set; }
        public string Ast1 { get; set; }
        public string Astt { get; set; }
        public string Asttc { get; set; }
        public string FrcAst { get; set; }
        public string FrcAsb { get; set; }
        public string FrcAsv { get; set; }
        /// <summary>
        /// 梁顶纵筋Asu
        /// </summary>
        public double BeamTopLongiReinAsu
        {
            get
            {
                double asu = 0.0;
                if (!string.IsNullOrEmpty(this.AsTop))
                {
                    string[] values = this.AsTop.Split(',');
                    double compareValue = 0.0;
                    foreach (string value in values)
                    {
                        compareValue = 0.0;
                        if (double.TryParse(value, out compareValue))
                        {
                            if (compareValue > asu)
                            {
                                asu = compareValue;
                            }
                        }
                    }
                }
                return asu;
            }
        }
        /// <summary>
        /// 梁底纵筋Asd
        /// </summary>
        public double BeamBottomLongiReinAsd
        {
            //表中该数值为9位数数组，取其中最大值。
            get
            {
                double asd = 0.0;
                if (!string.IsNullOrEmpty(this.AsBtm))
                {
                    string[] values = this.AsBtm.Split(',');
                    double compareValue = 0.0;
                    foreach (string value in values)
                    {
                        compareValue = 0.0;
                        if (double.TryParse(value, out compareValue))
                        {
                            if (compareValue > asd)
                            {
                                asd = compareValue;
                            }
                        }
                    }
                }
                return asd;
            }
        }
        /// <summary>
        /// 加密区箍筋Asv
        /// </summary>
        public double EncryptStirrupAsv
        {
            //表中该数值为9位数数组，取其中最大值
            get
            {
                double asv = 0.0;
                if (!string.IsNullOrEmpty(this.Asv))
                {
                    string[] values = this.Asv.Split(',');
                    double compareValue = 0.0;
                    foreach (string value in values)
                    {
                        compareValue = 0.0;
                        if (double.TryParse(value, out compareValue))
                        {
                            if (compareValue > asv)
                            {
                                asv = compareValue;
                            }
                        }
                    }
                }
                return asv;
            }
        }
        /// <summary>
        /// 抗扭箍筋 Ast1
        /// </summary>
        public double ResistTwistStirrupAst1
        {
            get
            {
                double ast1 = 0.0;
                if (!string.IsNullOrEmpty(this.Ast1))
                {
                    string[] values = this.Ast1.Split(',');
                    double compareValue = 0.0;
                    foreach (string value in values)
                    {
                        compareValue = 0.0;
                        if (double.TryParse(value, out compareValue))
                        {
                            if (compareValue > ast1)
                            {
                                ast1 = compareValue;
                            }
                        }
                    }
                }
                return ast1;
            }
        }

        /// <summary>
        /// 梁侧纵筋Ast
        /// </summary>
        public double BeamSideLongiReinforceAst
        {
            get
            {
                double ast = 0.0;
                if (!string.IsNullOrEmpty(this.Astt))
                {
                    string[] values = this.Astt.Split(',');
                    double compareValue = 0.0;
                    foreach (string value in values)
                    {
                        compareValue = 0.0;
                        if (double.TryParse(value, out compareValue))
                        {
                            if (compareValue > ast)
                            {
                                ast = compareValue;
                            }
                        }
                    }
                }
                return ast;
            }
        }
    }
}
