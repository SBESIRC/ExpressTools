﻿using System;
using System.Collections.Generic;
using AcHelper;
using Linq2Acad;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThMirror
{
    public class ThMirrorDocumentReactor
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorDocumentReactor instance = new ThMirrorDocumentReactor();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorDocumentReactor() { }
        internal ThMirrorDocumentReactor() { }
        public static ThMirrorDocumentReactor Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public void SubscribeToDoc(Document d)
        {
            d.CommandWillStart += Document_CommandWillStart;
            d.CommandEnded += Document_CommandEnded;
            d.CommandCancelled += Document_CommandCancelled;
            d.CommandFailed += Documet_CommandFailed;
        }

        public void UnsubscribeToDoc(Document d)
        {
            d.CommandWillStart -= Document_CommandWillStart;
            d.CommandEnded -= Document_CommandEnded;
            d.CommandCancelled -= Document_CommandCancelled;
        }

        private void Document_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                ThMirrorEngine.Instance.Start();
            }
        }

        private void Document_CommandEnded(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach(var mirrorData in ThMirrorEngine.Instance.Targets)
                    {
                        // 创建新的块定义
                        var blockName = mirrorData.blockRefenceId.NextMirroredBlockName();
                        var blockEntities = new List<Entity>();
                        foreach(DBObject dbObj in mirrorData.blockEntities)
                        {
                            if (dbObj is Entity entity)
                            {
                                blockEntities.Add(entity);
                            }
                        }
                        var blockId = acadDatabase.Database.AddBlockTableRecord(blockName, blockEntities);

                        // 插入新的块引用
                        var blockReference = acadDatabase.Element<BlockReference>(mirrorData.blockRefenceId);
                        blockReference.OwnerId.InsertBlockReference(blockReference.Layer,
                            blockName,
                            Point3d.Origin,
                            new Scale3d(1),
                            0.0);

                        // 删除旧的块引用
                        acadDatabase.Element<BlockReference>(mirrorData.blockRefenceId, true).Erase();
                    };

                    // 镜像命令结束后，“清零”所有数据
                    ThMirrorEngine.Instance.Sources.Clear();
                    ThMirrorEngine.Instance.Targets.Clear();
                }

                ThMirrorEngine.Instance.Stop();
                Instance.UnsubscribeToDoc(Active.Document);
            }
        }

        private void Document_CommandCancelled(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                ThMirrorEngine.Instance.Stop();
                Instance.UnsubscribeToDoc(Active.Document);
            }
        }

        private void Documet_CommandFailed(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                ThMirrorEngine.Instance.Stop();
                Instance.UnsubscribeToDoc(Active.Document);
            }
        }
    }
}
