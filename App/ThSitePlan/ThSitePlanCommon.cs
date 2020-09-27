using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ThSitePlan
{
    public class ThSitePlanCommon
    {
        public static readonly string LAYER_ZERO = "0";
        public static readonly string LAYER_FRAME = "P-AI-Frame";
        public static readonly string LAYER_LANDSCAPE_TREE = "P-Land-TREE";
        public static readonly string LAYER_STREET_TREE = "P-AI-steettree";
        public static readonly string LAYER_SITE_FIRE = "P-FIRE";
        public static readonly string LAYER_SITE_FENCE = "P-CONS-FENC";
        public static readonly string LAYER_SITE_PARKING = "P-CONS-PARK";
        public static readonly string LAYER_SITE_ACTIVITY = "P-AI-activitysite";
        public static readonly string LAYER_SITE_MISCELLANEOUS = "P-AI-ground";
        public static readonly string LAYER_ROAD_INTERNAL = "P-ROAD-DRWY";
        public static readonly string LAYER_ROAD_INTERNAL_AXIS = "P-ROAD-AXIS";
        public static readonly string LAYER_ROAD_PEDESTRIAN = "P-ROAD-WALK";
        public static readonly string LAYER_ROAD_EXTERNAL = "P-OUTD-ROAD";
        public static readonly string LAYER_ROAD_LANDSCAPE = "P-Land-ROAD";
        public static readonly string LAYER_BUILD_HATCH = "P-BUID-HACH";
        public static readonly string LAYER_BUILD_OUT = "P-OUTD-BUID";
        public static readonly string LAYER_GLOBAL_SHADOW = "P-AI-shadow";
        public static readonly string LAYER_PAVE_OUTD = "P-AI-pavement";
        public static readonly string LAYER_GREEN_WATER = "P-GREN-WATER";
        public static readonly string LAYER_GREEN_LANDSP = "P-AI-landscape";
        public static readonly string LAYER_TREE = "P-AI-tree";
        // 由于方案图纸的单位是米（m），设置0.1米作为种子点的偏移量
        public static readonly double seed_point_offset = 30;
        public static readonly double overkill_tolerance = 0.001;
        public static readonly Tolerance point_tolerance = new Tolerance(10e-10, 10e-8);
        public static readonly Tolerance global_tolerance = new Tolerance(10e-10, 10e-10);
        // Hatch
        public static readonly int hatch_color_index = 9;
        public static readonly string hatch_pattern = "Solid";
        // Frame
        public static readonly double frame_annotation_offset_X = 0.0;
        public static readonly double frame_annotation_offset_Y = 15.0;
        // Region
        public static readonly double hatch_density_ratio = 0.4;
        public static readonly double hatch_area_threshold = 400;
        // RegAppName
        public static readonly string RegAppName_ThSitePlan_Frame_Name = "THCAD_SP_FRAME_NAME";

        //FrameName
        public static readonly string ThSitePlan_Frame_Name_Unused = "天华彩总";
        public static readonly string ThSitePlan_Frame_Name_Unrecognized = "未识别对象";
        public static readonly string ThSitePlan_Frame_Name_Original = "天华彩总原始图框";

        // XRecord
        public static readonly string Configuration_Xrecord_Name = "THCAD_THSITEPLAN_CONFIG";

        //RoadCenterLine
        public static readonly string LAYER_RoadCenterLine = "P-TRAF-CITY";

        //文件保存路径名
        public static readonly string ThSitePlan_File_Save_Path = "一键彩总PDF图纸文件";
    }
}
