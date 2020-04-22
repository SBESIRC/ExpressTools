using System;
using PsApplication = Photoshop.Application;
using Photoshop;
using System.Drawing;
using ThSitePlan.Configuration;
using System.IO;
using System.Collections.Generic;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSService : IDisposable
    {
        public PsApplication Application { get; set; }

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
            Color FillColor = (Color)configItem.Properties["Color"];
            var RGB_Red = FillColor.R;
            var RGB_Green = FillColor.G;
            var RGB_Blue = FillColor.B;

            SolidColor ColorInPS = new SolidColor();
            ColorInPS.RGB.Red = (double)RGB_Red;
            ColorInPS.RGB.Green = (double)RGB_Green;
            ColorInPS.RGB.Blue = (double)RGB_Blue;
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

            //设置图层的不透明度
            newlayer.Opacity = Convert.ToDouble(configItem.Properties["Opacity"]);
            if (NewOpenDoc.Name.Contains("色块"))
            {
                newlayer.Opacity = 100;
                FillBySelectChannel(NewOpenDoc.Name, configItem);
            }
            return NewOpenDoc;

        }

        //在指定PS文档中检索指定图层，找到插入位置
        public LayerSet SearchInsertLoc(List<string> DocNameSpt, LayerSet SerLaySet, Document serdoc)
        {
            LayerSets FirstLayerSets = serdoc.LayerSets;

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

        //导出并保存PSD文件
        public void ExportToFile(string path)
        {
            Application.ActiveDocument.SaveAs(path);
        }
    }
}