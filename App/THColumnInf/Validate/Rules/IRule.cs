using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public interface IRule
    {
       List<ValidateResult> ValidateResults { get; set; }
       void Validate();
    }
    public enum ValidateResult
    {
        /// <summary>
        /// 截面过小
        /// </summary>
        SectionTooSmall,
        /// <summary>
        /// 长边小于等于短边的三倍
        /// </summary>
        LongLessThanShortTriple,
        /// <summary>
        /// 轴压比超限
        /// </summary>
        AxialCompressionRatioTransfinite,
        /// <summary>
        /// 脚筋根数是4的倍数
        /// </summary>
        AngularReinforcementNumFourTimes,
        /// <summary>
        /// 角筋直径不足
        /// </summary>
        AngularReinforcementDiaIsNotEnough,
        /// <summary>
        /// 纵向受力钢筋直径不宜小于12mm
        /// </summary>
        VerDirForceBarDiaLessThanTwelveMm,
        /// <summary>
        /// 全部纵向钢筋的配筋率不宜大于5%
        /// </summary>
        AllVerDirIronReinforceRatioBiggerThanFivePercent,
        /// <summary>
        /// 纵向钢筋净间距不足
        /// </summary>
        VerDirIronClearSpaceNotEnough,
        /// <summary>
        /// 纵向钢筋净间距过大
        /// </summary>
        VerDirIronClearSpaceNotEnoughTooLarge
    }
}
