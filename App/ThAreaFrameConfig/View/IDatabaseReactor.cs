using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.View
{
    public interface IAreaFrameDatabaseReactor
    {
        void RegisterAreaFrameModifiedEvent();
        void UnRegisterAreaFrameModifiedEvent();

        void RegisterAreaFrameErasedEvent();
        void UnRegisterAreaFrameErasedEvent();
    }
}
