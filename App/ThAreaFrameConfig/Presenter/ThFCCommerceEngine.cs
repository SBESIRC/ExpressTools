using System;
using System.Linq;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public class ThFCCommerceEngine
    {
        private readonly ThFCCommerceSettings settings;

        // 构造函数
        public ThFCCommerceEngine(ThFCCommerceSettings theSettings)
        {
            settings = theSettings;
        }

        // 防火分区
        public List<ThFireCompartment> Compartments
        {
            get
            {
                return settings.Compartments.Where(o => o.IsDefined).ToList();
            }
        }

        // 子项编号
        public UInt16 Subkey
        {
            get
            {
                return settings.SubKey;
            }
        }

        // "百人疏散宽度（m/百人）"
        public double EvacuationDensity(ThFireCompartment compartment)
        {
                switch (settings.Resistance)
                {
                case ThFCCommerceSettings.FireResistance.Level1:
                case ThFCCommerceSettings.FireResistance.Level2:
                {
                    if (compartment.Storey >= 4)
                    {
                        return 1.00;
                    }
                    else if (compartment.Storey == 3)
                    {
                        return 0.75;
                    }
                    else if (compartment.Storey > 0)
                    {
                        return 0.65;
                    }
                    else
                    {
                        return 1.00;
                    }
                }
                case ThFCCommerceSettings.FireResistance.Level3:
                {
                    if (settings.Storey >= 4)
                    {
                        return 1.25;
                    }
                    else if (settings.Storey == 3)
                    {
                        return 1.00;
                    }
                    else if (compartment.Storey > 0)
                    {
                        return 0.75;
                    }
                    else
                    {
                        return 0.00;
                    }
                }
                case ThFCCommerceSettings.FireResistance.Level4:
                {
                    if (settings.Storey >= 4)
                    {
                        return 0.00;
                    }
                    else if (settings.Storey == 3)
                    {
                        return 0.00;
                    }
                    else if (compartment.Storey > 0)
                    {
                        return 1.00;
                    }
                    else
                    {
                        return 0.00;
                    }
                }
                default:
                    return 0.00;
                }
        }


        public double OccupantDensity(ThFireCompartment compartment)
        {
            if (compartment.Storey >= 4)
            {
                switch(settings.PfValue)
                {
                    case ThFCCommerceSettings.Density.Low:
                        return 0.30;
                    case ThFCCommerceSettings.Density.Middle:
                        return (0.30 + 0.42) / 2.0;
                    case ThFCCommerceSettings.Density.High:
                        return 0.42;
                    default:
                        return 0.00;
                }
            }
            else if (compartment.Storey == 3)
            {
                switch (settings.PfValue)
                {
                    case ThFCCommerceSettings.Density.Low:
                        return 0.39;
                    case ThFCCommerceSettings.Density.Middle:
                        return (0.39 + 0.54) / 2.0;
                    case ThFCCommerceSettings.Density.High:
                        return 0.54;
                    default:
                        return 0.00;
                }
            }
            else if (compartment.Storey > 0)
            {
                switch (settings.PfValue)
                {
                    case ThFCCommerceSettings.Density.Low:
                        return 0.43;
                    case ThFCCommerceSettings.Density.Middle:
                        return (0.43 + 0.60) / 2.0;
                    case ThFCCommerceSettings.Density.High:
                        return 0.60;
                    default:
                        return 0.00;
                }
            }
            else if (compartment.Storey == -1)
            {
                return 0.60;
            }
            else if (compartment.Storey == -2)
            {
                return 0.56;
            }
            else
            {
                return 0.0;
            }
        }
    }
}
