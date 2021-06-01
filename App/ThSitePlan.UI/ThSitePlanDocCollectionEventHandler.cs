using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Linq2Acad;
using ThSitePlan.Configuration;
using System;

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
        static string masterDocName = "测试图纸01.dwg";
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

                //SyncConfigFromMaster(e.Document);
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

        private void SyncConfigFromMaster(Document doc)
        {
            if (null == doc || null == doc.Database || doc.Name.Contains(masterDocName))
            {
                return;
            }

            try
            {
                Document masterDoc = null;
                foreach (Document docIter in AcadApp.DocumentManager)
                {
                    if (docIter.Name.Contains(masterDocName))
                    {
                        masterDoc = docIter;
                        break;
                    }
                }

                if (null != masterDoc)
                {
                    using (var docDb = AcadDatabase.Use(doc.Database))
                    {
                        var docConfig = ThSitePlanConfigService.Instance.GetConfigFromDb(docDb);
                        // No config, config is empty or not synced then sync from master.
                        //
                        if (null == docConfig || string.IsNullOrEmpty(docConfig.Item1) || docConfig.Item2 == false)
                        {
                            using (var masterDb = AcadDatabase.Use(masterDoc.Database))
                            {
                                var masterConfig = ThSitePlanConfigService.Instance.GetConfigFromDb(masterDb);
                                if (null != masterConfig && !string.IsNullOrEmpty(masterConfig.Item1))
                                {
                                    ThSitePlanConfigService.Instance.WriteConfigIntoDb(masterConfig.Item1, docDb, true);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                ThSitePlanApp.Logger.Error(e.Message);
            }
        }
    }
}
