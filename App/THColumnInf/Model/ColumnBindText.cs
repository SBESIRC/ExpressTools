using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    public class ColumnBindText
    {
        private ObjectId columnId = ObjectId.Null;
        private ColumnCustomData customData;
        private ObjectId textId = ObjectId.Null;
        private ObjectId frameId = ObjectId.Null;
        private List<Point3d> originColumnPts = new List<Point3d>();

        /// <summary>
        /// 文字偏离柱方框的距离
        /// </summary>
        private double textOffsetDis = 5.0;
        public ObjectId ColumnId
        {
            get
            {
                return columnId;
            }
        }
        public ColumnCustomData CustomData
        {
            get
            {
                return customData;
            }
        }
        public ColumnBindText(ObjectId columnId)
        {
            this.columnId = columnId;
            Init();
        }

        private void Init()
        {
            PlantCalDataToDraw plantCalDataToDraw = new PlantCalDataToDraw(false);
            bool success = false;
            this.customData = plantCalDataToDraw.ReadEmbededColumnCustomData(columnId, out success);
            this.originColumnPts = ThColumnInfoUtils.GetPolylinePts(this.columnId);
        }

        private void CreateText(object textContent)
        {
            if(textContent==null || string.IsNullOrEmpty(textContent.ToString()))
            {
                return;
            }
            if(ThColumnInfoUtils.CheckObjIdIsValid(this.textId))
            {
                ThColumnInfoUtils.ChangeText(this.textId, textContent.ToString());
                return;
            }
            if(this.originColumnPts.Count==0)
            {
                return;
            }
            double minX = this.originColumnPts.OrderBy(i => i.X).First().X;
            double minY = this.originColumnPts.OrderBy(i => i.Y).First().Y;
            double minZ = this.originColumnPts.OrderBy(i => i.Z).First().Z;

            double maxX = this.originColumnPts.OrderByDescending(i => i.X).First().X;
            double maxY = this.originColumnPts.OrderByDescending(i => i.Y).First().Y;
            double maxZ = this.originColumnPts.OrderByDescending(i => i.Z).First().Z;

            Point3d firstPt = new Point3d(minX, minY, minZ);
            Point3d thirdPt = new Point3d(maxX, maxY, minZ);
            Point3d secondPt= new Point3d(maxX, minY, minZ);
            Point3d fourthPt = new Point3d(minX, maxY, minZ);

            Point3d firstMidPt = ThColumnInfoUtils.GetMidPt(firstPt, fourthPt);
            Point3d secondMidPt= ThColumnInfoUtils.GetMidPt(secondPt, thirdPt);
            Point3d textStart = ThColumnInfoUtils.GetExtendPt(secondMidPt, firstMidPt, -1.0*this.textOffsetDis);
            DBText dbText = new DBText();
            dbText.Position = textStart;
            dbText.TextString = textContent.ToString();
            this.textId = ThColumnInfoUtils.AddToBlockTable(dbText)[0];
        }
        private void CreateFrame()
        {
            if(!ThColumnInfoUtils.CheckObjIdIsValid(this.frameId))
            {
                List<Point3d> pts = ThColumnInfoUtils.GetPolylinePts(this.columnId);
                this.frameId = ThColumnInfoUtils.DrawOffsetColumn(
                    pts, PlantCalDataToDraw.offsetDisScale, true, PlantCalDataToDraw.lineWidth); 
            }
        }
        public void Modify(ModifyCustomDataType modifyCustomDataType,bool reset=false)
        {
            //更新柱子外框
            //更新文字
            UpdateFrame(modifyCustomDataType, reset);
            UpdateTextContent(modifyCustomDataType, reset);
            UpdateColor(modifyCustomDataType, reset);
            ShowOrHideFrameText(modifyCustomDataType);
            PlantCalDataToDraw plantCalDataToDraw = new PlantCalDataToDraw(false);
            plantCalDataToDraw.WriteEmbededColumnCustomData(this.columnId, this.customData);
        }
        /// <summary>
        /// 更新内容
        /// </summary>
        /// <param name="colorIndex"></param>
        public void Update(ModifyCustomDataType modifyCustomDataType)
        {
            //更新柱子外框
            //更新文字
            UpdateFrame(modifyCustomDataType);
            UpdateTextContent(modifyCustomDataType);
            UpdateColor(modifyCustomDataType);
            ShowOrHideFrameText(modifyCustomDataType);
        }
        private void UpdateColor(ModifyCustomDataType modifyCustomDataType, bool reset = false)
        {
            short colorIndex = GetColorIndex(modifyCustomDataType);
            if(colorIndex>0)
            {
                ThColumnInfoUtils.ChangeColor(this.textId, colorIndex);
                ThColumnInfoUtils.ChangeColor(this.frameId, colorIndex);
            }
            else
            {
                if(reset)
                {
                    ThColumnInfoUtils.ChangeColor(this.textId, 3);
                    ThColumnInfoUtils.ChangeColor(this.frameId, 3);
                }
            }
        }
        /// <summary>
        /// 更新文字内容
        /// </summary>
        /// <param name="modifyCustomDataType"></param>
        private void UpdateTextContent(ModifyCustomDataType modifyCustomDataType, bool reset = false)
        {
            short colorIndex = GetColorIndex(modifyCustomDataType);
            object textContent = GetTextValue(modifyCustomDataType);
            CreateText(textContent);
            if(colorIndex > 0)
            {
                ThColumnInfoUtils.ChangeColor(this.textId, colorIndex);
            }
            else
            {
                if (reset)
                {
                    ThColumnInfoUtils.ChangeColor(this.textId, 3);
                }
            }
        }
        /// <summary>
        /// 更新柱子外框
        /// </summary>
        /// <param name="modifyCustomDataType"></param>
        private void UpdateFrame(ModifyCustomDataType modifyCustomDataType,bool reset=false)
        {
            short colorIndex = GetColorIndex(modifyCustomDataType);
            CreateFrame();
            if (colorIndex > 0)
            {
                ThColumnInfoUtils.ChangeColor(this.frameId, colorIndex);
            }
            else
            {
                if(reset)
                {
                    ThColumnInfoUtils.ChangeColor(this.frameId, 3);
                }
            }
        }
        /// <summary>
        /// 获取要显示的文字内容
        /// </summary>
        /// <param name="modifyCustomDataType"></param>
        /// <returns></returns>
        private object GetTextValue(ModifyCustomDataType modifyCustomDataType)
        {
            object textContent=null;
            switch (modifyCustomDataType)
            {
                case ModifyCustomDataType.AntiSeismicGrade:
                    textContent = this.customData.AntiSeismicGrade;
                    break;
                case ModifyCustomDataType.ConcreteStrength:
                    textContent = this.customData.ConcreteStrength;
                    break;
                case ModifyCustomDataType.CornerColumn:
                    textContent = this.customData.CornerColumn;
                    break;
                case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                    textContent = this.customData.HoopReinforceFullHeightEncryption;
                    break;
                case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                    textContent = this.customData.HoopReinforcementEnlargeTimes;
                    break;
                case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
                    textContent = this.customData.LongitudinalReinforceEnlargeTimes;
                    break;
                case ModifyCustomDataType.ProtectLayerThickness:
                    textContent = this.customData.ProtectLayerThickness;
                    break;
            }
            return textContent;
        }
        private short GetColorIndex(ModifyCustomDataType modifyCustomDataType)
        {
            short colorIndex = 0;
            switch (modifyCustomDataType)
            {
                case ModifyCustomDataType.AntiSeismicGrade:
                    colorIndex = this.customData.AntiSeismicGradeColorIndex;
                    break;
                case ModifyCustomDataType.ConcreteStrength:
                    colorIndex = this.customData.ConcreteStrengthColorIndex;
                    break;
                case ModifyCustomDataType.CornerColumn:
                    colorIndex = this.customData.CornerColumnColorIndex;
                    break;
                case ModifyCustomDataType.HoopReinforceFullHeightEncryption:
                    colorIndex = this.customData.HoopFullHeightEncryptionColorIndex;
                    break;
                case ModifyCustomDataType.HoopReinforcementEnlargeTimes:
                    colorIndex = this.customData.HoopEnlargeTimesColorIndex;
                    break;
                case ModifyCustomDataType.LongitudinalReinforceEnlargeTimes:
                    colorIndex = this.customData.LongitudinalEnlargeTimesColorIndex;
                    break;
                case ModifyCustomDataType.ProtectLayerThickness:
                    colorIndex = this.customData.ProtectThickColorIndex;
                    break;
            }
            return colorIndex;
        }
        private void ShowOrHideFrameText(ModifyCustomDataType modifyCustomDataType)
        {
            short colorIndex = GetColorIndex(modifyCustomDataType);
            if(colorIndex>0)
            {
                ShowOrHideFrameText(true);
            }
            else
            {
                ShowOrHideFrameText(false);
            }
        }
        public void ShowOrHideFrameText(bool visible)
        {
            List<ObjectId> objIds = new List<ObjectId>();
            objIds.Add(this.textId);
            objIds.Add(this.frameId);
            ThColumnInfoUtils.ShowObjIds(objIds, visible);
        }
        /// <summary>
        /// 显示或隐藏柱子原位置框线
        /// </summary>
        /// <param name="visible"></param>
        public void ShowOrHideColumn(bool visible)
        {
            ThColumnInfoUtils.ShowObjId(this.columnId, visible);
        }
        private bool JudgeTextInColumnFrame()
        {
            bool result = false;
            if (this.textId == ObjectId.Null || this.textId.IsErased || this.textId.IsValid == false)
            {
                return result;
            }
            if (this.frameId == ObjectId.Null || this.frameId.IsErased || this.frameId.IsValid == false)
            {
                return result;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Polyline column = trans.GetObject(this.frameId,OpenMode.ForRead) as Polyline;
                DBText text = trans.GetObject(this.textId, OpenMode.ForRead) as DBText;                
                if(text.Bounds.HasValue)
                {
                    Point3d firstPt = text.Bounds.Value.MinPoint; 
                    Point3d thirdPt = text.Bounds.Value.MaxPoint;
                    thirdPt = new Point3d(thirdPt.X, thirdPt.Y, firstPt.Z);
                    Point3d secondPt = new Point3d(thirdPt.X, firstPt.Y, firstPt.Z);
                    Point3d fourthPt = new Point3d(firstPt.X, thirdPt.Y, firstPt.Z);

                    Point3dCollection pts = new Point3dCollection();
                    pts.Add(firstPt);
                    pts.Add(secondPt);
                    pts.Add(thirdPt);
                    pts.Add(fourthPt);
                    Polyline rectangle= ThColumnInfoUtils.CreateRectangle(
                        text.Bounds.Value.MinPoint, text.Bounds.Value.MaxPoint);

                    ThColumnInfoUtils.JudgeTwoCurveIsOverLap(column, rectangle);

                }
                trans.Commit();
            }
            return result;
        }
    }
    public enum ModifyCustomDataType
    {
        None,
        /// <summary>
        /// 抗震等级
        /// </summary>
        AntiSeismicGrade,
        /// <summary>
        /// 保护层厚度
        /// </summary>
        ProtectLayerThickness,
        /// <summary>
        /// 混凝土强度
        /// </summary>
        ConcreteStrength,
        /// <summary>
        /// 箍筋放大倍数
        /// </summary>
        HoopReinforcementEnlargeTimes,
        /// <summary>
        /// 纵筋放大倍数
        /// </summary>
        LongitudinalReinforceEnlargeTimes,
        /// <summary>
        /// 箍筋全高度加密
        /// </summary>
        HoopReinforceFullHeightEncryption,
        /// <summary>
        /// 是否角柱
        /// </summary>
        CornerColumn
    }
}
