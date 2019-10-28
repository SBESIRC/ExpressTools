using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThAreaFrame
{
    public interface IThAreaFrameDataSource : IDisposable
    {
        // 获取特定图层中所有面积框线的面积总和
        double SumOfArea(string layer);

        // 获取特定图层中面积框线的个数
        int CountOfAreaFrames(string layer);

        // 获取特定套内的套数
        int CountOfDwelling(string dwellingID);

        // 图层
        List<string> Layers();
    }
}
