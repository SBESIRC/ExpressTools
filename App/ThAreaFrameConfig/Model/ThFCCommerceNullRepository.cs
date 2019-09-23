using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThFCCommerceNullRepository
    {
        private ThCommerceFireProofSettings settings;
        public ThCommerceFireProofSettings Settings
        {
            get
            {
                return settings;
            }
        }

        public ThFCCommerceNullRepository()
        {
            settings = new ThCommerceFireProofSettings()
            {
                SubKey = 13,
                Storey = 1,
                Resistance = ThCommerceFireProofSettings.FireResistance.Level2,
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
