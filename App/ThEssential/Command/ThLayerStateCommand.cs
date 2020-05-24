using System;
using System.IO;
using Linq2Acad;
using AcHelper.Commands;
using ThEssential.LayerState;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThEssential.Command
{
    public class ThLayerStateCommand : IAcadCommand, IDisposable
    {
        private string StateName { get; set; }
        public ThLayerStateCommand(State state)
        {
            switch (state)
            {
                case State.VENTILATE:
                    StateName = "THTF";
                    break;
                case State.PIPE:
                    StateName = "THSG";
                    break;
                case State.EXTINGUISHMENT:
                    StateName = "THXF";
                    break;
                case State.ALL:
                    StateName = "THNT";
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
            //
        }

        public void Execute()
        {
            using (AcadDatabase currentDb = AcadDatabase.Active())
            using (AcadDatabase layerStateDb = AcadDatabase.Open(LayerStateDwgPath(), DwgOpenMode.ReadOnly, false))
            using (ThLayerStateManager manager = new ThLayerStateManager(currentDb.Database))
            {
                // 检查图层状态是否已经存在
                if (string.IsNullOrEmpty(StateName))
                {
                    return;
                }
                // 若不存在，导入图层状态
                if (!manager.HasLayerState(StateName))
                {
                    manager.ImportLayerStateFromDb(StateName, layerStateDb.Database);
                }
                // 若未发现图层状态，返回
                if (!manager.HasLayerState(StateName))
                {
                    return;
                }

                // 恢复图层状态
                manager.RestoreLayerState(StateName);
            }
        }

        private string LayerStateDwgPath()
        {
            return Path.Combine(ThCADCommon.SupportPath(), "图层工具.暖通.dwg");
        }
    }
}
