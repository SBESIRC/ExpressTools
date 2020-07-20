using System.IO;
using Linq2Acad;
using DotNetARX;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionDbExtension
    {
        public static ObjectId InsertModel(this Database database, string name)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.ModelSpace.ObjectId.InsertBlockReference(
                    ThFanSelectionCommon.BLOCK_FAN_LAYER,
                    name,
                    Point3d.Origin,
                    new Scale3d(1.0),
                    0.0);
            }
        }

        public static void ImportModel(this Database database, string name)
        {
            using (AcadDatabase currentDb = AcadDatabase.Use(database))
            using (AcadDatabase blockDb = AcadDatabase.Open(BlockDwgPath(), DwgOpenMode.ReadOnly, false))
            {
                currentDb.Blocks.Import(blockDb.Blocks.ElementOrDefault(name), false);
                currentDb.Layers.Import(blockDb.Layers.ElementOrDefault(ThFanSelectionCommon.BLOCK_FAN_LAYER), false);
            }
        }

        public static void AttachModel(this ObjectId obj, string identifier, int number)
        {
            TypedValueList valueList = new TypedValueList
            {
                { (int)DxfCode.ExtendedDataAsciiString, identifier },
                { (int)DxfCode.ExtendedDataInteger32, number }
            };
            obj.AddXData(ThFanSelectionCommon.RegAppName_FanSelection, valueList);
        }

        public static void MatchProperties(this ObjectId obj, FanDataModel model)
        {
            //
        }

        /// <summary>
        /// 提取块引用中的模型信息（模型标识和模型编号）
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static Dictionary<string, List<int>> ExtractModels(this ObjectIdCollection objs)
        {
            var models = new Dictionary<string, List<int>>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (ObjectId obj in objs)
                {
                    TypedValueList valueList = obj.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
                    if (valueList != null)
                    {
                        // 模型ID
                        string identifier = null;
                        var values = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataAsciiString);
                        if (values.Any())
                        {
                            identifier = (string)values.ElementAt(0).Value;
                        }

                        // 模型编号
                        int number = 0;
                        values = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger32);
                        if (values.Any())
                        {
                            number = (int)values.ElementAt(0).Value;
                        }

                        if (!string.IsNullOrEmpty(identifier))
                        {
                            if (models.ContainsKey(identifier))
                            {
                                models[identifier].Add(number);
                            }
                            else
                            {
                                models.Add(identifier, new List<int>()
                                {
                                    number
                                });
                            }
                        }
                    }
                }
            }
            return models;
        }

        private static string BlockDwgPath()
        {
            return Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.BLOCK_FAN_FILE);
        }
    }
}
