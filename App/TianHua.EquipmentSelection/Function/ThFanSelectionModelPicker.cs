﻿using System;
using System.Linq;
using GeoAPI.Geometries;
using System.Collections.Generic;
using ThCADCore.NTS;

namespace TianHua.FanSelection.Function
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
        /// <summary>
        ///  是否找到
        /// </summary>
        /// <returns></returns>
        bool IsFound();
    }

    public class ThFanSelectionModelPicker : IFanSelectionModelPicker
    { 
        private IPoint Point { get; set; }
        private Dictionary<IGeometry, IPoint> Models { get; set; }
        public ThFanSelectionModelPicker(List<FanParameters> models,  FanDataModel fanmodel, List<double> point)
        {
            //若是后倾离心风机（目前后倾离心只有单速）
            IEqualityComparer<FanParameters> comparer = null;
            if (ThFanSelectionUtils.IsHTFCBackwardModelStyle(fanmodel.VentStyle))
            {
                comparer = new CCCFRpmComparer();
            }
            else
            {
                comparer = new CCCFComparer();
            }

            var coordinate = new Coordinate(
                     ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point[0]),
                     ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point[1])
                );
            Point = ThCADCoreNTSService.Instance.GeometryFactory.CreatePoint(coordinate);
            Models = models.ToGeometries(comparer).ModelPick(Point);
        }

        public string Model()
        {
            if (Models.Count > 0)
            { 
                string moelkeyname = Models.First().Key.UserData as string;
                return moelkeyname.Split('@')[0];
            }
            else
            {
                return string.Empty;
            }
        }

        public double Pa()
        {
            if (Models.Count > 0)
            {
                return Models.First().Value.Y;
            }
            else
            {
                return 0;
            }
        }

        public double AirVolume()
        {
            if (Models.Count > 0)
            {
                return Models.First().Value.X;
            }
            else
            {
                return 0;
            }
        }

        public bool IsOptimalModel()
        {
            return Models.First().Key.IsOptimalModel(Point);
        }

        public bool IsFound()
        {
            return Models.Count > 0;
        }
    }

    public class ThFanSelectionAxialModelPicker : IFanSelectionModelPicker
    {
        private IPoint Point { get; set; }
        private Dictionary<IGeometry, IPoint> Models { get; set; }
        public ThFanSelectionAxialModelPicker(List<AxialFanParameters> models, List<double> point)
        {
            IEqualityComparer<AxialFanParameters> comparer = new AxialModelNumberComparer();
            var coordinate = new Coordinate(
                     ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point[0]),
                     ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(point[1])
                );
            Point = ThCADCoreNTSService.Instance.GeometryFactory.CreatePoint(coordinate);
            Models = models.ToGeometries(comparer).ModelPick(Point);
        }

        public string Model()
        {
            if (Models.Count > 0)
            {
                return Models.First().Key.UserData as string;
            }
            else
            {
                return string.Empty;
            }
        }

        public double Pa()
        {
            if (Models.Count > 0)
            {
                return Models.First().Value.Y;
            }
            else
            {
                return 0;
            }
        }

        public double AirVolume()
        {
            if (Models.Count > 0)
            {
                return Models.First().Value.X;
            }
            else
            {
                return 0;
            }
        }

        public bool IsOptimalModel()
        {
            return Models.First().Key.IsOptimalModel(Point);
        }

        public bool IsFound()
        {
            return Models.Count > 0;
        }

    }
}
