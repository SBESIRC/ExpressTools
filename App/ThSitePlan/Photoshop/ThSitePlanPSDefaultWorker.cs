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
        public override PsApplication PsAppInstance
        {
            get
            {
                return ThSitePlanPSService.Instance.Application;
            }
        }

        public override bool DoProcess(string path, ThSitePlanConfigItem configItem)
        {
            string fileName = (string)configItem.Properties["Name"] + ".pdf";
            string fullPath = Path.Combine(path, fileName);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            //装载PDF并处理
            Document NewOpenDoc = this.PsAppInstance.Open(fullPath);
            string CurDocNa = NewOpenDoc.Name;
            NewOpenDoc.ArtLayers[1].Name = CurDocNa;
            List<string> CurDoc_Sets = NewOpenDoc.Name.Split('-').ToList();     //依据当前打开的图纸名获取其各个图层分组名

            //设置图层的不透明度
            NewOpenDoc.ArtLayers[1].Opacity = Convert.ToDouble(configItem.Properties["Opacity"]) ;
            if (NewOpenDoc.Name.Contains("色块"))
            {
                NewOpenDoc.ArtLayers[1].Opacity = 100;
                this.FillBySelectChannel(NewOpenDoc.Name, configItem);
            }

            //图层分组
            Document FirstDoca11;
            LayerSet EndLayerSet = null;

            if (this.PsAppInstance.Documents.Count == 1)
            {
                FirstDoca11 = NewOpenDoc;

                //依据文件名获取各个分组名
                string FirstDoc_Name = FirstDoca11.Name;
                FirstDoca11.ArtLayers[1].Name = FirstDoc_Name;

                List<string> LayerSetsNaList = new List<string>();
                for (int i = 0; i < CurDoc_Sets.Count - 1; i++)
                {
                    LayerSetsNaList.Add(CurDoc_Sets[i]);

                    if (i == 0)
                    {
                        this.PsAppInstance.ActiveDocument.LayerSets.Add().Name = CurDoc_Sets[i];
                        EndLayerSet = this.PsAppInstance.ActiveDocument.LayerSets[CurDoc_Sets[i]];
                    }
                    else
                    {
                        EndLayerSet = EndLayerSet.LayerSets.Add();
                        EndLayerSet.Name = CurDoc_Sets[i];
                    }
                }

                FirstDoca11.ArtLayers.Add().IsBackgroundLayer = true;
            }

            else
            {
                FirstDoca11 = this.PsAppInstance.Documents[1];

                Document CurDoc = this.PsAppInstance.ActiveDocument;
                CurDoc.ArtLayers[1].Name = CurDocNa;

                this.PsAppInstance.ActiveDocument.ArtLayers[1].Duplicate(FirstDoca11, PsElementPlacement.psPlaceAtEnd);
                CurDoc.Close(PsSaveOptions.psDoNotSaveChanges);
                this.PsAppInstance.ActiveDocument = FirstDoca11;

                LayerSets FirstLayerSets = PsAppInstance.ActiveDocument.LayerSets;

                for (int i = 0; i < CurDoc_Sets.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        foreach (LayerSet LaysetInCurDOC in FirstLayerSets)
                        {
                            if (LaysetInCurDOC.Name == CurDoc_Sets[i])
                            {
                                EndLayerSet = LaysetInCurDOC;
                                EndLayerSet.Name = CurDoc_Sets[i];
                                break;
                            }
                        }

                        if (EndLayerSet == null)
                        {
                            EndLayerSet = FirstLayerSets.Add();
                            EndLayerSet.Name = CurDoc_Sets[i];
                            break;
                        }
                    }
                    else
                    {
                        bool FindOrNot = false;

                        foreach (LayerSet LaysetInCurDOC in EndLayerSet.LayerSets)
                        {
                            if (LaysetInCurDOC.Name == CurDoc_Sets[i])
                            {
                                EndLayerSet = LaysetInCurDOC;
                                EndLayerSet.Name = CurDoc_Sets[i];
                                FindOrNot = true;
                                break;
                            }
                        }

                        if (FindOrNot == false)
                        {
                            EndLayerSet = EndLayerSet.LayerSets.Add();
                            EndLayerSet.Name = CurDoc_Sets[i];
                            break;
                        }
                    }
                }
            }

            if (CurDoc_Sets.Count > 1)     //_CY04
            {
                int CurIndex = CurDoc_Sets.IndexOf(EndLayerSet.Name);

                //若当前图层指针指向的图层组名并不是当前待移动的图层的图层名中最内侧分组名
                if (CurIndex != CurDoc_Sets.Count - 2)
                {
                    for (int i = CurIndex + 1; i < CurDoc_Sets.Count - 1; i++)
                    {
                        EndLayerSet = EndLayerSet.LayerSets.Add();
                        EndLayerSet.Name = CurDoc_Sets[i];
                    }
                }

                FirstDoca11.ArtLayers[CurDocNa].Move(EndLayerSet, PsElementPlacement.psPlaceInside);
            }

            return true;
        }

        private void FillBySelectChannel(string LayerNameToBeFill, ThSitePlanConfigItem configItem)
        {
            var document = PsAppInstance.ActiveDocument;
            document.ActiveLayer = document.ArtLayers[LayerNameToBeFill];
            document.Selection.Load(document.Channels["绿"], PsSelectionType.psReplaceSelection, true);
            //document.Selection.Load(document.ComponentChannels, PsSelectionType.psReplaceSelection, true);
            //获取配置文件中传入的
            Color FillColor = (Color)configItem.Properties["Color"];
            var RGB_Red = FillColor.R;
            var RGB_Green = FillColor.G;
            var RGB_Blue = FillColor.B;

            PsAppInstance.ForegroundColor.RGB.Red = (double)RGB_Red;
            PsAppInstance.ForegroundColor.RGB.Green = (double)RGB_Green;
            PsAppInstance.ForegroundColor.RGB.Blue = (double)RGB_Blue;
            this.PsAppInstance.ActiveDocument.Selection.Fill(PsAppInstance.ForegroundColor);
            this.PsAppInstance.ActiveDocument.Selection.Deselect();
        }
    }
}
