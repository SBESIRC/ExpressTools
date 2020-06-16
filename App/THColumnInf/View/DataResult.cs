using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThColumnInfo.Validate;

namespace ThColumnInfo.View
{
    public partial class DataResult : UserControl
    {
        private IDataSource ds;
        private ThSpecificationValidate thSpecificationValidate;
        private ThCalculationValidate thCalculationValidate;
        private TreeNode innerframeNode;
        private TreeNode currentNode;
        private string flrName = "";
        private System.Drawing.Color cellBackColor;
        private System.Drawing.Color dgvBackColor;
        private System.Drawing.Color textForeClor;

        private int dgvColumnTableCurrentRowIndex = -1;
        private int dgvSpecificationCurrentRowIndex = -1;
        private int dgvCalculationCurrentRowIndex = -1;

        private string columnTableTabName = "";
        private string specificationTabName = "";
        private string calculationTabName = "";

        private int rowCount = 20;

        public DataResult(IDataSource ds, ThSpecificationValidate tsv,ThCalculationValidate tcv,TreeNode node)
        {
            this.ds = ds;
            this.thSpecificationValidate = tsv;
            this.thCalculationValidate = tcv;
            this.currentNode = node;
            this.innerframeNode = CheckPalette._checkResult.TraverseInnerFrameRoot(node);
            InitializeComponent();
            SetColor();
            InitDataGridView1();
            InitDataGridView2();
            InitDataGridView3();
            UpdateData();
            this.columnTableTabName = this.tabControl1.TabPages[0].Name;
            this.specificationTabName = this.tabControl1.TabPages[1].Name;
            this.calculationTabName = this.tabControl1.TabPages[2].Name;
        }
        private void SetColor()
        {
            this.textForeClor = System.Drawing.Color.White;
            this.cellBackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.dgvBackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.dgvColumnTable.BackgroundColor = this.dgvBackColor;
            this.dgvSpecificationRes.BackgroundColor = this.dgvBackColor;
            this.dgvCalculationRes.BackgroundColor = this.dgvBackColor;

            this.dgvColumnTable.GridColor = Color.Black;
            this.dgvSpecificationRes.GridColor = Color.Black;
            this.dgvCalculationRes.GridColor = Color.Black;
            this.dgvColumnTable.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvSpecificationRes.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvCalculationRes.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvColumnTable.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;
            this.dgvSpecificationRes.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;
            this.dgvCalculationRes.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;

            this.tabControl1.BackColor = this.dgvBackColor;
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabPage1.BackColor = this.dgvBackColor;
            this.tabPage2.BackColor = this.dgvBackColor;
            this.tabPage3.BackColor = this.dgvBackColor;
        }
        public void UpdateData(IDataSource ds, ThSpecificationValidate tsv, ThCalculationValidate tcv, TreeNode node)
        {
            this.ds = ds;
            this.thSpecificationValidate = tsv;
            this.thCalculationValidate = tcv;
            this.currentNode = node;
            this.innerframeNode = CheckPalette._checkResult.TraverseInnerFrameRoot(node);
            UpdateData();
        }
        public void ClearDataGridView()
        {
            this.dgvColumnTable.Rows.Clear();
            this.dgvSpecificationRes.Rows.Clear();
            this.dgvCalculationRes.Rows.Clear();
        }
        private void UpdateData()
        {
            this.flrName = "";
            if(this.innerframeNode!=null && this.innerframeNode.Tag!=null && this.innerframeNode.Tag.GetType()==typeof(ThStandardSign))
            {

                ThStandardSign thStandardSign = this.innerframeNode.Tag as ThStandardSign;
                this.flrName = thStandardSign.InnerFrameName;
            }
            AddDataToDataGridView1();
            AddDataToDataGridView2();
            AddDataToDataGridView3();
        }
        private void AddDataToDataGridView1()
        {
            this.dgvColumnTable.Rows.Clear();
            bool findCorrectNode = CheckPalette._checkResult.TraverseDataCorrectNode(this.currentNode);
            if(findCorrectNode)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.GetDataCorrectColumnInfs(this.currentNode);
                foreach (ColumnInf columnInf in correctColumnInfs)
                {
                    ColumnTableRecordInfo ctri = this.ds.ColumnTableRecordInfos.Where(j => j.Code == columnInf.Code).FirstOrDefault();
                    if(ctri==null)
                    {
                        continue;
                    }
                    int rowIndex = this.dgvColumnTable.Rows.Add();
                    this.dgvColumnTable.Rows[rowIndex].Cells["code"].Value = columnInf.Code;
                    this.dgvColumnTable.Rows[rowIndex].Cells["code"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["code"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["subCode"].Value = columnInf.Text;
                    this.dgvColumnTable.Rows[rowIndex].Cells["subCode"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["subCode"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["spec"].Value = ctri.Spec;
                    this.dgvColumnTable.Rows[rowIndex].Cells["spec"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["spec"].Style.ForeColor = this.textForeClor;

                    //this.dgvColumnTable.Rows[rowIndex].Cells["all"].Value = ctri.Replace132(ctri.AllLongitudinalReinforcement);
                    //this.dgvColumnTable.Rows[rowIndex].Cells["all"].Style.BackColor = this.cellBackColor;
                    //this.dgvColumnTable.Rows[rowIndex].Cells["all"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["corner"].Value = ctri.Replace132(ctri.AngularReinforcement);
                    this.dgvColumnTable.Rows[rowIndex].Cells["corner"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["corner"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["bSide"].Value = ctri.Replace132(ctri.BEdgeSideMiddleReinforcement);
                    this.dgvColumnTable.Rows[rowIndex].Cells["bSide"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["bSide"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["hside"].Value = ctri.Replace132(ctri.HEdgeSideMiddleReinforcement);
                    this.dgvColumnTable.Rows[rowIndex].Cells["hside"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["hside"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["hooping"].Value = ctri.Replace132(ctri.HoopReinforcement);
                    this.dgvColumnTable.Rows[rowIndex].Cells["hooping"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["hooping"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Cells["hoopType"].Value = ctri.HoopReinforcementTypeNumber;
                    this.dgvColumnTable.Rows[rowIndex].Cells["hoopType"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["hoopType"].Style.ForeColor = this.textForeClor;

                    this.dgvColumnTable.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                }
            }
            if (this.dgvColumnTable.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvColumnTable.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvColumnTable.Rows.Add();
                    for(int j=0;j< this.dgvColumnTable.Columns.Count;j++)
                    {
                        this.dgvColumnTable.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvColumnTable.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvColumnTable.Rows[rowIndex].Cells[j].Value = "";
                    }
                    this.dgvColumnTable.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                }
            }
        }
        private void AddDataToDataGridView2()
        {
            this.dgvSpecificationRes.Rows.Clear();
            bool findCorrectNode = CheckPalette._checkResult.TraverseDataCorrectNode(this.currentNode);
            if (findCorrectNode && this.thSpecificationValidate != null)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.GetDataCorrectColumnInfs(this.currentNode);
                foreach (ColumnInf columnInf in correctColumnInfs)
                {
                    var values = thSpecificationValidate.ColumnValidResultDic.Where(
                        i => i.Key.Code == columnInf.Code && i.Key.Text == columnInf.Text).Select(i => i.Value).First();
                    if (values == null || values.Count == 0)
                    {
                        continue;
                    }
                    int errorIndex = values.IndexOf("XXXXXX");
                    if (errorIndex == 0)
                    {
                        values.RemoveAt(0);
                        errorIndex = -1;
                    }
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (errorIndex == i)
                        {
                            continue;
                        }
                        int rowIndex = this.dgvSpecificationRes.Rows.Add();
                        if (i == 0)
                        {
                            this.dgvSpecificationRes.Rows[rowIndex].Cells["code"].Value = columnInf.Text;
                            this.dgvSpecificationRes.Rows[rowIndex].Cells["flrName"].Value = this.flrName;
                        }
                        this.dgvSpecificationRes.Rows[rowIndex].Cells["detail"].Value = values[i];

                        this.dgvSpecificationRes.Rows[rowIndex].Cells["code"].Style.BackColor = this.cellBackColor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells["code"].Style.ForeColor = this.textForeClor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells["flrName"].Style.BackColor = this.cellBackColor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells["flrName"].Style.ForeColor = this.textForeClor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells["detail"].Style.BackColor = this.cellBackColor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = this.textForeClor;

                        if (i < errorIndex)
                        {
                            this.dgvSpecificationRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = Color.Red;
                        }
                        this.dgvSpecificationRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                    }
                }
            }
            
            if (this.dgvSpecificationRes.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvSpecificationRes.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvSpecificationRes.Rows.Add();
                    for (int j = 0; j < this.dgvSpecificationRes.Columns.Count; j++)
                    {
                        this.dgvSpecificationRes.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvSpecificationRes.Rows[rowIndex].Cells[j].Value = "";
                    }
                    this.dgvSpecificationRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                }
            }
        }
        private void AddDataToDataGridView3()
        {
            this.dgvCalculationRes.Rows.Clear();
            bool findCorrectNode = CheckPalette._checkResult.TraverseDataCorrectNode(this.currentNode);
            if (findCorrectNode && this.thCalculationValidate != null)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.GetDataCorrectColumnInfs(this.currentNode);
                foreach (ColumnInf columnInf in correctColumnInfs)
                {
                    var values = this.thCalculationValidate.ColumnValidateResultDic.Where(i => i.Key.ModelColumnInfs.Count == 1 &&
                    i.Key.ModelColumnInfs[0].Code == columnInf.Code && i.Key.ModelColumnInfs[0].Text == columnInf.Text).Select(i => i.Value).First();
                    if (values == null || values.Count == 0)
                    {
                        continue;
                    }
                    int errorIndex = values.IndexOf("XXXXXX");
                    if(errorIndex==0)
                    {
                        values.RemoveAt(0);
                        errorIndex = -1;
                    }
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (errorIndex == i)
                        {
                            continue;
                        }
                        int rowIndex = this.dgvCalculationRes.Rows.Add();
                        if (i == 0)
                        {
                            this.dgvCalculationRes.Rows[rowIndex].Cells["code"].Value = columnInf.Text;
                            this.dgvCalculationRes.Rows[rowIndex].Cells["flrName"].Value = this.flrName;
                        }
                        this.dgvCalculationRes.Rows[rowIndex].Cells["detail"].Value = values[i];

                        this.dgvCalculationRes.Rows[rowIndex].Cells["code"].Style.BackColor = this.cellBackColor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells["code"].Style.ForeColor = this.textForeClor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells["flrName"].Style.BackColor = this.cellBackColor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells["flrName"].Style.ForeColor = this.textForeClor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells["detail"].Style.BackColor = this.cellBackColor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = this.textForeClor;
                        if (i < errorIndex)
                        {
                            this.dgvCalculationRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = Color.Red;
                        }
                        this.dgvCalculationRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                    }
                }
            }
            if (this.dgvCalculationRes.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvCalculationRes.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvCalculationRes.Rows.Add();
                    for (int j = 0; j < this.dgvCalculationRes.Columns.Count; j++)
                    {
                        this.dgvCalculationRes.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvCalculationRes.Rows[rowIndex].Cells[j].Value = "";
                    }
                    this.dgvCalculationRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                }
            }
        }
        private void InitDataGridView1()
        {
            this.dgvColumnTable.Columns.Add("code","柱编号");
            this.dgvColumnTable.Columns.Add("subCode", "子编号");
            this.dgvColumnTable.Columns["subCode"].HeaderCell.Style.ForeColor = Color.White;

            this.dgvColumnTable.Columns.Add("spec","bxh");
            //this.dgvColumnTable.Columns.Add("all", "全部纵筋");
            this.dgvColumnTable.Columns.Add("corner", "角筋");
            this.dgvColumnTable.Columns.Add("bSide", "b边一侧中部筋");
            this.dgvColumnTable.Columns.Add("hside", "h边一侧中部筋");
            this.dgvColumnTable.Columns.Add("hooping", "箍筋");
            this.dgvColumnTable.Columns.Add("hoopType", "箍筋类型号");

            int baseWidth = 80;
            for(int i=0;i<this.dgvColumnTable.Columns.Count;i++)
            {
                this.dgvColumnTable.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dgvColumnTable.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                switch (this.dgvColumnTable.Columns[i].Name)
                {
                    case "all":
                        this.dgvColumnTable.Columns[i].Width = (int)(baseWidth*2.5);
                        break;
                    case "bSide":
                    case "hside":
                        this.dgvColumnTable.Columns[i].Width = (int)(baseWidth * 1.75);
                        break;
                    case "hooping":
                        this.dgvColumnTable.Columns[i].Width = (int)(baseWidth * 1.5);
                        break;
                    case "limbNum":
                        this.dgvColumnTable.Columns[i].Width = (int)(baseWidth * 1.5);
                        break;
                    case "hoopType":
                        this.dgvColumnTable.Columns[i].Width = (int)(baseWidth * 1.5);
                        break;
                    default:
                        this.dgvColumnTable.Columns[i].Width = baseWidth;
                        break;
                }
            }
            this.dgvColumnTable.RowHeadersVisible = false;
            this.dgvColumnTable.AllowUserToAddRows = false;
            this.dgvColumnTable.AllowUserToOrderColumns = false;
            this.dgvColumnTable.AllowUserToResizeRows=false;
        }
        private void InitDataGridView2()
        {
            this.dgvSpecificationRes.Columns.Add("flrName", "楼层");
            this.dgvSpecificationRes.Columns.Add("code", "编号");
            this.dgvSpecificationRes.Columns.Add("detail", "详细");
            this.dgvSpecificationRes.Columns["flrName"].HeaderCell.Style.BackColor = this.cellBackColor;
            this.dgvSpecificationRes.Columns["code"].HeaderCell.Style.BackColor = this.cellBackColor;
            this.dgvSpecificationRes.Columns["detail"].HeaderCell.Style.BackColor = this.cellBackColor;

            int baseWidth = 80;
            for (int i = 0; i < this.dgvSpecificationRes.Columns.Count; i++)
            {
                this.dgvSpecificationRes.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dgvSpecificationRes.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                switch (this.dgvSpecificationRes.Columns[i].Name)
                {
                    case "detail":
                        this.dgvSpecificationRes.Columns[i].Width = (int)(baseWidth * 6);
                        break;
                    case "flrName":
                        this.dgvSpecificationRes.Columns[i].Width = (int)(baseWidth * 3);
                        break;
                    default:
                        this.dgvSpecificationRes.Columns[i].Width = baseWidth;
                        break;
                }
            }
            this.dgvSpecificationRes.RowHeadersVisible = false;
            this.dgvSpecificationRes.AllowUserToAddRows = false;
            this.dgvSpecificationRes.AllowUserToOrderColumns = false;
            this.dgvSpecificationRes.AllowUserToResizeRows = false;
        }
        private void InitDataGridView3()
        {
            this.dgvCalculationRes.Columns.Add("flrName", "楼层");
            this.dgvCalculationRes.Columns.Add("code", "编号");
            this.dgvCalculationRes.Columns.Add("detail", "详细");
            this.dgvCalculationRes.Columns["flrName"].HeaderCell.Style.BackColor = this.cellBackColor;
            this.dgvCalculationRes.Columns["code"].HeaderCell.Style.BackColor = this.cellBackColor;
            this.dgvCalculationRes.Columns["detail"].HeaderCell.Style.BackColor = this.cellBackColor;
            int baseWidth = 80;
            for (int i = 0; i < this.dgvCalculationRes.Columns.Count; i++)
            {
                this.dgvCalculationRes.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dgvCalculationRes.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                switch (this.dgvCalculationRes.Columns[i].Name)
                {
                    case "detail":
                        this.dgvCalculationRes.Columns[i].Width = (int)(baseWidth * 6);
                        break;
                    case "flrName":
                        this.dgvCalculationRes.Columns[i].Width = (int)(baseWidth * 3);
                        break;
                    default:
                        this.dgvCalculationRes.Columns[i].Width = baseWidth;
                        break;
                }
            }
            this.dgvCalculationRes.RowHeadersVisible = false;
            this.dgvCalculationRes.AllowUserToAddRows = false;
            this.dgvCalculationRes.AllowUserToOrderColumns = false;
            this.dgvCalculationRes.AllowUserToResizeRows = false;
        }
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            SolidBrush brush = new SolidBrush(this.dgvBackColor);
            Rectangle mainRec = tabControl1.ClientRectangle;
            e.Graphics.FillRectangle(brush, mainRec);

            SolidBrush white = new SolidBrush(Color.White);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                Rectangle rec = tabControl1.GetTabRect(i);
                e.Graphics.FillRectangle(brush, rec);
                e.Graphics.DrawString(tabControl1.TabPages[i].Text, new Font("宋体", 9),
                    white, rec, stringFormat);
            }
        }

        private void dgvColumnTable_Paint(object sender, PaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Pen pen = new Pen(this.dgvBackColor);
            e.Graphics.DrawRectangle(pen,new Rectangle(0,0,dgv.Width,dgv.Height));
        }

        private void dgvSpecificationRes_Paint(object sender, PaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Pen pen = new Pen(this.dgvBackColor);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, dgv.Width, dgv.Height));
        }
        private void dgvCalculationRes_Paint(object sender, PaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Pen pen = new Pen(this.dgvBackColor);
            e.Graphics.DrawRectangle(pen,new Rectangle(0, 0, dgv.Width, dgv.Height));
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            try
            {
                string columnName = this.dgvColumnTable.Columns[e.ColumnIndex].Name;
                if (columnName != "code" && columnName != "subCode")
                {
                    return;
                }
                this.dgvColumnTableCurrentRowIndex = e.RowIndex;
                string codeText = this.dgvColumnTable.Rows[e.RowIndex].Cells["code"].Value.ToString();
                string subcodeText = this.dgvColumnTable.Rows[e.RowIndex].Cells["subCode"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvColumnTable.Rows[e.RowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.FindTreeCode(thStandardSign.InnerFrameName, GetFindNodeMode(columnName), codeText, subcodeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.ShowSelectNodeFrameIds(findTreeCode);
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "dataGridView1_CellDoubleClick");
            }
        }
        private FindNodeMode GetFindNodeMode(string columnName)
        {
            FindNodeMode findNodeMode = FindNodeMode.None;
            if (columnName == "code")
            {
                findNodeMode = FindNodeMode.Code;
            }
            else if (columnName == "subCode")
            {
                findNodeMode = FindNodeMode.SubCode;
            }
            else if(columnName == "flrName")
            {
                findNodeMode = FindNodeMode.InnerFrame;
            }
            return findNodeMode;
        }
        private void dgvSpecificationRes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            try
            {
                string columnName = this.dgvSpecificationRes.Columns[e.ColumnIndex].Name;
                if (columnName != "code" && columnName != "flrName")
                {
                    return;
                }
                if(columnName== "code")
                {
                    columnName = "subCode";
                }
                this.dgvSpecificationCurrentRowIndex = e.RowIndex;
                string codeText = this.dgvSpecificationRes.Rows[e.RowIndex].Cells["code"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvSpecificationRes.Rows[e.RowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                //搜索叶子节点，列名不能设为code

                TreeNode findTreeCode = CheckPalette._checkResult.FindTreeCode(thStandardSign.InnerFrameName, GetFindNodeMode(columnName), "", codeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.ShowSelectNodeFrameIds(findTreeCode);
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "dgvSpecificationRes_CellDoubleClick");
            }
        }
        private void dgvCalculationRes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            try
            {
                string columnName = this.dgvCalculationRes.Columns[e.ColumnIndex].Name;
                if (columnName != "code" && columnName != "flrName")
                {
                    return;
                }
                if (columnName == "code")
                {
                    columnName = "subCode";
                }
                this.dgvCalculationCurrentRowIndex = e.RowIndex;
                string codeText = this.dgvCalculationRes.Rows[e.RowIndex].Cells["code"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvCalculationRes.Rows[e.RowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.FindTreeCode(thStandardSign.InnerFrameName, GetFindNodeMode(columnName), "", codeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.ShowSelectNodeFrameIds(findTreeCode);
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "dgvCalculationRes_CellDoubleClick");
            }
        }
        private void HideDgvCalculationRes(int rowIndex)
        {
            if (rowIndex < 0)
            {
                return;
            }
            if(this.dgvCalculationRes.Rows[rowIndex].Cells["code"].Value!=null)
            {
                string codeText = this.dgvCalculationRes.Rows[rowIndex].Cells["code"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvCalculationRes.Rows[rowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.FindTreeCode(thStandardSign.InnerFrameName, FindNodeMode.SubCode, "", codeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.HideTotalFrameIds(findTreeCode);
                }
            }
            else
            {
                if(this.innerframeNode!=null)
                {
                    CheckPalette._checkResult.HideTotalFrameIds(this.innerframeNode);
                }
            }
        }
        private void dgvColumnTable_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            if (!CheckTabIsActive(this.columnTableTabName))
            {
                HideDgvSpecificationRes(this.dgvSpecificationCurrentRowIndex);
                HideDgvCalculationRes(this.dgvCalculationCurrentRowIndex);
            }
            if (this.dgvColumnTableCurrentRowIndex >= 0) 
            {
                if (this.dgvColumnTableCurrentRowIndex != e.RowIndex)
                {
                    HideDgvColumnTable(dgvColumnTableCurrentRowIndex);
                }
            }
            this.dgvColumnTableCurrentRowIndex = e.RowIndex;
            if(this.dgvColumnTable.Rows[e.RowIndex].Tag!=null)
            {
                ThStandardSign thStandardSign = this.dgvColumnTable.Rows[e.RowIndex].Tag as ThStandardSign;
                object value=this.dgvColumnTable.Rows[e.RowIndex].Cells["subCode"].Value;
                if(value!=null)
                {
                    CheckPalette._checkResult.SelectTreeNode(thStandardSign.InnerFrameName, value.ToString());
                }
            }
        }

        private void dgvCalculationRes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.RowIndex<0 || e.ColumnIndex<0)
            {
                return;
            }
            if (!CheckTabIsActive(this.calculationTabName))
            {
                HideDgvColumnTable(this.dgvColumnTableCurrentRowIndex);
                HideDgvSpecificationRes(this.dgvSpecificationCurrentRowIndex);
            }
            if (dgvCalculationCurrentRowIndex >= 0)
            {
                if (this.dgvCalculationCurrentRowIndex != e.RowIndex)
                {
                    HideDgvCalculationRes(dgvCalculationCurrentRowIndex);
                }
            }
            this.dgvCalculationCurrentRowIndex = e.RowIndex;
            if (this.dgvCalculationRes.Rows[e.RowIndex].Tag != null)
            {
                ThStandardSign thStandardSign = this.dgvCalculationRes.Rows[e.RowIndex].Tag as ThStandardSign;
                object value = this.dgvCalculationRes.Rows[e.RowIndex].Cells["code"].Value;
                if (value != null)
                {
                    CheckPalette._checkResult.SelectTreeNode(thStandardSign.InnerFrameName, value.ToString());
                }
            }
        }

        private void dgvSpecificationRes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            if (!CheckTabIsActive(this.specificationTabName))
            {
                HideDgvColumnTable(this.dgvColumnTableCurrentRowIndex);
                HideDgvCalculationRes(this.dgvCalculationCurrentRowIndex);
            }
            if (this.dgvSpecificationCurrentRowIndex >= 0)
            {
                if (this.dgvSpecificationCurrentRowIndex != e.RowIndex)
                {
                    HideDgvSpecificationRes(dgvSpecificationCurrentRowIndex);
                }
            }
            this.dgvSpecificationCurrentRowIndex = e.RowIndex;
            if (this.dgvSpecificationRes.Rows[e.RowIndex].Tag != null)
            {
                ThStandardSign thStandardSign = this.dgvSpecificationRes.Rows[e.RowIndex].Tag as ThStandardSign;
                object value = this.dgvSpecificationRes.Rows[e.RowIndex].Cells["code"].Value;
                if (value != null)
                {
                    CheckPalette._checkResult.SelectTreeNode(thStandardSign.InnerFrameName, value.ToString());
                }
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
        private void HideDgvColumnTable(int rowIndex)
        {
            if (rowIndex < 0)
            {
                return;
            }
            if(this.dgvColumnTable.Rows[rowIndex].Cells["code"].Value==null &&
                this.dgvColumnTable.Rows[rowIndex].Cells["subCode"].Value==null
                )
            {
                if (this.innerframeNode != null)
                {
                    CheckPalette._checkResult.HideTotalFrameIds(this.innerframeNode);
                }
            }
            else
            {
                string codeText = this.dgvColumnTable.Rows[rowIndex].Cells["code"].Value.ToString();
                string subcodeText = this.dgvColumnTable.Rows[rowIndex].Cells["subCode"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvColumnTable.Rows[rowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.FindTreeCode(thStandardSign.InnerFrameName, FindNodeMode.Code, codeText, subcodeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.HideTotalFrameIds(findTreeCode);
                }
            }
        }
        private void HideDgvSpecificationRes(int rowIndex)
        {
            if (rowIndex < 0)
            {
                return;
            }
            if(this.dgvSpecificationRes.Rows[rowIndex].Cells["code"].Value!=null)
            {
                string codeText = this.dgvSpecificationRes.Rows[rowIndex].Cells["code"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvSpecificationRes.Rows[rowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.FindTreeCode(thStandardSign.InnerFrameName, FindNodeMode.SubCode, "", codeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.HideTotalFrameIds(findTreeCode);
                }
            }
            else
            {
                if(this.innerframeNode!=null)
                {
                    CheckPalette._checkResult.HideTotalFrameIds(this.innerframeNode);
                }
            }
        }
        public void SelectDataGridViewRow(ColumnInf columnInf,string innerName)
        {
            SelectDgvColumnTableRow(columnInf);
            SelectDgvSpecificationRow(columnInf, innerName);
            SelectDgvCalculationRow(columnInf, innerName);
        }
        private void SelectDgvColumnTableRow(ColumnInf columnInf)
        {
            foreach(DataGridViewRow dgvRow in this.dgvColumnTable.Rows)
            {
                if(dgvRow.Cells["code"].Value==null || dgvRow.Cells["subCode"].Value==null)
                {
                    continue;
                }
                if(dgvRow.Cells["code"].Value.ToString()== columnInf.Code &&
                    dgvRow.Cells["subCode"].Value.ToString() == columnInf.Text)
                {
                    dgvRow.Selected = true;
                    dgvColumnTable.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    break;
                }
            }
        }
        private void SelectDgvSpecificationRow(ColumnInf columnInf, string innerName)
        {
            foreach (DataGridViewRow dgvRow in this.dgvSpecificationRes.Rows)
            {
                if (dgvRow.Cells["flrName"].Value == null || dgvRow.Cells["code"].Value == null)
                {
                    continue;
                }
                if (dgvRow.Cells["flrName"].Value.ToString() == innerName && 
                    dgvRow.Cells["code"].Value.ToString() == columnInf.Text
                   )
                {
                    dgvRow.Selected = true;
                    dgvSpecificationRes.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    break;
                }
            }
        }
        private void SelectDgvCalculationRow(ColumnInf columnInf, string innerName)
        {
            foreach (DataGridViewRow dgvRow in this.dgvCalculationRes.Rows)
            {
                if (dgvRow.Cells["flrName"].Value == null || dgvRow.Cells["code"].Value == null)
                {
                    continue;
                }
                if (dgvRow.Cells["flrName"].Value.ToString() == innerName &&
                    dgvRow.Cells["code"].Value.ToString() == columnInf.Text
                   )
                {
                    dgvRow.Selected = true;
                    dgvCalculationRes.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    break;
                }
            }
        }
    }
}
