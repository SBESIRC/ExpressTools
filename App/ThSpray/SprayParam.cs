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
        public PlaceData placeData = new PlaceData();
        public SprayParam()
        {
            InitializeComponent();
            comboSprayType.SelectedIndex = 0;
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
    }
}
