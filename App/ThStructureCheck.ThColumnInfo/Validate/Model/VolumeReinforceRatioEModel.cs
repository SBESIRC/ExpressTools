﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class VolumeReinforceRatioEModel : ValidateModel
    {
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
                !IsContainsCodeSign(new List<string> { "LZ", "KZ", "ZHZ" })
                || Cdm == null)
            {
                return false;
            }
            return true;
        }
    }
}