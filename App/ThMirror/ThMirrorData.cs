using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThMirror
{
    public class ThMirrorData
    {
        // 块引用所在的图层
        public ObjectId layerId;

        // 块引用所对应的块
        public ObjectId blockId;

        // 镜像后创建的块
        public ObjectId mirroredBlockId;

        // MCS to Owner Block Space 变换
        public Matrix3d blockTransform;

        //  MCS to WCS 变换
        public Matrix3d nestedBlockTransform;

        // 块引用子实体集合（不包含嵌套块）
        public DBObjectCollection blockEntities;

        // 嵌套块
        public List<ThMirrorData> nestedBlockReferences;


        // ThMirrorData记录块引用的嵌套结构
        // nestedBlockTransform记录块引用相对于WCS的变换
        // Entity.Explode()将子实体置于WCS中，我们需要使用nestedBlockTransform将新创建的块引用转换到WCS
        // 在根据新创建的块引用创建新的块定义时，我们需要使用nestedBlockTransform.Inverse()将块定义的子实体转换到MCS
        // 这样新创建的块定义将保持原有的插入点
        //  https://spiderinnet1.typepad.com/blog/2013/10/autocad-net-matrix-transformations-recreate-exact-geometries-of-insert-blockreference.html
        //  https://spiderinnet1.typepad.com/blog/2013/10/recursively-recreate-blockreference-geometries-with-proper-matrix-transformations-regardless-of-insert-nested-levels.html
        //  https://spiderinnet1.typepad.com/blog/2014/02/autocad-net-add-entity-in-model-space-to-block.html
        public ThMirrorData(BlockReference blockReference, Matrix3d mat)
        {
            blockEntities = new DBObjectCollection();
            nestedBlockReferences = new List<ThMirrorData>();

            layerId = blockReference.LayerId;
            mirroredBlockId = ObjectId.Null;
            blockId = blockReference.BlockTableRecord;
            blockTransform = blockReference.BlockTransform;
            nestedBlockTransform = blockTransform.PreMultiplyBy(mat);
            using (DBObjectCollection entitySet = new DBObjectCollection())
            {
                blockReference.Burst(entitySet);
                foreach (DBObject dbObj in entitySet)
                {
                    if (dbObj is BlockReference nestedBlockReference)
                    {
                        // 嵌套块引用
                        if (nestedBlockReference.IsBlockReferenceContainText())
                        {
                            nestedBlockReferences.Add(new ThMirrorData(nestedBlockReference, nestedBlockTransform));
                        }
                        else
                        {
                            blockEntities.Add(dbObj);
                        }
                    }
                    else if (dbObj is Entity nestedEntity)
                    {
                        // 收录到子实体集合
                        blockEntities.Add(nestedEntity);
                    }
                    else
                    {
                        // 不支持非图形实体
                        throw new NotSupportedException();
                    }
                }
            }
        }
    }
}
