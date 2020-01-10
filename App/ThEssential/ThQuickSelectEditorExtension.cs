using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;

namespace ThEssential
{
    public static class ThQuickSelectEditorExtension
    {
        public static void QSelect(this Editor ed, Entity entity, QSelectFilterType filterType)
        {
            var result = ed.SelectAll(entity.QSelectFilter(filterType));
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
        }
    }
}
