using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThStructureCheck.ThBeamInfo.Model;

namespace ThStructureCheck.ThBeamInfo.View
{
    public partial class DataResult : UserControl
    {
        private System.Drawing.Color cellBackColor;
        private System.Drawing.Color dgvBackColor;
        private System.Drawing.Color textForeClor;
        private int rowCount = 20;

        //private int dgvDistinguishResCurrentRowIndex = -1;
        public DataResult()
        {
            InitializeComponent();
            SetColor();
            InitDistinguishResTabPage();
        }
        private void SetColor()
        {
            this.textForeClor = System.Drawing.Color.White;
            this.cellBackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.dgvBackColor = System.Drawing.Color.FromArgb(92, 92, 92);

            this.dgvDistinguishRes.BackgroundColor = this.dgvBackColor;
            this.dgvDistinguishRes.GridColor = Color.Black;
            this.dgvDistinguishRes.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvDistinguishRes.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;

            this.tabControl1.BackColor = this.dgvBackColor;
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tpDistinguishRes.BackColor = this.dgvBackColor;
        }
        private void InitDistinguishResTabPage()
        {
            this.dgvDistinguishRes.Columns.Add("index", "序号");
            this.dgvDistinguishRes.Columns.Add("code", "梁编号");
            this.dgvDistinguishRes.Columns.Add("span", "第几跨");
            this.dgvDistinguishRes.Columns.Add("spec", "截面尺寸");
            this.dgvDistinguishRes.Columns.Add("beamStirrup", "梁箍筋");
            this.dgvDistinguishRes.Columns.Add("upFullLengthTendon", "上部通长筋");
            this.dgvDistinguishRes.Columns.Add("downFullLengthTendon", "下部通长筋");
            this.dgvDistinguishRes.Columns.Add("leftOrUpSupportUpPartSteelBar", "左(上)支座上部钢筋");
            this.dgvDistinguishRes.Columns.Add("rightOrDownSupportUpPartSteelBar", "右(下)支座上部钢筋");
            this.dgvDistinguishRes.Columns.Add("beamSideSteelBar", "梁侧钢筋");
            this.dgvDistinguishRes.Columns.Add("beamTopElevationDiffer", "梁顶标高差值");

            int baseWidth = 100;
            for (int i = 0; i < this.dgvDistinguishRes.Columns.Count; i++)
            {
                this.dgvDistinguishRes.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dgvDistinguishRes.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                switch (this.dgvDistinguishRes.Columns[i].Name)
                {
                    case "index":
                        this.dgvDistinguishRes.Columns[i].Width = (int)(baseWidth * 0.5);
                        break;
                    case "beamStirrup":
                    case "upFullLengthTendon":
                    case "downFullLengthTendon":
                    case "leftOrUpSupportUpPartSteelBar":
                    case "rightOrDownSupportUpPartSteelBar":
                        this.dgvDistinguishRes.Columns[i].Width = (int)(baseWidth * 2.0);
                        break;
                    case "beamSideSteelBar":
                        this.dgvDistinguishRes.Columns[i].Width = (int)(baseWidth * 1.5);
                        break;
                    default:
                        this.dgvDistinguishRes.Columns[i].Width = baseWidth;
                        break;
                }
            }
            this.dgvDistinguishRes.RowHeadersVisible = false;
            this.dgvDistinguishRes.AllowUserToAddRows = false;
            this.dgvDistinguishRes.AllowUserToOrderColumns = false;
            this.dgvDistinguishRes.AllowUserToResizeRows = false;
        }
        public void UpdateDgvDistinguishRes(List<BeamDistinguishInfo> beamDistinguishInfos)
        {
            this.dgvDistinguishRes.Rows.Clear();
            int index = 1;
            foreach (BeamDistinguishInfo beamInf in beamDistinguishInfos)
            {
                int rowIndex = this.dgvDistinguishRes.Rows.Add();
                this.dgvDistinguishRes.Rows[rowIndex].Cells["index"].Value = index++;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["code"].Value = beamInf.Code;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["span"].Value = beamInf.Span;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["spec"].Value = beamInf.Spec;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["beamStirrup"].Value = beamInf.BeamStirrup;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["upFullLengthTendon"].Value = beamInf.UpFullLengthTendon;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["downFullLengthTendon"].Value = beamInf.DownFullLengthTendon;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["leftOrUpSupportUpPartSteelBar"].Value = beamInf.LeftOrUpSupportUpPartSteelBar;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["rightOrDownSupportUpPartSteelBar"].Value = beamInf.RightOrDownSupportUpPartSteelBar;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["beamSideSteelBar"].Value = beamInf.BeamSideSteelBar;
                this.dgvDistinguishRes.Rows[rowIndex].Cells["beamTopElevationDiffer"].Value = beamInf.BeamTopElevationDiffer;
                this.dgvDistinguishRes.Rows[rowIndex].Tag = beamInf;
            }
            for (int i = 0; i < this.dgvDistinguishRes.Rows.Count;i++)
            {
                for (int j = 0; j < this.dgvDistinguishRes.Rows[i].Cells.Count; j++)
                {
                    this.dgvDistinguishRes.Rows[i].Cells[j].Style.BackColor= this.cellBackColor;
                    this.dgvDistinguishRes.Rows[i].Cells[j].Style.ForeColor = this.textForeClor;
                    if(j==0)
                    {
                        this.dgvDistinguishRes.Rows[i].Cells[j].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }
            }
            if (this.dgvDistinguishRes.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvDistinguishRes.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvDistinguishRes.Rows.Add();
                    for(int j=0;j< this.dgvDistinguishRes.Columns.Count;j++)
                    {
                        this.dgvDistinguishRes.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvDistinguishRes.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvDistinguishRes.Rows[rowIndex].Cells[j].Value = "";
                    }
                    this.dgvDistinguishRes.Rows[rowIndex].Tag = null;
                }
            }
        }
        
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            SolidBrush brush = new SolidBrush(this.dgvBackColor);
            Rectangle mainRec = tabControl1.ClientRectangle;
            e.Graphics.FillRectangle(brush, mainRec);
            e.Graphics.FillRectangle(brush, e.Bounds);

            SolidBrush white = new SolidBrush(Color.White);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                Rectangle rec = tabControl1.GetTabRect(i);
                Rectangle newRec = new Rectangle(rec.X-2, rec.Y+2, rec.Width+4, rec.Height + 4);
                e.Graphics.FillRectangle(brush, newRec);
                e.Graphics.DrawString(tabControl1.TabPages[i].Text, new Font("宋体", 9),
                    white, rec, stringFormat);
            }
        }

        private void dgvDistinguishRes_Paint(object sender, PaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Pen pen = new Pen(this.dgvBackColor);
            e.Graphics.DrawRectangle(pen,new Rectangle(0, 0, dgv.Width, dgv.Height));
        }
       
        private void dgvDistinguishRes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            var obj= this.dgvDistinguishRes.Rows[e.RowIndex].Tag;
            if(obj==null)
            {
                return;
            }
            if(obj is BeamDistinguishInfo beamDistinguishInfo)
            {
                beamDistinguishInfo.DrawOutline();
            }
        }
       
        private void dgvDistinguishRes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.RowIndex<0 || e.ColumnIndex<0)
            {
                return;
            }
        }
        private bool CheckTabIsActive(string tabName)
        {
            if(this.tabControl1.SelectedTab.Name== tabName)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
