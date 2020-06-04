using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK
{
    public static class ThYjkDatbaseExtension
    {
        //用于程序中转
        public static string DtlModelPath = "";
        public static string DtlCalcPath = "";
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
        public static string GetStructureType(double paraValue)
        {
            string structureType = "";
            Dictionary<string, double> keyValues = GetStructureTypeDic();
            var value = keyValues.Where(i => i.Value == paraValue).Select(i => i.Key).FirstOrDefault();
            if (!string.IsNullOrEmpty(value))
            {
                structureType = value;
            }
            return structureType;
        }
        public static Dictionary<string, double> GetStructureTypeDic()
        {
            Dictionary<string, double> keyValues = new Dictionary<string, double>();
            keyValues.Add("框架结构", 10101);
            keyValues.Add("框剪结构", 10102);
            keyValues.Add("框筒结构", 10103);
            keyValues.Add("筒中筒结构", 10104);
            keyValues.Add("剪力墙结构", 10105);
            keyValues.Add("框支剪力墙结构", 10106);
            keyValues.Add("板柱-剪力墙结构", 10107);
            keyValues.Add("异形柱框架结构", 10108);
            keyValues.Add("异形柱框剪结构", 10109);
            keyValues.Add("配筋砌块砌体结构", 10110);
            keyValues.Add("砌体结构", 10111);
            keyValues.Add("底框结构", 10112);
            keyValues.Add("钢框架-中心支撑结构", 10113);
            keyValues.Add("钢框架-偏心支撑结构", 10114);
            keyValues.Add("单层工业厂房", 10115);
            keyValues.Add("多层工业厂房", 10116);
            return keyValues;
        }
        /// <summary>
        /// 获取设防烈度
        /// </summary>
        /// <param name="paraValue"></param>
        /// <param name="controlIndex">-1,0,1</param>
        /// <returns></returns>
        public static bool GetFortiCation(double paraValue, out double fortication)
        {
            bool findRes = false;
            fortication = 0.0;
            List<Tuple<double, double>> tuples = new List<Tuple<double, double>>();
            tuples.Add(new Tuple<double, double>(30201, 6));
            tuples.Add(new Tuple<double, double>(30202, 7));
            tuples.Add(new Tuple<double, double>(30203, 7));
            tuples.Add(new Tuple<double, double>(30204, 8));
            tuples.Add(new Tuple<double, double>(30205, 8));
            tuples.Add(new Tuple<double, double>(30206, 9));
            var res = tuples.Where(i => i.Item1 == paraValue).Select(i => i).ToList();
            if (res != null && res.Count > 0)
            {
                findRes = true;
                fortication = res.First().Item2;
            }
            return findRes;
        }        
    }
}
