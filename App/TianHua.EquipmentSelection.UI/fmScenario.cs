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
    public partial class fmScenario : DevExpress.XtraEditors.XtraForm
    {
        public FanDataModel m_Fan { get; set; }

        public string m_ScenarioType { get; set; }

        public fmScenario()
        {
            InitializeComponent();
        }

        private void TxtCalcValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        public void InitForm(FanDataModel _FanDataModel)
        {
            var _Json = FuncJson.Serialize(_FanDataModel);

            m_Fan = FuncJson.Deserialize<FanDataModel>(_Json);

            if (!m_Fan.ExhaustModel.IsNull())
            {
                TxtCalcValue.Text = GetTxtCalcValue();
                switch (m_Fan.ExhaustModel.ExhaustCalcType)
                {
                    case "空间-净高小于等于6m":
                        this.RadLessThan.Checked = true;
                        break;
                    case "空间-净高大于6m":
                        this.RadGreaterThan6.Checked = true;
                        break;
                    case "空间-汽车库":
                        this.RadGarage.Checked = true;
                        break;
                    case "走道回廊-仅走道或回廊设置排烟":
                        this.RadCloister.Checked = true;
                        break;
                    case "走道回廊-房间内和走道或回廊都设置排烟":
                        this.RadCloistersAndRooms.Checked = true;
                        break;
                    case "中庭-周围场所设有排烟系统":
                        this.RadSmokeExtraction.Checked = true;
                        break;
                    case "中庭-周围场所不设排烟系统":
                        this.RadNoSmokeExtraction.Checked = true;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                TxtCalcValue.Text = "无";
            }
        }

        private void BtnCalc_Click(object sender, EventArgs e)
        {
            fmExhaustCalc _fmExhaustCalc = new fmExhaustCalc();
            if (m_Fan.ExhaustModel.IsNull() || m_Fan.ExhaustModel.ExhaustCalcType != m_ScenarioType)
            {
                m_Fan.ExhaustModel = new Model.ExhaustCalcModel();
                m_Fan.ExhaustModel.SpatialTypes = "办公室、学校、客厅、走道";
            }
            _fmExhaustCalc.InitForm(m_Fan, m_ScenarioType);
            if (_fmExhaustCalc.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            m_Fan.ExhaustModel = _fmExhaustCalc.m_Fan.ExhaustModel;
            if (!m_Fan.ExhaustModel.IsNull())
            {
                TxtCalcValue.Text = GetTxtCalcValue();
            }
            else
            {
                TxtCalcValue.Text = "无";
            }
        }

        private void Rad_CheckedChanged(object sender, EventArgs e)
        {
            var _Rad = sender as RadioButton;
            if (_Rad == null) { return; }
            m_ScenarioType = _Rad.Text;
        }

        private string GetTxtCalcValue()
        {
            if (m_Fan.ExhaustModel.Final_CalcAirVolum == "无" || m_Fan.ExhaustModel.MaxSmokeExtraction == "无")
            {
                return "无";
            }
            else
            {
                return TxtCalcValue.Text = FuncStr.NullToStr(Math.Max(m_Fan.ExhaustModel.Final_CalcAirVolum.NullToDouble(), m_Fan.ExhaustModel.MaxSmokeExtraction.NullToDouble()));
            }
        }
    }
}
