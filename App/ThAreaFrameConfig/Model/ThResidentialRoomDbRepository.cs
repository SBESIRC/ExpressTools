﻿using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThResidentialRoomDbRepository : IThResidentialRoomRepository
    {
        private readonly Database database;
        private List<ThResidentialStorey> storeys;

        // 构造函数
        public ThResidentialRoomDbRepository()
        {
            database = Active.Database;
            ConstructRepository();
            ConstructAreaFrames();
        }

        public ThResidentialRoomDbRepository(Database db)
        {
            database = db;
            ConstructRepository();
            ConstructAreaFrames();
        }

        public void RegisterAreaFrameModifiedEvent(ObjectEventHandler handler)
        {
            database.ObjectModified += handler;
        }

        public void UnRegisterAreaFrameModifiedEvent(ObjectEventHandler handler)
        {
            database.ObjectModified -= handler;
        }

        public void RegisterAreaFrameErasedEvent(ObjectErasedEventHandler handler)
        {
            database.ObjectErased += handler;
        }

        public void UnRegisterAreaFrameErasedEvent(ObjectErasedEventHandler handler)
        {
            database.ObjectErased -= handler;
        }

        public ThResidentialAreaFrame AreaFrame(DBObject dbobj)
        {
            foreach (var storey in storeys)
            {
                foreach (var room in storey.Rooms)
                {
                    foreach(var component in room.Components)
                    {
                        foreach (var areaFrame in component.AreaFrames)
                        {
                            if (areaFrame.Frame == dbobj.ObjectId.OldIdPtr)
                            {
                                return areaFrame;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void AppendStorey(string identifier)
        {
            storeys.Add(ThResidentialRoomUtil.DefaultResidentialStorey(identifier));
        }

        public void RemoveStorey(string identifier)
        {
            var item = Storey(identifier);
            if (item != null)
            {
                storeys.Remove(item);
            }
        }

        public ThResidentialStorey Storey(string storey)
        {
            return storeys.Where(o => o.Identifier == storey).FirstOrDefault();
        }

        private void ConstructRepository()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                var residentialNames = names.Where(n => n.StartsWith(@"住宅构件"));
                storeys = ThResidentialRoomUtil.Residential(residentialNames.ToArray());
            }
        }

        private void ConstructAreaFrames()
        {
            List<string> layers = new List<string>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                acadDatabase.Layers.ForEachDbObject(l => layers.Add(l.Name));
            }

            foreach (var storey in storeys)
            {
                foreach (var room in storey.Rooms)
                {
                    // 套内
                    int index = 0;
                    var comoponent = room.Components.Where(o => o.Name == "套内").First();
                    foreach (string layer in layers.Where(n => n.StartsWith(@"住宅构件_套内")))
                    {
                        string[] tokens = layer.Split('_');
                        if (tokens[6] == storey.Identifier && tokens[5] == room.Identifier)
                        {
                            foreach (ObjectId objId in database.AreaFrameLines(layer))
                            {
                                comoponent.AreaFrames.Add(new ThResidentialAreaFrame()
                                {
                                    ID = Guid.NewGuid(),
                                    RoomID = room.ID,
                                    ComponentID = comoponent.ID,
                                    Number = ++index,
                                    Frame = objId.OldIdPtr,
                                    Coefficient = double.Parse(tokens[2]),
                                    FARCoefficient = double.Parse(tokens[3]),
                                });
                            }
                        }
                    }
                    ThResidentialRoomUtil.AppendPlaceHolderAreaFrame(room, comoponent);

                    // 阳台
                    index = 0;
                    comoponent = room.Components.Where(o => o.Name == "阳台").First();
                    foreach (string layer in layers.Where(n => n.StartsWith(@"住宅构件_阳台")))
                    {
                        // TODO: 
                        //  暂时不支持旧版本的图层命名规则v2.1
                        //  在v2.1中，"住宅构件_阳台"图层名并不包含楼层信息
                        string[] tokens = layer.Split('_');
                        if (tokens.Last() != ThResidentialRoomUtil.version)
                        {
                            continue;
                        }

                        foreach (ObjectId objId in database.AreaFrameLines(layer))
                        {
                            // 图层命名规则v2.2:
                            //  "住宅构件_阳台"图层名中添加“所属层”字段
                            if (tokens[5] == storey.Identifier && tokens[4] == room.Identifier)
                            {
                                comoponent.AreaFrames.Add(new ThResidentialAreaFrame()
                                {
                                    ID = Guid.NewGuid(),
                                    RoomID = room.ID,
                                    ComponentID = comoponent.ID,
                                    Number = ++index,
                                    Frame = objId.OldIdPtr,
                                    Coefficient = double.Parse(tokens[2]),
                                    FARCoefficient = double.Parse(tokens[3]),
                                });
                            }
                        }
                    }
                    ThResidentialRoomUtil.AppendPlaceHolderAreaFrame(room, comoponent);

                    // 飘窗
                    index = 0;
                    comoponent = room.Components.Where(o => o.Name == "飘窗").First();
                    foreach (string layer in layers.Where(n => n.StartsWith(@"住宅构件_飘窗")))
                    {
                        // TODO: 
                        //  暂时不支持旧版本的图层命名规则v2.1
                        //  在v2.1中，"住宅构件_阳台"图层名并不包含楼层信息
                        string[] tokens = layer.Split('_');
                        if (tokens.Last() != ThResidentialRoomUtil.version)
                        {
                            continue;
                        }

                        foreach (ObjectId objId in database.AreaFrameLines(layer))
                        {
                            // 图层命名规则v2.2:
                            //  "住宅构件_飘窗"图层名中添加“所属层”字段
                            if (tokens[5] == storey.Identifier && tokens[4] == room.Identifier)
                            {
                                comoponent.AreaFrames.Add(new ThResidentialAreaFrame()
                                {
                                    ID = Guid.NewGuid(),
                                    RoomID = room.ID,
                                    ComponentID = comoponent.ID,
                                    Number = ++index,
                                    Frame = objId.OldIdPtr,
                                    Coefficient = double.Parse(tokens[2]),
                                    FARCoefficient = double.Parse(tokens[3]),
                                });
                            }
                        }
                    }
                    ThResidentialRoomUtil.AppendPlaceHolderAreaFrame(room, comoponent);

                    // 其他构件
                    index = 0;
                    comoponent = room.Components.Where(o => o.Name == "其他构件").First();
                    foreach (string layer in layers.Where(n => n.StartsWith(@"住宅构件_其他构件")))
                    {
                        // TODO: 
                        //  暂时不支持旧版本的图层命名规则v2.1
                        //  在v2.1中，"住宅构件_阳台"图层名并不包含楼层信息
                        string[] tokens = layer.Split('_');
                        if (tokens.Last() != ThResidentialRoomUtil.version)
                        {
                            continue;
                        }

                        foreach (ObjectId objId in database.AreaFrameLines(layer))
                        {
                            // 图层命名规则v2.2:
                            //  "住宅构件_其他构件"图层名中添加“所属层”字段
                            if (tokens[5] == storey.Identifier && tokens[4] == room.Identifier)
                            {
                                comoponent.AreaFrames.Add(new ThResidentialAreaFrame()
                                {
                                    ID = Guid.NewGuid(),
                                    RoomID = room.ID,
                                    ComponentID = comoponent.ID,
                                    Number = ++index,
                                    Frame = objId.OldIdPtr,
                                    Coefficient = double.Parse(tokens[2]),
                                    FARCoefficient = double.Parse(tokens[3]),
                                });
                            }
                        }
                    }
                    ThResidentialRoomUtil.AppendPlaceHolderAreaFrame(room, comoponent);
                }
            }
        }

        public IEnumerable<ThResidentialRoom> Rooms(string storey)
        {
            return storeys.Where(o => o.Identifier == storey).First().Rooms;
        }

        public IEnumerable<ThResidentialRoom> Rooms(ThResidentialStorey storey)
        {
            return Rooms(storey.Identifier);
        }

        public IEnumerable<ThResidentialStorey> Storeys()
        {
            return storeys;
        }
    }
}
