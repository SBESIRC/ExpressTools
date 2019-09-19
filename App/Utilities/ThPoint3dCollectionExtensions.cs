using Autodesk.AutoCAD.Geometry;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ThPoint3dCollectionExtensions
    {
        public static void Swap(this Point3dCollection collection, int index1, int index2)
        {
            Point3d temp = collection[index1];
            collection[index1] = collection[index2];
            collection[index2] = temp;
        }
    }
}
