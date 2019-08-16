using AcHelper;
using ThAreaFrameConfig.View;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Presenter
{
    public class ThResidentialBuildingPresenter : IThAreaFramePresenter, IResidentialBuildingPresenterCallback
    {
        private readonly IResidentialBuildingView roomView;

        public ThResidentialBuildingPresenter(IResidentialBuildingView view)
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
            this.PickBuildingAreaFrames(name);
        }

        public void OnRenameAreaFrameLayer(string name, string newName)
        {
            using (Active.Document.LockDocument())
            {
                ThResidentialRoomDbUtil.RenameLayer(name, newName);
            }
        }
    }
}
