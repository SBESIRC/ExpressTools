using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class ColumnSectionModel: ValidateModel
    {
        /// <summary>
        /// 楼层总层数
        /// </summary>
        public int FloorTotalNums { get; set; }

        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> {"KZ", "ZHZ" })
               || Cdm == null)
            {
                return false;
            }
            if (Cdm.B <= 0 || Cdm.H <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
