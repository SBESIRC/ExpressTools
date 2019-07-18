using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    public interface IThAreaFrameCalculator
    {
        // 楼层总面积
        double AreaOfFloor(int floor, bool far = false);

        // 标准楼层面积
        List<double> AreaOfStandardStoreys(bool far = false);

        // 地上建筑面积
        double AreaOfAboveGround(double roofArea);

        // 地下面积
        double AreaOfUnderGround();

        // 计容面积
        double AreaOfCapacityBuilding(double roofArea);

        // 架空
        double AreaOfStilt();

        // 地上层数
        int AboveGroundStoreyNumber();

        // 地下层数
        int UnderGroundStoreyNumber();

        // 普通楼层
        IEnumerable<int> OrdinaryStoreyCollection();
    }
}
