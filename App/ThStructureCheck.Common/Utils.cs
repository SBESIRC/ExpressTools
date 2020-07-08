using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ThStructureCheck.Common
{
    public static class Utils
    {
        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool IsNumeric(string number)
        {
            bool res = false;
            if (string.IsNullOrEmpty(number))
            {
                return res;
            }
            string pattern = @"^[+-]?[\d]+[.]?[\d]+$";
            Regex rg = new Regex(pattern);
            if (rg.IsMatch(number))
            {
                res = true;
            }
            return res;
        }
        public static List<int> GetIntDatas(string str)
        {
            List<int> values = new List<int>();
            string pattern = @"[-]?\d+";
            MatchCollection matches = Regex.Matches(str, pattern);
            foreach (var match in matches)
            {
                if (!string.IsNullOrEmpty(match.ToString()))
                {
                    values.Add(Convert.ToInt32(match.ToString()));
                }
            }
            return values;
        }
        public static List<double> GetDoubleDatas(string str)
        {
            List<double> values = new List<double>();
            string pattern = @"[+-]?[\d]+[.]?[\d]+";
            MatchCollection matches = Regex.Matches(str, pattern);
            foreach (var match in matches)
            {
                if (!string.IsNullOrEmpty(match.ToString()))
                {
                    values.Add(Convert.ToInt32(match.ToString()));
                }
            }
            return values;
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
        /// 写入异常
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="exception">异常信息</param>
        /// <param name="specialText">自定义文本</param>
        public static void WriteException(System.Exception exception, string customText = "", string assemblyName="ThStructureCheck.Log")
        {
            //string fileName = Guid.NewGuid() + "_" + DateTime.Now.ToString("s") + ".log";
            string fileName = Guid.NewGuid() + ".log";
            string basePath = System.IO.Path.GetTempPath();
            if (!Directory.Exists(basePath + assemblyName))
            {
                Directory.CreateDirectory(basePath + assemblyName);
            }
            string text = string.Empty;
            if (exception != null)
            {
                Type exceptionType = exception.GetType();
                if (!string.IsNullOrEmpty(customText))
                {
                    text = text + customText + Environment.NewLine;
                }
                text += "Exception: " + exceptionType.Name + Environment.NewLine;
                text += "               " + "Message: " + exception.Message + Environment.NewLine;
                text += "               " + "Source: " + exception.Source + Environment.NewLine;
                text += "               " + "StackTrace: " + exception.StackTrace + Environment.NewLine;
            }
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            LogConfig(basePath + assemblyName + "\\" + fileName);
            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
            Logger.Error(text);
        }
        private static void LogConfig(string fileName)
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fileName };
            //var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            //config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            // Apply config           
            NLog.LogManager.Configuration = config;
        }
        /// <summary>
        /// 计算差值
        /// </summary>
        /// <param name="A2">起始值</param>
        /// <param name="A1">终点值</param>
        /// <param name="A0">插入值</param>
        /// <param name="x2">起始值对应的参数值</param>
        /// <param name="x1">终点值对应的参数值</param>
        /// <returns></returns>
        public static double DifferenceMethod(double A2,double A1,double A0,double x2,double x1)
        {
            if (A2 - A1==0.0)
            {
                return 0.0;
            }
            return x2 - (A2 - A0) / (A2 - A1) * (x2 - x1);
        }
    }
}
