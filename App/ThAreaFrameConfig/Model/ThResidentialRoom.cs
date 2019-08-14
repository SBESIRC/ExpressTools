using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThResidentialRoom
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 楼层ID
        [Display(AutoGenerateField = false)]
        public Guid StoreyID { get; set; }

        // 户型名
        public string Name { get; set; }

        // 户型标识
        public string Identifier { get; set; }


        private double ComponentArea(string name)
        {
            double area = 0.0;
            var component = Components.Where(o => o.Name == name).First();
            foreach (var frame in component.AreaFrames)
            {
                area += frame.Area;
            }
            return area;
        }

        // 套内面积
        public double DwellingArea {
            get
            {
                return ComponentArea("套内");
            }
        }

        // 阳台面积
        public double BalconyArea {
            get
            { 
                return ComponentArea("阳台");
            }
        }

        // 飘窗面积
        public double BaywindowArea {
            get
            {
                return ComponentArea("飘窗");
            }
        }

        // 其他面积
        public double MiscellaneousArea {
            get
            {
                return ComponentArea("其他构件");
            }
        }

        // 合计面积
        public double AggregationArea {
            get
            {
                return DwellingArea + BalconyArea + BaywindowArea + MiscellaneousArea;
            }
        }

        // 组件
        public List<ThResidentialRoomComponent> Components { get; set; }
    }
}
