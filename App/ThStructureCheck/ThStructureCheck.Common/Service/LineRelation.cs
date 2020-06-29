using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Model;

namespace ThStructureCheck.Common.Service
{
    public class LineRelation
    {
        private Point3d firstLineSpt;
        private Point3d firstLineEpt;
        private Point3d secondLineSpt;
        private Point3d secondLineEpt;
        private List<Relationship> relationships = new List<Relationship>();
        public List<Relationship> Relationships => relationships;
        public LineRelation(Coordinate firstLineSpt, Coordinate firstLineEpt,
            Coordinate secondLineSpt, Coordinate secondLineEpt)
        {
            this.firstLineSpt = firstLineSpt.Coord;
            this.firstLineEpt = firstLineEpt.Coord;
            this.secondLineSpt = secondLineSpt.Coord;
            this.secondLineEpt = secondLineEpt.Coord;
        }
        public void Relation()
        {
            Vector3d firstVec = firstLineEpt - firstLineSpt;
            Vector3d secondVec = secondLineEpt - secondLineSpt;
            firstVec = firstVec.GetNormal();
            secondVec = secondVec.GetNormal();
            bool isParallel = firstVec.IsParallelTo(secondVec);
            if (isParallel)
            {
                relationships.Add(Relationship.Parallel);
                if (CadTool.IsCollinear(firstVec, secondVec))
                {
                    relationships.Add(Relationship.Collinear);
                }
            }
            else
            {
                bool isPerdicular = firstVec.IsPerpendicularTo(secondVec);
                if (isPerdicular)
                {
                    relationships.Add(Relationship.Perpendicular);
                }
                else
                {
                    relationships.Add(Relationship.UnRegular);
                }
            }
        }
    }
    public enum Relationship
    {
        UnRegular,
        Parallel,
        Perpendicular,
        Collinear
    }
}
