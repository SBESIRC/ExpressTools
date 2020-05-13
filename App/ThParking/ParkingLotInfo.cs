using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

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
                    bitmap = BtrRecord.PreviewIcon.Bitmap2BitmapImage();
                }
            }
            return bitmap;
        }
    }
}
