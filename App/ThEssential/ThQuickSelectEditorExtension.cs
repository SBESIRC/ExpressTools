using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;

namespace ThEssential
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
            var result = ed.SelectAll(entity.QSelectFilter(filterType));
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
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
