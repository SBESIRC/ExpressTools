﻿using System;
using System.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ThAnalytics.ThAnalyticsCommands))]
[assembly: ExtensionApplication(typeof(ThAnalytics.ThAnalyticsApp))]

namespace ThAnalytics
{
    public class ThAnalyticsApp : IExtensionApplication
    {
        // command name filters
        private readonly ArrayList filters = new ArrayList {
            "THDAS",
            "THDAE",
            "QUIT",
            "OPEN",
            "CLOSE",
            "SAVE",
            "NEW",
            "SAVEAS",
            "UNDO",
            "MREDO",
            "LINE",
            "CIRCLE",
            "PLINE",
            "POLYGON",
            "RECTANG",
            "ARC",
            "SPLINE",
            "ELLIPSE",
            "INSERT",
            "BLOCK",
            "HATCH",
            "TEXT",
            "MTEXT",
            "DDEDIT",
            "MTEDIT",
            "BEDIT",
            "DIST",
            "DIMLINEAR",
            "DIMALIGNED",
            "DIMARC",
            "DIMCONTINUE",
            "XREF",
            "REFEDIT",
            "LAYER",
            "-LAYER",
            "ERASE",
            "COPY",
            "COPYBASE",
            "MIRROR",
            "OFFSET",
            "MOVE",
            "ROTATE",
            "SCALE",
            "TRIM",
            "EXTEND",
            "BREAK",
            "FILLET",
            "EXPLODE",
            "PROPERTIES",
            "MATCHPROP",
            "REGEN",
            "ZOOM",
            "CUTCLIP",
            "AI_SELALL",
            "FIND"
};

        private readonly Hashtable commandhashtable = new Hashtable();

        public void Initialize()
        {
            ThCountlyServices.Instance.Initialize();
            AcadApp.Idle += new EventHandler(Application_OnIdle);
        }

        public void Terminate()
        {
            // unhook event handlers
            RemoveCommandHandler();

            //end the user session
            ThCountlyServices.Instance.EndSession();
        }

        private void Application_OnIdle(object sender, EventArgs e)
        {
            AcadApp.Idle -= new EventHandler(Application_OnIdle);
            
            // hook event handlers
            AddCommandHandler();

            //start the user session
            ThCountlyServices.Instance.StartSession();
        }

        private void AddCommandHandler()
        {
            AcadApp.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;

            foreach (Document d in AcadApp.DocumentManager)
            {
                SubscribeToDoc(d);
            }
        }

        private void RemoveCommandHandler()
        {
            AcadApp.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;

            foreach (Document d in AcadApp.DocumentManager)
            {
                UnsubscribeToDoc(d);
            }
        }

        private void SubscribeToDoc(Document d)
        {
            d.CommandWillStart += Document_CommandWillStart;
            d.CommandEnded += Document_CommandEnded;
            d.CommandCancelled += Document_CommandCancelled;
            d.CommandFailed += Documet_CommandFailed;
            d.UnknownCommand += Document_UnknownCommand;
        }

        private void UnsubscribeToDoc(Document d)
        {
            d.CommandWillStart -= Document_CommandWillStart;
            d.CommandEnded -= Document_CommandEnded;
            d.CommandCancelled -= Document_CommandCancelled;
            d.CommandFailed -= Documet_CommandFailed;
            d.UnknownCommand -= Document_UnknownCommand;
        }

        private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            SubscribeToDoc(e.Document);
        }

        private void Document_CommandWillStart(object sender, CommandEventArgs e)
        {
            int commamndstarttime = (int)(DateTime.Now.Ticks / 10000000);
            commandhashtable.Add(e.GlobalCommandName, commamndstarttime);
        }

        private void Document_CommandEnded(object sender, CommandEventArgs e)
        {
            if (filters.Contains(e.GlobalCommandName))
                return;

            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                int commamndstoptime = (int)(DateTime.Now.Ticks / 10000000);
                int runtime = commamndstoptime - (int)commandhashtable[e.GlobalCommandName];
                ThCountlyServices.Instance.RecordCommandEvent(e.GlobalCommandName, runtime);
                commandhashtable.Remove(e.GlobalCommandName);
            }
        }

        private void Document_CommandCancelled(object sender, CommandEventArgs e)
        {
            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                commandhashtable.Remove(e.GlobalCommandName);
            }
        }

        private void Documet_CommandFailed(object sender, CommandEventArgs e)
        {
            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                commandhashtable.Remove(e.GlobalCommandName);
            }
        }

        private void Document_UnknownCommand(object sender, UnknownCommandEventArgs e)
        {
        }

    }

    public class ThAnalyticsCommands
    {
        // command name filters
        private readonly ArrayList filters = new ArrayList { "THDAS", "THDAE" };

        private readonly Hashtable commandhashtable = new Hashtable();

        [CommandMethod("TIANHUACAD", "THDAS", CommandFlags.Modal)]
        public void ThAnalyticsStart()
        {
            // hook event handlers
            AddCommandHandler();

            //start the user session
            ThCountlyServices.Instance.StartSession();
        }

        [CommandMethod("TIANHUACAD", "THDAE", CommandFlags.Modal)]
        public void ThAnalyticsEnd()
        {
            // unhook event handlers
            RemoveCommandHandler();

            //end the user session
            ThCountlyServices.Instance.EndSession();
        }

        private void AddCommandHandler()
        {
            AcadApp.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;

            foreach (Document d in AcadApp.DocumentManager)
            {
                SubscribeToDoc(d);
            }
        }

        private void RemoveCommandHandler()
        {
            AcadApp.DocumentManager.DocumentCreated -= DocumentManager_DocumentCreated;

            foreach (Document d in AcadApp.DocumentManager)
            {
                UnsubscribeToDoc(d);
            }
        }

        private void SubscribeToDoc(Document d)
        {
            d.CommandWillStart += Document_CommandWillStart;
            d.CommandEnded += Document_CommandEnded;
            d.CommandCancelled += Document_CommandCancelled;
            d.CommandFailed += Documet_CommandFailed;
            d.UnknownCommand += Document_UnknownCommand;
        }

        private void UnsubscribeToDoc(Document d)
        {
            d.CommandWillStart -= Document_CommandWillStart;
            d.CommandEnded -= Document_CommandEnded;
            d.CommandCancelled -= Document_CommandCancelled;
            d.CommandFailed -= Documet_CommandFailed;
            d.UnknownCommand -= Document_UnknownCommand;
        }

        private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            SubscribeToDoc(e.Document);
        }

        private void Document_CommandWillStart(object sender, CommandEventArgs e)
        {
            int commamndstarttime = (int)(DateTime.Now.Ticks / 10000000);
            commandhashtable.Add(e.GlobalCommandName, commamndstarttime);
        }

        private void Document_CommandEnded(object sender, CommandEventArgs e)
        {
            if (filters.Contains(e.GlobalCommandName))
                return;

            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                int commamndstoptime = (int)(DateTime.Now.Ticks / 10000000);
                int runtime = commamndstoptime - (int)commandhashtable[e.GlobalCommandName];
                ThCountlyServices.Instance.RecordCommandEvent(e.GlobalCommandName, runtime);
                commandhashtable.Remove(e.GlobalCommandName);
            }
        }

        private void Document_CommandCancelled(object sender, CommandEventArgs e)
        {
            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                commandhashtable.Remove(e.GlobalCommandName);
            }
        }

        private void Documet_CommandFailed(object sender, CommandEventArgs e)
        {
            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                commandhashtable.Remove(e.GlobalCommandName);
            }
        }

        private void Document_UnknownCommand(object sender, UnknownCommandEventArgs e)
        {
        }
    }
}
