using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ThColumnInfo.ViewModel
{
    public class CalculationInfoVM
    {
        public CalculationInfo CalculateInfo { get; set; }
        public DelegateCommand SelectToRightCommand { get; set; }
        public DelegateCommand SelectToLeftCommand { get; set; }
        public DelegateCommand BrowseDirectoryCommand { get; set; }


        public CalculationInfoVM(CalculationInfo calculationInfo)
        {
            this.CalculateInfo = calculationInfo;
            Init();
            this.SelectToRightCommand = new DelegateCommand();
            this.SelectToLeftCommand = new DelegateCommand();
            this.BrowseDirectoryCommand = new DelegateCommand();

            this.SelectToRightCommand.ExecuteAction = new Action<object>(this.SelectToRightCommandExecute);
            this.SelectToLeftCommand.ExecuteAction = new Action<object>(this.SelectToLeftCommandExecute);
            this.BrowseDirectoryCommand.ExecuteAction = new Action<object>(this.BrowseDirectoryCommandExecute);
        }
        private bool ValidateFileNameIsYjkFile(string filePath)
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
        private void Init()
        {
            if(Properties.Settings.Default.YjkUsedPathList!=null &&
                Properties.Settings.Default.YjkUsedPathList.Count>0)
            {
                ObservableCollection<string> pathList = new ObservableCollection<string>();
                foreach(var item in Properties.Settings.Default.YjkUsedPathList)
                {
                    pathList.Add(item);
                }
                this.CalculateInfo.YjkUsedPathList = pathList;
            }
            if(ValidateFileNameIsYjkFile(Properties.Settings.Default.YjkUsePath))
            {
                this.CalculateInfo.YjkPath = Properties.Settings.Default.YjkUsePath;
            }
            this.CalculateInfo.Angle = Properties.Settings.Default.Angle;
            this.CalculateInfo.ModelAppoint = Properties.Settings.Default.ModelPoint;
        }
        private void SelectToRightCommandExecute(object parameter)
        {
            if(ThColumnInfoCommands.columnCalulationInstance==null)
            {
                return;
            }            
            List<string> leftSelectItems = new List<string>();
            for (int i = 0; i < ThColumnInfoCommands.columnCalulationInstance.lbModelLayers.SelectedItems.Count; i++)
            {
                var item = ThColumnInfoCommands.columnCalulationInstance.lbModelLayers.SelectedItems[i];
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
            if(this.CalculateInfo.SelectByFloor) 
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
            if (ThColumnInfoCommands.columnCalulationInstance == null)
            {
                return;
            }
            List<string> rightSelectItems = new List<string>();
            for (int i = 0; i < ThColumnInfoCommands.columnCalulationInstance.lbSelectLayers.SelectedItems.Count; i++)
            {
                var item = ThColumnInfoCommands.columnCalulationInstance.lbSelectLayers.SelectedItems[i];
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
            if(!string.IsNullOrEmpty(ThColumnInfoCommands.columnCalulationInstance.cbYjkFilePath.Text))
            {
                FileInfo fi = new FileInfo(ThColumnInfoCommands.columnCalulationInstance.cbYjkFilePath.Text);
                if(fi.Exists)
                {
                    initDir = fi.Directory.FullName;
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(Properties.Settings.Default.YjkUsePath))
                {
                    FileInfo fi = new FileInfo(Properties.Settings.Default.YjkUsePath);
                    if(fi.Exists)
                    {
                        initDir = fi.Directory.FullName;
                    }
                }                
            }
            if(string.IsNullOrEmpty(initDir))
            {
                if(Properties.Settings.Default.YjkUsedPathList!=null)
                {
                    for(int i= Properties.Settings.Default.YjkUsedPathList.Count-1;i>=0;i--)
                    {
                        if(string.IsNullOrEmpty(Properties.Settings.Default.YjkUsedPathList[i]))
                        {
                            continue;
                        }
                        FileInfo fi = new FileInfo(Properties.Settings.Default.YjkUsedPathList[i]);
                        if(fi.Exists)
                        {
                            initDir = fi.Directory.FullName;
                            break;
                        }
                    }
                }
            }
            if(string.IsNullOrEmpty(initDir))
            {
                List<string> dirNames = new List<string> { "C:\\","D:\\","E:\\","F:\\" };
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
            if (this.CalculateInfo.SelectByFloor)
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
        public void UpdateSelectFloorList()
        {
            List<string> floorNames = GetSelectFloorList();
            ObservableCollection<string> modelLayers = new ObservableCollection<string>();
            ObservableCollection<string> selectLayers = new ObservableCollection<string>();
            floorNames.ForEach(i => modelLayers.Add(i));
            this.CalculateInfo.ModelLayers = modelLayers;
            this.CalculateInfo.SelectLayers = selectLayers;
        }
    }
}
