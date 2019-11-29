using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class CalculationInfo : CNotifyPropertyChange
    {
        private string yjkPath = "";
        private ObservableCollection<string> yjkUsedPathList = new ObservableCollection<string>();
        private bool selectByFloor = true;
        private bool selectByStandard = false;
        private double angle = 0;
        private bool modelAppoint = false;
        private string quickAppoint = "";

        private ObservableCollection<string> modelLayers  = new ObservableCollection<string>();
        private ObservableCollection<string> selectLayers = new ObservableCollection<string>();
        /// <summary>
        /// 计算库使用过的路径
        /// </summary>
        public ObservableCollection<string> YjkUsedPathList
        {
            get
            {
                return yjkUsedPathList;
            }
            set
            {
                yjkUsedPathList = value;
                NotifyPropertyChange("YjkUsedPathList");
            }
        }

        /// <summary>
        /// 计算书模型库路径
        /// </summary>
        public string YjkPath
        {
            get
            {
                return yjkPath;
            }
            set
            {
                yjkPath = value;
                NotifyPropertyChange("YjkPath");
            }
        }
        /// <summary>
        /// 按自然层选择
        /// </summary>
        public bool SelectByFloor
        {
            get
            {
                return selectByFloor;
            }
            set
            {
                selectByFloor = value;
                NotifyPropertyChange("SelectByFloor");
            }
        }
        /// <summary>
        /// 按标准层选择
        /// </summary>
        public bool SelectByStandard
        {
            get
            {
                return selectByStandard;
            }
            set
            {
                selectByStandard = value;
                NotifyPropertyChange("SelectByStandard");
            }
        }
        /// <summary>
        /// 模型角度
        /// </summary>
        public double Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                NotifyPropertyChange("Angle");
            }
        }
        /// <summary>
        /// 图中指定
        /// </summary>
        public bool ModelAppoint
        {
            get
            {
                return modelAppoint;
            }
            set
            {
                modelAppoint = value;
                NotifyPropertyChange("ModelAppoint");
            }
        }
        /// <summary>
        /// 快速指定
        /// </summary>
        public string QuickAppoint
        {
            get
            {
                return quickAppoint;
            }
            set
            {
                quickAppoint = value;
                NotifyPropertyChange("QuickAppoint");
            }
        }
        /// <summary>
        /// 模型层
        /// </summary>
        public ObservableCollection<string> ModelLayers
        {
            get
            {
                return modelLayers;
            }
            set
            {
                modelLayers = value;
                NotifyPropertyChange("ModelLayers");
            }
        }
        /// <summary>
        /// 标准层
        /// </summary>
        public ObservableCollection<string> SelectLayers
        {
            get
            {
                return selectLayers;
            }
            set
            {
                selectLayers = value;
                NotifyPropertyChange("SelectLayers");
            }
        }
        public string GetDtlCalcFullPath()
        {
            string dtlCalcPath = "";
            if(string.IsNullOrEmpty(this.yjkPath))
            {
                return dtlCalcPath;
            }
            FileInfo fi = new FileInfo(this.yjkPath);
            if(!fi.Exists)
            {
                return dtlCalcPath;
            }
            dtlCalcPath = fi.Directory+"\\施工图\\"+ "dtlCalc.ydb";
            if(!new FileInfo(dtlCalcPath).Exists)
            {
                dtlCalcPath = "";
            }
            return dtlCalcPath;
        }
        public string GetDtlmodelFullPath()
        {
            string dtlmodelPath = "";
            if (string.IsNullOrEmpty(this.yjkPath))
            {
                return dtlmodelPath;
            }
            FileInfo fi = new FileInfo(this.yjkPath);
            if (!fi.Exists)
            {
                return dtlmodelPath;
            }
            dtlmodelPath = fi.Directory + "\\施工图\\" + "dtlmodel.ydb";
            if (!new FileInfo(dtlmodelPath).Exists)
            {
                dtlmodelPath = "";
            }
            return dtlmodelPath;
        }
        public string GetColumnCalculationSetIniPath()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DirectoryInfo di = new DirectoryInfo(localAppDataPath);
            DirectoryInfo partentDi = di.Parent;
            string calucalationImportHistoryFilePath = partentDi.FullName + "\\Local\\Temp\\" + "CalucalationImportHistory.ini";
            return calucalationImportHistoryFilePath;
        }
        private string CleanString(string newStr)
        {
            string tempStr = newStr.Replace((char)13, (char)0);
            return tempStr.Replace((char)10, (char)0);
        }
        //暂时不用
        private void GetYjkUseFiles()
        {
            try
            {
                string iniPath = GetColumnCalculationSetIniPath();
                StreamReader sr = new StreamReader(iniPath); //创建文件流      
                while (sr.Peek() >= 0) //读取文件流          
                {
                    //将读取的信息创建菜单项  
                    string rowLine = sr.ReadLine();
                    if (!string.IsNullOrEmpty(rowLine))
                    {
                        FileInfo tempFi = new FileInfo(rowLine);
                        if (tempFi.Exists && this.yjkUsedPathList.IndexOf(rowLine) < 0)
                        {
                            rowLine=CleanString(rowLine);
                            this.yjkUsedPathList.Add(rowLine);
                        }
                    }
                }
                sr.Close();  //关闭文件流
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
        }
    }
}
