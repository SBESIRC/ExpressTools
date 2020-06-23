using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Service
{
    public class YjkTool
    {
        /// <summary>
        /// 获取抗震等级
        /// </summary>
        /// <param name="paraValue"></param>
        /// <param name="controlIndex">-1,0,1</param>
        /// <returns></returns>
        public static string GetAntiSeismicGrade(double paraValue, double controlIndex = 0.0)
        {
            string antiSeismicGrade = "";
            if (controlIndex != 0.0 && controlIndex != -1.0 && controlIndex != 1.0)
            {
                controlIndex = 0.0;
            }
            if (paraValue == 70101 && controlIndex == 1.0)
            {
                paraValue = 70101;
            }
            else if (paraValue == 70106 && controlIndex == -1.0)
            {
                paraValue = 70106;
            }
            else
            {
                paraValue += controlIndex * -1;
            }
            Dictionary<string, double> keyValues = new Dictionary<string, double>();
            keyValues.Add("特一级", 70101);
            keyValues.Add("一级", 70102);
            keyValues.Add("二级", 70103);
            keyValues.Add("三级", 70104);
            keyValues.Add("四级", 70105);
            keyValues.Add("非抗震", 70106);

            var value = keyValues.Where(i => i.Value == paraValue).Select(i => i.Key).First();
            if (!string.IsNullOrEmpty(value))
            {
                antiSeismicGrade = value;
            }
            return antiSeismicGrade;
        }
    }
}
