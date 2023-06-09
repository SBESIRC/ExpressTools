﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThRoof
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 编号
        public int Number { get; set; }

        // 所属类型
        public string Category { get; set; }

        // 计算系数
        public double Coefficient { get; set; }

        // 计容系数
        public double FARCoefficient { get; set; }

        // 面积框线
        public IntPtr Frame { get; set; }

        // 面积
        public double Area
        {
            get
            {
                return Frame.Area();
            }
        }

        // 状态
        public bool IsDefined
        {
            get
            {
                return Frame != (IntPtr)0;
            }
        }
    }
}
