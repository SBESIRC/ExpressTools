using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils.Menu;
using DevExpress.XtraTab;
using DevExpress.XtraTab.Buttons;
using DevExpress.XtraEditors.Controls;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThAOccupancyControl : DevExpress.XtraEditors.XtraUserControl, IAOccupancyView
    {
        private ThAOccupanyPresenter Presenter;
        private ThAOccupancyDbDepository DbRepository;

        public ThAOccupancyControl()
        {
            InitializeComponent();
            InitializeTabControl();
        }

        public List<ThAOccupancy> AOccupancies
        {
            get
            {
                return (List<ThAOccupancy>)gridControl_aoccupancy.DataSource;
            }

            set
            {
                gridControl_aoccupancy.DataSource = value;
            }
        }

        public void Attach(IAOccupanyPresenterCallback presenter)
        {
            //
        }

        public void Reload()
        {
            //
        }

        public void InitializeTabControl()
        {
            Presenter = new ThAOccupanyPresenter(this);
            DbRepository = new ThAOccupancyDbDepository();
            if (DbRepository.Storeys.Count == 0)
            {
                DbRepository.AppendStorey("c1");
            }
            foreach (var storey in DbRepository.Storeys)
            {
                XtraTabPage page = this.xtraTabControl1.TabPages.Add(storey.Identifier);
            }

            this.xtraTabControl1.TabPages.RemoveAt(0);
        }
    }
}
