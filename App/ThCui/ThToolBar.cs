using Autodesk.AutoCAD.Customization;
using DotNetARX;

namespace TianHua.AutoCAD.ThCui
{
    public class ThToolBar
    {
        public static void CreateThToolbar(CustomizationSection cs)
        {
            Toolbar toolbar = cs.MenuGroup.AddToolbar("天华工具栏");
            if (toolbar != null)
            { 
                toolbar.AddToolbarButton(-1, "颜色选择", "ID_THQS _COLOR");
                toolbar.AddToolbarButton(-1, "图层选择", "ID_THQS _LAYER");
                toolbar.AddToolbarButton(-1, "线型选择", "ID_THQS _LINETYPE");
                toolbar.AddToolbarButton(-1, "标注选择", "ID_THQS _DIMENSION");
                toolbar.AddToolbarButton(-1, "填充选择", "ID_THQS _HATCH");
                toolbar.AddToolbarButton(-1, "文字选择", "ID_THQS _TEXT");
                toolbar.AddToolbarButton(-1, "图块名选择", "ID_THQS _BLOCK");
                toolbar.AddToolbarButton(-1, "上次建立", "ID_THQS _LASTAPPEND");
                toolbar.AddToolbarButton(-1, "上次选取", "ID_THQS _PREVIOUS");
                toolbar.AddToolbarButton(-1, "向上对齐", "ID_THAL _TOP");
                toolbar.AddToolbarButton(-1, "水平居中", "ID_THAL _HORIZONTAL");
                toolbar.AddToolbarButton(-1, "向下对齐", "ID_THAL _BOTTOM");
                toolbar.AddToolbarButton(-1, "向左对齐", "ID_THAL _LEFT");
                toolbar.AddToolbarButton(-1, "垂直居中", "ID_THAL _VERTICAL");
                toolbar.AddToolbarButton(-1, "向右对齐", "ID_THAL _RIGHT");
                toolbar.AddToolbarButton(-1, "水平均分", "ID_THAL _XDISTRIBUTE");
                toolbar.AddToolbarButton(-1, "垂直均分", "ID_THAL _YDISTRIBUTE");
            }
        }
    }
}