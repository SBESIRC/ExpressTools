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

namespace ThColumnInfo.View
{
    public partial class CheckResult : UserControl
    {
        private List<string> nodeKeys = new List<string> ();
        private string dataCorrectNodeName = "DataCorrect";
        private string codeLostNodeName = "CodeLost";
        private string uncompleteNodeNme = "Uncomplete";
        private TreeNode lastShowDetailNode=null;
        // 记录鼠标（左键）点击次数
        private int cnt = 0;
        private TreeNode currentNode = null;
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
            ShowComponentPropPicture();
            this.SizeChanged += CheckResult_SizeChanged;
            this.panelUp.BackColor = System.Drawing.Color.FromArgb(74,74,74);
            this.panelMiddle.BackColor= System.Drawing.Color.FromArgb(92, 92, 92);
            this.tvCheckRes.BackColor= System.Drawing.Color.FromArgb(92, 92, 92);
            this.panelDown.BackColor= System.Drawing.Color.FromArgb(92, 92, 92);
            this.lblPaperDistinguishResult.ForeColor = Color.White;

            this.nodeKeys=new List<string> {this.dataCorrectNodeName,this.codeLostNodeName,this.uncompleteNodeNme }; 
            this.tvCheckRes.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.tvCheckRes.HideSelection = false;
            this.tvCheckRes.DrawNode += tvCheckRes_DrawNode;
            InitTreeView();
            LoadTree();
            SwitchShowDetailPicture();
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
                this.btnShowDetailData.Location.X-10- this.btnComponentDefinition.Width, this.btnComponentDefinition.Location.Y);
        }

        private void InitTreeView()
        {
            this.tvCheckRes.Nodes.Clear();
        }
        public void LoadTree(string docFullPath="")
        {
            if(!string.IsNullOrEmpty(docFullPath))
            {
                if (this.tvCheckRes.Nodes.Count > 0)
                {
                    foreach (TreeNode tn in this.tvCheckRes.Nodes)
                    {
                        if (tn.Tag != null)
                        {
                            ThStandardSignManager currentTsm = tn.Tag as ThStandardSignManager;
                            if (currentTsm.DocPath == docFullPath)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            ThStandardSignManager tm = ThStandardSignManager.LoadData("", false);
            FillColumnDataToTreeView(tm);
            if (this.tvCheckRes.Nodes != null && this.tvCheckRes.Nodes.Count > 0)
            {
                foreach (TreeNode tn in this.tvCheckRes.Nodes)
                {
                    if (tn.Nodes != null && tn.Nodes.Count > 0)
                    {
                        tn.Nodes[0].Expand();
                    }
                    else
                    {
                        tn.Expand();
                    }
                }
            }
            if (this.tvCheckRes.Nodes.Count > 0)
            {
                this.tvCheckRes.Nodes[0].Expand();
            }
        }
        private void FillDrawCheckInfToTreeView(IDataSource dataSource,TreeNode tn=null)
        {
            List<ColumnInf> correctList = new List<ColumnInf>();
            List<ColumnInf> codeEmptyList = new List<ColumnInf>();
            List<ColumnInf> infCompleteList = new List<ColumnInf>();
            if(dataSource == null)
            {
                return;
            }
            foreach (ColumnInf columnInf in dataSource.ColumnInfs)
            {
                switch (columnInf.Error)
                {
                    case ErrorMsg.OK:                    
                        correctList.Add(columnInf);
                        break;
                    case ErrorMsg.CodeEmpty:
                        codeEmptyList.Add(columnInf);
                        break;
                    case ErrorMsg.InfNotCompleted:
                        infCompleteList.Add(columnInf);
                        break;
                }
            }
            ThProgressBar.MeterProgress();
            List<string> codes = correctList.Select(i => i.Code).Distinct().ToList();
            Dictionary<string, List<ColumnInf>> codeColumnInf = new Dictionary<string, List<ColumnInf>>();
            foreach (string code in codes)
            {
                List<ColumnInf> columnInfs = correctList.Where(i => i.Code == code).Select(i => i).ToList();
                if (columnInfs == null || columnInfs.Count == 0)
                {
                    continue;
                }
                codeColumnInf.Add(code, columnInfs);
            }
            ThProgressBar.MeterProgress();
            TreeNode correctParentNode = tn.Nodes.Add(this.dataCorrectNodeName, "识别成功：数据正确(" + correctList.Count + ")");
            System.Drawing.Color correctColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.Related);
            correctParentNode.ForeColor = correctColor;
            if (correctList.Count > 0)
            {
                correctList.Sort(new ColumnInfCompare());
                codeColumnInf = new Dictionary<string, List<ColumnInf>>();
                codes = correctList.Select(i => i.Code).Distinct().ToList();
                foreach (string code in codes)
                {
                    List<ColumnInf> columnInfs = correctList.Where(i => i.Code == code).Select(i => i).ToList();
                    codeColumnInf.Add(code, columnInfs);
                }
                foreach (var item in codeColumnInf)
                {
                    List<ColumnInf> columnInfs = item.Value;
                    if (columnInfs == null && columnInfs.Count == 0)
                    {
                        continue;
                    }
                    string currentCode = columnInfs.First().Code;
                    ColumnTableRecordInfo ctri = dataSource.ColumnTableRecordInfos.Where(i => i.Code == currentCode).Select(i => i).First();
                    TreeNode codeSpecNode = correctParentNode.Nodes.Add(currentCode + "("+ columnInfs.Count+")"+" "+ ctri.Spec);
                    codeSpecNode.ForeColor= Color.FromArgb(0, 255, 0);
                    int index = 1;
                    for (int i=0;i< columnInfs.Count;i++)
                    {
                        if(columnInfs[i].HasOrigin)
                        {
                            columnInfs[i].Text = currentCode + "-" + index++;
                            TreeNode leafNode = codeSpecNode.Nodes.Add(columnInfs[i].Text);
                            leafNode.Tag = columnInfs[i];
                            leafNode.ForeColor = correctColor;
                            columnInfs.RemoveAt(i);
                            break;
                        }                        
                    }
                    ThProgressBar.MeterProgress();
                    foreach (ColumnInf columnInf in columnInfs)
                    {
                        columnInf.Text = currentCode + "-" + index++;
                        TreeNode leafNode = codeSpecNode.Nodes.Add(columnInf.Text);
                        leafNode.Tag = columnInf;
                        leafNode.ForeColor= correctColor;
                    }
                    ThProgressBar.MeterProgress();
                }
            }
            TreeNode codeEmptyParentNode = tn.Nodes.Add(this.codeLostNodeName, "识别异常：柱平法缺失(" + codeEmptyList.Count + ")");
            System.Drawing.Color lostColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.ColumnLost);
            codeEmptyParentNode.ForeColor = lostColor;
            if (codeEmptyList.Count > 0)
            {
                for (int i = 1; i <= codeEmptyList.Count; i++)
                {
                    TreeNode codeEmptyNode = codeEmptyParentNode.Nodes.Add(i.ToString());
                    codeEmptyNode.Tag = codeEmptyList[i - 1];
                    codeEmptyNode.ForeColor = lostColor;
                }
            }
            ThProgressBar.MeterProgress();
            TreeNode uncompleteParentNode = tn.Nodes.Add(this.uncompleteNodeNme, "识别异常：平法参数识别不全(" + infCompleteList.Count + ")");
            System.Drawing.Color unCompletedColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.ParameterNotFull);
            uncompleteParentNode.ForeColor = unCompletedColor;
            if (infCompleteList.Count > 0)
            {
                infCompleteList.Sort(new ColumnInfCompare());
                codeColumnInf = new Dictionary<string, List<ColumnInf>>();
                List<string> infCompleteCodes = infCompleteList.Select(i => i.Code).Distinct().ToList();
                foreach (string code in infCompleteCodes)
                {
                    List<ColumnInf> columnInfs = infCompleteList.Where(i => i.Code == code).Select(i => i).ToList();
                    if (columnInfs == null || columnInfs.Count == 0)
                    {
                        continue;
                    }
                    codeColumnInf.Add(code, columnInfs);
                }
                foreach (var item in codeColumnInf)
                {
                    if(item.Value.Count==0)
                    {
                        continue;
                    }
                    ColumnTableRecordInfo ctri = dataSource.ColumnTableRecordInfos.Where(i => i.Code == item.Value[0].Code).Select(i => i).First();
                    TreeNode codeNode = uncompleteParentNode.Nodes.Add(item.Key+"("+ item.Value.Count+")");
                    codeNode.ForeColor = unCompletedColor;
                    for (int i = 1; i <= item.Value.Count; i++)
                    {
                        TreeNode leafNode = codeNode.Nodes.Add(i.ToString());
                        leafNode.Tag = item.Value[i - 1];
                        leafNode.ForeColor= unCompletedColor;
                    }
                }
                ThProgressBar.MeterProgress();
            }
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
        #region----------双击树节点------------
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
            ShowSelectNodeFrameIds(treeNode);
        }
        private TreeNode TraverseRoot(TreeNode treeNode)
        {
            if(treeNode==null)
            {
                return null;
            }
            if(treeNode.Tag!=null)
            {
                if(treeNode.Tag is ThStandardSignManager)
                {
                    return treeNode;
                }
            }
            return TraverseRoot(treeNode.Parent);
        }
        public TreeNode TraverseInnerFrameRoot(TreeNode treeNode)
        {
            if (treeNode == null)
            {
                return null;
            }
            if (treeNode.Tag != null)
            {
                if (treeNode.Tag is ThStandardSign)
                {
                    return treeNode;
                }
            }
            return TraverseInnerFrameRoot(treeNode.Parent);
        }
        public void ShowSelectNodeFrameIds(TreeNode treeNode)
        {
            HideTotalFrameIds(treeNode);
            List<ObjectId> frameIds = new List<ObjectId>();
            TraverseNode(treeNode, ref frameIds);
            ShowHideFrameIds(frameIds, true);
            TreeNode thStandardSignNode = FindThStandardSignNode(treeNode);
            if (thStandardSignNode != null)
            {
                ThStandardSign thStandardSign = thStandardSignNode.Tag as ThStandardSign;
                LocateInnerFrame(thStandardSign);
            }
        }
        private TreeNode FindThStandardSignNode(TreeNode tn)
        {
            if(tn.Tag!=null && tn.Tag.GetType()==typeof(ThStandardSign))
            {
                return tn;
            }
            if(tn.Parent==null)
            {
                return null;
            }
            return FindThStandardSignNode(tn.Parent);
        }
        private void LocateInnerFrame(ThStandardSign thStandardSign)
        {
            Document document = acadApp.Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock= document.LockDocument())
            {
                ThColumnInfoUtils.ZoomWin(ThColumnInfoUtils.GetMdiActiveDocument().Editor,
                    thStandardSign.Br.Bounds.Value.MinPoint, thStandardSign.Br.Bounds.Value.MaxPoint);
            }    
        }
        public void HideTotalFrameIds(TreeNode currentNode)
        {
            bool isCurrentDocument = CheckRootNodeIsCurrentDocument(currentNode);
            if (isCurrentDocument == false)
            {
                return;
            }
            if (this.tvCheckRes.Nodes.Count ==0)
            {
                return;
            }
            TreeNode rootNode = TraverseRoot(currentNode);
            List<ObjectId> totalFrameIds = new List<ObjectId>();
            TraverseNode(rootNode, ref totalFrameIds);
            ShowHideFrameIds(totalFrameIds, false);
        }
        public bool GetTreeNodeHasVisibleFrame(TreeNode currentNode)
        {
            bool has = false;
            if (this.tvCheckRes.Nodes.Count == 0)
            {
                return has;
            }
            TreeNode rootNode = TraverseRoot(currentNode);
            List<ObjectId> totalFrameIds = new List<ObjectId>();
            TraverseNode(rootNode, ref totalFrameIds);
            Document document = acadApp.Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = document.TransactionManager.StartTransaction())
            {
                foreach (ObjectId objId in totalFrameIds)
                {
                    if (objId == ObjectId.Null || objId.IsErased || objId.IsValid == false)
                    {
                        continue;
                    }
                    Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                    if(ent.Visible)
                    {
                        has = true;
                        break;
                    }
                }
                trans.Commit();
            }
            return has;
        }
        private void ShowHideFrameIds(List<ObjectId> frameIds,bool visible)
        {
            Document document = acadApp.Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = document.LockDocument())
            {
                using (Transaction trans = document.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objId in frameIds)
                    {
                        if (objId == ObjectId.Null || objId.IsErased || objId.IsValid == false)
                        {
                            continue;
                        }
                        Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                        ent.UpgradeOpen();
                        ent.Visible = visible;
                        ent.DowngradeOpen();
                    }
                    trans.Commit();
                }
            }
        }
        #endregion
        private void tvCheckRes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode treeNode = e.Node;            
            if (treeNode == null)
            {
                return;
            }
            bool isCurrentDocument = CheckRootNodeIsCurrentDocument(treeNode);
            if (isCurrentDocument == false)
            {
                return;
            }
            Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
            if (this.currentNode!=null && CheckRootNodeIsCurrentDocument(this.currentNode))
            {
                if (this.currentNode != treeNode)
                {
                    TreeNode innerFrameNode = TraverseInnerFrameRoot(this.currentNode);
                    if (innerFrameNode!=null && innerFrameNode.Tag != null)
                    {                        
                        ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
                        if (thStandardSign.SignPlantCalData != null)
                        {
                            using (DocumentLock docLock = doc.LockDocument())
                            {
                                thStandardSign.SignPlantCalData.ShowFrameTextIds(true,true,false);
                            }
                        }
                    }
                    bool needHide = GetTreeNodeHasVisibleFrame(innerFrameNode);
                    if (needHide)
                    {
                        HideTotalFrameIds(innerFrameNode);
                        doc.Editor.Regen();
                    }
                    ShowDetailData(treeNode);
                }
            }
            this.currentNode = treeNode;
            this.tvCheckRes.SelectedNode = e.Node;
            this.tvCheckRes.ContextMenuStrip = null;
            if (e.Node.Tag != null)
            {
                if(e.Node.Tag.GetType() == typeof(ThStandardSignManager))
                {
                    this.tvCheckRes.ContextMenuStrip = this.contextMenuStrip2;
                }
                else if(e.Node.Tag.GetType() == typeof(ThStandardSign))
                {
                    this.tvCheckRes.ContextMenuStrip = this.contextMenuStrip1;
                }
            }
            if(e.Button == MouseButtons.Left)
            {
                if (e.Node.Tag != null && e.Node.Tag.GetType() == typeof(ColumnInf))
                {
                    if (treeNode.Parent != null && this.nodeKeys.IndexOf(treeNode.Parent.Name) >= 0)
                    {
                        if (treeNode.Tag != null && treeNode.Tag.GetType() == typeof(ColumnInf))
                        {
                            ColumnInf columnInf = treeNode.Tag as ColumnInf;
                            if (columnInf != null && !string.IsNullOrEmpty(columnInf.Code) && DataPalette._dateResult != null)
                            {
                                foreach (DataGridViewRow row in DataPalette._dateResult.dgvColumnTable.Rows)
                                {
                                    row.Selected = false;
                                }
                                foreach (DataGridViewRow row in DataPalette._dateResult.dgvColumnTable.Rows)
                                {
                                    if (row.Cells["code"].Value.ToString() == columnInf.Code)
                                    {
                                        row.Selected = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (this.nodeKeys.IndexOf(treeNode.Name) >= 0)
                    {
                        //ToDo
                    }
                }
            }
        }
        private void FillColumnDataToTreeView(ThStandardSignManager tsm)
        {
            if(tsm==null)
            {
                return;
            }
            TreeNode docNode = null;
            if(this.tvCheckRes.Nodes.Count>0)
            {
                foreach(TreeNode tn in this.tvCheckRes.Nodes)
                {
                    if(tn.Tag!=null)
                    {
                        ThStandardSignManager currentTsm = tn.Tag as ThStandardSignManager;
                        if(currentTsm.DocPath== tsm.DocPath)
                        {
                            docNode = tn;
                            docNode.Nodes.Clear();
                            break;
                        }
                    }
                }
            }
            if(docNode==null)
            {
                docNode = this.tvCheckRes.Nodes.Add(tsm.DocName);
            }
            docNode.ForeColor = Color.FromArgb(255, 255, 255);
            docNode.Tag = tsm;
            var enumerator = tsm.GetEnumerator();
            while(enumerator.MoveNext())
            {
                ThStandardSign thStandardSign= enumerator.Current as ThStandardSign;
                TreeNode subNode= docNode.Nodes.Add(thStandardSign.InnerFrameName);
                subNode.ForeColor = Color.FromArgb(255, 255, 255);
                subNode.Tag = thStandardSign;
                UpdateCheckResult(subNode, thStandardSign);
            }
            if(docNode!=null && !docNode.IsExpanded)
            {
                docNode.Expand();
            }
        }
        private void TraverseNode(TreeNode tn,ref List<ObjectId> columnIds)
        {
            if(tn.Tag!=null)
            {
                ObjectId frameId = ObjectId.Null;
                if (tn.Tag is ColumnInf columnInf)
                {
                    frameId = columnInf.FrameId;
                }
                else if(tn.Tag is ObjectId objId)
                {
                    frameId = objId;
                }
                if (frameId != ObjectId.Null && !frameId.IsErased && frameId.IsValid)
                {
                    columnIds.Add(frameId);
                }
            }
            if(tn.Nodes.Count==0)
            {
                return;
            }
            foreach(TreeNode tnItem in tn.Nodes)
            {
                TraverseNode(tnItem, ref columnIds);
            }
        }
        public TreeNode FindTreeCode(string innerFrameName, FindNodeMode findNodeMode, string codeText, string subcodeText)
        {
            TreeNode findTreeCode = null;
            foreach (TreeNode tn in CheckPalette._checkResult.tvCheckRes.Nodes[0].Nodes)
            {
                if (tn.Text != innerFrameName)
                {
                    continue;
                }
                if(findNodeMode== FindNodeMode.InnerFrame)
                {
                    return tn;
                }
                foreach (TreeNode firstNode in tn.Nodes)
                {
                    //从数据正确节点查找
                    if (firstNode.Name == CheckPalette._checkResult.DataCorrectNodeName)
                    {
                        foreach (TreeNode secondNode in firstNode.Nodes)
                        {
                            if (secondNode.Text.IndexOf(codeText) >= 0)
                            {
                                if (findNodeMode == FindNodeMode.Code)
                                {
                                    return secondNode;
                                }
                                if(!string.IsNullOrEmpty(subcodeText))
                                {
                                    foreach (TreeNode thirdNode in secondNode.Nodes)
                                    {
                                        if (thirdNode.Text.IndexOf(subcodeText) >= 0)
                                        {
                                            if (findNodeMode == FindNodeMode.SubCode)
                                            {
                                                return thirdNode;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            }
            return findTreeCode;
        }
        /// <summary>
        /// 校核
        /// </summary>
        private void ColumnCheck()
        {
            if (this.tvCheckRes.SelectedNode == null)
            {
                return;
            }
            bool isCurrentDocument = CheckRootNodeIsCurrentDocument(this.tvCheckRes.SelectedNode);
            if(!isCurrentDocument)
            {
                MessageBox.Show("在面板上所选节点的文档和当前不档不一致，无法识别");
                return;
            }
            //收集目录树上记录的ColumnFrameIdCollection
            List<ObjectId> treeColumnIds = new List<ObjectId>();
            TraverseNode(this.tvCheckRes.SelectedNode, ref treeColumnIds);
            Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = doc.LockDocument())
            {
                List<string> lockedLayerNames = ThColumnInfoUtils.UnlockedAllLayers();
                try
                {
                    ThProgressBar.Start("正在校核......" );
                    ThProgressBar.MeterProgress();
                    TreeNode tn = this.tvCheckRes.SelectedNode;
                    //删除ColumnFrameIdCollection
                    ThColumnInfoUtils.EraseObjIds(treeColumnIds.ToArray());
                    if (tn.Tag != null)
                    {
                        if (tn.Tag.GetType() == typeof(ThStandardSign))
                        {
                            //更新当前节点数据(识别图纸中柱子和柱表)
                            ThStandardSign thStandardSign = tn.Tag as ThStandardSign;
                            ThStandardSignManager.UpdateThStandardSign(thStandardSign);
                            UpdateCheckResult(tn, thStandardSign);
                            ShowDetailData(true); 
                        }
                        else if (tn.Tag.GetType() == typeof(ThStandardSignManager))
                        {
                            ThStandardSignManager tsm = ThStandardSignManager.LoadData("", true);
                            tn.Nodes.Clear();
                            FillColumnDataToTreeView(tsm);
                            if(tn.Nodes.Count>0)
                            {
                                this.tvCheckRes.SelectedNode = tn.Nodes[0];
                                ShowDetailData(true);
                            }
                        }
                    }
                    ThProgressBar.MeterProgress();
                }
                catch (Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "");
                }
                finally
                {
                    if (lockedLayerNames.Count > 0)
                    {
                        ThColumnInfoUtils.LockedLayers(lockedLayerNames);
                    }
                    ThProgressBar.Stop();
                }
            }
        }
        private void UpdateCheckResult(TreeNode tn, ThStandardSign thStandardSign)
        {
            try
            {
                //如果导入过计算书，则埋入数据
                if (thStandardSign.SignPlantCalData != null)
                {
                    thStandardSign.SignPlantCalData.EraseFrameTextIds();
                    thStandardSign.SignPlantCalData.Embed(); //埋入
                    thStandardSign.RelateColumnFrameId(); //关联
                }
                else
                {
                    thStandardSign.SignExtractColumnInfo.PrintColumnFrame();
                    //如果是校核，且没导入过计算书，则要设置默认值
                    PlantCalDataToDraw plantCalDataToDraw = new PlantCalDataToDraw(thStandardSign);
                    plantCalDataToDraw.DrawColumnOriginFrame();
                }
                //将识别的结果更新到面板
                tn.Nodes.Clear();
                FillDrawCheckInfToTreeView(thStandardSign.SignExtractColumnInfo, tn);
                FillPlantCalResultToTree(tn);

                List<ColumnInf> correctColumnInfs = GetDataCorrectColumnInfs(tn);
                //校核柱子
                if (thStandardSign.SignPlantCalData == null ||
                    thStandardSign.SignPlantCalData.CalInfo == null)
                {
                    thStandardSign.Validate(true, correctColumnInfs);
                }
                else
                {
                    thStandardSign.Validate(false, correctColumnInfs);
                }
                SortThCalculateValidateResult(thStandardSign, correctColumnInfs);
                tn.Expand();
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "UpdateCheckResult");
            }
        }
        private void SortThCalculateValidateResult(ThStandardSign thStandardSign,List<ColumnInf> correctSortInfs)
        {
            if(thStandardSign.ThCalculateValidate==null)
            {
                return;
            }
            Dictionary<ColumnRelateInf, List<string>> newResDic = new Dictionary<ColumnRelateInf, List<string>>();
            foreach (ColumnInf columnInf in correctSortInfs)
            {
                var item = thStandardSign.ThCalculateValidate.ColumnValidateResultDic.Where(i => i.Key.ModelColumnInfs.Count == 1 && i.Key.ModelColumnInfs[0].Code == columnInf.Code &&
                  i.Key.ModelColumnInfs[0].Text == columnInf.Text).Select(i => i).First();
                if (item.Key != null)
                {
                    newResDic.Add(item.Key, item.Value);
                }
            }
            thStandardSign.ThCalculateValidate.ColumnValidateResultDic = newResDic;
        }
        public List<ColumnInf> GetDataCorrectColumnInfs(TreeNode tn)
        {
            List<ColumnInf> columnInfs = new List<ColumnInf>();
            if(tn.Tag!=null && tn.Tag.GetType()==typeof(ColumnInf))
            {
                ColumnInf columnInf = tn.Tag as ColumnInf;
                if(columnInf.Error==ErrorMsg.OK)
                {
                    columnInfs.Add(columnInf);
                }
            }
            foreach(TreeNode treeNode in tn.Nodes)
            {
                List<ColumnInf> subColumnInfs = GetDataCorrectColumnInfs(treeNode);
                columnInfs.AddRange(subColumnInfs);
            }
            return columnInfs;
        }
        private static TreeNode FindRootNode(TreeNode tn)
        {
            if(tn.Parent==null)
            {
                return tn;
            }
            else
            {
               return FindRootNode(tn.Parent);
            }
        }
        public static bool CheckRootNodeIsCurrentDocument(TreeNode treeNode)
        {
            TreeNode rootNode = CheckResult.FindRootNode(treeNode);
            ThStandardSignManager tssm = rootNode.Tag as ThStandardSignManager;
            if(acadApp.Application.DocumentManager.MdiActiveDocument.Name== tssm.DocPath)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 导入计算书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importCalculationTsmi_Click(object sender, EventArgs e)
        {
            TreeNode tn = this.tvCheckRes.SelectedNode as TreeNode;
            bool isCurrentDocument = CheckRootNodeIsCurrentDocument(this.tvCheckRes.SelectedNode);
            if (!isCurrentDocument)
            {
                MessageBox.Show("在面板上所选节点的文档和当前不档不一致，无法识别");
                return;
            }
            if (tn.Tag != null)
            {
                if (tn.Tag.GetType() == typeof(ThStandardSign))
                {
                    ThStandardSign thStandardSign = tn.Tag as ThStandardSign;
                    Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
                    using (DocumentLock docLock = doc.LockDocument())
                    {
                        ThProgressBar.Start("导入计算书...");
                        ThProgressBar.MeterProgress();
                        try
                        {
                            CalculationInfoVM calculationInfoVM = new CalculationInfoVM();
                            if (thStandardSign.SignPlantCalData!=null && thStandardSign.SignPlantCalData.CalInfo!=null)
                            {
                                thStandardSign.SignPlantCalData.ClearFrameIds();
                                ThProgressBar.MeterProgress();
                                thStandardSign.SignPlantCalData.EraseFrameTextIds();
                                ThProgressBar.MeterProgress();
                                calculationInfoVM = new CalculationInfoVM(thStandardSign.SignPlantCalData.CalInfo);
                                calculationInfoVM.CalculateInfo = thStandardSign.SignPlantCalData.CalInfo;
                            }
                            else
                            {
                                calculationInfoVM = new CalculationInfoVM(new CalculationInfo());
                            }
                            ImportCalculation importCalculation = new ImportCalculation(calculationInfoVM);
                            calculationInfoVM.Owner = importCalculation;
                            importCalculation.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            importCalculation.ShowDialog();
                            if (calculationInfoVM.YnExport) 
                            {
                                PlantCalDataToDraw plantData = new PlantCalDataToDraw(calculationInfoVM.CalculateInfo, thStandardSign);
                                thStandardSign.SignPlantCalData = plantData;
                                plantData.Plant();
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ThColumnInfoUtils.WriteException(ex);
                        }
                        finally
                        {
                            ThProgressBar.Stop();
                        }
                    }
                }
            }
        }
        private void FillPlantCalResultToTree(TreeNode innerFrameNode)
        {
            //ToDo
            ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
            if (thStandardSign.SignPlantCalData == null)
            {
                return;
            }
            if(thStandardSign.SignPlantCalData.ColumnFrameIds.Count==0)
            {
                return;
            }
            List<TreeNode> dwgExistCalNotNodes = new List<TreeNode>();
            TreeNode dataCorrectNode = null;
            foreach (TreeNode node in innerFrameNode.Nodes)
            {
                if(node.Name== this.dataCorrectNodeName)
                {
                    dataCorrectNode = node;
                    for (int i = 0; i < dataCorrectNode.Nodes.Count; i++)
                    {
                        TreeNode fourthNode = dataCorrectNode.Nodes[i];
                        for (int j = 0; j < fourthNode.Nodes.Count; j++)
                        {
                            TreeNode fifthNode = fourthNode.Nodes[j];
                            ColumnInf columnInf = fifthNode.Tag as ColumnInf;
                            FrameColor frameColor=  thStandardSign.SignPlantCalData.GetFrameIdColorType(columnInf.FrameId);
                            if(frameColor==FrameColor.DwgHasCalNot)
                            {
                                PlantCalDataToDraw.ChangeColor(columnInf.FrameId, frameColor);
                                dwgExistCalNotNodes.Add(fifthNode);
                                fourthNode.Nodes.Remove(fifthNode);
                                j = j - 1;
                            }
                        }
                    }
                    break;
                }
            }
            ThProgressBar.MeterProgress();
            UpdateDataCorrectNode(dataCorrectNode);
            ThProgressBar.MeterProgress();
            AddDwgHasCalNotNode(innerFrameNode, dwgExistCalNotNodes);
            ThProgressBar.MeterProgress();
            AddDwgNotCalHasNode(innerFrameNode);
            ThProgressBar.MeterProgress();
        }
        private string dwgHasCalNotNodeName = "DwgHasCalNot";
        /// <summary>
        /// 添加图纸有计算书没有的节点
        /// </summary>
        /// <param name="innerFrameNode"></param>
        /// <param name="dwgExistCalNotColumnInfs"></param>
        private void AddDwgHasCalNotNode(TreeNode innerFrameNode, List<TreeNode> dwgExistCalNotColumnInfs)
        {
            if(innerFrameNode.Nodes.ContainsKey(this.dwgHasCalNotNodeName))
            {
                innerFrameNode.Nodes.RemoveByKey(this.dwgHasCalNotNodeName);
            }
            TreeNode dwgHasCalNotNode = innerFrameNode.Nodes.Add(
                this.dwgHasCalNotNodeName, "图有计算书无(" + dwgExistCalNotColumnInfs.Count + ")");
            System.Drawing.Color sysColor= PlantCalDataToDraw.GetFrameSystemColor(FrameColor.DwgHasCalNot);
            dwgHasCalNotNode.ForeColor = sysColor;
            foreach (TreeNode tn in dwgExistCalNotColumnInfs)
            {
                TreeNode leafNode = dwgHasCalNotNode.Nodes.Add(tn.Text);
                leafNode.ForeColor = sysColor;
                leafNode.Tag = tn.Tag;
            }                
        }
        private string dwgNotCalHasNodeName = "DwgNodCalHas";
        private void AddDwgNotCalHasNode(TreeNode innerFrameNode)
        {
            if(innerFrameNode==null || innerFrameNode.Tag==null)
            {
                return;
            }
            ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
            if (innerFrameNode.Nodes.ContainsKey(this.dwgNotCalHasNodeName))
            {
                innerFrameNode.Nodes.RemoveByKey(this.dwgNotCalHasNodeName);
            }
            TreeNode dwgNotCalHasNode = innerFrameNode.Nodes.Add(
                this.dwgNotCalHasNodeName, "图无计算书有(" + thStandardSign.SignPlantCalData.UnrelatedFrameIds.Count + ")");
            System.Drawing.Color sysColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.DwgNotCalHas);
            dwgNotCalHasNode.ForeColor = sysColor;
            for (int i=0;i< thStandardSign.SignPlantCalData.UnrelatedFrameIds.Count;i++)
            {
                TreeNode leafNode = dwgNotCalHasNode.Nodes.Add((i+1).ToString());
                leafNode.ForeColor = sysColor;
                leafNode.Tag = thStandardSign.SignPlantCalData.UnrelatedFrameIds[i];
            }
        }
        /// <summary>
        /// 更新数据正确节点
        /// </summary>
        /// <param name="dataCorrectNode"></param>
        private void UpdateDataCorrectNode(TreeNode dataCorrectNode)
        {
            if(dataCorrectNode==null || dataCorrectNode.Nodes.Count==0)
            {
                return;
            }
            int totalNum = 0;
            for (int i=0; i< dataCorrectNode.Nodes.Count;i++)
            {
                TreeNode columnNode = dataCorrectNode.Nodes[i];
                if(columnNode.Nodes.Count==0)
                {
                    dataCorrectNode.Nodes.Remove(columnNode);
                    i = i - 1;
                }
                else
                {
                    totalNum += columnNode.Nodes.Count;
                    string columnCode = columnNode.Text.Substring(0, columnNode.Text.IndexOf("("));
                    string columnSpec = columnNode.Text.Substring(columnNode.Text.IndexOf(")")+1);
                    columnNode.Text = columnCode + "(" + columnNode.Nodes.Count + ")" + columnSpec;
                }
            }
            dataCorrectNode.Text = "匹配成功：数据正确(" + totalNum + ")";
        }
        private void checkTsmi_Click(object sender, EventArgs e)
        {
            ColumnCheck();
        }
        private void distinguishAllTsmi_Click(object sender, EventArgs e)
        {
            ColumnCheck();
        }

        private void detailDataTsmi_Click(object sender, EventArgs e)
        {
            ShowDetailData();
        }
        /// <summary>
        /// 显示详细数据
        /// </summary>
        private void ShowDetailData(bool forceShow=false)
        {
            if (this.tvCheckRes.SelectedNode == null)
            {
                MessageBox.Show("请选择要查看详细数据的层节点");
                return;
            }
            if(forceShow)
            {
                DataPalette.ShowPaletteMark = false;
            }
            DataPalette.ShowPaletteMark = !DataPalette.ShowPaletteMark;
            SwitchShowDetailPicture();
            ShowDetailData(this.tvCheckRes.SelectedNode);
            DataPalette._ps.Visible = DataPalette.ShowPaletteMark;
        }
        private void ShowDetailData(TreeNode tn)
        {
            if(tn==null)
            {
                return;
            }
            ExportDetailData(tn);
            this.lastShowDetailNode = tn;
        }
        /// <summary>
        /// 导出详细数据
        /// </summary>
        /// <param name="tn"></param>
        public void ExportDetailData(TreeNode tn)
        {
            if(tn==null)
            {
                return;
            }
            TreeNode innerFrameNode = TraverseInnerFrameRoot(tn);
            if(innerFrameNode==null)
            {
                return;
            }
            ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
            DataPalette.Instance.Show(thStandardSign.SignExtractColumnInfo, thStandardSign.ThSpecificValidate,
                    thStandardSign.ThCalculateValidate, tn);
        }
        /// <summary>
        /// 切换详细数据按钮图片
        /// </summary>
        public void SwitchShowDetailPicture()
        {
            try
            {
                if (DataPalette.ShowPaletteMark)
                {
                    this.btnShowDetailData.BackgroundImage = Properties.Resources.DetailDataHidePng;
                }
                else
                {
                    this.btnShowDetailData.BackgroundImage = Properties.Resources.DetailDataShowPng;
                }
            }
            catch(Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "SwitchShowDetailPicture");
            }
        }
        private void ShowComponentPropPicture()
        {
            try
            {
                btnComponentDefinition.BackgroundImage = Properties.Resources.ComponentPropModifyPng;
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "SwitchShowDetailPicture");
            }
        }
        private void MakeButtonTransparent(Button btn)
        {
            Bitmap bm = (Bitmap)btn.Image;
            bm.MakeTransparent(bm.GetPixel(0, 0));
        }

        private void parameterSetTsmi_Click(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            acadApp.Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("\x03\x03" + "ThCpi" + " ", true, false, false);
        }
        private void tvCheckRes_MouseLeave(object sender, EventArgs e)
        {
            if(tvCheckRes.SelectedNode==null)
            {
                return;
            }
            bool isCurrentDocument = CheckRootNodeIsCurrentDocument(tvCheckRes.SelectedNode);
            if(isCurrentDocument==false)
            {
                return;
            }
        }
        private void btnComponentDefinition_Click(object sender, EventArgs e)
        {
            Document document = acadApp.Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock= document.LockDocument())
            {
                PlantCalDataToDraw plantCal = new PlantCalDataToDraw();
                plantCal.GetEmbededColumnIds();
                if (plantCal.EmbededColumnIds.Count == 0)
                {
                    MessageBox.Show("未能发现任何埋入的柱子实体，请执行计算书导入命令");
                    return;
                }
                try
                {
                    if(ComponentPropDefine.isOpened)
                    {
                        MessageBox.Show("构件属性修改窗体已打开!");
                        return;
                    }
                    ComponentPropDefineVM componentPropDefineVM = new ComponentPropDefineVM();
                    ComponentPropDefine componentPropDefine = new ComponentPropDefine(componentPropDefineVM);
                    componentPropDefineVM.Owner = componentPropDefine;
                    componentPropDefine.Topmost = true;
                    componentPropDefine.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    componentPropDefine.Show();
                    componentPropDefineVM.SelectModify();
                }
                catch(System.Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "btnComponentDefinition_Click");
                }
            }
        }

        private void btnShowDetailData_Click(object sender, EventArgs e)
        {
            ShowDetailData();
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
        }

        private void tvCheckRes_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode selectedNode = this.tvCheckRes.SelectedNode;
            if (selectedNode == null)
                return;
            if (cnt > 1)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        private void tvCheckRes_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode selectedNode = this.tvCheckRes.SelectedNode;
            if (selectedNode == null)
                return;
            if (cnt > 1)
                e.Cancel = true;
            else
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
