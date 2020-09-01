using System;
using System.Linq;
using ThCADCore.NTS;
using GeoAPI.Geometries;
using System.Collections.Generic;
using NetTopologySuite.LinearReferencing;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionModelPick
    {
        public static bool IsOptimalModel(this IGeometry model, IPoint point)
        {
            return model.Envelope.Contains(point);
        }

        public static Dictionary<IGeometry, IPoint> ModelPick(this List<IGeometry> models, IPoint point)
        {
            var points = new List<IPoint>();
            foreach (var model in models)
            {
                // 先取信封
                var envelope = model.EnvelopeInternal;
                // 计算点和信封的交线
                if (point.X <= envelope.MaxX &&
                    point.X >= envelope.MinX &&
                    point.Y <= envelope.MaxY)
                {
                    var coordinates = new List<Coordinate>()
                    {
                        new Coordinate(
                            ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point.X),
                            ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(envelope.MinY)),
                        new Coordinate(
                            ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point.X),
                            ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(envelope.MaxY)),

                    };
                    // 计算模型线和探测线的交点
                    var intersectPoints = model.Intersection(ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(coordinates.ToArray()));
                    if (intersectPoints is IPoint pt)
                    {
                        points.Add(pt);
                    }
                    else if (intersectPoints is IMultiPoint pts)
                    {
                        foreach (IPoint po in pts.Geometries)
                        {
                            points.Add(po);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            // 寻找探测点上方最近的交点作为热点
            var pickedModel = new Dictionary<IGeometry, IPoint>();
            var filterPoints = points.Where(o => o.Y >= point.Y).OrderBy(o => o.Y);
            if (filterPoints.Any())
            {
                // 寻找穿过热点的模型线
                var hotspot = filterPoints.First();
                foreach (var model in models)
                {
                    if (model.Distance(hotspot) < 1E-10)
                    {
                        pickedModel.Add(model, model.GetClosestVertexTo(hotspot));
                    }
                }
            }
            return pickedModel;
        }

        private static IPoint GetClosestVertexTo(this IGeometry geometry, IPoint point)
        {
            var line = new LocationIndexedLine(geometry);
            var location = line.Project(point.Coordinate);
            var segment = location.GetSegment(geometry);
            if (segment.P0.X >= point.X)
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePoint(segment.P0);
            }
            else if (segment.P1.X >= point.X)
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreatePoint(segment.P1);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
