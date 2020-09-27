using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using ThColumnInfo.Validate;
using Autodesk.AutoCAD.EditorInput;
using acadApp=Autodesk.AutoCAD.ApplicationServices;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using ThColumnInfo.ViewModel;
using System.Drawing;
using ThColumnInfo.Properties;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.Geometry;
using ThColumnInfo.Service;

namespace ThColumnInfo.View
{
    public partial class CheckResult : UserControl
    {
        // 记录鼠标（左键）点击次数
        private int cnt = 0;
        private bool isMouseRightClick = false;
        private TreeNode currentNode = null;
        private CheckResultVM checkResultVM;
        public CheckResultVM CheckResVM
        {
            get
            {
                return checkResultVM;
            }
        }       
        public CheckResult()
        {
            InitializeComponent();
            this.checkResultVM = new CheckResultVM(this);
            this.tvCheckRes.ContextMenuStrip = null;
            this.checkResultVM.ShowComponentPropPicture();
            this.SizeChanged += CheckResult_SizeChanged;
            this.panelUp.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.panelUp.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.tvCheckRes.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            this.tvCheckRes.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.tvCheckRes.HideSelection = false;
            this.tvCheckRes.DrawNode += tvCheckRes_DrawNode;
            this.checkResultVM.Load("",true);
            this.checkResultVM.SwitchShowDetailPicture();
        }
        private void btnParameterSet_Click(object sender, EventArgs e)
        {
            CheckResVM.ParameterSetCmd();
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            //ToDo
            CheckResVM.ExportExcelCmd();
        }
        private void btnComponentDefinition_Click(object sender, EventArgs e)
        {
            CheckResVM.ComponentDefinitionCmd();
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            CheckResVM.CheckAll();
        }
        private void btnShowDetailData_Click(object sender, EventArgs e)
        {
            CheckResVM.ShowDetailData();
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
        private void panel1_ControlAdded(object sender, ControlEventArgs e)
        {
            this.panelUp.VerticalScroll.Enabled = true;
            this.panelUp.VerticalScroll.Visible = true;
            this.panelUp.Scroll += Panel1_Scroll;
        }
        private void Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            this.panelUp.VerticalScroll.Value = e.NewValue;
        }
        /// <summary>
        /// 目录树节点双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvCheckRes_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }
            CheckResVM.ShowSelectNodeFrameIds(e.Node);
            tvCheckRes.SelectedNode = e.Node;
            e.Node.Expand();
        }
        private void tvCheckRes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }
            if(e.Button == MouseButtons.Right)
            {
                this.contextMenuStrip1.Visible = false;
                this.isMouseRightClick = true;
                if (e.Node.Tag != null)
                {
                    if (e.Node.Tag.GetType() == typeof(ThStandardSign))
                    {
                        this.contextMenuStrip1.Visible = true;
                        this.contextMenuStrip1.Show(this.tvCheckRes, e.Location);                        
                    }
                }
                this.tvCheckRes.SelectedNode = e.Node;
                return;
            }
            else
            {
                this.isMouseRightClick = false;
                this.tvCheckRes.ContextMenuStrip = null;
                this.contextMenuStrip1.Visible = false;
            }
            TreeNode selectNodeInnerFrameNode = checkResultVM.TraverseRoot(e.Node);
            Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
            if (this.currentNode != null)
            {
                TreeNode innerFrameNode = checkResultVM.TraverseRoot(this.currentNode);
                bool isExisted = false;
                foreach(TreeNode tn in this.tvCheckRes.Nodes)
                {
                    if(tn== innerFrameNode)
                    {
                        isExisted = true;
                        break;
                    }
                }
                if (this.currentNode != e.Node && isExisted)
                {
                    bool needHide = checkResultVM.GetTreeNodeHasVisibleFrame(innerFrameNode);
                    if (needHide && !this.isMouseRightClick)
                    {
                        checkResultVM.HideTotalFrameIds(innerFrameNode);
                        doc.SendStringToExecute("_.Regen ", true, false, true);
                    }
                }
                if (selectNodeInnerFrameNode != null && selectNodeInnerFrameNode != innerFrameNode)
                {
                    checkResultVM.ShowDetailData(selectNodeInnerFrameNode);
                }
            }
            this.currentNode = e.Node;
            this.tvCheckRes.SelectedNode = e.Node;            
            if (e.Button == MouseButtons.Left)
            {
                if (!checkResultVM.TraverseDataCorrectNode(e.Node))
                {
                    return;
                }
                if (e.Node.Tag != null && e.Node.Tag.GetType() == typeof(ColumnInf))
                {
                    ThStandardSign thStandardSign = selectNodeInnerFrameNode.Tag as ThStandardSign;
                    ColumnInf columnInf = e.Node.Tag as ColumnInf;
                    if(DataPalette._dateResult!=null)
                    {
                        DataPalette._dateResult.SelectDataGridViewRow(columnInf, thStandardSign.InnerFrameName);
                    }
                }
            }
        }
        /// <summary>
        /// 导入计算书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importCalculationTsmi_Click(object sender, EventArgs e)
        {
            CheckResVM.ImportCalculationCmd();
        }
        private void checkTsmi_Click(object sender, EventArgs e)
        {
            CheckResVM.Check();
            this.currentNode = tvCheckRes.SelectedNode;
        }
        private void detailDataTsmi_Click(object sender, EventArgs e)
        {
            CheckResVM.ShowDetailData();
        }
        private void tvCheckRes_Leave(object sender, EventArgs e)
        {
            if(this.tvCheckRes.SelectedNode!=null)
            {
                this.tvCheckRes.SelectedNode.BackColor = Color.Blue;
            }
        }
        private void tvCheckRes_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, e.Node.Bounds);
            if (e.State == TreeNodeStates.Selected)//做判断
            {
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, new Rectangle(e.Node.Bounds.Left, e.Node.Bounds.Top, e.Node.Bounds.Width, e.Node.Bounds.Height));//背景色为蓝色
                RectangleF drawRect = new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width + 10, e.Bounds.Height);
                e.Graphics.DrawString(e.Node.Text, this.tvCheckRes.Font, Brushes.White, drawRect);
                //字体为白色
            }
            else
            {
                e.DrawDefault = true;
            }
        }
        private void tvCheckRes_MouseDown(object sender, MouseEventArgs e)
        {
            //统计左键点击次数
            if (e.Button == MouseButtons.Left)
                cnt = e.Clicks;
            if(e.Button == MouseButtons.Right)
            {
                this.isMouseRightClick = true;
            }
            else
            {
                this.isMouseRightClick = false;
            }
            if(sender==this.tvCheckRes)
            {
                this.contextMenuStrip1.Visible = false;
                this.tvCheckRes.SelectedNode = null;
            }
        }
        private void tvCheckRes_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (cnt > 1)
                //如果是鼠标双击则禁止结点折叠
                e.Cancel = true;
            else
                //如果是鼠标单击则允许结点折叠
                e.Cancel = false;
        }
        private void tvCheckRes_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (cnt > 1)
                // 如果是鼠标双击则禁止结点展开
                e.Cancel = true;
            else
                // 如果是鼠标单击则允许结点展开
                e.Cancel = false;
        }
        private void btnShowDetailData_MouseHover(object sender, EventArgs e)
        {
            if(DataPalette.ShowPaletteMark)
            {
                this.toolTip2.SetToolTip(this.btnShowDetailData, "关闭详细面板");
            }
            else
            {
                this.toolTip2.SetToolTip(this.btnShowDetailData, "展开详细面板");
            }
        }

        private void addColumnTableTsmi_Click(object sender, EventArgs e)
        {
            this.checkResultVM.AddColumnTableCmd();
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
