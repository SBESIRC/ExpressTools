using System;
using System.Linq;
using System.Collections.Generic;

namespace ThAreaFrame
{
    public class ThAreaFrameStoreyManager
    {
        public enum StoreyType
        {
            // 无效楼层
            STInvalid,
            // 标准住宅
            STResidentStandard,
            // 标准公建
            STAOccupancyStandard,
            // 标准混合体
            STCompositeStandard,
            // 普通住宅
            STResidentOrdinay,
            // 普通公建
            STAOccupancyOrdinay,
            // 普通混合体
            STCompositeOrdinay
        }

        // 住宅楼层
        private readonly List<ResidentialStorey> residentialStoreys;
        // 公建楼层
        private readonly List<AOccupancyStorey> aOccupancyStoreys;
        // 复合楼层
        private readonly List<CompositeStorey> compositeStoreys;

        // 构造函数
        public ThAreaFrameStoreyManager(ResidentialBuilding residential, AOccupancyBuilding aOccupancy)
        {
            // 住宅楼层
            residentialStoreys = residential.Storeys();
            residentialStoreys.Sort();

            // 公建楼层
            aOccupancyStoreys = aOccupancy.Storeys();
            aOccupancyStoreys.Sort();

            // 复合楼层
            compositeStoreys = new List<CompositeStorey>();
            if (residential.Validate() && aOccupancy.Validate())
            {
                int uppermostStorey = Math.Max(residentialStoreys.Max(o => o.number), aOccupancyStoreys.Max(o => o.number));

                // 从1层开始合并复合楼层
                for (int i = 1; i <= uppermostStorey; i++)
                {
                    var residentialStorey = residentialStoreys.Where(o => o.number == i);
                    var aOccupancyStorey = aOccupancyStoreys.Where(o => o.number == i);
                    if (residentialStorey.Any() && aOccupancyStorey.Any())
                    {
                        // 复合楼层
                        if (residentialStorey.First().standard == aOccupancyStorey.First().standard)
                        {
                            compositeStoreys.Add(new CompositeStorey()
                            {
                                number = i,
                                tag = residentialStorey.First().tag,
                                standard = residentialStorey.First().standard

                            });
                        }
                        else
                        {
                            compositeStoreys.Add(new CompositeStorey()
                            {
                                tag = 'c',
                                number = i,
                                standard = false

                            });
                        }

                        // 从原楼层删除
                        residentialStoreys.RemoveAll(o => o.number == i);
                        aOccupancyStoreys.RemoveAll(o => o.number == i);
                    }
                }
            }
        }


        // 普通楼层
        public List<int> OrdinaryStoreyCollection
        {
            get
            {
                var rStoreyEnumerator = residentialStoreys.Where(o => o.standard == false && o.number > 0).Select(o => o.number);
                var aStoreyEnumerator = aOccupancyStoreys.Where(o => o.standard == false && o.number > 0).Select(o => o.number);
                var cStoreyEnumerator = compositeStoreys.Where(o => o.standard == false && o.number > 0).Select(o => o.number);
                return rStoreyEnumerator.Union(aStoreyEnumerator).Union(cStoreyEnumerator).ToList();
            }
        }

        // 标准楼层
        public List<List<int>> StandardStoreyCollections
        {
            get
            {
                List<List<int>> storeys = new List<List<int>>();
                var storeyEnumerator = residentialStoreys.Where(
                    o => o.standard == true && 
                    o.number > 0 &&
                    o.tag == 'c').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.ConsecutiveSequences());

                storeyEnumerator = residentialStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'o').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.OddSequences());

                storeyEnumerator = residentialStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'j').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.EvenSequences());

                storeyEnumerator = aOccupancyStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'c').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.ConsecutiveSequences());

                storeyEnumerator = aOccupancyStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'o').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.OddSequences());

                storeyEnumerator = aOccupancyStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'j').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.EvenSequences());

                storeyEnumerator = compositeStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'c').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.ConsecutiveSequences());

                storeyEnumerator = compositeStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'o').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.OddSequences());

                storeyEnumerator = compositeStoreys.Where(
                    o => o.standard == true &&
                    o.number > 0 &&
                    o.tag == 'j').Select(o => o.number);
                storeys.AddRange(storeyEnumerator.EvenSequences());

                return storeys;
            }
        }

        // 地下楼层
        public List<int> UnderGroundStoreyCollection
        {
            get
            {
                var rStoreyEnumerator = residentialStoreys.Where(o => o.number < 0).Select(o => o.number);
                var aStoreyEnumerator = aOccupancyStoreys.Where(o => o.number < 0).Select(o => o.number);
                var cStoreyEnumerator = compositeStoreys.Where(o => o.number < 0).Select(o => o.number);
                return rStoreyEnumerator.Union(aStoreyEnumerator).Union(cStoreyEnumerator).ToList();
            }
        }
    }
}