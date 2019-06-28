using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ThResourceLibrary;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ThElectricalSysDiagram
{
    public class ThSysDiagramViewModel : ThNotifyObject
    {
        public static string defaultUpStreamName = "提资块名";
        public static string defaultDownStreamName = "电气块名";

        public bool ResultState { get; set; }//结果状态

        /// <summary>
        /// 窗体读取命令
        /// </summary>
        private ThCommand _loadedCommand;
        public ThCommand LoadedCommand
        {
            get
            {
                if (_loadedCommand == null)
                    _loadedCommand = new ThCommand(
    o =>
    {
        //过滤出列表中没有的那些个块，避免重复添加
        var blockInfos = this.ElectricalTasks.GetThRelationInfos();
        foreach (var item in blockInfos)
        {
            this.RelationBlockInfos.Add(item);
        }


    });
                return _loadedCommand;
            }
        }

        /// <summary>
        /// 添加配置规则命令
        /// </summary>
        private ThCommand _ruleAddCommand;
        public ThCommand RuleAddCommand
        {
            get
            {
                if (_ruleAddCommand == null)
                {
                    _ruleAddCommand = new ThCommand(
        o =>
        {
            var dataGrid = (DataGrid)o;

            //如果没有选择一项，那么直接插入到底部的下一行，否则，插入到选中行的下一行
            var row = dataGrid.SelectedIndex == -1 ? dataGrid.Items.Count : dataGrid.SelectedIndex + 1;

            //*****不能够给一个无参构造函数，会导致obeserablecolletion误以为有一个值
            this.RelationBlockInfos.Insert(row, new ThRelationBlockInfo(new ThBlockInfo(defaultUpStreamName, null), new ThBlockInfo(defaultDownStreamName, null)));
            dataGrid.SelectedIndex = row;

            //添加表格中对应的行
            this.ElectricalTasks.AddTableRow(row);

            //*********左半边的，先不做验证，也不做转换块预览，直接罗列有哪些块被装进去了就可以了
            //*********右半边的，考虑做一个按钮，统一对配置界面的修改进行保存(实时操作的话过于复杂)，不考虑对原有块定义进行了改动的情况。由于增删改情况太多，一律对表格进行重新绘制。
            //******对于没有完成两个块拾取动作的规则，保存是一律忽略

        });

                }
                return _ruleAddCommand;
            }

        }


        /// <summary>
        /// 删除配置规则命令
        /// </summary>
        private ThCommand _ruleDeleteCommand;
        public ThCommand RuleDeleteCommand
        {
            get
            {
                if (_ruleDeleteCommand == null)
                {
                    _ruleDeleteCommand = new ThCommand(
        o =>
        {
            var dataGrid = (DataGrid)o;
            var row = dataGrid.SelectedIndex + 1;
            //通过Observercollection,对数据源的处理可直接反映到绑定对象上
            this.RelationBlockInfos.Remove((ThRelationBlockInfo)(dataGrid.SelectedItem));

            //删除表格中对应的行
            this.ElectricalTasks.DeleteTableRow(row);
        },
        o => this.RelationBlockInfos.Any());

                }
                return _ruleDeleteCommand;
            }

        }



        /// <summary>
        /// 删除拾取块表格行命令
        /// </summary>
        private ThCommand _hvacDeleteCommand;
        public ThCommand HvacDeleteCommand
        {
            get
            {
                if (_hvacDeleteCommand == null)
                {
                    _hvacDeleteCommand = new ThCommand(
        o =>
        {
            //通过Observercollection,对数据源的处理可直接反映到绑定对象上
            this.HvacBlockInfos.Remove((ThBlockInfo)o);
        },
        o => this.HvacBlockInfos.Select(info => info.RealName).Any());

                }
                return _hvacDeleteCommand;
            }

        }

        /// <summary>
        /// 清空拾取块命令
        /// </summary>
        private ThCommand _hvacClearCommand;
        public ThCommand HvacClearCommand
        {
            get
            {
                if (_hvacClearCommand == null)
                {
                    _hvacClearCommand = new ThCommand(
        o =>
        {
            //每次都拿掉第一个，直到全部拿完
            for (int i = 0; i < (int)o; i++)
            {
                this.HvacBlockInfos.RemoveAt(0);
            }
        },
        o => this.HvacBlockInfos.Select(info => info.RealName).Any());

                }
                return _hvacClearCommand;
            }

        }

        /// <summary>
        /// 拾取图块命令
        /// </summary>
        private ThCommand _chooseCommand;
        public ThCommand ChooseCommand
        {
            get
            {
                //拾取图块选择
                if (_chooseCommand == null)
                {
                    _chooseCommand = new ThCommand(
        o =>
        {
            //过滤出列表中没有的那些个块，避免重复添加
            var blockInfos = this.ElectricalTasks.GetThBlockInfos();
            var realblockInfos = blockInfos.Except(blockInfos.Join(this.HvacBlockInfos, p1 => p1.RealName, p2 => p2.RealName, (p1, p2) => p1));
            foreach (var item in realblockInfos)
            {
                this.HvacBlockInfos.Add(item);
            }

        });
                }
                return _chooseCommand;
            }

        }

        /// <summary>
        /// 获取对应列表命令
        /// </summary>
        private ThCommand _showCommand;
        public ThCommand ShowCommand
        {
            get
            {
                //拾取图块选择
                if (_showCommand == null)
                {
                    _showCommand = new ThCommand(
        o =>
        {
            //过滤出列表中没有的那些个块，避免重复添加
            var blockInfos = this.ElectricalTasks.GetThRelationInfos();
            foreach (var item in blockInfos)
            {
                this.RelationBlockInfos.Add(item);
            }

            //this.ElectricalTasks.Test();

        });
                }
                return _showCommand;
            }

        }

        /// <summary>
        /// 按当前规则转换块
        /// </summary>
        private ThCommand _convertCurrentCommand;
        public ThCommand ConvertCurrentCommand
        {
            get
            {
                //拾取图块选择
                if (_convertCurrentCommand == null)
                {
                    _convertCurrentCommand = new ThCommand(
        o =>
        {
            this.ResultState = true;//更改状态

            var window = (Window)o;
            window.Close();
            var tab = (TabItem)((TabControl)window.FindName("tctrRule")).SelectedItem;

            if (this.ResultState == true)
            {
                if (tab.Header.ToString() == "按图块转换")
                {
                    this.ElectricalTasks.ConvertBlock();
                }
                if (tab.Header.ToString() == "按图层转换")
                {
                    this.ElectricalTasks.ConvertFanBlock();
                }

                //执行完毕后，将状态改回
                this.ResultState = false;
            }

        }
        //, o =>
        //{
        //    var tab = (TabItem)o;
        //    return tab.Header.ToString() == "一对一转换" ? this.RelationBlockInfos.Any() : this.RelationFanInfos.Any();
        //}
        );
                }
                return _convertCurrentCommand;
            }

        }


        /// <summary>
        /// 按所有规则转换块
        /// </summary>
        private ThCommand _convertAllCommand;
        public ThCommand ConvertAllCommand
        {
            get
            {
                //拾取图块选择
                if (_convertAllCommand == null)
                {
                    _convertAllCommand = new ThCommand(
        o =>
        {
            //var tab = (TabItem)o;
            //if (tab.Header.ToString() == "一对一转换")
            //{
            //    this.ElectricalTasks.ConvertBlock();
            //}
            //if (tab.Header.ToString() == "风机转换")
            //{
            //    this.ElectricalTasks.ConvertFanBlock();
            //}
        }
        //, o =>
        //{
        //    var tab = (TabItem)o;
        //    return tab.Header.ToString() == "一对一转换" ? this.RelationBlockInfos.Any() : this.RelationFanInfos.Any();
        //}
        );
                }
                return _convertAllCommand;
            }

        }


        public ThElectricalTask ElectricalTasks { get; set; }//与电气转换块相关的方法类

        public ObservableCollection<ThBlockInfo> HvacBlockInfos { get; set; }//暖通图块信息

        public ObservableCollection<ThRelationBlockInfo> RelationBlockInfos { get; set; }//块对应关系信息

        public ObservableCollection<ThRelationFanInfo> RelationFanInfos { get; set; }//风机对应关系信息
        public ThSysDiagramViewModel()
        {
            //初始化任务类
            this.ElectricalTasks = new ThElectricalTask();
            //新建一个空的配置信息，用于初始化
            this.HvacBlockInfos = new ObservableCollection<ThBlockInfo>();

            this.RelationBlockInfos = new ObservableCollection<ThRelationBlockInfo>();

            this.RelationFanInfos = new ObservableCollection<ThRelationFanInfo>();
        }


    }
}
