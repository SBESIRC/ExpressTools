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

        // 楼梯间面积
        double AreaOfRoof(bool far = false);

        // 地上面积
        double AreaOfAboveGround(bool far = false);

        // 地下面积
        double AreaOfUnderGround(bool far = false);

        // 计容面积
        double AreaOfCapacityBuilding(bool far/*Floor Area Ratio*/ = false);

        // 架空
        double AreaOfStilt();

        // 地上层数
        int AboveGroundStoreyNumber();

        // 地下层数
        int UnderGroundStoreyNumber();

        // 普通楼层
        IEnumerable<int> OrdinaryStoreyCollection();

        // 标准楼层
        IEnumerable<int> StandardStoreyCollection();

        // 地下楼层
        IEnumerable<int> UnderGroundStoreyCollection();
    }
}
