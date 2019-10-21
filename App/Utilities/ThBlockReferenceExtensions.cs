using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ThBlockReferenceExtensions
    {
        public static void DecomposeBlockTransform(this Matrix3d blockTransform, 
            out Point3d insertPt,
            out double rotation,
            out Scale3d scale)
        {
            double[] dims = blockTransform.ToArray();
            double[] line1 = new[] { dims[0], dims[1], dims[2], dims[3] };
            double[] line2 = new[] { dims[4], dims[5], dims[6], dims[7] };
            double[] line3 = new[] { dims[8], dims[9], dims[10], dims[11] };
            double[] line4 = new[] { dims[12], dims[13], dims[14], dims[15] };

            insertPt = new Point3d(line1[3], line2[3], line3[3]);
            rotation = Math.Atan(line2[0] / line1[0]);
            scale = new Scale3d(
                line1[0] / Math.Cos(rotation),
                line2[1] / Math.Cos(rotation),
                line3[2]);
        }

        public static BlockReference CreateBlockReference(this ObjectId spaceId, string layer, string blockName, Point3d position, Scale3d scale, double rotateAngle)
        {
            Database db = spaceId.Database;//获取数据库对象
            //以读的方式打开块表
            BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            //如果没有blockName表示的块，则程序返回
            if (!bt.Has(blockName)) return null;
            //以写的方式打开空间（模型空间或图纸空间）
            BlockTableRecord space = (BlockTableRecord)spaceId.GetObject(OpenMode.ForWrite);
            //创建一个块参照并设置插入点
            BlockReference br = new BlockReference(position, bt[blockName]);
            br.ScaleFactors = scale;//设置块参照的缩放比例
            br.Layer = layer;//设置块参照的层名
            br.Rotation = rotateAngle;//设置块参照的旋转角度
            ObjectId btrId = bt[blockName];//获取块表记录的Id
            //打开块表记录
            BlockTableRecord record = (BlockTableRecord)btrId.GetObject(OpenMode.ForRead);
            //添加可缩放性支持
            if (record.Annotative == AnnotativeStates.True)
            {
                ObjectContextCollection contextCollection = db.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                ObjectContexts.AddContext(br, contextCollection.GetContext("1:1"));
            }
            return br;
        }

        /// <summary>
        ///Created thanks to the help of many @the Swamp.org.
        ///Intended to change the displayed insertion point of simple 2D blocks in ModelSpace(not sure how it affects dynamic or more complex blocks).
        ///Moves entities contained in block reference to center on the insertion/position point.
        ///but can be used to set the insertion point to any location desired.
        ///*Caution*, this will change the appearance of drawings where the insertion point is away from the displayed entities.
        /// </summary>
        // http://www.theswamp.org/index.php?topic=31859.msg525789#msg525789
        public static void ChangeInsertPoint()
        {
            Document document = AcadApp.DocumentManager.MdiActiveDocument;
            Editor editor = document.Editor;
            Database database = document.Database;

            using (document.LockDocument())
            {
                PromptEntityOptions options = new PromptEntityOptions("\nPick a block: ");
                options.SetRejectMessage("\nMust be a block reference: ");
                options.AddAllowedClass(typeof(BlockReference), true);

                Utils.PostCommandPrompt();
                PromptEntityResult result = editor.GetEntity(options);  //select a block reference in the drawing.

                if (result.Status == PromptStatus.OK)
                {
                    using (Transaction transaction = database.TransactionManager.StartTransaction())
                    {
                        BlockReference reference = (BlockReference)transaction.GetObject(result.ObjectId, OpenMode.ForRead);
                        BlockTableRecord record = (BlockTableRecord)transaction.GetObject(reference.BlockTableRecord, OpenMode.ForRead);

                        Point3d refPos = reference.Position;                           //current position of inserted block ref.                        
                        Point3d pmin = reference.Bounds.Value.MinPoint;                //bounding box of entities.
                        Point3d pmax = reference.Bounds.Value.MaxPoint;
                        Point3d newPos = (Point3d)(pmin + (pmax - pmin) / 2);          //center point of displayed graphics.

                        Vector3d vec = newPos.GetVectorTo(refPos);                     //apply your own desired points here.                  
                        vec = vec.TransformBy(reference.BlockTransform.Transpose());   //
                        Matrix3d mat = Matrix3d.Displacement(vec);                     //     


                        foreach (ObjectId eid in record)                               //update entities in the table record.
                        {
                            Entity entity = (Entity)transaction.GetObject(eid, OpenMode.ForRead) as Entity;

                            if (entity != null)
                            {
                                entity.UpgradeOpen();
                                entity.TransformBy(mat);
                                entity.DowngradeOpen();
                            }
                        }

                        ObjectIdCollection blockReferenceIds = record.GetBlockReferenceIds(false, false); //get all instances of same block ref.

                        foreach (ObjectId eid in blockReferenceIds)              //update all block references of the block modified.
                        {
                            BlockReference BlkRef = (BlockReference)transaction.GetObject(eid, OpenMode.ForWrite);

                            // BlkRef.TransformBy(mat.Inverse());  // include this line if you want block ref to stay in original location in dwg.

                            BlkRef.RecordGraphicsModified(true);
                        }

                        transaction.TransactionManager.QueueForGraphicsFlush();  //

                        editor.WriteMessage("\nInsertion points modified.");     //                               

                        transaction.Commit();                                    //                        
                    }
                }
                else
                {
                    editor.WriteMessage("Nothing picked: *" + result.Status + "*");
                }
            }
            AcadApp.UpdateScreen();
            Utils.PostCommandPrompt();
        }
    }
}
