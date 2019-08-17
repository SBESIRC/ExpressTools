using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Model
{
    public class ThAOccupancyDbDepository
    {
        private readonly Database database;
        private List<ThAOccupancyStorey> storeys;

        // 构造函数
        public ThAOccupancyDbDepository()
        {
            database = Active.Database;
            ConstructRepository();
            ConstructAreaFrames();
        }

        public ThAOccupancyDbDepository(Database db)
        {
            database = db;
            ConstructRepository();
            ConstructAreaFrames();
        }

        public List<ThAOccupancyStorey> Storeys
        {
            get
            {
                            return storeys;
            }
        }

        public List<ThAOccupancy> AOccupancies(string storey)
        {
            return storeys.Where(o => o.Identifier == storey).First().AOccupancies;
        }

        public List<ThAOccupancy> AOccupancies(ThAOccupancyStorey storey)
        {
            return AOccupancies(storey.Identifier);
        }

        public void AppendStorey(string identifier)
        {
            storeys.Add(DefaultAOccupancyStorey(identifier));
        }

        public void RemoveStorey(string identifier)
        {
            var storey = storeys.Where(o => o.Identifier == identifier).FirstOrDefault();
            if (storey != null)
            {
                storeys.Remove(storey);
            }
        }

        private ThAOccupancy DefaultAOccupancy(Guid storeyId)
        {
            return new ThAOccupancy()
            {
                ID = Guid.NewGuid(),
                StoreyID = storeyId,
                Number = 1,
                Component = "主体",
                Category = "商业",
                Coefficient = 1.0,
                FARCoefficient = 1.0,
                Floors = null,
                Frame = ObjectId.Null.OldIdPtr
            };
        }

        private ThAOccupancyStorey DefaultAOccupancyStorey(string storey)
        {
            Guid guid = Guid.NewGuid();
            return new ThAOccupancyStorey()
            {
                ID = guid,
                Identifier = storey,
                AOccupancies = new List<ThAOccupancy>()
                {
                    DefaultAOccupancy(guid)
                }
            };
        }

        private void ConstructRepository()
        {
            storeys = new List<ThAOccupancyStorey>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                var aOccupancyNames = names.Where(n => n.StartsWith(@"附属公建"));
                foreach (string name in aOccupancyNames)
                {
                    string[] tokens = name.Split('_');
                    if (storeys.Where(o => o.Identifier == tokens[7]).Any())
                    {
                        continue;
                    }
                    // 添加楼层
                    storeys.Add(new ThAOccupancyStorey()
                    {
                        ID = Guid.NewGuid(),
                        Identifier = tokens[7],
                        AOccupancies = new List<ThAOccupancy>()
                    });
                }
            }
        }

        private ObjectIdCollection AreaFrameLines(string layer)
        {
            var objectIdCollection = new ObjectIdCollection();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                acadDatabase.ModelSpace
                            .OfType<Polyline>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => objectIdCollection.Add(e.ObjectId));
            }
            return objectIdCollection;
        }

        private void ConstructAreaFrames()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                List<string> names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                var aOccupancyNames = names.Where(n => n.StartsWith(@"附属公建"));
                foreach(var name in aOccupancyNames)
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in AreaFrameLines(name))
                    {
                        switch (tokens[1])
                        {
                            case "主体":
                            case "架空":
                                {
                                    foreach(var storey in storeys.Where(o => o.Identifier == tokens[7]))
                                    {
                                        storey.AOccupancies.Add(new ThAOccupancy()
                                        {
                                            ID = Guid.NewGuid(),
                                            StoreyID = storey.ID,
                                            Number = storey.AOccupancies.Count + 1,
                                            Component = tokens[1],
                                            Category = tokens[2],
                                            Coefficient = double.Parse(tokens[4]),
                                            FARCoefficient = double.Parse(tokens[5]),
                                            Floors = NullableParser.TryParseInt(tokens[6]),
                                            Frame = objId.OldIdPtr
                                        });
                                    }
                                }
                                break;
                            case "阳台":
                            case "飘窗":
                            case "雨棚":
                            case "附属其他构件":
                                {
                                    foreach (var storey in storeys.Where(o => o.Identifier == tokens[6]))
                                    {
                                        storey.AOccupancies.Add(new ThAOccupancy()
                                        {
                                            ID = Guid.NewGuid(),
                                            StoreyID = storey.ID,
                                            Number = storey.AOccupancies.Count + 1,
                                            Component = tokens[1],
                                            Category = tokens[2],
                                            Coefficient = double.Parse(tokens[4]),
                                            FARCoefficient = double.Parse(tokens[5]),
                                            Floors = null,
                                            Frame = objId.OldIdPtr
                                        });
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
