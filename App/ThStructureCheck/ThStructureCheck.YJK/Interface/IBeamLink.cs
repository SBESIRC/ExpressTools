using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Interface
{
    public interface IBeamLink
    {
        List<BeamLink> BeamLinks { get;}
        void Build(); 
    }
}
