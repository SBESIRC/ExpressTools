using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThAreaFramePresenter
    {
        void Initialize();
        object UI { get; }
    }
}
