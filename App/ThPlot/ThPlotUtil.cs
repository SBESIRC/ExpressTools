using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.PlottingServices;
using pdftron;
using pdftron.PDF;
using pdftron.Common;
using PowerPoint = NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Tools;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi.Enums;
using System.IO;
using static ThPlot.ThPlotData;
using System.Threading;
using System.Diagnostics;
using AcHelper;

namespace ThPlot
{
    public class ThPlotUtil
    {
        public static void PlotWithNamedPlotSettings(string settingName, string outputFilePath, string printStyle = null)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction plotTrans = db.TransactionManager.StartTransaction())
            {
                // 获取当前文档中设置的布局方式
                LayoutManager layoutManager = LayoutManager.Current;
                ObjectId layoutId = layoutManager.GetLayoutId(layoutManager.CurrentLayout);
                Layout layoutObj = (Layout)plotTrans.GetObject(layoutId, OpenMode.ForRead);
                DBDictionary plotSettingsDic = (DBDictionary)plotTrans.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);
                if (!plotSettingsDic.Contains(settingName))
                {
                    return;
                }

                // 获取设置ID
                ObjectId plotSettingId = plotSettingsDic.GetAt(settingName);
                // 模型或者布局
                bool bModel = layoutObj.ModelType;
                PlotSettings plotSettings = plotTrans.GetObject(plotSettingId, OpenMode.ForRead) as PlotSettings;
                if (plotSettings.ModelType != bModel)
                {
                    return;
                }

                object backgroundPlot = Application.GetSystemVariable("BACKGROUNDPLOT");
                Application.SetSystemVariable("BACKGROUNDPLOT", 0);

                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = layoutObj.ObjectId;
                plotInfo.OverrideSettings = plotSettings;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.Validate(plotInfo);

                // 开始打印
                using (var plotEngine = PlotFactory.CreatePublishEngine())
                {
                    // 打印到文档.
                    plotEngine.BeginPlot(null, null);
                    plotEngine.BeginDocument(plotInfo, doc.Name, null, 1, true, outputFilePath);

                    // 打印页
                    PlotPageInfo ppi = new PlotPageInfo();
                    plotEngine.BeginPage(ppi, plotInfo, true, null);
                    plotEngine.BeginGenerateGraphics(null);
                    plotEngine.EndGenerateGraphics(null);

                    // 页结束
                    plotEngine.EndPage(null);

                    // 文档结束
                    plotEngine.EndDocument(null);

                    // plot结束
                    plotEngine.EndPlot(null);
                }

