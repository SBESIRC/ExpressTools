using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Internal;

namespace ThElectrical.Model.ThDraw
{
    public class ThDrawFactory
    {

        public const string drawNameFieldName = "Drawing Title";//图纸名称字段名
        public const double drawNameDis = 6000;//图纸名称与字段的间距
        public const string distributionDrawName = "*配电箱系统图*";
        public const string cabinetDrawName = "*低压配电柜系统图*";
        public const string mainlineDrawName = "*配电干线系统图*";

        public static List<string> drawNames = new List<string> { distributionDrawName, cabinetDrawName, mainlineDrawName };

        public static ThDraw CreateDraw(string type, ObjectId id, Point3d pt1, Point3d pt2)
        {
#if ACAD2016
            if (Utils.WcMatchEx(type, distributionDrawName, false))
            {
                return new ThDistributionDraw(type, id, pt1, pt2);
            }
            else if (Utils.WcMatchEx(type, cabinetDrawName, false))
            {
                return new ThCabinetDraw(type,id, pt1, pt2);
            }
            else if (Utils.WcMatchEx(type, mainlineDrawName, false))
            {
                return new ThMainlineDraw(type,id, pt1, pt2);
            }
            else
            {
                return null;
            }
#else
            if (Utils.WcMatch(type, distributionDrawName))
            {
                return new ThDistributionDraw(type, id, pt1, pt2);
            }
            else if (Utils.WcMatch(type, cabinetDrawName))
            {
                return new ThCabinetDraw(type,id, pt1, pt2);
            }
            else if (Utils.WcMatch(type, mainlineDrawName))
            {
                return new ThMainlineDraw(type,id, pt1, pt2);
            }
            else
            {
                return null;
            }
#endif
        }
    }
}
