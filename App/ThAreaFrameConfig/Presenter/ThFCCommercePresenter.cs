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

        public bool OnPickAreaFrames(ThFireCompartment compartment, string layer, string islandLayer)
        {
            return this.PickAreaFrames(compartment, layer, islandLayer);
        }

        public void OnCreateFCCommerceTable(ThFCCommerceSettings settings)
        {
            this.CreateFCCommerceTable(settings);
        }

        public void OnCreateFCCommerceFills(List<ThFireCompartment> compartments)
        {
            this.CreateFCCommerceFills(compartments);
        }

        public bool OnSetFCCommerceLayer(ThFCCommerceSettings settings, string key)
        {
            return this.PickAreaFrameLayer(settings, key);
        }
    }
}
