using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THColumnInfo.View
{
    public class CheckPalette
    {
        static CheckPalette instance = null;
        internal static PaletteSet _ps = null;
        static CheckResult _checkResult = null;
        public CheckPalette()
        {

        }
        public static CheckPalette Instance
        {
            get
            {
                if(instance==null)
                {
                    instance = new CheckPalette();
                }
                return instance;
            }
        }
        public void Show()
        {
            if(_ps == null)
            {
                _ps = new PaletteSet("检查结果",typeof(CheckPalette).GUID); //新建一个面板对象，标题为 “检查结果”
                _checkResult = new CheckResult();
                _ps.Add("", _checkResult);
                Application.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
                Application.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;
                _ps.Load += _ps_Load;
                _ps.DockEnabled = (DockSides)(DockSides.Left | DockSides.Right);
                _ps.Dock = DockSides.Left;
                _ps.MinimumSize = new System.Drawing.Size(200, 500);
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
