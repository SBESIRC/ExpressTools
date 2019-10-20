using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}