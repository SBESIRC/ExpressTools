using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Align
{
    public static class ThAlignExtent3dExtension
    {
        public static Vector3d Displacement(this Extents3d extents, AlignMode mode, Point3d point)
        {
            switch(mode)
            {
                case AlignMode.XFont:
                    {
                        var offset = extents.MinPoint - point;
                        return -offset.DotProduct(Vector3d.YAxis) * Vector3d.YAxis;
                    }
                case AlignMode.XBack:
                    {
                        var offset = point - extents.MaxPoint;
                        return offset.DotProduct(Vector3d.YAxis) * Vector3d.YAxis;
                    }
                case AlignMode.YLeft:
                    {
                        var offset = extents.MinPoint - point;
                        return -offset.DotProduct(Vector3d.XAxis) * Vector3d.XAxis;
                    }
                case AlignMode.YRight:
                    {
                        var offset = point - extents.MaxPoint;
                        return offset.DotProduct(Vector3d.XAxis) * Vector3d.XAxis;
                    }
                case AlignMode.XCenter:
                    {
                        var center = extents.MinPoint + (extents.MaxPoint - extents.MinPoint).DivideBy(2.0);
                        return -(center - point).DotProduct(Vector3d.YAxis) * Vector3d.YAxis;
                    }
                case AlignMode.YCenter:
                    {
                        var center = extents.MinPoint + (extents.MaxPoint - extents.MinPoint).DivideBy(2.0);
                        return -(center - point).DotProduct(Vector3d.XAxis) * Vector3d.XAxis;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
