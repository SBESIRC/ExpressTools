using AcHelper;
using ThAreaFrameConfig.View;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Presenter
{
    public class ThResidentialRoomPresenter : IThAreaFramePresenter, IThAreaFramePresenterCallback
    {
        private readonly IResidentialRoomView roomView;

        public ThResidentialRoomPresenter(IResidentialRoomView view)
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

        public void OnRemoveStorey(string[] names)
        {
            foreach(var name in names)
            {
                ThResidentialRoomDbUtil.RemoveLayer(name);
            }
        }
    }
}