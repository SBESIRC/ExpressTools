using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Service
{
    public class MathLogic
    {
        public static double Distance(Point a,Point b)
        {
            double x1 = a.X - b.X;
            double y1 = a.Y - b.Y;
            double z1 = a.Z - b.Z;
            return Math.Sqrt(x1*x1+y1*y1+z1*z1);
        }
        /// <summary>
        /// 是否共线
        /// 海伦公式
        /// S=sqrt(p(p-a)(p-b)(p-c)) 
        /// p=(|a|+|b|+|C|)/2
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool CollinearThreePoints(Point a, Point b, Point c)
        {
            double edgeA = Distance(a, b);
            double edgeB = Distance(b, c);
            double edgeC = Distance(a, c);

            double p = 0.5 * (edgeA + edgeB + edgeC);

            //面积大于零，就是一个三角形，三点不共线
            if(p*(p-edgeA)*(p-edgeB)*(p-edgeC)>0)
            {
                return false;
            }
            return true;
        }
    }
}
