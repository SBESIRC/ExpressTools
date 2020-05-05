using System;
using AcHelper;
using Linq2Acad;
using AcHelper.Commands;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using GeometryExtensions;
using ThEssential.BlockConvert;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThEssential.Command
{
    public class ThBlockConvertCommand : IAcadCommand, IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 在当前图纸中框选一个区域，获取块引用
                var extents = new Extents3d();
                var objs = new ObjectIdCollection();
                using (PointCollector pc = new PointCollector(PointCollector.Shape.Window))
                {
                    Point3dCollection points = pc.Collect();
                    var filterlist = OpFilter.Bulid(o =>
                        o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(BlockReference)).DxfName);
                    var entSelected = Active.Editor.SelectWindow(points[0], points[1], filterlist);
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        extents.AddExtents(points.ToExtents3d());
                        entSelected.Value.GetObjectIds().ForEach(o => objs.Add(o));
                    }
                }
                if (objs.Count == 0)
                {
                    return;
                }

                // 过滤所选的对象
                //  1. 对象类型是块引用
                //  2. 块引用是外部引用(Xref)
                //  3. 块引用是“Attach”
                // 对于每一个Xref块引用，获取其Xref数据库
                // 一个Xref块可以有多个块引用
                var xrefs = new List<Database>();
                foreach(ObjectId obj in objs)
                {
                    var blkRef = acadDatabase.Element<BlockReference>(obj);
                    var blkDef = blkRef.GetEffectiveBlockTableRecord();
                    if (blkDef.IsFromExternalReference && !blkDef.IsFromOverlayReference)
                    {
                        // 暂时不考虑unresolved的情况
                        xrefs.Add(blkDef.GetXrefDatabase(false));
                    }
                }

                // 遍历每一个XRef的Database，在其中寻找在选择框线内特定的块引用
                // 通过查找映射表，获取映射后的块信息
                // 根据获取后的块信息，在当前图纸中创建新的块引用
                // 并将源块引用中的属性”刷“到新的块引用
                foreach (var xref in xrefs)
                {
                    foreach(var rule in ThBlockConvertManager.Instance.Rules)
                    {
                        var block = rule.Transformation.Item1;
                        foreach (ObjectId blkRef in xref.GetBlockReferences(block, extents))
                        {
                            blkRef.CreateTransformedCopy();
                        }
                    }
                }
            }
        }
    }
}
