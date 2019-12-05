using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThAreaFrameConfig.View
{
    public interface IAreaFrameDocumentReactor
    {
        void RegisterCommandWillStartEvent();
        void UnRegisterCommandWillStartEvent();

        void RegisterCommandEndedEvent();
        void UnRegisterCommandEndedEvent();

        void RegisterCommandFailedEvent();
        void UnRegisterCommandFailedEvent();

        void RegisterCommandCancelledEvent();
        void UnRegisterCommandCancelledEvent();
    }
}
