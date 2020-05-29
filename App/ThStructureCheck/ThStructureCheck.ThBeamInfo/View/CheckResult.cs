using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using acadApp=Autodesk.AutoCAD.ApplicationServices;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using System.Drawing;

namespace ThStructureCheck.ThBeamInfo
{
    public partial class CheckResult : UserControl
    {
        private List<string> nodeKeys = new List<string>();
        private string dataCorrectNodeName = "DataCorrect";
        private string codeLostNodeName = "CodeLost";
        private string uncompleteNodeNme = "Uncomplete";
        private TreeNode lastShowDetailNode = null;
        // 记录鼠标（左键）点击次数
        private int cnt = 0;
        private TreeNode currentNode = null;
        private bool isMouseRightClick = false;
        /// <summary>
        /// 最后一次点击详细
        /// </summary>
        public TreeNode LastShowDetailNode
        {
            get
            {
                return lastShowDetailNode;
            }
        }

        public string DataCorrectNodeName
        {
            get { return dataCorrectNodeName; }
        }
        public CheckResult()
        {
            InitializeComponent();
            this.tvCheckRes.ContextMenuStrip = null;
            this.SizeChanged += CheckResult_SizeChanged;
            this.panelUp.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.panelMiddle.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.panelDown.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.tvCheckRes.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.lblPaperDistinguishResult.ForeColor = Color.White;

            this.nodeKeys = new List<string> { this.dataCorrectNodeName, this.codeLostNodeName, this.uncompleteNodeNme };
            this.tvCheckRes.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.tvCheckRes.HideSelection = false;
            InitTreeView();
            LoadTree();
        }

        private void CheckResult_MouseEnter(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.IsBalloon = true;
            toolTip.ShowAlways = true;
            if ((Control)sender == this.btnShowDetailData)
            {
                if (!this.btnShowDetailData.Enabled)
                {
                    toolTip.SetToolTip((Control)sender, "当前处于禁用状态！");
                }
            }
        }
        private void CheckResult_SizeChanged(object sender, EventArgs e)
        {
            this.btnComponentDefinition.Location = new Point(
                this.btnShowDetailData.Location.X - 10 - this.btnComponentDefinition.Width, this.btnComponentDefinition.Location.Y);
        }

        private void InitTreeView()
        {
            this.tvCheckRes.Nodes.Clear();
        }
        public void LoadTree(string docFullPath = "")
        {
        }
        private void panel1_ControlAdded(object sender, ControlEventArgs e)
        {
            this.panelMiddle.VerticalScroll.Enabled = true;
            this.panelMiddle.VerticalScroll.Visible = true;
            this.panelMiddle.Scroll += Panel1_Scroll;
        }
        private void Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            this.panelMiddle.VerticalScroll.Value = e.NewValue;
        }    
    }
    public enum FindNodeMode
    {
        None,
        /// <summary>
        /// 图框名
        /// </summary>
        InnerFrame,
        /// <summary>
        /// 匹配成功
        /// </summary>
        Match,
        /// <summary>
        /// 编号->KZ1
        /// </summary>
        Code,
        /// <summary>
        /// 子编号 ->KZ1-1
        /// </summary>
        SubCode
    }
}
