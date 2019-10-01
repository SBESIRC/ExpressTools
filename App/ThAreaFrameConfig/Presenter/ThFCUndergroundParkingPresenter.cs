using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThFCUndergroundParkingPresenter : IThFireCompartmentPresenter, IThFireCompartmentPresenterCallback
    {
        private readonly IFireCompartmentView compartmentView;

        public ThFCUndergroundParkingPresenter(IFireCompartmentView view)
        {
            compartmentView = view;
        }

        public object UI => compartmentView;

        public void OnCreateFCCommerceFills(List<ThFireCompartment> compartments)
        {
            throw new System.NotImplementedException();
        }

        public void OnCreateFCCommerceTable(ThFCCommerceSettings settings)
        {
            throw new System.NotImplementedException();
        }

        public bool OnDeleteFireCompartments(List<ThFireCompartment> compartments)
        {
            throw new System.NotImplementedException();
        }

        public bool OnMergeFireCompartments(List<ThFireCompartment> compartments)
        {
            throw new System.NotImplementedException();
        }

        public bool OnMergePickedFireCompartments(ThFCCommerceSettings settings)
        {
            throw new System.NotImplementedException();
        }

        public bool OnModifyFireCompartment(ThFireCompartment compartment)
        {
            throw new System.NotImplementedException();
        }

        public bool OnModifyFireCompartments(List<ThFireCompartment> compartments)
        {
            throw new System.NotImplementedException();
        }

        public bool OnPickAreaFrames(ThFireCompartment compartment, string layer, string islandLayer)
        {
            throw new System.NotImplementedException();
        }

        public bool OnSetFCCommerceLayer(ThFCCommerceSettings settings, string key)
        {
            throw new System.NotImplementedException();
        }
    }
}
