using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.PreferencesFiles;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public partial class ThToolPalette : Form
    {
        public ThToolPalette()
        {
            InitializeComponent();
        }

        //记录可加载和已加载的数量
        int sourceCount = 0;
        int currentCount = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            //上来先清空，重新加载
            lstCurrent.Items.Clear();
            lstSource.Items.Clear();

            //保存按钮初始为失效状态
            btnSave.Enabled = false;

            //找到工具选项版的路径
            string path = GetAppResourcePath();
            DirectoryInfo root = new DirectoryInfo(path);

            //找到已经加载的，显示再菜单中
            var addedPathes = root.GetDirectories().Join(GetAllToolPath(), d => d.FullName, p => p, (d, p) => p);
            foreach (var item in addedPathes)
            {
                var directory = new DirectoryInfo(item);
                lstCurrent.Items.Add(directory.Name);
            }
            //将未加载的显示在起始菜单中
            var sourcePathes = root.GetDirectories().Select(d => d.FullName).Except(addedPathes);
            foreach (var item in sourcePathes)
            {
                var directory = new DirectoryInfo(item);
                lstSource.Items.Add(directory.Name);
            }

            UpdateCount();
        }


        /// <summary>
        /// 更新listbox记录数量
        /// </summary>
        private void UpdateCount()
        {
            //更新数量记录
            sourceCount = lstSource.Items.Count;
            currentCount = lstCurrent.Items.Count;
        }

        /// <summary>
        /// 加载指定工具选项板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, EventArgs e)
        {
            if (lstSource.SelectedItem != null)
            {
                //修改界面显示
                lstCurrent.Items.Add(lstSource.SelectedItem);
                lstSource.Items.Remove(lstSource.SelectedItem);

                if (lstSource.Items.Count > 0)
                {
                    lstSource.SelectedIndex = 0;
                }
            }

        }

        /// <summary>
        /// 获取所有已经加载的工具选项板的路径
        /// </summary>
        private List<string> GetAllToolPath()
        {
            var paths = new ToolPalettePath(AcadApp.Preferences);
            return paths.GetPaths().ToList();
        }

        /// <summary>
        /// 为系统添加工具选项板路径
        /// </summary>
        /// <param name="currentDwgPath"></param>
        private void AddToolPath(string currentDwgPath)
        {
            var paths = new ToolPalettePath(AcadApp.Preferences);
            if (!paths.Contains(currentDwgPath))
            {
                paths.Add(currentDwgPath);
                paths.SaveChanges();
            }
        }

        /// <summary>
        /// 为系统删除工具选项板路径
        /// </summary>
        /// <param name="currentDwgPath"></param>
        private void DeleteToolPath(string currentDwgPath)
        {
            var paths = new ToolPalettePath(AcadApp.Preferences);
            if (paths.Contains(currentDwgPath))
            {
                paths.Remove(currentDwgPath);
                paths.SaveChanges();
            }
        }

        /// <summary>
        /// 卸载工具面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, EventArgs e)
        {
            if (lstCurrent.SelectedItem != null)
            {
                lstSource.Items.Add(lstCurrent.SelectedItem);
                lstCurrent.Items.Remove(lstCurrent.SelectedItem);

                if (lstCurrent.Items.Count > 0)
                {
                    lstCurrent.SelectedIndex = 0;
                }
            }

        }

        /// <summary>
        /// 工具选项版全部加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllOn_Click(object sender, EventArgs e)
        {
            foreach (var item in lstSource.Items)
            {
                lstCurrent.Items.Add(item);
            }
            lstCurrent.SelectedIndex = 0;
            lstSource.Items.Clear();
        }

        /// <summary>
        /// 工具全部卸载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllDown_Click(object sender, EventArgs e)
        {
            foreach (var item in lstCurrent.Items)
            {
                lstSource.Items.Add(item);
            }

            lstSource.SelectedIndex = 0;
            lstCurrent.Items.Clear();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 获取当前运行程序所在的目录
        /// </summary>
        /// <returns></returns>
        public string GetAppResourcePath()
        {
            return Path.Combine(ThCADCommon.SupportPath(), "ToolPalette");
        }

        private void lstSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            //一旦listbox总数发生变化，保存键激活
            if (lstSource.Items.Count != sourceCount)
            {
                btnSave.Enabled = true;
            }
        }

        private void lstCurrent_SelectedIndexChanged(object sender, EventArgs e)
        {
            //一旦listbox总数发生变化，保存键激活
            if (lstCurrent.Items.Count != currentCount)
            {
                btnSave.Enabled = true;
            }
        }

        /// <summary>
        /// 执行用户的修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            //获取已有的加载情况，进行比对
            string path = GetAppResourcePath();
            DirectoryInfo root = new DirectoryInfo(path);

            //找到原来已经加载的
            var addedPathes = root.GetDirectories().Join(GetAllToolPath(), d => d.FullName, p => p, (d, p) => new { p, name = p.Split('\\').Last() });


            //获取调整后，listbox中显示加载的
            var usAddedPathes = lstCurrent.Items.Cast<object>().OfType<string>();

            //如果调整后，比原来得多，那么对所有多出来得专业进行加载
            if (usAddedPathes.Count() >= addedPathes.Count())
            {
                var majors = usAddedPathes.Except(addedPathes.Select(a => a.name));

                foreach (var major in majors)
                {
                    //为系统添加工具选项板路径
                    AddToolPath(Path.Combine(path, major));
                }
            }
            //否则，对少掉的进行卸载
            else
            {
                var majors = addedPathes.Select(a => a.name).Except(usAddedPathes);

                foreach (var major in majors)
                {
                    //为系统删除工具选项板路径
                    DeleteToolPath(Path.Combine(path, major));
                }
            }

            //切换回失效状态
            btnSave.Enabled = false;
            //更新计数
            UpdateCount();
        }

        /// <summary>
        /// 显示工具选项板
        /// </summary>
        public void ShowToolPalette()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            doc.SendCommand("TOOLPALETTES ");
        }
    }

}
