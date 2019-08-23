using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThPlotSpaceDbRepository : IThPlotSpaceRepository
    {
        private List<ThPlotSpace> spaces;
        private readonly Database database;

        public List<ThPlotSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThPlotSpaceDbRepository()
        {
            database = Active.Database;
            ConstructPlotSpaces();
        }

        public ThPlotSpaceDbRepository(Database db)
        {
            database = db;
            ConstructPlotSpaces();
        }

        public void AppendDefaultPlotSpace()
        {
            spaces.Add(new ThPlotSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
                HouseHold = 0
            });
        }

        private void ConstructPlotSpaces()
        {
            spaces = database.PlotSpaces();
        }
    }
}
