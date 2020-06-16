using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.LayerState
{
    public class ThLayerStateUtils
    {
        public static bool IsInModel()
        {
            return Active.Database.TileMode;
        }

        public static bool IsInLayout()
        {
            return !IsInModel();
        }

        public static bool IsInLayoutPaper()
        {
            if (Active.Database.TileMode)
                return false;
            else
            {
                if (Active.Database.PaperSpaceVportId == ObjectId.Null)
                    return false;
                else if (Active.Editor.CurrentViewportObjectId == ObjectId.Null)
                    return false;
                else if (Active.Editor.CurrentViewportObjectId == Active.Database.PaperSpaceVportId)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsInLayoutViewport()
        {
            return IsInLayout() && !IsInLayoutPaper();
        }
    }
}
