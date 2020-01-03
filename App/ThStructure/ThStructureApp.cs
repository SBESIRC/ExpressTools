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
                            { "Layer", "S_DETL_REIN" },
                            { "ConstantWidth", 19.0}
                        }
                    }
                },
                { "DetailReinPoint", new ThSComponentDbStyle("DetailReinPoint")
                    {
                        Values = new Dictionary<string, object>()
                        {
                            { "Layer", "S_DETL_REIN_POINT" },
                            { "ConstantWidth", 25.0}
                        }
                    }
                }
            };

            // 箍筋
            var rein = new ThSDetailRein();
            rein.Parameters.Add(new ThSComponentParameter("Len_X", 500.0));
            rein.Parameters.Add(new ThSComponentParameter("Len_Y", 500.0));
            rein.Parameters.Add(new ThSComponentParameter("intCBarCount", 4));
            rein.Parameters.Add(new ThSComponentParameter("intCBarDia", 25.0));
            rein.GenerateLayout();

            // 绘制
            var render = new ThSComponentDbRender();
            rein.Render(render);
        }
    }
}
