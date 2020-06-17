using System.Linq;
using ThCADCore.NTS;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness.SprayLayout
{
    public class SprayLayoutPostPreprocessor
    {
        public DBObjectCollection CalculateBlindRegion(Database database, Polyline region)
        {
            // 获取所有喷淋对象
            var sprays = database.SprayFromDatabase();

            // 寻找所有落在指定范围内的喷淋对象
            var theSprays = sprays.Where(o => o.IsInRegion(region)).ToList();

            // 计算这些喷淋的保护半径的集合
            var curves = new DBObjectCollection();
            foreach (var radii in theSprays.Select(o => o.Radii))
            {
                curves.Add(radii);
            }

            var field = curves.Outline();


            return null;
        }
    }
}
