using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrame
{
    // 住宅区域
    public class ResidentialAreaUnit 
    {
        public string type;
        public string areaType;
        public string areaRatio;
        public string floorAreaRatio;
        public string dwelling;
        public string dwellingID;
        public string storeys;
        public string publicAreaID;
        public string version;
        public string layer;

        public ResidentialAreaUnit()
        {
        }

        public ResidentialAreaUnit(string name)
        {
            layer = name;
        }

        public static ResidentialAreaUnit Dwelling(string name)
        {
            string[] tokens = name.Split('_');
            ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
            {
                type = tokens[0],
                areaType = tokens[1],
                areaRatio = tokens[2],
                floorAreaRatio = tokens[3],
                dwelling = tokens[4],
                dwellingID = tokens[5],
                storeys = tokens[6],
                publicAreaID = tokens[7],
                version = tokens[8]
            };
            return unit;
        }

        public static ResidentialAreaUnit Balcony(string name)
        {
            string[] tokens = name.Split('_');
            if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
            {
                ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
                {
                    type = tokens[0],
                    areaType = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    dwellingID = tokens[4],
                    publicAreaID = tokens[5],
                    version = tokens[6]
                };
                return unit;
            }
            if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version)
            {
                ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
                {
                    type = tokens[0],
                    areaType = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    dwellingID = tokens[4],
                    storeys = tokens[5],
                    publicAreaID = tokens[6],
                    version = tokens[7]
                };
                return unit;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static ResidentialAreaUnit Baywindow(string name)
        {
            string[] tokens = name.Split('_');
            if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
            {
                ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
                {
                    type = tokens[0],
                    areaType = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    dwellingID = tokens[4],
                    version = tokens[5]
                };
                return unit;
            }
            else if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version)
            {
                ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
                {
                    type = tokens[0],
                    areaType = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    dwellingID = tokens[4],
                    storeys = tokens[5],
                    version = tokens[6]
                };
                return unit;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static ResidentialAreaUnit Miscellaneous(string name)
        {
            string[] tokens = name.Split('_');
            if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
            {
                ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
                {
                    type = tokens[0],
                    areaType = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    dwellingID = tokens[4],
                    version = tokens[5]
                };
                return unit;
            }
            else if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version)
            {
                ResidentialAreaUnit unit = new ResidentialAreaUnit(name)
                {
                    type = tokens[0],
                    areaType = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    dwellingID = tokens[4],
                    storeys = tokens[5],
                    version = tokens[6]
                };
                return unit;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    // 住宅户型
    public class ResidentialRoom
    {
        // 套内
        public ResidentialAreaUnit dwelling;
        // 阳台
        public List<ResidentialAreaUnit> balconies;
        // 飘窗
        public List<ResidentialAreaUnit> baywindows;
        // 其他构件
        public List<ResidentialAreaUnit> miscellaneous;

        public ResidentialRoom()
        {
            dwelling = null;
            balconies = new List<ResidentialAreaUnit>();
            baywindows = new List<ResidentialAreaUnit>();
            miscellaneous = new List<ResidentialAreaUnit>();
        }
        
        public List<ResidentialStorey> Storeys()
        {
            var storeys = new List<ResidentialStorey>();
            foreach (int number in ThAreaFrameUtils.ParseStoreyString(dwelling.storeys))
            {
                storeys.Add(new ResidentialStorey { tag = 'c', number = number, standard = false });
            }
            foreach (int number in ThAreaFrameUtils.ParseStandardStoreyString(dwelling.storeys))
            {
                storeys.Where(s => s.number == number).ForEach(s => s.standard = true);
                storeys.Where(s => s.number == number).ForEach(s => s.tag = dwelling.storeys.First());
            }
            return storeys;
        }
    }

    // 住宅楼栋
    public class ResidentialBuilding : IThAreaFrameBuilding
    {
        public List<ResidentialRoom> rooms;

        public ResidentialBuilding()
        {
            rooms = new List<ResidentialRoom>();
        }

        public static ResidentialBuilding CreateWithLayers(string[] names)
        {
            var building = new ResidentialBuilding();

            // 套内
            var dwellings = names.Where(n => n.StartsWith(@"住宅构件_套内"));
            foreach (string item in dwellings)
            {
                var room = new ResidentialRoom()
                {
                    dwelling = ResidentialAreaUnit.Dwelling(item)
                };
                building.rooms.Add(room);
            }

            // 阳台
            var balconies = names.Where(n => n.StartsWith(@"住宅构件_阳台"));
            foreach (string item in balconies)
            {
                var component = ResidentialAreaUnit.Balcony(item);
                if (component.version == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
                {
                    var room = building.rooms.Where(b => b.dwelling.dwellingID == component.dwellingID)
                        .FirstOrDefault();
                    room.balconies.Add(component);
                }
                else if (component.version == ThCADCommon.RegAppName_AreaFrame_Version)
                {
                    var room = building.rooms.Where(
                        o => o.dwelling.storeys == component.storeys &&
                        o.dwelling.dwellingID == component.dwellingID).First();
                    room.balconies.Add(component);
                }
            }

            // 飘窗
            var baywindows = names.Where(n => n.StartsWith(@"住宅构件_飘窗"));
            foreach (string item in baywindows)
            {
                var component = ResidentialAreaUnit.Baywindow(item);
                if (component.version == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
                {
                    var room = building.rooms.Where(b => b.dwelling.dwellingID == component.dwellingID)
                        .FirstOrDefault();
                    room.baywindows.Add(component);
                }
                else if (component.version == ThCADCommon.RegAppName_AreaFrame_Version)
                {
                    var room = building.rooms.Where(
                        o => o.dwelling.storeys == component.storeys &&
                        o.dwelling.dwellingID == component.dwellingID).First();
                    room.baywindows.Add(component);
                }
            }

            // 其他构件
            var miscellaneous = names.Where(n => n.StartsWith(@"住宅构件_其他构件"));
            foreach (string item in miscellaneous)
            {
                var component = ResidentialAreaUnit.Miscellaneous(item);
                if (component.version == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
                {
                    var room = building.rooms.Where(b => b.dwelling.dwellingID == component.dwellingID)
                        .FirstOrDefault();
                    room.miscellaneous.Add(component);
                }
                else if (component.version == ThCADCommon.RegAppName_AreaFrame_Version)
                {
                    var room = building.rooms.Where(
                        o => o.dwelling.storeys == component.storeys &&
                        o.dwelling.dwellingID == component.dwellingID).First();
                    room.miscellaneous.Add(component);
                }
            }

            return building;
        }

        public bool Validate()
        {
            return rooms.Count > 0;
        }

        public int UppermostStorey
        {
            get
            {
                return Storeys().Max(o => o.number);
            }
        }

        public List<ResidentialRoom> RoomsOnStorey(int floor)
        {
            var roomsOnFloor = new List<ResidentialRoom>();
            foreach (ResidentialRoom room in rooms)
            {
                if (room.Storeys().Where(s => s.number == floor).Any())
                {
                    roomsOnFloor.Add(room);
                }
            }

            return roomsOnFloor;
        }

        // 所有地上标准楼层
        public List<ResidentialStorey> StandardStoreys()
        {
            return AboveGroundStoreys().Where(o => o.standard == true).ToList();
        }

        // 所有地上普通楼层
        public List<ResidentialStorey> OrdinaryStoreys()
        {
            return AboveGroundStoreys().Where(o => o.standard == false).ToList();
        }

        // 所有地上楼层
        public List<ResidentialStorey> AboveGroundStoreys()
        {
            return Storeys().Where(o => o.number > 0).ToList();
        }

        // 所有地下楼层
        public List<ResidentialStorey> UnderGroundStoreys()
        {
            return Storeys().Where(o => o.number < 0).ToList();
        }

        public List<ResidentialStorey> Storeys()
        {
            var storeys = new List<List<ResidentialStorey>>();
            rooms.ForEach(r => storeys.Add(r.Storeys()));
            return storeys.SelectMany(o => o).ToList();
        }
    }
}