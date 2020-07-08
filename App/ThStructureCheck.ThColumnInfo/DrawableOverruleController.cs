using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

namespace ThColumnInfo
{
    public class DrawableOverruleController
    {
        public static ColumnDrawRule _columnDrawRule;
        public static void ShowHatchForColumn(List<string> handles)
        {
            List<ObjectId> objIds = handles.Select(i => ThColumnInfoDbUtils.GetObjId(i, Application.DocumentManager.MdiActiveDocument.Database)).ToList();
            if (_columnDrawRule==null)
            {
                _columnDrawRule = new ColumnDrawRule();                
                Overrule.AddOverrule(RXObject.GetClass(typeof(Polyline2d)), _columnDrawRule, false);
            }
            Overrule.Overruling = true; //开启规则重定义

            _columnDrawRule.SetIdFilter(objIds.ToArray());
            //_columnDrawRule.SetCustomFilter();

            //刷新屏幕，柱子被填充
            Application.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        public static void RemoveDrawableRule()
        {
            if(_columnDrawRule!=null)
            {
                Overrule.RemoveOverrule(RXObject.GetClass(typeof(Polyline2d)), _columnDrawRule);
                _columnDrawRule = null;
            }
        }
    }
}
