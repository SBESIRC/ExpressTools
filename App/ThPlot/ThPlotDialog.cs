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
using Autodesk.AutoCAD.EditorInput;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Colors;

namespace ThPlot
{
    public partial class ThPlotDialog : Form
    {
        public UserSelectData UserData = new UserSelectData();

        private DBObjectCollection previewLst = new DBObjectCollection(); // 界面预览数据
        private ObjectId textStyleId; // 文字显示样式
        // 进程内记忆功能
        private static string strPPTLayer = null;
        private static string strImageLayer = null;
        private static string strTextLayer = null;

        // 打印样式， 图片质量， 选择打印方式
        public static string strPrintStyle = null;
        public static UserSelectData.ImageQuality imageQuality = UserSelectData.ImageQuality.IMAGEUNKOWN;
        public static UserSelectData.SelectWay selectWay = UserSelectData.SelectWay.SELECTUNKNOWN;

        public static bool bTemplateFileUse = false;
        private static string strTemplatePPTLayer = "天华PPT框线";
        private static string strTemplateImageLayer = "天华打印窗口线";
        private static string strTemplateTextLayer = "天华PPT文字";

        private Document doc = AcadApp.DocumentManager.MdiActiveDocument;
        private string DwgFileName;
        private int indexPage = 0;

        public ThPlotDialog()
        {
            InitializeComponent();
            InitializeControlValue();
            textStyleId = ThPlotData.GetIdFromSymbolTable();
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

                // 手动选择的图层信息优先级更高
                if (bTemplateFileUse && strPPTLayer == null)
                {
                    comboPPTLayer.SelectedItem = strTemplatePPTLayer;
                    comboPrintImage.SelectedItem = strTemplateImageLayer;
                    comboPrintTextLayer.SelectedItem = strTemplateTextLayer;
                }
                else
                {
                    comboPPTLayer.SelectedIndex = 0;
                    comboPrintImage.SelectedIndex = 0;
                    comboPrintTextLayer.SelectedIndex = 0;
                }
            }

            // 如果图纸中有保存的插入图纸层
            if (layerNames.Contains("天华PPT框线"))
            {
                comboPPTLayer.SelectedItem = "天华PPT框线";
            }

            if (layerNames.Contains("天华打印窗口线"))
            {
                comboPrintImage.SelectedItem = "天华打印窗口线";
            }

            if (layerNames.Contains("天华PPT文字"))
            {
                comboPrintTextLayer.SelectedItem = "天华PPT文字";
            }


            if (strPPTLayer != null)
                comboPPTLayer.SelectedItem = strPPTLayer;
            if (strImageLayer != null)
                comboPrintImage.SelectedItem = strImageLayer;
            if (strTextLayer != null)
                comboPrintTextLayer.SelectedItem = strTextLayer;

            // 打印样式 ， 照片质量， 选择打印方式
            if (imageQuality != UserSelectData.ImageQuality.IMAGEUNKOWN)
            {
                if (imageQuality == UserSelectData.ImageQuality.IMAGELOWER)
                    radioImageLower.Checked = true;
                else if (imageQuality == UserSelectData.ImageQuality.IMAGEMEDIUM)
                    radioImageMedium.Checked = true;
                else
                    radioImageHigher.Checked = true;
            }

            if (selectWay != UserSelectData.SelectWay.SELECTUNKNOWN)
            {
                if (selectWay == UserSelectData.SelectWay.SINGLESELECT)
                    radioSelectPrint.Checked = true;
                else if (selectWay == UserSelectData.SelectWay.RECTSELECTLEFTRIGHT)
                    radioLeft2Right.Checked = true;
                else if (selectWay == UserSelectData.SelectWay.RECTSELECTUPDOWN)
                    radioTop2Down.Checked = true;
            }

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

            if (strPrintStyle != null)
                comboPrintStyle.SelectedItem = strPrintStyle;
        }

