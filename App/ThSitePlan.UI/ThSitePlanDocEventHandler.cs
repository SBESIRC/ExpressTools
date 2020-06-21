using AcHelper;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThSitePlan.UI
{
    public class ThSitePlanDocEventHandler
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanDocEventHandler instance = new ThSitePlanDocEventHandler();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanDocEventHandler() { }
        internal ThSitePlanDocEventHandler() { }
        public static ThSitePlanDocEventHandler Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public void SubscribeToDoc(Document doc)
        {
            doc.CommandEnded += new CommandEventHandler(DocEvent_CommandEnded);
            doc.CommandFailed += new CommandEventHandler(DocEvent_CommandFailed);
            doc.CommandCancelled += new CommandEventHandler(DocEvent_CommandCancelled);
            doc.CommandWillStart += new CommandEventHandler(DocEvent_CommandWillStart);
        }

        public void UnsubscribeFromDoc(Document doc)
        {
            doc.CommandEnded -= new CommandEventHandler(DocEvent_CommandEnded);
            doc.CommandFailed -= new CommandEventHandler(DocEvent_CommandFailed);
            doc.CommandCancelled -= new CommandEventHandler(DocEvent_CommandCancelled);
            doc.CommandWillStart -= new CommandEventHandler(DocEvent_CommandWillStart);
        }

        public void DocEvent_CommandFailed(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "THPGE" || e.GlobalCommandName == "THPUD")
            {
                ThSitePlanDbEventHandler.Instance.SubscribeToDb(Active.Database);
            }
        }

        public void DocEvent_CommandCancelled(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "THPGE" || e.GlobalCommandName == "THPUD")
            {
                ThSitePlanDbEventHandler.Instance.SubscribeToDb(Active.Database);
            }
        }

        public void DocEvent_CommandEnded(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "THPGE" || e.GlobalCommandName == "THPUD")
            {
                ThSitePlanDbEventHandler.Instance.SubscribeToDb(Active.Database);
            }
        }

        public void DocEvent_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "THPGE" || e.GlobalCommandName == "THPUD")
            {
                ThSitePlanDbEventHandler.Instance.UnsubscribeFromDb(Active.Database, false);
            }
        }
    }
}
