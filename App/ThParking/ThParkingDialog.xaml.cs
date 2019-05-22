using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows.Data;
using DotNetARX;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using TianHua.AutoCAD.Parking;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Path = System.IO.Path;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace TianHua.AutoCAD.Parking
{
    /// <summary>
    /// ThParkingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ThParkingDialog : Window
    {
        private static ThParkingDialog singleThParkingDialog;
        public static ThParkingDialog GetInstance()
        {
            if (singleThParkingDialog==null)
            {
                singleThParkingDialog = new ThParkingDialog();
            }

            return singleThParkingDialog;
        }

        /// <summary>
        /// 重写close方法
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel=true;
        }

        private ThParkingDialog()
        {
            InitializeComponent();
            //将viewmodel设置为datacontext
            DataContext = new ThParkingNumberViewModel();

            //**********
            //样式设定：1、button大小是固定的
            //可输入的control，上面都有一个label解释
            //一组要输入的内容，总是放在一个groupbox内
            //combox\textbox,横向拉伸，纵向不拉伸
            //radio\check,大小不变
        }

        //自动添加序号列
        private void ListViewBlocks_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        //自动减去序号列
        private void ListViewBlocks_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            var dgData = (DataGrid)sender;
            ListViewBlocks_LoadingRow(sender, e);
            if (dgData.Items != null)
            {
                for (int i = 0; i < dgData.Items.Count; i++)
                {
                    try
                    {
                        DataGridRow row = dgData.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                        if (row != null)
                        {
                            row.Header = (i + 1).ToString();
                        }
                    }
                    catch { }
                }
            }
        }

    }
}
