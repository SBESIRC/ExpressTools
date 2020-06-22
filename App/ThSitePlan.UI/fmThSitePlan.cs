using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
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
using TianHua.Publics.BaseCode;
using Linq2Acad;

namespace ThSitePlan.UI
{
    public partial class fmThSitePlan : DevExpress.XtraEditors.XtraForm
    {
        public fmThSitePlan()
        {
            InitializeComponent();

            this.TxtShadowAngle.Text = FuncStr.NullToStr(ThSitePlanSettingsService.Instance.ShadowAngle);
            this.TxtShadowLength.Text = FuncStr.NullToStr(ThSitePlanSettingsService.Instance.ShadowLengthScale);
            this.TxtTreeRadius.Text = FuncStr.NullToStr(ThSitePlanSettingsService.Instance.PlantRadius);
            this.TxtTreeDensity.Text = FuncStr.NullToStr(ThSitePlanSettingsService.Instance.PlantDensity);
            this.TxtWorkPath.Text = ThSitePlanSettingsService.Instance.OutputPath;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            ThSitePlanSettingsService.Instance.ShadowAngle = FuncStr.NullToDouble(this.TxtShadowAngle.Text);
            ThSitePlanSettingsService.Instance.ShadowLengthScale = FuncStr.NullToDouble(this.TxtShadowLength.Text);
            ThSitePlanSettingsService.Instance.PlantRadius = FuncStr.NullToDouble(this.TxtTreeRadius.Text);
            ThSitePlanSettingsService.Instance.PlantDensity = FuncStr.NullToDouble(this.TxtTreeDensity.Text);
            ThSitePlanSettingsService.Instance.OutputPath = this.TxtWorkPath.Text;

            ThSitePlanSettingsService.Instance.SaveProperties();
            this.Close();
        }

        private void BtnShadowUpd_Click(object sender, EventArgs e)
        {
            ThSitePlanSettingsService.Instance.ShadowAngle = FuncStr.NullToDouble(this.TxtShadowAngle.Text);
            ThSitePlanSettingsService.Instance.ShadowLengthScale = FuncStr.NullToDouble(this.TxtShadowLength.Text);
            ThSitePlanSettingsService.Instance.SaveProperties();

            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //获取scriptid为阴影的所有ThSitePlanConfigItem
                ThSitePlanConfigService.Instance.Initialize();
                List<ThSitePlanConfigItem> shadowconfigItem = new List<ThSitePlanConfigItem>();
                ThSitePlanConfigService.Instance.FindItemsByCADScript(ThSitePlanConfigService.Instance.Root, "3", ref shadowconfigItem);
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

                ThSitePlanSettingsService.Instance.SaveProperties();
                this.Close();
            }
        }

        private void BtnLandTreeUpd_Click(object sender, EventArgs e)
        {
            ThSitePlanSettingsService.Instance.PlantRadius = FuncStr.NullToDouble(this.TxtTreeRadius.Text);
            ThSitePlanSettingsService.Instance.PlantDensity = FuncStr.NullToDouble(this.TxtTreeDensity.Text);
            ThSitePlanSettingsService.Instance.SaveProperties();
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
                Active.Document.SendStringToExecute($"{commandst} ", true, false, false);

                ThSitePlanSettingsService.Instance.SaveProperties();
                this.Close();
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }

            if (keyData == Keys.Enter)
            {
                BtnConfirm.PerformClick();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BtnBrose_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            this.TxtWorkPath.Text = path.SelectedPath;
        }

        private void TxtShadowAngle_MouseEnter(object sender, EventArgs e)
        {
            this.ToolTip.ShowBeak = true;

            this.ToolTip.ShowShadow = false;

            this.ToolTip.Rounded = false;

            this.ToolTip.ShowHint("阴影角度沿X轴正方向逆时针!", TxtShadowAngle, DevExpress.Utils.ToolTipLocation.Default);
        }

        private void TxtShadowLength_MouseEnter(object sender, EventArgs e)
        {
            this.ToolTip.ShowBeak = true;

            this.ToolTip.ShowShadow = false;

            this.ToolTip.Rounded = false;

            this.ToolTip.ShowHint("阴影长度系数!", TxtShadowLength, DevExpress.Utils.ToolTipLocation.Default);
        }

        private void TxtTreeRadius_MouseEnter(object sender, EventArgs e)
        {
            this.ToolTip.ShowBeak = true;

            this.ToolTip.ShowShadow = false;

            this.ToolTip.Rounded = false;

            this.ToolTip.ShowHint("树木半径(mm)!", TxtTreeRadius, DevExpress.Utils.ToolTipLocation.Default);
        }

        private void TxtTreeDensity_MouseEnter(object sender, EventArgs e)
        {
            this.ToolTip.ShowBeak = true;

            this.ToolTip.ShowShadow = false;

            this.ToolTip.Rounded = false;

            this.ToolTip.ShowHint("树木密度!", TxtTreeDensity, DevExpress.Utils.ToolTipLocation.Default);
        }
    }
}
