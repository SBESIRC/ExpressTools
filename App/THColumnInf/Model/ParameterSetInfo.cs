using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThColumnInfo.Validate;

namespace ThColumnInfo
{
    public class ParameterSetInfo: CNotifyPropertyChange
    {
        private int floorCount;
        private string antiSeismicGrade = "一级"; //三级
        private double protectLayerThickness=25.0; 
        private string concreteStrength = "C30";
        private string structureType = "框架结构";
        private ObservableCollection<string> structureTypeList = new ObservableCollection<string>();
        private ObservableCollection<string> columnTableLayerList = new ObservableCollection<string>();
        private ObservableCollection<string> concreteStrengthList = new ObservableCollection<string>();
        private ObservableCollection<string> antiseismicGradeList = new ObservableCollection<string>();
        private ObservableCollection<SteeBarLevel> steeBarLevels =new ObservableCollection<SteeBarLevel>();
        private bool isFourClassHigherArchitecture = false;
        private string bottomFloorLayer = "";

        private string columnLayer = "*S_COLU";
        private string columnTableLayer = "S_TABL";

        public ParameterSetInfo()
        {
            //初始化钢筋强度等级
            steeBarLevels.Add(new SteeBarLevel() { SteelBarGrade= "HRB500", Value= 0.5,MatchStr="%%133"});
            steeBarLevels.Add(new SteeBarLevel() { SteelBarGrade = "HRB400", Value = 0.55, MatchStr = "%%132"});
            steeBarLevels.Add(new SteeBarLevel() { SteelBarGrade = "HRB335", Value = 0.6, MatchStr = "%%131" });
            steeBarLevels.Add(new SteeBarLevel() { SteelBarGrade = "HPB300", Value = 0.5, MatchStr = "%%130" });

            //初始化抗震等级列表
            antiseismicGradeList.Add("四级"); //级别最小
            antiseismicGradeList.Add("三级");
            antiseismicGradeList.Add("二级");
            antiseismicGradeList.Add("一级");
            antiseismicGradeList.Add("特一级");

            //初始化混凝土强度列表
            concreteStrengthList.Add("C20");
            concreteStrengthList.Add("C25");
            concreteStrengthList.Add("C30");
            concreteStrengthList.Add("C35");
            concreteStrengthList.Add("C40");
            concreteStrengthList.Add("C45");
            concreteStrengthList.Add("C50");
            concreteStrengthList.Add("C55");
            concreteStrengthList.Add("C60");
            concreteStrengthList.Add("C65");
            concreteStrengthList.Add("C70");
            concreteStrengthList.Add("C75");
            concreteStrengthList.Add("C80");

            Dictionary<string, double> structureTypeDic = ThValidate.GetStructureTypeDic();
            List<string> structureTypeNames = structureTypeDic.Select(i => i.Key).ToList();
            structureTypeNames.ForEach(i=>this.structureTypeList.Add(i));
        }       
        public ObservableCollection<SteeBarLevel> SteeBarLevels
        {
            get
            {
                return steeBarLevels;
            }
            set
            {
                steeBarLevels = value;
                NotifyPropertyChange("SteeBarLevels");
            }
        }     
        public ObservableCollection<string> ColumnTableLayerList
        {
            get
            {
                return columnTableLayerList;
            }
            set
            {
                columnTableLayerList = value;
                NotifyPropertyChange("ColumnTableLayerList");
            }
        }
        public ObservableCollection<string> ConcreteStrengthList
        {
            get
            {
                return concreteStrengthList;
            }
            set
            {
                concreteStrengthList = value;
                NotifyPropertyChange("ConcreteStrengthList");
            }
        }
        public ObservableCollection<string> StructureTypeList
        {
            get
            {
                return structureTypeList;
            }
            set
            {
                structureTypeList = value;
                NotifyPropertyChange("StructureTypeList");
            }
        }
        public ObservableCollection<string> AntiseismicGradeList
        {
            get
            {
                return antiseismicGradeList;
            }
            set
            {
                antiseismicGradeList = value;
                NotifyPropertyChange("AntiseismicGradeList");
            }
        }
        /// <summary>
        /// 混凝土强度
        /// </summary>
        public string ConcreteStrength
        {
            get
            {
                return concreteStrength;
            }
            set
            {
                concreteStrength = value;
                NotifyPropertyChange("ConcreteStrength");
            }
        }
        /// <summary>
        /// 楼层自然层数
        /// </summary>
        public int FloorCount
        {
            get
            {
                return floorCount;
            }
            set
            {
                floorCount = value;
                NotifyPropertyChange("FloorCount");
            }
        }
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade
        {
            get
            {
                return antiSeismicGrade;
            }
            set
            {
                antiSeismicGrade = value;
                NotifyPropertyChange("AntiSeismicGrade");
            }
        }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness
        {
            get
            {
                return protectLayerThickness;
            }
            set
            {
                protectLayerThickness = value;
                NotifyPropertyChange("ProtectLayerThickness");
            }
        }
        /// <summary>
        /// 结构类型
        /// </summary>
        public string StructureType
        {
            get
            {
                return structureType;
            }
            set
            {                
                structureType = value;
                NotifyPropertyChange("StructureType");
            }
        }
        /// <summary>
        /// 柱图层
        /// </summary>
        public string ColumnLayer
        {
            get
            {
                return columnLayer;
            }
            set
            {
                columnLayer = value;
                NotifyPropertyChange("ColumnLayer");
            }
        }
        /// <summary>
        /// 柱表图层
        /// </summary>
        public string ColumnTableLayer
        {
            get
            {
                return columnTableLayer;
            }
            set
            {
                columnTableLayer = value;
                NotifyPropertyChange("ColumnTableLayer");
            }
        }
        /// <summary>
        /// 是否为IV类场地较高建筑
        /// </summary>
        public bool IsFourClassHigherArchitecture
        {
            get
            {
                return isFourClassHigherArchitecture;
            }
            set
            {
                isFourClassHigherArchitecture = value;
                NotifyPropertyChange("IsFourClassHigherArchitecture");
            }
        }
        /// <summary>
        /// 底层图框名
        /// </summary>
        public string BottomFloorLayer
        {
            get
            {
                return bottomFloorLayer;
            }
            set
            {
                bottomFloorLayer = value;
                NotifyPropertyChange("BottomFloorLayer");
            }
        }
        public double GetConcreteStrengthValue()
        {
            double value = 0.0;
            if(!string.IsNullOrEmpty(this.concreteStrength))
            {
               List<double> values= ThColumnInfoUtils.GetDoubleValues(this.concreteStrength);
                if(values.Count>0)
                {
                    if(values[0]<=60)
                    {
                        value = 0.0;
                    }
                    else
                    {
                        value = 0.1;
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// 获取纵筋级别的等级
        /// </summary>
        /// <param name="longitudinalReinforcSpec"></param>
        /// <returns></returns>
        public double GetLongitudinalReinforcementGrade(string longitudinalReinforcSpec)
        {
            double value = 0.0;
            if(string.IsNullOrEmpty(longitudinalReinforcSpec))
            {
                return value;
            }
            byte[] buffers = Encoding.UTF32.GetBytes(longitudinalReinforcSpec);
            var res1 = buffers.Where(i => i == 132);
            if((res1!=null && res1.Count()>0) || longitudinalReinforcSpec.IndexOf("%%132") >= 0)
            {
                var steelBarLevel= this.steeBarLevels.Where(i => i.MatchStr.Contains("132")).FirstOrDefault();
                if(steelBarLevel!=null)
                {
                    value = steelBarLevel.Value;
                }
                return value;
            }            
            var res2 = buffers.Where(i => i == 133);
            if ((res2 != null && res2.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%133")>=0) 
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("133")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    value = steelBarLevel.Value;
                }
                return value;
            }
            var res3 = buffers.Where(i => i == 131);
            if ((res3 != null && res3.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%131") >= 0)
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("131")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    value = steelBarLevel.Value;
                }
                return value;
            }
            var res4 = buffers.Where(i => i == 130);
            if ((res4 != null && res4.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%130") >= 0)
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("130")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    value = steelBarLevel.Value;
                }
                return value;
            }
            return value;
        }
        /// <summary>
        /// 获取纵筋符号(%%130,%%131,%%132,%%133)
        /// </summary>
        /// <param name="longitudinalReinforcSpec"></param>
        /// <returns></returns>
        public string GetLongitudinalReinforcementSign(string longitudinalReinforcSpec)
        {
            string sign = "";
            if (string.IsNullOrEmpty(longitudinalReinforcSpec))
            {
                return sign;
            }
            byte[] buffers = Encoding.UTF32.GetBytes(longitudinalReinforcSpec);
            var res1 = buffers.Where(i => i == 132);
            if ((res1 != null && res1.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%132") >= 0)
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("132")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    return steelBarLevel.MatchStr;
                }
            }
            var res2 = buffers.Where(i => i == 133);
            if ((res2 != null && res2.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%133") >= 0)
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("133")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    return steelBarLevel.MatchStr;
                }
            }
            var res3 = buffers.Where(i => i == 131);
            if ((res3 != null && res3.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%131") >= 0)
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("131")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    return steelBarLevel.MatchStr;
                }
            }
            var res4 = buffers.Where(i => i == 130);
            if ((res4 != null && res4.Count() > 0) || longitudinalReinforcSpec.IndexOf("%%130") >= 0)
            {
                var steelBarLevel = this.steeBarLevels.Where(i => i.MatchStr.Contains("130")).FirstOrDefault();
                if (steelBarLevel != null)
                {
                    return steelBarLevel.MatchStr;
                }
            }
            return sign;
        }
    }
}
