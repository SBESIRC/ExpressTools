using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    // 附属公建
    public struct PublicBuildingUnit
    {
        public string type;
        public string areaType;
        public string category;
        public string attribute;
        public string areaRatio;
        public string densityRatio;
        public string parkings;
        public string floors;
        public string publicAreaID;
        public string version;

        public static PublicBuildingUnit CreateWithLayer(string name)
        {
            string[] tokens = name.Split('_');
            PublicBuildingUnit unit = new PublicBuildingUnit()
            {
                type = tokens[0],
                areaType = tokens[1],
                category = tokens[2],
                attribute = tokens[3],
                areaRatio = tokens[4],
                densityRatio = tokens[5],
                parkings = tokens[6],
                floors = tokens[7],
                publicAreaID = tokens[8],
                version = tokens[9]
            };
            return unit;
        }
    }
}
