using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThPlot
{
    public class ThPlotApp : IExtensionApplication
    {
        public void Initialize()
        {
            pdftron.PDFNet.Initialize();
        }

        public void Terminate()
        {
            pdftron.PDFNet.Terminate();
        }

        [CommandMethod("TIANHUACAD", "THPTP", CommandFlags.Modal)]
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
            var pos = userData.PrintOutPath.LastIndexOf(@"\");
            string outPath = userData.PrintOutPath.Substring(0, pos);
            string pptName = userData.PrintOutPath.Substring(pos + 1);

            // 生成PDF
            var outPdfPaths = new List<string>();
            foreach (var related in relatedDataLst)
            {
                if (related.ImagePolyline == null || related.PptPolyline == null)
                    continue;

                var ptLst = ThPlotData.GetPolylinePoints(related.ImagePolyline);
                var windowData = ThPlotData.GetEntityPointsWindow(ptLst);
                var window = new Extents2d(windowData.LeftBottomPoint.X, windowData.LeftBottomPoint.Y, windowData.RightTopPoint.X, windowData.RightTopPoint.Y);
                var pdfName = related.PageText.TextString + ".pdf";
                var outPdfPath = System.IO.Path.Combine(outPath, pdfName);
                outPdfPaths.Add(outPdfPath);
                ThPlotUtil.PlotWithWindow(window, outPdfPath, userData.PrintStyle);
            }

            // 生成PNG图片
            List<string> pngPaths = new List<string>();
            for (int i = 0; i < outPdfPaths.Count; i++)
            {
                var pngName = relatedDataLst[i].PageText.TextString + ".png";
                var pngPath = System.IO.Path.Combine(outPath, pngName);
                ThPlotUtil.DrawPdfToPng(outPdfPaths[i], pngPath, userData.ImageQua);
                pngPaths.Add(pngPath);
            }

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
