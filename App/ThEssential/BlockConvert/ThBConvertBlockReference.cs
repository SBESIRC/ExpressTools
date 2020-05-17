using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThEssential.BlockConvert
{
    public class ThBConvertBlockReference
    {
        public double Rotation { get; set; }
        public List<string> Texts { get; set; }
        public string EffectiveName { get; set; }
        public Database HostDatabase { get; set; }
        public Matrix3d BlockTransform { get; set; }
        public SortedDictionary<string, string> Attributes { get; set; }
        public DynamicBlockReferencePropertyCollection CustomProperties { get; set; }
        public ThBConvertBlockReference(ObjectId blockRef)
        {
            HostDatabase = blockRef.Database;
            Rotation = blockRef.GetBlockRotation();
            EffectiveName = blockRef.GetBlockName();
            CustomProperties = blockRef.GetDynProperties();
            BlockTransform = blockRef.GetBlockTransform();
            Attributes = blockRef.GetAttributesInBlockReference();
        }
    }
}