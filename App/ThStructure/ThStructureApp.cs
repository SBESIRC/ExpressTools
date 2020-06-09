using AcHelper;
using Linq2Acad;
using System.Linq;
using ThStructure.Model;
using NFox.Cad.Collections;
using System.Collections.Generic;
using ThStructure.BeamInfo.Command;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;


namespace ThStructure
{
    public class ThStructureApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THSTRUCTURE", CommandFlags.Modal)]
        public void ThStructure()
        {
            ThSComponentDbStyleManager.Instance.Styles = new Dictionary<string, ThSComponentDbStyle>()
            {
                { "DetailRein", new ThSComponentDbStyle("DetailRein")
                    {
                        Values = new Dictionary<string, object>()
                        {
                            { "ConstantWidth", 19.0},
                            { "Layer", "S_DETL_REIN" },
                        }
                    }
                },
                { "DetailReinPoint", new ThSComponentDbStyle("DetailReinPoint")
                    {
                        Values = new Dictionary<string, object>()
                        {
                            { "ConstantWidth", 25.0},
                            { "Layer", "S_DETL_REIN_POINT" },
                        }
                    }
                },
                {
                    "DetailSection", new ThSComponentDbStyle("DetailSection")
                    {
                        Values = new Dictionary<string, object>()
                        {
                            { "ConstantWidth", 0.0},
                            { "Layer", "S_WALL_REIN" },
                        }
                    }
                }
            };

            // 柱
            var column = new ThSDetailColumn();
            column.Parameters.Add(new ThSComponentParameter("Len_X", 500.0));
            column.Parameters.Add(new ThSComponentParameter("Len_Y", 500.0));
            column.Parameters.Add(new ThSComponentParameter("intCBarCount", 4));
            column.Parameters.Add(new ThSComponentParameter("intCBarDia", 25.0));
            column.GenerateLayout();

            // 绘制
            var render = new ThSComponentDbRender();
            var ipColumn = new ThSInplaceDetailColumn(column);
            ipColumn.Render(render);
        }
    }
}
