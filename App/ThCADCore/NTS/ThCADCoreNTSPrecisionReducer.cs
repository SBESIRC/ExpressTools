using System;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSPrecisionReducer : IDisposable
    {
        public ThCADCoreNTSPrecisionReducer()
        {
            ThCADCoreNTSService.Instance.PrecisionReduce = true;
        }

        public void Dispose()
        {
            ThCADCoreNTSService.Instance.PrecisionReduce = false;
        }
    }
}
