using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThSitePlan.UI
{
    public class ThSitePlanDocCollectionEventHandler
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanDocCollectionEventHandler instance = new ThSitePlanDocCollectionEventHandler();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanDocCollectionEventHandler() { }
        internal ThSitePlanDocCollectionEventHandler() { }
        public static ThSitePlanDocCollectionEventHandler Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public void Register()
        {
            AcadApp.DocumentManager.DocumentBecameCurrent += DocCollEvent_DocumentBecameCurrent_Handler;
            AcadApp.DocumentManager.DocumentToBeDeactivated += DocCollEvent_DocumentToBeDeactivated_Handler;
        }

        public void UnRegister()
        {
            AcadApp.DocumentManager.DocumentBecameCurrent -= DocCollEvent_DocumentBecameCurrent_Handler;
            AcadApp.DocumentManager.DocumentToBeDeactivated -= DocCollEvent_DocumentToBeDeactivated_Handler;
        }

        public void DocCollEvent_DocumentBecameCurrent_Handler(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document != null)
            {
                ThSitePlanDocEventHandler.Instance.SubscribeToDoc(e.Document);
                ThSitePlanDbEventHandler.Instance.SubscribeToDb(e.Document.Database);
            }
        }

        public void DocCollEvent_DocumentToBeDeactivated_Handler(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document != null)
            {
                ThSitePlanDocEventHandler.Instance.UnsubscribeFromDoc(e.Document);
                ThSitePlanDbEventHandler.Instance.UnsubscribeFromDb(e.Document.Database);
            }
        }
    }
}
