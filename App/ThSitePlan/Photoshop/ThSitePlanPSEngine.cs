using Autodesk.AutoCAD.Runtime;
using System;
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
            if (jobs.Status == UpdateStaus.NoUpdate)
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
            if (ValidateItem(job) == UpdateStaus.NoUpdate)
            {
                return;
            }

            foreach (var generator in Generators)
            {
                generator.Generate(path, job);
            }
        }

        private void Run(string path, ThSitePlanConfigItemGroup jobs, IntPtr progressform, string progressbarname)
        {
            if (jobs.Status == UpdateStaus.NoUpdate)
            {
                return;
            }

            while (jobs.Items.Count !=0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Run(path, item, progressform, progressbarname);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Run(path, group, progressform, progressbarname);
                }
            }
        }

        private void Run(string path, ThSitePlanConfigItem job, IntPtr progressform, string progressbarname)
        {
            if (ValidateItem(job) == UpdateStaus.NoUpdate)
            {
                return;
            }

            var prform = Form.FromHandle(progressform) as Form;
            ProgressBar pb = prform.Controls.Find(progressbarname, true)[0] as ProgressBar;

            foreach (var generator in Generators)
            {
                generator.Generate(path, job);
                pb.Value += 1;
            }
        }

        public void PSRun(string path, ThSitePlanConfigItemGroup jobs)
        {
            if (jobs.Status == UpdateStaus.NoUpdate)
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

        public void PSRun(string path, ThSitePlanConfigItemGroup jobs, IntPtr progressform, string progressbarname)
        {

            if (jobs.Status == UpdateStaus.NoUpdate)
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
                        Run(path, item, progressform, progressbarname);
                    }
                    else if (obj is ThSitePlanConfigItemGroup group)
                    {
                        Run(path, group, progressform, progressbarname);
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
            if (jobs.Status == UpdateStaus.NoUpdate)
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
            if (ValidateItem(job) == UpdateStaus.NoUpdate)
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
            if (jobs.Status == UpdateStaus.NoUpdate)
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

        public void PSClean(ThSitePlanConfigItemGroup jobs)
        {
            while (jobs.Items.Count != 0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Clean(item);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Clean(group);
                }
            }

        }

        public void Clean(ThSitePlanConfigItemGroup jobs)
        {
            if (jobs.Status == UpdateStaus.NoUpdate)
            {
                return;
            }

            while (jobs.Items.Count != 0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Clean(item);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Clean(group);
                }
            }
        }

        private void Clean(ThSitePlanConfigItem job)
        {
            if (job.Status == UpdateStaus.NoUpdate)
            {
                return;
            }

            foreach (var generator in Generators)
            {
                generator.Clean(job);
            }
        }


        private UpdateStaus ValidateItem(ThSitePlanConfigItem job)
        {
            return job.Status;
        }
    }
}
