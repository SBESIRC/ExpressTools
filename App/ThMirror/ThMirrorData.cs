using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using DotNetARX;

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


        // ThMirrorData记录块引用的嵌套结构，暂时不记录块定义的插入点
        // Entity.Explode()将子实体置于WCS中，所以后面还原的块定义的插入点都是原点
        public ThMirrorData(BlockReference blockReference, Matrix3d mat)
        {
            blockEntities = new DBObjectCollection();
            nestedBlockReferences = new List<ThMirrorData>();

            layerId = blockReference.LayerId;
            mirroredBlockId = ObjectId.Null;
            blockId = blockReference.BlockTableRecord;
            blockTransform = blockReference.BlockTransform;
            nestedBlockTransform = blockTransform.PreMultiplyBy(mat);
            DBObjectCollection entitySet = new DBObjectCollection();
            blockReference.Explode(entitySet);
            foreach (DBObject dbObj in entitySet)
            {
                if (dbObj is BlockReference nestedBlockReference)
                {
                    // 嵌套块引用
                    if (nestedBlockReference.IsBlockReferenceContainText())
                    {
                        nestedBlockReferences.Add(new ThMirrorData(nestedBlockReference, mat));
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
