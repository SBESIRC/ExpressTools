using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig
{
    public class ThAreaFrameHighlightOverrule : HighlightOverrule
    {
        public ThAreaFrameHighlightOverrule()
        {
        }

        public override bool IsApplicable(RXObject overruledSubject)
        {
            return false;
        }

        public override void Highlight(Entity entity, FullSubentityPath subId, bool highlightAll)
        {
            base.Highlight(entity, subId, highlightAll);
        }
        public override void Unhighlight(Entity entity, FullSubentityPath subId, bool highlightAll)
        {
            base.Unhighlight(entity, subId, highlightAll);
        }
    }
}
