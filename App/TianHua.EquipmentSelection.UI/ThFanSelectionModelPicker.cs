using System;
using GeoAPI.Geometries;
using System.Collections.Generic;

namespace TianHua.FanSelection.UI
{
    public interface IFanSelectionModelPicker
    {
        /// <summary>
        /// CCCF规格
        /// </summary>
        /// <returns></returns>
        string Model();
        /// <summary>
        /// 全压
        /// </summary>
        /// <returns></returns>
        double Pa();
        /// <summary>
        /// 风量
        /// </summary>
        /// <returns></returns>
        double AirVolume();
        /// <summary>
        /// 是否为最优
        /// </summary>
        /// <returns></returns>
        bool IsOptimalModel();
    }

    public class ThFanSelectionModelPicker : IFanSelectionModelPicker
    { 
        private Dictionary<IGeometry, IPoint> Models { get; set; }
        public ThFanSelectionModelPicker(List<FanParameters> models, List<double> point)
        {
            throw new NotImplementedException();
        }

        public string Model()
        {
            throw new NotImplementedException();
        }

        public double Pa()
        {
            throw new NotImplementedException();
        }

        public double AirVolume()
        {
            throw new NotImplementedException();
        }

        public bool IsOptimalModel()
        {
            throw new NotImplementedException();
        }
    }

    public class ThFanSelectionAxialModelPicker : IFanSelectionModelPicker
    {
        public ThFanSelectionAxialModelPicker(List<AxialFanParameters> models, List<double> point)
        {
            //
        }

        public double AirVolume()
        {
            throw new NotImplementedException();
        }

        public bool IsOptimalModel()
        {
            throw new NotImplementedException();
        }

        public string Model()
        {
            throw new NotImplementedException();
        }

        public double Pa()
        {
            throw new NotImplementedException();
        }
    }
}
