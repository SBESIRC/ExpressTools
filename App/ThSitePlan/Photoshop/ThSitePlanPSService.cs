using System;
using Photoshop;
using System.Drawing;
using ThSitePlan.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PsApplication = Photoshop.Application;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSService : IDisposable
    {
        public PsApplication Application { get; set; }

        public Document CurrentFirstDocument { get; set; }

        public ThSitePlanPSService()
        {
            Application = new PsApplication();
        }

        public void Dispose()
        {
            //
        }

        // 创建一个空白图纸
        public void NewEmptyDocument(string DocName)
        {
            var StartUnits = Application.Preferences.RulerUnits;
            Application.Preferences.RulerUnits = PsUnits.psCM;
            Application.Documents.Add(
                ThSitePlanCommon.PsDocOpenPropertity["DocWidth"],
                ThSitePlanCommon.PsDocOpenPropertity["DocHight"],
                ThSitePlanCommon.PsDocOpenPropertity["PPI"],
                DocName);
            Application.ActiveDocument.ArtLayers[1].IsBackgroundLayer = true;
            Application.Preferences.RulerUnits = StartUnits;
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

            fileName = (string)configItem.Properties["Name"] + ".pdf";
            fullPath = Path.Combine(path, fileName);
            //装载PDF获取图层名称
            Document NewOpenDoc = Application.Open(fullPath);
            string CurDocNa = NewOpenDoc.Name;
            ArtLayer newlayer = NewOpenDoc.ArtLayers[1];
            newlayer.Name = CurDocNa;
            string DocName = NewOpenDoc.Name;

            //设置图层的不透明度和填充颜色
            if (NewOpenDoc.Name.Contains("色块"))
            {
                newlayer.Opacity = Convert.ToDouble(configItem.Properties["Opacity"]);
                FillBySelectChannel(NewOpenDoc.Name, configItem);
            }
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
                        SerLaySet = SerLaySet.LayerSets.Add();
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
                OperateLayer.Move(EndLayerSet, PsElementPlacement.psPlaceInside);
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
        public void UpdateLayerInSet(ArtLayer OperateLayer, LayerSet EndLayerSet)
        {
            List<string> CurDoc_Sets = OperateLayer.Name.Split('-').ToList();
            OperateLayer.Name = CurDoc_Sets.Last();
            foreach (ArtLayer lyit in EndLayerSet.ArtLayers)
            {
                if (lyit.Name == OperateLayer.Name)
                {
                    lyit.Clear();
                    OperateLayer.Move(lyit, PsElementPlacement.psPlaceAfter);
                    lyit.Merge();
                }
            }
        }

        //导出并保存PSD文件
        public void ExportToFile(string path)
        {
            Application.ActiveDocument.SaveAs(path);
        }
    }
}