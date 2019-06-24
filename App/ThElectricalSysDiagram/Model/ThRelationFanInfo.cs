using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace ThElectricalSysDiagram
{
    public class ThRelationFanInfo
    {
        public string LayerName { get; set; }//对应图层
        public string FanStyleName { get; set; }//风机类型
        public string FanBlockName { get; set; }//风机块名
        public BitmapImage Icon { get; set; }//块缩略图
        public string PowerInfo { get; set; }//功率

        public ThRelationFanInfo(string layerName, string fanStyleName, string fanBlockName, Bitmap icon, string powerInfo = "未知")
        {
            this.LayerName = layerName;
            this.FanStyleName = fanStyleName;
            this.FanBlockName = fanBlockName;
            if (icon != null)
            {
                this.Icon = GetImage(icon);
            }
            this.PowerInfo = powerInfo;
        }

        private BitmapImage GetImage(Bitmap b)
        {
            BitmapImage bitmap = new BitmapImage();

            //将缩略图转为字节数组
            if (b != null)
            {
                MemoryStream ms = new MemoryStream();
                b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] bytes = ms.GetBuffer();

                //将字节数组以流的形式，转为bitmapImage的流的source
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                using (Stream ms2 = new MemoryStream(bytes))
                {
                    bitmap.StreamSource = ms2;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }

            return bitmap;
        }

    }
}
