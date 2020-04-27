﻿using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public class ThColumn : ThModelElement
    {
        public override Dictionary<string, object> Properties { get; set; }
    }

    public class ThColumnRecognitionEngine : ThModeltRecognitionEngine
    {
        public override List<ThModelElement> Elements { get; set; }

        public override bool Acquire(Database database, Polyline polygon)
        {
            throw new NotImplementedException();
        }
    }
}
