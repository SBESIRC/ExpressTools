using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThFireProofDbRepository
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


        public ThFireProofDbRepository()
        {
            database = Active.Database;
            ConstructFireCompartments();
        }

        public ThFireProofDbRepository(Database db)
        {
            database = db;
            ConstructFireCompartments();
        }

        private void ConstructFireCompartments()
        {
            compartments = database.FireCompartments();
        }
    }
}
