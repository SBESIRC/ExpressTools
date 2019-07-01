using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThElectricalSysDiagram
{
    public class ThBlockInfo
    {
        public string Name { get; set; }//普通名称
        public string RealName { get; set; }//真实名称
        //public string IconPath { get; set; }//车位缩略图路径
        //public BlockTableRecord BtrRecord { get; set; }//块表记录
        //public BlockTableRecord NormalBtrRecord { get; set; }//普通块的块表记录
        public BitmapImage Icon { get; set; }//块缩略图


        public ThBlockInfo(string name, string realName, Bitmap icon)
        {
            this.Name = name;
            //获取真实块名
            this.RealName = realName;
            if (icon != null)
            {
                this.Icon = GetImage(icon);
            }
        }

        public ThBlockInfo(string realName, Bitmap icon)
        {
            //获取真实块名
            this.RealName = realName;
            if (icon != null)
            {
                this.Icon = GetImage(icon);
            }
        }


        /// <summary>
        /// 创建可释放资源的bitmap
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private BitmapImage GetImage(Bitmap b)
        {
            BitmapImage bitmap = new BitmapImage();

            ////如果是布局块不处理
            //if (!(this.BtrRecord.IsLayout))
            //{
            //    //Bitmap b = null;
            ////如果是匿名块，进行特殊处理，获取其对应的有名块的截图
            //if (this.BtrRecord.IsAnonymous)
            //{
            //    b = GetRealIcon();
            //}
            //else
            //{
            //    if (this.BtrRecord.PreviewIcon != null)
            //    {
            //        b = this.BtrRecord.PreviewIcon;
            //    }
            //}

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


            //}
            return bitmap;
        }


        /// <summary>
        /// 对于设置了可见性的块，找到真正的缩略图
        /// </summary>
        /// <returns></returns>
        public Bitmap GetRealIcon()
        {
            Bitmap bm = null;
            using (var db = AcadDatabase.Active())
            {
                //从普通块定义中找到和它名字相同的，找出那个缩略图
                var btr = db.Blocks
                                 .OfType<BlockTableRecord>()
                                 .FirstOrDefault(b => b.Name == this.RealName);

                //找到了再继续
                if (btr != null)
                {
                    //有缩略图再继续
                    if (btr.PreviewIcon != null)
                    {
                        bm = btr.PreviewIcon;
                    }
                }
            }

            return bm;
        }

    }

}
