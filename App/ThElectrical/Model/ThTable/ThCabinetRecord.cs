using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThElectrical.Model.ThElement;
using ThResourceLibrary;

namespace ThElectrical.Model.ThTable
{
    public class ThCabinetRecord : ThNotifyObject
    {

        public ThCabinetRecord(ThCircuitElement circuitElement, ThPowerCapacityElement powerCapacityElement, ThOutCableElement outCableElement, ThBranchSwitchElement branchSwitchElement)
        {
            CircuitElement = circuitElement;
            PowerCapacityElement = powerCapacityElement;
            OutCableElement = outCableElement;
            BranchSwitchElement = branchSwitchElement;
        }

        public ThCabinetRecord(ThCircuitElement circuitElement, ThPowerCapacityElement powerCapacityElement)
        {
            CircuitElement = circuitElement;
            PowerCapacityElement = powerCapacityElement;
        }

        public ThCabinetRecord(ThCircuitElement circuitElement)
        {
            CircuitElement = circuitElement;
        }

        public ThCircuitElement CircuitElement { get; set; }

        private ThPowerCapacityElement _powerCapacityElement;
        public ThPowerCapacityElement PowerCapacityElement
        {
            get
            {
                return _powerCapacityElement;
            }
            set
            {
                _powerCapacityElement = value;
                RaisePropertyChanged("PowerCapacityElement");
            }
        }

        private ThOutCableElement _outCableElement;
        public ThOutCableElement OutCableElement
        {
            get
            {
                return _outCableElement;
            }
            set
            {
                _outCableElement = value;
                RaisePropertyChanged("OutCableElement");
            }
        }


        private ThBranchSwitchElement _branchSwitchElement;
        public ThBranchSwitchElement BranchSwitchElement
        {
            get
            {
                return _branchSwitchElement;
            }
            set
            {
                _branchSwitchElement = value;
                RaisePropertyChanged("BranchSwitchElement");
            }
        }


    }
}