        // 选择需要批量打印框线
        private void btnSelectPrintProfile_Click(object sender, EventArgs e)
        {
            try
            {
                string layer = comboPPTLayer.SelectedItem as string;
                PromptSelectionResult result;
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };

                var filterlist = OpFilter.Bulid(
                    o => o.Dxf((int)DxfCode.Start) == "LWPOLYLINE" &
                    o.Dxf((int)DxfCode.LayerName) == layer);

                if (radioSelectPrint.Checked) // 一个一个选择PPT框线
                {
                    using (EditorUserInteraction inter = Active.Editor.StartUserInteraction(this))
                    {
                        using (AcadDatabase db = AcadDatabase.Active())
                        {
                            CreateLayer(ThPlotData.PAGELAYER);
                            do
                            {
                                result = Active.Editor.GetSelection(options, filterlist);
                                if (result.Status == PromptStatus.OK)
                                {
                                    // 收集PPT图层框线 进行排版等插入页码处理
                                    var pptLines = new List<Polyline>();
                                    CollectPLines(result, pptLines);

                                    CalculatePagesAndInsert(pptLines, SelectDir.LEFT2RIGHTUP2DOWN, ref indexPage, ThPlotData.PAGELAYER);
                                }
                            } while (result.Status == PromptStatus.OK);
                        }
                    }
                }
                else // 框选， 左右、上下 或者上下/左右
                {
                    using (EditorUserInteraction inter = Active.Editor.StartUserInteraction(this))
                    {
                        using (AcadDatabase db = AcadDatabase.Active())
                        {
                            result = Active.Editor.GetSelection(options, filterlist);
                            if (result.Status == PromptStatus.OK)
                            {
                                CreateLayer(ThPlotData.PAGELAYER);
                                // 收集PPT图层框线 进行排版等插入页码处理
                                var pptLines = new List<Polyline>();
                                CollectPLines(result, pptLines);

                                SelectDir dir = (radioLeft2Right.Checked == true) ? SelectDir.LEFT2RIGHTUP2DOWN : SelectDir.UP2DOWNLEFT2RIGHT;
                                CalculatePagesAndInsert(pptLines, dir, ref indexPage, ThPlotData.PAGELAYER);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void CollectPLines(PromptSelectionResult result, List<Polyline> polylines)
        {
            using (var db = AcadDatabase.Active())
            {
                foreach (var objId in result.Value.GetObjectIds())
                {
                    var pLine = db.Element<Polyline>(objId);
                    polylines.Add(pLine);
                }
            }
        }

        /// <summary>
        /// 插入页码
        /// </summary>
        /// <param name="polylines"></param>
        /// <param name="dir"></param>
        /// <param name="indexPage"></param>
        private void CalculatePagesAndInsert(List<Polyline> polylines, SelectDir dir, ref int indexPage, string layerName)
        {
            var pageWithProfiles = ThPlotData.CalculateProfileWithPages(polylines, dir, ref indexPage);

            var doc = Application.DocumentManager.MdiActiveDocument;
            var database = doc.Database;

            // 插入页码处理
            using (var db = AcadDatabase.Active())
            {
                foreach (var pageWithProfile in pageWithProfiles)
                {
                    var dbText = ThPlotData.SetPageTextToProfileCorner(pageWithProfile.Profile, pageWithProfile.PageText);
                    ThPlotData.ShowPageText(previewLst, pageWithProfile.Profile, dbText, textStyleId);
                    dbText.ColorIndex = 256;
                    dbText.Visible = false;
                    var objectId = db.ModelSpace.Add(dbText);
                    db.ModelSpace.Element(objectId, true).Layer = layerName;
                    using (var tr = database.TransactionManager.StartTransaction())
                    {
                        database.TransactionManager.QueueForGraphicsFlush();
                    }
                }
            }

            lblSelectCount.Text = indexPage.ToString();
            var findCount = pageWithProfiles.Count;
            string CommndLineTip = "找到" + findCount.ToString() + "个，总计" + indexPage.ToString() + "个";
            Active.WriteMessage(CommndLineTip);
        }

        private void CreateLayer(string layerName)
        {
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                LayerTableRecord layerRecord = null;
                foreach (var layer in layers)
                {
                    if (layer.Name.Equals(layerName))
                    {
                        return;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(layerName);
                    layerRecord.Color = Color.FromRgb(0, 255, 0);
                    layerRecord.IsPlottable = false;
                }
            }
        }

        // 点选打印图层 PPT
        private void btnPPTSelectLayer_Click(object sender, EventArgs e)
        {
            var objectId = ThPickTool.PickEntity(this, "请选择PPT框线图层");
            if (objectId.IsNull)
                return;

            SelectLayerShow(comboPPTLayer, objectId);
        }

        // 点选打印图层 Image
        private void btnPrintImageLayer_Click(object sender, EventArgs e)
        {
            var objectId = ThPickTool.PickEntity(this, "指定打印窗口线图层");
            if (objectId.IsNull)
                return;

            SelectLayerShow(comboPrintImage, objectId);
        }

        // 点选打印图层 文本
        private void btnPrintTextLayer_Click(object sender, EventArgs e)
        {
            var objectId = ThPickTool.PickEntity(this, "指定需打印文字图层");
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


            strPrintStyle = UserData.PrintStyle;
            imageQuality = UserData.ImageQua;
            if (radioSelectPrint.Checked)
                UserData.SelectStyle = UserSelectData.SelectWay.SINGLESELECT;
            if (radioLeft2Right.Checked)
                UserData.SelectStyle = UserSelectData.SelectWay.RECTSELECTLEFTRIGHT;
            if (radioTop2Down.Checked)
                UserData.SelectStyle = UserSelectData.SelectWay.RECTSELECTUPDOWN;
            selectWay = UserData.SelectStyle;

            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                foreach (DBObject obj in previewLst)
                {
                    tm.EraseTransient(obj, new IntegerCollection());
                    obj.Dispose();
                }
            }
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
