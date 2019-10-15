using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThXClip
{    
    public class EntPropertyInf
    {
        public string Layer { get; set; }
        public int ColorIndex { get; set;}
        public LineWeight Lw { get; set; }
    }
    public class PublicFunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acadEdition">如2008,2009,2010...</param>
        /// <returns></returns>
        public static bool IsGreaterThanOrEqualTo(int acadEdition)
        {
            bool res = false;
            Version version = Application.Version;
            int major = version.Major;
            if(version.Major > 19 || (version.Major==19 && version.Minor==2))
            {
                res = true;
            }
            return res;
        }
        public static void ChangeEntityProperty(Entity ent, EntPropertyInf entPropertyInf)
        {
            ent.Layer = entPropertyInf.Layer;
            ent.ColorIndex = entPropertyInf.ColorIndex;
            ent.LineWeight = entPropertyInf.Lw;
        }

        public static List<Entity> ExplodeToModelSpace(BlockReference br)
        {
            List<Entity> ents = new List<Entity>();
            DBObjectCollection dBObjectCollection = new DBObjectCollection();
            br.Explode(dBObjectCollection);
            foreach(DBObject dbObj in dBObjectCollection)
            {
                if(dbObj is BlockReference)
                {
                    BlockReference newBr = dbObj as BlockReference;
                    List<Entity> subEnts= ExplodeToModelSpace(newBr);
                    if(subEnts.Count>0)
                    {
                        ents.AddRange(subEnts);
                    }
                }
                else if(dbObj is Entity)
                {
                    Entity ent = dbObj as Entity;                  
                    ents.Add(ent);
                }
            }
            return ents;
        }
        public static List<Entity> ExplodeToModelSpace(BlockReference br,Matrix3d mt)
        {
            List<Entity> ents = new List<Entity>();
            DBObjectCollection dBObjectCollection = new DBObjectCollection();
            br.Explode(dBObjectCollection);
            foreach (DBObject dbObj in dBObjectCollection)
            {
                if (dbObj is BlockReference)
                {
                    BlockReference newBr = dbObj as BlockReference;
                    List<Entity> subEnts = ExplodeToModelSpace(newBr);
                    ents.AddRange(subEnts);
                }
                else if (dbObj is Entity)
                {
                    Entity ent = dbObj as Entity;
                    ent.TransformBy(mt);
                    ents.Add(ent);
                }
            }
            return ents;
        }
    }
}
