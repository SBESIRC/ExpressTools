using System;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Copy
{
    public class ThHighlightOverride : IDisposable
    {
        private ObjectIdCollection Entities {get; set;}
        public ThHighlightOverride(ObjectIdCollection ents)
        {
            Entities = ents;
            Hightlight(true);
        }

        public void Dispose()
        {
            Hightlight(false);
        }

        private void Hightlight(bool isLight)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (ObjectId objId in Entities)
                {
                    var ent = acadDatabase.Element<Entity>(objId);
                    if (isLight)
                    {
                        ent.Highlight();
                    }
                    else
                    {
                        ent.Unhighlight();
                    }
                }
            }
        }
    }
}
