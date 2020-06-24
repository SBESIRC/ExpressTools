using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public class ThBeam : ThModelElement
    {
        public override Dictionary<string, object> Properties { get; set; }
    }

    public class ThBeamRecognitionEngine : ThModeltRecognitionEngine
    {
        public override List<ThModelElement> Elements { get; set; }

        public override bool Acquire(Database database, ObjectId polygon)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }
    }
}
