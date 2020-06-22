using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructure.BeamInfo.Model;
using ThStructureCheck.Common;

namespace ThStructureCheck.ThBeamInfo.Model
{
    public class BeamDistinguishInfo
    {
        #region----------Field-------------    
        private Beam beam;
        #endregion
        #region----------Property----------
        private Beam BeamEntity => beam;
        /// <summary>
        /// 梁编号
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 第几跨
        /// </summary>
        public string Span { get; set; } = "";
        /// <summary>
        /// 截面尺寸
        /// </summary>
        public string Spec { get; set; } = "";
        /// <summary>
        /// 梁箍筋
        /// </summary>
        public string BeamStirrup { get; set; } = "";
        /// <summary>
        /// 上部通长筋
        /// </summary>
        public string UpFullLengthTendon { get; set; } = "";
        /// <summary>
        /// 下部通长筋
        /// </summary>
        public string DownFullLengthTendon { get; set; } = "";
        /// <summary>
        /// 左(上)支座上部钢筋
        /// </summary>
        public string LeftOrUpSupportUpPartSteelBar { get; set; } = "";
        /// <summary>
        /// 右(下)支座上部钢筋
        /// </summary>
        public string RightOrDownSupportUpPartSteelBar { get; set; } = "";
        /// <summary>
        /// 梁侧钢筋
        /// </summary>
        public string BeamSideSteelBar { get; set; } = "";
        /// <summary>
        /// 梁顶标高差值
        /// </summary>
        public string BeamTopElevationDiffer { get; set; } = "";
        #endregion
        public BeamDistinguishInfo(Beam beam)
        {
            this.beam = beam;
            Init();
        }
        private void Init()
        {
            if(this.beam==null)
            {
                return;
            }            
            if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.BeamNum != null)
            {
                this.Code = this.beam.ThCentralizedMarkingP.BeamNum;
            }
            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.UpLeftSteel != null)
            {
                this.LeftOrUpSupportUpPartSteelBar = this.beam.ThOriginMarkingcsP.UpLeftSteel;
            }
            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.UpRightSteel != null)
            {
                this.RightOrDownSupportUpPartSteelBar = this.beam.ThOriginMarkingcsP.UpRightSteel;
            }
            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.SectionSize != null)
            {
                this.Spec = this.beam.ThOriginMarkingcsP.SectionSize;
            }
            else
            {
                if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.SectionSize != null)
                {
                    this.Spec = this.beam.ThCentralizedMarkingP.SectionSize;
                }
            }
            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.Hooping != null)
            {
                this.BeamStirrup = this.beam.ThOriginMarkingcsP.Hooping;
            }
            else
            {
                if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.Hooping != null)
                {
                    this.BeamStirrup = this.beam.ThCentralizedMarkingP.Hooping;
                }
            }

            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.UpErectingBar != null)
            {
                this.UpFullLengthTendon = this.beam.ThOriginMarkingcsP.UpErectingBar;
            }
            else
            {
                if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.ExposedReinforcement != null)
                {
                    this.UpFullLengthTendon = this.beam.ThCentralizedMarkingP.ExposedReinforcement;
                }
            }

            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.DownErectingBar != null)
            {
                this.DownFullLengthTendon = this.beam.ThOriginMarkingcsP.DownErectingBar;                
            }
            else
            {
                if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.ExposedReinforcement != null)
                {
                    string[] strAry = this.beam.ThCentralizedMarkingP.ExposedReinforcement.Split(';');
                    if (strAry.Length >= 2)
                    {
                        this.DownFullLengthTendon = strAry[1];
                    }
                }
            }

            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.TwistedSteel != null)
            {
                this.BeamSideSteelBar = this.beam.ThOriginMarkingcsP.TwistedSteel;                
            }
            else
            {
                if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.TwistedSteel != null)
                {
                    this.BeamSideSteelBar = this.beam.ThCentralizedMarkingP.TwistedSteel;
                }
            }

            if (this.beam.ThOriginMarkingcsP != null && this.beam.ThOriginMarkingcsP.LevelDValue != null)
            {
                this.BeamTopElevationDiffer = this.beam.ThOriginMarkingcsP.LevelDValue;
            }
            else
            {
                if (this.beam.ThCentralizedMarkingP != null && this.beam.ThCentralizedMarkingP.LevelDValue != null)
                {
                    this.BeamTopElevationDiffer = this.beam.ThCentralizedMarkingP.LevelDValue;
                }
            }
        }
        private short colorIndex = 3;
        public void DrawOutline()
        {
            if(this.beam==null)
            {
                return;
            }
            if(this.beam.BeamSPointSolid != null && !this.beam.BeamSPointSolid.IsDisposed)
            {
                Polyline startOutLine = this.beam.BeamSPointSolid.Clone() as Polyline;
                startOutLine.ColorIndex = colorIndex;
                CadTool.AddToBlockTable(startOutLine);
            }
            if (this.beam.BeamBoundary != null && !this.beam.BeamBoundary.IsDisposed)
            {
                Polyline beamBoundary = this.beam.BeamBoundary.Clone() as Polyline;
                beamBoundary.ColorIndex = colorIndex;
                CadTool.AddToBlockTable(beamBoundary);
            }
            if (this.beam.BeamEPointSolid != null && !this.beam.BeamEPointSolid.IsDisposed)
            {
                Polyline endOutLine = this.beam.BeamEPointSolid.Clone() as Polyline;
                endOutLine.ColorIndex = colorIndex;
                CadTool.AddToBlockTable(endOutLine);
            }
        }
    }
}
