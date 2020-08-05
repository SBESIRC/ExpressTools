using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThColumnInfo.View;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using System.Runtime.InteropServices;
using System;
using WinForm= System.Windows.Forms;

namespace ThColumnInfo.ViewModel
{
    public class ComponentPropDefineVM : CNotifyPropertyChange
    {
        private ColumnCustomData initCustomData = new ColumnCustomData();
        public ComponentPropDefine Owner { get; set; }
        private List<string> ynList = new List<string>() { "是","否"};
        private ObservableCollection<PropertyInfo> propertyInfos = new ObservableCollection<PropertyInfo>();
        private Dictionary<string, ModifyCustomDataType> propertyNameModType = new Dictionary<string, ModifyCustomDataType>();
        private ObservableCollection<ColorTextInfo> ctInfos=new ObservableCollection<ColorTextInfo>();
        //private string title = "";
        private string propertyName = "";
        private string propertySetText = "";
       
        private bool? recoveryInit = false;
        public bool SelectSwitch { get; set; } = true;
        private ColumnCustomData customData = new ColumnCustomData(); 
        public ParameterSetInfo ParaSetInfo { get; set; }
        public ObservableCollection<ColorTextInfo> CtInfos
        {
            get
            {
                return ctInfos;
            }
            set
            {
                ctInfos = value;
                NotifyPropertyChange("CtInfos");
            }
        }
        private ColumnBindTextManager columnBindManager = null;

