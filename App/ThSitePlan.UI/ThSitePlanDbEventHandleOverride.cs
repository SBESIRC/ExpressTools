using System;
using AcHelper;

namespace ThSitePlan.UI
{
    public class ThSitePlanDbEventHandleOverride : IDisposable
    {
        public ThSitePlanDbEventHandleOverride()
        {
            ThSitePlanDbEventHandler.Instance.UnsubscribeFromDb(Active.Database);
        }


        public void Dispose()
        {
            ThSitePlanDbEventHandler.Instance.SubscribeToDb(Active.Database);
        }
    }
}
