using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.View
{
    public interface IThAreaFrameView<TCallbacks>
    {
        void Attach(TCallbacks presenter);
    }
}
