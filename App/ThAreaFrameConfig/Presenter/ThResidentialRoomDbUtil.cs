using System;
using DotNetARX;
using Linq2Acad;
using System.Linq;
using AcHelper.Wrappers;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Presenter
{
    public class ThResidentialRoomDbUtil
    {
        private static readonly Dictionary<string, Dictionary<string, short>> LayerColorConfig = new Dictionary<string, Dictionary<string, short>>
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

        public static ObjectId ConfigLayer(string layerName)
        {
            string[] tokens = layerName.Split('_');
            short colorIndex = LayerColorConfig[tokens[0]][tokens[1]];
            return CreateAreaFrameLayer(layerName, colorIndex);
        }

        public static ObjectId ConfigRoofLayer(string layerName)
        {
            return CreateAreaFrameLayer(layerName, 80);
        }

        public static ObjectId ConfigBuildingLayer(string layerName)
        {
            return CreateAreaFrameLayer(layerName, 56);
        }

        public static ObjectId ConfigPlotSpaceLayer(string layerName)
        {
            return CreateAreaFrameLayer(layerName, Color.FromRgb(69, 119, 19).ColorIndex);
        }

        public static ObjectId ConfigPublicGreenSpaceLayer(string layerName)
        {
            return CreateAreaFrameLayer(layerName, Color.FromRgb(0, 87, 0).ColorIndex);
        }

        public static ObjectId ConfigOutdoorParkingSpaceLayer(string layerName)
        {
            return CreateAreaFrameLayer(layerName, Color.FromRgb(29, 99, 64).ColorIndex);
        }

        private static ObjectId CreateAreaFrameLayer(string name, short colorIndex)
        {
            using (var db = AcadDatabase.Active())
            {
                ObjectId objectId = LayerTools.AddLayer(db.Database, name);
                LayerTools.SetLayerColor(db.Database, name, colorIndex);
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

        public static void RenameLayer(string layerName, string newLayerName)
        {
            using (var db = AcadDatabase.Active())
            {
                db.Layers.Element(layerName, true).Name = newLayerName;
            }
        }

        public static string LayerName(IntPtr obj)
        {
            ObjectId objId = new ObjectId(obj);
            using (var db = AcadDatabase.Active())
            {
                return db.ModelSpace.Element(objId).Layer;
            }
        }
    }
}
