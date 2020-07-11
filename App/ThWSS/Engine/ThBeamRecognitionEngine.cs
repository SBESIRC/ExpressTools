using System;
using ThWSS.Model;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public class ThBeamRecognitionEngine : ThModeltRecognitionEngine
    {
        public override List<ThModelElement> Elements { get; set; }

        public override bool Acquire(Database database, Polyline floor, ObjectId frame)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(Database database, Polyline floor, ObjectIdCollection frames)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(Database database, Polyline floor, DBObjectCollection frames)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }
    }
}
