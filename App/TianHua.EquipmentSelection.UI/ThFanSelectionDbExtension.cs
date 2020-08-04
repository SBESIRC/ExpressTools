using System;
using System.IO;
using Linq2Acad;
using DotNetARX;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using System.Text;

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
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY, name);
            }
            else if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY2))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY2, name);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static string GetModelName(this ObjectId obj)
        {
            var dynamicProperties = obj.GetDynProperties();
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY))
            {
                return dynamicProperties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY) as string;
            }
            else if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY2))
            {
                return dynamicProperties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_VISIBILITY2) as string;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static string HTFCModelName(this ObjectId obj, string modelNumber, string form)
        {
            var blockReference = new ThFSBlockReference(obj);
            var visibilityStates = blockReference.DynablockVisibilityStates();
            var result = visibilityStates.Where(o => o.Key.Contains(modelNumber) && o.Key.Contains(form));
            if (result.Count() == 1)
            {
                return result.First().Key;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static string AXIALModelName(this ObjectId obj, string modelName)
        {
            var blockReference = new ThFSBlockReference(obj);
            var visibilityStates = blockReference.DynablockVisibilityStates();
            var result = visibilityStates.Where(o => ThFanSelectionUtils.MatchModelName(modelName, o.Key));
            if (result.Count() == 1)
            {
                return result.First().Key;
            }
            else
            {
                throw new ArgumentException();
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

        private static string BlockDwgPath()
        {
            return Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.BLOCK_FAN_FILE);
        }
    }
}
