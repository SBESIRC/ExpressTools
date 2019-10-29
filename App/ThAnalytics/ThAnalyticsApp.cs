using System;
using System.Diagnostics;
using System.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Text.RegularExpressions;

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
            "FIND",
            "UCS",
            "SETVAR",
            "U",
            "QSAVE",
            "LAYOUT",
            "UNDEFINE",
            "GRIP_STRETCH",
            "-PURGE",
            "COPYCLIP",
            "LAYOUT_CONTROL",
            "PASTECLIP",
            "STRETCH",
            "PLOT",
            "EXTERNALREFERENCES",
            "AUDIT",
            "PURGE",
            "OPTIONS",
            "DRAWORDER",
            "-XREF",
            "COLOR",
            "LINETYPE",
            "REFCLOSE",
            "MSPACE",
            "PSPACE",
            "GRIP_POPUP",
            "COMMANDLINE",
            "LAYOFF",
            "XOPEN",
            "DIMSTYLE",
            "HELP",
            "LAYON",
            "SELECT",
            "XATTACH",
            "NETLOAD"
        };

        private readonly Hashtable commandhashtable = new Hashtable();

        public void Initialize()
        {
            ThCybrosService.Instance.Initialize();
            ThCountlyServices.Instance.Initialize();
            AcadApp.Idle += new EventHandler(Application_OnIdle);
        }

        public void Terminate()
        {
            // unhook event handlers
            RemoveCommandHandler();
            // unhook application event handlers
            RemoveAppEventHandler();

            //end the user session
            ThCybrosService.Instance.EndSession();
            ThCountlyServices.Instance.EndSession();
        }

        private void Application_OnIdle(object sender, EventArgs e)
        {
            AcadApp.Idle -= new EventHandler(Application_OnIdle);
            
            // hook event handlers
            AddCommandHandler();
            // hook event handlers
            AddAppEventHandler();

            //start the user session
            ThCybrosService.Instance.StartSession();
            ThCountlyServices.Instance.StartSession();
        }

        private void AddAppEventHandler()
        {
            AcadApp.SystemVariableChanged += AcadApp_SystemVariableChanged;
        }

        private void RemoveAppEventHandler()
        {
            AcadApp.SystemVariableChanged -= AcadApp_SystemVariableChanged;
        }

        private void AcadApp_SystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            ThCountlyServices.Instance.RecordSysVerEvent(e.Name, AcadApp.GetSystemVariable(e.Name).ToString());
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

            d.LispWillStart += Document_LispWillStart;
        }

        private void Document_LispWillStart(object sender, LispWillStartEventArgs e)
        {
            // 特殊情况（C:THZ0）
            if (Regex.Match(e.FirstLine, @"^\([cC]:THZ0\)$").Success)
            {
                ThCountlyServices.Instance.RecordTHCommandEvent(e.FirstLine.Substring(3, e.FirstLine.Length - 4), 0);
                return;
            }

            // 正常情况（C:THXXX）
            string pattern = @"^\([cC]:TH[A-Z]{3,}\)$";
            if (Regex.Match(e.FirstLine, pattern).Success)
            {
                ThCybrosService.Instance.RecordTHCommandEvent(e.FirstLine.Substring(3, e.FirstLine.Length - 4), 0);
                ThCountlyServices.Instance.RecordTHCommandEvent(e.FirstLine.Substring(3, e.FirstLine.Length - 4), 0);
            }
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
            commandhashtable.Add(e.GlobalCommandName, Stopwatch.StartNew());
        }

        private void Document_CommandEnded(object sender, CommandEventArgs e)
        {
            if (filters.Contains(e.GlobalCommandName))
                return;

            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                Stopwatch sw = (Stopwatch)commandhashtable[e.GlobalCommandName];
                ThCybrosService.Instance.RecordCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
                ThCybrosService.Instance.RecordTHCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
                ThCountlyServices.Instance.RecordCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
                ThCountlyServices.Instance.RecordTHCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
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
            "FIND",
            "UCS",
            "SETVAR",
            "U",
            "QSAVE",
            "LAYOUT",
            "UNDEFINE",
            "GRIP_STRETCH",
            "-PURGE",
            "COPYCLIP",
            "LAYOUT_CONTROL",
            "PASTECLIP",
            "STRETCH",
            "PLOT",
            "EXTERNALREFERENCES",
            "AUDIT",
            "PURGE",
            "OPTIONS",
            "DRAWORDER",
            "-XREF",
            "COLOR",
            "LINETYPE",
            "REFCLOSE",
            "MSPACE",
            "PSPACE",
            "GRIP_POPUP",
            "COMMANDLINE",
            "LAYOFF",
            "XOPEN",
            "DIMSTYLE",
            "HELP",
            "LAYON",
            "SELECT",
            "XATTACH"
        };

        private readonly Hashtable commandhashtable = new Hashtable();

        [CommandMethod("TIANHUACAD", "THDAS", CommandFlags.Modal)]
        public void ThAnalyticsStart()
        {
            ThCybrosService.Instance.Initialize();
            ThCountlyServices.Instance.Initialize();
            // hook event handlers
            AddCommandHandler();
            // hook event handlers
            AddAppEventHandler();

            //start the user session
            ThCybrosService.Instance.StartSession();
            ThCountlyServices.Instance.StartSession();
        }

        [CommandMethod("TIANHUACAD", "THDAE", CommandFlags.Modal)]
        public void ThAnalyticsEnd()
        {
            // unhook event handlers
            RemoveCommandHandler();
            // unhook application event handlers
            RemoveAppEventHandler();

            //end the user session
            ThCybrosService.Instance.EndSession();
            ThCountlyServices.Instance.EndSession();
        }

        private void AddAppEventHandler()
        {
            AcadApp.SystemVariableChanged += AcadApp_SystemVariableChanged;
        }

        private void RemoveAppEventHandler()
        {
            AcadApp.SystemVariableChanged -= AcadApp_SystemVariableChanged;
        }

        private void AcadApp_SystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            ThCybrosService.Instance.RecordSysVerEvent(e.Name, AcadApp.GetSystemVariable(e.Name).ToString());
            ThCountlyServices.Instance.RecordSysVerEvent(e.Name, AcadApp.GetSystemVariable(e.Name).ToString());
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
            commandhashtable.Add(e.GlobalCommandName, Stopwatch.StartNew());
        }

        private void Document_CommandEnded(object sender, CommandEventArgs e)
        {
            if (filters.Contains(e.GlobalCommandName))
                return;

            if (commandhashtable.ContainsKey(e.GlobalCommandName))
            {
                Stopwatch sw = (Stopwatch)commandhashtable[e.GlobalCommandName];
                ThCybrosService.Instance.RecordCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
                ThCybrosService.Instance.RecordTHCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
                ThCountlyServices.Instance.RecordCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
                ThCountlyServices.Instance.RecordTHCommandEvent(e.GlobalCommandName, sw.Elapsed.TotalSeconds);
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
