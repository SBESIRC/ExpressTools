using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ThElectrical.Model.ThTable;

namespace ThElectrical.Model.ThDraw
{
    public class ThDistributionDraw : ThDraw
    {
        private ObservableCollection<List<ThColumn.ThColumn>> _columnGrps;//所有的列
        public ObservableCollection<List<ThColumn.ThColumn>> ColumnGrps
        {
            get
            {
                return _columnGrps;
            }
            set
            {
                _columnGrps = value;
            }
        }

        private ObservableCollection<ThCabinet> _cabinets;


        public ObservableCollection<ThCabinet> Cabinets
        {
            get
            {
                return _cabinets;
            }
            set
            {
                _cabinets = value;
                RaisePropertyChanged("Cabinets");
            }
        }//配电柜
        public ThDistributionDraw(string name, ObjectId boundaryId, Point3d minPoint, Point3d maxPoint) : base(name, boundaryId, minPoint, maxPoint)
        {
        }
    }
}
