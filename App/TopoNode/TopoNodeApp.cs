using Linq2Acad;
using DotNetARX;
using AcHelper;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using NFox.Cad.Collections;
using System.Threading;

namespace TopoNode
{
    public class TopoNodeApp
    {
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

        [CommandMethod("TopoNode", "TopoNode", CommandFlags.Modal)]
        static public void RoadDo()
        {

        }
    }
}
