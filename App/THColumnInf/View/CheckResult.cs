using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThColumnInfo.View
{
    public partial class CheckResult : UserControl
    {
        private List<ColumnInf> _checkColumnInf = new List<ColumnInf>();
        private string _paperCheckResult = "PaperCheckResult";
        private string _standardCheckResult = "StandardCheckResult";
        private List<string> nodeKeys = new List<string> { "DataCorrect" , "CodeLost" ,"Uncomplete"};

        public static SearchFields _searchFields;

        public CheckResult()
        {
            InitializeComponent();
            InitTreeView();
            if (_searchFields == null)
            {
                _searchFields = new SearchFields()
                {
                    ColumnRangeLayerName = "砼柱",
                    ZhuGuJingLayerName = "柱箍筋",
                    ZhuJiZhongMarkLayerName = "柱集中标注",
                    ZhuSizeMark = "柱尺寸标注",
                    ZhuYuanWeiMarkLayerName = "柱原位标注",
                    ZhuMarkLeaderLayerName = "柱标注引线"
                };
            }
        }
        private void InitTreeView()
        {
            this.tvCheckRes.Nodes.Add(this._paperCheckResult, "图纸识别结果");
            this.tvCheckRes.Nodes.Add(this._standardCheckResult, "规范检查结果");
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            this.tvCheckRes.Nodes.Clear();
            InitTreeView();
            FillColumnInfToTreeView();
        } 
        private void FillColumnInfToTreeView()
        {
            //List<ColumnInf> correctList = new List<ColumnInf>(); 
            //List<ColumnInf> codeEmptyList = new List<ColumnInf>();
            //List<ColumnInf> infCompleteList = new List<ColumnInf>();

            //foreach (ColumnInf columnInf in this._checkColumnInf)
            //{
            //    ErrorMsg errorMsg = columnInf.GetColumnInfStatus();
            //    switch(errorMsg)
            //    {
            //        case ErrorMsg.OK:
            //            correctList.Add(columnInf);
            //            break;
            //        case ErrorMsg.CodeEmpty:
            //            codeEmptyList.Add(columnInf);
            //            break;
            //        case ErrorMsg.InfNotCompleted:
            //            infCompleteList.Add(columnInf);
            //            break;
            //    }
            //}
            //List<string> codes = correctList.Select(i => i.Code).Distinct().ToList();
            //Dictionary<string, List<ColumnInf>> codeColumnInf = new Dictionary<string, List<ColumnInf>>();
            //foreach(string code in codes)
            //{
            //    List<ColumnInf> columnInfs = correctList.Where(i=>i.Code==code).Select(i=>i).ToList();
            //    if(columnInfs==null || columnInfs.Count==0)
            //    {
            //        continue;
            //    }
            //    codeColumnInf.Add(code, columnInfs);
            //}
            //correctList = new List<ColumnInf>();
            //foreach (var item in codeColumnInf)
            //{
            //    List<ColumnInf> res = item.Value.Where(i => i.OnlyCodeSetValue() == false).Select(i => i).ToList();
            //    if(res==null || res.Count==0)
            //    {
            //        infCompleteList.AddRange(item.Value);
            //    }
            //    else
            //    {
            //        correctList.AddRange(item.Value);
            //    }                
            //}
            //TreeNode paperCheckResNode = this.tvCheckRes.Nodes.Find(this._paperCheckResult, true).First();
            //if (correctList.Count>0) 
            //{              
            //    TreeNode correctParentNode = paperCheckResNode.Nodes.Add("DataCorrect","识别成功：数据正确("+correctList.Count+")");              

            //    correctList.Sort(new ColumnInfCompare());               
            //    codeColumnInf = new Dictionary<string, List<ColumnInf>>();
            //    codes = correctList.Select(i => i.Code).Distinct().ToList();
            //    foreach (string code in codes)
            //    {
            //        List<ColumnInf> columnInfs = correctList.Where(i => i.Code == code).Select(i => i).ToList();
            //        codeColumnInf.Add(code, columnInfs);
            //    }
            //    foreach(var item in codeColumnInf)
            //    {
            //        List<ColumnInf> columnInfs = item.Value.Where(i => !i.OnlyCodeSetValue()).Select(i => i).ToList();
            //        if(columnInfs==null && columnInfs.Count==0)
            //        {
            //            continue;
            //        }
            //        ColumnInf columnInf = columnInfs.First();
            //        TreeNode codeSpecNode = correctParentNode.Nodes.Add(columnInf.Code + " " + columnInf.Spec);
            //        codeSpecNode.Nodes.Add("类别："+BaseFunction.GetColumnCodeChinese(columnInf.Code));
            //        codeSpecNode.Nodes.Add("数量：" + item.Value.Count);
            //        codeSpecNode.Nodes.Add("全部纵筋：" + columnInf.IronSpec);
            //        codeSpecNode.Nodes.Add("角筋：" + columnInf.CornerIronSpec);
            //        codeSpecNode.Nodes.Add("B边：" + columnInf.XIronSpec);
            //        codeSpecNode.Nodes.Add("H边：" + columnInf.YIronSpec);
            //        codeSpecNode.Nodes.Add("箍筋：" + columnInf.NeiborGuJinHeightSpec);
            //        columnInf.Handles = item.Value.Select(i => i.CurrentHandle).ToList();
            //        columnInf.Num = item.Value.Count;
            //        codeSpecNode.Tag = columnInf;

            //        if (columnInf.XIronNum>0 && columnInf.YIronNum > 0)
            //        {
            //            codeSpecNode.Nodes.Add("肢数：" + columnInf.XIronNum + "x" + columnInf.YIronNum);
            //        }
            //    }
            //}
            //if(codeEmptyList.Count>0)
            //{
            //    TreeNode codeEmptyParentNode = paperCheckResNode.Nodes.Add("CodeLost", "识别异常：柱编号缺失");
            //    for(int i=1;i<= codeEmptyList.Count;i++)
            //    {
            //        TreeNode codeEmptyNode = codeEmptyParentNode.Nodes.Add(i.ToString());
            //        codeEmptyNode.Tag = codeEmptyList[i - 1];
            //    }
            //}
            //if (infCompleteList.Count > 0)
            //{
            //    TreeNode uncompleteParentNode = paperCheckResNode.Nodes.Add("Uncomplete", "识别异常：参数识别不全");
            //    infCompleteList.Sort(new ColumnInfCompare());
            //    codeColumnInf = new Dictionary<string, List<ColumnInf>>();
            //    List<string> infCompleteCodes = infCompleteList.Select(i => i.Code).Distinct().ToList();
            //    foreach (string code in infCompleteCodes)
            //    {
            //        List<ColumnInf> columnInfs = infCompleteList.Where(i => i.Code == code).Select(i => i).ToList();
            //        if (columnInfs == null || columnInfs.Count==0)
            //        {
            //            continue;
            //        }
            //        codeColumnInf.Add(code, columnInfs);
            //    }
            //    string columnSpec = "";
            //    foreach (var item in codeColumnInf)
            //    {
            //        List<ColumnInf> columnInfs = item.Value.Where(i => string.IsNullOrEmpty(i.Spec) == false).Select(i => i).ToList();                   
            //        if(columnInfs != null && columnInfs.Count>0)
            //        {
            //            columnSpec = columnInfs[0].Spec;       
            //        }
            //        else
            //        {
            //            columnSpec = "";
            //        }
            //        ColumnInf columnInf = columnInfs.First();
            //        columnInf.Handles = item.Value.Select(i => i.CurrentHandle).ToList();
            //        columnInf.Num = item.Value.Count;
            //        TreeNode codeNode = uncompleteParentNode.Nodes.Add(item.Key);
            //        codeNode.Tag = columnInf;
            //        codeNode.Nodes.Add("截面尺寸：" + columnSpec);
            //    }
            //}
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            //DataPalette._dateResult.dataGridView1.Rows.Clear();

            //TreeNode[] nodes= this.tvCheckRes.Nodes.Find("DataCorrect",true);
            //if(nodes==null || nodes.Length==0)
            //{
            //    return;
            //}
            //foreach(TreeNode tn in nodes[0].Nodes)
            //{
            //    if(tn.Tag==null || tn.Tag.GetType()!=typeof(ColumnInf))
            //    {
            //        continue;
            //    }
            //    ColumnInf columnInf = tn.Tag as ColumnInf;
            //    int rowIndex= DataPalette._dateResult.dataGridView1.Rows.Add();
            //    DataGridViewRow dgvRow = DataPalette._dateResult.dataGridView1.Rows[rowIndex];
            //    dgvRow.Tag = columnInf;
            //    dgvRow.Cells["code"].Value = columnInf.Code; //编号
            //    dgvRow.Cells["bh"].Value = columnInf.Spec; //规格
            //    dgvRow.Cells["num"].Value = columnInf.Num; //数量
            //    dgvRow.Cells["all"].Value = columnInf.IronSpec; //全部纵筋
            //    dgvRow.Cells["corner"].Value = columnInf.CornerIronSpec; //角筋
            //    dgvRow.Cells["bSide"].Value = columnInf.XIronSpec; //b边一侧中部筋
            //    dgvRow.Cells["hside"].Value = columnInf.YIronSpec; //h边一侧中部筋
            //    dgvRow.Cells["hooping"].Value = columnInf.NeiborGuJinHeightSpec; //箍筋

            //    if (columnInf.XIronNum>0 && columnInf.YIronNum > 0)
            //    {
            //        dgvRow.Cells["limbNum"].Value = columnInf.XIronNum+"x"+columnInf.YIronNum; //肢数
            //    }
            //    else
            //    {
            //        dgvRow.Cells["limbNum"].Value = ""; //肢数
            //    }
            //    for(int i=0;i< DataPalette._dateResult.dataGridView1.Columns.Count;i++)
            //    {
            //        dgvRow.Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;  //单元格居中对齐
            //        dgvRow.Cells[i].ReadOnly = true; //只读
            //    } 
            //}
        }

        private void panel1_ControlAdded(object sender, ControlEventArgs e)
        {
            this.panel1.VerticalScroll.Enabled = true;
            this.panel1.VerticalScroll.Visible = true;
            this.panel1.Scroll += Panel1_Scroll;
        }

        private void Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            this.panel1.VerticalScroll.Value = e.NewValue;
        }
        /// <summary>
        /// 目录树节点双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvCheckRes_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode treeNode = e.Node;
            if(treeNode==null) 
            {
                return;
            }
            //List<string> handles = new List<string>();
            //if(this.nodeKeys.IndexOf(treeNode.Name)>=0 || (treeNode.Parent!=null &&
            //    this.nodeKeys.IndexOf(treeNode.Parent.Name)>=0))
            //{
            //    if(this.nodeKeys.IndexOf(treeNode.Name)>=0)
            //    {
            //        foreach(TreeNode tn in treeNode.Nodes)
            //        {
            //            ColumnInf columnInf = tn.Tag as ColumnInf;
            //            if(columnInf==null)
            //            {
            //                continue;
            //            }
            //            if(columnInf.Handles.Count>0)
            //            {
            //                handles.AddRange(columnInf.Handles);
            //            }
            //            else if(!string.IsNullOrEmpty(columnInf.CurrentHandle))
            //            {
            //                handles.Add(columnInf.CurrentHandle);
            //            }
            //        }
            //    }
            //    else if(this.nodeKeys.IndexOf(treeNode.Parent.Name) >= 0)
            //    {
            //        ColumnInf columnInf = treeNode.Tag as ColumnInf;
            //        if (columnInf.Handles.Count > 0)
            //        {
            //            handles.AddRange(columnInf.Handles);
            //        }
            //        else if (!string.IsNullOrEmpty(columnInf.CurrentHandle))
            //        {
            //            handles.Add(columnInf.CurrentHandle);
            //        }
            //    }
            //}
            //if(handles.Count>0)
            //{
            //    handles= handles.Distinct().ToList();
            //}
            //DrawableOverruleController.RemoveDrawableRule(); //先移除，再附加
            //DrawableOverruleController.ShowHatchForColumn(handles);
        }
        /// <summary>
        /// 设置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnSet_Click(object sender, EventArgs e)
        {
            SearchFields searchFields = new SearchFields()
            {
                ColumnRangeLayerName = "砼柱",
                ZhuGuJingLayerName = "柱箍筋",
                ZhuJiZhongMarkLayerName = "柱集中标注",
                ZhuSizeMark = "柱尺寸标注",
                ZhuYuanWeiMarkLayerName = "柱原位标注",
                ZhuMarkLeaderLayerName = "柱标注引线"
            };
            SearchFieldVM searchFieldVM = new SearchFieldVM(searchFields);
            MainWindow setWindow = new MainWindow(searchFieldVM);
            setWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            setWindow.ShowDialog();
            _searchFields = searchFieldVM.m_UIModel;
        }       
    }
}
