using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThResidentialStorey
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 楼层标识
        public string Identifier { get; set; }

        // 户型
        public List<ThResidentialRoom> Rooms { get; set; }
    }
}
