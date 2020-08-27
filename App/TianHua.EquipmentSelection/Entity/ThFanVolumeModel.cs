﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TianHua.FanSelection.Model
{
    [KnownType(typeof(FireFrontModel))]
    [KnownType(typeof(FontroomNaturalModel))]
    [KnownType(typeof(FontroomWindModel))]
    [KnownType(typeof(RefugeFontRoomModel))]
    [KnownType(typeof(RefugeRoomAndCorridorModel))]
    [KnownType(typeof(StaircaseAirModel))]
    [KnownType(typeof(StaircaseNoAirModel))]
    public abstract class ThFanVolumeModel
    {
        public abstract string FireScenario { get; }

        public double QueryValue { get; set; }
    }
}