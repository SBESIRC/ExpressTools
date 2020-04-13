using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcHelper;
using System.Collections.Generic;
using Linq2Acad;
using System.Linq;

namespace ThEssential.QSelect
{
    public static class ThQuickSelectEditorExtension
    {
        public static void QSelect(this Editor ed, string dxfName)
        {
            var result = ed.SelectAll(dxfName.QSelectFilter());
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
        }

        public static void QSelect(this Editor ed, Entity entity, QSelectFilterType filterType)
        {
            if(filterType== QSelectFilterType.QSelectFilterColor)
            {
                List<ObjectId> sameColorObjIds = QSelectColor(ed, entity);
                if(sameColorObjIds.Count>0)
                {
                    Active.Editor.SetImpliedSelection(sameColorObjIds.ToArray());
                }
            }
            else if(filterType == QSelectFilterType.QSelectFilterLineType)
            {
                List<ObjectId> sameLineTypeObjIds = QSelectLineType(ed, entity);
                if (sameLineTypeObjIds.Count > 0)
                {
                    Active.Editor.SetImpliedSelection(sameLineTypeObjIds.ToArray());
                }
            }
            else
            {
                var result = ed.SelectAll(entity.QSelectFilter(filterType));
                if (result.Status == PromptStatus.OK)
                {
                    Active.Editor.SetImpliedSelection(result.Value);
                }
            }
        }
        private static List<ObjectId> QSelectLineType(Editor ed, Entity entity)
        {
            List<ObjectId> sameLineTypeObjIds = new List<ObjectId>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = ed.SelectAll();
                if (result.Status == PromptStatus.OK)
                {
                    ObjectId[] findObjIds = result.Value.GetObjectIds();
                    foreach (ObjectId objId in findObjIds)
                    {
                        Entity currentEnt = acadDatabase.Element<Entity>(objId);
                        if(!(currentEnt is Curve))
                        {
                            continue;
                        }
                        if(entity.Linetype== currentEnt.Linetype)
                        {
                            sameLineTypeObjIds.Add(objId);
                        }
                    }
                }
            }
            return sameLineTypeObjIds;
        }
        private static List<ObjectId> QSelectColor(Editor ed,Entity entity)
        {
            List<ObjectId> sameColorObjIds = new List<ObjectId>();
            var result = ed.SelectAll();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.Colors.Color entColor = entity.Color;
                if (entColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                {
                    entColor = ThQuickSelect.GetByLayerColor(acadDatabase.Database,entity);
                }
                if (result.Status == PromptStatus.OK)
                {
                    ObjectId[] findObjIds = result.Value.GetObjectIds();
                    foreach (ObjectId objId in findObjIds)
                    {
                        Entity currentEnt = acadDatabase.Element<Entity>(objId);
                        Autodesk.AutoCAD.Colors.Color currentColor = currentEnt.Color;
                        if (currentColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                        {
                            currentColor = ThQuickSelect.GetByLayerColor(acadDatabase.Database, currentEnt);
                        }
                        if (currentColor.Equals(entColor))
                        {
                            sameColorObjIds.Add(objId);
                        }
                    }
                }
            }
            return sameColorObjIds;
        }
        public static void QSelectLast(this Editor ed)
        {
            var result = ed.SelectLast();
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
        }

        public static void QSelectPrevious(this Editor ed)
        {
            var result = ed.SelectPrevious();
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
        }
    }
}
