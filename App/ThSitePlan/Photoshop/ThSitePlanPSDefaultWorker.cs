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
            //装载PDF获取图层名称
            Document NewOpenDoc = psService.OpenAndSet(path, configItem);
            string CurDocNa = NewOpenDoc.Name;
            ArtLayer newlayer = NewOpenDoc.ArtLayers[1];
            newlayer.Name = CurDocNa;
            string DocName = NewOpenDoc.Name;
            List<string> CurDoc_Sets = DocName.Split('-').ToList();     //依据当前打开的图纸名获取其各个图层分组名
            string LastLayerName = CurDoc_Sets.Last();
            string FirstLayerName = CurDoc_Sets.First();

            //图层分组
            Document FirstDoca11;
            LayerSet EndLayerSet = null;

            FirstDoca11 = PsAppInstance.Documents[1];              //获取PS初始化打开的空白文档

            //获取当前打开的文档及图层
            Document CurDoc = PsAppInstance.ActiveDocument;
            ArtLayer FirstLay_CurDoc = CurDoc.ArtLayers[1];
            FirstLay_CurDoc.Name = CurDocNa;

            //将当前打开的文档中的图层复制到第一个空白文档中，关闭当前文档
            //在复制图层前先检查当前首文档中是否已存在该复制图层，若有先删除，这主要用于Update情形

            FirstLay_CurDoc.Duplicate(FirstDoca11, PsElementPlacement.psPlaceAtEnd);
            CurDoc.Close(PsSaveOptions.psDoNotSaveChanges);
            PsAppInstance.ActiveDocument = FirstDoca11;

            EndLayerSet = psService.SearchInsertLoc(CurDoc_Sets, EndLayerSet, FirstDoca11);

            var OperateLayer = FirstDoca11.ArtLayers[CurDocNa];
            OperateLayer.Name = LastLayerName;

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
                OperateLayer.Move(FirstDoca11,PsElementPlacement.psPlaceAtEnd);
            }
            return true;
        }

        public override bool DoUpdate(string path, ThSitePlanConfigItem configItem)
        {
            Document NewOpenDoc = psService.OpenAndSet(path, configItem);
            Document FirstDoca11 = PsAppInstance.Documents[1];

            string NewDocName = NewOpenDoc.Name;
            ArtLayer NewDocLayer = NewOpenDoc.ArtLayers[1];
            NewDocLayer.Name = NewOpenDoc.Name;
            foreach (ArtLayer lyit in FirstDoca11.ArtLayers)
            {
                if (lyit.Name == NewDocLayer.Name)
                {
                    PsAppInstance.ActiveDocument = FirstDoca11;
                    FirstDoca11.ArtLayers[lyit.Name].Clear();
                    PsAppInstance.ActiveDocument = NewOpenDoc;
                    NewDocLayer.Duplicate(FirstDoca11.ArtLayers[lyit.Name], PsElementPlacement.psPlaceAfter);
                    NewOpenDoc.Close(PsSaveOptions.psDoNotSaveChanges);
                    PsAppInstance.ActiveDocument = FirstDoca11;
                    FirstDoca11.ArtLayers[lyit.Name].Merge();
                    return true;
                }
            }
            NewDocLayer.Duplicate(FirstDoca11, PsElementPlacement.psPlaceAtEnd);
            NewOpenDoc.Close(PsSaveOptions.psDoNotSaveChanges);
            PsAppInstance.ActiveDocument = FirstDoca11;

            List<string> CurDoc_Sets = NewDocName.Split('-').ToList();     //依据当前打开的图纸名获取其各个图层分组名
            string LastLayerName = CurDoc_Sets.Last();
            string FirstLayerName = CurDoc_Sets.First();
            LayerSet EndLayerSet = null;
            EndLayerSet = psService.SearchInsertLoc(CurDoc_Sets, EndLayerSet, FirstDoca11);

            var OperateLayer = FirstDoca11.ArtLayers[NewDocName];
            OperateLayer.Name = LastLayerName;

            foreach (ArtLayer lyit in EndLayerSet.ArtLayers)
            {
                if (lyit.Name == OperateLayer.Name)
                {
                    lyit.Clear();
                    OperateLayer.Move(lyit, PsElementPlacement.psPlaceAfter);
                    lyit.Merge();
                    return true;
                }
            }

            return true;
        }

    }
}
