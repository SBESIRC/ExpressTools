using System.Collections.Generic;
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
            PopMenu thMenu = cs.MenuGroup.AddPopMenu("天华效率工具", sc, "ID_THMenu");
            if (thMenu != null)//如果“我的菜单”还没有被添加，则添加菜单项
            {
                var  menuModify = thMenu.AddSubMenu(-1, "图块图库", "ID_THMenu_TuKuaiTuKu");
                var kps = new List<ThCommandInfo> {
                    new ThCommandInfo("图块集 <THBLI>", "THBLI"),
                    new ThCommandInfo("图块集配置 <THBLS>", "THBLS"),
                    new ThCommandInfo("提电气块转换 <THBEE>", "THBEE")
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });

                menuModify = menuModify.AddSubMenu(-1, "图块断线", "ID_THMenu_TuKuaiSplit");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("插块断线 <THBBR>", "THBBR"),
                    new ThCommandInfo("选块断线 <THBBE>", "THBBE"),
                    new ThCommandInfo("全选断线 <THBBS>", "THBBS")
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });

                menuModify = thMenu.AddSubMenu(-1, "标注工具", "ID_THMenu_BiaoZhu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("车位编号 <THCNU>", "THCNU"),
                    new ThCommandInfo("尺寸避让 <THDTA>", "THDTA")
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });
                var subMenu = thMenu.AddSubMenu(-1, "图层工具", "ID_THMenu_TuCengGongJu");
                menuModify = subMenu.AddSubMenu(-1, "建立天华图层", "ID_THSubMenu_TH_Tucenggongju");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("建立建筑图层 <THALC>", "THALC"),
                    new ThCommandInfo("建立结构图层 <THSLC>", "THSLC"),
                    new ThCommandInfo("建立暖通图层 <THMLC>", "THMLC"),
                    new ThCommandInfo("建立电气图层 <THELC>", "THELC"),
                    new ThCommandInfo("建立给排水图层 <THPLC>", "THPLC"),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });
                menuModify = subMenu.AddSubMenu(-1, "处理建筑结构底图", "ID_THSubMenu_TH_JianZhuDiTu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("暖通用 <THLPM>", "THLPM"),
                    new ThCommandInfo("电气用 <THLPE>", "THLPE"),
                    new ThCommandInfo("给排水用 <THLPP>", "THLPP"),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });
                menuModify = subMenu.AddSubMenu(-1, "暖通图层管理", "ID_THSubMenu_TH_NuanTongTuCeng");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("锁定暖通图层 <THMLK>", "THMLK"),
                    new ThCommandInfo("隔离暖通图层 <THMUK>", "THMUK"),
                    new ThCommandInfo("解锁所有图层 <THUKA>", "THUKA"),
                    new ThCommandInfo("关闭暖通图层 <THMOF>", "THMOF"),
                    new ThCommandInfo("开启暖通图层 <THMON>", "THMON"),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });
                menuModify = thMenu.AddSubMenu(-1, "计算工具", "ID_THMenu_JiSuanGongJu");
                kps = new List<ThCommandInfo> {
                      new ThCommandInfo("天华单体规整 <THBPS>", "THBPS"),
                      new ThCommandInfo("单体面积汇总 <THBAC>", "THBAC"),
                      new ThCommandInfo("天华总平规整 <THSPS>", "THSPS"),
                      new ThCommandInfo("综合经济技术指标表 <THTET>", "THTET"),
                      new ThCommandInfo("防火分区疏散表 <THFET>", "THFET"),
                      new ThCommandInfo("房间面积框线 <THABC>", "THABC")
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });

                menuModify = thMenu.AddSubMenu(-1, "文字表格", "ID_THMenu_WenZiBiaoGe");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("文字内容刷 <THMTC>", "THMTC")
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });

                menuModify = thMenu.AddSubMenu(-1, "辅助工具", "ID_THMenu_FuZhuGongJu");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("批量缩放 <THMSC>", "THMSC"),
                    new ThCommandInfo("Z值归零 <THZ0>", "THZ0"),
#if ACAD2012 || ACAD2014
                    new ThCommandInfo("DGN清理 <DGNPURGE>", "DGNPURGE"),
#else
                    new ThCommandInfo("DGN清理 <DGNPURGE>", "PURGE"),
#endif
                    new ThCommandInfo("批量打印PDF <THBPT>", "THBPT"),
                    new ThCommandInfo("批量导出DWF <THBPD>", "THBPD"),
                    new ThCommandInfo("批量打印PPT <THBPP>", "THBPP"),
                    new ThCommandInfo("版次信息修改 <THSVM>", "THSVM"),
                    new ThCommandInfo("管线断线 <THLTR>", "THLTR"),
                    new ThCommandInfo("文字块镜像 <THMIR>", "THMIR")
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });
                menuModify = thMenu.AddSubMenu(-1, "第三方支持", "ID_THMenu_TZLOOK");
                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("T20V4 <T20V4>", "T20V4"),
                    new ThCommandInfo("T20V5 <T20V5>", "T20V5"),
                };
                kps.ForEach(kp =>
                {
                    menuModify.AddMenuItem(-1, kp.Name, kp.MacroId);
                });


                kps = new List<ThCommandInfo> {
                    new ThCommandInfo("帮助文档 <THHLP>", "THHLP"),
                    new ThCommandInfo("检查更新 <THUPT>", "THUPT"),
                };
                kps.ForEach(kp =>
                {
                    PopMenuItem pi = new PopMenuItem(thMenu, -1)
                    {
                        Name = kp.Name,
                        MacroID = kp.MacroId
                    };
                });
            }
        }
    }
}
