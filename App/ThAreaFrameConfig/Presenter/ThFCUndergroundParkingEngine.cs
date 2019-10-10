using System.Linq;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public class ThFCUndergroundParkingEngine
    {
        private readonly ThFCUnderGroundParkingSettings settings;

        // 构造函数
        public ThFCUndergroundParkingEngine(ThFCUnderGroundParkingSettings theSettings)
        {
            settings = theSettings;
        }

        // 防火分区
        public List<ThFireCompartment> Compartments
        {
            get
            {
                return settings.Compartments.Where(o => o.IsDefined).ToList();
            }
        }
    }
}