        /// <summary>
        /// 恢复初始值
        /// </summary>
        public bool? RecoveryInit
        {
            get
            {
                return recoveryInit;
            }
            set
            {
                recoveryInit = value;
                NotifyPropertyChange("RecoveryInit");
            }
        }
        //public string Title
        //{
        //    get
        //    {
        //        return title;
        //    }
        //    set
        //    {
        //        title = value;
        //        NotifyPropertyChange("Title");
        //    }
        //}
        public List<string> YnList
        {
            get
            {
                return ynList;
            }
            set
            {
                ynList = value;
                NotifyPropertyChange("YnList");
            }
        }
        /// <summary>
        /// 属性名称列表
        /// </summary>
        public ObservableCollection<PropertyInfo> PropertyInfos
        {
            get
            {
                return propertyInfos;
            }
            set
            {
                propertyInfos = value;
                NotifyPropertyChange("PropertyInfos");
            }
        }
        /// <summary>
        /// 属性名称列表下选择的Name值
        /// </summary>
        public string PropertyName
        {
            get
            {
                return propertyName;
            }
            set
            {
                propertyName = value;
                NotifyPropertyChange("PropertyName");
            }
        }
        /// <summary>
        /// 属性设置文本
        /// </summary>
        public string PropertySetText
        {
            get
            {
                return propertySetText;
            }
            set
            {
                propertySetText = value;
                NotifyPropertyChange("PropertySetText");
            }
        }
        public ColumnCustomData CustomData
        {
            get
            {
                return customData;
            }
            set
            {
                customData = value;
                NotifyPropertyChange("CustomData");
            }
        }  
        public ComponentPropDefineVM()
        {
            this.ParaSetInfo = new ParameterSetVM().ParaSetInfo;
            this.initCustomData.AntiSeismicGrade = this.ParaSetInfo.AntiSeismicGrade;
            this.initCustomData.ConcreteStrength = this.ParaSetInfo.ConcreteStrength;
            this.initCustomData.ProtectLayerThickness = this.ParaSetInfo.ProtectLayerThickness.ToString();
            this.propertyInfos.Add(new PropertyInfo { Name= "cbAntiSeismicGrade", Text= "抗震等级" });
            this.propertyInfos.Add(new PropertyInfo { Name = "cbConcreteStrength", Text = "混凝土强度" });
            this.propertyInfos.Add(new PropertyInfo { Name = "tbProtectThickness", Text = "保护层厚度" });
            this.propertyInfos.Add(new PropertyInfo { Name = "tbHoopReinEnlargeTimes", Text = "箍筋放大倍数" });
            this.propertyInfos.Add(new PropertyInfo { Name = "tbLongitudalEnlargeTimes", Text = "纵筋放大倍数" });
            this.propertyInfos.Add(new PropertyInfo { Name = "cbHoopFullHeightEncryption", Text = "箍筋全高加密" });
            this.propertyInfos.Add(new PropertyInfo { Name = "cbCornerColumn", Text = "是否角柱" });

            this.propertyNameModType.Add("cbAntiSeismicGrade", ModifyCustomDataType.AntiSeismicGrade);
            this.propertyNameModType.Add("cbConcreteStrength", ModifyCustomDataType.ConcreteStrength);
            this.propertyNameModType.Add("tbProtectThickness", ModifyCustomDataType.ProtectLayerThickness);
            this.propertyNameModType.Add("tbHoopReinEnlargeTimes", ModifyCustomDataType.HoopReinforcementEnlargeTimes);
            this.propertyNameModType.Add("tbLongitudalEnlargeTimes", ModifyCustomDataType.LongitudinalReinforceEnlargeTimes);
            this.propertyNameModType.Add("cbHoopFullHeightEncryption", ModifyCustomDataType.HoopReinforceFullHeightEncryption);
            this.propertyNameModType.Add("cbCornerColumn", ModifyCustomDataType.CornerColumn);

            this.columnBindManager = new ColumnBindTextManager();
        }
        /// <summary>
        /// 获取修改数据类型
        /// </summary>
        /// <returns></returns>
        private ModifyCustomDataType GetModifyDataType()
        {
            ModifyCustomDataType modCustomDataType= ModifyCustomDataType.None;
            if(this.propertyNameModType.ContainsKey(this.propertyName))
            {
                modCustomDataType = this.propertyNameModType[this.propertyName];
            }
            return modCustomDataType;
        }
        /// <summary>
        /// 获取需要修改的值
        /// </summary>
        /// <returns></returns>
        private object GetModifyValue()
        {
            object value = null;
            ModifyCustomDataType modCustomDataType = GetModifyDataType();
            if(recoveryInit==false)
            {
                switch(modCustomDataType)
                {
                    case ModifyCustomDataType.AntiSeismicGrade:
                        value = this.customData.AntiSeismicGrade;
                        break;
                    case ModifyCustomDataType.ConcreteStrength:
                        value = this.customData.ConcreteStrength;
                        break;
                    case ModifyCustomDataType.CornerColumn:
                        value = this.customData.CornerColumn;
                        break;
                    case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                        value = this.customData.HoopReinforceFullHeightEncryption;
                        break;
                    case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                        value = this.customData.HoopReinforcementEnlargeTimes;
                        break;
                    case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
                        value = this.customData.LongitudinalReinforceEnlargeTimes;
                        break;
                    case ModifyCustomDataType.ProtectLayerThickness:
                        value = this.customData.ProtectLayerThickness;
                        break;
                }
            }
            else if (recoveryInit == true)
            {
                value = "";
            }
            return value;
        }
        public void UpdateTitle()
        {
            ModifyCustomDataType modifyCustomDataType = GetModifyDataType();
            //switch(modifyCustomDataType)
            //{
            //    case ModifyCustomDataType.AntiSeismicGrade:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.AntiSeismicGrade+")";
            //        break;
            //    case ModifyCustomDataType.ConcreteStrength:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.ConcreteStrength + ")";
            //        break;
            //    case ModifyCustomDataType.CornerColumn:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.CornerColumn + ")";
            //        break;
            //    case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.HoopReinforceFullHeightEncryption + ")";
            //        break;
            //    case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.HoopReinforcementEnlargeTimes + ")";
            //        break;
            //    case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.LongitudinalReinforceEnlargeTimes + ")";
            //        break;
            //    case ModifyCustomDataType.ProtectLayerThickness:
            //        this.title = "构件属性修改 (默认值：" + this.initCustomData.ProtectLayerThickness + ")";
            //        break;
            //}
        }
        /// <summary>
        /// 更新属性列表
        /// </summary>
        public void UpdatePropertyList() 
        {
            ModifyCustomDataType modifyCustomDataType = GetModifyDataType();
            List<ColorTextInfo> colorTextInfos = columnBindManager.GetColorTextInfos(modifyCustomDataType);
            this.ctInfos = new ObservableCollection<ColorTextInfo>();
            colorTextInfos.ForEach(i => this.ctInfos.Add(i));
            this.Owner.lbProperties.ItemsSource = this.ctInfos;
        }
        /// <summary>
        /// 把属性列表项的值，赋给需要设置的控件中
        /// </summary>
        /// <param name="value"></param>
        public void PropertyListItemToSetValue(object value)
        {
            if(value==null)
            {
                return;
            }
            ModifyCustomDataType modifyCustomDataType = GetModifyDataType();
            switch(modifyCustomDataType)
            {
                case ModifyCustomDataType.AntiSeismicGrade:
                    this.customData.AntiSeismicGrade = (string)value;
                    break;
                case ModifyCustomDataType.ConcreteStrength:
                    this.customData.ConcreteStrength = (string)value;
                    break;
                case ModifyCustomDataType.CornerColumn:
                    this.customData.CornerColumn = (string)value;
                    break;
                case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                    this.customData.HoopReinforceFullHeightEncryption = (string)value;
                    break;
                case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                    this.customData.HoopReinforcementEnlargeTimes = (string)value;
                    break;
                case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
                    this.customData.LongitudinalReinforceEnlargeTimes = (string)value;
                    break;
                case ModifyCustomDataType.ProtectLayerThickness:
                    this.customData.ProtectLayerThickness = (string)value;
                    break;
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if(Owner != null)
            {
                Owner.Close();
            }
        }
        #region---------- 与Cad交互 ----------
        public void SelectModify()
        {
            try
            {
                Document document = Application.DocumentManager.MdiActiveDocument;
                Editor ed = document.Editor;
                this.columnBindManager.ShowOrHideColumn(true);
                TypedValue[] tvs = new TypedValue[]
                        {
                    new TypedValue((int)DxfCode.Start, "LWPOLYLINE")
                        };
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\n请选择柱子边线";
                pso.RejectObjectsOnLockedLayers = true;
                SelectionFilter sf = new SelectionFilter(tvs);
                while (true)
                {
                    PromptSelectionResult psr = ed.GetSelection(pso, sf);
                    if(psr.Status== PromptStatus.Cancel)
                    {
                        Owner.CloseWindow();
                        break;
                    }
                    if (psr.Status == PromptStatus.OK)
                    {
                        List<ObjectId> selectObjIds = psr.Value.GetObjectIds().ToList();
                        List<ObjectId> currentColumnObjIds = this.columnBindManager.ColumnBindTexts.Select(i => i.ColumnId).ToList();
                        selectObjIds = selectObjIds.Where(i => currentColumnObjIds.IndexOf(i) >= 0).Select(i => i).ToList();
                        if(selectObjIds.Count>0)
                        {
                            ModifyCustomDataType modDataType= GetModifyDataType();
                            object modifyvalue = GetModifyValue();
                            if(this.recoveryInit==true)
                            {
                                this.columnBindManager.ModifyColumnCustomData(selectObjIds, modDataType, modifyvalue,true);
                            }
                            else
                            {
                                this.columnBindManager.ModifyColumnCustomData(selectObjIds, modDataType, modifyvalue);
                            }
                            UpdatePropertyList();
                        }
                    }
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "SelectModify");
            }
            finally
            {
                this.columnBindManager.ShowOrHideColumn(false);
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetFocus")]
        public static extern int SetFocus(IntPtr hWnd);
        public void Refresh()
        {
            ModifyCustomDataType modDataType = GetModifyDataType();
            this.columnBindManager.Refresh(modDataType);
            SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
        }
        
        /// <summary>
        /// 隐藏所有的
        /// </summary>
        public void Hide()
        {
            this.columnBindManager.ShowOrHideColumn(false);
            this.columnBindManager.ShowOrHideFrameText(false);
        }
        public void CancelSelect()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            //doc.SendCommand("\x03\x03");
            doc.SendStringToExecute("\x03\x03",true,false,true); 
        }
        public void ResetInit()
        {
            try
            {
                Document document = Application.DocumentManager.MdiActiveDocument;
                Editor ed = document.Editor;
                TypedValue[] tvs = new TypedValue[]
                        {
                    new TypedValue((int)DxfCode.Start, "LWPOLYLINE")
                        };
                SelectionFilter sf = new SelectionFilter(tvs);
                PromptSelectionResult psr = ed.SelectAll(sf);
                if (psr.Status == PromptStatus.OK)
                {
                    List<ObjectId> allObjIds = psr.Value.GetObjectIds().ToList();
                    List<ObjectId> currentColumnObjIds = this.columnBindManager.ColumnBindTexts.Select(i => i.ColumnId).ToList();
                    allObjIds = allObjIds.Where(i => currentColumnObjIds.IndexOf(i) >= 0).Select(i => i).ToList();
                    if (allObjIds.Count > 0)
                    {
                        ModifyCustomDataType modDataType = GetModifyDataType();
                        object modifyvalue = GetModifyValue();
                        this.columnBindManager.ModifyColumnCustomData(allObjIds, modDataType, modifyvalue,true);
                        this.ctInfos = new ObservableCollection<ColorTextInfo>();
                        this.Owner.lbProperties.ItemsSource = this.ctInfos;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ResetInit");
            }
        }
        #endregion
    }
}
