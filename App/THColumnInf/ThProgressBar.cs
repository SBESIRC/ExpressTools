using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class ThProgressBar
    {
        public static ProgressMeter ProgressBar;
        //分配进度条处理时间
        private static int limitLength = 0;
        public static int executeNum = 0;
        public static string message = "";
        static ThProgressBar()
        {
            Init();
        }
        private static void CreateProgressMeter()
        {
            if (ProgressBar == null || ProgressBar.IsDisposed)
            {
                ProgressBar = new ProgressMeter();
            }
        }
        private static void Init()
        {
            limitLength = 100;
            executeNum = 0;
        }
        public static void Dispose()
        {
            if(ProgressBar!=null && !ProgressBar.IsDisposed)
            {
                ProgressBar.Dispose();
            }
        }
        public static void MeterProgress()
        {
            if(ProgressBar==null)
            {
                return;
            }
            executeNum += 1;
            ProgressBar.MeterProgress();
            if (executeNum == limitLength) //Once again
            {
                executeNum = 0;
                ProgressBar.Start(message);
            }
        }
        public static void UpdateLimitLength()
        {
            ProgressBar.SetLimit(limitLength);
        }
        public static void Stop()
        {
            int count = limitLength - executeNum;
            for(int i=0;i< count;i++)
            {
                ProgressBar.MeterProgress();
            }
            ProgressBar.Stop();
            Init();
            Dispose();
        }
        public static void Start(string message)
        {
            Init();
            CreateProgressMeter();
            ThProgressBar.message = message;
            UpdateLimitLength();
            ProgressBar.Start(message);
        }
    }
}
