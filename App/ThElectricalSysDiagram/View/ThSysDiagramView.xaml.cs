using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Table = Autodesk.AutoCAD.DatabaseServices.Table;

namespace ThElectricalSysDiagram
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThSysDiagramView : Window
    {
        public ThSysDiagramView()
        {
            InitializeComponent();
            DataContext = new ThSysDiagramViewModel();
        }


        private void DgrElectricalBlocks_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void DgrElectricalBlocks_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            var dgData = (DataGrid)sender;
            DgrElectricalBlocks_LoadingRow(sender, e);
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

        private void dtrBlockRule_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void dtrBlockRule_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            var dgData = (DataGrid)sender;
            dtrBlockRule_LoadingRow(sender, e);
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


        private void DtrLayerRule_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void DtrLayerRule_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            var dgData = (DataGrid)sender;
            DtrLayerRule_UnloadingRow(sender, e);
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


        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            var viewModel = (ThSysDiagramViewModel)DataContext;
            //从外部数据库读取配置信息
            var blockInfos = viewModel.ElectricalTasks.GetThRelationInfos();
            foreach (var item in blockInfos)
            {
                viewModel.RelationBlockInfos.Add(item);
            }
            var fanInfos = viewModel.ElectricalTasks.GetThFanInfos();
            foreach (var item in fanInfos)
            {
                viewModel.RelationFanInfos.Add(item);
            }

            dtrLayerRule.SelectedIndex = 0;
            dtrBlockRule.SelectedIndex = 0;
        }


    }
}
