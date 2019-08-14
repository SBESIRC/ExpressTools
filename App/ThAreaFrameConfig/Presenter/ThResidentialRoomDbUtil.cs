using DotNetARX;
using Linq2Acad;
using System.Linq;
using AcHelper.Wrappers;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Presenter
{
    public class ThResidentialRoomDbUtil
    {
        public static ObjectId ConfigLayer(string layerName)
        {
            Dictionary<string, short> colors = new Dictionary<string, short>
            {
                { "套内", 1 },
                { "阳台", 3 }
            };

            string[] tokens = layerName.Split('_');
            short colorIndex = colors[tokens[1]];

            using (var db = AcadDatabase.Active())
            {
                ObjectId objectId = LayerTools.AddLayer(db.Database, layerName);
                LayerTools.SetLayerColor(db.Database, layerName, colors[tokens[1]]);
                return objectId;
            };
        }

        public static void RemoveLayer(string layerName)
        {
            using (var db = AcadDatabase.Active())
            {
                db.ModelSpace
                    .OfType<Polyline>()
                    .Where(e => e.Layer == layerName)
                    .UpgradeOpen()
                    .ForEachDbObject(e => e.Erase());

                LayerTools.DeleteLayer(db.Database, layerName);
            }
        }

        public static void MoveToLayer(ObjectId objectId, ObjectId layerId)
        {
            using (AcTransaction tr = new AcTransaction())
            {
                Polyline polyline = (Polyline)tr.Transaction.GetObject(objectId, OpenMode.ForRead);
                using (new WriteEnabler(polyline))
                {
                    polyline.LayerId = layerId;
                }
            }
        }
    }
}
