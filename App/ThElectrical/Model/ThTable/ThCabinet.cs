using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ThElectrical.Model.ThElement;

namespace ThElectrical.Model.ThTable
{
    public class ThCabinet
    {
        public ThCabinet(ThCabinetElement element)
        {
            Element = element;
        }

        public ThCabinet(ThCabinetElement element, ObservableCollection<ThCabinetRecord> records)
        {
            Element = element;
            Records = records;
        }

        public ThCabinetElement Element { get; set; }//配电箱实体
        public ObservableCollection<ThCabinetRecord> Records { get; set; }//配电箱出来的信息集合
        public Point3d TableMinPoint { get; set; }
        public Point3d TableMaxPoint { get; set; }
    }
}
