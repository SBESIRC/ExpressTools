using AcHelper;
using ThAreaFrameConfig.View;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Presenter
{
    public class ThUnderGroundParkingPresenter : IThAreaFramePresenter, IUnderGroundParkingPresenterCallback
    {
        private readonly IUnderGroundParkingView roomView;

        public ThUnderGroundParkingPresenter(IUnderGroundParkingView view)
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
