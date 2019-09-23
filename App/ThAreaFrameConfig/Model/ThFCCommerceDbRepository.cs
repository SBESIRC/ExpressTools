using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.Windows.Data;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig.Model
{
    public class ThFCCommerceDbRepository
    {
        private readonly Database database;
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
                var layers = new List<string>(); 
                foreach(var description in AcadApp.UIBindings.Collections.Layers)
                {
                    var properties = description.GetProperties();
                    layers.Add((string)properties["Name"].GetValue(description));

                }
                return layers;
            }
        }

        public ThFCCommerceDbRepository()
        {
            database = Active.Database;
            foreach (var description in AcadApp.UIBindings.Collections.Layers)
            {
                var properties = description.GetProperties();
                var name = (string)properties["Name"].GetValue(description);
            }
            ConstructFireCompartments();
        }

        public ThFCCommerceDbRepository(Database db)
        {
            database = db;
            ConstructFireCompartments();
        }

        public void AppendDefaultFireCompartment()
        {
            UInt16 index = (UInt16)(settings.Compartments.Count + 1);
            settings.Compartments.Add(new ThFireCompartment(settings.SubKey, settings.Storey, index));
        }

        public void ReloadFireCompartments()
        {
            settings.Compartments = database.CommerceFireCompartments();
            ThFireCompartmentDbHelper.NormalizeFireCompartments(settings.Compartments);
        }

        private void ConstructFireCompartments()
        {
            var compartments = database.CommerceFireCompartments();
            settings = new ThFCCommerceSettings()
            {
                Compartments = compartments,
                SubKey = 13,
                Storey = 1,
                Resistance = ThFCCommerceSettings.FireResistance.Level1,
                Layers = new Dictionary<string, string>()
                {
                    { "INNERFRAME", "AD-INDX"},
                    { "OUTERFRAME", "AD-AREA-DIVD" }
                },
                GenerateHatch = false,
            };
        }
    }
}
