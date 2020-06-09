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
using ThSitePlan.Configuration;
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
                //获取scriptid为阴影的所有ThSitePlanConfigItem
                ThSitePlanConfigService.Instance.Initialize();
                List<ThSitePlanConfigItem> shadowconfigItem = new List<ThSitePlanConfigItem>();
                ThSitePlanConfigService.Instance.FindItemsByCADScript(ThSitePlanConfigService.Instance.Root,  "3", ref shadowconfigItem);
                ThSitePlanConfigService.Instance.FindItemsByCADScript(ThSitePlanConfigService.Instance.Root, "4", ref shadowconfigItem);

                //获取所有阴影图框
                List<ObjectId> frames = new List<ObjectId>();
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                foreach (var item in shadowconfigItem)
                {
                    ObjectId frame = ThSitePlanDbEngine.Instance.FrameByName(item.Properties["Name"].ToString());
                    frames.Add(frame);
                }
                
                //创建对阴影图框的选择集，执行update命令
                Active.Editor.SetImpliedSelection(frames.ToArray());
                string commandst = "_.THPOPUD";
                Active.Document.SendStringToExecute($"{commandst} ", true, false, false);

                Properties.Settings.Default.Save();
                this.Close();
            }
        }

        private void LandTreeUpdBt_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PlantRadius = Convert.ToDouble(this.TreeRadiusSetBox.Text);
            Properties.Settings.Default.PlantDensity = Convert.ToInt32(this.TreeDensitySetBox.Text);
            Properties.Settings.Default.Save();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //获取scriptid为种树的所有ThSitePlanConfigItem
                ThSitePlanConfigService.Instance.Initialize();
                List<ThSitePlanConfigItem> shadowconfigItem = new List<ThSitePlanConfigItem>();
                ThSitePlanConfigService.Instance.FindItemsByCADScript(ThSitePlanConfigService.Instance.Root, "5", ref shadowconfigItem);

                //获取所有种树图框
                List<ObjectId> frames = new List<ObjectId>();
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                foreach (var item in shadowconfigItem)
                {
                    ObjectId frame = ThSitePlanDbEngine.Instance.FrameByName(item.Properties["Name"].ToString());
                    frames.Add(frame);
                }

                Active.Editor.SetImpliedSelection(frames.ToArray());
                string commandst = "_.THPOPUD";
                Active.Document.SendStringToExecute($"{commandst} ",true,false,false);

                Properties.Settings.Default.Save();
                this.Close();
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