                plotTrans.Commit();
                Application.SetSystemVariable("BACKGROUNDPLOT", backgroundPlot);
            }
        }

        /// <summary>
        /// 打印窗口操作 使用CAD中的PlotSetting信息设置
        /// </summary>
        /// <param name="window"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="printStyle"></param>
        public static void PlotWithWindow(Extents2d window, string outputFilePath, string printStyle)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction plotTrans = db.TransactionManager.StartTransaction())
            {
                // 获取当前文档中设置的布局方式
                LayoutManager layoutManager = LayoutManager.Current;
                ObjectId layoutId = layoutManager.GetLayoutId(layoutManager.CurrentLayout);
                Layout layoutObj = (Layout)plotTrans.GetObject(layoutId, OpenMode.ForRead);

                object backgroundPlot = Application.GetSystemVariable("BACKGROUNDPLOT");
                Application.SetSystemVariable("BACKGROUNDPLOT", 0);

                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = layoutObj.ObjectId;

                // 使用预定义的PlotSetting
                PlotSettings plotSetting = new PlotSettings(layoutObj.ModelType);
                plotSetting.CopyFrom(layoutObj);

                // 设置打印窗口等信息
                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                psv.RefreshLists(plotSetting);
                psv.SetCurrentStyleSheet(plotSetting, printStyle);
                psv.SetPlotWindowArea(plotSetting, window);

                psv.SetPlotType(plotSetting, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);
                plotInfo.OverrideSettings = plotSetting;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.Validate(plotInfo);

                // 开始打印
                using (var plotEngine = PlotFactory.CreatePublishEngine())
                {
                    // 打印到文档.
                    plotEngine.BeginPlot(null, null);
                    plotEngine.BeginDocument(plotInfo, doc.Name, null, 1, true, outputFilePath);

                    // 打印页
                    PlotPageInfo ppi = new PlotPageInfo();
                    plotEngine.BeginPage(ppi, plotInfo, true, null);
                    plotEngine.BeginGenerateGraphics(null);
                    plotEngine.EndGenerateGraphics(null);

                    // 页结束
                    plotEngine.EndPage(null);

                    // 文档结束
                    plotEngine.EndDocument(null);

                    // plot结束
                    plotEngine.EndPlot(null);
                }

                Application.SetSystemVariable("BACKGROUNDPLOT", backgroundPlot);
            }
        }

        /// <summary>
        /// 打印窗口操作 使用自定义的打印窗口设置
        /// </summary>
        /// <param name="window"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="printStyle"></param>
        public static void PlotWithWindowWithSelfPlot(Extents2d window, string outputFilePath, string printStyle)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction plotTrans = db.TransactionManager.StartTransaction())
            {
                // 获取当前文档中设置的布局方式
                LayoutManager layoutManager = LayoutManager.Current;
                ObjectId layoutId = layoutManager.GetLayoutId(layoutManager.CurrentLayout);
                Layout layoutObj = (Layout)plotTrans.GetObject(layoutId, OpenMode.ForRead);

                object backgroundPlot = Application.GetSystemVariable("BACKGROUNDPLOT");
                Application.SetSystemVariable("BACKGROUNDPLOT", 0);

                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = layoutObj.ObjectId;

                // 使用预定义的PlotSetting
                PlotSettings plotSetting = new PlotSettings(layoutObj.ModelType);
                plotSetting.CopyFrom(layoutObj);

                // 设置打印窗口等信息
                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                psv.RefreshLists(plotSetting);
                psv.SetCurrentStyleSheet(plotSetting, printStyle);
                psv.SetPlotWindowArea(plotSetting, window);
                psv.SetPlotType(plotSetting, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);

                // 自定义打印信息
                psv.SetUseStandardScale(plotSetting, true);
                psv.SetStdScaleType(plotSetting, StdScaleType.ScaleToFit);
                psv.SetPlotRotation(plotSetting, PlotRotation.Degrees090);
                if (HorizontalPrint(window))
                    psv.SetPlotRotation(plotSetting, PlotRotation.Degrees000);
                psv.SetPlotCentered(plotSetting, true);
                psv.SetPlotPaperUnits(plotSetting, PlotPaperUnit.Millimeters);
                psv.SetPlotConfigurationName(plotSetting, "DWG To PDF.pc3", "ISO_A4_(297.00_x_210.00_MM)");
                plotInfo.OverrideSettings = plotSetting;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.Validate(plotInfo);

                // 开始打印
                using (var plotEngine = PlotFactory.CreatePublishEngine())
                {
                    // 打印到文档.
                    plotEngine.BeginPlot(null, null);
                    plotEngine.BeginDocument(plotInfo, doc.Name, null, 1, true, outputFilePath);

                    // 打印页
                    PlotPageInfo ppi = new PlotPageInfo();
                    plotEngine.BeginPage(ppi, plotInfo, true, null);
                    plotEngine.BeginGenerateGraphics(null);
                    plotEngine.EndGenerateGraphics(null);

                    // 页结束
                    plotEngine.EndPage(null);

                    // 文档结束
                    plotEngine.EndDocument(null);

                    // plot结束
                    plotEngine.EndPlot(null);
                }

                Application.SetSystemVariable("BACKGROUNDPLOT", backgroundPlot);
            }
        }

        /// <summary>
        /// 是否水平打印
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static bool HorizontalPrint(Extents2d window)
        {
            var leftBottomPoint = window.MinPoint;
            var rightTopPoint = window.MaxPoint;
            var width = rightTopPoint.X - leftBottomPoint.X;
            var height = rightTopPoint.Y - leftBottomPoint.Y;
            if (width > height)
                return true;

            return false;
        }
        /// <summary>
        /// PDF转化成图片
        /// </summary>
        /// <param name="inputPdfPath"></param>
        /// <param name="outpdfPath"></param>
        /// <param name="quality"></param>
        public static void DrawPdfToPng(string inputPdfPath, string outpdfPath, UserSelectData.ImageQuality quality)
        {
            using (PDFDraw draw = new PDFDraw())
            {
                using (PDFDoc doc = new PDFDoc(inputPdfPath))
                {
                    doc.InitSecurityHandler();
                    if (quality == UserSelectData.ImageQuality.IMAGEHIGHER)
                    {
                        draw.SetDPI(300);
                    }
                    else if (quality == UserSelectData.ImageQuality.IMAGEMEDIUM)
                    {
                        draw.SetDPI(500);
                    }
                    else
                    {
                        draw.SetDPI(700);
                    }

                    Page pg = doc.GetPage(1);
                    draw.Export(pg, outpdfPath);
                }
            }
        }

        /// <summary>
        /// 判断是否等于aimValue值
        /// </summary>
        /// <param name="aimValue"></param>
        /// <returns></returns>
        public static bool DoubleEqualValue(double srcValue, double aimValue, double tolerance)
        {
            if (Math.Abs(srcValue - aimValue) < tolerance)
                return true;

            return false;
        }

        /// <summary>
        /// 向PPT中插入图片
        /// </summary>
        /// <param name="imagePaths"></param>
        /// <param name="outPathPPT"></param> 生成ppt目录
        /// <param name="namePPT"></param> ppt名字
        public static void InsertImageToPPT(List<string> imagePaths, List<RelatedData> relatedData, string outPathPPT, string namePPT)
        {
            PowerPoint.Application powerApplication = new PowerPoint.Application();

            CommonUtils utils = new CommonUtils(powerApplication);

            // 增加页且插入图片
            PowerPoint.Presentation presentation = powerApplication.Presentations.Add(MsoTriState.msoTrue);

            for (int i = 0; i < imagePaths.Count; i++)
            {
                PowerPoint.Slide slide = presentation.Slides.Add(1, PpSlideLayout.ppLayoutBlank);
                double xImageRation = 0;
                double yImageRation = 0;
                
                // 插入图片
                if (DoubleEqualValue(relatedData[i].PageText.Rotation, 0, 5))
                    ThPlotData.GetImagePosRelatedRation(relatedData[i].ImagePolyline, relatedData[i].PptPolyline, ref xImageRation, ref yImageRation);
                else   
                    ThPlotData.GetImagePosRelatedRationWithAngle(relatedData[i], ref xImageRation, ref yImageRation);

                // 宽高控制
                double pictureWidth = 620;
                double pictureHeight = 350;
                if (IsAdjustRationAndGetWidthHeight(relatedData[i],ref pictureWidth, ref pictureHeight))
                {
                    slide.Shapes.AddPicture(imagePaths[i], MsoTriState.msoFalse, MsoTriState.msoTrue, (float)(967 * xImageRation), (float)(544 * yImageRation), pictureWidth, pictureHeight);
                }
                else
                {
                    var shape = slide.Shapes.AddPicture(imagePaths[i], MsoTriState.msoFalse, MsoTriState.msoTrue, (float)(967 * xImageRation), (float)(544 * yImageRation));
                    shape.Rotation = 90;
                }
                
                // 增加文字
                var pptTextLst = relatedData[i].PptTextLst;
                var pptPolyline = relatedData[i].PptPolyline;

                // 对字体位置进行排序 然后分别设置字体大小
                if (pptTextLst.Count > 1)
                    SortTextVertical(ref pptTextLst);

                float fontSize = 22;
                MsoTriState fontValue = MsoTriState.msoTrue;
                for (int j = 0; j < pptTextLst.Count; j++)
                {
                    fontSize -= j * 2;
                    if (j != 0)
                        fontValue = MsoTriState.msoFalse;

                    double xRation = 0;
                    double yRation = 0;
                    GetPosRelatedRation(pptTextLst[j], pptPolyline, ref xRation, ref yRation);
                    slide.Shapes.AddTextEffect(MsoPresetTextEffect.msoTextEffect9, pptTextLst[j].TextString, "微软雅黑", fontSize,
                             fontValue, MsoTriState.msoFalse, (float)(967 * xRation), (float)(544 * yRation));
                }

                // 增加页码
                slide.Shapes.AddTextEffect(MsoPresetTextEffect.msoTextEffect1, relatedData[i].PageText.TextString, "微软雅黑", 12,
                            MsoTriState.msoFalse, MsoTriState.msoFalse, 900, 500);
            }

            // 保存
            string documentFile = Path.Combine(outPathPPT, namePPT);
            presentation.SaveAs(documentFile);
            powerApplication.Quit();
            powerApplication.Dispose();
        }

        /// <summary>
        /// 设置图片的宽和高值
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetPictureWidthAndHeight(RelatedData relatedData, ref double width, ref double height)
        {
            var imagePolyline = relatedData.ImagePolyline;
            var pptPolyline = relatedData.PptPolyline;

        }

        /// <summary>
        /// 判断是否需要控制宽高
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static bool IsAdjustRation(Polyline profile)
        {
            var ptLst = GetPolylinePoints(profile);
            var windowData = GetEntityPointsWindow(ptLst);
            double width = 0;
            double height = 0;

            GetWindowDataWidthAndHeight(windowData, ref width, ref height);
            if (width > height)
                return true;

            return false;
        }

        /// <summary>
        /// 判断是否进行宽高调整 如果调整，则宽和高值得获取
        /// </summary>
        /// <param name="relatedData"></param>
        /// <param name="pictureWidth"></param>
        /// <param name="pictureHeight"></param>
        /// <returns></returns>
        public static bool IsAdjustRationAndGetWidthHeight(RelatedData relatedData, ref double pictureWidth, ref double pictureHeight)
        {
            var imagePolyline = relatedData.ImagePolyline;
            var pptPolyline = relatedData.PptPolyline;
            var ptLst = GetPolylinePoints(imagePolyline);
            var windowDataImage = GetEntityPointsWindow(ptLst);
            double width = 0;
            double height = 0;

            GetWindowDataWidthAndHeight(windowDataImage, ref width, ref height);
            if (height > width)
                return false;

            // 水平打印需要调整
            var ptPptLst = GetPolylinePoints(pptPolyline);
            var windowDataPpt = GetEntityPointsWindow(ptPptLst);
            double widthPpt = 0;
            double heightPpt = 0;

            GetWindowDataWidthAndHeight(windowDataPpt, ref widthPpt, ref heightPpt);
            pictureWidth = width / widthPpt * 967;
            pictureHeight = height / heightPpt * 544;
            return true;
        }

        /// <summary>
        /// 获取窗口的宽和高
        /// </summary>
        /// <param name="winData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetWindowDataWidthAndHeight(WindowData winData, ref double width, ref double height)
        {
            var leftBottomPoint = winData.LeftBottomPoint;
            var rightTopPoint = winData.RightTopPoint;
            width = rightTopPoint.X - leftBottomPoint.X;
            height = rightTopPoint.Y - leftBottomPoint.Y;
        }

        /// <summary>
        /// 对文字进行纵向排序
        /// </summary>
        /// <param name="dbTexts"></param>
        public static void SortTextVertical(ref List<DBText> dbTexts)
        {
            var positions = new List<Autodesk.AutoCAD.Geometry.Point2d>();
            foreach (var dbText in dbTexts)
            {
                positions.Add(new Autodesk.AutoCAD.Geometry.Point2d(dbText.Position.X, dbText.Position.Y));
            }

            var windowData = GetEntityPointsWindow(positions);
            var topYValue = windowData.RightTopPoint.Y;
            dbTexts.Sort((s1, s2) => { return (Math.Abs(s1.Position.Y - topYValue)).CompareTo(Math.Abs(s2.Position.Y - topYValue)); });
        }

        /// <summary>
        /// 删除临时文件
        /// </summary>
        /// <param name="srcPath"></param>
        public static void EraseTempFiles(string srcPath, string pptName)
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                var pos = i.Name.IndexOf(".");
                var fileName = i.Name.Substring(0, pos);
                if (!fileName.Equals(pptName))
                    File.Delete(i.FullName);      //删除指定文件
            }
        }

        public static void EraseTempFiles(List<string> srcPaths)
        {
            foreach (var name in srcPaths)
            {
                File.Delete(name);      //删除指定文件
            }
        }
    }
}
