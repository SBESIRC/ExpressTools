using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK
{
    class YjkDatabase : IDatabaseSource
    {
        private string dtlModelPath = "";
        private string dtlCalculatePath = "";
        public YjkDatabase(string dtlModelPath,string dtlCalculatePath)
        {
            this.dtlModelPath = dtlModelPath;
            this.dtlCalculatePath = dtlCalculatePath;
        }
        public IList<IEntityInf> ExtractBeam(int floorNo)
        {
            return new YjkBeamQuery(this.dtlModelPath).Extract(floorNo);
        }

        public IList<IEntityInf> ExtractBoard(int floorNo)
        {
            return new YjkBoardQuery(this.dtlModelPath).Extract(floorNo);
        }

        public IList<IEntityInf> ExtractColumn(int floorNo)
        {
            return new YjkColumnQuery(this.dtlModelPath).Extract(floorNo);
        }

        public IList<IEntityInf> ExtractWall(int floorNo)
        {
            return new YjkWallQuery(this.dtlModelPath).Extract(floorNo);
        }

        public ICalculateInfo GetBeamCalculateInfo(IEntityInf beamInf)
        {
            return new YjkBeamCalculateInfo(this.dtlModelPath, this.dtlCalculatePath).GetCalculateInfo(beamInf);
        }

        public ICalculateInfo GetBoardCalculateInfo(IEntityInf boardInf)
        {
            return new YjkBoardCalculateInfo(this.dtlModelPath, this.dtlCalculatePath).GetCalculateInfo(boardInf);
        }

        public ICalculateInfo GetColumnCalculateInfo(IEntityInf columnInf)
        {
            return new YjkColumnCalculateInfo(this.dtlModelPath,this.dtlCalculatePath).GetCalculateInfo(columnInf);
        }

        public ICalculateInfo GetWallCalculateInfo(IEntityInf wallInf)
        {
            return new YjkWallCalculateInfo(this.dtlModelPath, this.dtlCalculatePath).GetCalculateInfo(wallInf);
        }
    }
}
