using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThFCCommerceNullRepository
    {
        private ThFCCommerceSettings settings;
        public ThFCCommerceSettings Settings
        {
            get
            {
                return settings;
            }
        }

        public List<string> Layers
        {
            get
            {
                return new List<string>()
                {
                    "0"
                };
            }
        }

        public ThFCCommerceNullRepository()
        {
            settings = new ThFCCommerceSettings()
            {
                SubKey = 13,
                Storey = 0,
                Resistance = ThFCCommerceSettings.FireResistance.Level2,
                Layers = new Dictionary<string, string>()
                {
                    { "INNERFRAME", "AD-INDX"},
                    { "OUTERFRAME", "AD-AREA-DIVD" }
                },
                Compartments = new List<ThFireCompartment>()
            };
        }

        public void AppendDefaultFireCompartment()
        {
            settings.Compartments.Add(new ThFireCompartment(settings.SubKey, 0, 0)
            {
                Number = settings.Compartments.Count + 1
            });
        }
    }
}
