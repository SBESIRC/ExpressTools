﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK
{
    interface IDatabaseSource
    {
        IList<IEntityInf> ExtractColumn(int floorNo);
        IList<IEntityInf> ExtractBeam(int floorNo);
        IList<IEntityInf> ExtractWall(int floorNo);
        IList<IEntityInf> ExtractBoard(int floorNo);
        ICalculateInfo GetColumnCalculateInfo(IEntityInf columnInf);
        ICalculateInfo GetBoardCalculateInfo(IEntityInf columnInf);
        ICalculateInfo GetBeamCalculateInfo(IEntityInf columnInf);
        ICalculateInfo GetWallCalculateInfo(IEntityInf wallInf);
    }
}
