using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using ThColumnInfo.Validate;
using System.Windows.Forms;
using System.Drawing;

namespace ThColumnInfo.View
{
   public class DataPalette
    {
        static DataPalette instance = null;
        internal static PaletteSet _ps = null;
        public static DataResult _dateResult = null;
        public static bool ShowPaletteMark = false;
        public DataPalette()
        {
        }
        public static DataPalette Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataPalette();
                }
                return instance;
            }
        }
        public void Show(IDataSource ds,ThSpecificationValidate tsv,ThCalculationValidate tcv=null, TreeNode node=null)
        {
            if (_ps == null)
            {
                _ps = new PaletteSet("", typeof(DataPalette).GUID); //新建一个面板对象，标题为 “检查结果”
                _dateResult = new DataResult(ds, tsv, tcv, node);
                _ps.Add("", _dateResult);
                _ps_Load();
                _ps.StateChanged += _ps_StateChanged;
            }
            else
            {
                _dateResult.UpdateData(ds, tsv, tcv, node);
            }
            _ps.Visible = ShowPaletteMark;
            _ps.Dock = DockSides.Bottom;
        }

        private void _ps_StateChanged(object sender, PaletteSetStateEventArgs e)
        {
            if(e.NewState== StateEventIndex.Hide)
            {
                ShowPaletteMark = false;
            }
            else
            {
                ShowPaletteMark = true;
            }
            CheckPalette._checkResult.SwitchShowDetailPicture();
        }
        private void _ps_Load()
        {
            _dateResult.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            _ps.Style = PaletteSetStyles.ShowAutoHideButton |
                    PaletteSetStyles.ShowCloseButton;
            _ps.DockEnabled = DockSides.Bottom;           
            _ps.Size = new System.Drawing.Size(800, 200);
            _ps.MinimumSize = new System.Drawing.Size(800, 200);
        }
    }
}
