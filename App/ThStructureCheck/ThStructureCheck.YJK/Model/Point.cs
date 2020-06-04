﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class Point
    {
        private double x;
        private double y;
        private double z;
        public double X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
        public double Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
            }
        }
        public Point(double x,double y,double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;                
        }
        /// <summary>
        /// 把Z重置零
        /// </summary>
        public void ResetZ()
        {
            this.z = 0.0;
        }
    }
}
