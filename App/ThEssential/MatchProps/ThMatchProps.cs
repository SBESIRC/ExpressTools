using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThEssential.MatchProps
{
    public enum MatchPropFlags  //dbmatch.h
    {
        ColorFlag = 0x1,
        LayerFlag = 0x2,
        LtypeFlag = 0x4,
        ThicknessFlag = 0x8,
        LtscaleFlag = 0x10,
        TextFlag = 0x20,
        DimensionFlag = 0x40,
        HatchFlag = 0x80,
        LweightFlag = 0x100,
        PlotstylenameFlag = 0x200,
        PolylineFlag = 0x400,
        ViewportFlag = 0x800,
        TableFlag = 0x1000,
        MaterialFlag = 0x2000,
        ShadowDisplayFlag = 0x4000,
        MultileaderFlag = 0x8000,
        TransparencyFlag = 0x10000,
        SetAllFlagsOn = 0x1FFFF,
    };
}
