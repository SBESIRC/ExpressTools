using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace TianHua.AutoCAD.ThCui
{
    public class ThMenuBar
    {
        public void AddTHMenu()
        {
            //获取当前dll所在的目录
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var direc = Path.GetDirectoryName(path);


            //设置CUI文件的名字，将其路径设置为当前运行目录
            string cuiFile = Path.Combine(direc.Right(@"App\ThCui\bin\Debug"), @"AutoLoader\Contents\Resources", "TianHuaCustom.cuix");
            string menuGroupName = "TianHuaCustom";//菜单组名
            //获得活动文档
            Document activeDoc = AcadApp.DocumentManager.MdiActiveDocument;

            //string currentPath = direc;//当前运行目录
            //装载局部CUI文件，若不存在，则创建
            CustomizationSection cs = activeDoc.AddCui(cuiFile, menuGroupName);

            //设置用于下拉菜单别名的字符串集合
            StringCollection sc = new StringCollection();
            sc.Add("THPop");

            //添加名为“我的菜单”的下拉菜单，如果已经存在，则返回null
            PopMenu thMenu = cs.MenuGroup.AddPopMenu("天华效率工具", sc, "ID_THMenu");
            if (thMenu != null)//如果“我的菜单”还没有被添加，则添加菜单项
            {
                var imageFolderPath = direc.Right(@"bin\Debug") + @"Images\";
                List<ThCommandInfo> kps = null;
                var marco = @"^C^C_";
                var id = @"ID_";

                //添加一个子菜单
                PopMenu menuModify = thMenu.AddSubMenu(-1, "图块图库", "ID_THMenu_TuKuaiTuKu");

                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("图块集", "THBLI", "打开图块集工具选项板",false),
                    new ThCommandInfo("图块集配置", "THBLS", "配置各专业图块集",false),
                    new ThCommandInfo("提电气块转换", "THBEE", "将暖通和给排水专业提资给电气的图块转换为电气专业所需的图块",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "标注工具", "ID_THMenu_BiaoZhu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("车位编号", "THCNU", "绘制多段线穿过所需编号停车位图块，根据多段线穿过停车位的先后顺序快速生成车位编号:",false),
                    new ThCommandInfo("尺寸避让", "THDTA", "调整交叉或重叠的标注文字以避免发生位置冲突",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                var subMenu = thMenu.AddSubMenu(-1, "图层工具", "ID_THMenu_TuCengGongJu");
                menuModify = subMenu.AddSubMenu(-1, "建立天华图层", "ID_THSubMenu_TH_Tucenggongju");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("建立建筑图层", "THALC", "建立建筑专业天华标准图层",true),
                    new ThCommandInfo("建立结构图层", "THSLC", "建立结构专业天华标准图层",true),
                    new ThCommandInfo("建立暖通图层", "THSLC", "建立暖通专业天华标准图层",true),
                    new ThCommandInfo("建立电气图层", "THELC", "建立电气专业天华标准图层",true),
                    new ThCommandInfo("建立给排水图层", "THPLC", "建立给排专业天华标准图层",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });

                menuModify = subMenu.AddSubMenu(-1, "处理建筑结构底图", "ID_THSubMenu_TH_JianZhuDiTu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("暖通用", "THLPM", "处理建筑结构提暖通底图的各图层颜色至相应的色号",true),
                    new ThCommandInfo("电气用", "THLPE", "处理建筑结构提电气底图的各图层颜色至相应的色号",true),
                    new ThCommandInfo("给排水用", "THLPP", "处理建筑结构提给排水底图的各图层颜色至相应的色号",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });

                menuModify = subMenu.AddSubMenu(-1, "暖通图层管理", "ID_THSubMenu_TH_NuanTongTuCeng");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("锁定暖通图层", "THMLK", "锁定所有暖通图层",true),
                    new ThCommandInfo("隔离暖通图层", "THMUK", "解锁所有暖通图层，同时锁定其他图层",true),
                    new ThCommandInfo("解锁所有图层", "THUKA", "解锁所有图层",true),
                    new ThCommandInfo("关闭暖通图层", "THMOF", "关闭所有暖通图层",true),
                    new ThCommandInfo("开启暖通图层", "THMON", "开启所有暖通图层",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "计算工具", "ID_THMenu_JiSuanGongJu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("单体面积汇总", "THBAC", "汇总单体每层各区域建筑面积和计容面积",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "辅助工具", "ID_THMenu_FuZhuGongJu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("批量缩放", "THMSC", "对多个选择对象以各自的开始点（插入点）为基准点进行批量比例缩放",false),
                    new ThCommandInfo("Z值归零", "THZ0", "将模型空间内所有对象Z值归零，使之处于同一平面",true),
                    new ThCommandInfo("DGN清理", "DGNPURGE", "清理图纸中多余DGN对象，含多余的DGN线型、注释比例等",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "文字表格", "ID_THMenu_WenZiBiaoGe");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("文字内容刷", "THMTC", "将目标文字内容替换为源文字内容",false)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


            }
            //cs.LoadCui();//必须装载CUI文件，才能看到添加的菜单

        }
    }
}
