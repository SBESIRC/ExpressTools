using Autodesk.AutoCAD.Windows;
using acadApp=Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThColumnInfo.Validate;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThColumnInfo.View
{
    public class CheckPalette
    {
        internal static CheckPalette instance = null;
        internal static PaletteSet _ps = null;
        internal static CheckResult _checkResult = null;
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
                _ps = new PaletteSet("柱配筋校核",typeof(CheckPalette).GUID); //新建一个面板对象，标题为 “检查结果”

                _checkResult = new CheckResult();
                _ps.Add("", _checkResult);
                _ps.Load += _ps_Load;
                _ps.SizeChanged += _ps_SizeChanged;
            }
            else
            {
                _checkResult.LoadTree(acadApp.Application.DocumentManager.MdiActiveDocument.Name);
            }
            _ps.Visible = true;
        }

        private void _ps_SizeChanged(object sender, PaletteSetSizeEventArgs e)
        {
            SizeChange();
        }

        private void _ps_Load(object sender, PalettePersistEventArgs e)
        {
            _checkResult.BackColor = System.Drawing.Color.FromArgb(92,92,92);
            _ps.Style = PaletteSetStyles.ShowAutoHideButton |
                    PaletteSetStyles.ShowCloseButton ;
            _ps.DockEnabled = DockSides.Left | DockSides.Right;
            _ps.Dock = DockSides.Left;
            SizeChange();
            _ps.Size = new System.Drawing.Size(250, 1000);
            _ps.MinimumSize = new System.Drawing.Size(250, 1000);
            _ps.Location = new System.Drawing.Point(-50, _ps.Location.Y);

            //_ps.SetSize(new System.Drawing.Size(250, 1000));
            //_ps.SetLocation(new System.Drawing.Point(-50, _ps.Location.Y));
            //_ps.DeviceIndependentLocation = new Point(-50,0);            
        }
        private void SizeChange()
        {
            _checkResult.Height = (int)_ps.PaletteSize.Height;
            _checkResult.Width = (int)_ps.PaletteSize.Width;
            _checkResult.panelUp.Width = _checkResult.Width-6;
            _checkResult.panelMiddle.Width = _checkResult.Width-6;
            _checkResult.panelDown.Width = _checkResult.Width-6;
            _checkResult.panelMiddle.Top = _checkResult.panelUp.Bottom;
            _checkResult.panelMiddle.Height = Math.Abs(_checkResult.panelDown.Top - _checkResult.panelUp.Bottom);
        }
    }
}
