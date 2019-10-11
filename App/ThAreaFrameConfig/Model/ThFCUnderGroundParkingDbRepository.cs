using System;
using System.Linq;
using System.Collections.Generic;
using AcHelper;
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
            // 选取最后一个防火分区作为“锚”，新的防火分区为其“下一个”。
            var compartment = settings.Compartments.LastOrDefault();
            if (compartment != null)
            {
                // 计算编号索引，即所有防火分区（地下车库）的下一个
                var compartments = Settings.Compartments.Where(
                    o => o.IsDefined &&
                    o.Subkey == compartment.Subkey);
                // 由于编号索引是连续的，下一个索引即为个数+1
                UInt16 index = (UInt16)(compartments.Count() + 1);
                settings.Compartments.Add(new ThFireCompartment(0, compartment.Storey, index)
                {
                    Number = settings.Compartments.Count + 1
                });
            }
            else
            {
                // 第一个防火分区（地下车库）
                settings.Compartments.Add(new ThFireCompartment(0, -1, 1)
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
            settings.Compartments = compartments.Where(o => o.Type == ThFireCompartment.FCType.FCUnderGroundParking).ToList();
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
            var compartments = database.LoadCommerceFireCompartments(layer, islandLayer);
            settings.Compartments = compartments.Where(o => o.Type == ThFireCompartment.FCType.FCUnderGroundParking).ToList();
        }
    }
}
