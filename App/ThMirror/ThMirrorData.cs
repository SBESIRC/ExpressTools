using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThMirror
{
    public class ThMirrorData
    {
        // 块引用
        public ObjectId blockRefenceId;

        // 块引用所在的图层
        public ObjectId layerId;

        // 块引用所在的块
        public ObjectId ownerId;

        // MCS to WCS 变换
        public Matrix3d blockTransform;

        // 块引用子实体集合
        public DBObjectCollection blockEntities;

        public ThMirrorData(BlockReference blockReference)
        {
            ownerId = blockReference.OwnerId;
            layerId = blockReference.LayerId;
            blockRefenceId = blockReference.ObjectId;
            blockTransform = blockReference.BlockTransform;
            blockEntities = new DBObjectCollection();
            blockReference.Explode(blockEntities);
        }
    }
}
