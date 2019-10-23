﻿using System;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Serilog;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThXClip
{    
    public class EntPropertyInfo
    {
        public string Layer { get; set; }
        public int ColorIndex { get; set;}
        public LineWeight Lw { get; set; }
    }
    public class ThXClipUtils
    {
        public static void ChangeEntityProperty(Entity ent, EntPropertyInfo entPropertyInf)
        {
            ent.Layer = entPropertyInf.Layer;
            ent.ColorIndex = entPropertyInf.ColorIndex;
            ent.LineWeight = entPropertyInf.Lw;
        }
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            //ts3.ToString("g").Substring(0, 8) 0:00:07.1
            //ts3.ToString("c").Substring(0, 8) 00:00:07
            //ts3.ToString("G").Substring(0, 8) 0:00:00
            return ts3.ToString("G").Substring(0, 8);
        }
        public static void WriteException(System.Exception exception, string specialText = "")
        {
            string fileName = Guid.NewGuid() + "_" + DateTime.Now.ToString("s") + ".log";
            string basePath = System.IO.Path.GetTempPath();
            if (!Directory.Exists(basePath + "\\ThXlpLog"))
            {
                Directory.CreateDirectory(basePath + "\\ThXlpLog");
            }
            string text = string.Empty;
            if (exception != null)
            {
                Type exceptionType = exception.GetType();
                if (!string.IsNullOrEmpty(specialText))
                {
                    text = text + specialText + Environment.NewLine;
                }
                text += "Exception: " + exceptionType.Name + Environment.NewLine;
                text += "               " + "Message: " + exception.Message + Environment.NewLine;
                text += "               " + "Source: " + exception.Source + Environment.NewLine;
                text += "               " + "StackTrace: " + exception.StackTrace + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(text))
            {
                Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Error()
       .WriteTo.File(basePath + "\\ThXlpLog\\" + fileName,
           rollingInterval: RollingInterval.Day,
           rollOnFileSizeLimit: true)
       .CreateLogger();
                Log.Error(text);
                Log.CloseAndFlush();
            }
        }
    }
}
