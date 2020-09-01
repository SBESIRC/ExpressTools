using System;
using GeoAPI.Geometries;
using System.Collections.Generic;

namespace TianHua.FanSelection.UI
{
    public static class FanParametersExtension
    {
        public static List<IGeometry> ToGeometries(this List<FanParameters> models)
        {
            throw new NotSupportedException();
        }

        public static List<IGeometry> ToGeometries(List<AxialFanParameters> models)
        {
            throw new NotSupportedException();
        }
    }
}
