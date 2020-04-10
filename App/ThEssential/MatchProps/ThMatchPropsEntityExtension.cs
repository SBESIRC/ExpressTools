using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcHelper;
using GeometryExtensions;

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
        public static void MatchProps(Entity src, Entity dest, MarchPropertySet marchPropertySet)
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
                if(marchPropertySet.ColorOp)
                {
                    propertymatcher.CopyProperties(src, dest, (int)MatchPropFlags.ColorFlag);
                }
                if(marchPropertySet.LayerOp)
                {
                    propertymatcher.CopyProperties(src, dest, (int)MatchPropFlags.LayerFlag);
                }
                if(marchPropertySet.LineTypeOp)
                {
                    propertymatcher.CopyProperties(src, dest, (int)MatchPropFlags.LtypeFlag);
                }
                if (marchPropertySet.LineWeightOp)
                {
                    propertymatcher.CopyProperties(src, dest, (int)MatchPropFlags.LweightFlag);
                }
            }
        }
        public static void MarchTextContentProperty(Entity srcEntity,Entity destEntity)
        {
            if(srcEntity is DBText  && destEntity is DBText)
            {
                DBText srcText = srcEntity as DBText;
                DBText destText = destEntity as DBText;
                destText.TextString = srcText.TextString;
            }
            else if (srcEntity is MText && destEntity is MText)
            {
                MText srcText = srcEntity as MText;
                MText destText = destEntity as MText;
                destText.Contents = srcText.Contents;
            }
            else if(srcEntity is DBText && destEntity is MText)
            {
                DBText srcText = srcEntity as DBText;
                MText destText = destEntity as MText;
                destText.Contents = srcText.TextString;
            }
            else if(srcEntity is MText && destEntity is DBText)
            {
                MText srcText = srcEntity as MText;
                string[] strs = srcText.Contents.Split(new char[] { '\\','P'});
                string value=string.Join("", strs);
                DBText destText = destEntity as DBText;
                destText.TextString = value;
            }
        }
        public static void MarchTextSizeProperty(Entity srcEntity, Entity destEntity)
        {
            if (srcEntity is DBText && destEntity is DBText)
            {
                DBText srcText = srcEntity as DBText;
                DBText destText = destEntity as DBText;
                destText.Height = srcText.Height;
            }
            else if (srcEntity is MText && destEntity is MText)
            {
                MText srcText = srcEntity as MText;
                MText destText = destEntity as MText;
                destText.TextHeight = srcText.TextHeight;
            }
            else if (srcEntity is DBText && destEntity is MText)
            {
                DBText srcText = srcEntity as DBText;
                MText destText = destEntity as MText;
                destText.TextHeight = srcText.Height;
            }
            else if (srcEntity is MText && destEntity is DBText)
            {
                MText srcText = srcEntity as MText;               
                DBText destText = destEntity as DBText;
                destText.Height = srcText.TextHeight;
            }
        }
        public static void MarchTextDirectionProperty(Entity srcEntity, Entity destEntity)
        {
            if (srcEntity is DBText && destEntity is DBText)
            {
                DBText srcText = srcEntity as DBText;
                DBText destText = destEntity as DBText;
                destText.Normal = srcText.Normal;
                destText.Rotation = srcText.Rotation;
            }
            else if (srcEntity is MText && destEntity is MText)
            {
                MText srcText = srcEntity as MText;
                MText destText = destEntity as MText;
                destText.Normal = srcText.Normal;
                destText.Direction = srcText.Direction;
            }
            else if (srcEntity is DBText && destEntity is MText)
            {
                DBText srcText = srcEntity as DBText;
                MText destText = destEntity as MText;
                Point3d position= destText.Location; 
                destText.Normal = srcText.Normal;
                Matrix3d mt=  Matrix3d.Rotation(srcText.Rotation, Vector3d.ZAxis, Point3d.Origin);
                Vector3d rotateXVec= Vector3d.XAxis.TransformBy(mt);
                destText.Direction = rotateXVec;
                destText.Location = position;
            }
            else if (srcEntity is MText && destEntity is DBText)
            {
                MText srcText = srcEntity as MText;
                DBText destText = destEntity as DBText;
                Point3d position = destText.Position;
                destText.Normal = srcText.Normal;
                double angle = srcText.Direction.GetAngleTo(Vector3d.XAxis);
                destText.Rotation = angle;
                destText.Position = position;
            }
        }
    }
}
