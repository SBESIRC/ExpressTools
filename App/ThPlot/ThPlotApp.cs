﻿using System.Windows.Forms;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.PlottingServices;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;

namespace ThPlot
{
    public class ThPlotApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("TIANHUACAD", "THBPP", CommandFlags.Modal)]
        static public void PlotPageSetup()
        {
            var thPlotForm = new ThPlotDialog();
            if (AcadApp.ShowModalDialog(thPlotForm) != DialogResult.OK)
            {
                return;
            }

            UserSelectData userData = thPlotForm.UserData;

            // 获取图层中的窗口
            var relatedDataLst = ThPlotData.MakeRelatedData(userData);
            if (relatedDataLst == null)
                return;

            var increStepPDF = 70.0 / relatedDataLst.Count;
            var pos = userData.PrintOutPath.LastIndexOf(@"\");
            string outPath = userData.PrintOutPath.Substring(0, pos);
            string pptName = userData.PrintOutPath.Substring(pos + 1);

            // 开始设置进度条打印
            double curProgressPos = 10;
            var plotDlg = new PlotProgressDialog(false, 1, false);
            ThPlotUtil.StartPlotProgressPPT(ref plotDlg);
            plotDlg.StartSheetProgress();
            plotDlg.SheetProgressPos = (int)curProgressPos;
            // 生成PDF
            var outPdfPaths = new List<string>();
            for (int m = 0; m < relatedDataLst.Count; m++)
            {
                var related = relatedDataLst[m];
                if (related.ImagePolyline == null || related.PptPolyline == null)
                    continue;

                var ptLst = ThPlotData.GetPolylinePoints(related.ImagePolyline);
                var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
                var ptLeft = windowData.LeftBottomPoint;
                var ptRight = windowData.RightTopPoint;
                var ptLeft3d = new Point3d(ptLeft.X, ptLeft.Y, 0);
                var ptRight3d = new Point3d(ptRight.X, ptRight.Y, 0);
                var ucsLeftBottomPoint = UCSTools.TranslateCoordinates(ptLeft3d, UCSTools.CoordSystem.UCS, UCSTools.CoordSystem.DCS);
                var ucsRightTopPoint = UCSTools.TranslateCoordinates(ptRight3d, UCSTools.CoordSystem.UCS, UCSTools.CoordSystem.DCS);
                var window = new Extents2d(ucsLeftBottomPoint.X, ucsLeftBottomPoint.Y, ucsRightTopPoint.X, ucsRightTopPoint.Y);
                var pdfName = related.PageText.TextString + ".pdf";
                var outPdfPath = System.IO.Path.Combine(outPath, pdfName);
                outPdfPaths.Add(outPdfPath);
                ThPlotUtil.PlotWithWindowWithSelfPlot(window, outPdfPath, userData.PrintStyle);
                curProgressPos += increStepPDF;
                plotDlg.SheetProgressPos = (int)(curProgressPos);
            }

            // 生成PNG图片
            var increStepPNG = 20.0 / outPdfPaths.Count;
            List<string> pngPaths = new List<string>();
            for (int i = 0; i < outPdfPaths.Count; i++)
            {
                var pngName = relatedDataLst[i].PageText.TextString + ".png";
                var pngPath = System.IO.Path.Combine(outPath, pngName);
                ThPlotUtil.DrawPdfToPng(outPdfPaths[i], pngPath, userData.ImageQua);
                pngPaths.Add(pngPath);
                curProgressPos += increStepPNG;
                plotDlg.SheetProgressPos = (int)curProgressPos;
            }

            plotDlg.EndSheetProgress();
            plotDlg.EndPlotProgress();
            // 生成ppt
            if (relatedDataLst.Count > 0)
                ThPlotUtil.InsertImageToPPT(pngPaths, relatedDataLst, outPath, pptName);

            // 删除临时文件
            var eraseTempFiles = new List<string>();
            eraseTempFiles.AddRange(outPdfPaths);
            eraseTempFiles.AddRange(pngPaths);
            ThPlotUtil.EraseTempFiles(eraseTempFiles);
        }
    }
}
