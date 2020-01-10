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
            { QSelectFilterType.QSelectFilterColor,         "Color" },
            { QSelectFilterType.QSelectFilterLayer,         "Layer" },
            { QSelectFilterType.QSelectFilterLineType,      "linEtype" },
            { QSelectFilterType.QSelectFilterBlock,         "Block" },
            { QSelectFilterType.QSelectFilterDimension,     "Dimension" },
            { QSelectFilterType.QSelectFilterHatch,         "Hatch" }
        };

        private readonly Dictionary<QSelectExtent, string> extents = new Dictionary<QSelectExtent, string>()
        {
            { QSelectExtent.QSelectAll,     "All" },
            { QSelectExtent.QSelectView,    "View" }
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
                keywordOptions.Keywords.Add("Layer",        "Layer",        "图层(L)");
                keywordOptions.Keywords.Add("Color",        "Color",        "颜色(C)");
                keywordOptions.Keywords.Add("linEtype",     "linEtype",     "线型(E)");
                keywordOptions.Keywords.Add("Block",        "Block",        "图块名(B)");
                keywordOptions.Keywords.Add("Dimension",    "Dimension",    "标注(D)");
                keywordOptions.Keywords.Add("Hatch",        "Hatch",        "填充(H)");
                keywordOptions.Keywords.Default = "Layer";
                PromptResult result = Active.Editor.GetKeywords(keywordOptions);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var filter = filters.Where(o => o.Value == result.StringResult).First().Key;
                switch (filter)
                {
                    case QSelectFilterType.QSelectFilterLayer:
                    case QSelectFilterType.QSelectFilterColor:
                    case QSelectFilterType.QSelectFilterLineType:
                    case QSelectFilterType.QSelectFilterBlock:
                        DoSelectWithProperty(filter);
                        break;
                    case QSelectFilterType.QSelectFilterDimension:
                        DoSelectWithDxfName("DIMENSION");
                        break;
                    case QSelectFilterType.QSelectFilterHatch:
                        DoSelectWithDxfName("HATCH");
                        break;
                    default:
                        break;
                }
            }
        }

        private PromptResult SpecifySelectExtent()
        {
            var keywordOptions = new PromptKeywordOptions("\n请指定选取范围：")
            {
                AllowNone = true
            };
            keywordOptions.Keywords.Add("All",  "All",  "选取全部范围(A)");
            keywordOptions.Keywords.Add("View", "View", "选取当前视窗(V)");
            keywordOptions.Keywords.Default = "All";
            return Active.Editor.GetKeywords(keywordOptions);
        }

        private void DoSelectWithDxfName(string dxfName)
        {
            var result = SpecifySelectExtent();
            if (result.Status != PromptStatus.OK)
            {
                return;
            }

            switch (extents.Where(o => o.Value == result.StringResult).First().Key)
            {
                case QSelectExtent.QSelectAll:
                    Active.Editor.QSelect(dxfName);
                    break;
                case QSelectExtent.QSelectView:
                    Active.Editor.GetCurrentView().QSelect(dxfName);
                    break;
                default:
                    break;
            }
        }

        private void DoSelectWithProperty(QSelectFilterType filterType)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
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
                var result = SpecifySelectExtent();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                switch (extents.Where(o => o.Value == result.StringResult).First().Key)
                {
                    case QSelectExtent.QSelectAll:
                        Active.Editor.QSelect(entity, filterType);
                        break;
                    case QSelectExtent.QSelectView:
                        Active.Editor.GetCurrentView().QSelect(entity, filterType);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
