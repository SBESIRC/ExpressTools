﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.PlottingServices;
using PowerPoint = NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Tools;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi.Enums;
using static ThPlot.ThPlotData;
using Linq2Acad;
using PdfiumViewer;
using DotNetARX;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThPlot
{
    public class ThPlotUtil
    {
        /// <summary>
        /// 打印窗口操作 使用自定义的打印窗口设置
        /// </summary>
        /// <param name="window"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="printStyle"></param>
        public static void PlotWithWindowWithSelfPlot(Extents2d window, string outputFilePath, string printStyle)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                using (new ThAppTools.ManagedSystemVariable("BACKGROUNDPLOT", 0))
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
                    psv.SetPlotConfigurationName(plotSetting, "DWG To PDF.pc3", "ISO_full_bleed_A2_(420.00_x_594.00_MM)");
                    psv.RefreshLists(plotSetting);
                    psv.SetCurrentStyleSheet(plotSetting, printStyle);

                    // 自定义打印信息
                    // 设置打印窗口等信息
                    psv.SetPlotWindowArea(plotSetting, window);
                    psv.SetPlotType(plotSetting, Autodesk.AutoCAD.DatabaseServices.PlotType.Window);
                    // 打印比例-布满图纸
                    psv.SetUseStandardScale(plotSetting, true);
                    psv.SetStdScaleType(plotSetting, StdScaleType.ScaleToFit);
                    // 图形方向（默认是横向）
                    psv.SetPlotRotation(plotSetting, PlotRotation.Degrees000);
                    if (HorizontalPrint(window))
                        psv.SetPlotRotation(plotSetting, PlotRotation.Degrees090);
                    // 打印偏移-居中打印
                    psv.SetPlotCentered(plotSetting, true);

                    // 开始打印
                    using (var plotEngine = PlotFactory.CreatePublishEngine())
                    {
                        plotEngine.Plot(layoutObj, plotSetting, outputFilePath, 1, false, false, true);
                    }
                }
            }
        }

        /// <summary>
        /// 打印PPT的进度条设置
        /// </summary>
        /// <param name="plotDlg"></param>
        public static void StartPlotProgressPPT(ref PlotProgressDialog plotDlg)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            //设置打印进度框中的提示信息
            string dialogTitle = "批量打印PPT";
            plotDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, dialogTitle);
            plotDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "正在打印PPT，请稍后...");
            plotDlg.LowerPlotProgressRange = 0;//开始的打印进度
            plotDlg.UpperPlotProgressRange = 100;//线束时的打印进度
            plotDlg.PlotProgressPos = 0;//当前进度为0，表示开始
            plotDlg.OnBeginPlot();//打印开始，进程框开始工作
            plotDlg.IsVisible = true;//显示打印进度框
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
            using (PdfDocument document = PdfDocument.Load(inputPdfPath))
            {
                System.Drawing.Image image = null;
                if (quality == UserSelectData.ImageQuality.IMAGELOWER)
                {
                    image = document.Render(0, 300, 300, PdfRenderFlags.None);
                }
                else if (quality == UserSelectData.ImageQuality.IMAGEMEDIUM)
                {
                    image = document.Render(0, 500, 500, PdfRenderFlags.None);
                }
                else if (quality == UserSelectData.ImageQuality.IMAGEHIGHER)
                {
                    image = document.Render(0, 700, 700, PdfRenderFlags.None);
                }

                if (image != null)
                {
                    image.Save(outpdfPath);
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
        /// 对角度进行标准化
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double StandardAngle(double angle)
        {
            if (angle > 0)
            {
                while (angle > 360)
                {
                    angle -= 360;
                }

                return angle;
            }
            else if (angle < 0)
            {
                while (angle < 0)
                {
                    angle += 360;
                }

                return angle;
            }

            return angle;
        }

        /// <summary>
        /// 向PPT中插入图片
        /// </summary>
        /// <param name="imagePaths"></param>
        /// <param name="outPathPPT"></param> 生成ppt目录
        /// <param name="namePPT"></param> ppt名字
        public static void InsertImageToPPT(List<string> imagePaths, List<RelatedData> relatedData, string outPathPPT, string namePPT)
        {
            try
            {
                PowerPoint.Application powerApplication = PowerPoint.Application.GetActiveInstance();
                if (powerApplication == null)
                {
                    powerApplication = new PowerPoint.Application();
                }
                CommonUtils utils = new CommonUtils(powerApplication);

                // 增加页且插入图片
                PowerPoint.Presentation presentation = powerApplication.Presentations.Add(MsoTriState.msoTrue);
                presentation.PageSetup.SlideSize = NetOffice.PowerPointApi.Enums.PpSlideSizeType.ppSlideSizeA3Paper;

                for (int i = 0; i < imagePaths.Count; i++)
                {
                    PowerPoint.Slide slide = presentation.Slides.Add(1, PpSlideLayout.ppLayoutBlank);
                    double xImageRatioPos = 0;
                    double yImageRatioPos = 0;

                    double textAngle = 0;
                    if (relatedData[i].PptTextLst == null || relatedData[i].PptTextLst.Count == 0)
                        textAngle = 0;
                    else
                        textAngle = relatedData[i].PptTextLst.First().Rotation / ThPlotData.PI * 180;
                    textAngle = StandardAngle(textAngle);
                    double pictureRotateWidth = 620;
                    double pictureRotateHeight = 350;

                    // 插入图片
                    if (DoubleEqualValue(textAngle, 0, 5) || DoubleEqualValue(textAngle, 360, 5))
                        ThPlotData.GetImagePosRelatedRation(relatedData[i].ImagePolyline, relatedData[i].PptPolyline, ref xImageRatioPos, ref yImageRatioPos);
                    else
                        ThPlotData.GetImagePosRelatedRationWithAngle(relatedData[i], ref xImageRatioPos, ref yImageRatioPos, ref pictureRotateWidth, ref pictureRotateHeight);

                    // 宽高控制
                    double pictureWidth = 620;
                    double pictureHeight = 350;
                    if (DoubleEqualValue(textAngle, 0, 5) || DoubleEqualValue(textAngle, 360, 5)) // 非旋转处理
                    {
                        if (IsAdjustRationAndGetWidthHeight(relatedData[i], ref pictureWidth, ref pictureHeight))
                        {
                            // 控制图片的宽度和高度值
                            slide.Shapes.AddPicture(imagePaths[i], MsoTriState.msoFalse, MsoTriState.msoTrue, (float)(xImageRatioPos), (float)(yImageRatioPos), pictureWidth, pictureHeight);
                        }
                        else
                        {
                            CalculateVerticalRatioSize(relatedData[i], ref pictureWidth, ref pictureHeight);
                            slide.Shapes.AddPicture(imagePaths[i], MsoTriState.msoFalse, MsoTriState.msoTrue, (float)(xImageRatioPos), (float)(yImageRatioPos), pictureWidth, pictureHeight);
                        }
                    }
                    else
                    {
                        // 旋转处理
                        var shape = slide.Shapes.AddPicture(imagePaths[i], MsoTriState.msoFalse, MsoTriState.msoTrue, (float)(xImageRatioPos), (float)(yImageRatioPos), pictureRotateWidth, pictureRotateHeight);
                        if (DoubleEqualValue(textAngle, 270, 5))
                            shape.Rotation = (float)(textAngle - 360);
                        else
                            shape.Rotation = (float)textAngle;
                    }

                    // 增加文字
                    var pptTextLst = relatedData[i].PptTextLst;
                    var pptPolyline = relatedData[i].PptPolyline;

                    // 对字体位置进行排序 然后分别设置字体大小
                    if (pptTextLst.Count > 1)
                    {
                        if (DoubleEqualValue(textAngle, 0, 5) || DoubleEqualValue(textAngle, 360, 5))
                        {
                            SortTextVertical(ref pptTextLst);
                        }
                        else
                        {
                            SortTextWithAngle(textAngle, ref pptTextLst);
                        }
                    }

                    float fontSize = 22;
                    for (int j = 0; j < pptTextLst.Count; j++)
                    {
                        fontSize -= j * 2;
                        double xRatio = 0;
                        double yRatio = 0;

                        if (DoubleEqualValue(textAngle, 0, 5) || DoubleEqualValue(textAngle, 360, 5))
                            GetTextPosRelatedRation(pptTextLst[j], pptPolyline, ref xRatio, ref yRatio);
                        else
                            GetTextPosRelatedRationWithAngle(pptTextLst[j], textAngle, pptPolyline, ref xRatio, ref yRatio);

                        pptTextLst[j].Rotation = textAngle;
                        var textShape = slide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal, (float)(ThPlotData.PPTWIDTH * xRatio), (float)(ThPlotData.PPTHEIGHT * yRatio), 300, 20);
                        textShape.TextFrame.TextRange.Text = pptTextLst[j].TextString;
                        textShape.TextFrame.TextRange.Font.Name = "微软雅黑";
                        textShape.TextFrame.TextRange.Font.Size = fontSize;
                        if (j == 0)
                            textShape.TextFrame.TextRange.Font.Bold = MsoTriState.msoTrue;
                    }

                    // 增加页码
                    var pageShape = slide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal, 950, 700, 30, 20);
                    pageShape.TextFrame.TextRange.Text = relatedData[i].PageText.TextString;
                    pageShape.TextFrame.TextRange.Font.Name = "微软雅黑";
                    pageShape.TextFrame.TextRange.Font.Size = 12;
                }

                // 保存成文件
                presentation.SaveAs(Path.Combine(outPathPPT, namePPT));

                // 退出PowerPoint
                powerApplication.Quit();
            }
            catch
            {
                // PowerPoint文件生成失败
            }
        }

        /// <summary>
        /// 不同角度进行文本排序
        /// </summary>
        /// <param name="pptTextLst"></param>
        public static void SortTextWithAngle(double textAngle, ref List<DBText> pptTextLst)
        {
            if (DoubleEqualValue(textAngle, 90, 5))
            {
                SortTextHorizontal(ref pptTextLst);
            }
            else if (DoubleEqualValue(textAngle, 180, 5))
            {
                SortTextVertical(ref pptTextLst);
                pptTextLst.Reverse();
            }
            else if (DoubleEqualValue(textAngle, 270, 5))
            {
                SortTextHorizontal(ref pptTextLst);
                pptTextLst.Reverse();
            }
        }

        /// <summary>
        /// 对文本进行水平方向上的排序
        /// </summary>
        /// <param name="pptTextLst"></param>
        public static void SortTextHorizontal(ref List<DBText> pptTextLst)
        {
            var positions = new List<Autodesk.AutoCAD.Geometry.Point2d>();
            foreach (var dbText in pptTextLst)
            {
                positions.Add(new Autodesk.AutoCAD.Geometry.Point2d(dbText.Position.X, dbText.Position.Y));
            }

            var windowData = GetEntityPointsWindow(positions);
            var minXValue = windowData.LeftBottomPoint.X;
            pptTextLst.Sort((s1, s2) => { return (Math.Abs(s1.Position.X - minXValue)).CompareTo(Math.Abs(s2.Position.X - minXValue)); });
        }

        /// <summary>
        /// 不同角度进行计算
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textAngle"></param>
        /// <param name="profile"></param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetTextPosRelatedRationWithAngle(DBText text, double textAngle, Polyline profile, ref double xRation, ref double yRation)
        {
            if (DoubleEqualValue(textAngle, 90, 5))
            {
                GetTextPosRelatedRationWithAngle90(text, profile, ref xRation, ref yRation);
            }
            else if (DoubleEqualValue(textAngle, 180, 5))
            {
                GetTextPosRelatedRationWithAngle180(text, profile, ref xRation, ref yRation);
            }
            else if (DoubleEqualValue(textAngle, 270, 5))
            {
                GetTextPosRelatedRationWithAngle270(text, profile, ref xRation, ref yRation);
            }
        }

        /// <summary>
        /// 左下顶点的文字处理 比例计算
        /// </summary>
        /// <param name="text"></param>
        /// <param name="profile"> PPT轮廓</param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetTextPosRelatedRationWithAngle90(DBText text, Polyline profile, ref double xRation, ref double yRation)
        {
            var ptLst = ThPlotData.GetPolylinePoints(profile);
            var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
            var height = windowData.RightTopPoint.Y - windowData.LeftBottomPoint.Y;
            var width = windowData.RightTopPoint.X - windowData.LeftBottomPoint.X;
            var leftBottomPos = windowData.LeftBottomPoint;
            var textPos = text.Position;

            var textPos2OrigionX = Math.Abs(textPos.Y - leftBottomPos.Y);
            var textPos2OrigionY = Math.Abs(textPos.X - leftBottomPos.X);
            xRation = textPos2OrigionX / height;
            yRation = textPos2OrigionY / width;
        }

        /// <summary>
        /// 右下顶点的文字处理 比例计算
        /// </summary>
        /// <param name="text"></param>
        /// <param name="profile"> PPT轮廓</param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetTextPosRelatedRationWithAngle180(DBText text, Polyline profile, ref double xRation, ref double yRation)
        {
            var ptLst = ThPlotData.GetPolylinePoints(profile);
            var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
            var height = windowData.RightTopPoint.Y - windowData.LeftBottomPoint.Y;
            var width = windowData.RightTopPoint.X - windowData.LeftBottomPoint.X;
            var rightTopPoint = windowData.RightTopPoint;
            var rightBottomPos = new Point2d(rightTopPoint.X, rightTopPoint.Y - height);
            var textPos = text.Position;

            var textPos2OrigionY = Math.Abs(textPos.Y - rightBottomPos.Y);
            var textPos2OrigionX = Math.Abs(textPos.X - rightBottomPos.X);
            xRation = textPos2OrigionX / width;
            yRation = textPos2OrigionY / height;
        }

        /// <summary>
        /// 右上顶点的文字处理 比例计算
        /// </summary>
        /// <param name="text"></param>
        /// <param name="profile"> PPT轮廓</param>
        /// <param name="xRation"></param>
        /// <param name="yRation"></param>
        public static void GetTextPosRelatedRationWithAngle270(DBText text, Polyline profile, ref double xRation, ref double yRation)
        {
            var ptLst = ThPlotData.GetPolylinePoints(profile);
            var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
            var height = windowData.RightTopPoint.Y - windowData.LeftBottomPoint.Y;
            var width = windowData.RightTopPoint.X - windowData.LeftBottomPoint.X;
            var rightTopPoint = windowData.RightTopPoint;
            var textPos = text.Position;

            var textPos2OrigionX = Math.Abs(textPos.Y - rightTopPoint.Y);
            var textPos2OrigionY = Math.Abs(textPos.X - rightTopPoint.X);
            xRation = textPos2OrigionX / height;
            yRation = textPos2OrigionY / width;
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
            pictureWidth = width / widthPpt * ThPlotData.PPTWIDTH;
            var paperSize = CalculatePaperSizeInfo(imagePolyline);
            pictureHeight = pictureWidth * paperSize;
            return true;
        }

        /// <summary>
        /// 调节纵向宽和高值
        /// </summary>
        /// <param name="relatedData"></param>
        /// <param name="pictureWidth"></param>
        /// <param name="pictureHeight"></param>
        /// <returns></returns>
        public static bool CalculateVerticalRatioSize(RelatedData relatedData, ref double pictureWidth, ref double pictureHeight)
        {
            var imagePolyline = relatedData.ImagePolyline;
            var pptPolyline = relatedData.PptPolyline;
            var ptLst = GetPolylinePoints(imagePolyline);
            var windowDataImage = GetEntityPointsWindow(ptLst);
            double width = 0;
            double height = 0;

            GetWindowDataWidthAndHeight(windowDataImage, ref width, ref height);

            // 水平打印需要调整
            var ptPptLst = GetPolylinePoints(pptPolyline);
            var windowDataPpt = GetEntityPointsWindow(ptPptLst);
            double widthPpt = 0;
            double heightPpt = 0;

            GetWindowDataWidthAndHeight(windowDataPpt, ref widthPpt, ref heightPpt);

            pictureHeight = height / heightPpt * ThPlotData.PPTHEIGHT;
            var paperSize = CalculatePaperSizeInfo(imagePolyline);
            pictureWidth = pictureHeight / paperSize;
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
