using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// 梁端箍筋加密区
    /// </summary>
    public class BeamPortEncryptStirrup
    {
        private string antiSeismicGrade = "";

        public BeamPortEncryptStirrup(string antiSeismicGrade)
        {
            this.antiSeismicGrade = antiSeismicGrade;
        }
        /// <summary>
        /// 获取 加密区长度 (mm)
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public double GetAsvLength(double h)
        {
            if(antiSeismicGrade.Contains("一")&& !antiSeismicGrade.Contains("特"))
            {
                return Math.Max(2*h,500);
            }
            else if (antiSeismicGrade.Contains("二"))
            {
                return Math.Max(1.5 * h, 500);
            }
            else if (antiSeismicGrade.Contains("三"))
            {
                return Math.Max(1.5 * h, 500);
            }
            else if (antiSeismicGrade.Contains("四"))
            {
                return Math.Max(1.5 * h, 500);
            }
            return 0.0;
        }
        /// <summary>
        /// 获取箍筋最大间距 (mm)
        /// </summary>
        /// <param name="h">梁截面高度</param>
        /// <param name="d">纵向钢筋直径</param>
        /// <returns></returns>
        public double GetStirrupMaximumSpacing(double h,double d)
        {
            double stirrupSpacing = 0.0;
            if (antiSeismicGrade.Contains("一") && !antiSeismicGrade.Contains("特"))
            {
                stirrupSpacing = Math.Min(h / 4.0, 6 * d);
                stirrupSpacing = Math.Min(stirrupSpacing,100);
            }
            else if (antiSeismicGrade.Contains("二"))
            {
                stirrupSpacing = Math.Min(h / 4.0, 8 * d);
                stirrupSpacing = Math.Min(stirrupSpacing, 100);
            }
            else if (antiSeismicGrade.Contains("三"))
            {
                stirrupSpacing = Math.Min(h / 4.0, 8 * d);
                stirrupSpacing = Math.Min(stirrupSpacing, 150);
            }
            else if (antiSeismicGrade.Contains("四"))
            {
                stirrupSpacing = Math.Min(h / 4.0, 8 * d);
                stirrupSpacing = Math.Min(stirrupSpacing, 150);
            }
            return stirrupSpacing;
        }
        /// <summary>
        /// 获取箍筋最小直径 (mm)
        /// </summary>
        /// <returns></returns>
        public double GetStirrupMinimumDiameter()
        {
            if (antiSeismicGrade.Contains("一") && !antiSeismicGrade.Contains("特"))
            {
                return 10.0;
            }
            else if (antiSeismicGrade.Contains("二"))
            {
                return 8.0;
            }
            else if (antiSeismicGrade.Contains("三"))
            {
                return 8.0;
            }
            else if (antiSeismicGrade.Contains("四"))
            {
                return 6.0;
            }
            return 0.0;
        }
    }
}
