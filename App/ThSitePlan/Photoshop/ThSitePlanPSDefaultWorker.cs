using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Photoshop;
using PsApplication = Photoshop.Application;
using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    /// <summary>
    /// 
    /// </summary>
    public class ThSitePlanPSDefaultWorker : ThSitePlanPSWorker
    {
        private ThSitePlanPSService psService;
        public override PsApplication PsAppInstance
        {
            get
            {
                return psService.Application;
            }
        }

        public ThSitePlanPSDefaultWorker(ThSitePlanPSService service)
        {
            psService = service;
        }

        public override bool DoProcess(string path, ThSitePlanConfigItem configItem)
        {
            string fileName = (string)configItem.Properties["Name"] + ".pdf";
            string fullPath = Path.Combine(path, fileName);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            //装载PDF获取图层名称
            Document NewOpenDoc = PsAppInstance.Open(fullPath);
            string CurDocNa = NewOpenDoc.Name;
            ArtLayer newlayer = NewOpenDoc.ArtLayers[1];
            newlayer.Name = CurDocNa;
            string DocName = NewOpenDoc.Name;
            List<string> CurDoc_Sets = DocName.Split('-').ToList();     //依据当前打开的图纸名获取其各个图层分组名
            string LastLayerName = CurDoc_Sets.Last();
            string FirstLayerName = CurDoc_Sets.First();

            //设置图层的不透明度
            newlayer.Opacity = Convert.ToDouble(configItem.Properties["Opacity"]) ;
            if (NewOpenDoc.Name.Contains("色块"))
            {
                newlayer.Opacity = 100;
                FillBySelectChannel(NewOpenDoc.Name, configItem);
            }

            //图层分组
            Document FirstDoca11;
            LayerSet EndLayerSet = null;


            FirstDoca11 = PsAppInstance.Documents[1];              //获取PS初始化打开的空白文档

            //获取当前打开的文档及图层
            Document CurDoc = PsAppInstance.ActiveDocument;
            ArtLayer FirstLay_CurDoc = CurDoc.ArtLayers[1];
            FirstLay_CurDoc.Name = CurDocNa;

            //将当前打开的文档中的图层复制到第一个空白文档中，关闭当前文档
            FirstLay_CurDoc.Duplicate(FirstDoca11, PsElementPlacement.psPlaceAtEnd);
            CurDoc.Close(PsSaveOptions.psDoNotSaveChanges);
            PsAppInstance.ActiveDocument = FirstDoca11;

            EndLayerSet = SearchInsertLoc(CurDoc_Sets, EndLayerSet, FirstDoca11);

            var OperateLayer = FirstDoca11.ArtLayers[CurDocNa];
            OperateLayer.Name = LastLayerName;
            if (CurDoc_Sets.Count > 1)     //_CY04
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
                OperateLayer.Move(FirstDoca11,PsElementPlacement.psPlaceAtEnd);
            }
            return true;
        }

        //PS中载入Channel选区并填充
        private void FillBySelectChannel(string LayerNameToBeFill, ThSitePlanConfigItem configItem)
        {
            var document = PsAppInstance.ActiveDocument;
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

        //在指定PS文档中检索指定图层，找到插入位置
        private LayerSet SearchInsertLoc(List<string> DocNameSpt, LayerSet SerLaySet, Document serdoc)
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

    }
}
