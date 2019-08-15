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
            Dictionary<string, Dictionary<string, short>> colors = new Dictionary<string, Dictionary<string, short>>
            {
                { "住宅构件", new Dictionary<string, short>
                    {
                        { "套内",             1   },
                        { "阳台",             3   },
                     }
                },

                { "附属公建", new Dictionary<string, short>
                    {
                        { "主体",             96  },
                        { "阳台",             3   },
                        { "架空",             2   },
                        { "飘窗",             4   },
                        { "雨棚",             226 },
                        { "附属其他构件",      185 }
                    }
                },

                { "屋顶构件", new Dictionary<string, short>
                    {
                        { "屋顶绿地",   77}
                    }
                },

                { "单体车位", new Dictionary<string, short>
                    {
                        { "小型汽车", 17 } 
                    }
                }
            };

            string[] tokens = layerName.Split('_');
            short colorIndex = colors[tokens[0]][tokens[1]];

            using (var db = AcadDatabase.Active())
            {
                ObjectId objectId = LayerTools.AddLayer(db.Database, layerName);
                LayerTools.SetLayerColor(db.Database, layerName, colorIndex);
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
