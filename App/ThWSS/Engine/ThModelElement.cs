using System;
using System.Collections.Generic;

namespace ThWSS.Engine
{
    public abstract class ThModelElement
    {
        public abstract Dictionary<string, object> Properties { get; set; }
    }
}
