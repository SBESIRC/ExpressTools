using System;
using System.Linq;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public class ThResidentialRoomUtil
    {
        public const string version = "V2.2";

        // 图层名称
        public static string LayerName(ThResidentialStorey storey,
            ThResidentialRoom room, 
            ThResidentialRoomComponent component, 
            ThResidentialAreaFrame frame)
        {
            return LayerName(storey.Identifier, room, component, frame);
        }

        public static string LayerName(string identifier,
            ThResidentialRoom room,
            ThResidentialRoomComponent component,
            ThResidentialAreaFrame frame)
        {
            switch(component.Name)
            {
                case "套内":
                    {
                        string[] tokens = {
                            "住宅构件",
                            component.Name,
                            String.Format("{0:0.0}", Convert.ToDouble(frame.Coefficient)),
                            String.Format("{0:0.0}", Convert.ToDouble(frame.FARCoefficient)),
                            room.Name,
                            room.Identifier,
                            identifier,
                            "",
                            version,
                        };

                        return string.Join("_", tokens);
                    }
                case "阳台":
                    {
                        string[] tokens = {
                            "住宅构件",
                            component.Name,
                            String.Format("{0:0.0}", Convert.ToDouble(frame.Coefficient)),
                            String.Format("{0:0.0}", Convert.ToDouble(frame.FARCoefficient)),
                            room.Identifier,
                            identifier,
                            "",
                            version,
                        };

                        return string.Join("_", tokens);
                    }
                case "飘窗":
                case "其他构件":
                    {
                        string[] tokens = {
                            "住宅构件",
                            component.Name,
                            String.Format("{0:0.0}", Convert.ToDouble(frame.Coefficient)),
                            String.Format("{0:0.0}", Convert.ToDouble(frame.FARCoefficient)),
                            room.Identifier,
                            identifier,
                            version,
                        };

                        return string.Join("_", tokens);
                    }
                default:
                    return "";
            }
        }

        // 图层名称
        public static string LayerName(ThAOccupancyStorey storey, ThAOccupancy aoccupancy)
        {
            return LayerName(storey.Identifier, aoccupancy);
        }

        public static string LayerName(string identifier, ThAOccupancy aoccupancy)
        {
            switch (aoccupancy.Component)
            {
                case "主体":
                case "架空":
                    {
                        string[] tokens =
                        {
                            "附属公建",
                            aoccupancy.Component,
                            aoccupancy.Category,
                            "",
                            String.Format("{0:0.0}", Convert.ToDouble(aoccupancy.Coefficient)),
                            String.Format("{0:0.0}", Convert.ToDouble(aoccupancy.FARCoefficient)),
                            // aoccupancy.Floors可以为null，
                            // Convert.ToString(null)返回一个空字符串
                            Convert.ToString(aoccupancy.Floors),
                            identifier,
                            "",
                            version,
                        };

                        return string.Join("_", tokens);
                    }
                case "阳台":
                case "飘窗":
                case "雨棚":
                case "附属其他构件":
                    {
                        string[] tokens =
                        {
                            "附属公建",
                            aoccupancy.Component,
                            aoccupancy.Category,
                            "",
                            String.Format("{0:0.0}", Convert.ToDouble(aoccupancy.Coefficient)),
                            String.Format("{0:0.0}", Convert.ToDouble(aoccupancy.FARCoefficient)),
                            // 没有车位层数
                            identifier,
                            "",
                            version,
                        };

                        return string.Join("_", tokens);
                    }
                default:
                    break;
            }

            //
            return null;
        }

        public static string LayerName(ThRoofGreenSpace greenSpace)
        {
            string[] tokens =
            {           
                "屋顶构件",
                "屋顶绿地",
                String.Format("{0:0.0}", Convert.ToDouble(greenSpace.Coefficient)),
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThPlotSpace plotSpace)
        {
            string[] tokens =
            {
                "用地",
                "规划净用地",
                plotSpace.HouseHold.ToString(),
                "",
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThPublicGreenSpace greenSpace)
        {
            string[] tokens =
            {
                "用地",
                "公共绿地",
                "",
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThOutdoorParkingSpace parkingSpace)
        {
            string[] tokens =
            {
                "车场车位",
                "室外车场",
                parkingSpace.Category,
                parkingSpace.ParkingCategory,
                parkingSpace.Storey.ToString(),
                "",
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThUnderGroundParking parking)
        {
            string[] tokens =
            {
                "单体车位",
                "小型汽车",
                parking.Floors.ToString(),
                parking.Storey,
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThResidentialBuilding building)
        {
            string[] tokens =
            {
                "单体基底",
                building.Number,                        // 建筑编号
                building.Name,                          // 建筑名称
                building.Category,                      // 单体性质
                building.AboveGroundFloorNumber,        // 地上层数
                building.UnderGroundFloorNumber,        // 地下层数
                "",                                     // 层高
                "",                                     // 地下室露高
                "",                                     // 室内外高差
                "",                                     // 屋面高度
                "是",                                   // 是否计入计算
                "",                                     // 概况
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThRoof roof)
        {
            string[] tokens =
            {
                "单体楼顶间",
                String.Format("{0:0.0}", Convert.ToDouble(roof.Coefficient)),
                String.Format("{0:0.0}", Convert.ToDouble(roof.FARCoefficient)),
                version,
            };

            return string.Join("_", tokens);
        }

        public static List<string> LayerNames(ThResidentialStorey storey)
        { 
            List<string> names = new List<string>();
            foreach(var room in storey.Rooms)
            {
                foreach(var component in room.Components)
                {
                    foreach(var frame in component.AreaFrames)
                    {
                        if (frame.Frame == (IntPtr)0)
                            continue;

                        names.Add(LayerName(storey, room, component, frame));
                    }
                }
            }

            return names.Distinct().ToList();
        }

        // 住宅户型
        public static List<ThResidentialStorey> Residential(string[] names)
        {
            List<ThResidentialStorey> storeys = new List<ThResidentialStorey>();

            // 楼层
            foreach (string name in names.Where(n => n.StartsWith(@"住宅构件_套内")))
            {
                string[] tokens = name.Split('_');
                if (storeys.Where(o => o.Identifier == tokens[6]).Any())
                {
                    continue;
                }

                // 添加楼层
                storeys.Add(new ThResidentialStorey()
                {
                    ID = Guid.NewGuid(),
                    Identifier = tokens[6],
                    Rooms = new List<ThResidentialRoom>()
                });
            }

            // 房型
            foreach (string name in names.Where(n => n.StartsWith(@"住宅构件_套内")))
            {
                string[] tokens = name.Split('_');
                foreach (var storey in storeys.Where(o => o.Identifier == tokens[6]))
                {
                    if (storey.Rooms.Where(o => o.Identifier == tokens[5]).Any())
                    {
                        continue;
                    }

                    // 添加户型
                    Guid guid = Guid.NewGuid();
                    storey.Rooms.Add(new ThResidentialRoom()
                    {
                        ID = guid,
                        StoreyID = storey.ID,
                        Name = tokens[4],
                        Identifier = tokens[5],
                        Components = new List<ThResidentialRoomComponent>()
                        {
                            ThResidentialRoomComponent.Dwelling(guid),
                            ThResidentialRoomComponent.Balcony(guid),
                            ThResidentialRoomComponent.Baywindow(guid),
                            ThResidentialRoomComponent.Miscellaneous(guid)
                        }
                    });
                }
            }

            return storeys;
        }

        public static List<ThRoofGreenSpace> RoofGreenSpace(string[] names)
        {
            List<ThRoofGreenSpace> spaces = new List<ThRoofGreenSpace>();
            foreach (string name in names.Where(n => n.StartsWith(@"屋顶构件_屋顶绿地")))
            {
                string[] tokens = name.Split('_');
                spaces.Add(new ThRoofGreenSpace()
                {
                    ID = Guid.NewGuid(),
                    Number = spaces.Count+1,
                    Coefficient = double.Parse(tokens[2])
                });
            }
            return spaces;
        }

        // 默认住宅楼层
        public static ThResidentialStorey DefaultResidentialStorey(string identifier)
        {
            Guid storeyId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            ThResidentialStorey storey = new ThResidentialStorey()
            {
                ID = storeyId,
                Identifier = identifier,
                Rooms = new List<ThResidentialRoom>()
                {
                    DefaultResidentialRoom(storeyId)
                }
            };
            return storey;
        }

        public static ThResidentialRoom DefaultResidentialRoom(Guid storeyID)
        {
            Guid roomId = Guid.NewGuid();
            ThResidentialRoom room = new ThResidentialRoom()
            {
                ID = roomId,
                StoreyID = storeyID,
                Name = "A",
                Identifier = "HT1",
                Components = new List<ThResidentialRoomComponent>()
                {
                    ThResidentialRoomComponent.Dwelling(roomId),
                    ThResidentialRoomComponent.Balcony(roomId),
                    ThResidentialRoomComponent.Baywindow(roomId),
                    ThResidentialRoomComponent.Miscellaneous(roomId)
                }
            };
            foreach(var component in room.Components)
            {
                AppendPlaceHolderAreaFrame(room, component);
            }
            return room;
        }

        public static void AppendPlaceHolderAreaFrame(ThResidentialRoom room, ThResidentialRoomComponent comoponent)
        {
            int index = comoponent.AreaFrames.Count;
            comoponent.AreaFrames.Add(new ThResidentialAreaFrame()
            {
                ID = Guid.NewGuid(),
                RoomID = room.ID,
                ComponentID = comoponent.ID,
                Number = ++index,
                Frame = (IntPtr)0,
                Coefficient = DefaultComponentCoefficient(comoponent),
                FARCoefficient = DefaultComponentFARCoefficient(comoponent),
            });
        }

        private static double DefaultComponentCoefficient(ThResidentialRoomComponent comoponent)
        {
            switch (comoponent.Name)
            {
                case "套内":
                    return 1.0;
                case "阳台":
                    return 0.5;
                case "飘窗":
                    return 0.0;
                case "其他构件":
                    return 0.5;
                default:
                    return 0.0;
            }
        }

        private static double DefaultComponentFARCoefficient(ThResidentialRoomComponent comoponent)
        {
            switch (comoponent.Name)
            {
                case "套内":
                    return 1.0;
                case "阳台":
                    return 0.5;
                case "飘窗":
                    return 0.0;
                case "其他构件":
                    return 0.5;
                default:
                    return 0.0;
            }
        }

        public static void AppendPlaceHolderAreaFrame(ThAOccupancyStorey storey)
        {
            int index = storey.AOccupancies.Count;
            storey.AOccupancies.Add(new ThAOccupancy()
            {
                ID = Guid.NewGuid(),
                StoreyID = storey.ID,
                Number = ++index,
                Component = "主体",
                Category = "商业",
                Coefficient = 1.0,
                FARCoefficient = 1.0,
                Floors = null,
                Frame = (IntPtr)0
            });
        }
    }
}
