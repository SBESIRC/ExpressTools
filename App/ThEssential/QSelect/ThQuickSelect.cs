using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;

namespace ThEssential.QSelect
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
        QSelectFilterHatch = 0x20,
        /// <summary>
        /// 文字
        /// </summary>
        QSelectFilterText = 0x40,
        /// <summary>
        /// 最近的对象
        /// </summary>
        QSelectFilterLast = 0x80,
        /// <summary>
        /// 上一次选择
        /// </summary>
        QSelectFilterPrevious = 0x100
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
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.Color) == entity.EntityColor.ColorIndex);
                case QSelectFilterType.QSelectFilterLayer:
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == entity.Layer);
                case QSelectFilterType.QSelectFilterLineType:
                    return OpFilter.Bulid(o => o.Dxf((int)DxfCode.LinetypeName) == entity.Linetype);
                case QSelectFilterType.QSelectFilterBlock:
                    if(entity is BlockReference blk)
                    {
                        return OpFilter.Bulid(o => o.Dxf((int)DxfCode.BlockName) == blk.Name);
                    }
                    return null;
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
        public static Autodesk.AutoCAD.Colors.Color GetByLayerColor(Database db, Entity ent)
        {
            Autodesk.AutoCAD.Colors.Color color = ent.Color;
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            LayerTableRecord ltr = lt[ent.Layer].GetObject(OpenMode.ForRead) as LayerTableRecord;
            color = ltr.Color;
            return color;
        }
    }
}
