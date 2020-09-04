using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.FanSelection.Model;
using System.Text.RegularExpressions;

namespace TianHua.FanSelection.UI
{
    /// <summary>
    ///独立或合用前室（楼梯间送风）
    /// </summary>
    public partial class SeparateOrSharedWind : ThAirVolumeUserControl
    {
        private UserControl subview;
        private FontroomWindModel Model { get; set; }
        private ModelValidator valid = new ModelValidator();

        public SeparateOrSharedWind(FontroomWindModel  model)
        {
            InitializeComponent();
            Model = model;
            gridControl1.DataSource = model.FrontRoomDoors;
            CheckPanel.Controls.Clear();
            subview = new ModelValidation(Model);
            CheckPanel.Controls.Add(subview);

            if (model.Count_Floor != 0)
            {
                layerCount.Text = model.Count_Floor.ToString();
                length.Text = model.Length_Valve.ToString();
                wide.Text = model.Width_Valve.ToString();
            }
        }

        public override ThFanVolumeModel Data()
        {
            return Model;
        }

        private void lowLoad_CheckedChanged(object sender, EventArgs e)
        {
            if (lowLoad.Checked)
            {
                Model.Load = Model.Load = FontroomWindModel.LoadHeight.LoadHeightLow; 
                UpdateWithModel(Model);
                CheckPanel.Controls.Clear();
                CheckPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            }
           
        }

        private void middleLoad_CheckedChanged(object sender, EventArgs e)
        {
            if (middleLoad.Checked)
            {
                Model.Load = Model.Load = FontroomWindModel.LoadHeight.LoadHeightMiddle;
                UpdateWithModel(Model);
            }
           
        }

        private void highLoad_CheckedChanged(object sender, EventArgs e)
        {
            if (highLoad.Checked)
            {
                Model.Load = Model.Load = FontroomWindModel.LoadHeight.LoadHeightHigh;
                UpdateWithModel(Model);
            }
            
        }

        private void layerCount_EditValueChanged(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(layerCount.Text, "^[0-9]+$"))
            {
                return;
            }
            Model.Count_Floor = Convert.ToInt32(layerCount.Text);
            UpdateWithModel(Model);
          
        }

        private void length_EditValueChanged(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(length.Text, "^[0-9]+$"))
            {
                return;
            }
            Model.Length_Valve = Convert.ToInt32(length.Text);
            UpdateWithModel(Model);
        }

        private void wide_EditValueChanged(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(wide.Text, "^[0-9]+$"))
            {
                return;
            }
            Model.Width_Valve = Convert.ToInt32(wide.Text);
            UpdateWithModel(Model);        
        }
        private void UpdateWithModel(FontroomWindModel model)
        {
            Lj.Text = Convert.ToString(Model.TotalVolume);
            L1.Text = Convert.ToString(Model.DoorOpeningVolume);
            L3.Text = Convert.ToString(Model.LeakVolume);
            if (lowLoad.Checked)
            {
                Tips.Text = " ";
            }
            else if (middleLoad.Checked)
            {
                CheckLjValue(Model.AAAA, Model.BBBB);
                CheckPanel.Controls.Clear();
                subview = new ModelValidation(Model);
                CheckPanel.Controls.Add(subview);
                CheckPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            }
            else if (highLoad.Checked)
            {
                CheckLjValue(Model.CCCC, Model.DDDD);
                CheckPanel.Controls.Clear();
                subview = new ModelValidation(Model);
                CheckPanel.Controls.Add(subview);
                CheckPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            Model.FrontRoomDoors.Add(new ThEvacuationDoor());
            gridControl1.DataSource = Model.FrontRoomDoors;
            gridView1.RefreshData();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (Model.FrontRoomDoors.Count() == 0)
            {
                return;
            }
            foreach (int row in gridView1.GetSelectedRows())
            {
                Model.FrontRoomDoors.RemoveAt(row);
            }
            UpdateWithModel(Model);
            gridControl1.DataSource = Model.FrontRoomDoors;
            gridView1.RefreshData();
        }

        private void MoveUp_Click(object sender, EventArgs e)
        {
            int index = gridView1.GetSelectedRows()[0];
            if (index == 0)
            {
                return;
            }
            var tmp = Model.FrontRoomDoors[index];
            Model.FrontRoomDoors[index] = Model.FrontRoomDoors[index - 1];
            Model.FrontRoomDoors[index - 1] = tmp;
            gridControl1.DataSource = Model.FrontRoomDoors;
            gridView1.RefreshData();
            gridView1.FocusedRowHandle = index - 1;
        }

        private void MoveDown_Click(object sender, EventArgs e)
        {
            int index = gridView1.GetSelectedRows()[0];
            if (index == Model.FrontRoomDoors.Count() - 1)
            {
                return;
            }
            var tmp = Model.FrontRoomDoors[index];
            Model.FrontRoomDoors[index] = Model.FrontRoomDoors[index + 1];
            Model.FrontRoomDoors[index + 1] = tmp;
            gridControl1.DataSource = Model.FrontRoomDoors;
            gridView1.RefreshData();
            gridView1.FocusedRowHandle = index + 1;
        }

        private void SeparateOrSharedWind_Load(object sender, EventArgs e)
        {
            middleLoad.Checked = true;
        }

        private void CheckLjValue(double minvalue, double maxvalue)
        {
            if (Model.OverAk > 3.2)
            {
                if (Model.TotalVolume < minvalue)
                {
                    Tips.Text = "计算值不满足规范";
                    Tips.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    Tips.Text = "计算值满足规范";
                    Tips.ForeColor = System.Drawing.Color.Green;
                }
            }
            else
            {
                if (Model.TotalVolume < 0.75 * minvalue)
                {
                    Tips.Text = "计算值不满足规范";
                    Tips.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    Tips.Text = "计算值满足规范";
                    Tips.ForeColor = System.Drawing.Color.Green;
                }

            }
        }

        private void DoorInfoChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            UpdateWithModel(Model);
            subview.Refresh();
        }

    }
}
