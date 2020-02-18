using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using NFox.Cad.Collections;

namespace ThEssential.Equipment
{
    public static class ThEquipmentEditorExtension
    {
        public static ObjectIdCollection Model(this Editor editor, ThAnchorPoint point)
        {
            // 根据锚点，获取设备区域
            using (var points = new Point3dCollection())
            using (var boundaries = editor.TraceBoundary(point.Position, false))
            {
                foreach (var dbObj in boundaries)
                {
                    if (dbObj is Region region)
                    {
                        using (var brep = new Brep(region))
                        {
                            foreach (var vertex in brep.Vertices)
                            {
                                points.Add(vertex.Point);
                            }
                        }
                    }
                }

                // 查找区域内的单行文字，其内容即为设备模型号
                var dbObjs = new ObjectIdCollection();
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "TEXT");
                var result = editor.SelectWindowPolygon(points, filterlist);
                if (result.Status == PromptStatus.OK)
                {
                    foreach(var obj in result.Value.GetObjectIds())
                    {
                        dbObjs.Add(obj);
                    }
                }
                return dbObjs;
            }
        }
    }
}