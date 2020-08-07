using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection.UI
{
    public partial class fmDragCalc : DevExpress.XtraEditors.XtraForm
    {

        public List<FanDataModel> m_ListFan = new List<FanDataModel>();

        public FanDataModel m_Fan { get; set; }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public fmDragCalc()
        {
            InitializeComponent();
        }

        private void fmDragCalc_Load(object sender, EventArgs e)
        {
            ;
        }

        public void InitForm(FanDataModel _FanDataModel)
        {
            //var _Json = FuncJson.Serialize(_FanDataModel);
            //m_Fan = FuncJson.Deserialize<FanDataModel>(_Json);
            m_Fan = _FanDataModel;
            m_ListFan = new List<FanDataModel>();
            m_ListFan.Add(m_Fan);
            Gdc.DataSource = m_ListFan;
            Gdc.Refresh();

        }

        private void Gdv_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (FuncStr.NullToDouble(e.Value) == 0)
                e.DisplayText = string.Empty;
            //if (e.Column.FieldName == "DuctResistance")
            //{
            //    if (m_ListFan == null || m_ListFan.Count == 0) { return; }
            //    var _Fan = m_ListFan.First();
            //    if (_Fan.DuctLength > 0 && _Fan.Friction > 0 && _Fan.LocRes > 0)
            //    {

            //        _Fan.DuctResistance = FuncStr.NullToInt(_Fan.DuctLength * _Fan.Friction * (1 + _Fan.LocRes));

            //        Gdv.RefreshData();
            //    }
            //}

            //if (e.Column.FieldName == "WindResis")
            //{
            //    if (m_ListFan == null || m_ListFan.Count == 0) { return; }
            //    var _Fan = m_ListFan.First();
            //    if (_Fan.DuctResistance > 0 && _Fan.Damper > 0 && _Fan.DynPress > 0)
            //    {

            //        _Fan.WindResis = FuncStr.NullToInt((_Fan.DuctResistance + _Fan.Damper + _Fan.DynPress) * 1.1);

            //        Gdv.RefreshData();
            //    }
            //}
        }

        private void Gdv_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (m_ListFan == null || m_ListFan.Count == 0) { return; }
            var _Fan = m_ListFan.First();
            if (_Fan.DuctLength > 0 && _Fan.Friction > 0 && _Fan.LocRes > 0)
            {

                _Fan.DuctResistance = FuncStr.NullToInt(_Fan.DuctLength * _Fan.Friction * (1 + _Fan.LocRes));

                Gdv.RefreshData();
            }
            if (_Fan.DuctResistance > 0 && _Fan.Damper > 0 && _Fan.DynPress > 0)
            {
                var _Tmp = FuncStr.NullToInt((_Fan.DuctResistance + _Fan.Damper + _Fan.DynPress) * _Fan.SelectionFactor);
                _Fan.CalcResistance = _Fan.DuctResistance + _Fan.Damper + _Fan.DynPress;
                var _UnitsDigit = FindNum(_Tmp, 1);
                if (_UnitsDigit != 0)
                {
                    _Tmp = _Tmp + 10 - _UnitsDigit;
                }
                _Fan.WindResis = _Tmp;
                Gdv.RefreshData();
            }
        }

        public int FindNum(int _Num, int _N)
        {
            int _Power = (int)Math.Pow(10, _N);
            return (_Num - _Num / _Power * _Power) * 10 / _Power;
        }
    }
}
