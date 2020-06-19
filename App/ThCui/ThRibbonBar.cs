﻿using Autodesk.AutoCAD.Customization;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public class ThRibbonBar
    { 
        public static void CreateThRibbonBar(CustomizationSection cs)
        {
            var tab = cs.AddNewTab(ThCADCommon.RibbonTabName, ThCADCommon.RibbonTabTitle);
            if (tab != null)
            {
                CreateHelpPanel(tab);
                CreateSettingsPanel(tab);
                CreateCheckToolPanel(tab);
                CreatStatisticPanel(tab);
                CreateDrawToolPanel(tab);
                CreatePlotToolPanel(tab);
                CreatePurgeToolPanel(tab);
                CreateBlockToolPanel(tab);
                CreateMiscellaneousPanel(tab);
                CreateSitePlanToolPanel(tab);
            }
        }

        private static void CreateHelpPanel(RibbonTabSource tab)
        {
            // 登录界面
            var panel = tab.AddNewPanel("Help", "登录界面");
            var row = panel.AddNewRibbonRow();

            // 登录
            {
                var subPanel = row.AddNewPanel();

                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("登录",
                    "天华登录",
                    ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME,
                    "登录天华效率平台",
                    "IDI_THCAD_THLOGIN_SMALL",
                    "IDI_THCAD_THLOGIN_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            // 退出
            {
                var subPanel = row.AddNewPanel();

                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("退出",
                    "天华退出",
                    ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME,
                    "退出天华效率平台",
                    "IDI_THCAD_THLOGOUT_SMALL",
                    "IDI_THCAD_THLOGOUT_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            {
                var subPanel = row.AddNewPanel();

                // 专业切换
                var subRow = subPanel.AddNewRibbonRow();
                var splitButton = subRow.AddNewSplitButton(
                    "专业切换",
                    RibbonSplitButtonBehavior.SplitFollow,
                    RibbonSplitButtonListStyle.IconText,
                    RibbonButtonStyle.SmallWithText);

                // 方案专业
                splitButton.AddNewButton("方案",
                    "天华方案",
                    "THPROFILE _P",
                    "切换到天华方案",
                    "IDI_THCAD_PROJECT_PLAN_SMALL",
                    "IDI_THCAD_PROJECT_PLAN_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 建筑专业
                splitButton.AddNewButton("建筑专业",
                    "天华建筑",
                    "THPROFILE _A",
                    "切换到天华建筑",
                    "IDI_THCAD_ARCHITECTURE_SMALL",
                    "IDI_THCAD_ARCHITECTURE_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 结构专业
                splitButton.AddNewButton("结构专业",
                    "天华结构",
                    "THPROFILE _S",
                    "切换到天华结构",
                    "IDI_THCAD_STRUCTURE_SMALL",
                    "IDI_THCAD_STRUCTURE_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 暖通专业
                splitButton.AddNewButton("暖通专业",
                    "天华暖通",
                    "THPROFILE _H",
                    "切换到天华暖通",
                    "IDI_THCAD_HAVC_SMALL",
                    "IDI_THCAD_HAVC_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 电气专业
                splitButton.AddNewButton("电气专业",
                    "天华电气",
                    "THPROFILE _E",
                    "切换到天华电气",
                    "IDI_THCAD_ELECTRICAL_SMALL",
                    "IDI_THCAD_ELECTRICAL_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 给排水专业
                splitButton.AddNewButton("给排水专业",
                    "天华给排水",
                    "THPROFILE _W",
                    "切换到天华给排水",
                    "IDI_THCAD_WATER_SMALL",
                    "IDI_THCAD_WATER_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 帮助文档
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("帮助文档",
                    "天华帮助",
                    ThCuiCommon.CMD_THHLP_GLOBAL_NAME,
                    "获取帮助文档",
                    "IDI_THCAD_THHLP_SMALL",
                    "IDI_THCAD_THHLP_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 反馈意见
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("反馈意见",
                    "天华反馈",
                    "THFBK",
                    "反馈意见",
                    "IDI_THCAD_THFBK_SMALL",
                    "IDI_THCAD_THFBK_LARGE",
                    RibbonButtonStyle.SmallWithText);
            }
        }

        private static void CreateSettingsPanel(RibbonTabSource tab)
        {
            // 设置
            var panel = tab.AddNewPanel("Settings", "设置");
            var row = panel.AddNewRibbonRow();

            // 快捷键
            row.AddNewButton("快捷键",
                "天华快捷键设置",
                "THALIAS",
                "设置快捷键",
                "IDI_THCAD_THALIAS_SMALL",
                "IDI_THCAD_THALIAS_LARGE",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreatStatisticPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Statistic", "分析计算");
            var row = panel.AddNewRibbonRow();

            // 天华单体规整
            row.AddNewButton("天华单体\r\n规整",
                "天华单体规整",
                "THBPS",
                "将建筑单体各层平面图中代表各区域的多段线图元设置到相应的图层，以供生成单体面积汇总表所用",
                "IDI_THCAD_THBPS_SMALL",
                "IDI_THCAD_THBPS_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 天华总平规整
            row.AddNewButton("天华总平\r\n规整",
                "天华总平规整",
                "THSPS",
                "将总平面图中代表各区域的多段线图元设置到相应的图层，以供生成综合经济技术指标表所用",
                "IDI_THCAD_THSPS_SMALL",
                "IDI_THCAD_THSPS_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 单体面积汇总
            row.AddNewButton("单体面积\r\n汇总",
                "单体面积汇总",
                "THBAC",
                "汇总单体每层各区域建筑面积和计容面积",
                "IDI_THCAD_THBAC_SMALL",
                "IDI_THCAD_THBAC_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 综合经济技术指标表
            row.AddNewButton("综合经济\r\n技术指标表",
                "综合经济技术指标表",
                "THTET",
                "汇总总平面及各单体各区域建筑面积和计容面积，形成综合经济技术指标表",
                "IDI_THCAD_THTET_SMALL",
                "IDI_THCAD_THTET_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 防火分区疏散表
            row.AddNewButton("防火分区\r\n疏散表",
                "防火分区疏散表",
                "THFET",
                "统计商业/地库各防火分区面积，自动计算应有疏散距离，并生成表格",
                "IDI_THCAD_THFET_SMALL",
                "IDI_THCAD_THFET_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 房间面积框线
            row.AddNewButton("房间面积\r\n框线",
                "房间面积框线",
                "THABC",
                "自动生成屏幕选择范围内所有房间的框线，且可选择插入面积值",
                "IDI_THCAD_THABC_SMALL",
                "IDI_THCAD_THABC_LARGE",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreatePlotToolPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("PlotTool", "布图打印");
            var row = panel.AddNewRibbonRow();

            {
                var subPanel = row.AddNewPanel();

                // 批量打印PDF
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量打印PDF",
                    "批量打印PDF",
                    "THBPT",
                    "选择需要批量打印的天华图框，将图纸批量打印为PDF文件，读取图框中的图纸编号重命名相应PDF文件",
                    "IDI_THCAD_THBPT_SMALL",
                    "IDI_THCAD_THBPT_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 批量打印DWF
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量打印DWF",
                    "批量打印DWF",
                    "THBPD",
                    "选择需要批量打印的天华图框，将图纸批量打印为DWF文件，读取图框中的图纸编号重命名相应DWF文件",
                    "IDI_THCAD_THBPD_SMALL",
                    "IDI_THCAD_THBPD_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 批量打印PPT
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量打印PPT",
                    "批量打印PPT",
                    "THBPP",
                    "选择需要批量打印的PPT框线，将图纸批量打印为单个PPT文件，读取打印窗口框线与PPT框线的位置完成PPT内图片的定位",
                    "IDI_THCAD_THBPP_SMALL",
                    "IDI_THCAD_THBPP_LARGE",
                    RibbonButtonStyle.SmallWithText);
            }
        }

        private static void CreateDrawToolPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("DrawTool", "绘图修改");
            var row = panel.AddNewRibbonRow();

            // 车位编号
            {
                var subPanel = row.AddNewPanel();

                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("车位编号",
                    "天华车位编号",
                    "THCNU",
                    "绘制多段线穿过所需编号停车位图块，根据多段线穿过停车位的先后顺序快速生成车位编号",
                    "IDI_THCAD_THCNU_SMALL",
                    "IDI_THCAD_THCNU_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            // 喷头布置
            {
                var subPanel = row.AddNewPanel();

                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("喷头布置",
                    "喷头布置",
                    "THSPC",
                    "1. 点击房间内一点自动布置喷淋点位 2.选择房间框线布置喷淋点位 3.绘制房间框线布置喷淋点位",
                    "IDI_THCAD_THSPC",
                    "IDI_THCAD_THSPC",
                    RibbonButtonStyle.LargeWithText);
            }

            // 天华快选
            {
                var subPanel = row.AddNewPanel();
                var subRow = subPanel.AddNewRibbonRow();
                var splitButton = subRow.AddNewSplitButton("快选命令集",
                    RibbonSplitButtonBehavior.SplitFollow,
                    RibbonSplitButtonListStyle.IconText,
                    RibbonButtonStyle.LargeWithText);

                // 颜色
                splitButton.AddNewButton("颜色",
                    "按颜色选取",
                    "THQS _COLOR",
                    "按颜色选取",
                    "IDI_THCAD_THQS_COLOR_SMALL",
                    "IDI_THCAD_THQS_COLOR_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 图层
                splitButton.AddNewButton("图层",
                    "按图层选取",
                    "THQS _LAYER",
                    "按图层选取",
                    "IDI_THCAD_THQS_LAYER_SMALL",
                    "IDI_THCAD_THQS_LAYER_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 线型
                splitButton.AddNewButton("线型",
                    "按线型选取",
                    "THQS _LINETYPE",
                    "按线型选取",
                    "IDI_THCAD_THQS_LINETYPE_SMALL",
                    "IDI_THCAD_THQS_LINETYPE_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 标注
                splitButton.AddNewButton("标注",
                    "按标注选取",
                    "THQS _DIMENSION",
                    "按标注选取",
                    "IDI_THCAD_THQS_ANNOTATION_SMALL",
                    "IDI_THCAD_THQS_ANNOTATION_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 填充
                splitButton.AddNewButton("填充",
                    "按填充选取",
                    "THQS _HATCH",
                    "按填充选取",
                    "IDI_THCAD_THQS_HATCH_SMALL",
                    "IDI_THCAD_THQS_HATCH_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 文字
                splitButton.AddNewButton("文字",
                    "按文字选取",
                    "THQS _TEXT",
                    "按文字选取",
                    "IDI_THCAD_THQS_TEXT_SMALL",
                    "IDI_THCAD_THQS_TEXT_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 图块名
                splitButton.AddNewButton("图块名",
                    "按图块名选取",
                    "THQS _BLOCK",
                    "按图块名选取",
                    "IDI_THCAD_THQS_BLOCK_SMALL",
                    "IDI_THCAD_THQS_BLOCK_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 分割线
                splitButton.AddNewSeparator(RibbonSeparatorStyle.Line);


                // 上次建立
                splitButton.AddNewButton("上次建立",
                    "按上次建立选择",
                    "THQS _LASTAPPEND",
                    "上次建立",
                    "IDI_THCAD_THQS_LASTAPPEND_SMALL",
                    "IDI_THCAD_THQS_LASTAPPEND_LARGE",
                    RibbonButtonStyle.LargeWithText);


                // 上次选择
                splitButton.AddNewButton("上次选择",
                    "按上次选择选择",
                    "THQS _PREVIOUS",
                    "上次选择",
                    "IDI_THCAD_THQS_LASTSELECT_SMALL",
                    "IDI_THCAD_THQS_LASTSELECT_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            // 天华对齐
            {
                var subPanel = row.AddNewPanel();
                var subRow = subPanel.AddNewRibbonRow();
                var splitButton = subRow.AddNewSplitButton("对齐命令集",
                    RibbonSplitButtonBehavior.SplitFollow,
                    RibbonSplitButtonListStyle.IconText,
                    RibbonButtonStyle.LargeWithText);

                // 向上对齐
                splitButton.AddNewButton("向上对齐",
                    "天华向上对齐",
                    "THAL _TOP",
                    "向上对齐",
                    "IDI_THCAD_THALIGN_TOP_SMALL",
                    "IDI_THCAD_THALIGN_TOP_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 向下对齐
                splitButton.AddNewButton("向下对齐",
                    "天华向下对齐",
                    "THAL _BOTTOM",
                    "向下对齐",
                    "IDI_THCAD_THALIGN_BOTTOM_SMALL",
                    "IDI_THCAD_THALIGN_BOTTOM_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 向左对齐
                splitButton.AddNewButton("向左对齐",
                    "天华向左对齐",
                    "THAL _LEFT",
                    "向左对齐",
                    "IDI_THCAD_THALIGN_LEFT_SMALL",
                    "IDI_THCAD_THALIGN_LEFT_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 向右对齐
                splitButton.AddNewButton("向右对齐",
                    "天华向右对齐",
                    "THAL _RIGHT",
                    "向右对齐",
                    "IDI_THCAD_THALIGN_RIGHT_SMALL",
                    "IDI_THCAD_THALIGN_RIGHT_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 分割线
                splitButton.AddNewSeparator(RibbonSeparatorStyle.Line);

                // 水平居中
                splitButton.AddNewButton("水平居中",
                    "天华水平居中",
                    "THAL _HORIZONTAL",
                    "水平居中",
                    "IDI_THCAD_THALIGN_HORIZONTAL_SMALL",
                    "IDI_THCAD_THALIGN_HORIZONTAL_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 垂直居中
                splitButton.AddNewButton("垂直居中",
                    "天华垂直居中",
                    "THAL _VERTICAL",
                    "垂直居中",
                    "IDI_THCAD_THALIGN_VERTICAL_SMALL",
                    "IDI_THCAD_THALIGN_VERTICAL_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 分割线
                splitButton.AddNewSeparator(RibbonSeparatorStyle.Line);

                // 水平均分
                splitButton.AddNewButton("水平均分",
                    "天华水平均分",
                    "THAL _XDISTRIBUTE",
                    "水平方向平均分布",
                    "IDI_THCAD_THALIGN_XDISTRIBUTE_SMALL",
                    "IDI_THCAD_THALIGN_XDISTRIBUTE_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 垂直均分
                splitButton.AddNewButton("垂直均分",
                    "天华垂直均分",
                    "THAL _YDISTRIBUTE",
                    "水平方向平均分布",
                    "IDI_THCAD_THALIGN_YDISTRIBUTE_SMALL",
                    "IDI_THCAD_THALIGN_YDISTRIBUTE_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            {
                var subPanel = row.AddNewPanel();

                // 天华复制
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("天华复制",
                    "天华复制",
                    "THCO",
                    "提供更灵活的均分和成倍复制",
                    "IDI_THCAD_THCP_SMALL",
                    "IDI_THCAD_THCP_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 批量缩放
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量缩放",
                    "天华批量缩放",
                    "THMSC",
                    "对多个选择对象以各自的开始点（插入点）为基准点进行批量比例缩放",
                    "IDI_THCAD_THMSC_SMALL",
                    "IDI_THCAD_THMSC_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 尺寸避让
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("尺寸避让",
                    "天华尺寸避让",
                    "THDTA",
                    "调整交叉或重叠的标注文字以避免发生位置冲突",
                    "IDI_THCAD_THDTA_SMALL",
                    "IDI_THCAD_THDTA_LARGE",
                    RibbonButtonStyle.SmallWithText);
            }

            {
                var subPanel = row.AddNewPanel();

                // 天华格式刷
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("天华格式刷",
                    "天华格式刷",
                    "THMA",
                    "将目标对象的某些属性刷取为源对象的对应属性",
                    "IDI_THCAD_THMA_SMALL",
                    "IDI_THCAD_THMA_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 版次信息修改
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("版次信息修改",
                    "版次信息修改",
                    "THSVM",
                    "批量修改图框内的版次信息或出图日期",
                    "IDI_THCAD_THSVM_SMALL",
                    "IDI_THCAD_THSVM_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // 管线断线
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("管线断线",
                    "管线断线",
                    "THLTR",
                    "批量处理管线断线",
                    "IDI_THCAD_THLTR_SMALL",
                    "IDI_THCAD_THLTR_LARGE",
                    RibbonButtonStyle.SmallWithText);
            }
        }

        private static void CreatePurgeToolPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("PurgeTool", "图纸清理");
            var row = panel.AddNewRibbonRow();

            {
                var subPanel = row.AddNewPanel();

                // Z值归零
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("Z值归零",
                    "天华Z值归零",
                    "THZ0",
                    "将模型空间内所有对象Z值归零，使之处于同一平面",
                    "IDI_THCAD_THZ0_SMALL",
                    "IDI_THCAD_THZ0_LARGE",
                    RibbonButtonStyle.SmallWithText);

                // DGN清理
                // 从AutoCAD 2016开始，“PURGE”命令可以实现“DGNPURGE”的功能
                // 这里直接将“DGNPURGE”切换到“PURGE”命令
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("DGN清理",
                    "DGN清理",
                    "THPURGE",
                    "清理图纸中多余DGN对象，含多余的DGN线型、注释比例等",
                    "IDI_THCAD_DGNPURGE_SMALL",
                    "IDI_THCAD_DGNPURGE_LARGE",
                    RibbonButtonStyle.SmallWithText);
            }
        }

        private static void CreateBlockToolPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("BlockTool", "图层图块");
            var row = panel.AddNewRibbonRow();

            // 建立建筑图层
            row.AddNewButton("建立建筑\r\n图层",
                "建立建筑图层",
                "THALC",
                "建立建筑专业天华标准图层",
                "IDI_THCAD_THALC_SMALL",
                "IDI_THCAD_THALC_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 建立总图图层
            row.AddNewButton("建立总图\r\n图层",
                "建立总图图层",
                "THAPL",
                "建立天华标准总图图层",
                "IDI_THCAD_THAPL_SMALL",
                "IDI_THCAD_THAPL_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 建立单体图层
            row.AddNewButton("建立单体\r\n图层",
                "建立单体图层",
                "THAUL",
                "建立天华标准单体图层",
                "IDI_THCAD_THAUL_SMALL",
                "IDI_THCAD_THAUL_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 建立结构图层
            row.AddNewButton("建立结构\r\n图层",
                "建立结构图层",
                "THSLC",
                "建立结构专业天华标准图层",
                "IDI_THCAD_THSLC_SMALL",
                "IDI_THCAD_THSLC_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 建立暖通图层
            row.AddNewButton("建立暖通\r\n图层",
                "建立暖通图层",
                "THMLC",
                "建立暖通专业天华标准图层",
                "IDI_THCAD_THMLC_SMALL",
                "IDI_THCAD_THMLC_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 建立电气图层
            row.AddNewButton("建立电气\r\n图层",
                "建立电气图层",
                "THELC",
                "建立电气专业天华标准图层",
                "IDI_THCAD_THELC_SMALL",
                "IDI_THCAD_THELC_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 建立给排水图层
            row.AddNewButton("建立给排水\r\n图层",
                "建立给排水图层",
                "THPLC",
                "建立给排专业天华标准图层",
                "IDI_THCAD_THPLC_SMALL",
                "IDI_THCAD_THPLC_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 处理暖通底图
            row.AddNewButton("处理底图\r\n（暖通用）",
                "处理暖通底图",
                "THLPM",
                "处理建筑结构提暖通底图的各图层颜色至相应的色号",
                "IDI_THCAD_THLPM_SMALL",
                "IDI_THCAD_THLPM_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 处理电气底图
            row.AddNewButton("处理底图\r\n（电气用）",
                "处理电气底图",
                "THLPE",
                "处理建筑结构提电气底图的各图层颜色至相应的色号",
                "IDI_THCAD_THLPE_SMALL",
                "IDI_THCAD_THLPE_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 处理给排水底图
            row.AddNewButton("处理底图\r\n（给排水用）",
                "处理给排水底图",
                "THLPP",
                "处理建筑结构提给排水底图的各图层颜色至相应的色号",
                "IDI_THCAD_THLPP_SMALL",
                "IDI_THCAD_THLPP_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 暖通图层管理
            {
                var splitButton = row.AddNewSplitButton("暖通图层管理",
                    RibbonSplitButtonBehavior.SplitFollow,
                    RibbonSplitButtonListStyle.IconText,
                    RibbonButtonStyle.LargeWithText);

                // 锁定暖通图层
                splitButton.AddNewButton("锁定暖通图层",
                    "锁定天华暖通图层",
                    "THMLK",
                    "锁定所有暖通图层",
                    "IDI_THCAD_THMLK_SMALL",
                    "IDI_THCAD_THMLK_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 隔离暖通图层
                splitButton.AddNewButton("隔离暖通图层",
                    "隔离天华暖通图层",
                    "THMUK",
                    "解锁所有暖通图层，同时锁定其他图层",
                    "IDI_THCAD_THMUK_SMALL",
                    "IDI_THCAD_THMUK_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 解锁所有图层
                splitButton.AddNewButton("解锁所有图层",
                    "解锁所有天华图层",
                    "THUKA",
                    "解锁所有图层",
                    "IDI_THCAD_THUKA_SMALL",
                    "IDI_THCAD_THUKA_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 关闭暖通图层
                splitButton.AddNewButton("关闭暖通图层",
                    "关闭天华暖通图层",
                    "THMOF",
                    "关闭所有暖通图层",
                    "IDI_THCAD_THMOF_SMALL",
                    "IDI_THCAD_THMOF_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 开启暖通图层
                splitButton.AddNewButton("开启暖通图层",
                    "开启天华暖通图层",
                    "THMON",
                    "开启所有暖通图层",
                    "IDI_THCAD_THMON_SMALL",
                    "IDI_THCAD_THMON_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            // 暖通图层模式
            {
                var splitButton = row.AddNewSplitButton("暖通图层模式",
                    RibbonSplitButtonBehavior.SplitFollow,
                    RibbonSplitButtonListStyle.IconText,
                    RibbonButtonStyle.LargeWithText);

                // 通风模式
                splitButton.AddNewButton("通风模式",
                    "通风模式",
                    "THTF",
                    "切换到通风模式",
                    "IDI_THCAD_THPSTF_SMALL",
                    "IDI_THCAD_THPSTF_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 水管模式
                splitButton.AddNewButton("水管模式",
                    "水管模式",
                    "THSG",
                    "切换到水管模式",
                    "IDI_THCAD_THSGPM_SMALL",
                    "IDI_THCAD_THSGPM_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 消防模式
                splitButton.AddNewButton("消防模式",
                    "消防模式",
                    "THXF",
                    "切换到消防模式",
                    "IDI_THCAD_THXFPM_SMALL",
                    "IDI_THCAD_THXFPM_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 暖通全显
                splitButton.AddNewButton("暖通全显",
                    "暖通全显",
                    "THNT",
                    "切换到暖通全显",
                    "IDI_THCAD_THNT_SMALL",
                    "IDI_THCAD_THNT_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            // 提电气块转换
            row.AddNewButton("提电气块\r\n转换",
                "提电气块转换",
                "THBEE",
                "将暖通和给排水专业提资给电气的图块转换为电气专业所需的图块",
                "IDI_THCAD_THBEE_SMALL",
                "IDI_THCAD_THBEE_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 图块集
            row.AddNewButton("图块集",
                "天华图块集",
                "THBLI",
                "打开图块集工具选项板",
                "IDI_THCAD_THBLI_SMALL",
                "IDI_THCAD_THBLI_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 图块断线
            {
                var splitButton = row.AddNewSplitButton(
                    "图块断线",
                    RibbonSplitButtonBehavior.SplitFollow,
                    RibbonSplitButtonListStyle.IconText,
                    RibbonButtonStyle.LargeWithText);

                // 插块断线
                splitButton.AddNewButton("插块断线",
                    "天华插块断线",
                    "THBBR",
                    "将选择的图块插入到直线/多段线时自动断线",
                    "IDI_THCAD_THBBR_SMALL",
                    "IDI_THCAD_THBBR_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 选块断线
                splitButton.AddNewButton("选块断线",
                    "天华选块断线",
                    "THBBE",
                    "点选单个图块，根据所需断线的切线方向自动调整图块角度且完成断线",
                    "IDI_THCAD_THBBE_SMALL",
                    "IDI_THCAD_THBBE_LARGE",
                    RibbonButtonStyle.LargeWithText);

                // 全选断线
                splitButton.AddNewButton("全选断线",
                    "天华全选断线",
                    "THBBS",
                    "批量选择需要断线的图块，根据各自所需断线的切线方向自动调整图块角度且完成断线",
                    "IDI_THCAD_THBBS_SMALL",
                    "IDI_THCAD_THBBS_LARGE",
                    RibbonButtonStyle.LargeWithText);
            }

            // 文字镜像
            row.AddNewButton("文字块镜像",
                "文字块镜像",
                "THMIR",
                "镜像含文字块，使文字不反向",
                "IDI_THCAD_THMIR_SMALL",
                "IDI_THCAD_THMIR_LARGE",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateMiscellaneousPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Miscellaneous", "第三方支持");
            var row = panel.AddNewRibbonRow();

            var splitButton = row.AddNewSplitButton("天正看图插件",
                RibbonSplitButtonBehavior.SplitFollow,
                RibbonSplitButtonListStyle.IconText,
                RibbonButtonStyle.LargeWithText);

            // 获取天正看图T20V4.0插件
            splitButton.AddNewButton("天正看图T20V4插件",
                "获取天正看图T20V4.0插件",
                "T20V4",
                "获取天正看图T20V4.0插件",
                "IDI_THCAD_T20V40_SMALL",
                "IDI_THCAD_T20V40_LARGE",
                RibbonButtonStyle.LargeWithText);

            // 获取天正看图T20V5.0插件
            splitButton.AddNewButton("天正看图T20V5插件",
                "获取天正看图T20V5.0插件",
                "T20V5",
                "获取天正看图T20V5.0插件",
                "IDI_THCAD_T20V50_SMALL",
                "IDI_THCAD_T20V50_LARGE",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateCheckToolPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("CheckTool", "校核审查");
            var row = panel.AddNewRibbonRow();

            //柱校核
            row.AddNewButton("柱校核\r\n（公测）",
                "天华柱校核",
                "THCRC",
                "柱配筋图纸校核",
                "IDI_THCAD_THCRC_SMALL",
                "IDI_THCAD_THCRC_LARGE",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateSitePlanToolPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("SitePlanTool", "一键彩总");
            var row = panel.AddNewRibbonRow();

            //一键生成
            row.AddNewButton("一键生成",
                "天华彩总一键生成",
                "THPGE",
                "天华彩总一键生成",
                "IDI_THCAD_THPGE_SMALL",
                "IDI_THCAD_THPGE_LARGE",
                RibbonButtonStyle.LargeWithText);

            //局部刷新
            row.AddNewButton("局部刷新",
                "天华彩总局部刷新",
                "THPUD",
                "天华彩总局部刷新",
                "IDI_THCAD_THPUD_SMALL",
                "IDI_THCAD_THPUD_LARGE",
                RibbonButtonStyle.LargeWithText);

            //映射管理
            row.AddNewButton("映射管理",
                "天华彩总映射管理",
                "THPCF",
                "天华彩总映射管理",
                "IDI_THCAD_THPCF_SMALL",
                "IDI_THCAD_THPCF_LARGE",
                RibbonButtonStyle.LargeWithText);

            //常用设置
            row.AddNewButton("常用设置",
                "天华彩总常用设置",
                "THPOP",
                "天华彩总常用设置",
                "IDI_THCAD_THPOP_SMALL",
                "IDI_THCAD_THPOP_LARGE",
                RibbonButtonStyle.LargeWithText);
        }
    }
}
