using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThFCCommerceDbRepository
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


        public ThFCCommerceDbRepository()
        {
            database = Active.Database;
            ConstructFireCompartments();
        }

        public ThFCCommerceDbRepository(Database db)
        {
            database = db;
            ConstructFireCompartments();
        }

        private void ConstructFireCompartments()
        {
            compartments = database.CommerceFireCompartments();
        }
    }
}
