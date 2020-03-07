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
using Autodesk.AutoCAD.Geometry;

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

                // 若选择分布方式，执行分布操作
                foreach (var distribution in distributions.Where(o => o.Value == result.StringResult))
                {
                    var objIds = new ObjectIdCollection(entSelected.Value.GetObjectIds());
                    objIds.Distribute(distribution.Key);
                    return;
                }

                // 若选择对齐方式，则指定对齐基点
                bool isSuccessSelectPt = false;
                Point3d alignPt = GetAlignPoint(result.StringResult, entSelected.Value.GetObjectIds(), out isSuccessSelectPt);
                if(!isSuccessSelectPt)
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
                            polyline.Align(alignment.Key, alignPt);
                        }
                        else if (obj is Circle circle)
                        {
                            circle.Align(alignment.Key, alignPt);
                        }
                        else if (obj is Arc arc)
                        {
                            arc.Align(alignment.Key, alignPt);
                        }
                        else if (obj is Ellipse ellipse)
                        {
                            ellipse.Align(alignment.Key, alignPt);
                        }
                        else if (obj is DBText dBText)
                        {
                            dBText.Align(alignment.Key, alignPt);
                        }
                        else if (obj is MText mText)
                        {
                            mText.Align(alignment.Key, alignPt);
                        }
                        else if (obj is BlockReference blockReference)
                        {
                            blockReference.Align(alignment.Key, alignPt);
                        }
                        else
                        {
                            obj.Align(alignment.Key, alignPt);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取对齐点
        /// </summary>
        /// <param name="alignMode">对齐方式</param>
        /// <param name="objectIds">传入对齐的物体</param>
        /// <param name="isPromptStatusOK"></param>
        /// <returns></returns>
        private Point3d GetAlignPoint(string alignMode,ObjectId[] objectIds,out bool isPromptStatusOK)
        {
            Point3d userSelectPt = Point3d.Origin;
            isPromptStatusOK = false;
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 执行对齐操作c
                List<Entity> copyEnts = new List<Entity>();
                foreach (var alignment in alignments.Where(o => o.Value == alignMode))
                {
                    foreach (var objId in objectIds)
                    {
                        var obj = acadDatabase.Element<Entity>(objId, true);
                        Entity cloneObj = obj.Clone() as Entity;
                        copyEnts.Add(cloneObj);
                        if (cloneObj is Polyline polyline)
                        {
                            polyline.Align(alignment.Key, Point3d.Origin);
                        }
                        else if (cloneObj is Circle circle)
                        {
                            circle.Align(alignment.Key, Point3d.Origin);
                        }
                        else if (cloneObj is Arc arc)
                        {
                            arc.Align(alignment.Key, Point3d.Origin);
                        }
                        else if (cloneObj is Ellipse ellipse)
                        {
                            ellipse.Align(alignment.Key, Point3d.Origin);
                        }
                        else if (cloneObj is DBText dBText)
                        {
                            dBText.Align(alignment.Key, Point3d.Origin);
                        }
                        else if (cloneObj is MText mText)
                        {
                            mText.Align(alignment.Key, Point3d.Origin);
                        }
                        else if (cloneObj is BlockReference blockReference)
                        {
                            blockReference.Align(alignment.Key, Point3d.Origin);
                        }
                        else
                        {
                            cloneObj.Align(alignment.Key, Point3d.Origin);
                        }
                    }
                }

                ThAlignDrawJig thAlignDrawJig = new ThAlignDrawJig(Point3d.Origin);
                copyEnts.ForEach(i => thAlignDrawJig.AddEntity(i));

                PromptResult pr= Active.Editor.Drag(thAlignDrawJig);
                if(pr.Status==PromptStatus.OK)
                {
                    isPromptStatusOK = true;
                    userSelectPt = thAlignDrawJig.Location.TransformBy(thAlignDrawJig.UCS.Inverse());
                }
                copyEnts.ForEach(i => i.Dispose());
            }
            return userSelectPt;
        }
    }
}
