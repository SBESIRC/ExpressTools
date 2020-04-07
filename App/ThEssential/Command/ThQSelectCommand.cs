using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using AcHelper;
using AcHelper.Commands;
using ThEssential.QSelect;

namespace ThEssential.Command
{
    public class ThQSelectCommand : IAcadCommand, IDisposable
    {
        private readonly Dictionary<QSelectFilterType, string> filters = new Dictionary<QSelectFilterType, string>()
        {
            { QSelectFilterType.QSelectFilterColor,         "Color" },
            { QSelectFilterType.QSelectFilterLayer,         "Layer" },
            { QSelectFilterType.QSelectFilterLineType,      "linEtype" },
            { QSelectFilterType.QSelectFilterBlock,         "Block" },
            { QSelectFilterType.QSelectFilterDimension,     "Dimension" },
            { QSelectFilterType.QSelectFilterHatch,         "Hatch" },
            { QSelectFilterType.QSelectFilterText,          "Text" },
            { QSelectFilterType.QSelectFilterLast,          "lastAppend" },
            { QSelectFilterType.QSelectFilterPrevious,      "preVious"}
        };

        private readonly Dictionary<QSelectExtent, string> extents = new Dictionary<QSelectExtent, string>()
        {
            { QSelectExtent.QSelectAll,     "All" },
            { QSelectExtent.QSelectView,    "View" }
        };

        public void Dispose()
        {
        }

        public void Execute()
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
                keywordOptions.Keywords.Add("Text",         "Text",         "文字(T)");
                keywordOptions.Keywords.Add("lastAppend",   "lastAppend",   "上次建立(A)");
                keywordOptions.Keywords.Add("preVious",     "preVious",     "上次选取(V)");
                string defaultKey= Properties.Settings.Default.QsKey;
                if(string.IsNullOrEmpty(defaultKey))
                {
                    defaultKey = "Layer";
                }
                keywordOptions.Keywords.Default = defaultKey;
                PromptResult result = Active.Editor.GetKeywords(keywordOptions);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                Properties.Settings.Default.QsKey = result.StringResult;
                Properties.Settings.Default.Save();

