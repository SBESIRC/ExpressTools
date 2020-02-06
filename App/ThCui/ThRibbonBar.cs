using Autodesk.AutoCAD.Customization;
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
                CreateBlockPanel(tab);
                CreateAnnotationPanel(tab);
                CreateLayerPanel(tab);
                CreatStatisticPanel(tab);
                CreateTextPanel(tab);
                CreateAuxiliaryPanel(tab);
                CreateMiscellaneousPanel(tab);
            }
        }

        private static void CreateHelpPanel(RibbonTabSource tab)
        {
            // 登录界面
            var panel = tab.AddNewPanel("Help", "登录界面");
            var row = panel.AddNewRibbonRow();

            // 登录
            row.AddNewButton("登录",
                "天华登录",
                ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME,
                "登录天华效率平台",
                "IDI_THCAD_THLOGIN",
                "IDI_THCAD_THLOGIN",
                RibbonButtonStyle.LargeWithText);

            // 退出
            row.AddNewButton("退出",
                "天华退出",
                ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME,
                "退出天华效率平台",
                "IDI_THCAD_THLOGOUT",
                "IDI_THCAD_THLOGOUT",
                RibbonButtonStyle.LargeWithText);

            // 帮助
            row.AddNewButton("帮助文档",
                "天华帮助",
                ThCuiCommon.CMD_THHLP_GLOBAL_NAME,
                "获取帮助文档",
                "IDI_THCAD_THHLP",
                "IDI_THCAD_THHLP",
                RibbonButtonStyle.LargeWithText);

            // 更新
            row.AddNewButton("检查更新",
                "天华自动更新",
                "THUPT",
                "检查更新",
                "IDI_THCAD_THUPT",
                "IDI_THCAD_THUPT",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateBlockPanel(RibbonTabSource tab)
        {
            // 图块图集
            var panel = tab.AddNewPanel("Block", "图块图库");
            var row = panel.AddNewRibbonRow();

            // 图块集
            row.AddNewButton("图块集",
                "天华图块集",
                "THBLI",
                "打开图块集工具选项板",
                "IDI_THCAD_THBLI",
                "IDI_THCAD_THBLI",
                RibbonButtonStyle.LargeWithText);

            // 图块集配置
            row.AddNewButton("图块集配置",
                "天华图块集配置",
                "THBLS",
                "配置各专业图块集",
                "IDI_THCAD_THBLS",
                "IDI_THCAD_THBLS",
                RibbonButtonStyle.LargeWithText);

            // 提电气块转换
            row.AddNewButton("提电气块\r\n转换",
                "天华提电气块转换",
                "THBEE",
                "将暖通和给排水专业提资给电气的图块转换为电气专业所需的图块",
                "IDI_THCAD_THBEE",
                "IDI_THCAD_THBEE",
                RibbonButtonStyle.LargeWithText);

            // 
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
                "IDI_THCAD_THBBR",
                "IDI_THCAD_THBBR",
                RibbonButtonStyle.LargeWithText);

            // 选块断线
            splitButton.AddNewButton("选块断线",
                "天华选块断线",
                "THBBE",
                "点选单个图块，根据所需断线的切线方向自动调整图块角度且完成断线",
                "IDI_THCAD_THBBE",
                "IDI_THCAD_THBBE",
                RibbonButtonStyle.LargeWithText);

            // 全选断线
            splitButton.AddNewButton("全选断线",
                "天华全选断线",
                "THBBS",
                "批量选择需要断线的图块，根据各自所需断线的切线方向自动调整图块角度且完成断线",
                "IDI_THCAD_THBBS",
                "IDI_THCAD_THBBS",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateAnnotationPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Annotation", "标注工具");
            var row = panel.AddNewRibbonRow();

            // 车位编号
            row.AddNewButton("车位编号",
                "天华车位编号",
                "THCNU",
                "绘制多段线穿过所需编号停车位图块，根据多段线穿过停车位的先后顺序快速生成车位编号",
                "IDI_THCAD_THCNU",
                "IDI_THCAD_THCNU",
                RibbonButtonStyle.LargeWithText);

            // 尺寸避让
            row.AddNewButton("尺寸避让",
                "天华尺寸避让",
                "THDTA",
                "调整交叉或重叠的标注文字以避免发生位置冲突",
                "IDI_THCAD_THDTA",
                "IDI_THCAD_THDTA",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateLayerPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Layer", "图层工具");
            var row = panel.AddNewRibbonRow();

            var splitButton = row.AddNewSplitButton("建立\r\n天华图层",
                RibbonSplitButtonBehavior.SplitFollow,
                RibbonSplitButtonListStyle.IconText,
                RibbonButtonStyle.LargeWithText);

            // 建立建筑图层
            splitButton.AddNewButton("建立建筑图层",
                "建立天华建筑图层",
                "THALC",
                "建立建筑专业天华标准图层",
                "IDI_THCAD_THALC",
                "IDI_THCAD_THALC",
                RibbonButtonStyle.LargeWithText);

            // 建立结构图层
            splitButton.AddNewButton("建立结构图层",
                "建立天华结构图层",
                "THSLC",
                "建立结构专业天华标准图层",
                "IDI_THCAD_THSLC",
                "IDI_THCAD_THSLC",
                RibbonButtonStyle.LargeWithText);

            // 建立暖通图层
            splitButton.AddNewButton("建立暖通图层",
                "建立天华暖通图层",
                "THMLC",
                "建立暖通专业天华标准图层",
                "IDI_THCAD_THMLC",
                "IDI_THCAD_THMLC",
                RibbonButtonStyle.LargeWithText);

            // 建立电气图层
            splitButton.AddNewButton("建立电气图层",
                "建立天华电气图层",
                "THELC",
                "建立电气专业天华标准图层",
                "IDI_THCAD_THELC",
                "IDI_THCAD_THELC",
                RibbonButtonStyle.LargeWithText);

            // 建立给排水图层
            splitButton.AddNewButton("建立给排水图层",
                "建立天华给排水图层",
                "THPLC",
                "建立给排专业天华标准图层",
                "IDI_THCAD_THPLC",
                "IDI_THCAD_THPLC",
                RibbonButtonStyle.LargeWithText);


            splitButton = row.AddNewSplitButton("处理建筑\r\n结构底图",
                RibbonSplitButtonBehavior.SplitFollow,
                RibbonSplitButtonListStyle.IconText,
                RibbonButtonStyle.LargeWithText);

            // 暖通用
            splitButton.AddNewButton("暖通用",
                "天华暖通用",
                "THLPM",
                "处理建筑结构提暖通底图的各图层颜色至相应的色号",
                "IDI_THCAD_THLPM",
                "IDI_THCAD_THLPM",
                RibbonButtonStyle.LargeWithText);

            // 电气用
            splitButton.AddNewButton("电气用",
                "天华电气用",
                "THLPE",
                "处理建筑结构提电气底图的各图层颜色至相应的色号",
                "IDI_THCAD_THLPE",
                "IDI_THCAD_THLPE",
                RibbonButtonStyle.LargeWithText);

            // 给排水用
            splitButton.AddNewButton("给排水用",
                "天华给排水用",
                "THLPP",
                "处理建筑结构提给排水底图的各图层颜色至相应的色号",
                "IDI_THCAD_THLPP",
                "IDI_THCAD_THLPP",
                RibbonButtonStyle.LargeWithText);

            splitButton = row.AddNewSplitButton("暖通\r\n图层管理",
                RibbonSplitButtonBehavior.SplitFollow,
                RibbonSplitButtonListStyle.IconText,
                RibbonButtonStyle.LargeWithText);

            // 锁定暖通图层
            splitButton.AddNewButton("锁定暖通图层",
                "锁定天华暖通图层",
                "THMLK",
                "锁定所有暖通图层",
                "IDI_THCAD_THMLK",
                "IDI_THCAD_THMLK",
                RibbonButtonStyle.LargeWithText);

            // 隔离暖通图层
            splitButton.AddNewButton("隔离暖通图层",
                "隔离天华暖通图层",
                "THMUK",
                "解锁所有暖通图层，同时锁定其他图层",
                "IDI_THCAD_THMUK",
                "IDI_THCAD_THMUK",
                RibbonButtonStyle.LargeWithText);

            // 解锁所有图层
            splitButton.AddNewButton("解锁所有图层",
                "解锁所有天华图层",
                "THUKA",
                "解锁所有图层",
                "IDI_THCAD_THUKA",
                "IDI_THCAD_THUKA",
                RibbonButtonStyle.LargeWithText);

            // 关闭暖通图层
            splitButton.AddNewButton("关闭暖通图层",
                "关闭天华暖通图层",
                "THMOF",
                "关闭所有暖通图层",
                "IDI_THCAD_THMOF",
                "IDI_THCAD_THMOF",
                RibbonButtonStyle.LargeWithText);

            // 开启暖通图层
            splitButton.AddNewButton("开启暖通图层",
                "开启天华暖通图层",
                "THMON",
                "开启所有暖通图层",
                "IDI_THCAD_THMON",
                "IDI_THCAD_THMON",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreatStatisticPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Statistic", "计算工具");
            var row = panel.AddNewRibbonRow();

            // 天华单体规整
            row.AddNewButton("天华单体规整",
                "天华单体规整",
                "THBPS",
                "将建筑单体各层平面图中代表各区域的多段线图元设置到相应的图层，以供生成单体面积汇总表所用",
                "IDI_THCAD_THBPS",
                "IDI_THCAD_THBPS",
                RibbonButtonStyle.LargeWithText);

            // 天华总平规整
            row.AddNewButton("天华总平规整",
                "天华总平规整",
                "THSPS",
                "将总平面图中代表各区域的多段线图元设置到相应的图层，以供生成综合经济技术指标表所用",
                "IDI_THCAD_THSPS",
                "IDI_THCAD_THSPS",
                RibbonButtonStyle.LargeWithText);

            // 单体面积汇总
            row.AddNewButton("单体面积\r\n汇总",
                "天华单体面积汇总",
                "THBAC",
                "汇总单体每层各区域建筑面积和计容面积",
                "IDI_THCAD_THBAC",
                "IDI_THCAD_THBAC",
                RibbonButtonStyle.LargeWithText);

            // 综合经济技术指标表
            row.AddNewButton("综合经济\r\n技术指标表",
                "天华综合经济技术指标表",
                "THTET",
                "汇总总平面及各单体各区域建筑面积和计容面积，形成综合经济技术指标表",
                "IDI_THCAD_THTET",
                "IDI_THCAD_THTET",
                RibbonButtonStyle.LargeWithText);

            // 防火分区疏散表
            row.AddNewButton("防火分区疏散表",
                "天华防火分区疏散表",
                "THFET",
                "统计商业/地库各防火分区面积，自动计算应有疏散距离，并生成表格",
                "IDI_THCAD_THFET",
                "IDI_THCAD_THFET",
                RibbonButtonStyle.LargeWithText);

            // 房间面积框线
            row.AddNewButton("房间面积框线",
                "天华房间面积框线",
                "THABC",
                "自动生成屏幕选择范围内所有房间的框线，且可选择插入面积值",
                "IDI_THCAD_THABC",
                "IDI_THCAD_THABC",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateTextPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Text", "文字表格");
            var row = panel.AddNewRibbonRow();

            // 文字内容刷
            row.AddNewButton("文字内容刷",
                "天华文字内容刷",
                "THMTC",
                "将目标文字内容替换为源文字内容",
                "IDI_THCAD_THMTC",
                "IDI_THCAD_THMTC",
                RibbonButtonStyle.LargeWithText);
        }

        private static void CreateAuxiliaryPanel(RibbonTabSource tab)
        {
            var panel = tab.AddNewPanel("Auxiliary", "辅助工具");
            var row = panel.AddNewRibbonRow();

            // 第一列
            {
                var subPanel = row.AddNewPanel();

                // 批量缩放
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量缩放",
                    "天华批量缩放",
                    "THMSC",
                    "对多个选择对象以各自的开始点（插入点）为基准点进行批量比例缩放",
                    "IDI_THCAD_THMSC",
                    "IDI_THCAD_THMSC",
                    RibbonButtonStyle.SmallWithText);

                // Z值归零
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("Z值归零",
                    "天华Z值归零",
                    "THZ0",
                    "将模型空间内所有对象Z值归零，使之处于同一平面",
                    "IDI_THCAD_THZ0",
                    "IDI_THCAD_THZ0",
                    RibbonButtonStyle.SmallWithText);

                // DGN清理
                // 从AutoCAD 2016开始，“PURGE”命令可以实现“DGNPURGE”的功能
                // 这里直接将“DGNPURGE”切换到“PURGE”命令
                subRow = subPanel.AddNewRibbonRow();
#if ACAD_ABOVE_2014
                subRow.AddNewButton("DGN清理",
                    "DGN清理",
                    "PURGE",
                    "清理图纸中多余DGN对象，含多余的DGN线型、注释比例等",
                    "IDI_THCAD_DGNPURGE",
                    "IDI_THCAD_DGNPURGE",
                    RibbonButtonStyle.SmallWithText);
#else
                subRow.AddNewButton("DGN清理",
                    "DGN清理",
                    "DGNPURGE",
                    "清理图纸中多余DGN对象，含多余的DGN线型、注释比例等",
                    "IDI_THCAD_DGNPURGE",
                    "IDI_THCAD_DGNPURGE",
                    RibbonButtonStyle.SmallWithText);
#endif

            }

            // 第二列
            {
                var subPanel = row.AddNewPanel();

                // 批量打印PDF
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量打印PDF",
                    "批量打印PDF",
                    "THBPT",
                    "选择需要批量打印的天华图框，将图纸批量打印为PDF文件，读取图框中的图纸编号重命名相应PDF文件",
                    "IDI_THCAD_THBPT",
                    "IDI_THCAD_THBPT",
                    RibbonButtonStyle.SmallWithText);

                // 批量打印DWF
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量打印DWF",
                    "批量打印DWF",
                    "THBPD",
                    "选择需要批量打印的天华图框，将图纸批量打印为DWF文件，读取图框中的图纸编号重命名相应DWF文件",
                    "IDI_THCAD_THBPD",
                    "IDI_THCAD_THBPD",
                    RibbonButtonStyle.SmallWithText);

                // 批量打印PPT
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("批量打印PPT",
                    "批量打印PPT",
                    "THBPP",
                    "选择需要批量打印的PPT框线，将图纸批量打印为单个PPT文件，读取打印窗口框线与PPT框线的位置完成PPT内图片的定位",
                    "IDI_THCAD_THBPP",
                    "IDI_THCAD_THBPP",
                    RibbonButtonStyle.SmallWithText);
            }

            // 第三列
            {
                var subPanel = row.AddNewPanel();

                // 版次信息修改
                var subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("版次信息修改",
                    "版次信息修改",
                    "THSVM",
                    "批量修改图框内的版次信息或出图日期",
                    "IDI_THCAD_THSVM",
                    "IDI_THCAD_THSVM",
                    RibbonButtonStyle.SmallWithText);

                // 管线断线
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("管线断线",
                    "管线断线",
                    "THLTR",
                    "批量处理管线断线",
                    "IDI_THCAD_THLTR",
                    "IDI_THCAD_THLTR",
                    RibbonButtonStyle.SmallWithText);

                // 文字块镜像
                subRow = subPanel.AddNewRibbonRow();
                subRow.AddNewButton("文字块镜像",
                    "文字块镜像",
                    "THMIR",
                    "镜像含文字块，使文字不反向",
                    "IDI_THCAD_THMIR",
                    "IDI_THCAD_THMIR",
                    RibbonButtonStyle.SmallWithText);
            }
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
                "IDI_THCAD_T20V40",
                "IDI_THCAD_T20V40",
                RibbonButtonStyle.LargeWithText);

            // 获取天正看图T20V5.0插件
            splitButton.AddNewButton("天正看图T20V5插件",
                "获取天正看图T20V5.0插件",
                "T20V5",
                "获取天正看图T20V5.0插件",
                "IDI_THCAD_T20V50",
                "IDI_THCAD_T20V50",
                RibbonButtonStyle.LargeWithText);
        }
    }
}
