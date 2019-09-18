using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThFCUnderGroundParkingDbRepository
    {
        private readonly Database database;
        private List<ThFireCompartment> compartments;
        public List<ThFireCompartment> Compartments
        {
            get
            {
                return compartments;
            }
        }

        public ThFCUnderGroundParkingDbRepository()
        {
            database = Active.Database;
            ConstructFireCompartments();
        }

        public ThFCUnderGroundParkingDbRepository(Database db)
        {
            database = db;
            ConstructFireCompartments();
        }

        private void ConstructFireCompartments()
        {
            compartments = database.UnderGroundParkingFireCompartments();
        }
    }
}
