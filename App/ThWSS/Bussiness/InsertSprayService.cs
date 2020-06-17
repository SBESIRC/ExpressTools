using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThWSS.Bussiness
{
    public static class InsertSprayService
    {
        /// <summary>
        /// 插入喷淋图块
        /// </summary>
        /// <param name="insertPts"></param>
        public static void InsertSprayBlock(List<Point3d> insertPts, SprayType type)
        {
            CreateLayer("W-FRPT-SPRL", Color.FromRgb(191, 255, 0));

            string propName = "上喷";
            if (type == SprayType.SPRAYDOWN)
                propName = "下喷";
            using (var db = AcadDatabase.Active())
            {
                var filePath = Path.Combine(ThCADCommon.SupportPath(), "SprayBlockUp.dwg");
                db.Database.ImportBlocksFromDwg(filePath);
                foreach (var insertPoint in insertPts)
                {
                    var blockId = db.ModelSpace.ObjectId.InsertBlockReference("W-FRPT-SPRL", "喷头", insertPoint, new Scale3d(1, 1, 1), 0);
                    var props = blockId.GetDynProperties();
                    foreach (DynamicBlockReferenceProperty prop in props)
                    {
                        // 如果动态属性的名称与输入的名称相同
                        prop.Value = propName;
                    }
                }
            }
        }

        /// <summary>
        /// 创建新的图层
        /// </summary>
        /// <param name="allLayers"></param>
        /// <param name="aimLayer"></param>
        public static void CreateLayer(string aimLayer, Color color)
        {
            LayerTableRecord layerRecord = null;
            using (var db = AcadDatabase.Active())
            {
                foreach (var layer in db.Layers)
                {
                    if (layer.Name.Equals(aimLayer))
                    {
                        layerRecord = db.Layers.Element(aimLayer);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(aimLayer);
                    layerRecord.Color = color;
                    layerRecord.IsPlottable = false;
                }
            }
        }
    }

    // 喷淋放置的一些参数
    public enum SprayType
    {
        SPRAYUP = 0,
        SPRAYDOWN = 1,
    }
}
