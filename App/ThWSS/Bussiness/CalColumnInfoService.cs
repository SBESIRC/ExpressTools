using Linq2Acad;
using System.Linq;
using ThWSS.Engine;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class CalColumnInfoService
    {
        public List<Polyline> GetAllColumnInfo(Polyline room, Polyline floor)
        {
            using (var acdb = AcadDatabase.Active())
            using (var columnEngine = new ThColumnRecognitionEngine())
            {
                columnEngine.Acquire(acdb.Database, floor, new DBObjectCollection()
                {
                    room,
                });

                return columnEngine.Elements
                    .SelectMany(x => x.Properties.Values)
                    .Cast<Polyline>().ToList();
            }
        }
    }
}
