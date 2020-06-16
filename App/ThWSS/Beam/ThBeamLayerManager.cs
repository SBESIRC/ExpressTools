using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Beam
{
    public class ThBeamLayerManager
    {
        public static List<string> GeometryLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers.Where(o =>
                {
                    // 图层名未包含S_BEAM
                    if (!o.Name.Contains("S_BEAM"))
                    {
                        return false;
                    }

                    // 若图层名包含S_BEAM，
                    // 则继续判断是否包含TEXT
                    if (o.Name.Contains("TEXT"))
                    {
                        return false;
                    }

                    // 继续判断是否包含REIN
                    if (o.Name.Contains("REIN"))
                    {
                        return false;

                    }

                    // 返回指定的图层
                    return true;
                }).ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }

        public static List<string> AnnotationLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers.Where(o =>
                {
                    if (o.Name.Contains("S_BEAM_TEXT"))
                    {
                        return true;
                    }

                    if (o.Name.Contains("S_BEAM_SECD_TEXT"))
                    {
                        return true;
                    }

                    if (o.Name.Contains("S_BEAM_XL_TEXT"))
                    {
                        return true;
                    }

                    if (o.Name.Contains("S_BEAM_WALL_TEXT"))
                    {
                        return true;
                    }

                    return false;
                }).ForEachDbObject(o => layers.Contains(o.Name));
                return layers;
            }
        }
    }
}
