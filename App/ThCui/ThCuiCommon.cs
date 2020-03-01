namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiCommon
    {
        public static readonly string CMD_GROUPNAME = "TIANHUACAD";
        public static readonly string CMD_THHLP_GLOBAL_NAME = "THHLP";
        public static readonly string CMD_THBLS_GLOBAL_NAME = "THBLS";
        public static readonly string CMD_THBLI_GLOBAL_NAME = "THBLI";
        public static readonly string CMD_THT20PLUGINV4_GLOBAL_NAME = "T20V4";
        public static readonly string CMD_THT20PLUGINV5_GLOBAL_NAME = "T20V5";
        public static readonly string CMD_THLOGIN_GLOBAL_NAME = "THLOGIN";
        public static readonly string CMD_THLOGOUT_GLOBAL_NAME = "THLOGOUT";
        public static readonly string CMD_THDUMPCUI_GLOBAL_NAME = "THDUMPCUI";
        public static readonly string CMD_THPROFILE_GLOBAL_NAME = "THPROFILE";

        public static readonly string profile_ribbon_architecture = @"---
            name : 'ThRibbonBar'
            title : '天华效率工具'
            panels :
                - UID : 'pnlHelp'
                  name : 'Help'
                  title : '登录界面'
                  items:
                    - text : '专业切换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '帮助文档'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '检查更新'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlBlock'
                  name : 'Block'
                  title : '图块图库'
                  items:
                    - text : '图块集'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块集配置'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '提电气块\r\n转换'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块断线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlAnnotation'
                  name : 'Annotation'
                  title : '标注工具'
                  items:
                    - text : '车位编号'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '尺寸避让'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlLayer'
                  name : 'Layer'
                  title : '图层工具'
                  items :
                    - text : '建立天华图层'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '处理建筑结构底图'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '暖通图层管理'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlStatistic'
                  name : 'Statistic'
                  title : '计算工具'
                  items :
                    - text : '天华单体规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '天华总平规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '单体面积\r\n汇总'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '综合经济\r\n技术指标表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '防火分区疏散表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '房间面积框线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlPlanePut'
                  name : 'PlanePut'
                  title : '平面绘图'
                  items :
                    - text : '喷头布置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlText'
                  name : 'Text'
                  title : '文字表格'
                  items :
                    - text : '文字内容刷'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlAuxiliary'
                  name : 'Auxiliary'
                  title : '辅助工具'
                  rowPanels :
                    - text : ''
                      items :
                        - text : '快选命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '对齐命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量缩放'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'Z值归零'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'DGN清理'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量打印PDF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印DWF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印PPT'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '版次信息修改'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '管线断线'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '文字块镜像'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlMiscellaneous'
                  name : 'Miscellaneous'
                  title : '第三方支持'
                  items :
                    - text : '天正看图插件'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
        ";

        public static readonly string profile_ribbon_structure = @"---
            name : 'ThRibbonBar'
            title : '天华效率工具'
            panels :
                - UID : 'pnlHelp'
                  name : 'Help'
                  title : '登录界面'
                  items:
                    - text : '专业切换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '帮助文档'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '检查更新'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlBlock'
                  name : 'Block'
                  title : '图块图库'
                  items:
                    - text : '图块集'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块集配置'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '提电气块\r\n转换'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块断线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlAnnotation'
                  name : 'Annotation'
                  title : '标注工具'
                  items:
                    - text : '车位编号'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '尺寸避让'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlLayer'
                  name : 'Layer'
                  title : '图层工具'
                  items :
                    - text : '建立天华图层'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '处理建筑结构底图'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '暖通图层管理'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlStatistic'
                  name : 'Statistic'
                  title : '计算工具'
                  items :
                    - text : '天华单体规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '天华总平规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '单体面积\r\n汇总'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '综合经济\r\n技术指标表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '防火分区疏散表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '房间面积框线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlPlanePut'
                  name : 'PlanePut'
                  title : '平面绘图'
                  items :
                    - text : '喷头布置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlText'
                  name : 'Text'
                  title : '文字表格'
                  items :
                    - text : '文字内容刷'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlAuxiliary'
                  name : 'Auxiliary'
                  title : '辅助工具'
                  rowPanels :
                    - text : ''
                      items :
                        - text : '快选命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '对齐命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量缩放'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'Z值归零'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'DGN清理'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量打印PDF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印DWF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印PPT'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '版次信息修改'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '管线断线'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '文字块镜像'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlMiscellaneous'
                  name : 'Miscellaneous'
                  title : '第三方支持'
                  items :
                    - text : '天正看图插件'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
        ";

        public static readonly string profile_ribbon_havc = @"---
            name : 'ThRibbonBar'
            title : '天华效率工具'
            panels :
                - UID : 'pnlHelp'
                  name : 'Help'
                  title : '登录界面'
                  items:
                    - text : '专业切换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '帮助文档'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '检查更新'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlBlock'
                  name : 'Block'
                  title : '图块图库'
                  items:
                    - text : '图块集'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '图块集配置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '提电气块\r\n转换'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块断线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlAnnotation'
                  name : 'Annotation'
                  title : '标注工具'
                  items:
                    - text : '车位编号'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '尺寸避让'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlLayer'
                  name : 'Layer'
                  title : '图层工具'
                  items :
                    - text : '建立天华图层'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '处理建筑结构底图'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '暖通图层管理'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlStatistic'
                  name : 'Statistic'
                  title : '计算工具'
                  items :
                    - text : '天华单体规整'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '天华总平规整'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '单体面积\r\n汇总'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '综合经济\r\n技术指标表'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '防火分区疏散表'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '房间面积框线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlPlanePut'
                  name : 'PlanePut'
                  title : '平面绘图'
                  items :
                    - text : '喷头布置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlText'
                  name : 'Text'
                  title : '文字表格'
                  items :
                    - text : '文字内容刷'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlAuxiliary'
                  name : 'Auxiliary'
                  title : '辅助工具'
                  rowPanels :
                    - text : ''
                      items :
                        - text : '快选命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '对齐命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量缩放'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'Z值归零'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'DGN清理'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量打印PDF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印DWF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印PPT'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '版次信息修改'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '管线断线'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '文字块镜像'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlMiscellaneous'
                  name : 'Miscellaneous'
                  title : '第三方支持'
                  items :
                    - text : '天正看图插件'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
        ";

        public static readonly string profile_ribbon_electrical = @"---
            name : 'ThRibbonBar'
            title : '天华效率工具'
            panels :
                - UID : 'pnlHelp'
                  name : 'Help'
                  title : '登录界面'
                  items:
                    - text : '专业切换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '帮助文档'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '检查更新'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlBlock'
                  name : 'Block'
                  title : '图块图库'
                  items:
                    - text : '图块集'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '图块集配置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '提电气块\r\n转换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '图块断线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlAnnotation'
                  name : 'Annotation'
                  title : '标注工具'
                  items:
                    - text : '车位编号'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '尺寸避让'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlLayer'
                  name : 'Layer'
                  title : '图层工具'
                  items :
                    - text : '建立天华图层'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '处理建筑结构底图'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '暖通图层管理'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlStatistic'
                  name : 'Statistic'
                  title : '计算工具'
                  items :
                    - text : '天华单体规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '天华总平规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '单体面积\r\n汇总'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '综合经济\r\n技术指标表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '防火分区疏散表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '房间面积框线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlPlanePut'
                  name : 'PlanePut'
                  title : '平面绘图'
                  items :
                    - text : '喷头布置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlText'
                  name : 'Text'
                  title : '文字表格'
                  items :
                    - text : '文字内容刷'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlAuxiliary'
                  name : 'Auxiliary'
                  title : '辅助工具'
                  rowPanels :
                    - text : ''
                      items :
                        - text : '快选命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '对齐命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量缩放'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'Z值归零'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'DGN清理'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量打印PDF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印DWF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印PPT'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '版次信息修改'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '管线断线'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '文字块镜像'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlMiscellaneous'
                  name : 'Miscellaneous'
                  title : '第三方支持'
                  items :
                    - text : '天正看图插件'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
        ";

        public static readonly string profile_ribbon_wss = @"---
            name : 'ThRibbonBar'
            title : '天华效率工具'
            panels :
                - UID : 'pnlHelp'
                  name : 'Help'
                  title : '登录界面'
                  items:
                    - text : '专业切换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '帮助文档'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '检查更新'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlBlock'
                  name : 'Block'
                  title : '图块图库'
                  items:
                    - text : '图块集'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '图块集配置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '提电气块\r\n转换'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块断线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlAnnotation'
                  name : 'Annotation'
                  title : '标注工具'
                  items:
                    - text : '车位编号'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '尺寸避让'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlLayer'
                  name : 'Layer'
                  title : '图层工具'
                  items :
                    - text : '建立天华图层'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '处理建筑结构底图'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '暖通图层管理'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlStatistic'
                  name : 'Statistic'
                  title : '计算工具'
                  items :
                    - text : '天华单体规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '天华总平规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '单体面积\r\n汇总'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '综合经济\r\n技术指标表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '防火分区疏散表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '房间面积框线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlPlanePut'
                  name : 'PlanePut'
                  title : '平面绘图'
                  items :
                    - text : '喷头布置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlText'
                  name : 'Text'
                  title : '文字表格'
                  items :
                    - text : '文字内容刷'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlAuxiliary'
                  name : 'Auxiliary'
                  title : '辅助工具'
                  rowPanels :
                    - text : ''
                      items :
                        - text : '快选命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '对齐命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量缩放'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'Z值归零'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'DGN清理'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量打印PDF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印DWF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印PPT'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '版次信息修改'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '管线断线'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '文字块镜像'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlMiscellaneous'
                  name : 'Miscellaneous'
                  title : '第三方支持'
                  items :
                    - text : '天正看图插件'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
        ";

        public static readonly string profile_ribbon_project = @"---
            name : 'ThRibbonBar'
            title : '天华效率工具'
            panels :
                - UID : 'pnlHelp'
                  name : 'Help'
                  title : '登录界面'
                  items:
                    - text : '专业切换'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '帮助文档'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                    - text : '检查更新'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlBlock'
                  name : 'Block'
                  title : '图块图库'
                  items:
                    - text : '图块集'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块集配置'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '提电气块\r\n转换'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                    - text : '图块断线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlAnnotation'
                  name : 'Annotation'
                  title : '标注工具'
                  items:
                    - text : '车位编号'
                      attributes : { 'IsVisible' : false, 'IsEnabled' : true } 
                    - text : '尺寸避让'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlLayer'
                  name : 'Layer'
                  title : '图层工具'
                  items :
                    - text : '建立天华图层'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '处理建筑结构底图'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '暖通图层管理'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlStatistic'
                  name : 'Statistic'
                  title : '计算工具'
                  items :
                    - text : '天华单体规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '天华总平规整'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '单体面积\r\n汇总'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '综合经济\r\n技术指标表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '防火分区疏散表'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : '房间面积框线'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlPlanePut'
                  name : 'PlanePut'
                  title : '平面绘图'
                  items :
                    - text : '喷头布置'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlText'
                  name : 'Text'
                  title : '文字表格'
                  items :
                    - text : '文字内容刷'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : false, 'IsEnabled' : true }
                - UID : 'pnlAuxiliary'
                  name : 'Auxiliary'
                  title : '辅助工具'
                  rowPanels :
                    - text : ''
                      items :
                        - text : '快选命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '对齐命令集'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量缩放'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'Z值归零'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : 'DGN清理'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '批量打印PDF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印DWF'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '批量打印PPT'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                    - text : ''
                      items :
                        - text : '版次信息修改'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '管线断线'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                        - text : '文字块镜像'
                          attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
                - UID : 'pnlMiscellaneous'
                  name : 'Miscellaneous'
                  title : '第三方支持'
                  items :
                    - text : '天正看图插件'
                      attributes : { 'IsVisible' : true, 'IsEnabled' : true } 
                  attributes : { 'IsVisible' : true, 'IsEnabled' : true }
        ";
    }
}
