using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Presenter
{
    public interface IResidentialBuildingPresenterCallback : IThAreaFramePresenterCallback
    {
        void OnRenameAreaFrameLayer(string name, string newName);
    }
}
