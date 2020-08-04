﻿using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.FanSelection.UI
{
    public partial class fmDesignData : DevExpress.XtraEditors.XtraForm
    {
        public List<FanDesignDataModel> m_ListFanDesign { get; set; }

        public FanDesignDataModel m_FanDesign { get; set; }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bSCan, int dwFlags, int dwExtraInfo);

        public string m_ActionType = string.Empty;

        public string m_Path = string.Empty;

        public double m_FilterDate = 0;

        public fmDesignData()
        {
            InitializeComponent();
        }

        public void InitForm(List<FanDesignDataModel> _ListFanDesign, string _ActionType, string _Path)
        {
            m_Path = _Path;
            m_ActionType = _ActionType;
            m_ListFanDesign = _ListFanDesign;
            if (m_ListFanDesign == null) m_ListFanDesign = new List<FanDesignDataModel>();

            if (m_ActionType == "保存")
            {
                this.Text = "保存设计数据";
                m_FanDesign = new FanDesignDataModel();
                m_FanDesign.ID = Guid.NewGuid().ToString();
                m_FanDesign.CreateDate = DateTime.Now;
                m_FanDesign.LastOperationDate = DateTime.Now;
                m_FanDesign.Name = "新建设计数据";
                //TO DO:
                m_FanDesign.LastOperationName = "S";

                m_FanDesign.Name = SetFanDesignDataName(m_FanDesign);
                m_FanDesign.Path = GetPath(m_FanDesign);
                m_ListFanDesign.Insert(0, m_FanDesign);
            }
            if (m_ActionType == "打开")
            {
                this.Text = "打开设计数据";

            }

            Gdc.DataSource = m_ListFanDesign;
            Gdc.Refresh();

            Gdv.FocusedRowHandle = 0;
            Gdv.FocusedColumn = Gdv.Columns["Name"];
            Gdv.ShowEditor();

            PicSeven_Click(null, null);
        }

        public string SetFanDesignDataName(FanDesignDataModel _FanData)
        {
            var _List = m_ListFanDesign.FindAll(p => p.Name.Contains(_FanData.Name));
            if (_List == null || _List.Count == 0) { return _FanData.Name + "(1)"; }
            for (int i = 1; i < 10000; i++)
            {
                if (i == 1)
                {
                    var _ListTemp1 = m_ListFanDesign.FindAll(p => p.Name == _FanData.Name + "(1)");
                    if (_ListTemp1 == null || _ListTemp1.Count == 0) { return _FanData.Name + "(1)"; }
                }
                else
                {
                    var _ListTemp = m_ListFanDesign.FindAll(p => p.Name == _FanData.Name + "(" + i + ")");
                    if (_ListTemp == null || _ListTemp.Count == 0) { return _FanData.Name + "(" + i + ")"; }
                }

            }
            return string.Empty;
        }


        private string GetPath(FanDesignDataModel _FanDesign)
        {
            if (_FanDesign == null || FuncStr.NullToStr(_FanDesign.Name) == string.Empty) { return string.Empty; }
            return Path.Combine(m_Path , FuncStr.NullToStr(_FanDesign.Name) + ".json");
        }

        private void fmDesignData_Load(object sender, EventArgs e)
        {
            if (m_ActionType == "保存")
                keybd_event((byte)Keys.Tab, 0, 0, 0);
        }

        private void ComBoxName_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (m_ListFanDesign == null || m_ListFanDesign.Count == 0) return;
            var _FanDesign = Gdv.GetRow(Gdv.FocusedRowHandle) as FanDesignDataModel;
            if (XtraMessageBox.Show(" 设计数据[" + _FanDesign.Name + "]将被删除，是否继续？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                File.Delete(_FanDesign.Path);
                m_ListFanDesign.Remove(_FanDesign);
                Gdc.DataSource = m_ListFanDesign;
                Gdv.RefreshData();
            }


        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Gdv.PostEditor();
            if (m_FanDesign != null) m_FanDesign.LastOperationDate = DateTime.Now;
            if (!Directory.Exists(m_Path))
            {
                Directory.CreateDirectory(m_Path);
            }

            var _Json = FuncJson.Serialize(m_ListFanDesign);

            JsonExporter.Instance.SaveToFile(Path.Combine(m_Path, "FanDesignData.json"), Encoding.UTF8, _Json);

            if (m_ActionType == "打开")
            {
                m_FanDesign = Gdv.GetRow(Gdv.FocusedRowHandle) as FanDesignDataModel;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (m_ActionType == "保存")
            {
                m_ListFanDesign.Remove(m_FanDesign);
            }

        }

        private void Gdv_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Name")
            {
                var _FanDesign = Gdv.GetRow(Gdv.FocusedRowHandle) as FanDesignDataModel;
                _FanDesign.Path = GetPath(m_FanDesign);
            }
        }

        private void TxtSearch_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            Filter(FuncStr.NullToStr(e.NewValue));
        }

        private void Filter(string _FilterStr)
        {

            var _DateTime = DateTime.Now;

            var _FilterString = @" Name LIKE '%" + _FilterStr + "%'";

            if (m_FilterDate != 0)
                _FilterString += @"  And ( LastOperationDate  >  '" + _DateTime.AddDays(m_FilterDate) + "')";

            //if (m_FilterDate != 0)
            //    _FilterString += @"  And ( LastOperationDate  Between '" + _DateTime.AddDays(m_FilterDate) + "' And '" + _DateTime + "')";


            (Gdv as ColumnView).ActiveFilterString = _FilterString;
        }

        private void PicAll_Click(object sender, EventArgs e)
        {
            PicAll.Image = Properties.Resources.全部_选中;
            PicSeven.Image = Properties.Resources._7天_未选中;
            PicThree.Image = Properties.Resources._3天_未选中;
            m_FilterDate = 0;
            Filter(FuncStr.NullToStr(TxtSearch.Text));
        }

        private void PicSeven_Click(object sender, EventArgs e)
        {
            PicAll.Image = Properties.Resources.全部_未选中;
            PicSeven.Image = Properties.Resources._7天_选中;
            PicThree.Image = Properties.Resources._3天_未选中;
            m_FilterDate = -7; ;
            Filter(FuncStr.NullToStr(TxtSearch.Text));
        }

        private void PicThree_Click(object sender, EventArgs e)
        {
            PicAll.Image = Properties.Resources.全部_未选中;
            PicSeven.Image = Properties.Resources._7天_未选中;
            PicThree.Image = Properties.Resources._3天_选中;
            m_FilterDate = -3;
            Filter(FuncStr.NullToStr(TxtSearch.Text));

        }
    }
}
