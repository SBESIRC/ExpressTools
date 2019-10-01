using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.Windows.Data;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig.Model
{
    public class ThFCUnderGroundParkingDbRepository
    {
        private readonly Database database;
        private ThFCUnderGroundParkingSettings settings;
        public ThFCUnderGroundParkingSettings Settings
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
                foreach (var description in AcadApp.UIBindings.Collections.Layers)
                {
                    var properties = description.GetProperties();
                    layers.Add((string)properties["Name"].GetValue(description));

                }
                return layers;
            }
        }

        public ThFCUnderGroundParkingDbRepository()
        {
            database = Active.Database;
            foreach (var description in AcadApp.UIBindings.Collections.Layers)
            {
                var properties = description.GetProperties();
                var name = (string)properties["Name"].GetValue(description);
            }
            ConstructFireCompartments();
        }

        public ThFCUnderGroundParkingDbRepository(Database db)
        {
            database = db;
            ConstructFireCompartments();
        }

        public void AppendDefaultFireCompartment()
        {
            settings.Compartments.Add(new ThFireCompartment(0, 0, 0)
            {
                Number = settings.Compartments.Count + 1
            });
        }

        public void ReloadFireCompartments()
        {
            string layer = settings.Layers["OUTERFRAME"];
            string islandLayer = settings.Layers["INNERFRAME"];
            settings.Compartments = database.LoadCommerceFireCompartments(layer, islandLayer);
            ThFireCompartmentDbHelper.NormalizeFireCompartments(settings.Compartments);
            for (int i = 0; i < settings.Compartments.Count; i++)
            {
                settings.Compartments[i].Number = i + 1;
            }
        }

        private void ConstructFireCompartments()
        {
            settings = new ThFCUnderGroundParkingSettings()
            {
                Layers = new Dictionary<string, string>()
                {
                    { "INNERFRAME", "0"},
                    { "OUTERFRAME", "0" }
                }
            };

            if (Layers.Contains("AD-AREA-DIVD"))
            {
                settings.Layers["OUTERFRAME"] = "AD-AREA-DIVD";
            }
            if (Layers.Contains("AD-INDX"))
            {
                settings.Layers["INNERFRAME"] = "AD-INDX";
            }

            string layer = settings.Layers["OUTERFRAME"];
            string islandLayer = settings.Layers["INNERFRAME"];
            settings.Compartments = database.LoadCommerceFireCompartments(layer, islandLayer);
        }
    }
}
