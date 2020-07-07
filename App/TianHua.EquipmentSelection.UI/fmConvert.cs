using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection.UI
{
    public partial class fmConvert : XtraForm
    {
        private DataManager m_DataMgr;

        public fmConvert()
        {
            InitializeComponent();
        }

        private void fmConvert_Load(object sender, EventArgs e)
        {
            m_DataMgr = new DataManager();
        }

        private void Panel_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.RestoreDirectory = true;
            dlg.Filter = "Excel File(*.xlsx)|*.xlsx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadExcelAsync(dlg.FileName);
            }
        }


        /// <summary>
        /// 使用BackgroundWorker加载Excel文件，使用UI中的Options设置
        /// </summary>
        /// <param name="_Path">Excel文件路径</param>
        private void LoadExcelAsync(string _Path)
        {
            var mCurrentXlsx = _Path;
            var FileName = System.IO.Path.GetFileName(_Path);
            this.labelExcelFile.Text = FileName;
            this.BGWorker.RunWorkerAsync(_Path);
        }

        private void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (this.m_DataMgr)
            {
                this.m_DataMgr.LoadExcel((string)e.Argument);
            }

        }

        private void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.labelExcelFile.Text = "Open you .xlsx file here!";
                XtraMessageBox.Show(e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lock (this.m_DataMgr)
            {
                if (m_DataMgr.m_ListFanSelection != null && m_DataMgr.m_ListFanSelection.Count > 0)
                {
                    CheckFanSelection.Enabled = true;
                    CheckFanSelection.Checked = true;
                }
                else
                {
                    CheckFanSelection.Enabled = false;
                    CheckFanSelection.Checked = false;
                }

                if (m_DataMgr.m_ListAxialFanSelection != null && m_DataMgr.m_ListAxialFanSelection.Count > 0)
                {
                    CheckAxialFan.Enabled = true;
                    CheckAxialFan.Checked = true;
                }
                else
                {
                    CheckAxialFan.Enabled = false;
                    CheckAxialFan.Checked = false;
                }

                if (m_DataMgr.m_ListFanParameters != null && m_DataMgr.m_ListFanParameters.Count > 0)
                {

                    CheckFanParameters.Enabled = true;
                    CheckFanParameters.Checked = true;
                }
                else
                {
                    CheckFanParameters.Enabled = false;
                    CheckFanParameters.Checked = false;
                }

                if (m_DataMgr.m_ListAxialFanParameters != null && m_DataMgr.m_ListAxialFanParameters.Count > 0)
                {

                    CheckAxialFanPara.Enabled = true;
                    CheckAxialFanPara.Checked = true;
                }
                else
                {
                    CheckAxialFanPara.Enabled = false;
                    CheckAxialFanPara.Checked = false;
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {

            if(!CheckFanSelection.Checked && !CheckAxialFan.Checked && !CheckFanParameters.Checked && !CheckAxialFanPara.Checked)
            {
                XtraMessageBox.Show("请正确选择需要转换的 .Xlsx 数据!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CheckFanSelection.Checked)
            {
                var _Json = FuncJson.Serialize(m_DataMgr.m_ListFanSelection);
                m_DataMgr.m_Json.SaveToFile(System.Environment.CurrentDirectory + @"\FanSelection.json", Encoding.UTF8, _Json);
            }
            if (CheckAxialFan.Checked)
            {
                var _Json = FuncJson.Serialize(m_DataMgr.m_ListAxialFanSelection);
                m_DataMgr.m_Json.SaveToFile(System.Environment.CurrentDirectory + @"\AxialFanSelection.json", Encoding.UTF8, _Json);
            }
            if (CheckFanParameters.Checked)
            {
                var _Json = FuncJson.Serialize(m_DataMgr.m_ListFanParameters);
                m_DataMgr.m_Json.SaveToFile(System.Environment.CurrentDirectory + @"\FanParameters.json", Encoding.UTF8, _Json);
            }
            if (CheckAxialFanPara.Checked)
            {
                var _Json = FuncJson.Serialize(m_DataMgr.m_ListAxialFanParameters);
                m_DataMgr.m_Json.SaveToFile(System.Environment.CurrentDirectory + @"\AxialFanParameters.json", Encoding.UTF8, _Json);
            }

            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
