using AcHelper;
using Linq2Acad;
using ThStructure.Model;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using ThStructure.BeamInfo.Command;
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

        [CommandMethod("TIANHUACAD", "THDISBEAM", CommandFlags.Modal)]
        public void ThDistinguishBeam()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "ARC,LINE,LWPOLYLINE" & o.Dxf((int)DxfCode.LayerName) == "__覆盖_S20-平面_TEN25CUZ_设计区$0$S_BEAM");//"__覆盖_S20-平面_TEN25CUZ_设计区$0$S_BEAM"
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    dBObjects.Add(acdb.Element<Entity>(obj));
                }

                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                thDisBeamCommand.CalBeamStruc(dBObjects);
            }
        }

    }
}
