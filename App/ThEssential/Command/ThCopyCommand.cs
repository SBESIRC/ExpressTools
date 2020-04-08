﻿using System;
using AcHelper;
using Linq2Acad;
using AcHelper.Commands;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThEssential.Copy;

namespace ThEssential.Command
{
    public class ThCopyCommand : IAcadCommand, IDisposable
    {
        public void Dispose()
        {
            //
        }

        public void Execute()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var entSelected = Active.Editor.GetSelection(options);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };
                var objs = new ObjectIdCollection(entSelected.Value.GetObjectIds());

                using (ThHighlightOverride o = new ThHighlightOverride(objs))
                {
                    // 指定起始参考点
                    PromptPointOptions prOpt = new PromptPointOptions("\n指定基点");
                    PromptPointResult ppr = Active.Editor.GetPoint(prOpt);
                    if (ppr.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    // 指定终点参考点
                    var jig = new ThCopyDistanceJig(ppr.Value);
                    foreach (ObjectId obj in objs)
                    {
                        jig.AddEntity(acadDatabase.Element<Entity>(obj));
                    }
                    jig.Options = ThCopyArrayOptions.Array;
                    PromptResult jigRes = Active.Editor.Drag(jig);
                    if (jigRes.Status == PromptStatus.OK)
                    {
                        Active.Editor.Copy(objs, 
                            ppr.Value, 
                            ppr.Value + jig.Displacement);
                        while (true)
                        {
                            using (ThHighlightOverride ho = new ThHighlightOverride(objs))
                            {
                                jig.Options = ThCopyArrayOptions.All;
                                PromptResult jigRes2 = Active.Editor.Drag(jig);
                                if (jigRes2.Status == PromptStatus.OK)
                                {
                                    Active.Editor.Copy(objs,
                                        ppr.Value,
                                        ppr.Value + jig.Displacement);
                                    continue;
                                }
                                else if (jigRes2.Status == PromptStatus.Keyword)
                                {
                                    switch (jigRes2.StringResult)
                                    {
                                        case "Array":
                                            {
                                                OptionArrayHandler(objs, ppr.Value);
                                                break;
                                            }
                                        case "Copy":
                                            {
                                                OptionCopyHandler(objs, ppr.Value, jig.Displacement);
                                                break;
                                            }
                                        case "Divide":
                                            {
                                                OptionDivideHandler(objs, ppr.Value, jig.Displacement);
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                else if (jigRes.Status == PromptStatus.Other)
                                {
                                    // 按“Space”键和“Enter”键，退出循环
                                    break;
                                }
                                else
                                {
                                    // 其他未处理情况，退出循环
                                    break;
                                }
                            }
                        }
                    }
                    else if (jigRes.Status == PromptStatus.Keyword)
                    {
                        switch (jigRes.StringResult)
                        {
                            case "Array":
                                {
                                    OptionArrayHandler(objs, ppr.Value);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                    else if (jigRes.Status == PromptStatus.Other)
                    {
                        // 按“Space”键和“Enter”键，退出
                        return;
                    }
                    else
                    {
                        // 其他未处理情况，退出
                        return;
                    }
                }
            }
        }

        private void OptionArrayHandler(ObjectIdCollection objs, Point3d basePt)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                while(true)
                {
                    PromptIntegerOptions prPntOpt = new PromptIntegerOptions("输入要进行阵列的项目数")
                    {
                        AllowNone = true,
                        AllowArbitraryInput = true,
                        AllowNegative = false,
                        AllowZero = false,
                        LowerLimit = 2,
                        UpperLimit = 32767,
                    };
                    PromptIntegerResult prPntRes = Active.Editor.GetInteger(prPntOpt);
                    if (prPntRes.Status == PromptStatus.OK)
                    {
                        var arrayJig = new ThCopyArrayJig(basePt)
                        {
                            Parameter = (uint)prPntRes.Value,
                        };
                        foreach (ObjectId obj in objs)
                        {
                            arrayJig.AddEntity(acadDatabase.Element<Entity>(obj));
                        }
                        PromptResult arrayJigRes = Active.Editor.Drag(arrayJig);
                        if (arrayJigRes.Status == PromptStatus.OK)
                        {
                            Active.Editor.CopyWithArray(objs,
                                basePt,
                                basePt + arrayJig.Displacement,
                                arrayJig.Parameter
                                );
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (prPntRes.Status == PromptStatus.Keyword)
                    {
                        Active.Editor.WriteLine("需要2和32767之间的整数.");
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void OptionCopyHandler(ObjectIdCollection objs, Point3d basePt, Vector3d displacement)
        {
            using (var transient = new ThCopyArrayTransient())
            {
                while (true)
                {
                    var entities = objs.TimesAlongPath(displacement, transient.Parameter + 1);
                    transient.CreateTransGraphics(entities);
                    PromptIntegerOptions prPntOpt = new PromptIntegerOptions("输入要进行复制的次数")
                    {
                        AllowNone = true,
                        AllowArbitraryInput = true,
                        AllowNegative = false,
                        AllowZero = false,
                        LowerLimit = 2,
                        UpperLimit = 32767,
                    };
                    PromptIntegerResult prPntRes = Active.Editor.GetInteger(prPntOpt);
                    transient.ClearTransGraphics(entities);
                    if (prPntRes.Status == PromptStatus.OK)
                    {
                        transient.Parameter = (uint)prPntRes.Value;
                        continue;
                    }
                    else if (prPntRes.Status == PromptStatus.None)
                    {
                        Active.Editor.CopyWithArray(objs,
                            basePt,
                            basePt + displacement,
                            transient.Parameter + 1
                            );
                        break;
                    }
                    else if (prPntRes.Status == PromptStatus.Keyword)
                    {
                        Active.Editor.WriteLine("需要2和32767之间的整数.");
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void OptionDivideHandler(ObjectIdCollection objs, Point3d basePt, Vector3d displacement)
        {
            using (var transient = new ThCopyArrayTransient())
            {
                while (true)
                {
                    var entities = objs.DivideAlongPath(displacement, transient.Parameter + 1);
                    transient.CreateTransGraphics(entities);
                    PromptIntegerOptions prPntOpt = new PromptIntegerOptions("输入要进行均分的段数")
                    {
                        AllowNone = true,
                        AllowArbitraryInput = true,
                        AllowNegative = false,
                        AllowZero = false,
                        LowerLimit = 2,
                        UpperLimit = 32767,
                    };
                    PromptIntegerResult prPntRes = Active.Editor.GetInteger(prPntOpt);
                    transient.ClearTransGraphics(entities);
                    if (prPntRes.Status == PromptStatus.OK)
                    {
                        transient.Parameter = (uint)prPntRes.Value;
                        continue;
                    }
                    else if (prPntRes.Status == PromptStatus.None)
                    {
                        Active.Editor.CopyWithFit(objs,
                            basePt,
                            basePt + displacement,
                            transient.Parameter + 1
                            );
                        break;
                    }
                    else if(prPntRes.Status == PromptStatus.None)
                    {
                        Active.Editor.WriteLine("需要2和32767之间的整数.");
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
