using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;

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

        public bool OnDeleteFireCompartments(List<ThFireCompartment> compartments)
        {
            return ThFireCompartmentDbHelper.DeleteFireCompartments(compartments);
        }

        public bool OnMergeFireCompartments(List<ThFireCompartment> compartments)
        {
            return ThFireCompartmentDbHelper.MergeFireCompartment(compartments);
        }

        public bool OnModifyFireCompartment(ThFireCompartment compartment)
        {
            return ThFireCompartmentDbHelper.ModifyFireCompartment(compartment);
        }

        public bool OnPickAreaFrames(ThFireCompartment compartment, string name)
        {
            return this.PickAreaFrames(compartment, name);
        }
    }
}
