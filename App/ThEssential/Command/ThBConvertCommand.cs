using System;
using AcHelper;
using Linq2Acad;
using System.IO;
using DotNetARX;
using AcHelper.Commands;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using NFox.Cad.Collections;
using GeometryExtensions;
using ThEssential.BlockConvert;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThEssential.Command
{ 
    public class ThBConvertCommand : IAcadCommand, IDisposable
    {
        public ConvertMode Mode { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mode"></param>
        public ThBConvertCommand(ConvertMode mode)
        {
            Mode = mode;
        }

        public void Dispose()
        {
            //
        }

        private string BlockDwgPath()
        {
            return Path.Combine(ThCADCommon.SupportPath(), ThBConvertCommon.BLOCK_MAP_RULES_FILE);
        }

        private ThBConvertEngine CreateConvertEngine(ConvertMode mode)
        {
            switch (mode)
            {
                case ConvertMode.STRONGCURRENT:
                    return new ThBConvertEngineStrongCurrent();
                case ConvertMode.WEAKCURRENT:
                    return new ThBConvertEngineWeakCurrent();
                default:
                    throw new NotSupportedException();
            }
        }

        public void Execute()
        {
            using (AcadDatabase currentDb = AcadDatabase.Active())
            using (ThBConvertEngine engine = CreateConvertEngine(Mode))
            using (AcadDatabase blockDb = AcadDatabase.Open(BlockDwgPath(), DwgOpenMode.ReadOnly, false))
            using (ThBConvertManager manager = ThBConvertManager.CreateManager(blockDb.Database, Mode))
            {
                // 在当前图纸中框选一个区域，获取块引用
                var extents = new Extents3d();
                var objs = new ObjectIdCollection();
                using (PointCollector pc = new PointCollector(PointCollector.Shape.Window))
                {
                    try
                    {
                        pc.Collect();
                    }
                    catch
                    {
                        return;
                    }
                    Point3dCollection winCorners = pc.CollectedPoints;
                    var filterlist = OpFilter.Bulid(o =>
                        o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(BlockReference)).DxfName);
                    var entSelected = Active.Editor.SelectCrossingWindow(winCorners[0], winCorners[1], filterlist);
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        extents.AddExtents(winCorners.ToExtents3d());
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
                //  3. 块引用是“Overlay”
                // 对于每一个Xref块引用，获取其Xref数据库
                // 一个Xref块可以有多个块引用
                var xrefs = new List<Database>();
                foreach (ObjectId obj in objs)
                {
                    var blkRef = currentDb.Element<BlockReference>(obj);
                    var blkDef = blkRef.GetEffectiveBlockTableRecord();
                    if (blkDef.IsFromExternalReference && blkDef.IsFromOverlayReference)
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
                    using (var xf = XrefFileLock.LockFile(xref.XrefBlockId))
                    {
                        xref.RestoreOriginalXrefSymbols();
                        if (Path.GetFileName(xref.OriginalFileName).StartsWith("H"))
                        {
                            foreach (var rule in manager.Rules)
                            {
                                var block = rule.Transformation.Item1;
                                foreach (ObjectId blkRef in xref.GetBlockReferences(block, extents))
                                {
                                    try
                                    {
                                        // 根据块引用的“块名”，匹配转换后的块定义的信息
                                        var blockReference = blkRef.Database.GetBlockReference(blkRef);
                                        ThBlockConvertBlock transformedBlock = null;
                                        if (Mode == ConvertMode.STRONGCURRENT)
                                        {
                                            transformedBlock = manager.TransformRule(blockReference.EffectiveName);
                                        }
                                        else if (Mode == ConvertMode.WEAKCURRENT)
                                        {
                                            transformedBlock = manager.TransformRule(
                                                blockReference.EffectiveName,
                                                blockReference.CurrentVisibilityStateValue());
                                        }
                                        else
                                        {
                                            throw new NotSupportedException();
                                        }
                                        if (transformedBlock == null)
                                        {
                                            continue;
                                        }

                                        // 在当前图纸中查找是否存在新的块定义
                                        // 若不存在，则插入新的块定义；
                                        // 若存在，则保持现有的块定义
                                        var name = (string)transformedBlock.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK];
                                        var result = currentDb.Blocks.Import(blockDb.Blocks.ElementOrDefault(name), false);

                                        // 插入新的块引用
                                        var objId = currentDb.ModelSpace.ObjectId.InsertBlockReference(
                                            "0",
                                            name,
                                            Point3d.Origin,
                                            new Scale3d(transformedBlock.Scale()),
                                            0.0,
                                            new Dictionary<string, string>());

                                        // 将新插入的块引用调整到源块引用所在的位置
                                        engine.TransformBy(objId, blockReference);

                                        // 将源块引用的属性“刷”到新的块引用
                                        engine.MatchProperties(objId, blockReference);
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        xref.RestoreForwardingXrefSymbols();
                    }
                }
            }
        }
    }
}
