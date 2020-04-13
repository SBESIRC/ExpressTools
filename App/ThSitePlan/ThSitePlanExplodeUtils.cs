using AcHelper;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan
{
    public static class ThSitePlanExplodeUtils
    {
        public static void ExplodeToOwnerSpace(this Database database, ObjectId frame)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                while (true)
                {
                    var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == ThCADCommon.DxfName_Insert);
                    PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                        frame,
                        PolygonSelectionMode.Crossing,
                        filter);
                    if (psr.Status != PromptStatus.OK)
                    {
                        break;
                    }

                    var blockReferences = new List<BlockReference>();
                    foreach(var obj in psr.Value.GetObjectIds())
                    {
                        var item = acadDatabase.Element<BlockReference>(obj, true);
                        if (item.IsBlockReferenceExplodable())
                        {
                            blockReferences.Add(item);
                        }
                    }
                    if (!blockReferences.Any())
                    {
                        break;
                    }
                    blockReferences.ForEachDbObject(b => b.ExplodeToOwnerSpace());
                    blockReferences.ForEachDbObject(b => b.Erase());
                }
            }
        }
    }
}
