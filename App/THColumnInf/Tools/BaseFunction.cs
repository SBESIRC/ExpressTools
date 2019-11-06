using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    public class BaseFunction
    {
        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0:x2}{1:x2}", bytes[i + 1], bytes[i]);
            }
            return stringBuilder.ToString();
        }
        /// <summary>  
        /// 字符串转为UniCode码字符串  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        public static string StringToUnicode(string s)
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(String.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }
        /// <summary>  
        /// Unicode字符串转为正常字符串  
        /// </summary>  
        /// <param name="srcText"></param>  
        /// <returns></returns>  
        public static string UnicodeToString(string srcText)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 6;
            for (int i = 0; i <= len - 1; i++)
            {
                string str = "";
                str = src.Substring(0, 6).Substring(2);
                src = src.Substring(6);
                byte[] bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }
        /// <summary>
        /// 检查点是否在矩形盒子里
        /// </summary>
        /// <param name="pt">检查点</param>
        /// <param name="leftDownPt">左下角点</param>
        /// <param name="rightUpPt">右上角点</param>
        /// <returns></returns>
        public static bool CheckPtInBox(Point3d pt, Point3d leftDownPt,Point3d rightUpPt)
        {
            bool isIn = false;
            if ((pt.X >= leftDownPt.X && pt.X <= rightUpPt.X) &&
                (pt.Y >= leftDownPt.Y && pt.Y <= rightUpPt.Y) &&
                (pt.Z >= leftDownPt.Z && pt.Z <= rightUpPt.Z)
                )
            {
                isIn = true;
            }
            return isIn;
        }
        /// <summary>
        /// 检查点在矩形框内
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="leftDownPt"></param>
        /// <param name="rightUpPt"></param>
        /// <returns></returns>
        public static bool CheckPtRectangle(Point3d pt, Point3d leftDownPt, Point3d rightUpPt)
        {
            bool isIn = false;
            if ((pt.X >= leftDownPt.X && pt.X <= rightUpPt.X) &&
                (pt.Y >= leftDownPt.Y && pt.Y <= rightUpPt.Y) 
                )
            {
                isIn = true;
            }
            return isIn;
        }
        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double RadToAng(double rad)
        {
            return rad / Math.PI * 180.0;
        }
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="ang"></param>
        /// <returns></returns>
        public static double AngToRad(double ang)
        {
            return ang / 180.0 * Math.PI;
        }
        /// <summary>
        /// 获取两点的中点
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Point3d GetMidPt(Point3d pt1, Point3d pt2)
        {
            return new Point3d((pt1.X+pt2.X)/2.0, (pt1.Y + pt2.Y) / 2.0, (pt1.Z + pt2.Z) / 2.0);
        }
        /// <summary>
        /// 获取ErrorMsg各项中文翻译
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static string GetErrorMsg(ErrorMsg errorMsg)
        {
            string inf = "";
            switch(errorMsg)
                {
                case ErrorMsg.CodeEmpty:
                    inf = "柱编号缺失";
                    break;
                case ErrorMsg.InfNotCompleted:
                    inf = "参数识别不全";
                    break;
                default:
                    inf = "数据正确";
                    break;
            }
            return inf;
        }
        /// <summary>
        /// 获取柱子编号中文翻译
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetColumnCodeChinese(string code)
        {
            string res = "";
            if(code.ToUpper().Contains("KZ"))
            {
                res = "框架柱";
            }
            return res;
        }
    }
}
