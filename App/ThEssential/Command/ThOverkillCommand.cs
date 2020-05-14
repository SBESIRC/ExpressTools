using System;
using AcHelper;
using Linq2Acad;
using AcHelper.Commands;
using ThEssential.Overkill;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;

namespace ThEssential.Command
{
    public class ThOverkillCommand : IAcadCommand, IDisposable
    {
        public void Dispose()
        {
            //
        }

        public void Execute()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "LINE");
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 选择公差
                PromptDoubleOptions promptDouble = new PromptDoubleOptions("\n请输入允许的公差") {
                    AllowNegative = false,
                    DefaultValue = Tolerance.Global.EqualPoint,
                };
                var numSelected = Active.Editor.GetDouble(promptDouble);
                if (numSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    dBObjects.Add(acdb.Element<Entity>(obj));
                }
                dBObjects.MergeOverlappingCurves(new Tolerance(numSelected.Value, numSelected.Value));
                dBObjects.RemoveDuplicateCurves(new Tolerance(numSelected.Value, numSelected.Value));
            }
        }
    }
}
