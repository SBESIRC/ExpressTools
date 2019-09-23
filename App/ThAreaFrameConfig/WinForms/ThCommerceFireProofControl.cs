﻿using DevExpress.XtraEditors;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcHelper;
using System;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThCommerceFireProofControl : XtraUserControl, IFCCommerceView
    {
        private BindingSource bindingSource;
        private ThFCCommercePresenter Presenter;
        private ThFCCommerceDbRepository DbRepository;

        public ThCommerceFireProofControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeDataBindings();
        }

        public ThFCCommerceSettings Settings
        {
            get
            {
                return DbRepository.Settings;
            }
        }

        public List<ThFireCompartment> Compartments
        {
            get
            {
                return (List<ThFireCompartment>)gridControl_fire_compartment.DataSource;
            }

            set
            {
                gridControl_fire_compartment.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository.ReloadFireCompartments();
            gridControl_fire_compartment.DataSource = Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThFCCommercePresenter(this);
            DbRepository = new ThFCCommerceDbRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
        }

        public void InitializeDataBindings()
        {
            // 创建BindingSource
            bindingSource = new BindingSource()
            {
                DataSource = Settings
            };

            // 子项编号
            textBox_sub_key.DataBindings.Add(
                "Text",
                bindingSource,
                "SubKey", 
                true, 
                DataSourceUpdateMode.OnPropertyChanged);

            // 地上层数
            textBox_storey.DataBindings.Add(
                "Text",
                bindingSource,
                "Storey",
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            // 耐火等级
            comboBox_fire_resistance.SelectedIndex = (int)Settings.Resistance;

            // 指定外框线图层
            comboBox_outer_frame.DataSource = DbRepository.Layers;
            if (DbRepository.Layers.Contains(Settings.Layers["OUTERFRAME"]))
            {
                comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];
            }
            else
            {
                comboBox_outer_frame.SelectedItem = "0";
            }

            // 指定内框线图层
            comboBox_inner_frame.DataSource = DbRepository.Layers;
            if (DbRepository.Layers.Contains(Settings.Layers["INNERFRAME"]))
            {
                comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
            }
            else
            {
                comboBox_inner_frame.SelectedItem = "0";
            }
        }

        private void gridView_fire_compartment_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }
            if (e.IsGetData)
            {
                ThFireCompartment compartment = e.Row as ThFireCompartment;
                e.Value = compartment.IsDefined ? "" : "选择";
            }
        }

        private void gridView_fire_compartment_RowClick(object sender, RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column == null)
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView gridView = sender as GridView;
            ThFireCompartment compartment = gridView.GetRow(e.RowHandle) as ThFireCompartment;
            if (!compartment.IsDefined)
            {
                // 面积框线图层名
                string name = Settings.Layers["OUTERFRAME"];

                // 选取面积框线
                using (EditorUserInteraction inter = Active.Editor.StartUserInteraction(FindForm()))
                {
                    if (Presenter.OnPickAreaFrames(compartment, name))
                    {
                        // 更新界面
                        this.Reload();
                    }
                }
            }
        }

        private void gridView_fire_compartment_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }
    }
}
