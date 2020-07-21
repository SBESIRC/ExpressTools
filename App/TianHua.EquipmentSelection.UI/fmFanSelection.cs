using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using AcHelper;
using Linq2Acad;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace TianHua.FanSelection.UI
{
    public partial class fmFanSelection : DevExpress.XtraEditors.XtraForm, IFanSelection
    {

        public PresentersFanSelection m_Presenter;

        public List<FanDataModel> m_ListFan { get; set; }

        public List<string> m_ListScenario { get; set; }

        public List<string> m_ListVentStyle { get; set; }

        public List<string> m_ListVentConnect { get; set; }

        public List<string> m_ListVentLev { get; set; }

        public List<string> m_ListEleLev { get; set; }

        public List<int> m_ListMotorTempo { get; set; }

        public List<string> m_ListMountType { get; set; }

        public List<FanDesignDataModel> m_ListFanDesign { get; set; }

        public FanDesignDataModel m_FanDesign { get; set; }

        public DataManager m_DataMgr = new DataManager();

        /// <summary>
        /// 风机箱选型
        /// </summary>
        public List<FanSelectionData> m_ListFanSelection = new List<FanSelectionData>();
        /// <summary>
        /// 轴流风机选型
        /// </summary>
        public List<FanSelectionData> m_ListAxialFanSelection = new List<FanSelectionData>();
        /// <summary>
        /// 风机箱参数
        /// </summary>
        public List<FanParameters> m_ListFanParameters = new List<FanParameters>();
        /// <summary>
        /// 轴流风机参数
        /// </summary>
        public List<AxialFanParameters> m_ListAxialFanParameters = new List<AxialFanParameters>();

        public string m_Path = Path.Combine(ThCADCommon.SupportPath(), "DesignData");

        public void RessetPresenter()
        {
            if (m_Presenter != null)
            {
                this.Dispose();
                m_Presenter = null;
            }
            m_Presenter = new PresentersFanSelection(this);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        public fmFanSelection()
        {
            InitializeComponent();
        }

        private void fmFanSelection_Load(object sender, EventArgs e)
        {
            InitForm();




        }

        private void InitForm()
        {
            RessetPresenter();
            ComBoxScene.Properties.Items.Clear();
            ComBoxScene.Properties.Items.AddRange(m_ListScenario);
            ComBoxScene.EditValue = "厨房排油烟";

            ComBoxVentStyle.Items.Clear();
            ComBoxVentStyle.Items.AddRange(m_ListVentStyle);

            ComBoxVentConnect.Items.Clear();
            ComBoxVentConnect.Items.AddRange(m_ListVentConnect);

            ComBoxVentLev.Items.Clear();
            ComBoxVentLev.Items.AddRange(m_ListVentLev);

            ComBoxEleLev.Items.Clear();
            ComBoxEleLev.Items.AddRange(m_ListEleLev);

            ComBoxMotorTempo.Items.Clear();
            ComBoxMotorTempo.Items.AddRange(m_ListMotorTempo);

            ComBoxMountType.Items.Clear();
            ComBoxMountType.Items.AddRange(m_ListMountType);

            this.TreeList.ParentFieldName = "PID";
            this.TreeList.KeyFieldName = "ID";
            if (m_ListFan != null && m_ListFan.Count > 0)
                m_ListFan = m_ListFan.OrderBy(p => p.SortID).ToList();
            TreeList.DataSource = m_ListFan;
            this.TreeList.ExpandAll();

            InitFanDesign();

            //TreeList.Columns["SortID"].SortIndex = 0;
            //TreeList.Columns["SortID"].SortMode = DevExpress.XtraGrid.ColumnSortMode.Value;
            //TreeList.Columns["SortID"].SortOrder = SortOrder.Descending;

            InitData();
        }

        private void InitFanDesign()
        {
            m_ListFanDesign = new List<FanDesignDataModel>();
        }

        public void InitData()
        {
            var _JsonFanSelection = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.HTFC_Selection));
            m_ListFanSelection = FuncJson.Deserialize<List<FanSelectionData>>(_JsonFanSelection);

            var _JsonAxialFanSelection = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.AXIAL_Selection));
            m_ListAxialFanSelection = FuncJson.Deserialize<List<FanSelectionData>>(_JsonAxialFanSelection);

            var _JsonFanParameters = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.HTFC_Parameters));
            m_ListFanParameters = FuncJson.Deserialize<List<FanParameters>>(_JsonFanParameters);

            var _JsonAxialFanParameters = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.AXIAL_Parameters));
            m_ListAxialFanParameters = FuncJson.Deserialize<List<AxialFanParameters>>(_JsonAxialFanParameters);

            if (File.Exists(m_Path + @"FanDesignData.json"))
            {
                var _JsonmFanDesign = ReadTxt(m_Path + @"FanDesignData.json");
                m_ListFanDesign = FuncJson.Deserialize<List<FanDesignDataModel>>(_JsonmFanDesign);
            }


        }


        private void TreeList_CustomColumnDisplayText(object sender, DevExpress.XtraTreeList.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "AirVolume")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _Fan = m_ListFan.Find(p => p.ID == _ID);
                if (_Fan == null) { return; }

                if (_Fan.AirVolume > 0)
                {
                    e.DisplayText = _Fan.AirVolume.ToString("#,##0");
                }


            }
            if (e.Column.FieldName == "VentQuan" || e.Column.FieldName == "MotorTempo")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _Fan = m_ListFan.Find(p => p.ID == _ID);
                if (_Fan == null) { return; }
                if (_Fan.PID != "0")
                {
                    e.DisplayText = "-";
                }
            }




        }

        private void PicRemark_Click(object sender, EventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            fmRemark _fmRemark = new fmRemark();
            _fmRemark.m_Remark = _Fan.Remark;
            if (_fmRemark.ShowDialog() == DialogResult.OK)
            {
                _Fan.Remark = _fmRemark.m_Remark;
                TreeList.Refresh();
            }

        }

        private void TxtWindResis_Click(object sender, EventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            fmDragCalc _fmDragCalc = new fmDragCalc();
            _fmDragCalc.InitForm(_Fan);
            if (_fmDragCalc.ShowDialog() == DialogResult.OK)
            {
                if (_fmDragCalc.m_ListFan != null && _fmDragCalc.m_ListFan.Count > 0)
                    _Fan = _fmDragCalc.m_ListFan.First();
                SetFanModel();
                TreeList.Refresh();
            }
        }

        private void TxtAirVolume_Click(object sender, EventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            if (_Fan.AirCalcFactor == 0)
            {
                if(_Fan.ScenarioType == 1)
                {
                    _Fan.AirCalcFactor = 1.2;
                }
                else
                {
                    _Fan.AirCalcFactor = 1.1;
                }

            }
            fmAirVolumeCalc _fmAirVolumeCalc = new fmAirVolumeCalc();

            _fmAirVolumeCalc.InitForm(_Fan);
            if (_fmAirVolumeCalc.ShowDialog() == DialogResult.OK)
            {
                if (_fmAirVolumeCalc.m_ListFan != null && _fmAirVolumeCalc.m_ListFan.Count > 0)
                {
                    _Fan.AirVolume = _fmAirVolumeCalc.m_ListFan.First().AirVolume;
                    _Fan.AirCalcFactor = _fmAirVolumeCalc.m_ListFan.First().AirCalcFactor;
                    _Fan.AirCalcValue = _fmAirVolumeCalc.m_ListFan.First().AirCalcValue;
                }
                 
                SetFanModel();
                TreeList.Refresh();
            }
        }

        private void TxtModelName_Click(object sender, EventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }

            fmFanModel _fmFanModel = new fmFanModel();
            _fmFanModel.InitForm(_Fan, m_ListFan);
            if (_fmFanModel.ShowDialog() == DialogResult.OK)
            {
                if (_fmFanModel.m_Fan != null)
                    _Fan = _fmFanModel.m_Fan;
                TreeList.Refresh();
            }

        }

        private void TreeList_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            //if (e.Column.FieldName == "VentStyle")
            //{
            //    if (FuncStr.NullToStr(_Fan.VentStyle) == "轴流")
            //    {
            //        _Fan.VentConnect = "直连";
            //        _Fan.IntakeForm = "直进式";
            //        TreeList.Refresh();
            //    }


            //    SetFanModel();
            //}

            if (e.Column.FieldName == "VentNum")
            {
                MatchCollection _Matche = Regex.Matches(_Fan.VentNum, @"\d+\,*\-*");

                _Fan.ListVentQuan = new List<int>();

                string _Sign = string.Empty;

                if (_Matche.Count > 0)
                {
                    for (int i = 0; i < _Matche.Count; i++)
                    {
                        string _Str = string.Empty;
                        string _TmpSign = string.Empty;
                        if (FuncStr.NullToStr(_Matche[i]).Contains("-"))
                        {
                            _TmpSign = "-";
                        }
                        if (FuncStr.NullToStr(_Matche[i]).Contains(","))
                        {

                            _TmpSign = ",";
                        }
                        _Str = FuncStr.NullToStr(_Matche[i]).Replace(",", "").Replace("-", "");
                        if (_Str == string.Empty) continue;

                        var _Tmp = FuncStr.NullToInt(_Str);

                        CalcVentQuan(_Fan.ListVentQuan, _Tmp, _Sign);
                        _Sign = _TmpSign;
                    }
                }

                if (_Fan.ListVentQuan.Count > 0)
                {
                    _Fan.ListVentQuan = _Fan.ListVentQuan.Distinct().ToList();
                    _Fan.ListVentQuan.Sort();
                    _Fan.VentQuan = _Fan.ListVentQuan.Count();
                }


            }

            if (e.Column.FieldName == "AirVolume")
            {
                var _Rem = FuncStr.NullToInt(e.Value) % 50;
                if (_Rem != 0)
                {
                    var _UnitsDigit = FindNum(FuncStr.NullToInt(e.Value), 1);

                    var _TensDigit = FindNum(FuncStr.NullToInt(e.Value), 2);

                    var _Tmp = FuncStr.NullToInt(_TensDigit.ToString() + _UnitsDigit.ToString());

                    if (_Tmp < 50)
                        _Fan.AirVolume = FuncStr.NullToInt(FuncStr.NullToStr(e.Value).Replace(FuncStr.NullToStr(_Tmp), "50"));
                    else
                    {
                        var _DifferenceValue = 100 - _Tmp;
                        _Fan.AirVolume = FuncStr.NullToInt(e.Value) + _DifferenceValue;
                    }
                }


                SetFanModel();
            }


        }

        public void SetFanModel()
        {
            if (TreeList == null) { return; }
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            if (_Fan.AirVolume == 0 || _Fan.WindResis == 0) { return; }
            if (FuncStr.NullToStr(_Fan.AirVolume) == string.Empty || FuncStr.NullToStr(_Fan.VentStyle) == string.Empty || FuncStr.NullToStr(_Fan.WindResis) == string.Empty)
            {
                ClearFanModel(_Fan);
                return;
            }
            if (FuncStr.NullToStr(_Fan.VentStyle) == "轴流")
            {
                var _ListFanSelection = m_ListAxialFanSelection.FindAll(p => FuncStr.NullToInt(p.X) >= _Fan.AirVolume && FuncStr.NullToInt(p.Y) >= _Fan.WindResis);
                if (_ListFanSelection == null || _ListFanSelection.Count == 0) { ClearFanModel(_Fan); _Fan.FanModelName = "无此风机"; return; }
                _ListFanSelection = _ListFanSelection.OrderBy(p => FuncStr.NullToInt(p.X)).OrderBy(p => FuncStr.NullToInt(p.Y)).ToList();
                var _FanSelection = _ListFanSelection.First();
                if (_FanSelection == null || FuncStr.NullToStr(_FanSelection.Value) == string.Empty) { ClearFanModel(_Fan); _Fan.FanModelName = "无此风机"; return; }
                var _ListStr = FuncStr.NullToStr(_FanSelection.Value).Split('/');
                if (_ListStr != null && _ListStr.Count() == 2)
                {
                    var _ModelNum = FuncStr.NullToStr(_ListStr[0]);
                    var _No = FuncStr.NullToInt(_ListStr[1]);
                    var _FanParameters = m_ListAxialFanParameters.Find(p => p.No == FuncStr.NullToStr(_No) && p.ModelNum == _ModelNum);
                    if (_FanParameters != null)
                    {
                        _Fan.FanModelID = _FanParameters.No;
                        _Fan.FanModelName = _FanParameters.ModelNum;
                        _Fan.FanModelNum = _FanParameters.No;
                        _Fan.FanModelCCCF = _FanParameters.ModelNum;
                        _Fan.FanModelAirVolume = _FanParameters.AirVolume;
                        _Fan.FanModelPa = _FanParameters.Pa;
                        _Fan.FanModelMotorPower = _FanParameters.Power;
                        _Fan.FanModelNoise = _FanParameters.Noise;
                        _Fan.FanModelFanSpeed = _FanParameters.Rpm;
                        _Fan.FanModelPower = string.Empty;
                        _Fan.FanModelLength = _FanParameters.Length;
                        _Fan.FanModelDIA = _FanParameters.Diameter;
                        _Fan.FanModelWeight = _FanParameters.Weight;
                    }
                }
                TreeList.RefreshNode(TreeList.FocusedNode);
            }
            else
            {
                var _ListFanSelection = m_ListFanSelection.FindAll(p => FuncStr.NullToInt(p.X) >= _Fan.AirVolume && FuncStr.NullToInt(p.Y) >= _Fan.WindResis);
                if (_ListFanSelection == null || _ListFanSelection.Count == 0) { ClearFanModel(_Fan); _Fan.FanModelName = "无此风机"; return; }
                _ListFanSelection = _ListFanSelection.OrderBy(p => FuncStr.NullToInt(p.X)).OrderBy(p => FuncStr.NullToInt(p.Y)).ToList();
                var _FanSelection = _ListFanSelection.First();
                if (_FanSelection == null || FuncStr.NullToStr(_FanSelection.Value) == string.Empty) { ClearFanModel(_Fan); _Fan.FanModelName = "无此风机"; return; }
                var _ListStr = FuncStr.NullToStr(_FanSelection.Value).Split('/');
                if (_ListStr != null && _ListStr.Count() == 2)
                {
                    var _CCCF = FuncStr.NullToStr(_ListStr[0]);
                    var _No = FuncStr.NullToInt(_ListStr[1]);

                    //    var _FanParameters = m_ListFanParameters.Find(p => p.Suffix == FuncStr.NullToStr(_No) && p.CCCF_Spec == _CCCF && FuncStr.NullToStr(p.AirVolume) == FuncStr.NullToStr(_FanSelection.X) && FuncStr.NullToStr(p.Pa) == FuncStr.NullToStr(_FanSelection.Y));
                    var _FanParameters = m_ListFanParameters.Find(p => p.Suffix == FuncStr.NullToStr(_No) && p.CCCF_Spec == _CCCF);
                    if (_FanParameters != null)
                    {
                        _Fan.FanModelID = _FanParameters.Suffix;
                        _Fan.FanModelName = _FanParameters.CCCF_Spec;
                        _Fan.FanModelNum = _FanParameters.No;
                        _Fan.FanModelCCCF = _FanParameters.CCCF_Spec;
                        _Fan.FanModelAirVolume = _FanParameters.AirVolume;
                        _Fan.FanModelPa = _FanParameters.Pa;
                        _Fan.FanModelMotorPower = _FanParameters.Power;
                        _Fan.FanModelNoise = _FanParameters.Noise;
                        _Fan.FanModelFanSpeed = _FanParameters.Rpm;
                        _Fan.FanModelPower = string.Empty;
                        _Fan.FanModelLength = _FanParameters.Length;
                        _Fan.FanModelWidth = _FanParameters.Width;
                        _Fan.FanModelHeight = _FanParameters.Height;
                        _Fan.FanModelWeight = _FanParameters.Weight;
                    }


                }

                TreeList.RefreshNode(TreeList.FocusedNode);
            }


        }

        private static void ClearFanModel(FanDataModel _Fan)
        {
            _Fan.FanModelID = string.Empty;
            _Fan.FanModelName = string.Empty;
            _Fan.FanModelNum = string.Empty;
            _Fan.FanModelCCCF = string.Empty;
            _Fan.FanModelAirVolume = string.Empty;
            _Fan.FanModelPa = string.Empty;
            _Fan.FanModelMotorPower = string.Empty;
            _Fan.FanModelNoise = string.Empty;
            _Fan.FanModelFanSpeed = string.Empty;
            _Fan.FanModelPower = string.Empty;
            _Fan.FanModelLength = string.Empty;
            _Fan.FanModelWidth = string.Empty;
            _Fan.FanModelHeight = string.Empty;
            _Fan.FanModelWeight = string.Empty;
        }

        private void CalcVentQuan(List<int> _ListVentQuan, int _Tmp, string _Sign)
        {
            if (_ListVentQuan.Count == 0 || _Sign == string.Empty || _Sign == ",") { _ListVentQuan.Add(_Tmp); return; }
            var _OldValue = FuncStr.NullToInt(_ListVentQuan.Last());
            if (_OldValue > _Tmp)
            {
                for (int i = _Tmp + 1; i <= _OldValue; i++)
                {
                    _ListVentQuan.Add(i);
                }
            }
            else if (_OldValue < _Tmp)
            {
                for (int i = _OldValue + 1; i <= _Tmp; i++)
                {
                    _ListVentQuan.Add(i);
                }
            }

        }

        private void TreeList_CustomNodeCellEditForEditing(object sender, GetCustomNodeCellEditEventArgs e)
        {
            var _TreeList = sender as TreeList;
            if (_TreeList == null) { return; }
            var _Fan = _TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            if (e.Column.FieldName == "VentConnect")
            {

                var _Edit = TreeList.RepositoryItems["ComBoxVentConnect"] as DevExpress.XtraEditors.Repository.RepositoryItemComboBox;


                if (FuncStr.NullToStr(_Fan.VentStyle) == "轴流")
                {
                    _Edit.Items.Clear();
                    ComBoxVentConnect.Items.Add("直连");
                }
                else
                {
                    _Edit.Items.Clear();
                    ComBoxVentConnect.Items.AddRange(m_ListVentConnect);
                }

                e.RepositoryItem = _Edit;
            }

            if (e.Column.FieldName == "Use")
            {

                if (FuncStr.NullToStr(_Fan.Scenario) == "平时排风兼消防排烟" || FuncStr.NullToStr(_Fan.Scenario) == "平时送风兼消防补风")
                {
                    var _EditTxt = TreeList.RepositoryItems["TxtUse"] as DevExpress.XtraEditors.Repository.RepositoryItemTextEdit;

                    e.RepositoryItem = _EditTxt;
                }

                if (FuncStr.NullToStr(_Fan.Scenario) == "平时排风兼事故排风" || FuncStr.NullToStr(_Fan.Scenario) == "平时送风兼事故补风")
                {
                    var _EditComBox = TreeList.RepositoryItems["ComBoxUse"] as DevExpress.XtraEditors.Repository.RepositoryItemComboBox;


                    e.RepositoryItem = _EditComBox;
                }




            }






        }


        public string ReadTxt(string _Path)
        {
            try
            {
                using (StreamReader _StreamReader = File.OpenText(_Path))
                {
                    return _StreamReader.ReadToEnd();
                }
            }
            catch
            {
                XtraMessageBox.Show("数据文件读取时发生错误！");
                return string.Empty;

            }
        }

        private void ComBoxScene_SelectedValueChanged(object sender, EventArgs e)
        {
            switch (FuncStr.NullToStr(ComBoxScene.EditValue))
            {
                case "平时送风":
                case "平时排风":
                    ColAddAuxiliary.Visible = true;
                    BandUse.Visible = false;
                    break;
                case "平时排风兼消防排烟":
                case "平时送风兼消防补风":
                case "平时排风兼事故排风":
                case "平时送风兼事故补风":
                    BandUse.Visible = true;
                    ColAddAuxiliary.Visible = false;
                    break;
                default:
                    BandUse.Visible = false;
                    ColAddAuxiliary.Visible = false;
                    break;
            }


            var _FilterString = @" Scenario =  '" + FuncStr.NullToStr(ComBoxScene.EditValue) + "'";

            TreeList.ActiveFilterString = _FilterString;

            if (m_ListFan == null) { m_ListFan = new List<FanDataModel>(); }

            if (m_ListFan == null || m_ListFan.Count == 0)
            {
                BtnAdd_Click(null, null);
                return;
            }

            var _List = m_ListFan.FindAll(p => p.Scenario == FuncStr.NullToStr(ComBoxScene.EditValue));

            if (_List == null || _List.Count == 0)
            {
                BtnAdd_Click(null, null);
                return;
            }
        }

        private void PictAddAuxiliary_Click(object sender, EventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null || _Fan.PID != "0") { return; }
            var _ListFan = m_ListFan.FindAll(p => p.PID == _Fan.ID && p.ID != _Fan.ID);
            if (_ListFan == null || _ListFan.Count == 0)
            {
                FanDataModel _FanDataModel = new FanDataModel();
                _FanDataModel.ID = Guid.NewGuid().ToString();
                _FanDataModel.Scenario = FuncStr.NullToStr(ComBoxScene.EditValue);
                _FanDataModel.PID = _Fan.ID;
                _FanDataModel.Name = "低速";
                _FanDataModel.AirVolume = 0;

                _FanDataModel.InstallSpace = "-";
                _FanDataModel.InstallFloor = "-";
                _FanDataModel.VentQuan = 0;
                _FanDataModel.VentNum = "-";

                _FanDataModel.VentStyle = "-";
                _FanDataModel.VentConnect = "-";
                _FanDataModel.VentLev = "-";
                _FanDataModel.EleLev = "-";
                _FanDataModel.FanModelName = "-";

                _FanDataModel.MountType = "-";
                m_ListFan.Add(_FanDataModel);
            }
            _Fan.Control = "双速";
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();
        }

        private void TreeList_ShowingEditor(object sender, CancelEventArgs e)
        {
            var _TreeList = sender as TreeList;
            if (_TreeList == null) { return; }

            var _FanDataModel = _TreeList.GetFocusedRow() as FanDataModel;
            if (_FanDataModel == null) { return; }
            if (_FanDataModel.PID != "0")
            {
                if (_TreeList.FocusedColumn.FieldName != "AirVolume" && _TreeList.FocusedColumn.FieldName != "WindResis")
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (_TreeList.FocusedColumn.FieldName == "FanModelName")
            {
                if (_FanDataModel.FanModelName == string.Empty || _FanDataModel.FanModelName == "无此风机")
                {
                    e.Cancel = true;
                    return;
                }

            }



            if (_TreeList.FocusedColumn.FieldName == "Use")
            {
                if (FuncStr.NullToStr(_FanDataModel.Scenario) == "平时排风兼消防排烟" || FuncStr.NullToStr(_FanDataModel.Scenario) == "平时送风兼消防补风")
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (_TreeList.FocusedColumn.FieldName == "IntakeForm")
            {
                if (FuncStr.NullToStr(_FanDataModel.VentStyle) == "轴流")
                {
                    e.Cancel = true;
                    return;

                }

            }



        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            TreeList.PostEditor();
            BtnAdd.Focus();
            FanDataModel _FanDataModel = new FanDataModel();
            _FanDataModel.Scenario = FuncStr.NullToStr(ComBoxScene.EditValue);
            _FanDataModel.ID = Guid.NewGuid().ToString();
            _FanDataModel.PID = "0";
            _FanDataModel.Name = "未命名风机";
            _FanDataModel.InstallSpace = "未指定子项";
            _FanDataModel.InstallFloor = "未指定楼层";
            _FanDataModel.VentNum = "1";
            _FanDataModel.VentQuan = 1;
            _FanDataModel.Remark = string.Empty;
            _FanDataModel.AirVolume = 0;
            _FanDataModel.WindResis = 0;
            _FanDataModel.VentStyle = "前倾离心";
            _FanDataModel.VentConnect = "皮带";
            _FanDataModel.IntakeForm = "直进式";
            _FanDataModel.VentLev = "2级";
            _FanDataModel.EleLev = "2级";
            _FanDataModel.MotorTempo = 1450;
            _FanDataModel.FanModelName = string.Empty;
            _FanDataModel.MountType = "吊装";
            _FanDataModel.Control = "单速";
            _FanDataModel.PowerType = "普通";
            _FanDataModel.SortID = m_ListFan.Count + 1;
            if (_FanDataModel.Scenario.Contains("消防"))
            {
                _FanDataModel.ScenarioType = 1;
            }
            else
            {
                _FanDataModel.ScenarioType = 2;
            }

            var _FanPrefixDict = PubVar.g_ListFanPrefixDict.Find(s => s.FanUse == _FanDataModel.Scenario);
            if (_FanPrefixDict != null)
            {
                _FanDataModel.SortScenario = _FanPrefixDict.No;
            }
            if (FuncStr.NullToStr(ComBoxScene.EditValue) == "平时排风兼消防排烟" || FuncStr.NullToStr(ComBoxScene.EditValue) == "平时送风兼消防补风")
            {
                _FanDataModel.Remark = "消防兼用";
                _FanDataModel.Use = "消防排烟";
                _FanDataModel.Control = "双速";
                AddAuxiliary(_FanDataModel);
            }

            if (FuncStr.NullToStr(ComBoxScene.EditValue) == "平时送风兼事故补风" || FuncStr.NullToStr(ComBoxScene.EditValue) == "平时排风兼事故排风")
            {
                _FanDataModel.Remark = "事故兼用";
                _FanDataModel.Use = "事故排风";
                _FanDataModel.Control = "双速";
                AddAuxiliary(_FanDataModel);
            }

            if (FuncStr.NullToStr(_FanDataModel.Scenario).Contains("消防"))
            {
                _FanDataModel.PowerType = "消防";
                _FanDataModel.VentStyle = "轴流";
                _FanDataModel.VentConnect = "直连";
                _FanDataModel.IntakeForm = "直进式";
            }
            if (FuncStr.NullToStr(_FanDataModel.Scenario).Contains("事故"))
            {
                _FanDataModel.PowerType = "事故";
            }

            m_ListFan.Add(_FanDataModel);
            if (m_ListFan != null && m_ListFan.Count > 0)
                m_ListFan = m_ListFan.OrderBy(p => p.SortID).ToList();
            TreeList.DataSource = m_ListFan;
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();


            TreeList.FocusedNode = TreeList.Nodes.LastNode;
            TreeList.ShowEditor();
        }

        public void AddAuxiliary(FanDataModel _MainFan)
        {

            FanDataModel _FanDataModel = new FanDataModel();
            _FanDataModel.ID = Guid.NewGuid().ToString();
            _FanDataModel.Scenario = FuncStr.NullToStr(ComBoxScene.EditValue);
            _FanDataModel.PID = _MainFan.ID;
            _FanDataModel.AirVolume = 0;

            _FanDataModel.InstallSpace = "-";
            _FanDataModel.InstallFloor = "-";
            _FanDataModel.VentQuan = 0;
            _FanDataModel.VentNum = "-";

            _FanDataModel.VentStyle = "-";
            _FanDataModel.VentConnect = "-";
            _FanDataModel.VentLev = "-";
            _FanDataModel.EleLev = "-";
            _FanDataModel.FanModelName = "-";
            _FanDataModel.MountType = "-";

            if (FuncStr.NullToStr(ComBoxScene.EditValue) == "平时排风兼消防排烟" || FuncStr.NullToStr(ComBoxScene.EditValue) == "平时送风兼消防补风")
            {
                _FanDataModel.Name = "平时";
                _FanDataModel.Use = "平时排风";
            }

            if (FuncStr.NullToStr(ComBoxScene.EditValue) == "平时送风兼事故补风" || FuncStr.NullToStr(ComBoxScene.EditValue) == "平时排风兼事故排风")
            {
                _FanDataModel.Name = "兼用";
                _FanDataModel.Use = "平时排风";
            }
            m_ListFan.Add(_FanDataModel);
        }


        private void BtnDle_Click(object sender, EventArgs e)
        {
            TreeList.PostEditor();
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null || TreeList.FocusedNode == null) { return; }
            if (_Fan.Scenario == "平时送风" || _Fan.Scenario == "平时排风")
            {
                if (_Fan.PID == "0")
                {
                    if (XtraMessageBox.Show(" 已插入图纸的风机图块也将被删除，是否继续？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        TreeList.DeleteSelectedNodes();
                    }
                }
                else
                {
                    if (XtraMessageBox.Show(" 是否确认删除低速工况？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        TreeList.DeleteSelectedNodes();
                    }
                }

            }
            else
            {
                if (XtraMessageBox.Show(" 已插入图纸的风机图块也将被删除，是否继续？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (_Fan.PID == "0")
                    {
                        TreeList.DeleteSelectedNodes();
                    }
                    else
                    {
                        TreeList.DeleteSelectedNodes();
                        var _MainFan = m_ListFan.Find(p => p.ID == _Fan.PID);
                        if (_MainFan != null)
                        {
                            m_ListFan.Remove(_MainFan);
                            TreeList.RefreshDataSource();
                            this.TreeList.ExpandAll();
                        }
                    }


                }

            }





            //var _List = TreeList.GetAllCheckedNodes();
            //List<FanDataModel> _ListFan = new List<FanDataModel>();
            //if (_List != null && _List.Count > 0)
            //{
            //    _List.ForEach(p =>
            //   {
            //       var _ID = p.GetValue("ID");
            //       var _Fan = m_ListFan.Find(s => FuncStr.NullToStr(s.ID) == FuncStr.NullToStr(_ID));
            //       if (_Fan != null)
            //           _ListFan.Add(_Fan);
            //   });
            //    if (_ListFan != null && _ListFan.Count > 0)
            //    {
            //        if (XtraMessageBox.Show(" 已插入图纸的风机图块也将被删除，是否继续？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //        {
            //            m_ListFan.RemoveAll(p => _ListFan.Contains(p));
            //            TreeList.DataSource = m_ListFan;
            //            TreeList.RefreshDataSource();
            //            this.TreeList.ExpandAll();

            //        }
            //    }

            //}
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {


            //TreeListNode _FocuesNode = this.TreeList.FocusedNode;
            //TreeList.SetNodeIndex(_FocuesNode, 0);



            TreeList.PostEditor();
            var _Index = TreeList.GetNodeIndex(TreeList.FocusedNode);
            if (_Index == 0) { return; }
            TreeListNode _FocuesNode = this.TreeList.FocusedNode;
            var _FocusedNodeID = this.TreeList.FocusedNode.Id;
            TreeList.BeginUpdate();
            int PrevNodeIndex = this.TreeList.GetNodeIndex(_FocuesNode.PrevNode);
            TreeList.SetNodeIndex(_FocuesNode, PrevNodeIndex);
            TreeList.EndUpdate();
            for (int i = 0; i < TreeList.Nodes.Count; i++)
            {
                var _ID = TreeList.Nodes[i].GetValue("ID");
                var _Name = TreeList.Nodes[i].GetValue("Name");
                var _iX = TreeList.GetNodeIndex(TreeList.Nodes[i]);
                var _Fan = m_ListFan.Find(p => FuncStr.NullToStr(p.ID) == FuncStr.NullToStr(_ID));
                if (_Fan != null)
                    _Fan.SortID = _iX;

            }
            if (m_ListFan != null && m_ListFan.Count > 0)
                m_ListFan = m_ListFan.OrderBy(p => p.SortID).ToList();
            TreeList.DataSource = m_ListFan;
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();

            TreeList.FocusedNode = TreeList.Nodes.LastNode;
            TreeList.FocusedNode = TreeList.FindNodeByID(_FocusedNodeID - 1);




        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            TreeList.PostEditor();
            var _Index = TreeList.GetNodeIndex(TreeList.FocusedNode);
            if (_Index == m_ListFan.Count - 1) { return; }
            TreeList.Columns["SortID"].SortOrder = SortOrder.None;
            TreeListNode _FocuesNode = this.TreeList.FocusedNode;
            var _FocusedNodeID = this.TreeList.FocusedNode.Id;
            TreeList.BeginUpdate();
            int PrevNodeIndex = this.TreeList.GetNodeIndex(_FocuesNode.NextNode);
            TreeList.SetNodeIndex(_FocuesNode, PrevNodeIndex);
            TreeList.EndUpdate();
            for (int i = 0; i < TreeList.Nodes.Count; i++)
            {
                var _ID = TreeList.Nodes[i].GetValue("ID");
                var _Name = TreeList.Nodes[i].GetValue("Name");
                var _iX = TreeList.GetNodeIndex(TreeList.Nodes[i]);
                var _Fan = m_ListFan.Find(p => FuncStr.NullToStr(p.ID) == FuncStr.NullToStr(_ID));
                if (_Fan != null)
                    _Fan.SortID = _iX;

            }
            if (m_ListFan != null && m_ListFan.Count > 0)
                m_ListFan = m_ListFan.OrderBy(p => p.SortID).ToList();
            TreeList.DataSource = m_ListFan;
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();
            TreeList.FocusedNode = TreeList.FindNodeByID(_FocusedNodeID + 1);

        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null || TreeList.FocusedNode == null) { return; }
            List<FanDataModel> _ListTemp = new List<FanDataModel>();
            string _Guid = Guid.NewGuid().ToString();
            var _Json = FuncJson.Serialize(_Fan);
            var _FanDataModel = FuncJson.Deserialize<FanDataModel>(_Json);
            if (_Fan.PID == "0")
            {
                _FanDataModel.ID = _Guid;
                _FanDataModel.PID = "0";
                _FanDataModel.Name = SetFanDataModelName(_FanDataModel);
                _ListTemp.Add(_FanDataModel);

                var _SonFan = m_ListFan.Find(p => p.PID == _Fan.ID);
                if (_SonFan != null)
                {
                    var _SonJson = FuncJson.Serialize(_SonFan);
                    var _SonFanData = FuncJson.Deserialize<FanDataModel>(_SonJson);

                    _SonFanData.ID = Guid.NewGuid().ToString();
                    _SonFanData.PID = _Guid;
                    _ListTemp.Add(_SonFanData);
                }


                var _Inidex = m_ListFan.IndexOf(_Fan);
                m_ListFan.InsertRange(_Inidex + 1, _ListTemp);



            }
            else
            {
                var _MainFan = m_ListFan.Find(p => p.ID == _FanDataModel.PID);
                if (_MainFan != null)
                {
                    var _MainJson = FuncJson.Serialize(_MainFan);
                    var _MainFanData = FuncJson.Deserialize<FanDataModel>(_MainJson);
                    _MainFanData.ID = _Guid;
                    _MainFanData.PID = "0";
                    _MainFanData.Name = SetFanDataModelName(_MainFanData);
                    _ListTemp.Add(_MainFanData);
                    var _Inidex = m_ListFan.IndexOf(_MainFan);


                    _FanDataModel.ID = Guid.NewGuid().ToString();
                    _FanDataModel.PID = _Guid;
                    _ListTemp.Add(_FanDataModel);
                    m_ListFan.InsertRange(_Inidex + 1, _ListTemp);
                }


            }
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();
        }

        public string SetFanDataModelName(FanDataModel _FanDataModel)
        {
            var _List = m_ListFan.FindAll(p => p.Name.Contains(_FanDataModel.Name) && p.PID == _FanDataModel.PID && p.ID != _FanDataModel.ID);
            if (_List == null || _List.Count == 0) { return _FanDataModel.Name + " - 副本"; }
            for (int i = 1; i < 10000; i++)
            {
                if (i == 1)
                {
                    var _ListTemp1 = m_ListFan.FindAll(p => p.Name == _FanDataModel.Name + " - 副本" && p.PID == _FanDataModel.PID && p.ID != _FanDataModel.ID);
                    if (_ListTemp1 == null || _ListTemp1.Count == 0) { return _FanDataModel.Name + " - 副本"; }
                }
                else
                {
                    var _ListTemp = m_ListFan.FindAll(p => p.Name == _FanDataModel.Name + " - 副本(" + i + ")" && p.PID == _FanDataModel.PID && p.ID != _FanDataModel.ID);
                    if (_ListTemp == null || _ListTemp.Count == 0) { return _FanDataModel.Name + " - 副本(" + i + ")"; }
                }

            }
            return string.Empty;
        }

        private void ComBoxUse_EditValueChanged(object sender, EventArgs e)
        {
            TreeList.PostEditor();
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null || TreeList.FocusedNode == null) { return; }
            var _SonFan = m_ListFan.Find(p => p.PID == _Fan.ID);
            if (_SonFan != null)
            {
                if (FuncStr.NullToStr(_Fan.Use) == "事故排风")
                {
                    _SonFan.Use = "平时排风";
                }
                else
                {
                    _SonFan.Use = "事故排风";
                }
                TreeList.RefreshDataSource();
            }

        }

        private void BarBtnOpen_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            fmDesignData _fmDesignData = new fmDesignData();

            _fmDesignData.InitForm(m_ListFanDesign, "打开");

            if (_fmDesignData.ShowDialog() == DialogResult.OK)
            {
                if (_fmDesignData.m_FanDesign != null && FuncStr.NullToStr(_fmDesignData.m_FanDesign.Path) != string.Empty && FuncStr.NullToStr(_fmDesignData.m_FanDesign.Name) != string.Empty)
                {
                    m_FanDesign = _fmDesignData.m_FanDesign;
                    m_ListFanDesign = _fmDesignData.m_ListFanDesign;
                    var _JsonListFan = ReadTxt(m_FanDesign.Path);

                    m_ListFan = FuncJson.Deserialize<List<FanDataModel>>(_JsonListFan);

                    if (m_ListFan != null && m_ListFan.Count > 0)
                        m_ListFan = m_ListFan.OrderBy(p => p.SortID).ToList();
                    TreeList.DataSource = m_ListFan;
                    this.TreeList.ExpandAll();

                }
            }
        }

        private void BarBtnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_ListFan == null || m_ListFan.Count == 0) { return; }
            TreeList.PostEditor();
            if (m_FanDesign == null || FuncStr.NullToStr(m_FanDesign.Name) == string.Empty)
            {
                fmDesignData _fmDesignData = new fmDesignData();
                _fmDesignData.InitForm(m_ListFanDesign, "保存");
                if (_fmDesignData.ShowDialog() == DialogResult.OK)
                {
                    if (_fmDesignData.m_FanDesign != null && FuncStr.NullToStr(_fmDesignData.m_FanDesign.Path) != string.Empty && FuncStr.NullToStr(_fmDesignData.m_FanDesign.Name) != string.Empty)
                    {
                        var _Json = FuncJson.Serialize(m_ListFan);
                        JsonExporter.Instance.SaveToFile(FuncStr.NullToStr(_fmDesignData.m_FanDesign.Path), Encoding.UTF8, _Json);
                        m_FanDesign = _fmDesignData.m_FanDesign;
                        m_ListFanDesign = _fmDesignData.m_ListFanDesign;
                    }
                }
            }
            else
            {
                m_FanDesign.LastOperationDate = DateTime.Now;
                var _JsonFanDesign = FuncJson.Serialize(m_ListFanDesign);
                JsonExporter.Instance.SaveToFile(m_Path + @"FanDesignData.json", Encoding.UTF8, _JsonFanDesign);

                var _JsonFan = FuncJson.Serialize(m_ListFan);
                JsonExporter.Instance.SaveToFile(FuncStr.NullToStr(m_FanDesign.Path), Encoding.UTF8, _JsonFan);
            }


        }

        private void BarBtnSaveAs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_ListFan == null || m_ListFan.Count == 0) { return; }
            TreeList.PostEditor();
            m_FanDesign = null;
            if (m_FanDesign == null || FuncStr.NullToStr(m_FanDesign.Name) == string.Empty)
            {
                fmDesignData _fmDesignData = new fmDesignData();
                _fmDesignData.InitForm(m_ListFanDesign, "保存");
                if (_fmDesignData.ShowDialog() == DialogResult.OK)
                {
                    if (_fmDesignData.m_FanDesign != null && FuncStr.NullToStr(_fmDesignData.m_FanDesign.Path) != string.Empty && FuncStr.NullToStr(_fmDesignData.m_FanDesign.Name) != string.Empty)
                    {
                        var _Json = FuncJson.Serialize(m_ListFan);
                        JsonExporter.Instance.SaveToFile(FuncStr.NullToStr(_fmDesignData.m_FanDesign.Path), Encoding.UTF8, _Json);
                        m_FanDesign = _fmDesignData.m_FanDesign;
                    }
                }
            }


        }

        private void BarBtnExportFanPara_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string _ImportExcelPath = m_Path + "FanPara.xlsx";
            Microsoft.Office.Interop.Excel.Application _ExclApp = new Microsoft.Office.Interop.Excel.Application();
            _ExclApp.DisplayAlerts = false;
            _ExclApp.Visible = false;
            _ExclApp.ScreenUpdating = false;
            Microsoft.Office.Interop.Excel.Workbook _WorkBook = _ExclApp.Workbooks.Open(_ImportExcelPath, System.Type.Missing, System.Type.Missing, System.Type.Missing,
              System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
            System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);

            Microsoft.Office.Interop.Excel.Worksheet _Sheet = _WorkBook.Worksheets[1];

            var _List = GetListExportFanPara();
            if (_List == null || _List.Count == 0) { return; }
            if (_List != null && _List.Count > 0) _List = _List.OrderBy(p => p.SortScenario).OrderBy(p => p.SortID).ToList();
            var i = 4;
            _List.ForEach(p =>
            {
                _Sheet.Cells[i, 1] = p.No;
                _Sheet.Cells[i, 2] = p.Coverage;
                _Sheet.Cells[i, 3] = p.FanForm;
                _Sheet.Cells[i, 4] = p.CalcAirVolume;
                _Sheet.Cells[i, 5] = p.FanDelivery;
                _Sheet.Cells[i, 6] = p.Pa;
                _Sheet.Cells[i, 7] = p.FanEnergyLevel;
                _Sheet.Cells[i, 8] = p.FanEfficiency;
                _Sheet.Cells[i, 9] = p.FanRpm;
                _Sheet.Cells[i, 10] = p.DriveMode;

                _Sheet.Cells[i, 11] = p.ElectricalEnergyLevel;
                _Sheet.Cells[i, 12] = p.MotorPower;
                _Sheet.Cells[i, 13] = p.PowerSource;
                _Sheet.Cells[i, 14] = p.ElectricalRpm;
                _Sheet.Cells[i, 15] = p.IsDoubleSpeed;
                _Sheet.Cells[i, 16] = p.IsFrequency;
                _Sheet.Cells[i, 17] = p.WS;
                _Sheet.Cells[i, 18] = p.IsFirefighting;


                _Sheet.Cells[i, 19] = p.dB;
                _Sheet.Cells[i, 20] = p.Weight;
                _Sheet.Cells[i, 21] = p.Length;
                _Sheet.Cells[i, 22] = p.Width;
                _Sheet.Cells[i, 23] = p.Height;



                _Sheet.Cells[i, 24] = p.VibrationMode;
                _Sheet.Cells[i, 25] = p.Amount;
                _Sheet.Cells[i, 26] = p.Remark;

                i++;
            });

            SaveFileDialog _SaveFileDialog = new SaveFileDialog();
            _SaveFileDialog.Filter = "Xlsx Files(*.xlsx)|*.xlsx";
            _SaveFileDialog.RestoreDirectory = true;
            _SaveFileDialog.FileName = "风机参数表 - " + DateTime.Now.ToString("yyyy.MM.dd HH.mm");
            var DialogResult = _SaveFileDialog.ShowDialog();
            if (DialogResult == DialogResult.OK)
            {
                TreeList.PostEditor();
                var _FilePath = _SaveFileDialog.FileName.ToString();

                _WorkBook.SaveAs(_FilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                   Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                ClosePro(_ExclApp, _WorkBook);
            }



        }

        public void ClosePro(Microsoft.Office.Interop.Excel.Application _ExclApp, Microsoft.Office.Interop.Excel.Workbook _WorkBook)
        {
            if (_WorkBook != null)
                _WorkBook.Close(true, Type.Missing, Type.Missing);
            _ExclApp.Quit();
            System.GC.GetGeneration(_ExclApp);
            IntPtr _IntPtr = new IntPtr(_ExclApp.Hwnd);
            int _K = 0;
            GetWindowThreadProcessId(_IntPtr, out _K);
            System.Diagnostics.Process _Process = System.Diagnostics.Process.GetProcessById(_K);
            _Process.Kill();
        }


        public List<ExportFanParaModel> GetListExportFanPara()
        {
            List<ExportFanParaModel> _List = new List<ExportFanParaModel>();
            m_ListFan.ForEach(p =>
           {
               if (p.FanModelName == string.Empty || p.FanModelName == "无此风机") { return; }
               var _FanPrefixDict = PubVar.g_ListFanPrefixDict.Find(s => s.FanUse == p.Scenario);
               if (_FanPrefixDict == null) return;
               ExportFanParaModel _ExportFanPara = new ExportFanParaModel();
               _ExportFanPara.ID = p.ID;
               _ExportFanPara.SortScenario = _FanPrefixDict.No;
               _ExportFanPara.SortID = p.SortID;
               _ExportFanPara.No = _FanPrefixDict.Prefix + p.InstallFloor + p.VentNum;
               _ExportFanPara.Coverage = p.Name;
               _ExportFanPara.FanForm = p.VentStyle;
               //_ExportFanPara.CalcAirVolume = FuncStr.NullToStr(p.AirVolume);
               _ExportFanPara.CalcAirVolume = string.Empty;
               _ExportFanPara.FanEnergyLevel = p.VentLev;
               _ExportFanPara.DriveMode = p.VentConnect;
               _ExportFanPara.ElectricalEnergyLevel = p.EleLev;
               _ExportFanPara.MotorPower = string.Empty;
               _ExportFanPara.PowerSource = "380-3-50";
               _ExportFanPara.ElectricalRpm = FuncStr.NullToStr(p.MotorTempo);
               _ExportFanPara.IsDoubleSpeed = p.Control;
               _ExportFanPara.IsFrequency = p.IsFre ? "是" : "否";
               _ExportFanPara.WS = string.Empty;
               _ExportFanPara.IsFirefighting = p.PowerType == "消防" ? "Y" : "N";
               _ExportFanPara.VibrationMode = "R";
               _ExportFanPara.Amount = FuncStr.NullToStr(p.VentQuan);
               _ExportFanPara.Remark = p.Remark;
               if (FuncStr.NullToStr(p.VentStyle) == "轴流")
               {
                   var _FanParameters = m_ListAxialFanParameters.Find(s => s.No == FuncStr.NullToStr(p.FanModelID) && s.ModelNum == p.FanModelName);
                   if (_FanParameters == null) return;
                   _ExportFanPara.FanDelivery = FuncStr.NullToStr(p.AirVolume);
                   _ExportFanPara.Pa = FuncStr.NullToStr(p.WindResis);
                   _ExportFanPara.FanEfficiency = string.Empty;
                   _ExportFanPara.FanRpm = _FanParameters.Rpm;
                   _ExportFanPara.dB = _FanParameters.Noise;
                   _ExportFanPara.Weight = _FanParameters.Weight;
                   _ExportFanPara.Length = _FanParameters.Length;
                   _ExportFanPara.Width = _FanParameters.Diameter;
                   _ExportFanPara.Height = string.Empty;
               }
               else
               {
                   var _FanParameters = m_ListFanParameters.Find(s => s.Suffix == FuncStr.NullToStr(p.FanModelID) && s.CCCF_Spec == p.FanModelName);
                   if (_FanParameters == null) return;
                   _ExportFanPara.FanDelivery = FuncStr.NullToStr(p.AirVolume);
                   _ExportFanPara.Pa = FuncStr.NullToStr(p.WindResis);
                   _ExportFanPara.FanEfficiency = string.Empty;
                   _ExportFanPara.FanRpm = _FanParameters.Rpm;
                   _ExportFanPara.dB = _FanParameters.Noise;
                   _ExportFanPara.Weight = _FanParameters.Weight;
                   _ExportFanPara.Length = _FanParameters.Length;
                   _ExportFanPara.Width = _FanParameters.Weight;
                   _ExportFanPara.Height = _FanParameters.Height;
               }

               _List.Add(_ExportFanPara);
           });
            return _List;
        }

        private void BarBtnExportFanCalc_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string _ImportExcelPath = m_Path + "FanCalc.xlsx";
            Microsoft.Office.Interop.Excel.Application _ExclApp = new Microsoft.Office.Interop.Excel.Application();
            _ExclApp.DisplayAlerts = false;
            _ExclApp.Visible = false;
            _ExclApp.ScreenUpdating = false;
            Microsoft.Office.Interop.Excel.Workbook _WorkBook = _ExclApp.Workbooks.Open(_ImportExcelPath, System.Type.Missing, System.Type.Missing, System.Type.Missing,
              System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
            System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);

            Microsoft.Office.Interop.Excel.Worksheet _Sheet = _WorkBook.Worksheets[1];

            var _List = m_ListFan;
            if (_List != null && _List.Count > 0) _List = _List.OrderBy(p => p.SortScenario).OrderBy(p => p.SortID).ToList();

            var i = 4;
            _List.ForEach(p =>
            {
                if (p.FanModelName == string.Empty || p.FanModelName == "无此风机") { return; }
                var _FanPrefixDict = PubVar.g_ListFanPrefixDict.Find(s => s.FanUse == p.Scenario);
                if (_FanPrefixDict == null) return;
                _Sheet.Cells[i, 1] = _FanPrefixDict.Prefix + p.InstallFloor + p.VentNum;
                _Sheet.Cells[i, 2] = p.Name;
                _Sheet.Cells[i, 3] = p.Scenario;
                _Sheet.Cells[i, 14] = p.AirVolume;
                _Sheet.Cells[i, 15] = p.DuctLength;

                _Sheet.Cells[i, 16] = p.Friction;
                _Sheet.Cells[i, 17] = p.LocRes;

                _Sheet.Cells[i, 18] = p.DuctResistance;

                _Sheet.Cells[i, 19] = p.Damper;

                _Sheet.Cells[i, 20] = p.DynPress;
                _Sheet.Cells[i, 21] = p.WindResis;

                if (FuncStr.NullToStr(p.VentStyle) == "轴流")
                {
                    var _FanParameters = m_ListAxialFanParameters.Find(s => s.No == FuncStr.NullToStr(p.FanModelID) && s.ModelNum == p.FanModelName);
                    if (_FanParameters == null) return;
                    _Sheet.Cells[i, 22] = _FanParameters.Pa;
                }
                else
                {
                    var _FanParameters = m_ListFanParameters.Find(s => s.Suffix == FuncStr.NullToStr(p.FanModelID) && s.CCCF_Spec == p.FanModelName);
                    if (_FanParameters == null) return;
                    _Sheet.Cells[i, 22] = _FanParameters.Pa;
                }

                _Sheet.Cells[i, 23] = string.Empty;

                i++;
            });

            SaveFileDialog _SaveFileDialog = new SaveFileDialog();
            _SaveFileDialog.Filter = "Xlsx Files(*.xlsx)|*.xlsx";
            _SaveFileDialog.RestoreDirectory = true;
            _SaveFileDialog.FileName = "风机计算书 - " + DateTime.Now.ToString("yyyy.MM.dd HH.mm");
            var DialogResult = _SaveFileDialog.ShowDialog();

            if (DialogResult == DialogResult.OK)
            {
                TreeList.PostEditor();
                var _FilePath = _SaveFileDialog.FileName.ToString();

                _WorkBook.SaveAs(_FilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                   Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                ClosePro(_ExclApp, _WorkBook);
            }


        }


        public int FindNum(int _Num, int _N)
        {
            int _Power = (int)Math.Pow(10, _N);
            return (_Num - _Num / _Power * _Power) * 10 / _Power;
        }

        private void ComBoxVentStyle_EditValueChanged(object sender, EventArgs e)
        {
            TreeList.PostEditor();
            var _Fan = TreeList.GetFocusedRow() as FanDataModel;
            if (_Fan == null) { return; }
            if (FuncStr.NullToStr(_Fan.VentStyle) == "轴流")
            {
                _Fan.VentConnect = "直连";
                _Fan.IntakeForm = "直进式";
                TreeList.Refresh();
            }
            SetFanModel();
        }

        private void barBtnNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_ListFan == null || m_ListFan.Count == 0)
            {
                NewFanList();
                return;
            }
            var _List = m_ListFan.FindAll(p => p.FanModelName != string.Empty && p.FanModelName != "无此风机");
            if (_List == null || _List.Count == 0)
            {
                NewFanList();
                return;
            }
            var _Result = XtraMessageBox.Show(" 否保存当前版本？ ", "提示", MessageBoxButtons.YesNoCancel);
            if (_Result == DialogResult.Yes)
            {
                BarBtnSave.PerformClick();
                NewFanList();
            }
            else if (_Result == DialogResult.No)
            {
                NewFanList();
            }
        }

        private void NewFanList()
        {
            m_ListFan = new List<FanDataModel>();
            TreeList.DataSource = m_ListFan;
            this.TreeList.ExpandAll();
            ComBoxScene_SelectedValueChanged(null, null);
        }

        private void PicInsertMap_Click(object sender, EventArgs e)
        {
            var _FocusedColumn = TreeList.FocusedColumn;
            var _FanDataModel = TreeList.GetFocusedRow() as FanDataModel;
            if (_FanDataModel == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(_FanDataModel.VentStyle))
            {
                return;
            }

            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            using (EditorUserInteraction UI = Active.Editor.StartUserInteraction(this))
            using (ThFanSelectionDbManager dbManager = new ThFanSelectionDbManager(Active.Database))
            {
                if (!dbManager.Contains(_FanDataModel.ID))
                {
                    // 场景1：若检测到图纸中没有对应的风机图块
                    //  插入新的图块
                    ThFanSelectionEngine.InsertModels(_FanDataModel);
                }
                else if (dbManager.Models[_FanDataModel.ID].Count != _FanDataModel.VentQuan)
                {
                    // 场景2：若检测到图纸中有对应的风机图块，但图块数量不同
                    //  删除原图块，插入新的图块
                    ThFanSelectionEngine.RemoveModels(_FanDataModel);
                    ThFanSelectionEngine.InsertModels(_FanDataModel);
                }
                else if (dbManager.Models[_FanDataModel.ID].Count == _FanDataModel.VentQuan)
                {
                    // 场景3：若检测到图纸中有对应的风机图块，且图块数量相同
                    var models = dbManager.GetModels(_FanDataModel.ID);
                    var blockReference = new ThFSBlockReference(models[0]);
                    var attributes = new Dictionary<string, string>(blockReference.Attributes);
                    if (_FanDataModel.IsModified(attributes))
                    {
                        // 场景3.1：图块参数变化
                        //  更新参数和编号
                        ThFanSelectionEngine.ModifyModels(_FanDataModel);
                    }
                    else
                    {
                        // 场景3.2：图块参数没有变化
                        //  缩放到到对应的图块
                        ThFanSelectionEngine.ZoomToModels(_FanDataModel);
                    }
                }
            }
        }
    }
}
