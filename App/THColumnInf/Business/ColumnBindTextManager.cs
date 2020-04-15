using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class ColumnBindTextManager
    {
        private List<ColumnBindText> columnBindTexts = new List<ColumnBindText>();
        public List<ColumnBindText> ColumnBindTexts
        {
            get
            {
                return columnBindTexts;
            }
        }
        //获取图纸中所有柱子
        public ColumnBindTextManager()
        {
            PlantCalDataToDraw plantCalDataToDraw = new PlantCalDataToDraw(false);
            plantCalDataToDraw.GetEmbededColumnIds();
            plantCalDataToDraw.EmbededColumnIds.ForEach(i => this.columnBindTexts.Add(new ColumnBindText(i)));
        }
        public void ShowOrHideColumn(bool visible)
        {
            columnBindTexts.ForEach(i => i.ShowOrHideColumn(visible));
        }
        public void ShowOrHideFrameText(bool visible)
        {
            columnBindTexts.ForEach(i => i.ShowOrHideFrameText(visible));
        }
        public void Refresh(ModifyCustomDataType modifyCustomDataType)
        {
            //刷新当前显示内容
            if (modifyCustomDataType == ModifyCustomDataType.None)
            {
                return;
            }
            this.columnBindTexts.ForEach(i => i.Update(modifyCustomDataType));           
        }
        #region----------修改柱子中的属性值----------------
        public void ModifyColumnCustomData(List<ObjectId> modifyObjIds, ModifyCustomDataType modifyCustomDataType, object value)
        {
            //1、第一次修改的值，记录->1，第二次修改的值，记录->2,依次类推
            //2、如果修改的值，在之前的记录中已存在则顺序与已存在的记录保持一致
            if (modifyObjIds.Count == 0 || modifyCustomDataType == ModifyCustomDataType.None || value == null)
            {
                return;
            }
           short colorIndex= GetColorIndex(modifyCustomDataType, value);
           List<ColumnBindText> modifyColumnBindTexts = this.columnBindTexts.Where(
               i => modifyObjIds.IndexOf(i.ColumnId) >= 0).Select(i => i).ToList();
            switch (modifyCustomDataType)
            {
                case ModifyCustomDataType.AntiSeismicGrade:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.AntiSeismicGrade = (string)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.AntiSeismicGradeColorIndex = colorIndex);
                    break;
                case ModifyCustomDataType.ConcreteStrength:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.ConcreteStrength = (string)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.ConcreteStrengthColorIndex = colorIndex);
                    break;
                case ModifyCustomDataType.CornerColumn:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.CornerColumn = (string)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.CornerColumnColorIndex = colorIndex);
                    break;
                case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.HoopReinforceFullHeightEncryption = (string)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.HoopFullHeightEncryptionColorIndex = colorIndex);
                    break;
                case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.HoopReinforcementEnlargeTimes = (int)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.HoopEnlargeTimesColorIndex = colorIndex);
                    break;
                case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.LongitudinalReinforceEnlargeTimes = (int)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.LongitudinalEnlargeTimesColorIndex = colorIndex);
                    break;
                case ModifyCustomDataType.ProtectLayerThickness:
                    modifyColumnBindTexts.ForEach(i => i.CustomData.ProtectLayerThickness = (int)value);
                    modifyColumnBindTexts.ForEach(i => i.CustomData.ProtectThickColorIndex = colorIndex);
                    break;
            }
            modifyColumnBindTexts.ForEach(i => i.Modify(modifyCustomDataType));
        }
        /// <summary>
        /// 获取要设置的颜色值，在所有柱子中是否已存在，
        /// </summary>
        /// <param name="modifyCustomDataType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private short GetColorIndex(ModifyCustomDataType modifyCustomDataType, object value)
        {
            short colorIndex = 0;
            List<short> colorIndexList = new List<short>();
            List<short> allColorIndexes = new List<short>();
            switch (modifyCustomDataType)
            {
                case ModifyCustomDataType.AntiSeismicGrade:
                    colorIndexList=this.columnBindTexts.Where(i => i.CustomData.AntiSeismicGrade ==
                    (string)value).Select(i => i.CustomData.AntiSeismicGradeColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.AntiSeismicGradeColorIndex).ToList();
                    break;
                case ModifyCustomDataType.ConcreteStrength:
                    colorIndexList = this.columnBindTexts.Where(i => i.CustomData.ConcreteStrength ==
                      (string)value).Select(i => i.CustomData.ConcreteStrengthColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.ConcreteStrengthColorIndex).ToList();
                    break;
                case ModifyCustomDataType.CornerColumn:
                    colorIndexList = this.columnBindTexts.Where(i => i.CustomData.CornerColumn ==
                      (string)value).Select(i => i.CustomData.CornerColumnColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.CornerColumnColorIndex).ToList();
                    break;
                case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                    colorIndexList = this.columnBindTexts.Where(i => i.CustomData.HoopReinforceFullHeightEncryption ==
                      (string)value).Select(i => i.CustomData.HoopFullHeightEncryptionColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.HoopFullHeightEncryptionColorIndex).ToList();
                    break;
                case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                    colorIndexList = this.columnBindTexts.Where(i => i.CustomData.HoopReinforcementEnlargeTimes ==
                      (int)value).Select(i => i.CustomData.HoopEnlargeTimesColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.HoopEnlargeTimesColorIndex).ToList();
                    break;
                case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
                    colorIndexList = this.columnBindTexts.Where(i => i.CustomData.LongitudinalReinforceEnlargeTimes ==
                      (int)value).Select(i => i.CustomData.LongitudinalEnlargeTimesColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.LongitudinalEnlargeTimesColorIndex).ToList();
                    break;
                case ModifyCustomDataType.ProtectLayerThickness:
                    colorIndexList = this.columnBindTexts.Where(i => i.CustomData.ProtectLayerThickness ==
                      (double)value).Select(i => i.CustomData.ProtectThickColorIndex).ToList();
                    allColorIndexes = this.columnBindTexts.Select(i => i.CustomData.ProtectThickColorIndex).ToList();
                    break;
            }
            //获取当前修改的值的颜色索引
            colorIndexList = colorIndexList.Where(i => i > 0).Select(i => i).ToList();
            colorIndexList = colorIndexList.Distinct().ToList();
            allColorIndexes = allColorIndexes.Distinct().ToList();
            short maxShortIndex = allColorIndexes.OrderByDescending(i => i).FirstOrDefault();
            if (colorIndexList.Count>0)
            {
                colorIndex = colorIndexList[0];
            }
            else
            {
                colorIndex = (short)(maxShortIndex + 1);
            }
            return colorIndex;
        }
        #endregion
        #region----------获取柱子中存在的颜色信息----------
        /// <summary>
        /// 获取柱子中存在的颜色信息
        /// </summary>
        public List<ColorTextInfo> GetColorTextInfos(ModifyCustomDataType modifyCustomDataType)
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> filterColumnBindText = new List<ColumnBindText>();
            switch (modifyCustomDataType)
            {
                case ModifyCustomDataType.AntiSeismicGrade:
                    colorTextInfos = GenerateAntiSeismicGradeColorInfs();
                    break;
                case ModifyCustomDataType.ConcreteStrength:
                    colorTextInfos = GenerateConcreteStrengthColorInfs();
                    break;
                case ModifyCustomDataType.CornerColumn:
                    colorTextInfos = GenerateCornerColumnColorInfs();
                    break;
                case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                    colorTextInfos = GenerateHoopFullHeightEncryptionColorInfs();
                    break;
                case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                    colorTextInfos = GenerateHoopReinforcementEnlargeTimesColorInfs();
                    break;
                case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:                  
                    colorTextInfos = GenerateLongitudinalEnlargeTimesColorInfs();
                    break;
                case ModifyCustomDataType.ProtectLayerThickness:
                    colorTextInfos = GenerateProtectThicknessColorInfs();
                    break;
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 抗震等级设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateAntiSeismicGradeColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.AntiSeismicGradeColorIndex > 0
            && !string.IsNullOrEmpty(i.CustomData.AntiSeismicGrade)).Select(i => i).ToList();
            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.AntiSeismicGradeColorIndex).ToList();
            List<string> antiSeismicGradeCollection = coluBindTexts.Select(i => i.CustomData.AntiSeismicGrade).ToList();
            antiSeismicGradeCollection = antiSeismicGradeCollection.Distinct().ToList();
            foreach (string antiSeismicGrade in antiSeismicGradeCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.AntiSeismicGrade == antiSeismicGrade)
                    .Select(i => i.CustomData.AntiSeismicGradeColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = antiSeismicGrade,
                    Content = antiSeismicGrade,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 混凝土强度设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateConcreteStrengthColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.ConcreteStrengthColorIndex > 0 &&
            !string.IsNullOrEmpty(i.CustomData.ConcreteStrength)).Select(i => i).ToList();

            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.ConcreteStrengthColorIndex).ToList();
            List<string> concreteStrengthCollection = coluBindTexts.Select(i => i.CustomData.ConcreteStrength).ToList();
            concreteStrengthCollection = concreteStrengthCollection.Distinct().ToList();
            foreach (string concreteStrength in concreteStrengthCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.ConcreteStrength == concreteStrength)
                    .Select(i => i.CustomData.ConcreteStrengthColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = concreteStrength,
                    Content = concreteStrength,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 角柱设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateCornerColumnColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.CornerColumnColorIndex > 0
            && !string.IsNullOrEmpty(i.CustomData.CornerColumn)).Select(i => i).ToList();

            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.CornerColumnColorIndex).ToList();
            List<string> cornerColumnCollection = coluBindTexts.Select(i => i.CustomData.CornerColumn).ToList();
            cornerColumnCollection = cornerColumnCollection.Distinct().ToList();
            foreach (string cornerColumn in cornerColumnCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.CornerColumn == cornerColumn)
                    .Select(i => i.CustomData.CornerColumnColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = cornerColumn,
                    Content = cornerColumn,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 全高度加密设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateHoopFullHeightEncryptionColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.HoopFullHeightEncryptionColorIndex > 0
            && !string.IsNullOrEmpty(i.CustomData.HoopReinforceFullHeightEncryption)).Select(i => i).ToList();

            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.HoopFullHeightEncryptionColorIndex).ToList();
            List<string> hoopReinforceFullHeightCollection = coluBindTexts.Select(
                i => i.CustomData.HoopReinforceFullHeightEncryption).ToList();
            hoopReinforceFullHeightCollection = hoopReinforceFullHeightCollection.Distinct().ToList();
            foreach (string hoopReinforceFullHeight in hoopReinforceFullHeightCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.HoopReinforceFullHeightEncryption == hoopReinforceFullHeight)
                    .Select(i => i.CustomData.HoopFullHeightEncryptionColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = hoopReinforceFullHeight,
                    Content = hoopReinforceFullHeight,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 箍筋放大倍数设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateHoopReinforcementEnlargeTimesColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.HoopEnlargeTimesColorIndex > 0).Select(i => i).ToList();
            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.HoopEnlargeTimesColorIndex).ToList();
            List<int> hoopReinEnlargeTimesCollection = coluBindTexts.Select(
                i => i.CustomData.HoopReinforcementEnlargeTimes).ToList();
            hoopReinEnlargeTimesCollection = hoopReinEnlargeTimesCollection.Distinct().ToList();
            foreach (int hoopReinforceEnlargeTime in hoopReinEnlargeTimesCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.HoopReinforcementEnlargeTimes == hoopReinforceEnlargeTime)
                    .Select(i => i.CustomData.HoopEnlargeTimesColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = hoopReinforceEnlargeTime.ToString(),
                    Content = hoopReinforceEnlargeTime,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 纵筋放大倍数设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateLongitudinalEnlargeTimesColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.LongitudinalEnlargeTimesColorIndex > 0).Select(i => i).ToList();
            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.LongitudinalEnlargeTimesColorIndex).ToList();
            List<int> longitudinalEnlargeTimesCollection = coluBindTexts.Select(
                i => i.CustomData.LongitudinalReinforceEnlargeTimes).ToList();
            longitudinalEnlargeTimesCollection = longitudinalEnlargeTimesCollection.Distinct().ToList();
            foreach (int longitudinalEnlargeTime in longitudinalEnlargeTimesCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.LongitudinalReinforceEnlargeTimes == longitudinalEnlargeTime)
                    .Select(i => i.CustomData.LongitudinalEnlargeTimesColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = longitudinalEnlargeTime.ToString(),
                    Content = longitudinalEnlargeTime,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        /// <summary>
        /// 保护层厚度设置值顺序
        /// </summary>
        /// <param name="columnBindTexts"></param>
        /// <returns></returns>
        private List<ColorTextInfo> GenerateProtectThicknessColorInfs()
        {
            List<ColorTextInfo> colorTextInfos = new List<ColorTextInfo>();
            List<ColumnBindText> coluBindTexts = this.columnBindTexts.Where(i => i.CustomData.ProtectThickColorIndex > 0).Select(i => i).ToList();
            coluBindTexts = coluBindTexts.OrderByDescending(i => i.CustomData.ProtectThickColorIndex).ToList();
            List<double> protectThickCollection = coluBindTexts.Select(
                i => i.CustomData.ProtectLayerThickness).ToList();
            protectThickCollection = protectThickCollection.Distinct().ToList();
            foreach (double protectThick in protectThickCollection)
            {
                short colorIndex = coluBindTexts.Where(i => i.CustomData.ProtectLayerThickness == protectThick)
                    .Select(i => i.CustomData.LongitudinalEnlargeTimesColorIndex).FirstOrDefault();
                Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByColor, colorIndex);
                colorTextInfos.Add(new ColorTextInfo
                {
                    BackGroundBrush = ThColumnInfoUtils.SysColorConvertBrush(ThColumnInfoUtils.AcadColorToSystemColor(color)),
                    Text = protectThick.ToString(),
                    Content = protectThick,
                    ColorIndex = colorIndex
                });
            }
            return colorTextInfos;
        }
        #endregion
    }
}
