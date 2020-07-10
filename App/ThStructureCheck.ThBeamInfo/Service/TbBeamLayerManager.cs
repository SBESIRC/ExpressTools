using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;

namespace ThStructureCheck.ThBeamInfo.Service
{
    public class TbBeamLayerManager
    {
        public static string ThBeamTextLayer = "ThBeamText";
        public static string ThBeamOutlineLayer = "ThBeamOutline";
        public static string ThBeamCenterLineLayer = "ThBeamCenterLine";
        public static string ThColumnOutLineLayer = "ThColumnOutline";
        public static string ThWallOutLineLayer = "ThWallOutline";
        public static string ThBeamSegIdLayer = "ThBeamSegId";

        /// <summary>
        /// 创建需要的图层
        /// </summary>
        public static void CreateLayer()
        {
            LayerTool.CreateLayer(ThBeamTextLayer);
            LayerTool.CreateLayer(ThBeamOutlineLayer);
            LayerTool.CreateLayer(ThBeamCenterLineLayer);
            LayerTool.CreateLayer(ThColumnOutLineLayer);
            LayerTool.CreateLayer(ThWallOutLineLayer);
            LayerTool.CreateLayer(ThBeamSegIdLayer);
            
            //层关闭
            LayerTool.SetLayerOff(ThBeamSegIdLayer, true);
            LayerTool.SetLayerOff(ThBeamOutlineLayer, true);
        }        
    }
}
