using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.CheWei
{
    class DBTextEx : DBText
    {
        public DBTextEx(string text,Point3d point,double height,string layerName,double rotation) : base()
        {

            this.HorizontalMode = TextHorizontalMode.TextCenter;
            this.VerticalMode = TextVerticalMode.TextVerticalMid;
            this.AlignmentPoint = point;//对其点
            this.Rotation = rotation;
            //this.Position = point;
            this.Height = height;           

            this.TextString = text;
            this.Layer = layerName;

        }

        public DBTextEx(string text, Point3d point, double height, string layerName) : base()
        {

            this.HorizontalMode = TextHorizontalMode.TextCenter;
            this.VerticalMode = TextVerticalMode.TextVerticalMid;
            this.AlignmentPoint = point;//对其点
            //this.Position = point;
            this.Height = height;

            this.TextString = text;
            this.Layer = layerName;

        }
    }
}
