using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ThSitePlan
{
    public class ColorGeneralDataModel
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 父ID
        /// </summary>
        public string PID { get; set; }

        /// <summary>
        /// 图层名称
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 图层类型:0图层  1群组
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// PSD颜色
        /// </summary>
        public string PSD_Color { get; set; }


        /// <summary>
        /// PSD透明
        /// </summary>
        public int PSD_Transparency { get; set; }

        /// <summary>
        /// CAD图框
        /// </summary>
        public string CAD_Frame { get; set; }

        /// <summary>
        /// CAD图层
        /// </summary>
        public LayerDataModel CAD_Layer { get; set; }


        ///// <summary>
        ///// CAD图层选择
        ///// </summary>
        //public Bitmap CAD_SelectImg { get; set; }

        /// <summary>
        /// CAD脚本
        /// </summary>
        public string CAD_Script { get; set; }


        /// <summary>
        /// 数据类型：0 Sys,1 User
        /// </summary>
        public string DataType { get; set; }


        /// <summary>
        /// CAD图层选择
        /// </summary>
        public Bitmap CAD_SelectImg
        {
            get
            {
                return this.GetImg();
            }

        }

        private Bitmap GetImg()
        {
            if (Type == "1")
            {
                return Properties.Resources.空白32x32;
            }
            else
            {
                return Properties.Resources._24;
            }
        }


    }
}
