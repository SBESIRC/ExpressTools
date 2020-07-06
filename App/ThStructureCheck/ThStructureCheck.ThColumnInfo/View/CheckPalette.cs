﻿using Autodesk.AutoCAD.Windows;
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
                _ps = new PaletteSet("图纸列表-柱", Guid.NewGuid()); //新建一个面板对象，标题为 “检查结果” typeof(CheckPalette).GUID
                _checkResult = new CheckResult();
                _ps.Add("", _checkResult);
            }
            else
            {
                _checkResult.LoadTree(acadApp.Application.DocumentManager.MdiActiveDocument.Name);
            }
            _ps_Load();
            if (!_ps.Visible)
            {
                _ps.Visible = true;
            }
            _ps.Dock = DockSides.Left;
        }
        private void _ps_Load()
        {
            _ps.Style = 
                PaletteSetStyles.ShowAutoHideButton |
                PaletteSetStyles.ShowCloseButton | 
                PaletteSetStyles.Snappable;
            _ps.DockEnabled = DockSides.Left | DockSides.Right;            
            _ps.Size = new System.Drawing.Size(250, 1000);
            _ps.MinimumSize = new System.Drawing.Size(250, 1000);
            _checkResult.BackColor = System.Drawing.Color.FromArgb(92, 92, 92);
        }
    }
}