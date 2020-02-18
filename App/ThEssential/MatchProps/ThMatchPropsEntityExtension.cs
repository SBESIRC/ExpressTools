using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.MatchProps
{
    public class ThMatchPropsEntityExtension
    {
        /// <summary>
        /// 拷贝实体属性（MatchProperties Protocol Extension）
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// http://www.theswamp.org/index.php?topic=36986.0
        public static void MatchProps(Entity src, Entity dest)
        {
            // make sure we have acmatch.arx
            HostApplicationServices.Current.LoadApplication("AcadMatch",
                ApplicationLoadReasons.OnLoadRequest, false, false);

            IntPtr _propertyMatcher = IntPtr.Zero;
            RXClass srcClass = src.GetRXClass();
            RXClass destClass = dest.GetRXClass();

            if (srcClass.IsDerivedFrom(destClass) || destClass.IsDerivedFrom(srcClass))
            {
                // Unmanaged AcDbMatchProperties protocol extension class specific to dest entity
                _propertyMatcher = dest.QueryX(RXClass.GetClass(typeof(MatchProperties)));
            }
            else
            {
                // Unmanaged generic AcDbMatchProperties protocol extension class
                _propertyMatcher = RXClass.GetClass(typeof(Entity)).QueryX(RXClass.GetClass(typeof(MatchProperties)));
            }
            if (_propertyMatcher == IntPtr.Zero) return;

            using (MatchProperties propertymatcher = MatchProperties.Create(_propertyMatcher, false) as MatchProperties)
            {
                int matchFlags = (int)MatchPropFlags.SetAllFlagsOn;
                propertymatcher.CopyProperties(src, dest, matchFlags);
            }
        }
    }
}
