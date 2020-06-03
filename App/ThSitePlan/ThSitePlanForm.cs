using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThSitePlan.Engine;

namespace ThSitePlan
{
    public partial class ThSitePlanForm : Form
    {
        private ToolTip HelpToolTip = new ToolTip
        {
            AutoPopDelay = 10000,
            ShowAlways = true,
            Active = true
        };


        public ThSitePlanForm()
        {
            InitializeComponent();

            this.ShadowAngleSetBox.DataBindings.Add("Text", Properties.Settings.Default, "shadowAngle", false, DataSourceUpdateMode.OnValidation);
            this.ShadowLengthSetBox.DataBindings.Add("Text", Properties.Settings.Default, "shadowLengthScale", false, DataSourceUpdateMode.OnValidation);
            this.TreeRadiusSetBox.DataBindings.Add("Text", Properties.Settings.Default, "PlantRadius", false, DataSourceUpdateMode.OnValidation);
            this.TreeDensitySetBox.DataBindings.Add("Text", Properties.Settings.Default, "PlantDensity", false, DataSourceUpdateMode.OnValidation);
        }

        private void CancelBt_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
        }

        private void ThSitePlanForm_Load(object sender, EventArgs e)
        {

        }

        private void ConfirmBt_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.shadowAngle = Convert.ToDouble(this.ShadowAngleSetBox.Text);
            Properties.Settings.Default.shadowLengthScale = Convert.ToDouble(this.ShadowLengthSetBox.Text);
            Properties.Settings.Default.PlantRadius = Convert.ToDouble(this.TreeRadiusSetBox.Text);
            Properties.Settings.Default.PlantDensity = Convert.ToInt32(this.TreeDensitySetBox.Text);

            Properties.Settings.Default.Save();
            this.Close();
        }

        private void ShadowUpdBt_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.shadowAngle = Convert.ToDouble(this.ShadowAngleSetBox.Text);
            Properties.Settings.Default.shadowLengthScale = Convert.ToDouble(this.ShadowLengthSetBox.Text);
            Properties.Settings.Default.Save();

            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                ObjectId frame = ThSitePlanDbEngine.Instance.FrameByName("全局阴影");
                ObjectId[] frames = new ObjectId[] { frame };

                Active.Editor.SetImpliedSelection(frames);
                string commandst = "_.THSPUPD";
                Active.Document.SendStringToExecute($"{commandst} ", true, false, false);
            }
        }

        private void LandTreeUpdBt_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PlantRadius = Convert.ToDouble(this.TreeRadiusSetBox.Text);
            Properties.Settings.Default.PlantDensity = Convert.ToInt32(this.TreeDensitySetBox.Text);
            Properties.Settings.Default.Save();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                ObjectId frame = ThSitePlanDbEngine.Instance.FrameByName("树木-行道树-树木色块");
                ObjectId[] frames = new ObjectId[] { frame };

                Active.Editor.SetImpliedSelection(frames);
                string commandst = "_.THSPUPD";
                Active.Document.SendStringToExecute($"{commandst} ",true,false,false);
            }
        }

        private void ShadowAngleHelp(object sender, EventArgs e)
        {
            HelpToolTip.SetToolTip(ShadowAngleSetBox, "阴影角度沿X轴正方向逆时针");
        }

        private void ShadowLengthHelp(object sender, EventArgs e)
        {
            HelpToolTip.SetToolTip(ShadowLengthSetBox, "阴影长度系数");
        }

        private void TreeRadiusHelp(object sender, EventArgs e)
        {
            HelpToolTip.SetToolTip(TreeRadiusSetBox, "树木半径(mm)");
        }

        private void TreeDensityHelp(object sender, EventArgs e)
        {
            HelpToolTip.SetToolTip(TreeDensitySetBox, "树木密度");
        }

    }
}
