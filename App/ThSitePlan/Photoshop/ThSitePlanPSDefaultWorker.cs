using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSDefaultWorker : ThSitePlanPSWorker
    {
        private ThSitePlanPSService psService;

        public ThSitePlanPSDefaultWorker(ThSitePlanPSService service)
        {
            psService = service;
        }

        public override bool DoProcess(string path, ThSitePlanConfigItem configItem)
        {
            //DoProcess用于新生成PSD和部分configupdate更新操作
            //configupdate更新操作是基于当前图纸操作，不会新开一张空白图纸为背景图
            //此时CurrentFirstDocument为空
            if (psService.CurrentFirstDocument == null)
            {
                //configupdate时，若PS未打开图纸，此时返回
                if (psService.Application.Documents.Count == 0)
                {
                    return false;
                }
                psService.CurrentFirstDocument = psService.Application.ActiveDocument;
            }
            //1. 在PS中打开并处理PDF
            var NewOpenDocument = psService.OpenAndSet(path, configItem);
            if (NewOpenDocument == null)
            {
                return false;
            }
            //1.5 依据当前打开的图纸名获取其各个图层分组名(第一步打开文档后,第二步关闭文档前)
            string DocName = NewOpenDocument.Name;

            //2. 将新打开的PS文档图层复制到首文档
            psService.CopyNewToFirst(NewOpenDocument, psService.CurrentFirstDocument);

            //3. 依据当前文档名查找其在PS中的插入位置
            var EndLayerSet = psService.SearchInsertLoc(DocName, psService.CurrentFirstDocument);

            if (EndLayerSet == null)
            {
                return false;
            }

            //4. 将之前新插入的图层移动到指定的插入位置
            var OperateLayer = psService.CurrentFirstDocument.ArtLayers[DocName];
            psService.MoveLayerIntoSet(OperateLayer,EndLayerSet);

            return true;
        }

        public override bool DoUpdate(string path, ThSitePlanConfigItem configItem)
        {
            //1. 在PS中打开并处理需要更新的PDF
            if (psService.Application.Documents.Count == 0)
            {
                return false;
            }
            psService.CurrentFirstDocument = psService.Application.ActiveDocument;
            if (psService.CurrentFirstDocument == null)
            {
                return false;
            }

            var NewOpenDoc = psService.OpenAndSet(path, configItem);
            if (NewOpenDoc == null)
            {
                return false;
            }

            //1.5 依据当前打开的图纸名获取其各个图层分组名(第一步打开文档后,第二步关闭文档前)
            string NewDocName = NewOpenDoc.Name;

            //2. 在首文档最外层layers中按名称查找需要更新层，若找到直接替换返回
            bool findornot = psService.UpdateLayersInOutSet(NewOpenDoc, psService.CurrentFirstDocument);
            if (findornot == true)
            {
                return true;
            }

            //3. 将新打开的PS文档图层复制到首文档
            psService.CopyNewToFirst(NewOpenDoc, psService.CurrentFirstDocument);

            //4. 依据当前文档名查找其在PS中的插入位置
            var findlayer = psService.FindUpdateLocation(NewDocName, psService.CurrentFirstDocument);

            if (findlayer == null)
            {
                psService.CurrentFirstDocument.ArtLayers[NewDocName].Delete();
                return false;
            }

            //5. 在插入位置下找到该文档并替换
            var OperateLayer = psService.CurrentFirstDocument.ArtLayers[NewDocName];
            psService.UpdateLayerInSet(OperateLayer, findlayer);

            return true;
        }

        public bool CleanInPS(ThSitePlanConfigItem configItem)
        {
            //执行configupdate时，若PS未打开图纸，直接返回
            if (psService.Application.Documents.Count == 0)
            {
                return false;
            }
            string itemname = configItem.Properties["Name"].ToString();
            // 依据当前文档名查找其在PS中的插入位置
            var findlayer = psService.FindLayerByName(itemname);

            if (findlayer == null)
            {
                return false;
            }

            findlayer.Delete();
            return true;

        }
    }
}
