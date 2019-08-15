using AcHelper;
using ThAreaFrameConfig.View;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Presenter
{
    public class ThRoofGreenSpacePresenter : IThAreaFramePresenter, IRoofGreenSpacePresenterCallback
    {
        private readonly IRoofGreenSpaceView roomView;

        public ThRoofGreenSpacePresenter(IRoofGreenSpaceView view)
        {
            roomView = view;
        }

        public object UI => roomView;

        public void Initialize()
        {
            roomView.Attach(this);
        }

        public void OnPickAreaFrames(string name)
        {
            using (Active.Document.LockDocument())
            {
                ObjectId objectId = PickTool.PickEntity("请选择面积框线");
                if (objectId.IsNull)
                    return;

                // 复制面积框线
                ObjectId clonedObjId = ThEntTool.DeepClone(objectId);
                if (clonedObjId.IsNull)
                    return;

                // 图层管理
                //  1. 如果指定图层不存在，创建图层
                //  2. 如果指定图层存在，返回此图层
                ObjectId layerId = ThResidentialRoomDbUtil.ConfigLayer(name);
                if (layerId.IsNull)
                    return;

                // 将复制的放置在指定图层上
                ThResidentialRoomDbUtil.MoveToLayer(clonedObjId, layerId);
            }
        }
    }
}