                var filter = filters.Where(o => o.Value == result.StringResult).First().Key;
                switch (filter)
                {
                    case QSelectFilterType.QSelectFilterLayer:
                    case QSelectFilterType.QSelectFilterColor:
                    case QSelectFilterType.QSelectFilterLineType:
                    case QSelectFilterType.QSelectFilterBlock:
                        DoSelectByObjectProperty(filter);
                        break;
                    case QSelectFilterType.QSelectFilterDimension:
                        DoSelectByObjectType("DIMENSION");
                        break;
                    case QSelectFilterType.QSelectFilterHatch:
                        DoSelectByObjectType("HATCH");
                        break;
                    case QSelectFilterType.QSelectFilterText:
                        DoSelectByObjectType("TEXT,MTEXT");
                        break;
                    case QSelectFilterType.QSelectFilterLast:
                        DoSelectLast();
                        break;
                    case QSelectFilterType.QSelectFilterPrevious:
                        DoSelectPrevious();
                        break;
                    default:
                        break;
                }
            }
        }

        private void DoSelectLast()
        {
            Active.Editor.QSelectLast();
        }

        private void DoSelectPrevious()
        {
            Active.Editor.QSelectPrevious();
        }

        private void DoSelectByObjectType(string dxfName)
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

        private void DoSelectByObjectProperty(QSelectFilterType filterType)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 指定源实体               
                ObjectId sourceEntityId = ObjectId.Null;
                var entSelected = Active.Editor.SelectImplied();
                if (entSelected.Status == PromptStatus.OK)
                {
                    if (entSelected.Value.GetObjectIds().Length == 1)
                    {
                        sourceEntityId = entSelected.Value.GetObjectIds().First();
                    }
                };
                //如果是图块选项，则只能选择BlockReference
                if (filterType== QSelectFilterType.QSelectFilterBlock)
                {
                    if(sourceEntityId!= ObjectId.Null)
                    {
                        Entity sourceEnt = acadDatabase.Element<Entity>(sourceEntityId);
                        if(!(sourceEnt is BlockReference))
                        {
                            sourceEntityId = ObjectId.Null;
                        }
                    }
                }
                else if(filterType == QSelectFilterType.QSelectFilterLineType)
                {
                    if (sourceEntityId != ObjectId.Null)
                    {
                        Entity sourceEnt = acadDatabase.Element<Entity>(sourceEntityId);
                        if (!(sourceEnt is Curve))
                        {
                            sourceEntityId = ObjectId.Null;
                        }
                    }
                }
                if(sourceEntityId== ObjectId.Null)
                {
                    var entityResult = SelectEntity(acadDatabase, filterType);
                    if (entityResult==null || entityResult.Status != PromptStatus.OK)
                    {
                        return;
                    }
                    sourceEntityId = entityResult.ObjectId;
                }
                if(sourceEntityId== ObjectId.Null)
                {
                    return;
                }

                // 指定选取范围
                var promptResult = SpecifySelectExtent();
                if (promptResult.Status != PromptStatus.OK)
                {
                    return;
                }
                Entity entity = acadDatabase.Element<Entity>(sourceEntityId);
                switch (extents.Where(o => o.Value == promptResult.StringResult).First().Key)
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

        private PromptResult SpecifySelectExtent()
        {
            var keywordOptions = new PromptKeywordOptions("\n请指定选取范围：")
            {
                AllowNone = true
            };
            keywordOptions.Keywords.Add("All", "All", "选取全部范围(A)");
            keywordOptions.Keywords.Add("View", "View", "选取当前视窗(V)");
            keywordOptions.Keywords.Default = "All";
            return Active.Editor.GetKeywords(keywordOptions);
        }

        private PromptEntityResult SpecifySelectEntity()
        {
            PromptEntityOptions entityOptions = new PromptEntityOptions("\n请选取源对象：")
            {
                AllowNone = false,
                AllowObjectOnLockedLayer = false
            };
            return Active.Editor.GetEntity(entityOptions);
        }
        private PromptEntityResult SelectEntity(AcadDatabase acadDb,QSelectFilterType filterType)
        {
            PromptEntityResult per = null;
            string message = "\n请选取源对象：";
            if(filterType== QSelectFilterType.QSelectFilterBlock)
            {
                message = "\n请选取一个块参照：";
            }
            else if(filterType == QSelectFilterType.QSelectFilterLineType)
            {
                message = "\n请选取一个块曲线：";
            }
            PromptEntityOptions entityOptions = new PromptEntityOptions(message)
            {
                AllowNone = false,
                AllowObjectOnLockedLayer = false
            };
            bool domark = true;
            while(domark)
            {
                per = Active.Editor.GetEntity(entityOptions);
                if(per.Status==PromptStatus.OK)
                {
                    if(filterType== QSelectFilterType.QSelectFilterBlock)
                    {
                        Entity entity = acadDb.Element<Entity>(per.ObjectId);
                        if(entity is BlockReference)
                        {
                            domark = false;
                        }
                        else
                        {
                            Active.Editor.WriteMessage(message);
                        }
                    }
                    else if(filterType == QSelectFilterType.QSelectFilterLineType)
                    {
                        Entity entity = acadDb.Element<Entity>(per.ObjectId);
                        if (entity is Curve)
                        {
                            domark = false;
                        }
                        else
                        {
                            Active.Editor.WriteMessage(message);
                        }
                    }
                    else
                    {
                        domark = false;
                    }
                }
                else if(per.Status== PromptStatus.Cancel)
                {
                    domark = false;
                }
                else if(per.Status == PromptStatus.None)
                {
                    Active.Editor.WriteMessage(message);
                }
            }
            return per;
        }
    }
}
