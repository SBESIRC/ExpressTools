using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using ThColumnInfo.Validate;
using System.Windows.Forms;

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
                _ps.Load += _ps_Load;
                _ps.PaletteSetDestroy += _ps_PaletteSetDestroy;
            
            }
            else
            {
                _dateResult.UpdateData(ds, tsv, tcv, node);
            }
            _ps.Visible = ShowPaletteMark;
        }

        private void _ps_PaletteSetDestroy(object sender, EventArgs e)
        {
            ShowPaletteMark = false;
            CheckPalette._checkResult.SwitchShowDetailPicture();           
        }
        private void _ps_Load(object sender, PalettePersistEventArgs e)
        {
            _dateResult.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            _ps.Style = PaletteSetStyles.Snappable;
            _ps.DockEnabled = DockSides.Bottom;
            _ps.Dock = DockSides.Bottom;
            _ps.Size = new System.Drawing.Size(800, 200);
            _ps.MinimumSize = new System.Drawing.Size(800, 200);
        }
    }
}
