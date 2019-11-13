using Linq2Acad;
using MbUnit.Framework;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThXClip.Test
{
    public class SampleTest
    {
        [Test]
        public void CreateCircle()
        {
            ObjectId circleId;
            using (var db = AcadDatabase.Active())
            {
                var obj = new Circle(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), 10);
                ObjectId objId = db.ModelSpace.Add(obj, true);
                var circle = db.Element<Circle>(objId, true);
                if (circle.Radius < 1.0)
                {
                    circle.ColorIndex = 2;
                }
                else if (circle.Radius > 10.0)
                {
                    circle.ColorIndex = 1;
                }
                else
                {
                    circle.ColorIndex = 3;
                }
                circleId = objId;
            }

            using (var db = AcadDatabase.Active())
            {
                var circle = db.Element<Circle>(circleId);
                Assert.AreEqual(circle.ColorIndex, 3);
            }
        }
    }
}
