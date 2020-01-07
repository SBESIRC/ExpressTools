using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using ThStructure.Model;

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
