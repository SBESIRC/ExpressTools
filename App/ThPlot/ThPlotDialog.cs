using System;
using System.Windows.Forms;
using Linq2Acad;
using DotNetARX;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System.IO;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThPlot
{
    public partial class ThPlotDialog : Form
    {
        public UserSelectData UserData = new UserSelectData();
        private static string strPPTLayer = null;
        private static string strImageLayer = null;
        private static string strTextLayer = null;
        private Document doc = AcadApp.DocumentManager.MdiActiveDocument;
        private string DwgFileName;
        private int indexPage = 0;

        public ThPlotDialog()
        {
            InitializeComponent();
            InitializeControlValue();
        }

        // 初始化控件值
        private void InitializeControlValue()
        {
            var layerNames = ThPlotData.GetLayerNames();
            if (layerNames.Count != 0)
            {
                foreach (var layer in layerNames)
                {
                    comboPPTLayer.Items.Add(layer);
                    comboPrintImage.Items.Add(layer);
                    comboPrintTextLayer.Items.Add(layer);
                }

                comboPPTLayer.SelectedIndex = 0;
                comboPrintImage.SelectedIndex = 0;
                comboPrintTextLayer.SelectedIndex = 0;
            }

            if (strPPTLayer != null)
                comboPPTLayer.SelectedItem = strPPTLayer;
            if (strImageLayer != null)
                comboPrintImage.SelectedItem = strImageLayer;
            if (strTextLayer != null)
                comboPrintTextLayer.SelectedItem = strTextLayer;

            var fullName = doc.Name;
            var fullNameExtensionPos = fullName.LastIndexOf(@".");
            string pptFullName = fullName.Substring(0, fullNameExtensionPos);
            textOutPut.Text = pptFullName + ".pptx";
            UserData.PrintOutPath = pptFullName;
            var dwgFilePos = fullName.LastIndexOf(@"\");
            DwgFileName = pptFullName.Substring(dwgFilePos + 1);
            InitializeSyleCombox();
        }

        /// <summary>
        /// 初始化打印样式表的combox
        /// </summary>
        private void InitializeSyleCombox()
        {
            string ctbfolder = AcadApp.GetSystemVariable("ROAMABLEROOTPREFIX") as string;
            ctbfolder += "\\Plotters\\Plot Styles";

            DirectoryInfo dir = new DirectoryInfo(ctbfolder);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i.Name.Contains(".ctb"))
                {
                    comboPrintStyle.Items.Add(i.Name);
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction plotTrans = db.TransactionManager.StartTransaction())
            {
                LayoutManager layoutManager = LayoutManager.Current;
                ObjectId layoutId = layoutManager.GetLayoutId(layoutManager.CurrentLayout);
                Layout layoutObj = (Layout)plotTrans.GetObject(layoutId, OpenMode.ForRead);


                // 使用预定义的PlotSetting
                PlotSettings plotSetting = new PlotSettings(layoutObj.ModelType);
                plotSetting.CopyFrom(layoutObj);
                comboPrintStyle.SelectedItem = plotSetting.CurrentStyleSheet;
            }
        }

        // 选择需要批量打印框线
        private void btnSelectPrintProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioSelectPrint.Checked) // 一个一个选择PPT框线
                {
                    while (true)
                    {
                        using (AcadDatabase db = AcadDatabase.Active())
                        {
                            var totalPlines = Active.Database.GetSelection<Polyline>();
                            if (totalPlines == null || totalPlines.Count == 0)
                                return;

                            var pptLines = new List<Polyline>();

                            // 收集PPT图层框线 进行排版等插入页码处理
                            foreach (var pLine in totalPlines)
                            {
                                if (pLine.Layer.Equals(comboPPTLayer.SelectedItem as string))
                                    pptLines.Add(pLine);
                            }

                            if (pptLines.Count == 0)
                                return;

                            var pageWithProfiles = ThPlotData.CalculateProfileWithPages(pptLines, SelectDir.LEFT2RIGHTUP2DOWN, ref indexPage);

                            // 插入页码处理
                            foreach (var pageWithProfile in pageWithProfiles)
                            {
                                var dbText = ThPlotData.SetPageTextToProfileCorner(pageWithProfile.Profile, pageWithProfile.PageText);
                                var objectId = db.ModelSpace.Add(dbText);
                                db.ModelSpace.Element(objectId, true).Layer = pageWithProfile.Profile.Layer;
                            }

                            lblSelectCount.Text = indexPage.ToString();
                            var findCount = pageWithProfiles.Count;
                            string CommndLineTip = "找到" + findCount.ToString() + "个，总计" + indexPage.ToString() + "个";
                            Active.WriteMessage(CommndLineTip);
                        }
                    }
                }
                else // 框选， 左右、上下 或者上下/左右
                {
                    using (AcadDatabase db = AcadDatabase.Active())
                    {
                        var totalPlines = Active.Database.GetSelection<Polyline>();
                        var pptLines = new List<Polyline>();

                        // 收集PPT图层框线 进行排版等插入页码处理
                        foreach (var pLine in totalPlines)
                        {
                            if (pLine.Layer.Equals(comboPPTLayer.SelectedItem as string))
                                pptLines.Add(pLine);
                        }

                        SelectDir dir = (radioLeft2Right.Checked == true) ? SelectDir.LEFT2RIGHTUP2DOWN : SelectDir.UP2DOWNLEFT2RIGHT;
                        var pageWithProfiles = ThPlotData.CalculateProfileWithPages(pptLines, dir, ref indexPage);

                        // 插入页码处理
                        foreach (var pageWithProfile in pageWithProfiles)
                        {
                            var dbText = ThPlotData.SetPageTextToProfileCorner(pageWithProfile.Profile, pageWithProfile.PageText);
                            var objectId = db.ModelSpace.Add(dbText);
                            db.ModelSpace.Element(objectId, true).Layer = pageWithProfile.Profile.Layer;
                        }

                        lblSelectCount.Text = indexPage.ToString();
                        var findCount = pageWithProfiles.Count;
                        string CommndLineTip = "找到" + findCount.ToString() + "个，总计" + indexPage.ToString() + "个";
                        Active.WriteMessage(CommndLineTip);
                    }
                }
            }
            catch
            {

            }
        }

        // 点选打印图层 PPT
        private void btnPPTSelectLayer_Click(object sender, EventArgs e)
        {
            var objectId = PickTool.PickEntity(this, "请选择PPT框线图层");
            if (objectId.IsNull)
                return;

            SelectLayerShow(comboPPTLayer, objectId);
        }

        // 点选打印图层 Image
        private void btnPrintImageLayer_Click(object sender, EventArgs e)
        {
            var objectId = PickTool.PickEntity(this, "指定打印窗口线图层");
            if (objectId.IsNull)
                return;

            SelectLayerShow(comboPrintImage, objectId);
        }

        // 点选打印图层 文本
        private void btnPrintTextLayer_Click(object sender, EventArgs e)
        {
            var objectId = PickTool.PickEntity(this, "指定需打印文字图层");
            if (objectId.IsNull)
                return;

            SelectLayerShow(comboPrintTextLayer, objectId);
        }

        /// <summary>
        /// 选择需要显示的图层
        /// </summary>
        /// <param name="SelectBox"></param>
        /// <param name="objectId"></param>
        private void SelectLayerShow(ComboBox SelectBox, ObjectId objectId)
        {
            using (AcadDatabase db = AcadDatabase.Active())
            {
                SelectBox.SelectedItem = db.ModelSpace.Element(objectId).Layer;
            }
        }

        private void ThPlotDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            UserData.PPTLayer = comboPPTLayer.SelectedItem as string;
            UserData.ImageLayer = comboPrintImage.SelectedItem as string;
            UserData.PrintTextLayer = comboPrintTextLayer.SelectedItem as string;
            UserData.PrintStyle = comboPrintStyle.SelectedItem as string;
            if (UserData.PrintStyle == null)
                UserData.PrintStyle = "monochrome.ctb";

            strPPTLayer = UserData.PPTLayer;
            strImageLayer = UserData.ImageLayer;
            strTextLayer = UserData.PrintTextLayer;
            if (radioImageLower.Checked)
                UserData.ImageQua = UserSelectData.ImageQuality.IMAGELOWER;
            else if (radioImageMedium.Checked)
                UserData.ImageQua = UserSelectData.ImageQuality.IMAGEMEDIUM;
            else
                UserData.ImageQua = UserSelectData.ImageQuality.IMAGEHIGHER;

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = DwgFileName;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fileSrcName = dialog.FileName;
                var extensionPos = fileSrcName.IndexOf(@".");
                string fileValidName;
                if (extensionPos == -1)
                {
                    fileValidName = fileSrcName;
                }
                else
                {
                    fileValidName = fileSrcName.Substring(0, extensionPos);
                }

                UserData.PrintOutPath = fileValidName;
                textOutPut.Text = fileValidName + ".pptx";
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            // 插入模板文件
            if (UserData.InsertTemplateFile)
                return;

            UserData.InsertTemplateFile = true;
        }
    }
}
