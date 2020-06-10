using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ThColumnInfo.View;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThColumnInfo.ViewModel
{
    public class CalculationInfoVM
    {
        public CalculationInfo CalculateInfo { get; set; }
        public DelegateCommand SelectToRightCommand { get; set; }
        public DelegateCommand SelectToLeftCommand { get; set; }
        public DelegateCommand BrowseDirectoryCommand { get; set; }

        public DelegateCommand ImportCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }
        public ImportCalculation Owner { get; set; }
        public bool YnExport { get; set; } = false;
        public CalculationInfoVM(CalculationInfo calculationInfo)
        {
            this.CalculateInfo = calculationInfo;
            this.SelectToRightCommand = new DelegateCommand();
            this.SelectToLeftCommand = new DelegateCommand();
            this.BrowseDirectoryCommand = new DelegateCommand();
            this.ImportCommand = new DelegateCommand();
            this.ExitCommand = new DelegateCommand();


            this.SelectToRightCommand.ExecuteAction = new Action<object>(this.SelectToRightCommandExecute);
            this.SelectToLeftCommand.ExecuteAction = new Action<object>(this.SelectToLeftCommandExecute);
            this.BrowseDirectoryCommand.ExecuteAction = new Action<object>(this.BrowseDirectoryCommandExecute);
            this.ImportCommand.ExecuteAction = new Action<object>(this.ImportCommandExecute);
            this.ExitCommand.ExecuteAction= new Action<object>(this.ExitCommandExecute);
        }
        public CalculationInfoVM()
        {
        }
        public bool ValidateFileNameIsYjkFile(string filePath)
        {
            bool res = true;
            if(string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            FileInfo fi = new FileInfo(filePath);
            if(!fi.Exists)
            {
                return false;
            }
            if(fi.Extension.ToUpper()!=".YJK")
            {
                return false;
            }
            return res;
        }
        private void ImportCommandExecute(object parameter)
        {
            this.YnExport = true;
            if(this.CalculateInfo.SelectLayers.Count==0)
            {
                MessageBox.Show("未能选择要导出计算书的楼层，请返回重新选择!");
                return;
            }
            Owner.Close();
        }
        private void ExitCommandExecute(object parameter)
        {
            this.YnExport = false;
            if (Owner == null)
            {
                return;
            }
            Owner.Close();
        }
        private void SelectToRightCommandExecute(object parameter)
        {
            if(Owner == null)
            {
                return;
            }            
            List<string> leftSelectItems = new List<string>();
            for (int i = 0; i < Owner.lbModelLayers.SelectedItems.Count; i++)
            {
                var item = Owner.lbModelLayers.SelectedItems[i];
                leftSelectItems.Add(item.ToString());
            }
            string errorMsg=JudgeRightItemCanBeSelected(leftSelectItems);
            if(!string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg,"选择提示");
                return;
            }
            for (int i=0;i< leftSelectItems.Count;i++)
            {
                int index =this.CalculateInfo.ModelLayers.IndexOf(leftSelectItems[i].ToString());
                if(index>=0)
                {
                    if(this.CalculateInfo.SelectLayers.IndexOf(leftSelectItems[i]) <0)
                    {
                        this.CalculateInfo.SelectLayers.Add(leftSelectItems[i]);
                    }
                    this.CalculateInfo.ModelLayers.RemoveAt(index);
                    i = i - 1;
                }
            }
            if(this.CalculateInfo.SelectLayers.Count>0)
            {
                List<string> newRightItems = this.CalculateInfo.SelectLayers.OrderBy(i => ThColumnInfoUtils.GetDatas(i)[0]).Select(i => i).ToList();
                ObservableCollection<string> newCols = new ObservableCollection<string>();
                newRightItems.ForEach(i => newCols.Add(i));
                this.CalculateInfo.SelectLayers = newCols;
            }
        }
        private string JudgeRightItemCanBeSelected(List<string> selectItems)
        {
            string errorMsg = "";
            if(!this.CalculateInfo.SelectByStandard) 
            {
                //按自然层选择
                List<FloorInfo> floorInfos = LoadYjkDbInfo();
                List<int> stdFlrIds = new List<int>();
                foreach (string floorName in selectItems)
                {
                    int floorIndex = ThColumnInfoUtils.GetDatas(floorName)[0];
                    int stdFlrId = floorInfos.Where(i => i.Name == floorIndex + "F").Select(i => i.StdFlrID).FirstOrDefault();
                    stdFlrIds.Add(stdFlrId);
                }                
                if (selectItems.Count>1)
                {
                    for(int i=1;i< stdFlrIds.Count;i++)
                    {
                        if(stdFlrIds[i]!= stdFlrIds[0])
                        {
                            return "按自然层选择，必须所在的标准层要相同！";
                        }
                    }
                }
                if(string.IsNullOrEmpty(errorMsg))
                {
                    if(this.CalculateInfo.SelectLayers.Count==0)
                    {
                        return errorMsg;
                    }
                    string firstIItem=this.CalculateInfo.SelectLayers[0];
                    int floorIndex = ThColumnInfoUtils.GetDatas(firstIItem)[0];
                    for(int i=0;i< stdFlrIds.Count;i++)
                    {
                        if(floorIndex!= stdFlrIds[i])
                        {
                            return "按自然层选择，所选的标准层要与右边列表所选的标准层相同！";
                        }
                    }
                } 
            }
            else
            {
                if(selectItems.Count>1)
                {
                    return "按标准层选择，只能选择一个标准层来导出！";
                }
                if (this.CalculateInfo.SelectLayers.Count == 0)
                {
                    return errorMsg;
                }
                else
                {
                    return "按标准层选择，只能选择一个标准层";
                }
            }
            return errorMsg;
        }
        private void SelectToLeftCommandExecute(object parameter)
        {
            if (Owner == null)
            {
                return;
            }
            List<string> rightSelectItems = new List<string>();
            for (int i = 0; i < Owner.lbSelectLayers.SelectedItems.Count; i++)
            {
                var item = Owner.lbSelectLayers.SelectedItems[i];
                rightSelectItems.Add(item.ToString());
            }
            for (int i = 0; i < rightSelectItems.Count; i++)
            {
                int index = this.CalculateInfo.SelectLayers.IndexOf(rightSelectItems[i].ToString());
                if (index >= 0)
                {
                    if (this.CalculateInfo.ModelLayers.IndexOf(rightSelectItems[i]) < 0)
                    {
                        this.CalculateInfo.ModelLayers.Add(rightSelectItems[i]);
                    }
                    this.CalculateInfo.SelectLayers.RemoveAt(index);
                    i = i - 1;
                }
            }
            if (this.CalculateInfo.ModelLayers.Count > 0)
            {
                List<string> newLeftItems = this.CalculateInfo.ModelLayers.OrderBy(i => ThColumnInfoUtils.GetDatas(i)[0]).Select(i => i).ToList();
                ObservableCollection<string> newCols = new ObservableCollection<string>();
                newLeftItems.ForEach(i => newCols.Add(i));
                this.CalculateInfo.ModelLayers = newCols;
            }
        }
        private string GetInitDirectory()
        {
            string initDir = "";
            try
            {
                if (!string.IsNullOrEmpty(Owner.cbYjkFilePath.Text))
                {
                    FileInfo fi = new FileInfo(Owner.cbYjkFilePath.Text);
                    if (fi.Exists)
                    {
                        initDir = fi.Directory.FullName;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.CalculateInfo.YjkPath))
                    {
                        FileInfo fi = new FileInfo(this.CalculateInfo.YjkPath);
                        if (fi.Exists)
                        {
                            initDir = fi.Directory.FullName;
                        }
                    }
                }
                if (string.IsNullOrEmpty(initDir))
                {
                    if (this.CalculateInfo.YjkUsedPathList != null)
                    {
                        for (int i = this.CalculateInfo.YjkUsedPathList.Count - 1; i >= 0; i--)
                        {
                            if (string.IsNullOrEmpty(this.CalculateInfo.YjkUsedPathList[i]))
                            {
                                continue;
                            }
                            FileInfo fi = new FileInfo(this.CalculateInfo.YjkUsedPathList[i]);
                            if (fi.Exists)
                            {
                                initDir = fi.Directory.FullName;
                                break;
                            }
                        }
                    }
                }
                //从当前dwg图纸获取路径
                if(string.IsNullOrEmpty(initDir))
                {
                    FileInfo fi= new FileInfo(acadApp.DocumentManager.MdiActiveDocument.Name);
                    if(fi.Exists)
                    {
                        initDir = fi.Directory.FullName;
                        if(fi.Directory.Parent!=null && fi.Directory.Parent.Parent!=null)
                        {
                            string calculationModel = fi.Directory.Parent.Parent.FullName + "\\计算模型";
                            if(Directory.Exists(calculationModel))
                            {
                                initDir = calculationModel;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(initDir))
                {
                    List<string> dirNames = new List<string> { "C:\\", "D:\\", "E:\\", "F:\\" };
                    for (int i = 0; i < dirNames.Count; i++)
                    {
                        DirectoryInfo di = new DirectoryInfo(dirNames[i]);
                        if (di.Exists)
                        {
                            initDir = dirNames[i];
                            break;
                        }
                    }
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetInitDirectory");
            }
            return initDir;
        }
        private void BrowseDirectoryCommandExecute(object parameter)
        {
            OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.InitialDirectory = GetInitDirectory();
            openFileDialog1.Filter = "yjk files (*.yjk)|*.yjk";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog1.FileName);
                if(fileInfo.Extension.ToUpper()==".YJK")
                {
                    if(this.CalculateInfo.YjkUsedPathList.IndexOf(openFileDialog1.FileName)<=0)
                    {
                        this.CalculateInfo.YjkUsedPathList.Add(openFileDialog1.FileName);
                        this.CalculateInfo.YjkPath = openFileDialog1.FileName;
                        UpdateSelectFloorList();
                    }
                }
            }
        }
        private void WriteToCalculationHistory(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }
                FileInfo fi = new FileInfo(fileName);
                if (!fi.Exists)
                {
                    return;
                }
                string path = CalculateInfo.GetColumnCalculationSetIniPath();
                StreamWriter s = new StreamWriter(path, true);  //创建流
                s.WriteLine(fileName); //流写入文件
                s.Flush();
                s.Close();
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "WriteToCalculationHistory");
            }
        }
        public List<string> GetSelectFloorList()
        {
            List<string> floorNames = new List<string>();
            List<FloorInfo> floorInfs = LoadYjkDbInfo();
            if (!this.CalculateInfo.SelectByStandard)
            {
                floorNames = floorInfs.Select(i => i.Name + "(" + i.StdFlrName + ")").ToList();
            }
            else
            {
                List<string> stdFlrNames = floorInfs.Select(i => i.StdFlrName).ToList();
                stdFlrNames = stdFlrNames.Distinct().ToList();
                foreach(string strFlrName in stdFlrNames)
                {
                   List<string> strFlrIncludeNaturalFlrs = floorInfs.Where(i => i.StdFlrName == strFlrName).Select(i => i.Name).ToList();
                   string stdFlrIncludeName=  GenerateStandFlrName(strFlrIncludeNaturalFlrs);
                    if(!string.IsNullOrEmpty(stdFlrIncludeName))
                    {
                        floorNames.Add(strFlrName + " (" + stdFlrIncludeName + ")");
                    }
                }
            }
            return floorNames;
        }
        private string GenerateStandFlrName(List<string> stdFlrInclueNaturalFlrs)
        {
            string name = "";
            if(stdFlrInclueNaturalFlrs==null || stdFlrInclueNaturalFlrs.Count==0)
            {
                return name;
            }
            if(stdFlrInclueNaturalFlrs.Count==1)
            {
                name = stdFlrInclueNaturalFlrs[0];
            }
            else
            {
                for (int i = 0; i < stdFlrInclueNaturalFlrs.Count - 1; i++)
                {
                    List<int> datas1 = ThColumnInfoUtils.GetDatas(stdFlrInclueNaturalFlrs[i]);
                    int firstFloorIndex = datas1[0];
                    int startIndex = i;
                    int endIndex = i;
                    for (int j = i + 1; j < stdFlrInclueNaturalFlrs.Count; j++)
                    {
                        List<int> datas2 = ThColumnInfoUtils.GetDatas(stdFlrInclueNaturalFlrs[j]);
                        int secondFloorIndex = datas2[0];
                        if ((firstFloorIndex < 0 && secondFloorIndex < 0) || (firstFloorIndex > 0 && secondFloorIndex > 0))
                        {
                            if (secondFloorIndex - firstFloorIndex == 1)
                            {
                                endIndex = j;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (firstFloorIndex < 0 && secondFloorIndex > 0)
                            {
                                endIndex = j;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    string subName = "";
                    if (endIndex - startIndex > 0)
                    {
                        subName = stdFlrInclueNaturalFlrs[startIndex] + "~" + stdFlrInclueNaturalFlrs[endIndex];
                    }
                    else
                    {
                        subName = stdFlrInclueNaturalFlrs[startIndex];
                    }
                    if (string.IsNullOrEmpty(name))
                    {
                        name = subName;
                    }
                    else
                    {
                        name += "," + subName;
                    }
                    i = endIndex;
                }
            }
            return name;
        }
        public List<FloorInfo> LoadYjkDbInfo()
        {
            List<FloorInfo> floorInfs = new List<FloorInfo>();
            string dtModelPath = this.CalculateInfo.GetDtlmodelFullPath();
            if (string.IsNullOrEmpty(dtModelPath))
            {
                return floorInfs;
            }
            FileInfo fi = new FileInfo(dtModelPath);
            if(!fi.Exists)
            {
                return floorInfs;
            }
            //获取自然层及对应的标准层
            ExtractYjkColumnInfo extractYjkColumnInfo = new ExtractYjkColumnInfo(dtModelPath);
            floorInfs= extractYjkColumnInfo.GetNaturalFloorInfs();
            floorInfs=floorInfs.OrderBy(i => ThColumnInfoUtils.GetDatas(i.Name)[0]).ToList();
            List<int> stdFlrIds = new List<int>();
            foreach(var item in floorInfs)
            {
                if(stdFlrIds.IndexOf(item.StdFlrID)<0)
                {
                    stdFlrIds.Add(item.StdFlrID);
                }
            }
            Dictionary<int, string> stdFlrNameDic = new Dictionary<int, string>();
            for(int i=1;i<= stdFlrIds.Count;i++)
            {
                stdFlrNameDic.Add(stdFlrIds[i - 1], "标准层" + i);
            }
            foreach(var item in floorInfs)
            {
                item.StdFlrName = stdFlrNameDic[item.StdFlrID];
            }
            return floorInfs;
        }
        /// <summary>
        /// 获取项目配置中选择的自然层数 
        /// </summary>
        /// <returns></returns>
        public List<string> GetSelectFloors()
        {
            List<string> naturalFloors = new List<string>();
            List<string> selFloors = this.CalculateInfo.SelectLayers.ToList();     
            if (selFloors.Count > 0)
            {
                int index = selFloors[0].IndexOf("标准层");
                if (index == 0)
                {
                    string stdFlrName = selFloors[0];
                    int leftBracketIndex = stdFlrName.IndexOf("(");
                    int rightBracketIndex = stdFlrName.IndexOf(")");
                    string naturalStdFlrName = stdFlrName.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
                    string[] floors = naturalStdFlrName.Split(',');
                    foreach (string floor in floors)
                    {
                        string[] rangeFloors = floor.Split('~');
                        if (rangeFloors.Length == 1)
                        {
                            naturalFloors.Add(rangeFloors[0]);
                        }
                        else
                        {
                            int startFloor = ThColumnInfoUtils.GetDatas(rangeFloors[0])[0];
                            int endFloor = ThColumnInfoUtils.GetDatas(rangeFloors[1])[0];
                            for (int i = startFloor; i <= endFloor; i++)
                            {
                                naturalFloors.Add(i + "F");
                            }
                        }

                    }
                }
                else
                {
                    for (int i = 0; i < selFloors.Count; i++)
                    {
                        int floorIndex = ThColumnInfoUtils.GetDatas(selFloors[i])[0];
                        naturalFloors.Add(floorIndex + "F");
                    }
                }
            }
            return naturalFloors;
        }
        public void UpdateSelectFloorList()
        {
            List<string> floorNames = GetSelectFloorList();
            ObservableCollection<string> modelLayers = new ObservableCollection<string>();
            ObservableCollection<string> selectLayers = new ObservableCollection<string>();
            floorNames.ForEach(i => modelLayers.Add(i));
            this.CalculateInfo.ModelLayers = modelLayers;
            var filterRes=this.CalculateInfo.SelectLayers.Where(i => modelLayers.IndexOf(i) >= 0).Select(i=>i).ToList();
            if(filterRes!=null && filterRes.Count>0)
            {
                filterRes.ForEach(i => selectLayers.Add(i));
            }
            this.CalculateInfo.SelectLayers = selectLayers;
            Owner.lbModelLayers.ItemsSource = modelLayers;
        }
        public bool Validate()
        {
            return true;
        }
        public void UpdateSelectMode(bool selectByStandard)
        {
            this.CalculateInfo.SelectByStandard = selectByStandard;
        }
    }
}
