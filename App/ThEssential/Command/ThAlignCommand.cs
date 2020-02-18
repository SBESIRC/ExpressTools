using System;
using System.Linq;
using System.Collections.Generic;
using AcHelper;
using Linq2Acad;
using AcHelper.Commands;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThEssential.Align;
using ThEssential.Distribute;

namespace ThEssential.Command
{
    public class ThAlignCommand : IAcadCommand, IDisposable
    {
        private readonly Dictionary<AlignMode, string> alignments = new Dictionary<AlignMode, string>()
        {
            { AlignMode.XFont,      "Bottom" },
            { AlignMode.XCenter,    "Horizontal" },
            { AlignMode.XBack,      "Top" },
            { AlignMode.YLeft,      "Left" },
            { AlignMode.YCenter,    "Vertical" },
            { AlignMode.YRight,     "Right" }   
        };

        private readonly Dictionary<DistributeMode, string> distributions = new Dictionary<DistributeMode, string>()
        {
            { DistributeMode.XGap,  "Xdistribute" },
            { DistributeMode.YGap,  "Ydistribute" }
        };

        public void Dispose()
        {
            //
        }

        public void Execute()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
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

                // 指定对齐方式
                PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n请指定对齐方式：")
                {
                    AllowNone = true
                };
                keywordOptions.Keywords.Add("Top", "Top", "向上对齐(T)");
                keywordOptions.Keywords.Add("Bottom", "Bottom", "向下对齐(B)");
                keywordOptions.Keywords.Add("Left", "Left", "向左对齐(L)");
                keywordOptions.Keywords.Add("Right", "Right", "向右对齐(R)");
                keywordOptions.Keywords.Add("Horizontal", "Horizontal", "水平居中(H)");
                keywordOptions.Keywords.Add("Vertical", "Vertical", "垂直居中(V)");
                keywordOptions.Keywords.Add("Xdistribute", "Xdistribute", "水平均分(X)");
                keywordOptions.Keywords.Add("Ydistribute", "Ydistribute", "垂直均分(Y)");
                keywordOptions.Keywords.Default = "Top";
                PromptResult result = Active.Editor.GetKeywords(keywordOptions);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                // 若选择分布方式，执行分布操作
                foreach (var distribution in distributions.Where(o => o.Value == result.StringResult))
                {
                    var objIds = new ObjectIdCollection(entSelected.Value.GetObjectIds());
                    objIds.Distribute(distribution.Key);
                    return;
                }

                // 若选择对齐方式，则指定对齐基点
                var pointOptions = new PromptPointOptions("\n请指定参考点：")
                {
                    AllowNone = true
                };
                var ptResult = Active.Editor.GetPoint(pointOptions);
                if (ptResult.Status != PromptStatus.OK)
                {
                    return;
                }

                // 执行对齐操作
                foreach (var alignment in alignments.Where(o => o.Value == result.StringResult))
                {
                    foreach (var objId in entSelected.Value.GetObjectIds())
                    {
                        var obj = acadDatabase.Element<Entity>(objId, true);
                        if (obj is Polyline polyline)
                        {
                            polyline.Align(alignment.Key, ptResult.Value);
                        }
                        else if (obj is Circle circle)
                        {
                            circle.Align(alignment.Key, ptResult.Value);
                        }
                        else if (obj is Arc arc)
                        {
                            arc.Align(alignment.Key, ptResult.Value);
                        }
                        else if (obj is Ellipse ellipse)
                        {
                            ellipse.Align(alignment.Key, ptResult.Value);
                        }
                        else if (obj is DBText dBText)
                        {
                            dBText.Align(alignment.Key, ptResult.Value);
                        }
                        else if (obj is MText mText)
                        {
                            mText.Align(alignment.Key, ptResult.Value);
                        }
                        else if (obj is BlockReference blockReference)
                        {
                            blockReference.Align(alignment.Key, ptResult.Value);
                        }
                        else
                        {
                            obj.Align(alignment.Key, ptResult.Value);
                        }
                    }
                }
            }
        }
    }
}
