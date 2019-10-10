using System;
using System.Linq;
using System.Collections.Generic;
using AcHelper;
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
            // 选取最后一个防火分区作为“锚”，新的防火分区为其“下一个”。
            var compartment = Settings.Compartments.LastOrDefault();
            if (compartment != null)
            {
                // 计算编号索引，即同一楼层所有防火分区（商业）下一个
                var compartments = Settings.Compartments.Where(
                    o => o.IsDefined &&
                    o.Subkey == compartment.Subkey &&
                    o.Storey == compartment.Storey);
                // 由于编号索引是连续的，下一个索引即为个数+1
                UInt16 index = (UInt16)(compartments.Count() + 1);
                settings.Compartments.Add(new ThFireCompartment(compartment.Subkey, compartment.Storey, index)
                {
                    Number = settings.Compartments.Count + 1
                });
            }
            else
            {
                // 第一个防火分区
                settings.Compartments.Add(new ThFireCompartment(settings.SubKey, 1, 1)
                {
                    Number = settings.Compartments.Count + 1
                });
            }
        }

        public void ReloadFireCompartments()
        {
            string layer = settings.Layers["OUTERFRAME"];
            string islandLayer = settings.Layers["INNERFRAME"];
            var compartments = database.LoadCommerceFireCompartments(layer, islandLayer);
            settings.Compartments = compartments.Where(o => o.Type == ThFireCompartment.FCType.FCCommerce).ToList();
            ThFireCompartmentDbHelper.NormalizeFireCompartments(settings.Compartments);
            for(int i = 0; i < settings.Compartments.Count; i++)
            {
                settings.Compartments[i].Number = i + 1;
            }
        }

        private void ConstructFireCompartments()
        {
            settings = new ThFCCommerceSettings()
            {
                SubKey = 1,
                Storey = 1,
                Density = ThFCCommerceSettings.OccupantDensity.Middle,
                Resistance = ThFCCommerceSettings.FireResistance.Level1,
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
            var compartments = database.LoadCommerceFireCompartments(layer, islandLayer);
            settings.Compartments = compartments.Where(o => o.Type == ThFireCompartment.FCType.FCCommerce).ToList();
        }
    }
}
