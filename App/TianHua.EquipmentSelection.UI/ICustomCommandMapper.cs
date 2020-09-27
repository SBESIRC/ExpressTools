using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.FanSelection.UI
{
    public interface ICustomCommandMapper
    {
        string GetMappedCustomCommand(ObjectId entId);
    }
}
