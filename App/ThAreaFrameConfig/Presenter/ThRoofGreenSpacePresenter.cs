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
            this.PickAreaFrames(name);
        }
    }
}
