using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    public class BaseFunction
    {
        public static string getJsonByObject(Object obj)

        {

            //实例化DataContractJsonSerializer对象，需要待序列化的对象类型

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());

            //实例化一个内存流，用于存放序列化后的数据

            MemoryStream stream = new MemoryStream();

            //使用WriteObject序列化对象

            serializer.WriteObject(stream, obj);

            //写入内存流中

            byte[] dataBytes = new byte[stream.Length];

            stream.Position = 0;

            stream.Read(dataBytes, 0, (int)stream.Length);

            //通过UTF8格式转换为字符串

            return Encoding.UTF8.GetString(dataBytes);

        }
        public static Object getObjectByJson(string jsonString, Object obj)

        {
            //实例化DataContractJsonSerializer对象，需要待序列化的对象类型
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //把Json传入内存流中保存
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            // 使用ReadObject方法反序列化成对象
            return serializer.ReadObject(stream);

        }
        /// <summary>
        /// 读取txt文件
        /// </summary>
        /// <param name="path"></param>
        public static string Read(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            return sr.ReadLine();
        }
        /// <summary>
        /// 写入txt文件
        /// </summary>
        /// <param name="path"></param>
        public static void Write(string path, string str)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(str);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "BaseFunction.Write");
            }
        }
        /// <summary>
        /// 纯数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str) //接收一个string类型的参数,保存到str里
        {
            if (str == null || str.Length == 0)    //验证这个参数是否为空
                return false;                           //是，就返回False
            ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例
            byte[] bytestr = ascii.GetBytes(str);         //把string类型的参数保存到数组里
            int count = 0;
            foreach (byte c in bytestr)                   //遍历这个数组里的内容
            {
                if (c == 46)
                {
                    count++;
                }
                else if (c < 48 || c > 57)                          //判断是否为数字
                {
                    return false;                              //不是，就返回False
                }
            }
            if (count > 1)
            {
                return false;
            }
            return true;                                        //是，就返回True
        }
        public static List<string> SplitCode(string code)
        {
            List<string> strs = new List<string>();
            string str = "";
            string num = "";
            byte[] arr = System.Text.Encoding.ASCII.GetBytes(code);
            int startIndex = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (!((int)(arr[i]) >= 48 && (int)(arr[i]) <= 57))
                {
                    str += (char)arr[i];
                }
                else
                {
                    startIndex = i;
                    break;
                }
            }
            for (int i = startIndex; i < arr.Length; i++)
            {
                if ((int)(arr[i]) >= 48 && (int)(arr[i]) <= 57)
                {
                    num += (char)arr[i];
                }
                else
                {
                    break;
                }
            }
            if (str != "" && num != "")
            {
                strs.Add(str);
                strs.Add(num);
            }
            return strs;
        }
        public static bool IsColumnCode(string content)
        {
            //具体判断等有规则后再完善
            List<string> codes = new List<string> { "KZ", "ZHZ", "XZ", "LZ", "QZ" };
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }
            content = content.Trim().ToUpper();
            var res = codes.Where(i => content.IndexOf(i) >= 0).Select(i => i).ToList();
            if (res != null && res.Count > 0)
            {
                return true;
            }
            return false;
        }
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
            if(string.IsNullOrEmpty(code))
            {
                return res;
            }
            code = code.ToUpper();
            if (code.Contains("KZ"))
            {
                res = "框架柱";
            }
            else if(code.Contains("ZHZ"))
            {
                res = "转换柱";
            }
            else if(code.Contains("XZ"))
            {
                res = "芯柱";
            }
            else if (code.Contains("LZ"))
            {
                res = "梁柱";
            }
            else if (code.Contains("QZ"))
            {
                res = "墙上柱";
            }
            return res;
        }
        /// <summary>
        /// 小数点保留位数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double MathRound(double value,int num)
        {
            if(value==0.0)
            {
                return 0.0;
            }
            return Math.Round(value, num);
        }
    }
}
