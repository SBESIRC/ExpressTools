using System;
using System.Linq;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public class ThResidentialRoomUtil
    {
        private const string version = "V1.0";

        // 图层名称
        public static string LayerName(ThResidentialStorey storey,
            ThResidentialRoom room, 
            ThResidentialRoomComponent component, 
            ThResidentialAreaFrame frame)
        {
            string[] tokens =
            {
                "住宅构件",
                component.Name,
                frame.Coefficient.ToString(),
                frame.FARCoefficient.ToString(),
                room.Name,
                room.Identifier,
                storey.Identifier,
                "",
                version,
            };

            return string.Join("_", tokens);
        }

        // 图层名称
        public static string LayerName(ThAOccupancyStorey storey, ThAOccupancy aoccupancy)
        {
            switch(aoccupancy.Component)
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
                            aoccupancy.Coefficient.ToString(),
                            aoccupancy.FARCoefficient.ToString(),
                            // aoccupancy.Floors可以为null，
                            // Convert.ToString(null)返回一个空字符串
                            Convert.ToString(aoccupancy.Floors),
                            storey.Identifier,
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
                            aoccupancy.Coefficient.ToString(),
                            aoccupancy.FARCoefficient.ToString(),
                            // 没有车位层数
                            storey.Identifier,
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
                greenSpace.Coefficient.ToString(),
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
                building.Number,
                building.Name,
                building.Category,
                building.AboveGroundFloorNumber,
                building.UnderGroundFloorNumber,
                "",
                "",
                "",
                "",
                "",
                version,
            };

            return string.Join("_", tokens);
        }

        public static string LayerName(ThRoof roof)
        {
            string[] tokens =
            {
                "单体楼顶间",
                roof.Coefficient.ToString(),
                roof.FARCoefficient.ToString(),
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
                Coefficient = 1.0,
                FARCoefficient = 1.0,
            });
        }
    }
}
