using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.Windows.Forms;
using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSEngine
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanPSEngine instance = new ThSitePlanPSEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanPSEngine() { }
        internal ThSitePlanPSEngine() { }
        public static ThSitePlanPSEngine Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public List<ThSitePlanPSGenerator> Generators { get; set; }

        private void Run(string path, ThSitePlanConfigItemGroup jobs)
        {
            if (!jobs.IsEnabled)
            {
                return;
            }

            while (jobs.Items.Count !=0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Run(path, item);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Run(path, group);
                }
            }
        }

        private void Run(string path, ThSitePlanConfigItem job)
        {
            if (!ValidateItem(job))
            {
                return;
            }

            foreach (var generator in Generators)
            {
                generator.Generate(path, job);
            }
        }

        public void PSRun(string path, ThSitePlanConfigItemGroup jobs)
        {
            if (!jobs.IsEnabled)
            {
                return;
            }

            using (ProgressMeter pm = new ProgressMeter())
            {
                // 启动进度条
                int progresslimit = jobs.GetEnableItemsCount();
                pm.SetLimit(progresslimit);
                pm.Start("正在生成PhotoShop图形");
                while (jobs.Items.Count != 0)
                {
                    var obj = jobs.Items.Dequeue();
                    if (obj is ThSitePlanConfigItem item)
                    {
                        Run(path, item);
                    }
                    else if (obj is ThSitePlanConfigItemGroup group)
                    {
                        Run(path, group);
                    }
                    // 更新进度条
                    pm.MeterProgress();
                    // 让CAD在长时间任务处理时任然能接收消息
                    Application.DoEvents();
                }
                // 停止进度条
                pm.Stop();
            }
        }

        public void Update(string path, ThSitePlanConfigItemGroup jobs)
        {
            if (!jobs.IsEnabled)
            {
                return;
            }

            while (jobs.Items.Count != 0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Update(path, item);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Update(path, group);
                }
            }
        }

        private void Update(string path, ThSitePlanConfigItem job)
        {
            if (!ValidateItem(job))
            {
                return;
            }

            foreach (var generator in Generators)
            {
                generator.Update(path, job);
            }
        }

        public void PSUpdate(string path, ThSitePlanConfigItemGroup jobs)
        {
            if (!jobs.IsEnabled)
            {
                return;
            }

            using (ProgressMeter pm = new ProgressMeter())
            {
                // 启动进度条
                int progresslimit = jobs.GetEnableItemsCount();
                pm.SetLimit(progresslimit);
                pm.Start("正在更新PhotoShop图形");

                while (jobs.Items.Count != 0)
                {
                    var obj = jobs.Items.Dequeue();
                    if (obj is ThSitePlanConfigItem item)
                    {
                        Update(path, item);
                    }
                    else if (obj is ThSitePlanConfigItemGroup group)
                    {
                        Update(path, group);
                    }

                    // 更新进度条
                    pm.MeterProgress();
                    // 让CAD在长时间任务处理时任然能接收消息
                    Application.DoEvents();
                }

                // 停止进度条
                pm.Stop();
            }
        }


        private bool ValidateItem(ThSitePlanConfigItem job)
        {
            return job.IsEnabled;
        }
    }
}
