using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThSpray
{
    public partial class SprayParam : Form
    {
        public static PlaceData placeData = new PlaceData();
        public SprayParam()
        {
            InitializeComponent();
            comboSprayType.SelectedIndex = 0;
            InitParamDialog();
        }

        private void InitParamDialog()
        {
            // 喷头间距
            txtSprayMin.Text = placeData.minSprayGap.ToString();
            txtSprayMax.Text = placeData.maxSprayGap.ToString();

            txtWallMin.Text = placeData.minWallGap.ToString();
            txtWallMax.Text = placeData.maxWallGap.ToString();
            txtBeamMin.Text = placeData.minBeamGap.ToString();
            txtBeamMax.Text = placeData.maxBeamGap.ToString();

            if (placeData.putType == PutType.PICKPOINT)
            {
                btnPickPoint.Select(); // 选中按钮
                lblPutWay.Text = "点选布置";
            }
            else if (placeData.putType == PutType.CHOOSECURVE)
            {
                btnChooseCurve.Select();
                lblPutWay.Text = "选线布置";
            }
            else
            {
                btnDrawCurve.Select();
                lblPutWay.Text = "绘线布置";
            }

            if (placeData.type == SprayType.SPRAYUP)
            {
                comboSprayType.SelectedIndex = 0;
            }
            else
            {
                comboSprayType.SelectedIndex = 1;
            }
        }

        private void btnPlace_Click(object sender, EventArgs e)
        {
            double value = 0;
            // 喷头间距最小和最大值
            double.TryParse(txtSprayMin.Text, out value);
            placeData.minSprayGap = value;

            double.TryParse(txtSprayMax.Text, out value);
            placeData.maxSprayGap = value;

            // 距墙距离最小最大值
            double.TryParse(txtWallMin.Text, out value);
            placeData.minWallGap = value;

            double.TryParse(txtWallMax.Text, out value);
            placeData.maxWallGap = value;
            // 距梁最小距离最大值
            double.TryParse(txtBeamMin.Text, out value);
            placeData.minBeamGap = value;

            double.TryParse(txtBeamMax.Text, out value);
            placeData.maxBeamGap = value;

            // 喷头类型
            var spraySelectIndex = comboSprayType.SelectedIndex;
            if (spraySelectIndex == 0)
            {
                placeData.type = SprayType.SPRAYUP;
            }
            else if (spraySelectIndex == 1)
            {
                placeData.type = SprayType.SPRAYDOWN;
            }

            // 布置类型
            if (lblPutWay.Text == "点选布置")
            {
                placeData.putType = PutType.PICKPOINT;
            }
            else if (lblPutWay.Text == "选线布置")
            {
                placeData.putType = PutType.CHOOSECURVE;
            }
            else if (lblPutWay.Text == "绘线布置")
            {
                placeData.putType = PutType.DRAWCURVE;
            }
        }

        private void btnPickPoint_Click(object sender, EventArgs e)
        {
            lblPutWay.Text = "点选布置";
        }

        private void btnChooseCurve_Click(object sender, EventArgs e)
        {
            lblPutWay.Text = "选线布置";
        }

        private void btnDrawCurve_Click(object sender, EventArgs e)
        {
            lblPutWay.Text = "绘线布置";
        }

        private void txtSprayMin_Leave(object sender, EventArgs e)
        {
            double sprayMinValue = 0;

            // 喷头间距最小和最大值
            double.TryParse(txtSprayMin.Text, out sprayMinValue);

            double sprayMaxValue = 0;
            double.TryParse(txtSprayMax.Text, out sprayMaxValue);

            if (sprayMinValue >= 1800 && sprayMinValue <= 4400  && sprayMinValue <= sprayMaxValue)
            {
                return;
            }

            MessageBox.Show("喷头间距最小值[1800, 4400],最小值≤最大值", "参数提示", MessageBoxButtons.OK);
            txtSprayMin.Text = placeData.minSprayGap.ToString();
        }

        private void txtSprayMax_Leave(object sender, EventArgs e)
        {
            double sprayMinValue = 0;

            // 喷头间距最小和最大值
            double.TryParse(txtSprayMin.Text, out sprayMinValue);

            double sprayMaxValue = 0;
            double.TryParse(txtSprayMax.Text, out sprayMaxValue);

            if (sprayMaxValue <= 4400 && sprayMinValue <= sprayMaxValue)
            {
                return;
            }

            MessageBox.Show("最大值≤4400，最小值≤最大值", "参数设置提示", MessageBoxButtons.OK);
            txtSprayMax.Text = placeData.maxSprayGap.ToString();
        }

        private void txtWallMin_Leave(object sender, EventArgs e)
        {
            double wallMinValue = 0;

            // 距墙距离最小最大值
            double.TryParse(txtWallMin.Text, out wallMinValue);

            double wallMaxValue = 0;
            double.TryParse(txtWallMax.Text, out wallMaxValue);

            if (wallMinValue >= 100 && wallMinValue  <= 2200 && wallMinValue <= wallMaxValue)
            {
                return;
            }

            MessageBox.Show("喷头距墙最小值[100，2200]，最小值≤最大值", "参数设置提示", MessageBoxButtons.OK);
            txtWallMin.Text = placeData.minWallGap.ToString();
        }

        private void txtWallMax_Leave(object sender, EventArgs e)
        {
            double wallMinValue = 0;

            // 距墙距离最小最大值
            double.TryParse(txtWallMin.Text, out wallMinValue);

            double wallMaxValue = 0;
            double.TryParse(txtWallMax.Text, out wallMaxValue);

            if (wallMaxValue <= 2200 && wallMinValue <= wallMaxValue)
            {
                return;
            }

            MessageBox.Show("最大值≤2200，最小值≤最大值", "参数设置提示", MessageBoxButtons.OK);
            txtWallMax.Text = placeData.maxWallGap.ToString();
        }

        private void txtBeamMin_Leave(object sender, EventArgs e)
        {
            double beamMinValue = 0;

            // 距梁距离最小最大值
            double.TryParse(txtBeamMin.Text, out beamMinValue);

            double beamMaxValue = 0;
            double.TryParse(txtBeamMax.Text, out beamMaxValue);

            if (beamMinValue >= 100 && beamMinValue <= 2200 && beamMinValue <= beamMaxValue)
            {
                return;
            }

            MessageBox.Show("喷头距梁最小值[100，2200],最小值≤最大值。", "参数设置提示", MessageBoxButtons.OK);
            txtBeamMin.Text = placeData.minBeamGap.ToString();
        }

        private void txtBeamMax_Leave(object sender, EventArgs e)
        {
            double beamMinValue = 0;

            // 距梁距离最小最大值
            double.TryParse(txtBeamMin.Text, out beamMinValue);

            double beamMaxValue = 0;
            double.TryParse(txtBeamMax.Text, out beamMaxValue);

            if (beamMaxValue <= 2200 && beamMinValue <= beamMaxValue)
            {
                return;
            }

            MessageBox.Show("最大值≤2200,最小值≤最大值。", "参数设置提示", MessageBoxButtons.OK);
            txtBeamMax.Text = placeData.maxBeamGap.ToString();
        }
    }
}
