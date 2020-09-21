﻿using AcHelper;
using DevExpress.XtraEditors;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.AutoCAD.Utility.ExtensionTools;
using TianHua.FanSelection.ExcelExport;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection.UI
{
    public partial class fmOverView : DevExpress.XtraEditors.XtraForm
    {
        public List<FanDataModel> m_ListFan { get; set; }

        public List<FanDataModel> m_ListFanRoot = new List<FanDataModel>();

        //public List<FanDataModel> m_ListMainFan { get; set; }


        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        /// <summary>
        /// 风机箱参数
        /// </summary>
        public List<FanParameters> m_ListFanParameters = new List<FanParameters>();


        /// <summary>
        /// 风机箱参数-单速
        /// </summary>
        public List<FanParameters> m_ListFanParametersSingle = new List<FanParameters>();


        /// <summary>
        /// 风机箱参数 双速
        /// </summary>
        public List<FanParameters> m_ListFanParametersDouble = new List<FanParameters>();


        /// <summary>
        /// 轴流风机参数
        /// </summary>
        public List<AxialFanParameters> m_ListAxialFanParameters = new List<AxialFanParameters>();

        /// <summary>
        /// 轴流风机参数 双速
        /// </summary>
        public List<AxialFanParameters> m_ListAxialFanParametersDouble = new List<AxialFanParameters>();

        public fmOverView()
        {
            InitializeComponent();

            foreach (Control _Ctrl in this.layoutControl2.Controls)
            {
                if (_Ctrl is CheckEdit)
                {
                    var _Edit = _Ctrl as CheckEdit;
                    if (_Edit.Name == "CheckAll") continue;
                    _Edit.CheckedChanged += Check_CheckedChanged;

                    _Edit.EditValueChanged += _Edit_EditValueChanged; ;
                }
            }

        }


        public void Init(List<FanDataModel> _ListFan, List<FanParameters> _ListFanParameters, List<FanParameters> _ListFanParametersSingle,
           List<FanParameters> _ListFanParametersDouble, List<AxialFanParameters> _ListAxialFanParameters, List<AxialFanParameters> _ListAxialFanParametersDouble)
        {
            //m_ListMainFan = _ListFan;
            var _Json = FuncJson.Serialize(_ListFan);
            m_ListFan = FuncJson.Deserialize<List<FanDataModel>>(_Json);

            m_ListFanParameters = _ListFanParameters;
            m_ListFanParametersSingle = _ListFanParametersSingle;
            m_ListFanParametersDouble = _ListFanParametersDouble;
            m_ListAxialFanParameters = _ListAxialFanParameters;
            m_ListAxialFanParametersDouble = _ListAxialFanParametersDouble;

        }




        private void _Edit_EditValueChanged(object sender, EventArgs e)
        {
            string _FilterString = FilterOverView();

            TreeList.ActiveFilterString = _FilterString;
        }


        private void Check_CheckedChanged(object sender, EventArgs e)
        {
            CheckEdit _CheckEdit = sender as CheckEdit;
            if (_CheckEdit.Checked == true)
            {
                foreach (Control _Ctrl in this.layoutControl2.Controls)
                {
                    if (_Ctrl is CheckEdit)
                    {
                        var _Edit = _Ctrl as CheckEdit;
                        if (_Edit.Name != "CheckAll" && _Edit.Checked == false)
                            return;
                    }
                }
                this.CheckAll.CheckedChanged -= new System.EventHandler(this.CheckAll_CheckedChanged);
                CheckAll.Checked = true;
                this.CheckAll.CheckedChanged += new System.EventHandler(this.CheckAll_CheckedChanged);
            }
            else
            {
                this.CheckAll.CheckedChanged -= new System.EventHandler(this.CheckAll_CheckedChanged);
                CheckAll.EditValue = false;
                this.CheckAll.CheckedChanged += new System.EventHandler(this.CheckAll_CheckedChanged);
            }





        }

        private string FilterOverView()
        {
            List<string> _List = new List<string>();
            foreach (Control _Ctrl in this.layoutControl2.Controls)
            {
                if (_Ctrl is CheckEdit)
                {
                    var _Edit = _Ctrl as CheckEdit;
                    if (_Edit.Checked)
                        _List.Add(_Edit.Text);
                }
            }

            var _FilterString = string.Empty;

            if (_List != null && _List.Count > 0)
            {
                for (int i = 0; i < _List.Count; i++)
                {
                    if (i == 0)
                        _FilterString = @"  Scenario =  '" + FuncStr.NullToStr(_List[i]) + "'";
                    else
                        _FilterString += @" OR Scenario =  '" + FuncStr.NullToStr(_List[i]) + "'";
                }

            }

            if (_FilterString == string.Empty) { _FilterString = " 1 <> 1 "; }

            return _FilterString;
        }

        private void fmOverView_Load(object sender, EventArgs e)
        {
            InitListFan();
        }

        private void InitListFan()
        {
            m_ListFan.ForEach(p =>
            {
                if (p.PID == "0")
                {
                    var _FanPrefix = PubVar.g_ListFanPrefixDict.Find(s => s.FanUse == p.Scenario);
                    if (_FanPrefix != null)
                        p.PID = FuncStr.NullToStr(_FanPrefix.No);
                }
            });

            m_ListFanRoot = new List<FanDataModel>();
            if (PubVar.g_ListFanPrefixDict != null && PubVar.g_ListFanPrefixDict.Count > 0)
            {
                for (int i = 0; i < PubVar.g_ListFanPrefixDict.Count; i++)
                {
                    FanDataModel _FanDataModel = new FanDataModel();
                    _FanDataModel.ID = PubVar.g_ListFanPrefixDict[i].No.ToString();
                    _FanDataModel.PID = "0";
                    _FanDataModel.SortID = PubVar.g_ListFanPrefixDict[i].No;
                    _FanDataModel.SortScenario = PubVar.g_ListFanPrefixDict[i].No;
                    _FanDataModel.Scenario = PubVar.g_ListFanPrefixDict[i].FanUse;
                    m_ListFanRoot.Add(_FanDataModel);
                }
            }

            m_ListFan.AddRange(m_ListFanRoot);


            this.TreeList.ParentFieldName = "PID";
            this.TreeList.KeyFieldName = "ID";
            if (m_ListFan != null && m_ListFan.Count > 0)
                m_ListFan = m_ListFan.OrderBy(p => p.SortID).ToList();
            TreeList.DataSource = m_ListFan;
            this.TreeList.ExpandAll();
        }

        private void TreeList_CustomColumnDisplayText(object sender, DevExpress.XtraTreeList.CustomColumnDisplayTextEventArgs e)
        {
            if (e == null || e.Node == null) { return; }

            if (e.Column.FieldName == "Scenario" || e.Column.FieldName == "OverViewFanNum")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _Fan = m_ListFan.Find(p => p.ID == _ID);
                if (_Fan == null) { return; }

                if (IsGUID(_Fan.PID))
                {
                    e.DisplayText = " - ";
                }

            }
            if (e.Column.FieldName == "VentQuan" || e.Column.FieldName == "AirVolume" || e.Column.FieldName == "WindResis")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _Fan = m_ListFan.Find(p => p.ID == _ID);
                if (_Fan == null) { return; }
                if (_Fan.PID == "0")
                    e.DisplayText = string.Empty;
            }
        }


        public bool IsGUID(string _Expression)
        {
            if (_Expression != null)
            {
                Regex _GuidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");
                return _GuidRegEx.IsMatch(_Expression);
            }
            return false;
        }

        private void CheckAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control _Ctrl in this.layoutControl2.Controls)
            {
                if (_Ctrl is CheckEdit)
                {

                    var _Edit = _Ctrl as CheckEdit;
                    _Edit.Checked = CheckAll.Checked;

                }
            }

            string _FilterString = FilterOverView();

            TreeList.ActiveFilterString = _FilterString;
        }

        private void Check_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {

        }

        private void TreeList_CustomDrawNodeCell(object sender, DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs e)
        {
            FanDataModel _Fan = TreeList.GetDataRecordByNode(e.Node) as FanDataModel;
            if (_Fan == null) return;
            if (e.Column.FieldName == "Scenario")
            {
                if (_Fan.PID == "0")
                {
                    e.Appearance.ForeColor = Color.FromArgb(27, 161, 226);
                    e.Appearance.Font = new System.Drawing.Font(e.Appearance.Font, e.Appearance.Font.Style | FontStyle.Bold);
                }
            }
        }

        private void BarBtnExportFanPara_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<string> _ListSceneScreening = GetSceneScreening();

            string _ImportExcelPath = Path.Combine(ThCADCommon.SupportPath(), "DesignData", "FanPara.xlsx");

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

            if (_ListSceneScreening != null && _ListSceneScreening.Count > 0)
            {
                _List = _List.FindAll(p => _ListSceneScreening.Contains(p.Scenario));
            }

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
                _Sheet.Cells[i, 7] = FuncStr.NullToInt(p.StaticPa);
                _Sheet.Cells[i, 8] = p.FanEnergyLevel;
                _Sheet.Cells[i, 9] = p.FanEfficiency;
                _Sheet.Cells[i, 10] = p.FanRpm;
                _Sheet.Cells[i, 11] = p.DriveMode;

                _Sheet.Cells[i, 12] = p.ElectricalEnergyLevel;
                _Sheet.Cells[i, 13] = p.MotorPower;
                _Sheet.Cells[i, 14] = p.PowerSource;
                _Sheet.Cells[i, 15] = p.ElectricalRpm;
                _Sheet.Cells[i, 16] = p.IsDoubleSpeed;
                _Sheet.Cells[i, 17] = p.IsFrequency;
                _Sheet.Cells[i, 18] = p.WS;
                _Sheet.Cells[i, 19] = p.IsFirefighting;


                _Sheet.Cells[i, 20] = p.dB;
                _Sheet.Cells[i, 21] = p.Weight;
                _Sheet.Cells[i, 22] = p.Length;
                _Sheet.Cells[i, 23] = p.Width;
                _Sheet.Cells[i, 24] = p.Height;



                _Sheet.Cells[i, 25] = p.VibrationMode;
                _Sheet.Cells[i, 26] = p.Amount;
                _Sheet.Cells[i, 27] = p.Remark;

                i++;
            });

            SaveFileDialog _SaveFileDialog = new SaveFileDialog();
            _SaveFileDialog.Filter = "Xlsx Files(*.xlsx)|*.xlsx";
            _SaveFileDialog.RestoreDirectory = true;
            _SaveFileDialog.FileName = "风机参数表 - " + DateTime.Now.ToString("yyyy.MM.dd HH.mm");
            _SaveFileDialog.InitialDirectory = Active.DocumentDirectory;
            var _DialogResult = _SaveFileDialog.ShowDialog();

            if (_DialogResult == DialogResult.OK)
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


        private List<ExportFanParaModel> GetListExportFanPara()
        {
            List<ExportFanParaModel> _List = new List<ExportFanParaModel>();
            m_ListFan.ForEach(p =>
           {
               if (p.FanModelName == string.Empty || p.FanModelName == "无此风机") { return; }
               var _FanPrefixDict = PubVar.g_ListFanPrefixDict.Find(s => s.FanUse == p.Scenario);
               if (_FanPrefixDict == null) return;
               ExportFanParaModel _ExportFanPara = new ExportFanParaModel();
               _ExportFanPara.ID = p.ID;
               _ExportFanPara.Scenario = p.Scenario;
               _ExportFanPara.SortScenario = _FanPrefixDict.No;
               _ExportFanPara.SortID = p.SortID;
               _ExportFanPara.No = p.FanNum;
               _ExportFanPara.Coverage = p.Name;
               _ExportFanPara.FanForm = p.VentStyle.Replace("(电机内置)", "").Replace("(电机外置)", "");
               //_ExportFanPara.CalcAirVolume = FuncStr.NullToStr(p.AirVolume);
               _ExportFanPara.CalcAirVolume = FuncStr.NullToStr(p.AirCalcValue);
               _ExportFanPara.FanEnergyLevel = p.VentLev;
               _ExportFanPara.DriveMode = p.VentConnect;
               _ExportFanPara.ElectricalEnergyLevel = p.EleLev;
               _ExportFanPara.MotorPower = p.FanModelMotorPower;
               _ExportFanPara.PowerSource = "380-3-50";
               _ExportFanPara.ElectricalRpm = FuncStr.NullToStr(p.MotorTempo);
               _ExportFanPara.IsDoubleSpeed = p.Control;
               _ExportFanPara.IsFrequency = p.IsFre ? "是" : "否";
               _ExportFanPara.WS = p.FanModelPower;
               _ExportFanPara.IsFirefighting = p.PowerType == "消防" ? "Y" : "N";
               _ExportFanPara.VibrationMode = p.VibrationMode;
               _ExportFanPara.Amount = FuncStr.NullToStr(p.VentQuan);
               _ExportFanPara.Remark = p.Remark;
               _ExportFanPara.FanEfficiency = p.FanInternalEfficiency;
               _ExportFanPara.StaticPa = FuncStr.NullToStr((p.DuctResistance + p.Damper) * p.SelectionFactor);
               if (FuncStr.NullToStr(p.VentStyle) == "轴流")
               {
                   List<AxialFanParameters> _ListAxialFanParameters = GetAxialFanParametersByControl(p);
                   var _FanParameters = _ListAxialFanParameters.Find(s => s.No == FuncStr.NullToStr(p.FanModelID) && s.ModelNum == p.FanModelName);
                   if (_FanParameters == null) return;
                   _ExportFanPara.FanDelivery = FuncStr.NullToStr(p.AirVolume);
                   _ExportFanPara.Pa = FuncStr.NullToStr(p.WindResis);

                   _ExportFanPara.FanRpm = _FanParameters.Rpm;
                   _ExportFanPara.dB = _FanParameters.Noise;
                   _ExportFanPara.Weight = _FanParameters.Weight;
                   _ExportFanPara.Length = _FanParameters.Length;
                   _ExportFanPara.Width = _FanParameters.Diameter;
                   _ExportFanPara.Height = string.Empty;
               }
               else
               {
                   List<FanParameters> _ListFanParameters = GetFanParametersByControl(p);
                   var _FanParameters = _ListFanParameters.Find(s => s.Suffix == FuncStr.NullToStr(p.FanModelID) && s.CCCF_Spec == p.FanModelName);
                   if (_FanParameters == null) return;
                   _ExportFanPara.FanDelivery = FuncStr.NullToStr(p.AirVolume);
                   _ExportFanPara.Pa = FuncStr.NullToStr(p.WindResis);
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

        private List<AxialFanParameters> GetAxialFanParametersByControl(FanDataModel p)
        {
            List<AxialFanParameters> _ListAxialFanParameters = new List<AxialFanParameters>();
            if (p.Control == "双速")
            {
                _ListAxialFanParameters = m_ListAxialFanParametersDouble;
            }
            else
            {
                _ListAxialFanParameters = m_ListAxialFanParameters;
            }

            return _ListAxialFanParameters;
        }

        private List<FanParameters> GetFanParametersByControl(FanDataModel p)
        {
            List<FanParameters> _ListFanParameters = new List<FanParameters>();
            if (p.Control == "双速")
            {
                _ListFanParameters = m_ListFanParametersDouble;
            }
            else
            {
                _ListFanParameters = m_ListFanParameters;
            }
            if (FuncStr.NullToStr(p.VentStyle).Contains("后倾离心"))
            {
                _ListFanParameters = m_ListFanParametersSingle;
            }
            return _ListFanParameters;
        }

        private List<string> GetSceneScreening()
        {
            List<string> _List = new List<string>();
            foreach (Control _Ctrl in this.layoutControl2.Controls)
            {
                if (_Ctrl is CheckEdit)
                {
                    var _Edit = _Ctrl as CheckEdit;
                    if (_Edit.Checked)
                        _List.Add(_Edit.Text);
                }
            }

            return _List;
        }

        private void BarBtnExportFanCalc_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<string> _ListSceneScreening = GetSceneScreening();
            ExcelFile excelfile = new ExcelFile();
            Workbook targetWorkbookb = excelfile.OpenWorkBook(Path.Combine(ThCADCommon.SupportPath(), "DesignData", "FanCalc.xlsx"));
            var sourceWorkbookb = excelfile.OpenWorkBook(Path.Combine(ThCADCommon.SupportPath(), "DesignData", "SmokeProofScenario.xlsx"));

            Worksheet _Sheet = targetWorkbookb.Worksheets[1];
            var targetsheet = targetWorkbookb.GetSheetFromSheetName("防烟计算");

            var _List = m_ListFan;

            if (_ListSceneScreening != null && _ListSceneScreening.Count > 0)
            {
                _List = _List.FindAll(p => _ListSceneScreening.Contains(p.Scenario));
            }

            if (_List != null && _List.Count > 0) _List = _List.OrderBy(p => p.SortScenario).OrderBy(p => p.SortID).ToList();

            var i = 4;
            ExcelExportEngine.Instance.File = excelfile;
            ExcelExportEngine.Instance.Sourcebook = sourceWorkbookb;
            ExcelExportEngine.Instance.Targetsheet = targetsheet;
            _List.ForEach(p =>
            {
                if (p.FanModelName == string.Empty || p.FanModelName == "无此风机") { return; }
                var _FanPrefixDict = PubVar.g_ListFanPrefixDict.Find(s => s.FanUse == p.Scenario);
                if (_FanPrefixDict == null) return;
                if (p.PID != "0") { return; }
                _Sheet.Cells[i, 1] = p.FanNum;
                _Sheet.Cells[i, 2] = p.Name;
                _Sheet.Cells[i, 3] = p.Scenario;
                _Sheet.Cells[i, 13] = p.AirCalcValue;
                _Sheet.Cells[i, 14] = p.AirVolume;
                _Sheet.Cells[i, 15] = p.DuctLength;

                _Sheet.Cells[i, 16] = p.Friction;
                _Sheet.Cells[i, 17] = p.LocRes;

                _Sheet.Cells[i, 18] = p.DuctResistance;

                _Sheet.Cells[i, 19] = p.Damper;
                _Sheet.Cells[i, 20] = p.EndReservedAirPressure;
                _Sheet.Cells[i, 21] = p.DynPress;


                _Sheet.Cells[i, 22] = p.CalcResistance;
                _Sheet.Cells[i, 23] = p.WindResis;

                _Sheet.Cells[i, 24] = p.FanModelPower;

                var model = p.FanVolumeModel;
                if (!model.IsNull())
                {
                    ExcelExportEngine.Instance.Model = p;
                    ExcelExportEngine.Instance.Run();
                }

                i++;
            });

            SaveFileDialog _SaveFileDialog = new SaveFileDialog();
            _SaveFileDialog.Filter = "Xlsx Files(*.xlsx)|*.xlsx";
            _SaveFileDialog.RestoreDirectory = true;
            _SaveFileDialog.InitialDirectory = Active.DocumentDirectory;
            _SaveFileDialog.FileName = "风机计算书 - " + DateTime.Now.ToString("yyyy.MM.dd HH.mm");
            var DialogResult = _SaveFileDialog.ShowDialog();

            if (DialogResult == DialogResult.OK)
            {
                TreeList.PostEditor();
                var _FilePath = _SaveFileDialog.FileName.ToString();

                targetWorkbookb.SaveAs(_FilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                   Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                ClosePro(excelfile.ExcelApp, targetWorkbookb);
            }
        }



        public void DataSourceChanged(List<FanDataModel> _List)
        {
            if (_List == null || _List.Count == 0) { return; }
            var _Json = FuncJson.Serialize(_List);
            m_ListFan = FuncJson.Deserialize<List<FanDataModel>>(_Json);
            InitListFan();
        }
    }
}