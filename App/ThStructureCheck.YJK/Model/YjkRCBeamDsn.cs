using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class CalcRCBeamDsn : YjkEntityInfo
    {
        /// <summary>
        /// 上部梁顶纵筋
        /// </summary>
        public string AsTop { get; set; }
        /// <summary>
        /// 底部梁底纵筋
        /// </summary>
        public string AsBtm { get; set; }
        /// <summary>
        /// 加密区箍筋
        /// </summary>
        public string Asv { get; set; }
        /// <summary>
        /// 抗扭箍筋
        /// </summary>
        public string Ast1 { get; set; }
        /// <summary>
        /// 梁侧纵筋
        /// </summary>
        public string Astt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Asttc { get; set; }
        public string FrcAst { get; set; }
        public string FrcAsb { get; set; }
        public string FrcAsv { get; set; }
        /// <summary>
        /// 把加密区箍筋字符串转成double集合
        /// </summary>
        public List<double> AsvCollection
        {
            get
            {
                List<double> datas = new List<double>();
                if(!string.IsNullOrEmpty(this.Asv))
                {
                    string[] values = this.Asv.Split(',');
                    foreach (string value in values)
                    {
                        datas.Add(Convert.ToDouble(value));
                    }
                }
                return datas;
            }
        }
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
        /// 左侧梁顶纵筋
        /// </summary>
        public double LeftAsu
        {
            get
            {
                double leftAsu = 0.0;
                if (!string.IsNullOrEmpty(this.AsTop))
                {
                    string[] values = this.AsTop.Split(',');
                    if(values.Length>0)
                    {
                        if (!double.TryParse(values[0], out leftAsu))
                        {
                            leftAsu = 0.0;
                        }
                    }
                }
                return leftAsu;
            }
        }
        /// <summary>
        /// 左侧梁顶纵筋
        /// </summary>
        public double RightAsu
        {
            get
            {
                double rightAsu = 0.0;
                if (!string.IsNullOrEmpty(this.AsTop))
                {
                    string[] values = this.AsTop.Split(',');
                    if (values.Length > 0)
                    {
                        if (!double.TryParse(values[values.Length-1], out rightAsu))
                        {
                            rightAsu = 0.0;
                        }
                    }
                }
                return rightAsu;
            }
        }
        /// <summary>
        /// 梁底纵筋Asd (下部通长钢筋)
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
