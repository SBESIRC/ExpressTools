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
            settings.Compartments.Add(new ThFireCompartment(settings.SubKey, 0, 0)
            {
                Number = settings.Compartments.Count + 1
            });
        }

        public void ReloadFireCompartments()
        {
            string layer = settings.Layers["OUTERFRAME"];
            string islandLayer = settings.Layers["INNERFRAME"];
            settings.Compartments = database.CommerceFireCompartments(layer, islandLayer);
            ThFireCompartmentDbHelper.NormalizeFireCompartments(settings.Compartments);
        }

        private void ConstructFireCompartments()
        {
            settings = new ThFCCommerceSettings()
            {
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

            string layer = settings.Layers["OUTERFRAME"];
            string islandLayer = settings.Layers["INNERFRAME"];
            settings.Compartments = database.CommerceFireCompartments(layer, islandLayer);
        }
    }
}
