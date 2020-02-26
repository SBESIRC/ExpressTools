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
                ToolbarButton button = null;
                button = toolbar.AddToolbarButton(0, "向上对齐", "ID_THALIGN _TOP");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(1, "水平居中", "ID_THALIGN _HORIZONTAL");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(2, "向下对齐", "ID_THALIGN _BOTTOM");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(3, "向左对齐", "ID_THALIGN _LEFT");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(4, "垂直居中", "ID_THALIGN _VERTICAL");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(5, "向右对齐", "ID_THALIGN _RIGHT");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(6, "水平均分", "ID_THALIGN _XDISTRIBUTE");
                toolbar.ToolbarItems.Add(button);
                toolbar.AddToolbarButton(7, "垂直均分", "ID_THALIGN _YDISTRIBUTE");
                toolbar.ToolbarItems.Add(button);
            }
        }
    }
}