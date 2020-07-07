using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;


namespace ThWSS.Engine
{
    public class ThAreaOutlineLayerManager
    {
        public static List<string> GeometryLayers(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var layers = new List<string>();
                acadDatabase.Layers.Where(o =>
                {
                    // 图层名未包含AD-AREA-OUTL
                    if (!o.Name.ToUpper().Contains("AD-AREA-OUTL"))
                    {
                        return false;
                    }

                    // 返回指定的图层
                    return true;
                }).ForEachDbObject(o => layers.Add(o.Name));
                return layers;
            }
        }
    }
}
