using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;

namespace THColumnInfo.View
{
   public class DataPalette
    {
        static DataPalette instance = null;
        internal static PaletteSet _ps = null;
        public static DataResult _dateResult = null;
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
        public void Show()
        {
            if (_ps == null)
            {
                _ps = new PaletteSet("", typeof(DataPalette).GUID); //新建一个面板对象，标题为 “检查结果”
                _dateResult = new DataResult();
                _ps.Add("", _dateResult);
                Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
                Application.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;
                _ps.Load += _ps_Load;
                _ps.DockEnabled = (DockSides)(DockSides.Bottom);
                _ps.Dock = DockSides.Bottom;
                _ps.MinimumSize = new System.Drawing.Size(800, 200);
            }
            _ps.Visible = true;

        }

        private void _ps_Load(object sender, PalettePersistEventArgs e)
        {
        }

        private void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
        }

        private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
        }
    }
}
