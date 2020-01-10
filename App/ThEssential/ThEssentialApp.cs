using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using AcHelper;
using System.Linq;
using System.Collections.Generic;

namespace ThEssential
{
    public class ThEssentialApp : IExtensionApplication
    {
        private readonly Dictionary<QSelectFilterType, string> filters = new Dictionary<QSelectFilterType, string>()
        {
            { QSelectFilterType.QSelectFilterColor, "Color" },
            { QSelectFilterType.QSelectFilterLayer, "Layer" },
            { QSelectFilterType.QSelectFilterLineType, "linEtype" },
            { QSelectFilterType.QSelectFilterBlock, "Block" }
        };

        private readonly Dictionary<QSelectMode, string> modes = new Dictionary<QSelectMode, string>()
        {
            { QSelectMode.QSelectAll, "All" },
            { QSelectMode.QSelectView, "View" }
        };

        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THQSELECT", CommandFlags.Modal)]
        public void ThQSelect()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 指定过滤条件
                PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n请指定过滤条件：")
                {
                    AllowNone = true
                };
                keywordOptions.Keywords.Add("Layer", "图层");
                keywordOptions.Keywords.Add("Color", "颜色");
                keywordOptions.Keywords.Add("linEtype", "线型");
                keywordOptions.Keywords.Add("Block", "图块名");
                keywordOptions.Keywords.Default = "Layer";
                PromptResult result = Active.Editor.GetKeywords(keywordOptions);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                QSelectFilterType filter = filters.Where(o => o.Value == result.StringResult).First().Key;

                // 指定源实体
                PromptEntityOptions entityOptions = new PromptEntityOptions("\n请选取源对象：")
                {
                    AllowNone = false,
                    AllowObjectOnLockedLayer = false
                };
                PromptEntityResult entityResult = Active.Editor.GetEntity(entityOptions);
                if (entityResult.Status != PromptStatus.OK)
                {
                    return;
                }
                Entity entity = acadDatabase.Element<Entity>(entityResult.ObjectId);

                // 指定选取范围
                keywordOptions = new PromptKeywordOptions("\n请指定选取范围：")
                {
                    AllowNone = true
                };
                keywordOptions.Keywords.Add("All", "选取全部范围");
                keywordOptions.Keywords.Add("View", "选取当前视窗");
                keywordOptions.Keywords.Default = "All";
                result = Active.Editor.GetKeywords(keywordOptions);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                QSelectMode mode = modes.Where(o => o.Value == result.StringResult).First().Key;

                if (mode == QSelectMode.QSelectAll)
                {
                    Active.Editor.QSelect(entity, filter);
                }
                else if (mode == QSelectMode.QSelectView)
                {
                    Active.Editor.GetCurrentView().QSelect(entity, filter);
                }
            }
        }
    }
}
