using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure
{
    public static class ThBeamDbExtension
    {
        public static DBObjectCollection BeamCurves(this BlockReference blockReference)
        {
            var objs = new DBObjectCollection();
            blockReference.Explode(objs);

            var curves = new DBObjectCollection();
            foreach (Entity obj in objs)
            {
                // 指定图层上的图元
                if (obj.Layer != ThBeamCommon.LAYER_BEAM)
                {
                    continue;
                }

                // 指定图元
                if (obj is Line line)
                {
                    curves.Add(line);
                }
                else if (obj is Polyline polyline)
                {
                    curves.Add(polyline);
                }
                else if (obj is Arc arc)
                {
                    curves.Add(arc);
                }
                else if (obj is BlockReference nestBlockReference)
                {
                    foreach(DBObject item in nestBlockReference.BeamCurves())
                    {
                        curves.Add(item);
                    }
                }
            }

            return curves;
        }
    }
}
