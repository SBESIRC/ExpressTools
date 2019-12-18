using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThAreaFrameConfig.View
{
    public interface IAreaFrameDocumentCollectionReactor
    {
        void RegisterDocumentLockModeChangedEvent();

        void UnRegisterDocumentLockModeChangedEvent();
    }
}
