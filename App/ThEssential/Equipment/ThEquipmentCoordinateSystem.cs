using System;
using Autodesk.AutoCAD.Geometry;

namespace ThEssential.Equipment
{
    public class ThAnchorPoint
    {
        public double Flow { get; set; }
        public double Lift { get; set; }
        public Point3d Position { get; set; }
    }

    public class ThEquipmentCoordinateSystem
    {
        public ThAnchorPoint Xaxis { get; set; }
        public ThAnchorPoint Yaxis { get; set; }
        public ThAnchorPoint Origin { get; set; }

        public double ScaleX
        {
            get
            {
                return (Xaxis.Position - Origin.Position).Length / (Math.Log(Xaxis.Flow) - Math.Log(Origin.Flow));
            }
        }

        public double ScaleY
        {
            get
            {
                return (Yaxis.Position - Origin.Position).Length / (Math.Log(Yaxis.Lift) - Math.Log(Origin.Lift));
            }
        }

        public ThAnchorPoint Target(double flow, double lift)
        {
            return new ThAnchorPoint()
            {
                Flow = flow,
                Lift = lift,
                Position = new Point3d(
                    ScaleX * (Math.Log(flow) - Math.Log(Origin.Flow)) + Origin.Position.X,
                    ScaleY * (Math.Log(lift) - Math.Log(Origin.Lift)) + Origin.Position.Y,
                    0.0)
            };
        }
    }
}
