using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThWSS.Model
{
    public class SprayLayoutModel
    {
        public LayoutWay sparyLayoutWay { get; set; }

        public double sparySSpcing { get; set; }

        public double sparyESpcing { get; set; }

        public double otherSSpcing { get; set; }

        public double otherESpcing { get; set; }

        /// <summary>
        /// 是否考虑梁
        /// </summary>
        public bool UseBeam { get; set; }

        public static SprayLayoutModel Create()
        {
            return new SprayLayoutModel()
            {
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
        /// 来自框线
        /// </summary>
        frame,

        /// <summary>
        /// 自定义区域
        /// </summary>
        customPart
    }
}
