using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ThResourceLibrary;

namespace ThElectrical.Model.ThDraw
{
    public abstract class ThDraw : ThNotifyObject
    {


        protected ThDraw(string name, ObjectId boundaryId, Point3d minPoint, Point3d maxPoint)
        {
            Name = name;
            BoundaryId = boundaryId;
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        public ObjectId BoundaryId { get; set; }//块参照的ID
        public Point3d MinPoint { get; set; }
        public Point3d MaxPoint { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }//图纸名称

    }
}
