using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using DotNetARX;

namespace TianHua.AutoCAD.Parking
{
    public class ParkingLotInfo
    {
        public string Name { get; set; }//车位名称
        public string IconPath { get; set; }//车位缩略图路径
        public BlockTableRecord BtrRecord { get; set; }//车位块表记录
        public BitmapImage Icon { get; set; }//车位缩略图


        public ParkingLotInfo(string name, string iconPath, BlockTableRecord btrRecord)
        {
            this.Name = name;
            this.IconPath = iconPath;
            this.BtrRecord = btrRecord;
            this.Icon = GetImage();

        }

        //public ParkingLotInfo(string name, string iconPath)
        //{
        //    this.Name = name;
        //    this.IconPath = iconPath;

        //    this.Icon = GetImage();
        //}

        /// <summary>
        /// 创建可释放资源的bitmap
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private BitmapImage GetImage()
        {
            BitmapImage bitmap = new BitmapImage();

            if (!(this.BtrRecord.IsAnonymous || this.BtrRecord.IsLayout))
            {
                if (this.BtrRecord.PreviewIcon != null)
                {
                    //将缩略图转为字节数组
                    Bitmap b = this.BtrRecord.PreviewIcon;
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
            }
            return bitmap;
        }

    }
}
