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
            //***预览图的类是Bitmap,缩略图更新失败
            BitmapImage bitmap = new BitmapImage();
            //如果不是动态块、布局块，那么可以处理缩略图
            if (!(this.BtrRecord.IsAnonymous || this.BtrRecord.IsLayout))
            {
                //如果存在图片，则先删掉，永远以当前的块参照的缩略图作为显示依据
                if (File.Exists(this.IconPath))
                {
                    File.Delete(this.IconPath);
                }

                if (this.BtrRecord.PreviewIcon!=null)
                {
                    this.BtrRecord.PreviewIcon.Save(this.IconPath);
                }

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                using (Stream ms = new MemoryStream(File.ReadAllBytes(this.IconPath)))
                {
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }

            return bitmap;

        }


        //public event PropertyChangedEventHandler PropertyChanged;

        //// This method is called by the Set accessor of each property.  
        //// The CallerMemberName attribute that is applied to the optional propertyName  
        //// parameter causes the property name of the caller to be substituted as an argument.  
        //private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
