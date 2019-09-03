using System;
using Autodesk.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.ThCui
{
    public class RibbonCommandHandler : System.Windows.Input.ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        // https://blogs.msdn.microsoft.com/trevor/2008/08/14/c-warning-cs0067-the-event-event-is-never-used/
        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            if (parameter is RibbonCommandItem ribbonItem)
            {
                if (ribbonItem.CommandParameter == null)
                {
                    return;
                }

                Document dwg = AcadApp.DocumentManager.MdiActiveDocument;
                string cmdString = (ribbonItem.CommandParameter as string).TrimEnd();
                if (!cmdString.EndsWith(";"))
                {
                    cmdString = cmdString + " ";
                }

                dwg.SendStringToExecute("\x03\x03" + cmdString, true, false, true);
            }
        }
    }
}
