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
            //1. 在PS中打开并处理PDF
            var NewOpenDocument = psService.OpenAndSet(path, configItem);

            //1.5 依据当前打开的图纸名获取其各个图层分组名(第一步打开文档后,第二步关闭文档前)
            string DocName = NewOpenDocument.Name;

            //2. 将新打开的PS文档图层复制到首文档
            psService.CopyNewToFirst(NewOpenDocument, psService.CurrentFirstDocument);

            //3. 依据当前文档名查找其在PS中的插入位置
            var EndLayerSet = psService.SearchInsertLoc(DocName, psService.CurrentFirstDocument);

            //4. 将之前新插入的图层移动到指定的插入位置
            var OperateLayer = psService.CurrentFirstDocument.ArtLayers[DocName];
            psService.MoveLayerIntoSet(OperateLayer,EndLayerSet);

            return true;
        }

        public override bool DoUpdate(string path, ThSitePlanConfigItem configItem)
        {
            //1. 在PS中打开并处理需要更新的PDF
            psService.CurrentFirstDocument = psService.Application.ActiveDocument;
            if (psService.CurrentFirstDocument == null)
            {
                return false;
            }

            var NewOpenDoc = psService.OpenAndSet(path, configItem);

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
            var EndLayerSet = psService.SearchInsertLoc(NewDocName, psService.CurrentFirstDocument);

            //5. 在插入位置下找到该文档并替换
            var OperateLayer = psService.CurrentFirstDocument.ArtLayers[NewDocName];
            psService.UpdateLayerInSet(OperateLayer, EndLayerSet);

            return true;
        }
    }
}
