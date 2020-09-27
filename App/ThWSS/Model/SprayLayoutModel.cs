﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThWSS.Model
{
    public class SprayLayoutModel
    {
        public LayoutWay sparyLayoutWay { get; set; }

        /// <summary>
        /// 起始最小间距
        /// </summary>
        public double sparySSpcing { get; set; }
        
        /// <summary>
        /// 起始最大间距
        /// </summary>
        public double sparyESpcing { get; set; }

        /// <summary>
        /// 距离墙/柱最小距离
        /// </summary>
        public double otherSSpcing { get; set; }

        /// <summary>
        /// 距离墙/柱最大距离
        /// </summary>
        public double otherESpcing { get; set; }

        /// <summary>
        /// 喷头类型
        /// </summary>
        public int sprayType { get; set; } 

        /// <summary>
        /// 保护半径
        /// </summary>
        public double protectRadius { get; set; }
        
        /// <summary>
        /// 是否考虑梁
        /// </summary>
        public bool UseBeam { get; set; }

        /// <summary>
        /// 梁高
        /// </summary>
        public double beamHeight { get; set; }

        /// <summary>
        /// 板厚
        /// </summary>
        public double floorThcik { get; set; }

        public static SprayLayoutModel Create()
        {
            return new SprayLayoutModel()
            {
                sparySSpcing = 300.0,
                sparyESpcing = 3600.0,
                otherSSpcing = 300.0,
                otherESpcing = 1800.0,
                sprayType = 0,
                protectRadius = 4000.0,
                beamHeight = 600.0,
                floorThcik = 100.0,
                UseBeam = true,
            };
        }
    }

    public enum LayoutWay
    {
        /// <summary>
        /// 防火分区
        /// </summary>
        fire,

        /// <summary>
        /// 已绘房间
        /// </summary>
        frame,
    }
}
