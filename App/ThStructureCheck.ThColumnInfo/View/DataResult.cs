using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThColumnInfo.Validate;
using Autodesk.AutoCAD.DatabaseServices;
using ThColumnInfo.Validate.Model;
using ThColumnInfo.Service;

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
        private int dgvIndicatorCurrentRowIndex = -1;
        private int dgvCheckResCurrentRowIndex = -1;

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
            this.innerframeNode = CheckPalette._checkResult.CheckResVM.TraverseRoot(node);
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
            this.dgvIndicator.BackgroundColor = this.dgvBackColor;
            this.dgvCheckRes.BackgroundColor = this.dgvBackColor;

            this.tableLayoutPanel1.BackColor= this.dgvBackColor;
            this.groupBox1.BackColor = this.dgvBackColor;
            this.rbShowAll.BackColor = this.dgvBackColor;
            this.rbShowInvalid.BackColor = this.dgvBackColor;
            this.rbShowAll.ForeColor= this.textForeClor;
            this.rbShowInvalid.ForeColor = this.textForeClor;

            this.dgvColumnTable.GridColor = Color.Black;
            this.dgvIndicator.GridColor = Color.Black;
            this.dgvCheckRes.GridColor = Color.Black;
            this.dgvColumnTable.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvIndicator.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvCheckRes.ColumnHeadersDefaultCellStyle.BackColor = this.dgvBackColor;
            this.dgvColumnTable.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;
            this.dgvIndicator.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;
            this.dgvCheckRes.ColumnHeadersDefaultCellStyle.ForeColor = this.textForeClor;

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
            this.innerframeNode = CheckPalette._checkResult.CheckResVM.TraverseRoot(node);
            UpdateData();
        }
        public void ClearDataGridView()
        {
            this.dgvColumnTable.Rows.Clear();
            this.dgvIndicator.Rows.Clear();
            this.dgvCheckRes.Rows.Clear();
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
            bool findCorrectNode = CheckPalette._checkResult.CheckResVM.TraverseDataCorrectNode(this.currentNode);
            if(findCorrectNode)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.CheckResVM.GetDataCorrectColumnInfs(this.currentNode);
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

                    if(string.IsNullOrEmpty(columnInf.JointCorehooping))
                    {
                        string ctriJointCoreHoop = ctri.Replace132(ctri.JointCoreHoop);
                        if(ctriJointCoreHoop.Length>1)
                        {
                            if(ctriJointCoreHoop[0]>='A'&& ctriJointCoreHoop[0] <= 'Z')
                            {
                                this.dgvColumnTable.Rows[rowIndex].Cells["coreHooping"].Value = ctriJointCoreHoop.Substring(0);
                            }
                            else
                            {
                                this.dgvColumnTable.Rows[rowIndex].Cells["coreHooping"].Value = ctriJointCoreHoop;
                            }
                        }                        
                    }
                    else
                    {
                        this.dgvColumnTable.Rows[rowIndex].Cells["coreHooping"].Value = columnInf.JointCorehooping;
                    }
                    this.dgvColumnTable.Rows[rowIndex].Cells["coreHooping"].Style.BackColor = this.cellBackColor;
                    this.dgvColumnTable.Rows[rowIndex].Cells["coreHooping"].Style.ForeColor = this.textForeClor;

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
            this.dgvIndicator.Rows.Clear();
            if(ThValidate.columnDataModels==null)
            {
                return;
            }
            if (this.thCalculationValidate != null)
            {
                FillThValidationIndicator();
            }
            else if (this.thSpecificationValidate != null)
            {
                FillThSpecificationIndicator();
            }           
        }
        private void FillThSpecificationIndicator()
        {
            bool findCorrectNode = CheckPalette._checkResult.CheckResVM.TraverseDataCorrectNode(this.currentNode);
            if (findCorrectNode)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.CheckResVM.GetDataCorrectColumnInfs(this.currentNode);
                foreach (ColumnInf columnInf in correctColumnInfs)
                {
                    ColumnDataModel cdm = null;
                    var cdmRes = ThValidate.columnDataModels.Where(i => i.Code == columnInf.Code).Select(i => i);
                    if (cdmRes != null && cdmRes.Count() > 0)
                    {
                        cdm = cdmRes.First();
                    }
                    if(cdm==null)
                    {
                        continue;
                    }
                    NoCalculationValidate noCV = new NoCalculationValidate(columnInf);
                    int rowIndex = this.dgvIndicator.Rows.Add();
                    this.dgvIndicator.Rows[rowIndex].Cells["code"].Value = columnInf.Code;
                    this.dgvIndicator.Rows[rowIndex].Cells["subCode"].Value = columnInf.Text;
                    this.dgvIndicator.Rows[rowIndex].Cells["xLongitudinalBarArea"].Value = cdm.GetXLongitudinalBarArea();
                    this.dgvIndicator.Rows[rowIndex].Cells["yLongitudinalBarArea"].Value = cdm.GetYLongitudinalBarArea();
                    this.dgvIndicator.Rows[rowIndex].Cells["xStirrupArea"].Value = cdm.GetXStirrupArea();
                    this.dgvIndicator.Rows[rowIndex].Cells["yStirrupArea"].Value = cdm.GetYStirrupArea();
                    ColuJointCoreAnalysis coluJointCoreAnalysis;
                    if (!string.IsNullOrEmpty(columnInf.JointCorehooping))
                    {
                        coluJointCoreAnalysis = new ColuJointCoreAnalysis(columnInf.JointCorehooping);
                    }
                    else
                    {
                        coluJointCoreAnalysis = cdm.ColuJointCore;
                    }
                    double xCoreStirrupArea = cdm.GetXCoreReinforcementArea(coluJointCoreAnalysis);
                    double yCoreStirrupArea = cdm.GetYCoreReinforcementArea(coluJointCoreAnalysis);
                    this.dgvIndicator.Rows[rowIndex].Cells["coreStirrupArea"].Value = Math.Min(xCoreStirrupArea, yCoreStirrupArea);
                    this.dgvIndicator.Rows[rowIndex].Cells["dblXSpace"].Value = cdm.GetXStirrupLimbSpace(noCV.ProtectLayerThickness);
                    this.dgvIndicator.Rows[rowIndex].Cells["dblYSpace"].Value = cdm.GetYStirrupLimbSpace(noCV.ProtectLayerThickness);
                    this.dgvIndicator.Rows[rowIndex].Cells["dblXP"].Value = Math.Round(cdm.DblXP * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["dblYP"].Value = Math.Round(cdm.DblYP * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["dblP"].Value = Math.Round(cdm.DblP * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["volumeStirrupRatio"].Value = Math.Round(cdm.GetVolumeStirrupRatio(noCV.ProtectLayerThickness)*100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["shearSpanRatio"].Value = "";
                    this.dgvIndicator.Rows[rowIndex].Cells["antiseismic"].Value = noCV.AntiSeismicGrade;
                    this.dgvIndicator.Rows[rowIndex].Cells["concreteStrength"].Value = noCV.ConcreteStrength;
                    this.dgvIndicator.Rows[rowIndex].Cells["protectLayerThickness"].Value = noCV.ProtectLayerThickness;
                    if(noCV.CornerColumn)
                    {
                        this.dgvIndicator.Rows[rowIndex].Cells["cornerColumn"].Value = "是";
                    }
                    else
                    {
                        this.dgvIndicator.Rows[rowIndex].Cells["cornerColumn"].Value = "否";
                    }
                }
                for(int i=0;i<this.dgvIndicator.Rows.Count;i++)
                {
                    for(int j=0;j<this.dgvIndicator.Columns.Count;j++)
                    {
                        this.dgvIndicator.Rows[i].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvIndicator.Rows[i].Cells[j].Style.ForeColor = this.textForeClor;
                    }
                }
            }
            if (this.dgvIndicator.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvIndicator.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvIndicator.Rows.Add();
                    for (int j = 0; j < this.dgvIndicator.Columns.Count; j++)
                    {
                        this.dgvIndicator.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvIndicator.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvIndicator.Rows[rowIndex].Cells[j].Value = "";
                    }
                }
            }
        }
        private void FillThValidationIndicator()
        {
            bool findCorrectNode = CheckPalette._checkResult.CheckResVM.TraverseDataCorrectNode(this.currentNode);
            if (findCorrectNode)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.CheckResVM.GetDataCorrectColumnInfs(this.currentNode);
                foreach (ColumnInf columnInf in correctColumnInfs)
                {
                    ColumnDataModel cdm = null;
                    var cdmRes = ThValidate.columnDataModels.Where(i => i.Code == columnInf.Code).Select(i => i);
                    if (cdmRes != null && cdmRes.Count() > 0)
                    {
                        cdm = cdmRes.First();
                    }
                    if (cdm == null)
                    {
                        continue;
                    }
                    int rowIndex = this.dgvIndicator.Rows.Add();
                    this.dgvIndicator.Rows[rowIndex].Cells["code"].Value = columnInf.Code;
                    this.dgvIndicator.Rows[rowIndex].Cells["subCode"].Value = columnInf.Text;
                    this.dgvIndicator.Rows[rowIndex].Cells["xLongitudinalBarArea"].Value = cdm.GetXLongitudinalBarArea();
                    this.dgvIndicator.Rows[rowIndex].Cells["yLongitudinalBarArea"].Value = cdm.GetYLongitudinalBarArea();
                    this.dgvIndicator.Rows[rowIndex].Cells["xStirrupArea"].Value = cdm.GetXStirrupArea();
                    this.dgvIndicator.Rows[rowIndex].Cells["yStirrupArea"].Value = cdm.GetYStirrupArea();
                    double protectThickness = 0.0;
                    double jkb = 0.0;
                    ColumnRelateInf columnRelateInf = this.thCalculationValidate.
                        ColumnValidateResultDic.Where(i => i.Key.ModelColumnInfs.Count == 1 &&
              i.Key.ModelColumnInfs[0].Code == columnInf.Code).Select(i => i.Key).FirstOrDefault();
                    CalculationValidate calculationValidate=null;
                    if (columnRelateInf != null)
                    {
                        calculationValidate = new CalculationValidate(columnRelateInf);
                        protectThickness = calculationValidate.ProtectLayerThickness;
                        jkb = columnRelateInf.YjkColumnData.Jkb;
                    }
                    ColuJointCoreAnalysis coluJointCoreAnalysis;
                    if (!string.IsNullOrEmpty(columnInf.JointCorehooping))
                    {
                        coluJointCoreAnalysis = new ColuJointCoreAnalysis(columnInf.JointCorehooping);
                    }
                    else
                    {
                        coluJointCoreAnalysis = cdm.ColuJointCore;
                    }
                    double xCoreStirrupArea = cdm.GetXCoreReinforcementArea(coluJointCoreAnalysis);
                    double yCoreStirrupArea = cdm.GetYCoreReinforcementArea(coluJointCoreAnalysis);
                    this.dgvIndicator.Rows[rowIndex].Cells["coreStirrupArea"].Value = Math.Min(xCoreStirrupArea, yCoreStirrupArea);
                    this.dgvIndicator.Rows[rowIndex].Cells["dblXSpace"].Value = cdm.GetXStirrupLimbSpace(protectThickness);
                    this.dgvIndicator.Rows[rowIndex].Cells["dblYSpace"].Value = cdm.GetYStirrupLimbSpace(protectThickness);
                    this.dgvIndicator.Rows[rowIndex].Cells["dblXP"].Value = Math.Round(cdm.DblXP * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["dblYP"].Value = Math.Round(cdm.DblYP * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["dblP"].Value = Math.Round(cdm.DblP * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["volumeStirrupRatio"].Value = Math.Round(cdm.GetVolumeStirrupRatio(protectThickness) * 100, 3) + "%";
                    this.dgvIndicator.Rows[rowIndex].Cells["shearSpanRatio"].Value = Math.Round(jkb, 3);
                    if (calculationValidate != null)
                    {
                        this.dgvIndicator.Rows[rowIndex].Cells["antiseismic"].Value = calculationValidate.AntiSeismicGrade;
                        this.dgvIndicator.Rows[rowIndex].Cells["concreteStrength"].Value = calculationValidate.ConcreteStrength;
                        this.dgvIndicator.Rows[rowIndex].Cells["protectLayerThickness"].Value = calculationValidate.ProtectLayerThickness;
                        if (calculationValidate.CornerColumn)
                        {
                            this.dgvIndicator.Rows[rowIndex].Cells["cornerColumn"].Value = "是";
                        }
                        else
                        {
                            this.dgvIndicator.Rows[rowIndex].Cells["cornerColumn"].Value = "否";
                        }
                    }
                    else
                    {
                        this.dgvIndicator.Rows[rowIndex].Cells["cornerColumn"].Value = "否";
                    }
                }
                for (int i = 0; i < this.dgvIndicator.Rows.Count; i++)
                {
                    for (int j = 0; j < this.dgvIndicator.Columns.Count; j++)
                    {
                        this.dgvIndicator.Rows[i].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvIndicator.Rows[i].Cells[j].Style.ForeColor = this.textForeClor;
                    }
                }
            }
            if (this.dgvIndicator.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvIndicator.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvIndicator.Rows.Add();
                    for (int j = 0; j < this.dgvIndicator.Columns.Count; j++)
                    {
                        this.dgvIndicator.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvIndicator.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvIndicator.Rows[rowIndex].Cells[j].Value = "";
                    }
                }
            }
        }
        private void AddDataToDataGridView3()
        {
            this.dgvCheckRes.Rows.Clear();
            if(this.thCalculationValidate != null)
            {
                FillThValidationData();
            }
            else if(this.thSpecificationValidate != null)
            {
                FillThSpecificationData();
            }
        }
        private void FillThSpecificationData()
        {
            bool findCorrectNode = CheckPalette._checkResult.CheckResVM.TraverseDataCorrectNode(this.currentNode);
            if (findCorrectNode)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.CheckResVM.GetDataCorrectColumnInfs(this.currentNode);
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
                        int rowIndex = this.dgvCheckRes.Rows.Add();
                        if (i == 0)
                        {
                            this.dgvCheckRes.Rows[rowIndex].Cells["code"].Value = columnInf.Code;
                            this.dgvCheckRes.Rows[rowIndex].Cells["subCode"].Value = columnInf.Text;
                        }
                        this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Value = values[i];

                        this.dgvCheckRes.Rows[rowIndex].Cells["code"].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["code"].Style.ForeColor = this.textForeClor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["subCode"].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["subCode"].Style.ForeColor = this.textForeClor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = this.textForeClor;

                        if (i < errorIndex)
                        {
                            this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = Color.Red;
                        }
                        this.dgvCheckRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                    }
                }
            }

            if (this.dgvCheckRes.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvCheckRes.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvCheckRes.Rows.Add();
                    for (int j = 0; j < this.dgvCheckRes.Columns.Count; j++)
                    {
                        this.dgvCheckRes.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvCheckRes.Rows[rowIndex].Cells[j].Value = "";
                    }
                    this.dgvCheckRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                }
            }
        }
        private void FillThValidationData()
        {
            bool findCorrectNode = CheckPalette._checkResult.CheckResVM.TraverseDataCorrectNode(this.currentNode);
            if (findCorrectNode && this.thCalculationValidate != null)
            {
                List<ColumnInf> correctColumnInfs = CheckPalette._checkResult.CheckResVM.GetDataCorrectColumnInfs(this.currentNode);
                foreach (ColumnInf columnInf in correctColumnInfs)
                {
                    var res = this.thCalculationValidate.ColumnValidateResultDic.Where(i => i.Key.ModelColumnInfs.Count == 1 &&
                     i.Key.ModelColumnInfs[0].Code == columnInf.Code && i.Key.ModelColumnInfs[0].Text == columnInf.Text).Select(i => i.Value);
                    if(res.Count()==0)
                    {
                        continue;
                    }
                    var values = this.thCalculationValidate.ColumnValidateResultDic.Where(i => i.Key.ModelColumnInfs.Count == 1 &&
                    i.Key.ModelColumnInfs[0].Code == columnInf.Code && i.Key.ModelColumnInfs[0].Text == columnInf.Text).Select(i => i.Value).First();
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
                        int rowIndex = this.dgvCheckRes.Rows.Add();
                        if (i == 0)
                        {
                            this.dgvCheckRes.Rows[rowIndex].Cells["code"].Value = columnInf.Code;
                            this.dgvCheckRes.Rows[rowIndex].Cells["subCode"].Value = columnInf.Text;
                        }
                        this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Value = values[i];

                        this.dgvCheckRes.Rows[rowIndex].Cells["code"].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["code"].Style.ForeColor = this.textForeClor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["subCode"].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["subCode"].Style.ForeColor = this.textForeClor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = this.textForeClor;
                        if (i < errorIndex)
                        {
                            this.dgvCheckRes.Rows[rowIndex].Cells["detail"].Style.ForeColor = Color.Red;
                        }
                        this.dgvCheckRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                    }
                }
            }
            if (this.dgvCheckRes.Rows.Count < this.rowCount)
            {
                int addRowCount = this.rowCount - this.dgvCheckRes.Rows.Count;
                for (int i = 0; i < addRowCount; i++)
                {
                    int rowIndex = this.dgvCheckRes.Rows.Add();
                    for (int j = 0; j < this.dgvCheckRes.Columns.Count; j++)
                    {
                        this.dgvCheckRes.Rows[rowIndex].Cells[j].Style.BackColor = this.cellBackColor;
                        this.dgvCheckRes.Rows[rowIndex].Cells[j].Style.ForeColor = this.textForeClor;
                        this.dgvCheckRes.Rows[rowIndex].Cells[j].Value = "";
                    }
                    this.dgvCheckRes.Rows[rowIndex].Tag = this.innerframeNode.Tag;
                }
            }
        }
        /// <summary>
        /// 柱表
        /// </summary>
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
            this.dgvColumnTable.Columns.Add("coreHooping", "节点核心区箍筋");
            this.dgvColumnTable.Columns.Add("hoopType", "箍筋类型号");

            int baseWidth = 80;
            foreach (DataGridViewColumn dgvCol in this.dgvColumnTable.Columns)
            {
                dgvCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                switch (dgvCol.Name)
                {
                    case "all":
                        dgvCol.Width = (int)(baseWidth * 2.5);
                        break;
                    case "bSide":
                    case "hside":
                    case "coreHooping":
                        dgvCol.Width = (int)(baseWidth * 1.75);
                        break;
                    case "hooping":
                        dgvCol.Width = (int)(baseWidth * 1.5);
                        break;
                    case "limbNum":
                        dgvCol.Width = (int)(baseWidth * 1.5);
                        break;
                    case "hoopType":
                        dgvCol.Width = (int)(baseWidth * 1.5);
                        break;
                    default:
                        dgvCol.Width = baseWidth;
                        break;
                }
            }
            this.dgvColumnTable.RowHeadersVisible = false;
            this.dgvColumnTable.AllowUserToAddRows = false;
            this.dgvColumnTable.AllowUserToOrderColumns = false;
            this.dgvColumnTable.AllowUserToResizeRows=false;
        }
        /// <summary>
        /// 计算指标
        /// </summary>
        private void InitDataGridView2()
        {
            this.dgvIndicator.Columns.Add("code", "柱编号");
            this.dgvIndicator.Columns.Add("subCode", "子编号");
            this.dgvIndicator.Columns.Add("xLongitudinalBarArea", "X侧纵筋值");
            this.dgvIndicator.Columns.Add("yLongitudinalBarArea", "Y侧纵筋值");
            this.dgvIndicator.Columns.Add("xStirrupArea", "X侧箍筋值");
            this.dgvIndicator.Columns.Add("yStirrupArea", "Y侧箍筋值");
            this.dgvIndicator.Columns.Add("coreStirrupArea", "节点核心区配箍值"); 
            this.dgvIndicator.Columns.Add("dblXSpace", "X侧箍筋肢距");
            this.dgvIndicator.Columns.Add("dblYSpace", "Y侧箍筋肢距");
            this.dgvIndicator.Columns.Add("dblXP", "X侧配筋率");
            this.dgvIndicator.Columns.Add("dblYP", "Y侧配筋率");
            this.dgvIndicator.Columns.Add("dblP", "全部纵筋配筋率");
            this.dgvIndicator.Columns.Add("volumeStirrupRatio", "体积配箍率");
            this.dgvIndicator.Columns.Add("shearSpanRatio", "剪跨比");
            this.dgvIndicator.Columns.Add("antiseismic", "抗震等级");
            this.dgvIndicator.Columns.Add("concreteStrength", "砼强度");
            this.dgvIndicator.Columns.Add("protectLayerThickness", "保护层厚度");
            this.dgvIndicator.Columns.Add("cornerColumn", "角柱");

            int baseWidth = 80;
            foreach (DataGridViewColumn dgvColumn in this.dgvIndicator.Columns)
            {
                dgvColumn.HeaderCell.Style.BackColor= this.cellBackColor;
                dgvColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                switch (dgvColumn.Name)
                {
                    case "coreStirrupArea":                    
                        dgvColumn.Width = (int)(baseWidth * 1.5);
                        break;
                    case "dblP":
                        dgvColumn.Width = (int)(baseWidth * 1.3);
                        break;
                    case "xStirrupArea":
                    case "yStirrupArea":
                        dgvColumn.Width = (int)(baseWidth * 1.1);
                        break;
                    case "shearSpanRatio":
                    case "concreteStrength":
                        dgvColumn.Width = (int)(baseWidth * 0.75);
                        break;
                    case "cornerColumn":
                        dgvColumn.Width = (int)(baseWidth * 0.6);
                        break;
                    default:
                        dgvColumn.Width = baseWidth;
                        break;
                }
            }
            this.dgvIndicator.RowHeadersVisible = false;
            this.dgvIndicator.AllowUserToAddRows = false;
            this.dgvIndicator.AllowUserToOrderColumns = false;
            this.dgvIndicator.AllowUserToResizeRows = false;
        }
        /// <summary>
        /// 校核结果
        /// </summary>
        private void InitDataGridView3()
        {
            this.dgvCheckRes.Columns.Add("code", "编号");
            this.dgvCheckRes.Columns.Add("subCode", "子编号");
            this.dgvCheckRes.Columns.Add("detail", "详细");
            int baseWidth = 80;
            foreach (DataGridViewColumn dgvColumn in this.dgvCheckRes.Columns)
            {
                dgvColumn.HeaderCell.Style.BackColor = this.cellBackColor;
                dgvColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                switch (dgvColumn.Name)
                {
                    case "detail":
                        dgvColumn.Width = (int)(baseWidth * 6);
                        break;                   
                    default:
                        dgvColumn.Width = baseWidth;
                        break;
                }
            }
            this.dgvCheckRes.RowHeadersVisible = false;
            this.dgvCheckRes.AllowUserToAddRows = false;
            this.dgvCheckRes.AllowUserToOrderColumns = false;
            this.dgvCheckRes.AllowUserToResizeRows = false;
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
                e.Graphics.DrawString(tabControl1.TabPages[i].Text, new System.Drawing.Font("宋体", 9),
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
                TreeNode findTreeCode = CheckPalette._checkResult.CheckResVM.
                    FindTreeCode(thStandardSign.InnerFrameName, GetFindNodeMode(columnName), codeText, subcodeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.CheckResVM.ShowSelectNodeFrameIds(findTreeCode);
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
        private void dgvCheckRes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            try
            {
                string columnName = this.dgvCheckRes.Columns[e.ColumnIndex].Name;
                if (columnName != "code" && columnName != "subCode")
                {
                    return;
                }
                this.dgvCheckResCurrentRowIndex = e.RowIndex;
                string codeText = this.dgvCheckRes.Rows[e.RowIndex].Cells["code"].Value.ToString();
                string subCodeText = this.dgvCheckRes.Rows[e.RowIndex].Cells["subCode"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvCheckRes.Rows[e.RowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.CheckResVM.FindTreeCode(thStandardSign.InnerFrameName, GetFindNodeMode(columnName), codeText, subCodeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.CheckResVM.ShowSelectNodeFrameIds(findTreeCode);
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
            if(this.dgvCheckRes.Rows[rowIndex].Cells["code"].Value!=null)
            {
                string codeText = this.dgvCheckRes.Rows[rowIndex].Cells["code"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvCheckRes.Rows[rowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.CheckResVM.FindTreeCode(thStandardSign.InnerFrameName, FindNodeMode.SubCode, "", codeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.CheckResVM.HideTotalFrameIds(findTreeCode);
                }
            }
            else
            {
                if(this.innerframeNode!=null)
                {
                    CheckPalette._checkResult.CheckResVM.HideTotalFrameIds(this.innerframeNode);
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
                HideDgvSpecificationRes(this.dgvIndicatorCurrentRowIndex);
                HideDgvCalculationRes(this.dgvCheckResCurrentRowIndex);
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
                    CheckPalette._checkResult.CheckResVM.SelectTreeNode(thStandardSign.InnerFrameName, value.ToString());
                }
            }
        }
        private void dgvCheckRes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.RowIndex<0 || e.ColumnIndex<0)
            {
                return;
            }
            if (!CheckTabIsActive(this.calculationTabName))
            {
                HideDgvColumnTable(this.dgvColumnTableCurrentRowIndex);
                HideDgvSpecificationRes(this.dgvIndicatorCurrentRowIndex);
            }
            if (dgvCheckResCurrentRowIndex >= 0)
            {
                if (this.dgvCheckResCurrentRowIndex != e.RowIndex)
                {
                    HideDgvCalculationRes(dgvCheckResCurrentRowIndex);
                }
            }
            this.dgvCheckResCurrentRowIndex = e.RowIndex;
            if (this.dgvCheckRes.Rows[e.RowIndex].Tag != null)
            {
                ThStandardSign thStandardSign = this.dgvCheckRes.Rows[e.RowIndex].Tag as ThStandardSign;
                object value = this.dgvCheckRes.Rows[e.RowIndex].Cells["subCode"].Value;
                if (value != null)
                {
                    CheckPalette._checkResult.CheckResVM.SelectTreeNode(thStandardSign.InnerFrameName, value.ToString());
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
                    CheckPalette._checkResult.CheckResVM.HideTotalFrameIds(this.innerframeNode);
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
                TreeNode findTreeCode = CheckPalette._checkResult.CheckResVM.FindTreeCode(thStandardSign.InnerFrameName, FindNodeMode.Code, codeText, subcodeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.CheckResVM.HideTotalFrameIds(findTreeCode);
                }
            }
        }
        private void HideDgvSpecificationRes(int rowIndex)
        {
            if (rowIndex < 0)
            {
                return;
            }
            if(this.dgvIndicator.Rows[rowIndex].Cells["code"].Value!=null)
            {
                string codeText = this.dgvIndicator.Rows[rowIndex].Cells["code"].Value.ToString();
                ThStandardSign thStandardSign = this.dgvIndicator.Rows[rowIndex].Tag as ThStandardSign;
                if (thStandardSign == null)
                {
                    return;
                }
                TreeNode findTreeCode = CheckPalette._checkResult.CheckResVM.FindTreeCode(thStandardSign.InnerFrameName, FindNodeMode.SubCode, "", codeText);
                if (findTreeCode != null)
                {
                    CheckPalette._checkResult.CheckResVM.HideTotalFrameIds(findTreeCode);
                }
            }
            else
            {
                if(this.innerframeNode!=null)
                {
                    CheckPalette._checkResult.CheckResVM.HideTotalFrameIds(this.innerframeNode);
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
            foreach (DataGridViewRow dgvRow in this.dgvIndicator.Rows)
            {
                if (dgvRow.Cells["code"].Value == null || dgvRow.Cells["subCode"].Value == null)
                {
                    continue;
                }
                if (dgvRow.Cells["code"].Value.ToString() == columnInf.Code && 
                    dgvRow.Cells["subCode"].Value.ToString() == columnInf.Text
                   )
                {
                    dgvRow.Selected = true;
                    dgvIndicator.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    break;
                }
            }
        }
        private void SelectDgvCalculationRow(ColumnInf columnInf, string innerName)
        {
            foreach (DataGridViewRow dgvRow in this.dgvCheckRes.Rows)
            {
                if (dgvRow.Cells["code"].Value == null || dgvRow.Cells["subCode"].Value == null)
                {
                    continue;
                }
                if (dgvRow.Cells["code"].Value.ToString() == columnInf.Code &&
                    dgvRow.Cells["subCode"].Value.ToString() == columnInf.Text
                   )
                {
                    dgvRow.Selected = true;
                    if(dgvRow.Visible)
                    {
                        dgvCheckRes.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    }
                    break;
                }
            }
        }

        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            GroupBox gBox = (GroupBox)sender;
            e.Graphics.Clear(this.dgvBackColor);
            e.Graphics.DrawString(gBox.Text, gBox.Font, Brushes.White, 10, 1);
            var vSize = e.Graphics.MeasureString(gBox.Text, gBox.Font);
            Pen vPen = new Pen(this.dgvBackColor);
            e.Graphics.DrawLine(vPen, 1, vSize.Height / 2, 8, vSize.Height / 2);
            e.Graphics.DrawLine(vPen, vSize.Width + 8, vSize.Height / 2, gBox.Width - 2, vSize.Height / 2);
            e.Graphics.DrawLine(vPen, 1, vSize.Height / 2, 1, gBox.Height - 2);
            e.Graphics.DrawLine(vPen, 1, gBox.Height - 2, gBox.Width - 2, gBox.Height - 2);
            e.Graphics.DrawLine(vPen, gBox.Width - 2, vSize.Height / 2, gBox.Width - 2, gBox.Height - 2);
        }

        private void rbShowAll_CheckedChanged(object sender, EventArgs e)
        {
            ShowDgvCheckRows();
        }

        private void rbShowInvalid_CheckedChanged(object sender, EventArgs e)
        {
            ShowDgvCheckRows(false);
        }
        private void ShowDgvCheckRows(bool showAll = true)
        {
            foreach(DataGridViewRow dgvRow in this.dgvCheckRes.Rows)
            {
                if(showAll)
                {
                    dgvRow.Visible = true;
                }
                else
                {
                    if(dgvRow.Cells["detail"].Style.ForeColor == Color.Red)
                    {
                        dgvRow.Visible = true;
                    }
                    else
                    {
                        dgvRow.Visible = false;
                    }
                }
            }
        }
    }
}
