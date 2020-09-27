using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ThColumnInfo.Validate
{
    public class ThConverter
    {
        /// <summary>
        /// 转换角筋的数量
        /// </summary>
        /// <param name="angularReinforcementSpec"></param>
        /// <returns></returns>
        public static int AngularReinforcementSpecToInt(string angularReinforcementSpec)
        {
            int num = -1;
            List<int> values = new List<int>();
            if (string.IsNullOrEmpty(angularReinforcementSpec))
            {
                return num;
            }
            MatchCollection mc = Regex.Matches(angularReinforcementSpec, @"\d+");
            foreach (var item in mc)
            {
                values.Add(Convert.ToInt32(item.ToString()));
            }
            if(values.Count>0)
            {
                num = values[0];
            }
            return num;
        }
        public static double StirrUpSpecToDouble(string stirrUpSpec)
        {
            double value = 0.0;
            List<double> values = new List<double>();
            if (string.IsNullOrEmpty(stirrUpSpec))
            {
                return value;
            }
            MatchCollection mc = Regex.Matches(stirrUpSpec, @"\d+");
            foreach (var item in mc)
            {
                values.Add(Convert.ToDouble(item.ToString()));
            }
            if (values.Count > 0)
            {
                value = values[0];
            }
            return value;
        }

        public static List<double> ReinforcementSpecToList(string reinforcementSpec)
        {
            List<double> values = new List<double>();
            string content = "";
            int index = ThColumnInfoUtils.IndexOfSpecialChar(reinforcementSpec, out content);
            if (index > 0)
            {
                double firstValue = Convert.ToDouble(reinforcementSpec.Substring(0, index));
                values.Add(firstValue);
                List<double> secondValues= ThColumnInfoUtils.GetDoubleValues(reinforcementSpec.Substring(index + content.Length));
                if (secondValues.Count > 0)
                {
                    values.Add(secondValues[0]);
                }
                else
                {
                    values.Add(0.0);
                }
            }
            else
            {
                byte[] buffers = Encoding.UTF32.GetBytes(reinforcementSpec);
                if (index < 0)
                {
                    for (int i = 0; i < buffers.Length; i++)
                    {
                        if (buffers[i] == 133 || buffers[i] == 132 || buffers[i] == 131 || buffers[i] == 130)
                        {
                            index = i;
                            break;
                        }
                    }
                }
                if (index > 0)
                {
                    //根据实际情况调试
                    double firstValue =Convert.ToDouble(Encoding.UTF32.GetString(buffers, 0,index));
                    double secondValue = Convert.ToDouble(Encoding.UTF32.GetString(buffers, index+4, buffers.Length-(index+4)));
                    values.Add(firstValue);
                    values.Add(secondValue);
                }
            }
            return values;
        }
        public static List<double> SplitSpec(string value)
        {
            List<double> values = new List<double>();
            if (string.IsNullOrEmpty(value))
            {
                return values;
            }
            MatchCollection mc = Regex.Matches(value, @"\d+");
            foreach (var item in mc)
            {
                values.Add(Convert.ToDouble(item.ToString()));
            }
            return values;
        }
        public static int AntiSeismicGradeStringToInt(string antiSeismicGrade)
        {
            int res=- 1;
            if(string.IsNullOrEmpty(antiSeismicGrade))
            {
                return res;
            }
            if(antiSeismicGrade.Contains("抗震"))
            { 
                if(antiSeismicGrade.Contains("十"))
                {
                    res = 10;
                }
                else if(antiSeismicGrade.Contains("九"))
                {
                    res = 9;
                }
                else if(antiSeismicGrade.Contains("八"))
                {
                    res = 8;
                }
                else if(antiSeismicGrade.Contains("七"))
                {
                    res = 7;
                }
                else if (antiSeismicGrade.Contains("六"))
                {
                    res = 6;
                }
                else if (antiSeismicGrade.Contains("五"))
                {
                    res = 5;
                }
                else if (antiSeismicGrade.Contains("四"))
                {
                    res = 4;
                }
                else if (antiSeismicGrade.Contains("三"))
                {
                    res = 3;
                }
                else if (antiSeismicGrade.Contains("二"))
                {
                    res = 2;
                }
                else if (antiSeismicGrade.Contains("一"))
                {
                    res = 1;
                }
            }
            return res;
        }
    }    
}
