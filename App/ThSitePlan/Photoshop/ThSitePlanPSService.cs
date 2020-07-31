using System;
using Photoshop;
using System.IO;
using System.Linq;
using ThSitePlan.Configuration;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PsApplication = Photoshop.Application;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSService : IDisposable
    {
        public int ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public PsApplication Application { get; set; }
        public Document CurrentFirstDocument { get; set; }

        public ThSitePlanPSService()
        {
            try
            {
                Application = new PsApplication();
                Application.Visible = false;
            }
            catch(COMException e) 
            {
                Application = null;
                ErrorCode = e.ErrorCode;
                ErrorMessage = e.Message;
            }
        }

        public void ShowPSApp()
        {
            this.Application.Visible = true;
        }

        public void Dispose()
        {
            //
        }

        // 创建一个空白图纸
        public void NewEmptyDocument(string DocName, double psdocwidth, double psdochight)
        {
            var StartUnits = Application.Preferences.RulerUnits;
            Application.Preferences.RulerUnits = PsUnits.psCM;
            Document currentdoc = Application.Documents.Add(
                psdocwidth,
                psdochight,
                300,
                DocName);
            Application.Preferences.RulerUnits = StartUnits;

            ArtLayer firstlayer = Application.ActiveDocument.ArtLayers[1];
            currentdoc.ActiveLayer = firstlayer;
            Selection ChannelSelection = currentdoc.Selection;
            SolidColor ColorInPS = new SolidColor();
            ColorInPS.RGB.Red = 228;
            ColorInPS.RGB.Green = 228;
            ColorInPS.RGB.Blue = 221;
            ChannelSelection.Fill(ColorInPS);

            CurrentFirstDocument = currentdoc;
        }

        //PS中载入Channel选区并填充
        public void FillBySelectChannel(string LayerNameToBeFill, ThSitePlanConfigItem configItem)
        {
            var document = Application.ActiveDocument;
            document.ActiveLayer = document.ArtLayers[LayerNameToBeFill];
            Selection ChannelSelection = document.Selection;
            ChannelSelection.Load(document.Channels[2], PsSelectionType.psReplaceSelection, true);
            //获取配置文件中传入的
            string FillColor = configItem.Properties["Color"].ToString();
            var RGB_Red = FillColor.Split(',')[0];
            var RGB_Green = FillColor.Split(',')[1];
            var RGB_Blue = FillColor.Split(',')[2];

            SolidColor ColorInPS = new SolidColor();
            ColorInPS.RGB.Red = Convert.ToDouble(RGB_Red);
            ColorInPS.RGB.Green = Convert.ToDouble(RGB_Green);
            ColorInPS.RGB.Blue = Convert.ToDouble(RGB_Blue);
            ChannelSelection.Fill(ColorInPS);
            ChannelSelection.Deselect();
        }

        //PS中打开并设置图层
        public Document OpenAndSet(string path, ThSitePlanConfigItem configItem)
        {
            string fileName = (string)configItem.Properties["Name"] + ".pdf";
            string fullPath = Path.Combine(path, fileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            //fileName = (string)configItem.Properties["Name"] + ".pdf";
            //fullPath = Path.Combine(path, fileName);
            //装载PDF获取图层名称
            PDFOpenOptions pdfop = new PDFOpenOptions
            {
                CropPage = PsCropToType.psBoundingBox
            };
            Document NewOpenDoc = Application.Open(fullPath, pdfop);
            string CurDocNa = NewOpenDoc.Name;
            ArtLayer newlayer = NewOpenDoc.ArtLayers[1];
            newlayer.Translate(0,0);
            newlayer.Name = CurDocNa;
            string DocName = NewOpenDoc.Name;

            //设置图层的不透明度和填充颜色
            FillBySelectChannel(NewOpenDoc.Name, configItem);
            newlayer.Opacity = Convert.ToDouble(configItem.Properties["Opacity"]);

            return NewOpenDoc;

        }

        //在指定PS文档中检索指定图层，找到插入位置
        public LayerSet SearchInsertLoc(string docname, Document serdoc)
        {
            List<string> DocNameSpt = docname.Split('-').ToList();
            LayerSets FirstLayerSets = serdoc.LayerSets;
            LayerSet SerLaySet = null;
            for (int i = 0; i < DocNameSpt.Count - 1; i++)
            {
                if (i == 0)
                {
                    foreach (LayerSet LaysetInCurDOC in FirstLayerSets)
                    {
                        if (LaysetInCurDOC.Name == DocNameSpt[i])
                        {
                            SerLaySet = LaysetInCurDOC;
                            SerLaySet.Name = DocNameSpt[i];
                            break;
                        }
                    }

                    if (SerLaySet == null)
                    {
                        SerLaySet = FirstLayerSets.Add();
                        SerLaySet.Name = DocNameSpt[i];

                        SerLaySet.Move(serdoc, PsElementPlacement.psPlaceAtEnd);

                        break;
                    }
                }
                else
                {
                    bool FindOrNot = false;

                    if (SerLaySet.IsNull())
                    {
                        return null;
                    }

                    foreach (LayerSet LaysetInCurDOC in SerLaySet.LayerSets)
                    {
                        if (LaysetInCurDOC.Name == DocNameSpt[i])
                        {
                            SerLaySet = LaysetInCurDOC;
                            SerLaySet.Name = DocNameSpt[i];
                            FindOrNot = true;
                            break;
                        }
                    }

                    if (FindOrNot == false)
                    {
                        LayerSets parentlayerserts = SerLaySet.LayerSets;
                        SerLaySet = parentlayerserts.Add();
                        int layersetcount = parentlayerserts.Count;
                        if (layersetcount > 1)
                        {
                            for (int j = layersetcount; j >1 ; j--)
                            {
                                parentlayerserts[layersetcount].Move(parentlayerserts[1], PsElementPlacement.psPlaceBefore);
                            }
                        }
                        SerLaySet.Name = DocNameSpt[i];
                        break;
                    }
                }
            }

            return SerLaySet;
        }

        //将图层移动到指定位置，如需要，为其创建分组
        public void MoveLayerIntoSet(ArtLayer OperateLayer,LayerSet EndLayerSet)
        {
            List<string> CurDoc_Sets = OperateLayer.Name.Split('-').ToList();
            OperateLayer.Name = CurDoc_Sets.Last();
            if (CurDoc_Sets.Count > 1)
            {
                int CurIndex = CurDoc_Sets.IndexOf(EndLayerSet.Name);

                //若当前图层指针指向的图层组名并不是当前待移动的图层的图层名中最内侧分组名
                if (CurIndex != CurDoc_Sets.Count - 2)
                {
                    for (int i = CurIndex + 1; i < CurDoc_Sets.Count - 1; i++)
                    {
                        LayerSets endsets = EndLayerSet.LayerSets;
                        EndLayerSet = endsets.Add();
                        EndLayerSet.Name = CurDoc_Sets[i];
                    }
                }
                OperateLayer.Move(EndLayerSet, PsElementPlacement.psPlaceAtEnd);
            }

            else
            {
                OperateLayer.Move(CurrentFirstDocument, PsElementPlacement.psPlaceAtEnd);
            }

        }

        //将当前打开的文档中的图层复制到第一个空白文档中，关闭当前文档
        public void CopyNewToFirst(Document newdoc,Document firsdoc)
        {
            newdoc.ArtLayers[1].Duplicate(firsdoc, PsElementPlacement.psPlaceAtEnd);
            newdoc.Close(PsSaveOptions.psDoNotSaveChanges);
        }

        //在指定PS文档中检索指定图层，找到插入位置
        public ArtLayer FindUpdateLocation(string docname, Document serdoc)
        {
            List<string> DocNameSpt = docname.Split('-').ToList();
            LayerSets FirstLayerSets = serdoc.LayerSets;
            LayerSet SerLaySet = null;
            for (int i = 0; i < DocNameSpt.Count - 1; i++)
            {
                if (i == 0)
                {
                    foreach (LayerSet LaysetInCurDOC in FirstLayerSets)
                    {
                        if (LaysetInCurDOC.Name == DocNameSpt[i])
                        {
                            SerLaySet = LaysetInCurDOC;
                            SerLaySet.Name = DocNameSpt[i];
                            break;
                        }
                    }

                    if (SerLaySet.IsNull())
                    {
                        return null;
                    }
                }
                else
                {
                    foreach (LayerSet LaysetInCurDOC in SerLaySet.LayerSets)
                    {
                        if (LaysetInCurDOC.Name == DocNameSpt[i])
                        {
                            SerLaySet = LaysetInCurDOC;
                            break;
                        }
                    }
                }
            }

            if (SerLaySet == null)
            {
                return null;
            }

            foreach (ArtLayer artlayer in SerLaySet.ArtLayers)
            {
                if (DocNameSpt.Last() == artlayer.Name)
                {
                    return artlayer;
                }
            }

            return null;
        }


        //在首文档最外层layers中按名称查找需要更新层，更新其内容
        public bool UpdateLayersInOutSet(Document searchdoc, Document origdoc)
        {
            foreach (ArtLayer lyit in origdoc.ArtLayers)
            {
                if (lyit.Name == searchdoc.ArtLayers[1].Name)
                {
                    Application.ActiveDocument = origdoc;
                    lyit.Clear();
                    Application.ActiveDocument = searchdoc;
                    searchdoc.ArtLayers[1].Duplicate(lyit, PsElementPlacement.psPlaceAfter);
                    searchdoc.Close(PsSaveOptions.psDoNotSaveChanges);
                    Application.ActiveDocument = origdoc;
                    lyit.Merge();
                    return true;
                }
            }
            return false;
        }

        //在插入位置下找到该文档并替换
        public void UpdateLayerInSet(ArtLayer OperateLayer, ArtLayer findlayer)
        {
            List<string> CurDoc_Sets = OperateLayer.Name.Split('-').ToList();
            OperateLayer.Name = CurDoc_Sets.Last();
            OperateLayer.Move(findlayer, PsElementPlacement.psPlaceAfter);
            findlayer.Delete();
            //foreach (ArtLayer lyit in EndLayerSet.ArtLayers)
            //{
            //    if (lyit.Name == OperateLayer.Name)
            //    {
            //        OperateLayer.Move(lyit, PsElementPlacement.psPlaceAfter);
            //        lyit.Delete();
            //        break;
            //    }
            //}
        }

        //用于更新操作时导出并保存PSD文件
        public void ExportToFileForUpdate(string path)
        {
            if (Application.Documents.Count == 0)
            {
                return;
            }
            Application.ActiveDocument.SaveAs(path);
            Application.Visible = true;
        }

        //用于生成操作时导出并保存PSD文件
        public void ExportToFile(string path)
        {
            string nameprefi = "一键彩总图";
            int originaldwgindex = 1;
            string psfilename = nameprefi + originaldwgindex + ".psd";
            while (File.Exists(Path.Combine(path, psfilename)))
            {
                originaldwgindex++;
                psfilename = nameprefi + originaldwgindex + ".psd";
            }
            Application.ActiveDocument.SaveAs(Path.Combine(path, psfilename));
            Application.Visible = true;
        }

        //在当前文档中查找指定名称的图层
        public ArtLayer FindLayerByName(string docname)
        {
            Document searchdoc = Application.ActiveDocument;
            //在PS根节点下检索，若找到直接返回
            foreach (ArtLayer outlayer in searchdoc.ArtLayers)
            {
                if (outlayer.Name == docname)
                {
                    return outlayer;
                }
            }

            //在PS中所有图层组中检索
            return FindUpdateLocation(docname, searchdoc);
        }

    }
}