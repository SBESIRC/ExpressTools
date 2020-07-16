using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThColumnInfo.Service;
using ThColumnInfo.View;

namespace ThColumnInfo.ViewModel
{
    /// <summary>
    /// 用于与CheckResult更新数据
    /// </summary>
    public class CheckResultVM
    {
        private CheckResult owner;
        private TreeNode lastShowDetailNode = null;
        private List<string> nodeKeys = new List<string>();
        private string dataCorrectNodeName = "DataCorrect";
        private string codeLostNodeName = "CodeLost";
        private string uncompleteNodeNme = "Uncomplete";
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
            get
            {
                return dataCorrectNodeName;
            }
        }
        public CheckResultVM(CheckResult owner)
        {
            this.owner = owner;
            this.nodeKeys = new List<string> { this.dataCorrectNodeName, this.codeLostNodeName, this.uncompleteNodeNme };
        }
        private string dwgHasCalNotNodeName = "DwgHasCalNot";
        /// <summary>
        /// 添加图纸有计算书没有的节点
        /// </summary>
        /// <param name="innerFrameNode"></param>
        /// <param name="dwgExistCalNotColumnInfs"></param>
        private void AddDwgHasCalNotNode(TreeNode innerFrameNode)
        {
            List<ColumnInf> columnInfs = new List<ColumnInf>();
            if (innerFrameNode == null || innerFrameNode.Tag == null)
            {
                return;
            }
            if (innerFrameNode.Nodes.ContainsKey(this.dwgHasCalNotNodeName))
            {
                innerFrameNode.Nodes.RemoveByKey(this.dwgHasCalNotNodeName);
            }
            ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
            if (thStandardSign.SignPlantCalData != null)
            {
                columnInfs = thStandardSign.SignPlantCalData.DwgHasCalNotColumns;
            }
            TreeNode dwgHasCalNotNode = innerFrameNode.Nodes.Add(
                this.dwgHasCalNotNodeName, "图有计算书无(" + columnInfs.Count + ")");
            System.Drawing.Color sysColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.DwgHasCalNot);
            dwgHasCalNotNode.ForeColor = sysColor;
            List<string> codes = columnInfs.Select(i => i.Code).Distinct().ToList();
            codes.Sort();
            Dictionary<string, List<ColumnInf>> codeColumnInf = new Dictionary<string, List<ColumnInf>>();
            foreach (string code in codes)
            {
                List<ColumnInf> tempColumnInfs = columnInfs.Where(i => i.Code == code).Select(i => i).ToList();
                if (columnInfs == null || columnInfs.Count == 0)
                {
                    continue;
                }
                codeColumnInf.Add(code, tempColumnInfs);
            }
            foreach (var item in codeColumnInf)
            {
                List<ColumnInf> tempColumnInfs = item.Value;
                if (tempColumnInfs == null && tempColumnInfs.Count == 0)
                {
                    continue;
                }
                string currentCode = item.Key;
                int index = 1;
                for (int i = 0; i < tempColumnInfs.Count; i++)
                {
                    if (tempColumnInfs[i].HasOrigin)
                    {
                        tempColumnInfs[i].Text = currentCode + "-" + index++;
                        TreeNode leafNode = dwgHasCalNotNode.Nodes.Add(tempColumnInfs[i].Text);
                        leafNode.Tag = tempColumnInfs[i];
                        leafNode.ForeColor = sysColor;
                        tempColumnInfs.RemoveAt(i);
                        break;
                    }
                }
                tempColumnInfs.Sort(new ColumnCordCompare());
                foreach (ColumnInf columnInf in tempColumnInfs)
                {
                    columnInf.Text = currentCode + "-" + index++;
                    TreeNode leafNode = dwgHasCalNotNode.Nodes.Add(columnInf.Text);
                    leafNode.Tag = columnInf;
                    leafNode.ForeColor = sysColor;
                }
            }
            ThProgressBar.MeterProgress();
        }
        private string dwgNotCalHasNodeName = "DwgNotCalHas";
        private void AddDwgNotCalHasNode(TreeNode innerFrameNode)
        {
            if (innerFrameNode == null || innerFrameNode.Tag == null)
            {
                return;
            }
            ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
            if (innerFrameNode.Nodes.ContainsKey(this.dwgNotCalHasNodeName))
            {
                innerFrameNode.Nodes.RemoveByKey(this.dwgNotCalHasNodeName);
            }
            List<ObjectId> objIds = new List<ObjectId>();
            if (thStandardSign.SignPlantCalData != null)
            {
                objIds = thStandardSign.SignPlantCalData.DwgNotCalHasFrameIds;
            }
            TreeNode dwgNotCalHasNode = innerFrameNode.Nodes.Add(
                this.dwgNotCalHasNodeName, "图无计算书有(" + objIds.Count + ")");
            System.Drawing.Color sysColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.DwgNotCalHas);
            dwgNotCalHasNode.ForeColor = sysColor;
            objIds.Sort(new ColumnPolylineCompare());
            for (int i = 0; i < objIds.Count; i++)
            {
                ObjectId objId = objIds[i];
                if (objId == ObjectId.Null || objId.IsErased || !objId.IsValid)
                {
                    continue;
                }
                TreeNode leafNode = dwgNotCalHasNode.Nodes.Add((i + 1).ToString());
                leafNode.ForeColor = sysColor;
                leafNode.Tag = objId;
            }
        }
        public void AddColumnTableCmd()
        {

        }
        /// <summary>
        /// 校核
        /// </summary>
        public void Check()
        {
            if (owner.tvCheckRes.SelectedNode == null)
            {
                return;
            }
            //收集目录树上记录的ColumnFrameIdCollection
            List<ObjectId> treeColumnIds = new List<ObjectId>();
            TraverseNode(owner.tvCheckRes.SelectedNode, ref treeColumnIds);
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            using (DocumentLock docLock = doc.LockDocument())
            {
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    List<string> lockedLayerNames = ThColumnInfoUtils.UnlockedAllLayers();
                    try
                    {
                        ThProgressBar.Start("正在校核......");
                        ThProgressBar.MeterProgress();
                        //删除ColumnFrameIdCollection
                        ThColumnInfoUtils.EraseObjIds(treeColumnIds.ToArray());
                        //更新当前节点数据(识别图纸中柱子和柱表)
                        ThStandardSign thStandardSign = owner.tvCheckRes.SelectedNode.Tag as ThStandardSign;
                        UpdateCheckResult(owner.tvCheckRes.SelectedNode, thStandardSign);
                        ShowDetailData(true);
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
                    trans.Commit();
                }
            }
        }
        public void CheckAll()
        {
            //收集目录树上记录的ColumnFrameIdCollection
            List<ObjectId> treeColumnIds = new List<ObjectId>();
            foreach(TreeNode node in owner.tvCheckRes.Nodes)
            {
                TraverseNode(node, ref treeColumnIds);
            }            
            Document doc = ThColumnInfoUtils.GetMdiActiveDocument();
            using (DocumentLock docLock = doc.LockDocument())
            {
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    List<string> lockedLayerNames = ThColumnInfoUtils.UnlockedAllLayers();
                    try
                    {
                        ThProgressBar.Start("正在校核......");
                        ThProgressBar.MeterProgress();
                        //删除ColumnFrameIdCollection
                        ThColumnInfoUtils.EraseObjIds(treeColumnIds.ToArray());
                        ThStandardSignManager tm = ThColunmDocManager.GetThStandardSignManager(doc.Name);
                        ThStandardSignManager.LoadData(tm);
                        FillColumnDataToTreeView(tm, false);
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
                    trans.Commit();
                }
            }
        }
        public void ComponentDefinitionCmd()
        {
            if (ComponentPropDefine.isOpened)
            {
                return;
            }
            List<ObjectId> showObjIds = new List<ObjectId>();
            Document document = ThColumnInfoUtils.GetMdiActiveDocument();
            using (DocumentLock docLock = document.LockDocument())
            {
                try
                {
                    PlantCalDataToDraw plantCal = new PlantCalDataToDraw();
                    plantCal.GetEmbededColumnIds();
                    if (plantCal.EmbededColumnIds.Count == 0)
                    {
                        MessageBox.Show("未能发现任何埋入的柱子实体，请运行【计算书导入】或【校核】命令后再执行此操作!");
                        return;
                    }
                    showObjIds = CheckResultCadInteractive.GetShowObjIds();
                    if (showObjIds.Count > 0)
                    {
                        ThColumnInfoUtils.ShowObjIds(showObjIds, false);
                    }
                    ComponentPropDefineVM componentPropDefineVM = new ComponentPropDefineVM();
                    ComponentPropDefine componentPropDefine = new ComponentPropDefine(componentPropDefineVM);
                    componentPropDefineVM.Owner = componentPropDefine;
                    componentPropDefine.Topmost = true;
                    componentPropDefine.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    componentPropDefine.Show();
                    componentPropDefineVM.SelectModify();
                }
                catch (System.Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "btnComponentDefinition_Click");
                }
                finally
                {
                    ThColumnInfoUtils.ShowObjIds(showObjIds, true);
                }
            }
        }
        public static bool CheckRootNodeIsCurrentDocument(TreeNode treeNode)
        {
            if (treeNode == null)
            {
                return false;
            }
            TreeNode rootNode = FindRootNode(treeNode);
            ThStandardSignManager tssm = rootNode.Tag as ThStandardSignManager;
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            if (doc.Name == tssm.DocPath)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 导出详细数据
        /// </summary>
        /// <param name="tn"></param>
        public void ExportDetailData(TreeNode tn)
        {
            if (tn == null)
            {
                return;
            }
            TreeNode innerFrameNode = TraverseRoot(tn);
            if (innerFrameNode == null)
            {
                return;
            }
            foreach (TreeNode treeNode in tn.Nodes)
            {
                if (treeNode.Name == this.dataCorrectNodeName)
                {
                    ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
                    DataPalette.Instance.Show(thStandardSign.SignExtractColumnInfo, thStandardSign.ThSpecificValidate,
                            thStandardSign.ThCalculateValidate, treeNode);
                    break;
                }
            }
        }
        public void ExportExcelCmd()
        {

        }
        /// <summary>
        /// 查找树节点
        /// </summary>
        /// <param name="innerFrameName">内框名称</param>
        /// <param name="findNodeMode">查找节点</param>
        /// <param name="codeText">编号 KZ</param>
        /// <param name="subcodeText">子编号 KZ-1</param>
        /// <returns></returns>
        public TreeNode FindTreeCode(string innerFrameName, FindNodeMode findNodeMode, string codeText, string subcodeText)
        {
            //查找内框节点
            TreeNode innerFrameNode = null;
            foreach (TreeNode tn in CheckPalette._checkResult.tvCheckRes.Nodes[0].Nodes)
            {
                if (tn.Text == innerFrameName)
                {
                    innerFrameNode = tn;
                    break;
                }
            }
            if (findNodeMode == FindNodeMode.InnerFrame || innerFrameNode == null)
            {
                return innerFrameNode;
            }

            //查找数据正确节点
            TreeNode dataCorrectNode = null;
            foreach (TreeNode tn in innerFrameNode.Nodes)
            {
                //从数据正确节点查找
                if (tn.Name == this.dataCorrectNodeName)
                {
                    dataCorrectNode = tn;
                    break;
                }
            }
            if (findNodeMode == FindNodeMode.Match || dataCorrectNode == null)
            {
                return null;
            }

            //查找编号节点或子编号节点
            foreach (TreeNode codeNode in dataCorrectNode.Nodes)
            {
                if (codeNode.Text.Substring(0, codeNode.Text.IndexOf('(')).Trim() == codeText)
                {
                    if (findNodeMode == FindNodeMode.Code)
                    {
                        return codeNode;
                    }
                }
                foreach (TreeNode subCodeNode in codeNode.Nodes)
                {
                    if (subCodeNode.Text == subcodeText)
                    {
                        if (findNodeMode == FindNodeMode.SubCode)
                        {
                            return subCodeNode;
                        }
                    }
                }
            }
            return null;
        }
        private static TreeNode FindRootNode(TreeNode tn)
        {
            if (tn.Parent == null)
            {
                return tn;
            }
            else
            {
                return FindRootNode(tn.Parent);
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
            UpdateDataCorrectNode(innerFrameNode);
            ThProgressBar.MeterProgress();
            AddDwgHasCalNotNode(innerFrameNode);
            ThProgressBar.MeterProgress();
            AddDwgNotCalHasNode(innerFrameNode);
            ThProgressBar.MeterProgress();
        }
        private void FillColumnDataToTreeView(ThStandardSignManager tsm, bool loadTree = true)
        {
            if (tsm == null)
            {
                return;
            }
            owner.tvCheckRes.Nodes.Clear();
            var enumerator = tsm.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ThStandardSign thStandardSign = enumerator.Current as ThStandardSign;
                TreeNode subNode = owner.tvCheckRes.Nodes.Add(thStandardSign.InnerFrameName);
                subNode.ForeColor = Color.FromArgb(255, 255, 255);
                subNode.Tag = thStandardSign;
                if (loadTree)
                {
                    continue;
                }
                UpdateCheckResult(subNode, thStandardSign);
            }
        }
        private void FillDrawCheckInfToTreeView(IDataSource dataSource, TreeNode tn = null, bool ynExportErrorNode = false)
        {
            List<ColumnInf> correctList = new List<ColumnInf>();
            List<ColumnInf> codeEmptyList = new List<ColumnInf>();
            List<ColumnInf> infCompleteList = new List<ColumnInf>();
            if (dataSource == null)
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
            TreeNode correctParentNode = tn.Nodes.Add(this.dataCorrectNodeName, "数据正确(" + correctList.Count + ")");
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
                    ColumnTableRecordInfo ctri = new ColumnTableRecordInfo();
                    var res = dataSource.ColumnTableRecordInfos.Where(i => i.Code == currentCode).Select(i => i);
                    if (res != null && res.Count() > 0)
                    {
                        ctri = res.First();
                    }
                    else
                    {
                        res = dataSource.InvalidCtris.Where(i => i.Code == currentCode).Select(i => i);
                        if (res != null && res.Count() > 0)
                        {
                            ctri = res.First();
                        }
                    }
                    TreeNode codeSpecNode = correctParentNode.Nodes.Add(currentCode + "(" + columnInfs.Count + ")" + " " + ctri.Spec);
                    codeSpecNode.ForeColor = Color.FromArgb(0, 255, 0);
                    int index = 1;
                    for (int i = 0; i < columnInfs.Count; i++)
                    {
                        if (columnInfs[i].HasOrigin)
                        {
                            columnInfs[i].Text = currentCode + "-" + index++;
                            TreeNode leafNode = codeSpecNode.Nodes.Add(columnInfs[i].Text);
                            leafNode.Tag = columnInfs[i];
                            leafNode.ForeColor = correctColor;
                            columnInfs.RemoveAt(i);
                            break;
                        }
                    }
                    columnInfs.Sort(new ColumnCordCompare());
                    foreach (ColumnInf columnInf in columnInfs)
                    {
                        columnInf.Text = currentCode + "-" + index++;
                        TreeNode leafNode = codeSpecNode.Nodes.Add(columnInf.Text);
                        leafNode.Tag = columnInf;
                        leafNode.ForeColor = correctColor;
                    }
                }
            }
            if (!ynExportErrorNode)
            {
                return;
            }
            TreeNode codeEmptyParentNode = tn.Nodes.Add(this.codeLostNodeName, "柱编号缺失(" + codeEmptyList.Count + ")");
            System.Drawing.Color lostColor = PlantCalDataToDraw.GetFrameSystemColor(FrameColor.ColumnLost);
            codeEmptyParentNode.ForeColor = lostColor;
            if (codeEmptyList.Count > 0)
            {
                codeEmptyList.Sort(new ColumnCordCompare());
                for (int i = 1; i <= codeEmptyList.Count; i++)
                {
                    TreeNode codeEmptyNode = codeEmptyParentNode.Nodes.Add(i.ToString());
                    codeEmptyNode.Tag = codeEmptyList[i - 1];
                    codeEmptyNode.ForeColor = lostColor;
                }
            }
            ThProgressBar.MeterProgress();
            TreeNode uncompleteParentNode = tn.Nodes.Add(this.uncompleteNodeNme, "平法参数错误(" + infCompleteList.Count + ")");
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
                    if (item.Value.Count == 0)
                    {
                        continue;
                    }
                    item.Value.Sort(new ColumnCordCompare());
                    TreeNode codeNode = uncompleteParentNode.Nodes.Add(item.Key + "(" + item.Value.Count + ")");
                    codeNode.ForeColor = unCompletedColor;
                    for (int i = 1; i <= item.Value.Count; i++)
                    {
                        TreeNode leafNode = codeNode.Nodes.Add(i.ToString());
                        leafNode.Tag = item.Value[i - 1];
                        leafNode.ForeColor = unCompletedColor;
                    }
                }
                ThProgressBar.MeterProgress();
            }
        }
        private TreeNode GetCurrentDocumentInnerframeNode()
        {
            TreeNode innerFrameNode = null;
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            FileInfo fi = new FileInfo(doc.Name);
            string docName = "";
            if (fi.Exists)
            {
                docName = fi.Name;
            }
            else
            {
                docName = doc.Name;
            }
            foreach (TreeNode tn in owner.tvCheckRes.Nodes)
            {
                if (tn.Tag == null || tn.Tag.GetType() != typeof(ThStandardSign))
                {
                    continue;
                }
                ThStandardSign thStandardSign = tn.Tag as ThStandardSign;
                if (thStandardSign.InnerFrameName == docName)
                {
                    innerFrameNode = tn;
                    break;
                }
            }
            return innerFrameNode;
        }
        /// <summary>
        /// 获取计算书导入路径
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        private CalculationInfo GetCalculationImportPath(TreeNode tn)
        {
            CalculationInfo ci = new CalculationInfo();
            TreeNode rootNode = TraverseRoot(tn);
            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Tag == null)
                {
                    continue;
                }
                ThStandardSign thStandardSign = node.Tag as ThStandardSign;
                if (thStandardSign.SignPlantCalData == null)
                {
                    continue;
                }
                if (thStandardSign.SignPlantCalData.CalInfo == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(thStandardSign.SignPlantCalData.CalInfo.YjkPath))
                {
                    continue;
                }
                else
                {
                    ci = thStandardSign.SignPlantCalData.CalInfo;
                    break;
                }
            }
            return ci;
        }
        public List<ColumnInf> GetDataCorrectColumnInfs(TreeNode tn)
        {
            List<ColumnInf> columnInfs = new List<ColumnInf>();
            if (tn.Tag != null && tn.Tag.GetType() == typeof(ColumnInf))
            {
                ColumnInf columnInf = tn.Tag as ColumnInf;
                if (columnInf.Error == ErrorMsg.OK)
                {
                    columnInfs.Add(columnInf);
                }
            }
            foreach (TreeNode treeNode in tn.Nodes)
            {
                List<ColumnInf> subColumnInfs = GetDataCorrectColumnInfs(treeNode);
                columnInfs.AddRange(subColumnInfs);
            }
            return columnInfs;
        }
        public bool GetTreeNodeHasVisibleFrame(TreeNode currentNode)
        {
            bool has = false;
            if (owner.tvCheckRes.Nodes.Count == 0)
            {
                return has;
            }
            if (currentNode == null)
            {
                return has;
            }
            TreeNode rootNode = TraverseRoot(currentNode);
            List<ObjectId> totalFrameIds = new List<ObjectId>();
            TraverseNode(rootNode, ref totalFrameIds);
            Document document = ThColumnInfoUtils.GetMdiActiveDocument();
            using (Transaction trans = document.TransactionManager.StartTransaction())
            {
                foreach (ObjectId objId in totalFrameIds)
                {
                    if (objId == ObjectId.Null || objId.IsErased || objId.IsValid == false)
                    {
                        continue;
                    }
                    Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (ent.Visible)
                    {
                        has = true;
                        break;
                    }
                }
                trans.Commit();
            }
            return has;
        }
        public void HideTotalFrameIds(TreeNode currentNode)
        {
            if (owner.tvCheckRes.Nodes.Count == 0)
            {
                return;
            }
            TreeNode rootNode = TraverseRoot(currentNode);
            List<ObjectId> totalFrameIds = new List<ObjectId>();
            TraverseNode(rootNode, ref totalFrameIds);
            CheckResultCadInteractive.ShowHideFrameIds(totalFrameIds, false);
        }
        public void ImportCalculationCmd()
        {
            TreeNode tn = owner.tvCheckRes.SelectedNode as TreeNode;
            List<ObjectId> treeColumnIds = new List<ObjectId>();
            TraverseNode(owner.tvCheckRes.SelectedNode, ref treeColumnIds);

            ThStandardSign thStandardSign = tn.Tag as ThStandardSign;
            Document doc = ThColumnInfoUtils.GetMdiActiveDocument();
            using (DocumentLock docLock = doc.LockDocument())
            {
                try
                {
                    CalculationInfoVM calculationInfoVM = new CalculationInfoVM();
                    if (thStandardSign.SignPlantCalData != null && thStandardSign.SignPlantCalData.CalInfo != null)
                    {
                        calculationInfoVM = new CalculationInfoVM(thStandardSign.SignPlantCalData.CalInfo);
                        calculationInfoVM.CalculateInfo = thStandardSign.SignPlantCalData.CalInfo;
                    }
                    else
                    {
                        calculationInfoVM = new CalculationInfoVM(new CalculationInfo());
                    }
                    if (string.IsNullOrEmpty(calculationInfoVM.CalculateInfo.YjkPath))
                    {
                        CalculationInfo ci = GetCalculationImportPath(tn);
                        calculationInfoVM.CalculateInfo.YjkPath = ci.YjkPath;
                        if (calculationInfoVM.CalculateInfo.YjkUsedPathList.Count == 0)
                        {
                            calculationInfoVM.CalculateInfo.YjkUsedPathList = ci.YjkUsedPathList;
                        }
                    }
                    calculationInfoVM.CalculateInfo.Title = "导入计算书（" + thStandardSign.InnerFrameName + "）";
                    ImportCalculation importCalculation = new ImportCalculation(calculationInfoVM);
                    calculationInfoVM.Owner = importCalculation;
                    importCalculation.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    importCalculation.ShowDialog();
                    if (calculationInfoVM.YnExport)
                    {
                        if (thStandardSign.SignPlantCalData != null)
                        {
                            //删除已经绘制的FrameId和TextId
                            thStandardSign.SignPlantCalData.ClearFrameIds();
                            thStandardSign.SignPlantCalData.EraseJtIdTextIds();
                        }
                        PlantCalDataToDraw plantData = new PlantCalDataToDraw(calculationInfoVM.CalculateInfo, thStandardSign);
                        thStandardSign.SignPlantCalData = plantData;
                        bool res = plantData.Plant();
                        if (res)
                        {
                            try
                            {
                                ThProgressBar.Start("导入计算书...");
                                ThProgressBar.MeterProgress();
                                ThColumnInfoUtils.EraseObjIds(treeColumnIds.ToArray());
                                UpdateCheckResult(tn, thStandardSign, true);
                            }
                            finally
                            {
                                ThProgressBar.Stop();
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex);
                }
            }
        }
        public void Load(string docFullPath = "",bool forLoadTree=true)
        {
            if (string.IsNullOrEmpty(docFullPath))
            {
                docFullPath = ThColumnInfoUtils.GetMdiActiveDocument().Name;
            }
            if (!ThColunmDocManager.IsExisted(docFullPath))
            {
                ThColunmDocManager.AddThStandardSignManager(docFullPath);
            }
            var tm = ThColunmDocManager.GetThStandardSignManager(docFullPath);
            FillColumnDataToTreeView(tm, forLoadTree);
        }
        public void ParameterSetCmd()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            doc.SendStringToExecute("\x03\x03" + "ThCpi" + " ", true, false, false);
        }
        /// <summary>
        /// 显示详细数据
        /// </summary>
        public void ShowDetailData(bool forceShow = false)
        {
            if (owner.tvCheckRes.SelectedNode == null)
            {
                MessageBox.Show("请选择要查看详细数据的层节点");
                return;
            }
            TreeNode innerFrameNode = TraverseRoot(owner.tvCheckRes.SelectedNode);
            if (innerFrameNode == null)
            {
                return;
            }
            if (forceShow)
            {
                DataPalette.ShowPaletteMark = false;
            }
            DataPalette.ShowPaletteMark = !DataPalette.ShowPaletteMark;
            SwitchShowDetailPicture();
            ShowDetailData(owner.tvCheckRes.SelectedNode);
            DataPalette._ps.Visible = DataPalette.ShowPaletteMark;
        }
        public void ShowDetailData(TreeNode tn)
        {
            if (tn == null)
            {
                return;
            }
            ExportDetailData(tn);
            this.lastShowDetailNode = tn;
        }
        public void SelectTreeNode(string innerFrameName, string subcodeText)
        {
            TreeNode subNode = FindTreeCode(innerFrameName, FindNodeMode.SubCode, "", subcodeText);
            if (subNode != null)
            {
                owner.tvCheckRes.SelectedNode = subNode;
            }
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
                    owner.btnShowDetailData.BackgroundImage = Properties.Resources.DetailDataHidePng;
                }
                else
                {
                    owner.btnShowDetailData.BackgroundImage = Properties.Resources.DetailDataShowPng;
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "SwitchShowDetailPicture");
            }
        }
        public void ShowComponentPropPicture()
        {
            try
            {
                owner.btnComponentDefinition.BackgroundImage = Properties.Resources.ComponentPropModifyPng;
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "SwitchShowDetailPicture");
            }
        }
        private void SortThCalculateValidateResult(ThStandardSign thStandardSign, List<ColumnInf> correctSortInfs)
        {
            if (thStandardSign.ThCalculateValidate == null)
            {
                return;
            }
            Dictionary<ColumnRelateInf, List<string>> newResDic = new Dictionary<ColumnRelateInf, List<string>>();
            foreach (ColumnInf columnInf in correctSortInfs)
            {
                var item = thStandardSign.ThCalculateValidate.ColumnValidateResultDic.Where(i => i.Key.ModelColumnInfs.Count == 1 && i.Key.ModelColumnInfs[0].Code == columnInf.Code &&
                  i.Key.ModelColumnInfs[0].Text == columnInf.Text).Select(i => i).First();
                if (item.Key != null && !newResDic.ContainsKey(item.Key))
                {
                    newResDic.Add(item.Key, item.Value);
                }
            }
            thStandardSign.ThCalculateValidate.ColumnValidateResultDic = newResDic;
        }
        public void ShowSelectNodeFrameIds(TreeNode treeNode)
        {
            HideTotalFrameIds(treeNode);
            List<ObjectId> frameIds = new List<ObjectId>();
            TraverseNode(treeNode, ref frameIds);
            CheckResultCadInteractive.ShowHideFrameIds(frameIds, true);
            TreeNode thStandardSignNode = TraverseRoot(treeNode);
            if (thStandardSignNode != null)
            {
                frameIds = frameIds.Where(i => i.IsValid).Select(i => i).ToList();
                ThStandardSign thStandardSign = thStandardSignNode.Tag as ThStandardSign;
                if (frameIds.Count > 0)
                {
                    CheckResultCadInteractive.LocateColumnFrameIds(frameIds);
                }
                else
                {
                    CheckResultCadInteractive.LocateInnerFrame(thStandardSign);
                }
            }
        }
        public TreeNode TraverseRoot(TreeNode treeNode)
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
            return TraverseRoot(treeNode.Parent);
        }
        //遍历当前节点往父节点路径上是否有
        public bool TraverseDataCorrectNode(TreeNode tn)
        {
            if (tn == null)
            {
                return false;
            }
            if (tn.Name == this.dataCorrectNodeName)
            {
                return true;
            }
            return TraverseDataCorrectNode(tn.Parent);
        }

        private void TraverseNode(TreeNode tn, ref List<ObjectId> columnIds)
        {
            if (tn.Tag != null)
            {
                ObjectId frameId = ObjectId.Null;
                if (tn.Tag is ColumnInf columnInf)
                {
                    frameId = columnInf.FrameId;
                }
                else if (tn.Tag is ObjectId objId)
                {
                    frameId = objId;
                }
                if (frameId != ObjectId.Null && !frameId.IsErased && frameId.IsValid)
                {
                    columnIds.Add(frameId);
                }
            }
            if (tn.Nodes.Count == 0)
            {
                return;
            }
            foreach (TreeNode tnItem in tn.Nodes)
            {
                TraverseNode(tnItem, ref columnIds);
            }
        }
        /// <summary>
        /// 更新数据正确节点
        /// </summary>
        /// <param name="dataCorrectNode"></param>
        private void UpdateDataCorrectNode(TreeNode innerFrameNode)
        {
            if (innerFrameNode == null || innerFrameNode.Tag == null)
            {
                return;
            }
            ThStandardSign thStandardSign = innerFrameNode.Tag as ThStandardSign;
            TreeNode dataCorrectNode = null;
            List<ObjectId> invalidObjIds = new List<ObjectId>();
            foreach (TreeNode firstNode in innerFrameNode.Nodes)
            {
                if (firstNode.Name == this.dataCorrectNodeName)
                {
                    dataCorrectNode = firstNode;
                    for (int i = 0; i < firstNode.Nodes.Count; i++)
                    {
                        TreeNode secondNode = firstNode.Nodes[i];
                        for (int j = 0; j < secondNode.Nodes.Count; j++)
                        {
                            TreeNode thirdNode = secondNode.Nodes[j];
                            ColumnInf columnInf = thirdNode.Tag as ColumnInf;
                            bool res = thStandardSign.SignPlantCalData.
                                CheckCorrectColumnInDwgHasCalNotColumns(columnInf);
                            if (res)
                            {
                                secondNode.Nodes.Remove(thirdNode);
                                invalidObjIds.Add(columnInf.FrameId);
                                j = j - 1;
                            }
                        }
                    }
                    break;
                }
            }
            if (invalidObjIds.Count > 0)
            {
                ThColumnInfoUtils.EraseObjIds(invalidObjIds.ToArray());
            }
            int totalNum = 0;
            for (int i = 0; i < dataCorrectNode.Nodes.Count; i++)
            {
                TreeNode columnNode = dataCorrectNode.Nodes[i];
                if (columnNode.Nodes.Count == 0)
                {
                    dataCorrectNode.Nodes.Remove(columnNode);
                    i = i - 1;
                }
                else
                {
                    totalNum += columnNode.Nodes.Count;
                    string columnCode = columnNode.Text.Substring(0, columnNode.Text.IndexOf("("));
                    string columnSpec = columnNode.Text.Substring(columnNode.Text.IndexOf(")") + 1);
                    columnNode.Text = columnCode + "(" + columnNode.Nodes.Count + ")" + columnSpec;
                }
            }
            dataCorrectNode.Text = "匹配成功：数据正确(" + totalNum + ")";
        }
        private void UpdateCheckResult(TreeNode tn, ThStandardSign thStandardSign, bool showImportCalInf = false)
        {
            try
            {
                //如果导入过计算书，则埋入数据
                if (thStandardSign.SignPlantCalData != null)
                {
                    thStandardSign.SignPlantCalData.Embed(showImportCalInf); //埋入
                }
                else
                {
                    ThStandardSignManager.UpdateThStandardSign(thStandardSign);
                }
                //hasCheckErrorNode-> 控制是否往树节点中填充"柱平法缺失"和"柱信息不完整两个节点"
                bool ynExportErrorNode = true;
                if (showImportCalInf)
                {
                    ynExportErrorNode = false;
                    //如果执行导入计算书操作，且已执行过计算书校核
                    foreach (TreeNode item in tn.Nodes)
                    {
                        if (item.Name == this.codeLostNodeName || item.Name == this.uncompleteNodeNme)
                        {
                            ynExportErrorNode = true;
                            break;
                        }
                    }
                }
                if (thStandardSign.SignExtractColumnInfo != null)
                {
                    if (showImportCalInf)
                    {
                        if (ynExportErrorNode)
                        {
                            thStandardSign.SignExtractColumnInfo.PrintErrorColumnFrame();
                        }
                    }
                    else
                    {
                        //没有导入过计算书
                        thStandardSign.SignExtractColumnInfo.PrintColumnFrame();
                        //如果是校核，且没导入过计算书，则要设置默认值
                        PlantCalDataToDraw plantCalDataToDraw = new PlantCalDataToDraw(thStandardSign);
                        plantCalDataToDraw.DrawColumnOriginFrame();
                    }
                }
                //将识别的结果更新到面板
                tn.Nodes.Clear();
                FillDrawCheckInfToTreeView(thStandardSign.SignExtractColumnInfo, tn, ynExportErrorNode);
                FillPlantCalResultToTree(tn);
                if (!showImportCalInf) //如是导入计算书，则无需校验
                {
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
                }
                tn.Expand();
                foreach (TreeNode tnItem in tn.Nodes)
                {
                    tnItem.Expand();
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "UpdateCheckResult");
            }
        }
    }
}
