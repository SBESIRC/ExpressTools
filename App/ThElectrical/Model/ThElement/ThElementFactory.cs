using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThElectrical.Model.ThColumn;

namespace ThElectrical.Model.ThElement
{
    public class ThElementFactory
    {
        public const double circuitToPowerCapacityTol = 325;//回路与容量的位置容差
        public const double circuitToOutCableTol = 401;//回路与出现电缆规格的位置容差
        public const double circuitToBranchSwitchTol = 401;//回路与分支开关的位置容差

        public static ThElement CreateElement(Type type, ObjectId id)
        {
            if (type == typeof(ThPowerCapacityColumn))
            {
                return new ThPowerCapacityElement(id);
            }
            else if (type == typeof(ThCircuitColumn))
            {
                return new ThCircuitElement(id);
            }
            else if (type == typeof(ThOutCableColumn))
            {
                return new ThOutCableElement(id);
            }
            else if (type == typeof(ThBranchSwitchColumn))
            {
                return new ThBranchSwitchElement(id);
            }
            else if (type == typeof(ThCabinetColumn))
            {
                return new ThCabinetElement(id);
            }
            else
            {
                return null;
            }
        }
    }
}
