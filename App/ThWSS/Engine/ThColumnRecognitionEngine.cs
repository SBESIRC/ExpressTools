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

        public override bool Acquire(Database database, ObjectId polygon)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(Database database, ObjectIdCollection frames)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(Database database, DBObjectCollection frames)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }
    }
}
