using System;
using System.IO;
using Linq2Acad;
using DotNetARX;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionDbExtension
    {
        public static ObjectId InsertModel(this Database database, string name, Dictionary<string, string> attNameValues)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.ModelSpace.ObjectId.InsertBlockReference(
                    ThFanSelectionCommon.BLOCK_FAN_LAYER,
                    name,
                    Point3d.Origin,
                    new Scale3d(1.0),
                    0.0, 
                    attNameValues);
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

        public static void SetModelNumber(this ObjectId obj, string storey, int number)
        {
            obj.UpdateAttributesInBlock(new Dictionary<string, string>()
            {
                { ThFanSelectionCommon.BLOCK_ATTRIBUTE_STOREY_AND_NUMBER, ThFanSelectionUtils.StoreyNumber(storey, number.ToString()) }
            });
        }

        public static void SetModelIdentifier(this ObjectId obj, string identifier, int number, string style)
        {
            TypedValueList valueList = new TypedValueList
            {
                { (int)DxfCode.ExtendedDataAsciiString, identifier },
                { (int)DxfCode.ExtendedDataInteger32, number },
                { (int)DxfCode.ExtendedDataBinaryChunk,  Encoding.UTF8.GetBytes(style) },
            };
            obj.AddXData(ThFanSelectionCommon.RegAppName_FanSelection, valueList);
        }

        public static void UpdateModelIdentifier(this ObjectId obj, int number)
        {
            var oldValue = obj.GetModelNumber();
            if (oldValue > 0 && (oldValue != number))
            {
                obj.ModXData(
                    ThFanSelectionCommon.RegAppName_FanSelection,
                    DxfCode.ExtendedDataInteger32,
                    oldValue, number);
            }
        }

        public static string GetModelIdentifier(this ObjectId obj)
        {
            var valueList = obj.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
            if (valueList == null)
            {
                return string.Empty;
            }

            var values = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataAsciiString);
            if (!values.Any())
            {
                return string.Empty;
            }

            return (string)values.ElementAt(0).Value;
        }

        public static void SetModelXDataFrom(this ObjectId obj, ObjectId other)
        {
            var xdata = other.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
            obj.AddXData(ThFanSelectionCommon.RegAppName_FanSelection, xdata);
        }

        public static bool IsModel(this ObjectId obj, string identifier)
        {
            var valueList = obj.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
            if (valueList == null)
            {
                return false;
            }

            var values = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataAsciiString);
            if (!values.Any())
            {
                return false;
            }

            return (string)values.ElementAt(0).Value == identifier;
        }

        public static void SetModelName(this ObjectId obj, string name)
        {
            var dynamicProperties = obj.GetDynProperties();
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_SPECIFICATION_MODEL))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_SPECIFICATION_MODEL, name);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static string GetModelName(this ObjectId obj)
        {
            var dynamicProperties = obj.GetDynProperties();
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_SPECIFICATION_MODEL))
            {
                return dynamicProperties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_SPECIFICATION_MODEL) as string;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static string GetModelStyle(this ObjectId obj)
        {
            var valueList = obj.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
            if (valueList == null)
            {
                return string.Empty;
            }

            var values = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataBinaryChunk).First();
            return Encoding.UTF8.GetString(values.Value as byte[]);
        }

        public static int GetModelNumber(this ObjectId obj)
        {
            var valueList = obj.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
            if (valueList == null)
            {
                return 0;
            }

            var values = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger32);
            if (!values.Any())
            {
                return 0;
            }

            return (int)values.ElementAt(0).Value;
        }

        public static void ModifyModelAttributes(this ObjectId obj, Dictionary<string, string> attributes)
        {
            obj.UpdateAttributesInBlock(attributes);
        }

        public static void SetModelCustomPropertiesFrom(this ObjectId obj, DynamicBlockReferencePropertyCollection properties)
        {
            var dynamicProperties = obj.GetDynProperties();
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE1) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE1))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE1,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE1));
            }

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE2) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE2))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE2,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANGLE2));
            }

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE));
            }

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_X) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_X))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_X,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_X));
            }

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_Y) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_Y))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_Y,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_POSITION1_Y));
            }
        }

        private static string BlockDwgPath()
        {
            return Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.BLOCK_FAN_FILE);
        }
    }
}
