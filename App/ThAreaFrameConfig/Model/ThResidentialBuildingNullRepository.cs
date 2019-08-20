﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThResidentialBuildingNullRepository : IThResidentialBuildingRepository
    {
        private ThResidentialBuilding building;

        public ThResidentialBuilding Building
        {
            get
            {
                return building;
            }
        }

        public ThResidentialBuildingNullRepository()
        {
            building = new ThResidentialBuilding()
            {
                Number = "",
                Name = "",
                Category = "住宅",
                AboveGroundFloorNumber = "",
                UnderGroundFloorNumber = "",
                Layer = "",
            };
        }
    }
}