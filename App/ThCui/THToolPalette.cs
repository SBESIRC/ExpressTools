using DotNetARX;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public partial class ThToolPalette : Form
    {
        public ThToolPalette()
        {
            InitializeComponent();
        }

        //安装路径
        string path = @"";
        //记录可加载和已加载的数量
        int sourceCount = 0;
        int currentCount = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            //保存按钮初始为失效状态
            btnSave.Enabled = false;

            //找到工具选项版的路径
            path = GetAppResourcePath();
            //MessageBox.Show(path);
            DirectoryInfo root = new DirectoryInfo(path);


            //找到已经加载的，显示再菜单中
            var addedPathes = root.GetDirectories().Join(GetAllToolPath(), d => d.FullName, p => p, (d, p) => p);
            foreach (var item in addedPathes)
            {
                var name = item.ToString().Split('\\').Last();
                lstCurrent.Items.Add(name);
            }

            //将未加载的显示在起始菜单中
            var sourcePathes = root.GetDirectories().Select(d => d.FullName).Except(addedPathes);
            foreach (var item in sourcePathes)
            {
                var name = item.ToString().Split('\\').Last();
                lstSource.Items.Add(name);
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
        /// 将安装包中的atc文件数据源路径进行更新
        /// </summary>
        /// <param name="path"></param>
        /// <param name="formalDwgPath"></param>
        /// <param name="currentDwgPath"></param>
        private static void UpdateDwgPath(string path, string currentDwgPath)
        {
            //原始的面板数据源地址
            //var formalDwgPath = @"C:\Program Files\ToolPalette\ToolPalette.dwg";

            //遍历所有的atc文件，并将数据源关联路径设置为安装所在路径
            DirectoryInfo root = new DirectoryInfo(path);
            var files = root.GetFiles().Where(f => f.FullName.Contains(".atc"));
            foreach (var item in files)
            {
                //MessageBox.Show(item.FullName);
                string strContent = File.ReadAllText(item.FullName);
                strContent = Regex.Replace(strContent, @"<SourceFile>.*?</SourceFile>", @"<SourceFile>" + currentDwgPath + @"</SourceFile>");
                //strContent = strContent.Replace(formalDwgPath, currentDwgPath);
                File.WriteAllText(item.FullName, strContent);
            }
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
                ////获取所选专业
                //var major = lstSource.SelectedItem.ToString();

                ////安装包数据源地址,允许在此路径下，以任意名称命名的dwg文件为数据源
                ////var currentDwgPath = path + major + @"\ToolPalette.dwg";
                //var currentDwgPath = new DirectoryInfo(path + major).GetFiles(@"*.dwg").First().FullName;

                ////更新安装包内数据源地址
                //UpdateDwgPath(path + major + @"\Palettes\", currentDwgPath);

                ////为系统添加工具选项板路径
                //AddToolPath(path + major);

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
            return Preferences.Files.ToolPalettePath.Replace(@"/", @"\").Split(';').ToList();

            #region 注册表的方法
            //var autoCADKeyName = GetAutoCADKeyName();

            ////确定是HKEY_CURRENT_USER还是HKEY_LOCAL_MACHINE
            //RegistryKey keyRoot = true ? Registry.CurrentUser : Registry.LocalMachine;
            //// 由于某些AutoCAD版本的HKEY_CURRENT_USER可能不包括Applications键值，因此要创建该键值
            //// 如果已经存在该鍵，无须担心可能的覆盖操作问题，因为CreateSubKey函数会以写的方式打开它而不会执行覆盖操作
            //RegistryKey keyApp = keyRoot.CreateSubKey(autoCADKeyName + "\\" + "Profiles");

            ////将安装包路径设置为工具选项版路径
            //var toolKey = keyApp.OpenSubKey("<<未命名配置>>\\General", true);
            //return toolKey.GetValue("ToolPalettePath").ToString().Split(';').ToList(); 
            #endregion
        }



        /// <summary>
        /// 为系统添加工具选项板路径
        /// </summary>
        /// <param name="currentDwgPath"></param>
        private void AddToolPath(string currentDwgPath)
        {
            Preferences.Files.ToolPalettePath += ";" + currentDwgPath;

            #region 原先修改注册表的方式
            //var autoCADKeyName = GetAutoCADKeyName();

            ////确定是HKEY_CURRENT_USER还是HKEY_LOCAL_MACHINE
            //RegistryKey keyRoot = true ? Registry.CurrentUser : Registry.LocalMachine;
            //// 由于某些AutoCAD版本的HKEY_CURRENT_USER可能不包括Applications键值，因此要创建该键值
            //// 如果已经存在该鍵，无须担心可能的覆盖操作问题，因为CreateSubKey函数会以写的方式打开它而不会执行覆盖操作
            //RegistryKey keyApp = keyRoot.CreateSubKey(autoCADKeyName + "\\" + "Profiles");

            ////将安装包路径设置为工具选项版路径
            //var toolKey = keyApp.OpenSubKey("<<未命名配置>>\\General", true);
            //var toolPath = toolKey.GetValue("ToolPalettePath").ToString();
            //toolKey.SetValue("ToolPalettePath", currentDwgPath + ";" + toolPath, RegistryValueKind.ExpandString); 
            #endregion
        }

        /// <summary>
        /// 为系统删除工具选项板路径
        /// </summary>
        /// <param name="currentDwgPath"></param>
        private void DeleteToolPath(string currentDwgPath)
        {
            //如果此路径为最后一个，则少删除一个分号，否则多删除一个分号
            Preferences.Files.ToolPalettePath = Preferences.Files.ToolPalettePath.EndsWith(currentDwgPath) ? Preferences.Files.ToolPalettePath.Replace(currentDwgPath, "") : Preferences.Files.ToolPalettePath.Replace(currentDwgPath + ";", "");

            #region 注册表的方式
            //var autoCADKeyName = GetAutoCADKeyName();

            ////确定是HKEY_CURRENT_USER还是HKEY_LOCAL_MACHINE
            //RegistryKey keyRoot = true ? Registry.CurrentUser : Registry.LocalMachine;
            //// 由于某些AutoCAD版本的HKEY_CURRENT_USER可能不包括Applications键值，因此要创建该键值
            //// 如果已经存在该鍵，无须担心可能的覆盖操作问题，因为CreateSubKey函数会以写的方式打开它而不会执行覆盖操作
            //RegistryKey keyApp = keyRoot.CreateSubKey(autoCADKeyName + "\\" + "Profiles");

            ////将安装包路径从默认路径中删除
            //var toolKey = keyApp.OpenSubKey("<<未命名配置>>\\General", true);
            //var toolPath = toolKey.GetValue("ToolPalettePath").ToString();
            //toolKey.SetValue("ToolPalettePath", toolPath.Replace(currentDwgPath + ";", ""), RegistryValueKind.ExpandString); 
            #endregion
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
                ////获取所选专业
                //var major = lstCurrent.SelectedItem.ToString();
                ////安装包数据源地址
                //var currentDwgPath = new DirectoryInfo(path + major).GetFiles(@"*.dwg").First().FullName;

                //////更新安装包内数据源地址
                ////UpdateDwgPath(path, currentDwgPath);

                ////为系统删除工具选项板路径
                //DeleteToolPath(path + major);

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

                ////获取所选专业
                //var major = item.ToString();
                ////安装包数据源地址
                //var currentDwgPath = new DirectoryInfo(path + major).GetFiles(@"*.dwg").First().FullName;

                ////更新安装包内数据源地址
                //UpdateDwgPath(path + major + @"\Palettes\", currentDwgPath);

                ////为系统添加工具选项板路径
                //AddToolPath(path + major);
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

                ////获取所选专业
                //var major = item.ToString();
                ////安装包数据源地址
                //var currentDwgPath = new DirectoryInfo(path + major).GetFiles(@"*.dwg").First().FullName;

                ////更新安装包内数据源地址
                //UpdateDwgPath(path, currentDwgPath);

                ////为系统删除工具选项板路径
                //DeleteToolPath(path + major);
            }

            lstSource.SelectedIndex = 0;
            lstCurrent.Items.Clear();
        }


        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }




        public static string GetAutoCADKeyName()
        {
            // 获取HKEY_CURRENT_USER键
            RegistryKey keyCurrentUser = Registry.CurrentUser;
            // 打开AutoCAD所属的注册表键:HKEY_CURRENT_USER\Software\Autodesk\AutoCAD
            RegistryKey keyAutoCAD = keyCurrentUser.OpenSubKey("Software\\Autodesk\\AutoCAD");
            //获得表示当前的AutoCAD版本的注册表键值:R18.2
            string valueCurAutoCAD = keyAutoCAD.GetValue("CurVer").ToString();
            if (valueCurAutoCAD == null) return "";//如果未安装AutoCAD，则返回
            //获取当前的AutoCAD版本的注册表键:HKEY_LOCAL_MACHINE\Software\Autodesk\AutoCAD\R18.2
            RegistryKey keyCurAutoCAD = keyAutoCAD.OpenSubKey(valueCurAutoCAD);
            //获取表示AutoCAD当前语言的注册表键值:ACAD-a001:804
            string language = keyCurAutoCAD.GetValue("CurVer").ToString();
            //获取AutoCAD当前语言的注册表键:HKEY_LOCAL_MACHINE\Software\Autodesk\AutoCAD\R18.2\ACAD-a001:804
            RegistryKey keyLanguage = keyCurAutoCAD.OpenSubKey(language);
            //返回去除HKEY_LOCAL_MACHINE前缀的当前AutoCAD注册表项的键名:Software\Autodesk\AutoCAD\R18.2\ACAD-a001:804
            return keyLanguage.Name.Substring(keyCurrentUser.Name.Length + 1);
        }


        /// <summary>
        /// 获取当前运行程序所在的目录
        /// </summary>
        /// <returns></returns>
        public string GetAppResourcePath()
        {
            //获取appdata放置插件的路径
            var bundleName = @"ThCADPlugin.bundle";
            var destDirName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\ApplicationPlugins\" + bundleName;
            return destDirName + @"\Contents\Resources\ToolPalette\";
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

            path = GetAppResourcePath();
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
                    //安装包数据源地址
                    var currentDwgPath = new DirectoryInfo(path + major).GetFiles(@"*.dwg").First().FullName;

                    //更新安装包内数据源地址
                    UpdateDwgPath(path + major + @"\Palettes\", currentDwgPath);

                    //为系统添加工具选项板路径
                    AddToolPath(path + major);
                }
            }
            //否则，对少掉的进行卸载
            else
            {
                var majors = addedPathes.Select(a => a.name).Except(usAddedPathes);

                foreach (var major in majors)
                {
                    //安装包数据源地址
                    var currentDwgPath = new DirectoryInfo(path + major).GetFiles(@"*.dwg").First().FullName;

                    //更新安装包内数据源地址
                    UpdateDwgPath(path, currentDwgPath);

                    //MessageBox.Show(path+major);
                    //为系统删除工具选项板路径
                    DeleteToolPath(path + major);
                }
            }

            //切换回失效状态
            btnSave.Enabled = false;
            //更新计数
            UpdateCount();
            //MessageBox.Show("天华图库已经重新配置，请重启CAD后生效！");
        }
    }
}
