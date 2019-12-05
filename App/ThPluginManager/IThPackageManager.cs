using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThPluginManager
{
    /// <summary>
    /// Package封装类
    /// </summary>
    public interface IThPackage
    {
        //
    }

    /// <summary>
    /// Package管理器
    /// </summary>
    public interface IThPackageManager
    {
        // 创建Package
        void CreatePackage(IThPackage package);

        // 发布Package
        void PublishPackage(IThPackage package);
    }
}
