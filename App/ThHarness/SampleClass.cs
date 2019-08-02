using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MbUnit.Framework;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThHarness
{
    public class SampleClass
    {
        [Test]
        public static void ChangeCircleColors()
        {
            var document = AcadApp.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            using (var tr = database.TransactionManager.StartOpenCloseTransaction())
            {
                try
                {
                    var circleClass = RXObject.GetClass(typeof(Circle));
                    var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(database);
                    var modelSpace = (BlockTableRecord)tr.GetObject(modelSpaceId, OpenMode.ForRead);

                    foreach (ObjectId id in modelSpace)
                    {
                        if (!id.ObjectClass.IsDerivedFrom(circleClass))
                            continue;

                        var circle = (Circle)tr.GetObject(id, OpenMode.ForWrite);
                        circle.ColorIndex = circle.Radius < 1.0 ? 2
                                          : circle.Radius > 10.0 ? 1
                                          : 3;
                    }
                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    tr.Abort();
                    document.Editor.WriteMessage(ex.Message);
                }
            }
        }
    }
}
