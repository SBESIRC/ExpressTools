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
        public void LoadThMenu()
        {
            //获得活动文档
            Document activeDoc = AcadApp.DocumentManager.MdiActiveDocument;
            string menuGroupName = "TianHuaCustom";
            var cuiFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents\Resources",
                                @"THMenubar.cuix");
            CustomizationSection cs = activeDoc.AddCui(cuiFile, menuGroupName);
            cs.LoadCui();
        }

        public void CreateTHMenu()
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
                    new ThCommandInfo("图块集 <THBLI>", "THBLI", "打开图块集工具选项板",false),
                    new ThCommandInfo("图块集配置 <THBLS>", "THBLS", "配置各专业图块集",false),
                    new ThCommandInfo("提电气块转换 <THBEE>", "THBEE", "将暖通和给排水专业提资给电气的图块转换为电气专业所需的图块",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "标注工具", "ID_THMenu_BiaoZhu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("车位编号 <THCNU>", "THCNU", "绘制多段线穿过所需编号停车位图块，根据多段线穿过停车位的先后顺序快速生成车位编号:",false),
                    new ThCommandInfo("尺寸避让 <THDTA>", "THDTA", "调整交叉或重叠的标注文字以避免发生位置冲突",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                var subMenu = thMenu.AddSubMenu(-1, "图层工具", "ID_THMenu_TuCengGongJu");
                menuModify = subMenu.AddSubMenu(-1, "建立天华图层", "ID_THSubMenu_TH_Tucenggongju");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("建立建筑图层 <THALC>", "THALC", "建立建筑专业天华标准图层",true),
                    new ThCommandInfo("建立结构图层 <THSLC>", "THSLC", "建立结构专业天华标准图层",true),
                    new ThCommandInfo("建立暖通图层 <THMLC>", "THMLC", "建立暖通专业天华标准图层",true),
                    new ThCommandInfo("建立电气图层 <THELC>", "THELC", "建立电气专业天华标准图层",true),
                    new ThCommandInfo("建立给排水图层 <THPLC>", "THPLC", "建立给排专业天华标准图层",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });

                menuModify = subMenu.AddSubMenu(-1, "处理建筑结构底图", "ID_THSubMenu_TH_JianZhuDiTu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("暖通用 <THLPM>", "THLPM", "处理建筑结构提暖通底图的各图层颜色至相应的色号",true),
                    new ThCommandInfo("电气用 <THLPE>", "THLPE", "处理建筑结构提电气底图的各图层颜色至相应的色号",true),
                    new ThCommandInfo("给排水用 <THLPP>", "THLPP", "处理建筑结构提给排水底图的各图层颜色至相应的色号",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });

                menuModify = subMenu.AddSubMenu(-1, "暖通图层管理", "ID_THSubMenu_TH_NuanTongTuCeng");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("锁定暖通图层 <THMLK>", "THMLK", "锁定所有暖通图层",true),
                    new ThCommandInfo("隔离暖通图层 <THMUK>", "THMUK", "解锁所有暖通图层，同时锁定其他图层",true),
                    new ThCommandInfo("解锁所有图层 <THUKA>", "THUKA", "解锁所有图层",true),
                    new ThCommandInfo("关闭暖通图层 <THMOF>", "THMOF", "关闭所有暖通图层",true),
                    new ThCommandInfo("开启暖通图层 <THMON>", "THMON", "开启所有暖通图层",true),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "计算工具", "ID_THMenu_JiSuanGongJu");
                kps = new List<ThCommandInfo> {
                      new ThCommandInfo("天华单体规整 <THBPS>", "THBPS", "将建筑单体各层平面图中代表各区域的多段线图元设置到相应的图层，以供生成单体面积汇总表所用",true),
                      new ThCommandInfo("单体面积汇总 <THBAC>", "THBAC", "汇总单体每层各区域建筑面积和计容面积",true),
                      new ThCommandInfo("天华总平规整 <THSPS>", "THSPS", "将总平面图中代表各区域的多段线图元设置到相应的图层，以供生成综合经济技术指标表所用",true),
                      new ThCommandInfo("综合经济技术指标表 <THTET>", "THTET", "汇总总平面及各单体各区域建筑面积和计容面积，形成综合经济技术指标表",true),
                      new ThCommandInfo("消防疏散表 <THFET>", "THFET", "统计商业/地库各防火分区面积，自动计算应有疏散距离，并生成表格",true),
                      new ThCommandInfo("房间面积框线 <THABC>", "THABC", "自动生成屏幕选择范围内所有房间的框线，且可选择插入面积值",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });

                menuModify = thMenu.AddSubMenu(-1, "系统详图", "ID_THMenu_XiTongXiangTu");
                kps = new List<ThCommandInfo> {

                      new ThCommandInfo("配电箱系统图修改 <THLDC>", "THLDC", "识别图纸系统图中各配电箱的回路、开关型号等相关信息。修改回路功率值，回路和配电箱其他参数自动更新",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "辅助工具", "ID_THMenu_FuZhuGongJu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("批量缩放 <THMSC>", "THMSC", "对多个选择对象以各自的开始点（插入点）为基准点进行批量比例缩放",false),
                    new ThCommandInfo("Z值归零 <THZ0>", "THZ0", "将模型空间内所有对象Z值归零，使之处于同一平面",true),
#if ACAD2012 || ACAD2014
                    new ThCommandInfo("DGN清理 <DGNPURGE>", "DGNPURGE", "清理图纸中多余DGN对象，含多余的DGN线型、注释比例等",true),
#else
                    new ThCommandInfo("DGN清理 <DGNPURGE>", "PURGE", "清理图纸中多余DGN对象，含多余的DGN线型、注释比例等",true),
#endif
                    new ThCommandInfo("批量打印PDF <THBPT>", "THBPT", "选择需要批量打印的天华图框，将图纸批量打印为PDF文件，读取图框中的图纸编号重命名相应PDF文件",true),
                    new ThCommandInfo("批量导出DWF <THBPD>", "THBPD", "选择需要批量打印的天华图框，将图纸批量打印为DWF文件，读取图框中的图纸编号重命名相应DWF文件",true),
                    new ThCommandInfo("批量打印PPT <THBPP>", "THBPP", "选择需要批量打印的PPT框线，将图纸批量打印为单个PPT文件，读取打印窗口框线与PPT框线的位置完成PPT内图片的定位",true),
                    new ThCommandInfo("版次信息修改 <THSVM>", "THSVM", "批量修改图框内的版次信息或出图日期",true)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });


                menuModify = thMenu.AddSubMenu(-1, "文字表格", "ID_THMenu_WenZiBiaoGe");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("文字内容刷 <THMTC>", "THMTC", "将目标文字内容替换为源文字内容",false)
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, id + kp.Command);
                    cs.AddMacro(kp.Name, marco + kp.Command + " ", id + kp.Command, kp.HelpString, imageFolderPath + kp.Command + kp.Suffix);
                });

                {
                    var cmdkp = new ThCommandInfo("帮助文档 <THHLP>", "THHLP", "获取帮助文档", false);
                    cs.AddMacro(cmdkp.Name, marco + cmdkp.Command + " ", id + cmdkp.Command, cmdkp.HelpString, imageFolderPath + cmdkp.Command + cmdkp.Suffix);

                    PopMenuItem newPmi = new PopMenuItem(thMenu, -1);
                    if (cmdkp.Name != null) newPmi.Name = cmdkp.Name;
                    newPmi.MacroID = id + cmdkp.Command;
                }

                {
                    var cmdkp = new ThCommandInfo("检查更新 <THUPT>", "THUPT", "检查更新", true);
                    cs.AddMacro(cmdkp.Name, marco + cmdkp.Command + " ", id + cmdkp.Command, cmdkp.HelpString, imageFolderPath + cmdkp.Command + cmdkp.Suffix);

                    PopMenuItem newPmi = new PopMenuItem(thMenu, -1);
                    if (cmdkp.Name != null) newPmi.Name = cmdkp.Name;
                    newPmi.MacroID = id + cmdkp.Command;

                }

            }

            cs.LoadCui();//必须装载CUI文件，才能看到添加的菜单

        }
    }
}
