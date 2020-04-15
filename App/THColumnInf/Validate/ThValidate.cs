using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class ThValidate
    {
        public static IDataSource dataSource;
        public static List<ColumnDataModel> columnDataModels;
        public ThValidate(IDataSource ds)
        {
            dataSource = ds;
            columnDataModels = new List<ColumnDataModel>();
            dataSource.ColumnTableRecordInfos.ForEach(i => columnDataModels.Add(new ColumnDataModel(i)));
        }
        public static double GetIronMinimumReinforcementPercent(string antiseismicGrade, string columnType,string structureType="")
        {
            double value = 0.0;
            if(columnType.Contains("中柱") || columnType.Contains("边柱"))
            {
                if(structureType.Contains("框架结构"))
                {
                    if(antiseismicGrade.Contains("一级"))
                    {
                        value = 1.0;
                    }
                    else if (antiseismicGrade.Contains("二级"))
                    {
                        value = 0.8;
                    }
                    else if (antiseismicGrade.Contains("三级"))
                    {
                        value = 0.7;
                    }
                    else if (antiseismicGrade.Contains("四级"))
                    {
                        value = 0.6;
                    }
                }
                else
                {
                    if (antiseismicGrade.Contains("一级"))
                    {
                        value = 0.9;
                    }
                    else if (antiseismicGrade.Contains("二级"))
                    {
                        value = 0.7;
                    }
                    else if (antiseismicGrade.Contains("三级"))
                    {
                        value = 0.6;
                    }
                    else if (antiseismicGrade.Contains("四级"))
                    {
                        value = 0.5;
                    }
                }
            }
            else if(columnType.Contains("角柱") || columnType.Contains("框支柱"))
            {
                if (antiseismicGrade.Contains("一级"))
                {
                    value = 1.1;
                }
                else if (antiseismicGrade.Contains("二级"))
                {
                    value = 0.9;
                }
                else if (antiseismicGrade.Contains("三级"))
                {
                    value = 0.8;
                }
                else if (antiseismicGrade.Contains("四级"))
                {
                    value = 0.7;
                }
            }
            return value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paraValue"></param>
        /// <param name="controlIndex">-1,0,1</param>
        /// <returns></returns>
        public static string GetAntiSeismicGrade(double paraValue,double controlIndex=0.0)
        {
            string antiSeismicGrade = "";
            if(controlIndex!=0.0 && controlIndex != -1.0 && controlIndex != 1.0)
            {
                controlIndex = 0.0;
            }
            if(paraValue== 70101 && controlIndex==1.0)
            {
                paraValue = 70101;
            }
            else if(paraValue == 70106 && controlIndex == -1.0)
            {
                paraValue = 70106;
            }
            else
            {
                paraValue += controlIndex*-1;
            }
            Dictionary<string, double> keyValues = new Dictionary<string, double>();
            keyValues.Add("特一级", 70101);
            keyValues.Add("一级", 70102);
            keyValues.Add("二级", 70103);
            keyValues.Add("三级", 70104);
            keyValues.Add("四级", 70105);
            keyValues.Add("非抗震", 70106);

            var value= keyValues.Where(i => i.Value == paraValue).Select(i => i.Key).First();
            if(!string.IsNullOrEmpty(value))
            {
                antiSeismicGrade = value;
            }
            return antiSeismicGrade;
        }
        public static Dictionary<string,double> GetStructureTypeDic()
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
        public static string GetStructureType(double paraValue)
        {
            string structureType = "";
            Dictionary<string, double> keyValues = GetStructureTypeDic();
            var value=keyValues.Where(i => i.Value == paraValue).Select(i => i.Key).FirstOrDefault();
            if(!string.IsNullOrEmpty(value))
            {
                structureType = value;
            }
            return structureType;
        }
        /// <summary>
        /// 获取箍筋最小直径（表11.4.12-2）
        /// </summary>
        /// <param name="antiSeismic"></param>
        /// <param name="isGroundFloor"></param>
        /// <returns></returns>
        public static double GetStirrupMinimumDiameter(string antiSeismic,bool isGroundFloor)
        {
            double limitedValue = 0.0;
            if(string.IsNullOrEmpty(antiSeismic))
            {
                return limitedValue;
            }
            if(antiSeismic.Contains("特") && antiSeismic.Contains("一级"))
            {
                return limitedValue;
            }
            if(antiSeismic.Contains("一级"))
            {
                return 10.0;
            }
            if(antiSeismic.Contains("二级") || antiSeismic.Contains("三级"))
            {
                return 8.0;
            }
            if (antiSeismic.Contains("四级") )
            {
                if (isGroundFloor)
                {
                    return 8.0;
                }
                else
                {
                    return 6.0;
                }
            }
            return limitedValue;
        }
        /// <summary>
        /// 获取箍筋最大间距（表11.4.12-2）
        /// </summary>
        /// <param name="antiSeismic"></param>
        /// <param name="isGroundFloor"></param>
        /// <param name="longitudinalIronDia"></param>
        /// <returns></returns>
        public static double GetStirrupMaximumDiameter(string antiSeismic, bool isGroundFloor,double longitudinalIronDia)
        {
            double spaceing = 0.0;
            if (string.IsNullOrEmpty(antiSeismic))
            {
                return spaceing;
            }
            if (antiSeismic.Contains("特") && antiSeismic.Contains("一级"))
            {
                return spaceing;
            }
            if (antiSeismic.Contains("一级"))
            {
                return Math.Min(6* longitudinalIronDia,100);
            }
            if (antiSeismic.Contains("二级"))
            {
                return Math.Min(8 * longitudinalIronDia, 100);
            }
            if (antiSeismic.Contains("三级") || antiSeismic.Contains("四级"))
            {
                if(isGroundFloor)
                {
                    return Math.Min(8 * longitudinalIronDia, 100);
                }
                else
                {
                    return Math.Min(8 * longitudinalIronDia, 150);
                }               
            }            
            return spaceing;
        }
        /// <summary>
        /// 获取 体积配筋率限值
        /// </summary>
        /// <param name="code"></param>
        /// <param name="antiSeismic"></param>
        /// <returns></returns>

        public static double GetVolumeReinforcementRatioLimited(string code,string antiSeismic)
        {
            double limitedValue = 0.0;
            if (string.IsNullOrEmpty(antiSeismic) || string.IsNullOrEmpty(code))
            {
                return limitedValue;
            }
            if (antiSeismic.Contains("特") && antiSeismic.Contains("一级"))
            {
                return limitedValue;
            }
            if(code.ToUpper().Contains("KZ"))
            {
                if (antiSeismic.Contains("一级") && !antiSeismic.Contains("特"))
                {
                    return 0.008;
                }
                if (antiSeismic.Contains("二级"))
                {
                    return 0.006;
                }
                if (antiSeismic.Contains("三级") || antiSeismic.Contains("四级"))
                {
                    return 0.004;
                }
            }
            if(code.ToUpper().Contains("ZHZ"))
            {
                return 0.015;
            }
            return limitedValue;
        }
        /// <summary>
        /// 获取钢筋截面积
        /// </summary>
        /// <param name="ironSpec"></param>
        /// <returns></returns>
        public static double GetIronSectionArea(int dn)
        {
            double sectionArea = dn;
            switch (dn)
            {
                case 6:
                    sectionArea = 28.3;
                    break;
                case 8:
                    sectionArea = 50.3;
                    break;
                case 10:
                    sectionArea = 78.5;
                    break;
                case 12:
                    sectionArea = 113.1;
                    break;
                case 14:
                    sectionArea = 153.9;
                    break;
                case 16:
                    sectionArea = 201.1;
                    break;
                case 18:
                    sectionArea = 254.5;
                    break;
                case 20:
                    sectionArea = 314.2;
                    break;
                case 22:
                    sectionArea = 380.1;
                    break;
                case 25:
                    sectionArea = 490.9;
                    break;
                case 28:
                    sectionArea = 615.8;
                    break;
                case 32:
                    sectionArea = 804.2;
                    break;
                case 36:
                    sectionArea = 1017.9;
                    break;
                case 40:
                    sectionArea = 1256.6;
                    break;
                case 50:
                    sectionArea = 1963.5;
                    break;
            }
            return sectionArea;
        }
    }
}
