using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.ThBeamInfo.Service
{
    public class BeamLinkExtension
    {
        private Tuple<BeamLink, List<Polyline>, List<Curve>> beamLink;
        public BeamLinkExtension(Tuple<BeamLink, List<Polyline>, List<Curve>> beamLink)
        {
            this.beamLink = beamLink;
        }
        public BeamLinkExtension()
        {
        }
        public List<Entity> DrawInfo()
        {
            List<Entity> entities = new List<Entity>();
            for (int i=0; i<this.beamLink.Item1.Beams.Count;i++)
            {
                Point3d beamSpt = this.beamLink.Item3[i].StartPoint;
                Point3d beamEpt = this.beamLink.Item3[i].EndPoint;
                double textRotation = TextRotateAngle(beamSpt, beamEpt);
                Point3d midPt = CadTool.GetMidPt(beamSpt, beamEpt);
                Vector3d perpendVec = GeometricCalculation.GetOffsetDirection(beamSpt, beamEpt);
                entities.Add(CreateText(this.beamLink.Item1.Beams[i].ID.ToString(), midPt, 0.0, perpendVec, textRotation));
            }
            return entities;
        }
        public List<Entity> DrawTexts(BeamCalculationIndex beamCalculationIndex, Curve curve)
        {
            List<Entity> texts = new List<Entity>();
            if(beamCalculationIndex == null || curve==null)
            {
                return texts;
            }
            Point3d beamSpt = curve.StartPoint;
            Point3d beamEpt = curve.EndPoint;
            double textRotation = TextRotateAngle(beamSpt, beamEpt);
            Point3d midPt = CadTool.GetMidPt(beamSpt, beamEpt);
            Vector3d perpendVec = GeometricCalculation.GetOffsetDirection(beamSpt, beamEpt);
            texts.Add(CreateText(beamCalculationIndex.AsuFormat, midPt, textHeight * 0.0, perpendVec, textRotation));
            texts.Add(CreateText(beamCalculationIndex.GFormat, midPt, textHeight * 1.0, perpendVec, textRotation));
            texts.Add(CreateText(beamCalculationIndex.Asd.ToString(), midPt, textHeight * -1.0, perpendVec, textRotation));
            texts.Add(CreateText(beamCalculationIndex.VtFormat, midPt, textHeight * -2.0, perpendVec, textRotation));
            texts.Add(CreateText(beamCalculationIndex.Spec, midPt, textHeight * -3.0, perpendVec, textRotation));
            return texts;
        }
        #region ----------Print Calculation IndicatorText
        private double textHeight = 250.0;
        public List<Entity> DrawTexts()
        {            
            List<Entity> texts = new List<Entity>();
            if(beamLink == null)
            {
                return texts;
            }
            List<Point3d> pts = new List<Point3d>();
            beamLink.Item3.ForEach(o =>
            {
                pts.Add(o.StartPoint);
                pts.Add(o.EndPoint);
            });           
            int m = 0;
            int n = 1;
            double maxDis = pts[m].DistanceTo(pts[n]);
            for (int i = 0; i < pts.Count; i++)
            {
                for (int j = 0; j < pts.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (pts[i].DistanceTo(pts[j]) > maxDis)
                    {
                        m = i;
                        n = j;
                        maxDis = pts[i].DistanceTo(pts[j]);
                    }
                }
            }
            Point3d beamSpt = pts[m];
            Point3d beamEpt = pts[n];
            double textRotation = TextRotateAngle(beamSpt, beamEpt);
            Point3d midPt = CadTool.GetMidPt(beamSpt, beamEpt);
            Vector3d perpendVec = GeometricCalculation.GetOffsetDirection(beamSpt, beamEpt);
            texts.Add(CreateText(beamLink.Item1.BeamCalIndex.AsuFormat, midPt, textHeight * 0.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.Item1.BeamCalIndex.GFormat, midPt, textHeight * 1.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.Item1.BeamCalIndex.Asd.ToString(), midPt, textHeight * -1.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.Item1.BeamCalIndex.VtFormat, midPt, textHeight * -2.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.Item1.BeamCalIndex.Spec, midPt, textHeight * -3.0, perpendVec, textRotation));
            return texts;
        }
        private DBText CreateText(string content, Point3d basePt, double offsetHeight, Vector3d offsetVec, double textRotation)
        {
            DBText dBText = new DBText();
            dBText.TextString = content;
            dBText.Position = Point3d.Origin;
            dBText.Height = this.textHeight;            
            Matrix3d mt = Matrix3d.Rotation(textRotation, Vector3d.ZAxis, Point3d.Origin);
            dBText.TransformBy(mt);
            Point3d position = basePt + offsetVec.GetNormal().MultiplyBy(offsetHeight);
            Vector3d vec = Point3d.Origin.GetVectorTo(position);
            Matrix3d moveMt = Matrix3d.Displacement(vec);
            dBText.TransformBy(moveMt);
            return dBText;
        }

        private double TextRotateAngle(Point3d firstPt, Point3d secondPt)
        {
            Vector3d vec = secondPt - firstPt;
            double rad = vec.GetAngleTo(Vector3d.XAxis);
            double ang = Utils.RadToAng(rad);
            if (ang == 0.0 || ang == 180.0 || ang == 360.0)
            {
                return 0.0;
            }
            if (ang == 90.0 || ang == 270.0)
            {
                return Math.PI / 2.0;
            }
            if (ang > 0 && ang < 90)
            {
                return rad;
            }
            if (ang > 90 && ang < 180)
            {
                return rad - Math.PI;
            }
            if (ang > 180 && ang < 270)
            {
                return rad - Math.PI;
            }
            if (ang > 270 && ang < 360)
            {
                return rad - Math.PI * 2;
            }
            return rad;
        }
        #endregion
    }
}
