using System;
using Autodesk.Windows;
using Autodesk.AutoCAD.Ribbon;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.ThCui
{
    class ThRibbonHelper
    {
        Action<RibbonControl> action = null;
        bool idleHandled = false;
        bool created = false;

        ThRibbonHelper(Action<RibbonControl> action)
        {
            this.action = action;
            SetIdle(true);
        }

        /// <summary>
        /// 
        /// Pass a delegate that takes the RibbonControl
        /// as its only argument, and it will be invoked
        /// when the RibbonControl is available. 
        /// 
        /// If the RibbonControl exists when the constructor
        /// is called, the delegate will be invoked on the
        /// next idle event. Otherwise, the delegate will be
        /// invoked on the next idle event following the 
        /// creation of the RibbonControl.
        /// 
        /// </summary>
        public static void OnRibbonFound(Action<RibbonControl> action)
        {
            new ThRibbonHelper(action);
        }

        void SetIdle(bool value)
        {
            if (value ^ idleHandled)
            {
                if (value)
                    AcadApp.Idle += Application_OnIdle;
                else
                    AcadApp.Idle -= Application_OnIdle;
                idleHandled = value;
            }
        }

        void Application_OnIdle(object sender, EventArgs e)
        {
            SetIdle(false);
            if (action != null)
            {
                var ps = RibbonServices.RibbonPaletteSet;
                if (ps != null)
                {
                    var ribbon = ps.RibbonControl;
                    if (ribbon != null)
                    {
                        action(ribbon);
                        action = null;
                    }
                }
                else if (!created)
                {
                    created = true;
                    RibbonServices.RibbonPaletteSetCreated += RibbonPaletteSetCreated;
                }
            }
        }

        void RibbonPaletteSetCreated(object sender, EventArgs e)
        {
            RibbonServices.RibbonPaletteSetCreated -= RibbonPaletteSetCreated;
            SetIdle(true);
        }
    }
}
