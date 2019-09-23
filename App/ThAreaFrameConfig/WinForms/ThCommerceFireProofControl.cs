using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using DevExpress.XtraEditors;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThCommerceFireProofControl : XtraUserControl, IFCCommerceView
    {
        private BindingSource bindingSource;
        private ThFCCommerceNullRepository DbRepository;
        //private ThFCCommerceDbRepository DbRepository;

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
            DbRepository = new ThFCCommerceNullRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = DbRepository.Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            DbRepository = new ThFCCommerceNullRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = DbRepository.Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
        }

        public List<string> Layers
        {
            get
            {
                var layers = new List<string>();
                foreach (var description in AcadApp.UIBindings.Collections.Layers)
                {
                    var properties = description.GetProperties();
                    layers.Add((string)properties["Name"].GetValue(description));

                }
                return layers;
            }
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
            comboBox_outer_frame.DataSource = Layers;
            if (Layers.Contains(Settings.Layers["OUTERFRAME"]))
            {
                comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];
            }
            else
            {
                comboBox_outer_frame.SelectedItem = "0";
            }

            // 指定内框线图层
            comboBox_inner_frame.DataSource = Layers;
            if (Layers.Contains(Settings.Layers["INNERFRAME"]))
            {
                comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
            }
            else
            {
                comboBox_inner_frame.SelectedItem = "0";
            }
        }
    }
}
