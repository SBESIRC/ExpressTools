using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThAOccupancyDbDepository
    {
        private readonly Database database;
        private List<ThAOccupancyStorey> storeys;

        // 构造函数
        public ThAOccupancyDbDepository()
        {
            database = Active.Database;
            ConstructRepository();
            ConstructAreaFrames();
        }

        public ThAOccupancyDbDepository(Database db)
        {
            database = db;
            ConstructRepository();
            ConstructAreaFrames();
        }

        public List<ThAOccupancyStorey> Storeys
        {
            get
            {
                return storeys;
            }
        }

        public void AppendStorey(string identifier)
        {
            //
        }

        private void ConstructRepository()
        {
            //
        }

        private void ConstructAreaFrames()
        {
            //
        }
    }
}
