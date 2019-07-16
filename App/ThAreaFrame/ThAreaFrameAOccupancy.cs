using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrame
{
    // 公建楼层
    public class AOccupancyStorey : IEquatable<AOccupancyStorey>
    {
        public int number;
        public bool standard;

        #region Equality
        public bool Equals(AOccupancyStorey other)
        {
            if (other == null) return false;
            return (this.number == other.number) && (this.standard == other.standard);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as AOccupancyStorey);
        }
        public override int GetHashCode()
        {
            return new { number, standard }.GetHashCode();
        }
        #endregion
    }

    // 公建实体
    public class ThAreaFrameAOccupancy
    {
        // 附属公建
        public string type;

        // 实体名
        public string areaType;

        // 类型
        public string occupancyType;

        // 构件名称
        public string component;

        // 性质
        public string category;

        // 计算系数
        public string areaRatio;

        // 计容系数
        public string floorAreaRatio;

        // 车位层数
        public string parkingStoreys;

        // 所属层
        public string storeys;

        // 公摊标识
        public string publicAreaID;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        // 楼层集合
        private List<AOccupancyStorey> storeyCollection;
        public List<AOccupancyStorey> StoreyCollection
        {
            get
            {
                if (storeyCollection == null)
                {
                    storeyCollection = new List<AOccupancyStorey>();
                    foreach (int number in ThAreaFrameUtils.ParseStoreyString(this.storeys))
                    {
                        storeyCollection.Add(new AOccupancyStorey { number = number, standard = false });
                    }
                    foreach (int number in ThAreaFrameUtils.ParseStandardStoreyString(this.storeys))
                    {
                        storeyCollection.Where(s => s.number == number).ForEach(s => s.standard = true);
                    }
                }

                return storeyCollection;
            }
        }

        public ThAreaFrameAOccupancy(string name)
        {
            layer = name;
        }

        // 主体
        public static ThAreaFrameAOccupancy Main(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy main = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                parkingStoreys      = tokens[6],
                storeys             = tokens[7],
                publicAreaID        = tokens[8],
                version             = tokens[9]
            };
            return main;
        }

        // 阳台
        public static ThAreaFrameAOccupancy Balcony(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy balcony = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                storeys             = tokens[6],
                publicAreaID        = tokens[7],
                version             = tokens[8]

            };
            return balcony;
        }

        // 架空
        public static ThAreaFrameAOccupancy Stilt(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy open = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                parkingStoreys      = tokens[6],
                storeys             = tokens[7],
                version             = tokens[8]

            };
            return open;
        }

        // 飘窗
        public static ThAreaFrameAOccupancy Baywindow(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy baywindow = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                storeys             = tokens[6],
                version             = tokens[7]

            };
            return baywindow;
        }

        // 雨棚
        public static ThAreaFrameAOccupancy Rainshed(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy rainshed = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                storeys             = tokens[6],
                version             = tokens[7]

            };
            return rainshed;
        }

        // 附属其他构件
        public static ThAreaFrameAOccupancy Miscellaneous(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy miscellaneous = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                component           = tokens[2],
                occupancyType       = tokens[3],
                category            = tokens[4],
                areaRatio           = tokens[5],
                floorAreaRatio      = tokens[6],
                storeys             = tokens[7],
                version             = tokens[8]

            };
            return miscellaneous;
        }

        // 是否在指定楼层
        public bool IsOnStorey(int stoery)
        {
            return this.StoreyCollection.Where(a => a.number == stoery).Any();
        }
    }

    // 公建建筑
    public class AOccupancyBuilding
    {
        public List<ThAreaFrameAOccupancy> aOccupancies;

        public AOccupancyBuilding()
        {
            aOccupancies = new List<ThAreaFrameAOccupancy>();
        }

        public static AOccupancyBuilding CreateWithLayers(string[] names)
        {
            var building = new AOccupancyBuilding();

            // 主体
            var main = names.Where(n => n.StartsWith(@"附属公建_主体"));
            if (main.Any())
            {
                building.aOccupancies.Add(ThAreaFrameAOccupancy.Main(main.ElementAt(0)));
            }

            // 阳台
            var balcony = names.Where(n => n.StartsWith(@"附属公建_阳台"));
            if (balcony.Any())
            {
                building.aOccupancies.Add(ThAreaFrameAOccupancy.Balcony(balcony.ElementAt(0)));
            }

            // 架空
            var stilt = names.Where(n => n.StartsWith(@"附属公建_架空"));
            if (stilt.Any())
            {
                building.aOccupancies.Add(ThAreaFrameAOccupancy.Stilt(stilt.ElementAt(0)));
            }

            // 飘窗
            var baywindow = names.Where(n => n.StartsWith(@"附属公建_飘窗"));
            if (baywindow.Any())
            {
                building.aOccupancies.Add(ThAreaFrameAOccupancy.Baywindow(baywindow.ElementAt(0)));
            }

            // 雨棚
            var rainshed = names.Where(n => n.StartsWith(@"附属公建_雨棚"));
            if (rainshed.Any())
            {
                building.aOccupancies.Add(ThAreaFrameAOccupancy.Rainshed(rainshed.ElementAt(0)));
            }

            // 附属其他构件
            var misc = names.Where(n => n.StartsWith(@"附属公建_附属其他构件"));
            if (misc.Any())
            {
                building.aOccupancies.Add(ThAreaFrameAOccupancy.Miscellaneous(misc.ElementAt(0)));
            }

            // 返回
            return building;
        }

        public bool Validate()
        {
            return aOccupancies.Count > 0;
        }

        // 所有标准楼层
        public List<List<AOccupancyStorey>> StandardStoreys()
        {
            return Storeys().Where(s => s.Count > 1).ToList();
        }

        // 所有普通楼层
        public List<AOccupancyStorey> OrdinaryStoreys()
        {
            var storeys = new List<AOccupancyStorey>();
            foreach (var item in Storeys().Where(s => (s.Count == 1) && (s[0].number > 0)))
            {
                storeys = storeys.Union(item).ToList();
            }
            return storeys;
        }

        // 所有地下楼层
        public List<AOccupancyStorey> UnderGroundStoreys()
        {
            var storeys = new List<AOccupancyStorey>();
            foreach (var item in Storeys().Where(s => (s.Count == 1) && (s[0].number < 0)))
            {
                storeys = storeys.Union(item).ToList();
            }
            return storeys;
        }

        // 所有楼层
        private List<List<AOccupancyStorey>> Storeys()
        {
            var storeys = new List<List<AOccupancyStorey>>();
            aOccupancies.Where(o => o.areaType == "主体").ForEach(o => storeys.Add(o.StoreyCollection));
            return storeys.Distinct().ToList();
        }
    }
}
