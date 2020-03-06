using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Linq2Acad;
using DotNetARX;
using PdfiumViewer;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan
{
    public static class ThSitePlanPlotUtils
    {
        public static PlotSettings CreatePlotSettings(Extents2d window)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 获取当前文档中设置的布局方式
                Layout layoutObj = acadDatabase.Layouts.Element(LayoutManager.Current.CurrentLayout);

                // 使用预定义的PlotSetting
                PlotSettings plotSetting = new PlotSettings(layoutObj.ModelType);
                plotSetting.CopyFrom(layoutObj);

                // 获取当前打印设置验证类
                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                var plotDevices = psv.GetPlotDeviceList();
                // 更新打印设备、图纸尺寸和打印样式表信息
                psv.SetPlotConfigurationName(plotSetting, 
                    Properties.Settings.Default.plotDeviceName, 
                    Properties.Settings.Default.mediaName);
                psv.RefreshLists(plotSetting);
                psv.SetCurrentStyleSheet(plotSetting, 
                    Properties.Settings.Default.styleSheetName);

                // 自定义打印信息
                // 设置打印窗口等信息
                psv.SetPlotWindowArea(plotSetting, window);
                psv.SetPlotType(plotSetting, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);
                // 打印比例-布满图纸
                psv.SetUseStandardScale(plotSetting, true);
                psv.SetStdScaleType(plotSetting, StdScaleType.ScaleToFit);
                // 图形方向（默认是横向）
                psv.SetPlotRotation(plotSetting, PlotRotation.Degrees000);
                // 打印偏移-居中打印
                psv.SetPlotCentered(plotSetting, true);

                // 返回打印配置
                return plotSetting;
            }
        }

        public static void DoPlot(Extents2d window, string fileName)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                using (new ThAppTools.ManagedSystemVariable("BACKGROUNDPLOT", 0))
                {
                    // 获取当前文档中设置的布局方式
                    Layout layoutObj = acadDatabase.Layouts.Element(LayoutManager.Current.CurrentLayout);

                    // 打印设置
                    PlotSettings plotSettings = CreatePlotSettings(window);

                    // 进行打印
                    using (var plotEngine = PlotFactory.CreatePublishEngine())
                    {
                        PlotTools.Plot(plotEngine, layoutObj, plotSettings, fileName, 1, false, true, true);
                    }
                }
            }
        }

        public static void RenderPdfToPng(string inputPdfPath, string outpdfPath)
        {
            using (PdfDocument document = PdfDocument.Load(inputPdfPath))
            {
                System.Drawing.Image image = null;
                image = document.Render(0, 96, 96, PdfRenderFlags.Transparent);
                if (image != null)
                {
                    image.Save(outpdfPath);
                }
            }
        }
    }
}
