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

namespace TianHua.FanSelection.UI
{
    /// <summary>
    /// 避难走道前室
    /// </summary>
    public partial class RefugeFont : ThAirVolumeUserControl
    {
        private RefugeFontRoomModel Model { get; set; }
        public RefugeFont(RefugeFontRoomModel model)
        {
            InitializeComponent();
            Model = model;
            gridControl1.DataSource = model.FrontRoomDoors;
            UpdateWithModel(Model);
        }

        public override ThFanVolumeModel Data()
        {
            return Model;
        }

        private void Add_Click(object sender, EventArgs e)
        {
            Model.FrontRoomDoors.Add(new ThEvacuationDoor());
            gridControl1.DataSource = Model.FrontRoomDoors;
            UpdateWithModel(Model);
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
            gridControl1.DataSource = Model.FrontRoomDoors;
            UpdateWithModel(Model);
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
            UpdateWithModel(Model);
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
            UpdateWithModel(Model);
            gridView1.RefreshData();
            gridView1.FocusedRowHandle = index + 1;
        }

        private void UpdateWithModel(RefugeFontRoomModel Model)
        {
            Result.Text = Model.DoorOpeningVolume.ToString();
        }

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var _ThEvacuationDoor = gridView1.GetRow(gridView1.FocusedRowHandle) as ThEvacuationDoor;
            if (_ThEvacuationDoor == null) { return; }
            UpdateWithModel(Model);
        }

        //private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        //{
        //    var _ThEvacuationDoor = gridView1.GetRow(gridView1.FocusedRowHandle) as ThEvacuationDoor;
        //    if (_ThEvacuationDoor == null) { return; }
        //    UpdateWithModel(Model);
        //}

        private void DoorInfoChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            UpdateWithModel(Model);
        }
    }
}
