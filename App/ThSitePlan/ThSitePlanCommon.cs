using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ThSitePlan
{
    public class ThSitePlanCommon
    {
        public static readonly string LAYER_FRAME = "P-AI-Frame";
        public static readonly string LAYER_LANDSCAPE_TREE = "P-Land-TREE";
        public static readonly string LAYER_STREET_TREE = "P-AI-steettree";
        public static readonly string LAYER_SITE_FIRE = "P-FIRE";
        public static readonly string LAYER_SITE_PARKING = "P-CONS-PARK";
        public static readonly string LAYER_SITE_ACTIVITY = "P-AI-activitysite";
        public static readonly string LAYER_SITE_MISCELLANEOUS = "P-AI-ground";
        public static readonly string LAYER_ROAD_INTERNAL = "P-ROAD-DRWY";
        public static readonly string LAYER_ROAD_INTERNAL_AXIS = "P-ROAD-AXIS";
        public static readonly string LAYER_ROAD_PEDESTRIAN = "P-ROAD-WALK";
        public static readonly string LAYER_ROAD_EXTERNAL = "P-OUTD-ROAD";
        public static readonly string LAYER_ROAD_LANDSCAPE = "P-Land-ROAD";
        public static readonly string LAYER_BUILD_HATCH = "P-BULD-HACH";
        public static readonly string LAYER_GLOBAL_SHADOW = "P-AI-shadow";
        public static readonly string LAYER_PAVE_OUTD = "P-AI-pavement";
        public static readonly string LAYER_GREEN_WATER = "P-GREN-WATER";
        public static readonly string LAYER_GREEN_LANDSP = "P-AI-landscape";
        // 由于方案图纸的单位是米（m），设置0.1米作为种子点的偏移量
        public static readonly double seed_point_offset = 30;
        public static readonly Tolerance global_tolerance = new Tolerance(10e-10, 10e-10);
        // Hatch
        public static readonly int hatch_color_index = 9;
        public static readonly string hatch_pattern = "Solid";
        // Plant
        public static readonly int plant_interval = 5;
        public static readonly double plant_radius = 5.0;
        public static readonly double plant_interval_distance = 10.0;
        // Frame
        public static readonly double frame_annotation_offset_X = 0.0;
        public static readonly double frame_annotation_offset_Y = 15.0;
        // RegAppName
        public static readonly string RegAppName_ThSitePlan_Frame_Name = "THCAD_SP_FRAME_NAME";

        //photoshop文档尺寸
        public static readonly Dictionary<string, double> PsDocOpenPropertity = new Dictionary<string, double>
        {
            {"DocWidth", 41.84},
            {"DocHight", 59.17},
            {"PPI", 300},
        };
    }
}
