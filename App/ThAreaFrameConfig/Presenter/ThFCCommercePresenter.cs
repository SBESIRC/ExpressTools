using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using AcHelper;
using Autodesk.AutoCAD.EditorInput;

namespace ThAreaFrameConfig.Presenter
{
    public class ThFCCommercePresenter : IThFireCompartmentPresenter, IThFireCompartmentPresenterCallback
    {
        private readonly IFCCommerceView compartmentView;

        public ThFCCommercePresenter(IFCCommerceView view)
        {
            compartmentView = view;
        }

        public object UI => compartmentView;

        public bool OnPickAreaFrames(ThFireCompartment compartment, string name)
        {
            return this.PickAreaFrames(compartment, name);
        }
    }
}
