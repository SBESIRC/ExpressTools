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
        private ThCommerceFireProofSettings settings;
        public ThCommerceFireProofSettings Settings
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
            //
        }

        private void ConstructFireCompartments()
        {
            var compartments = database.CommerceFireCompartments();
            settings = new ThCommerceFireProofSettings()
            {
                GenerateHatch = false,
                Compartments = compartments,
                Info = new ThCommerceFireProofSettings.BuildingInfo()
                {
                    subKey = 13,
                    AboveGroundStoreys = 1,
                    fireResistance = ThCommerceFireProofSettings.FireResistance.Level1
                },
                Layers = new Dictionary<string, string>()
                {
                    { "INNERFRAME", "AD-INDX"},
                    { "OUTERFRAME", "AD-AREA-DIVD" }
                },
            };
        }
    }
}
