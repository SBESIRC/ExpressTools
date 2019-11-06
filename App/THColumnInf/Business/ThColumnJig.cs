using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.ApplicationServices;
namespace ThColumnInfo
{
    public class ThColumnJig:DrawJig
    {
        private Point3d mPosition;
        private List<Autodesk.AutoCAD.DatabaseServices.Polyline> polylines=new List<Autodesk.AutoCAD.DatabaseServices.Polyline>();
        private Matrix3d currentUcs ;
        public ThColumnJig(List<Autodesk.AutoCAD.DatabaseServices.Polyline> polylines)
        {
            mPosition = Point3d.Origin;
            this.polylines = polylines;
            this.currentUcs = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
        }
        public Point3d Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\n选择安装的基点:");
            prOptions1.UseBasePoint = false;
            PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
            if (prResult1.Status == PromptStatus.Cancel || prResult1.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;
            Point3d tempPt = prResult1.Value.TransformBy(this.currentUcs.Inverse());
            if (tempPt.Equals(mPosition))  //Use better comparison method if necessary.
            {
                return SamplerStatus.NoChange;
            }
            else
            {
                Matrix3d mt = Matrix3d.Displacement(mPosition.GetVectorTo(tempPt));
                this.polylines.ForEach(i => i.TransformBy(mt));
                mPosition = tempPt;
                return SamplerStatus.OK;
            }
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            for(int i=0;i<this.polylines.Count;i++)
            {
                draw.Geometry.Draw(this.polylines[i]);
            }
            return true;
        }
    }
}
