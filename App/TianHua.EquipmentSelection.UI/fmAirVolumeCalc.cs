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
    public partial class fmAirVolumeCalc : DevExpress.XtraEditors.XtraForm
    {
        public List<FanDataModel> m_ListFan { get; set; }

        public FanDataModel m_Fan { get; set; }

        public fmAirVolumeCalc()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {

        }

        public void InitForm(FanDataModel _FanDataModel)
        {
            var _Json = FuncJson.Serialize(_FanDataModel);
            m_Fan = FuncJson.Deserialize<FanDataModel>(_Json);



            //m_Fan = _FanDataModel;
            if (m_ListFan == null) m_ListFan = new List<FanDataModel>();
            m_ListFan.Add(m_Fan);

            Gdc.DataSource = m_ListFan;
            Gdv.RefreshData();
            if (m_Fan.Scenario == "消防加压送风")
            {
                this.TxtAirCalcValue.ContextImageOptions.SvgImage = Properties.Resources.计算器;
            }
            else
            {
                this.TxtAirCalcValue.ContextImageOptions.SvgImage = null;
            }
        }

        private void Gdv_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var _Fan = Gdv.GetFocusedRow() as FanDataModel;

            if (_Fan == null) { return; }

            if (e.Column.FieldName == "AirCalcValue")
            {

            }

            if (e.Column.FieldName == "AirCalcFactor")
            {
                if (_Fan.ScenarioType == 1)
                {
                    if (_Fan.AirCalcFactor < 1.2)
                    {
                        _Fan.AirCalcFactor = 1.2;
                    }
                }
                else
                {
                    if (_Fan.AirCalcFactor < 1.1)
                    {
                        _Fan.AirCalcFactor = 1.1;
                    }
                }

            }

            var _Value = _Fan.AirCalcValue * _Fan.AirCalcFactor;

            var _Rem = FuncStr.NullToInt(_Value) % 50;

            if (_Rem != 0)
            {
                var _UnitsDigit = FindNum(FuncStr.NullToInt(_Value), 1);

                var _TensDigit = FindNum(FuncStr.NullToInt(_Value), 2);

                var _Tmp = FuncStr.NullToInt(_TensDigit.ToString() + _UnitsDigit.ToString());

                if (_Tmp < 50)
                    _Fan.AirVolume = FuncStr.NullToInt(FuncStr.NullToStr(_Value).Replace(FuncStr.NullToStr(_Tmp), "50"));
                else
                {
                    var _DifferenceValue = 100 - _Tmp;
                    _Fan.AirVolume = FuncStr.NullToInt(_Value) + _DifferenceValue;
                }
            }
            else
            {
                _Fan.AirVolume = FuncStr.NullToInt(_Value);
            }


        }

        public int FindNum(int _Num, int _N)
        {
            int _Power = (int)Math.Pow(10, _N);
            return (_Num - _Num / _Power * _Power) * 10 / _Power;
        }

        private void TxtAirCalcValue_Click(object sender, EventArgs e)
        {
            if (m_Fan.Scenario != "消防加压送风") { return; }
            //TODO:调用陈越窗体.
        }
    }
}
