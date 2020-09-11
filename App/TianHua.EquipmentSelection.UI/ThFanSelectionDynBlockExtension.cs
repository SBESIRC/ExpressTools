using System;
using Linq2Acad;
using DotNetARX;
using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionDynBlockExtension
    {
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

        public static void SetModelTextHeight(this ObjectId obj)
        {
            var dynamicProperties = obj.GetDynProperties();
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_MODEL_TEXT_HEIGHT))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_MODEL_TEXT_HEIGHT, 375);
            }
            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANNOTATION_TEXT_HEIGHT))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANNOTATION_TEXT_HEIGHT, 375);
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

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE1) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE1))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE1,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE1));
            }

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE2) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE2))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE2,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ROTATE2));
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

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_MODEL_TEXT_HEIGHT) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_MODEL_TEXT_HEIGHT))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_MODEL_TEXT_HEIGHT,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_MODEL_TEXT_HEIGHT));
            }

            if (dynamicProperties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANNOTATION_TEXT_HEIGHT) &&
                properties.Contains(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANNOTATION_TEXT_HEIGHT))
            {
                dynamicProperties.SetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANNOTATION_TEXT_HEIGHT,
                    properties.GetValue(ThFanSelectionCommon.BLOCK_DYNAMIC_PROPERTY_ANNOTATION_TEXT_HEIGHT));
            }
        }
    }
}
