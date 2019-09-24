using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using GeometryExtensions;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Model
{
    public static class ThAreaFrameDbExtension
    {
        public static ObjectIdCollection AreaFrameLines(this Database db, string layer)
        {
            var objectIdCollection = new ObjectIdCollection();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(db))
            {
                acadDatabase.ModelSpace
                            .OfType<Curve>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => objectIdCollection.Add(e.ObjectId));
            }
            return objectIdCollection;
        }

        public static List<IntPtr> AreaFrameLinesEx(this Database db, string layer)
        {
            var areaFrames = new List<IntPtr>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(db))
            {
                acadDatabase.ModelSpace
                            .OfType<Curve>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => areaFrames.Add(e.ObjectId.OldIdPtr));
            }
            return areaFrames;
        }

        public static double Area(this IntPtr frame)
        {
            if (frame == (IntPtr)0)
            {
                return 0.0;
            }

            ObjectId objId = new ObjectId(frame);
            if (objId.IsErased)
            {
                return 0.0;
            }

            using (AcadDatabase acadDatabase = AcadDatabase.Use(objId.Database))
            {
                return acadDatabase.ElementOrDefault<Curve>(objId).Area * (1.0 / 1000000.0);
            }
        }

        public static double AreaEx(this IntPtr frame)
        {
            return frame.Area();
            //if (frame == (IntPtr)0)
            //{
            //    return 0.0;
            //}

            //ObjectId objId = new ObjectId(frame);
            //if (objId.IsErased)
            //{
            //    return 0.0;
            //}

            //using (AcadDatabase acadDatabase = AcadDatabase.Use(objId.Database))
            //{
            //    // 根据面积框线轮廓创建“区域”
            //    //  https://www.keanw.com/2015/08/getting-the-centroid-of-an-autocad-region-using-net.html
            //    DBObjectCollection curves = new DBObjectCollection()
            //    {
            //        acadDatabase.Element<Curve>(objId)
            //    };
            //    DBObjectCollection regions = Region.CreateFromCurves(curves);
            //    Region region = regions[0] as Region;
            //    Point3d centroid = region.Centroid();
            //    if (!region.ContainsPoint(centroid))
            //    {
            //        return 0.0;
            //    }

            //    // 在“面心”处作为“种子”点，创建“轮廓”。“轮廓”会考虑到“空岛”的存在
            //    //  https://www.keanw.com/2010/06/tracing-a-boundary-defined-by-autocad-geometry-using-net.html
            //    //  https://www.keanw.com/2010/06/creating-transparent-hatches-in-autocad-using-net.html
            //    DBObjectCollection boundaries = Active.Editor.TraceBoundary(region.Centroid(), false);

            //    // 根据“轮廓”创建新的“区域”
            //    DBObjectCollection boundaryRegions = Region.CreateFromCurves(boundaries);

            //    // 计算“区域”面积
            //    // 第一个“区域”是轮廓区域
            //    var outmostRegion = boundaryRegions[0] as Region;
            //    double area = outmostRegion.Area;
            //    for (int i = 1; i < boundaryRegions.Count; i++)
            //    {
            //        // 减去“空岛区域“
            //        if (boundaryRegions[i] is Region boundaryRegion)
            //        {
            //            area -= boundaryRegion.Area;
            //        }
            //    }

            //    return area * (1.0 / 1000000.0);
            //}
        }

        public static List<ThOutdoorParkingSpace> OutdoorParkingSpaces(this Database database)
        {
            var spaces = new List<ThOutdoorParkingSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"车场车位_室外车场")))
                {
                    string[] tokens = name.Split('_');
                    if (tokens[2] != "露天车场" || tokens[3] != "小型汽车")
                        continue;

                    var frames = new List<IntPtr>();
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        frames.Add(objId.OldIdPtr);
                    }
                    if (frames.Count == 0)
                        continue;

                    spaces.Add(new ThOutdoorParkingSpace()
                    {
                        ID = Guid.NewGuid(),
                        Number = spaces.Count + 1,
                        Storey = int.Parse(tokens[4]),
                        Frames = frames
                    });
                }
            }
            return spaces;
        }

        public static List<ThPlotSpace> PlotSpaces(this Database database)
        {
            var spaces = new List<ThPlotSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"用地_规划净用地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        spaces.Add(new ThPlotSpace()
                        {
                            ID = Guid.NewGuid(),
                            Number = spaces.Count + 1,
                            Frame = objId.OldIdPtr,
                            HouseHold = UInt16.Parse(tokens[2])
                        });
                    }
                }
            }
            return spaces;
        }

        public static List<ThPublicGreenSpace> PublicGreenSpaces(this Database database)
        {
            var spaces = new List<ThPublicGreenSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"用地_公共绿地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        spaces.Add(new ThPublicGreenSpace()
                        {
                            ID = Guid.NewGuid(),
                            Number = spaces.Count + 1,
                            Frame = objId.OldIdPtr,
                        });
                    }
                }
            }
            return spaces;
        }

        public static List<ThRoof> Roofs(this Database database)
        {
            var roofs = new List<ThRoof>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"单体楼顶间")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        roofs.Add(new ThRoof()
                        {
                            ID = Guid.NewGuid(),
                            Number = roofs.Count + 1,
                            Frame = objId.OldIdPtr,
                            Coefficient = double.Parse(tokens[1]),
                            FARCoefficient = double.Parse(tokens[2])
                        });
                    }
                }
            }
            return roofs;
        }

        public static List<ThRoofGreenSpace> RoofGreenSpaces(this Database database)
        {
            var spaces = new List<ThRoofGreenSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"屋顶构件_屋顶绿地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        spaces.Add(new ThRoofGreenSpace()
                        {
                            ID = Guid.NewGuid(),
                            Number = spaces.Count + 1,
                            Frame = objId.OldIdPtr,
                            Coefficient = double.Parse(tokens[2])
                        });
                    }
                }
            }
            return spaces;
        }

        public static List<ThUnderGroundParking> UnderGroundParkings(this Database database)
        {
            var parkings = new List<ThUnderGroundParking>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"单体车位_小型汽车")))
                {
                    string[] tokens = name.Split('_');
                    parkings.Add(new ThUnderGroundParking()
                    {
                        ID = Guid.NewGuid(),
                        Number = parkings.Count + 1,
                        Floors = UInt16.Parse(tokens[2]),
                        Storey = tokens[3],
                        Frames = database.AreaFrameLinesEx(name)
                    });
                }
            }
            return parkings;
        }
    }
}
