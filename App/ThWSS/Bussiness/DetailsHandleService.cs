using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThWSS.Model;

namespace ThWSS.Bussiness
{
    public class DetailsHandleService
    {
        SprayLayoutModel sprayLayoutModel;
        public DetailsHandleService(SprayLayoutModel layoutModel)
        {
            sprayLayoutModel = layoutModel;
        }

        public List<SprayLayoutData> AdjustmentSpray(Polyline polyline, List<SprayLayoutData> allSprays)
        {
            foreach (var spray in allSprays)
            {
                var closedPt = polyline.GetClosestPointTo(spray.Position, true);
                double distance = closedPt.DistanceTo(spray.Position);
                if (distance < sprayLayoutModel.otherSSpcing)
                {
                    Vector3d ptDir = (spray.Position - closedPt).GetNormal();
                    Vector3d yDir = spray.mainDir;
                    Vector3d xDir = spray.otherDir;
                    if (ptDir.DotProduct(yDir) < 0)
                    {
                        yDir = -yDir;
                    }
                    if (ptDir.DotProduct(xDir) < 0)
                    {
                        xDir = -xDir;
                    }
                    double moveDis = sprayLayoutModel.otherSSpcing - distance;
                    double yDis = moveDis / Math.Cos(yDir.GetAngleTo(ptDir));
                    double xDis = moveDis / Math.Cos(xDir.GetAngleTo(ptDir));
                    if (yDis < xDis)
                    {
                        spray.Position = spray.Position + yDis * yDir;
                    }
                    else
                    {
                        spray.Position = spray.Position + xDis * xDir;
                    }
                }
            }

            return allSprays;
        }
    }
}
