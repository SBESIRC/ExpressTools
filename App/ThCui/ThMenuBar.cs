using System.Collections.Specialized;
using Autodesk.AutoCAD.Customization;
using DotNetARX;

namespace TianHua.AutoCAD.ThCui
{
    public class ThMenuBar
    {
        public static void CreateThMenu(CustomizationSection cs)
        {
            //设置用于下拉菜单别名的字符串集合
            StringCollection sc = new StringCollection();
            sc.Add("THPop");
            //添加名为“我的菜单”的下拉菜单，如果已经存在，则返回null
            PopMenu theMenu = cs.MenuGroup.AddPopMenu("天华效率工具", sc, "ID_THMenu");
            if (theMenu != null)//如果“我的菜单”还没有被添加，则添加菜单项
            {
                theMenu.AddMenuItem(-1, "帮助文档", "ID_THHLP");
                theMenu.AddMenuItem(-1, "检查更新", "ID_THUPT");

                // 专业
                {
                    var subMenu = theMenu.AddSubMenu(-1, "专业", "ID_THMenu_Profile");
                    subMenu.AddMenuItem(-1, "建筑", "ID_THPROFILE _A");
                    subMenu.AddMenuItem(-1, "结构", "ID_THPROFILE _S");
                    subMenu.AddMenuItem(-1, "暖通", "ID_THPROFILE _H");
                    subMenu.AddMenuItem(-1, "电气", "ID_THPROFILE _E");
                    subMenu.AddMenuItem(-1, "给排水", "ID_THPROFILE _W");
                    subMenu.AddMenuItem(-1, "方案", "ID_THPROFILE _P");
                }

                // 第三方支持
                {
                    var subMenu = theMenu.AddSubMenu(-1, "第三方支持", "ID_THMenu_Miscellaneous");
                    subMenu.AddMenuItem(-1, "T20V4", "ID_T20V4");
                    subMenu.AddMenuItem(-1, "T20V5", "ID_T20V5");
                }
            }
        }
    }
}
