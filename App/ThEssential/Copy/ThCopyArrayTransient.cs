using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace ThEssential.Copy
{
    public class ThCopyArrayTransient : Transient
    {
        public uint Parameter { get; set; }

        public ThCopyArrayTransient()
        {
            Parameter = 1;
        }

        protected override int SubSetAttributes(DrawableTraits traits)
        {
            throw new NotImplementedException();
        }

        protected override void SubViewportDraw(ViewportDraw vd)
        {
            throw new NotImplementedException();
        }

        protected override bool SubWorldDraw(WorldDraw wd)
        {
            throw new NotImplementedException();
        }

        public void CreateTransGraphics(List<Entity> entities)
        {
            foreach(Drawable drawable in entities)
            {
                TransientManager.CurrentTransientManager.AddTransient(drawable, 
                    TransientDrawingMode.DirectShortTerm, 128, new IntegerCollection());
            }
        }

        public void UpdateTransGraphics(List<Entity> entities)
        {
            foreach(Drawable drawable in entities)
            {
                TransientManager.CurrentTransientManager.UpdateTransient(drawable,
                    new IntegerCollection());
            }
        }

        public void ClearTransGraphics(List<Entity> entities)
        {
            TransientManager.CurrentTransientManager.EraseTransients(
                TransientDrawingMode.DirectShortTerm,
                128, new IntegerCollection());
        }
    }
}
