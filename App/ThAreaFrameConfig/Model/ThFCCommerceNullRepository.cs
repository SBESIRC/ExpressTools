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

        public ThFCCommerceNullRepository()
        {
            settings = new ThFCCommerceSettings()
            {
                SubKey = 13,
                Storey = 1,
                Resistance = ThFCCommerceSettings.FireResistance.Level2,
                Layers = new Dictionary<string, string>()
                {
                    { "INNERFRAME", "AD-INDX"},
                    { "OUTERFRAME", "AD-AREA-DIVD" }
                },
                Compartments = new List<ThFireCompartment>(),
                GenerateHatch = false,
            };
        }

        public void AppendDefaultFireCompartment()
        {
            //
        }
    }
}
