using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;

namespace ThEssential
{
    /// <summary>
    /// 过滤器类型
    /// </summary>
    public enum QSelectFilterType
    {
        /// <summary>
        /// 颜色
        /// </summary>
        QSelectFilterColor = 0x1,
        /// <summary>
        /// 图层名
        /// </summary>
        QSelectFilterLayer = 0x2,
        /// <summary>
        /// 线型名
        /// </summary>
        QSelectFilterLineType = 0x4,
        /// <summary>
        /// 块名
        /// </summary>
        QSelectFilterBlock = 0x8,
        /// <summary>
        /// 标注
        /// </summary>
        QSelectFilterDimension = 0x10,
        /// <summary>
        /// 填充
        /// </summary>
        QSelectFilterHatch = 0x20
    }

    /// <summary>
    /// 寻找模式
    /// </summary>
    public enum QSelectExtent
    {
        /// <summary>
        /// 全选
        /// </summary>
        QSelectAll = 0x1,
        /// <summary>
        /// 当前视图
        /// </summary>
        QSelectView = 0x2
    }

    public static class ThQuickSelect
    {
        /// <summary>
        /// 用实体属性创建选择过滤器
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public static SelectionFilter QSelectFilter(this Entity entity, QSelectFilterType filterType)
        {
            switch (filterType)
            {
                case QSelectFilterType.QSelectFilterColor:
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.Color) == entity.ColorIndex);
                case QSelectFilterType.QSelectFilterLayer:
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == entity.Layer);
                case QSelectFilterType.QSelectFilterLineType:
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.LinetypeName) == entity.Linetype);
                case QSelectFilterType.QSelectFilterBlock:
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.BlockName) == entity.BlockName);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 用实体类型创建选择过滤器
        /// </summary>
        /// <param name="dxfName"></param>
        /// <returns></returns>
        public static SelectionFilter QSelectFilter(this string dxfName)
        {
            return OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == dxfName);
        }
    }
}
