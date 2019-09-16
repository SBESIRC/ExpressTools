using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcHelper;
using AcHelper.Wrappers;
using System;
using System.IO;
using Linq2Acad;
using System.Collections.Generic;
using System.Linq;

[assembly: CommandClass(typeof(THColumnInfo.THColumnInfoCommands))]
[assembly: ExtensionApplication(typeof(THColumnInfo.THColumnInfoApp))]
namespace THColumnInfo
{
    public class THColumnInfoApp : IExtensionApplication
    {
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }
    public class THColumnInfoCommands
    {
        [CommandMethod("TIANHUACAD", "THLIU", CommandFlags.Modal)]
        public void THLIUCommand()
        {
            SearchFields searchFields = new SearchFields()
            {
                ColumnRangeLayerName = "砼柱",
                ZhuGuJingLayerName = "柱箍筋",
                ZhuJiZhongMarkLayerName = "柱集中标注",
                ZhuSizeMark = "柱尺寸标注",
                ZhuYuanWeiMarkLayerName = "柱原位标注",
                ZhuMarkLeaderLayerName = "柱标注引线"
            };
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            Editor editor = document.Editor;
            PromptPointResult ppr1 = editor.GetPoint("\n选择左下角点");
            if (ppr1.Status != PromptStatus.OK)
            {
                return;
            }
            PromptCornerOptions pco = new PromptCornerOptions("\n选择右上角点：", ppr1.Value);
            pco.AllowArbitraryInput = false;
            PromptPointResult ppr2 = editor.GetCorner(pco);
            if (ppr2.Status != PromptStatus.OK)
            {
                return;
            }
            TypedValue[] tvs = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,searchFields.ColumnRangeLayerName),
                 new TypedValue((int)DxfCode.Start,"POLYLINE")
            };

            SelectionFilter sf = new SelectionFilter(tvs);
            //把框选范围内的柱子全部找到
            PromptSelectionResult psr = editor.SelectCrossingWindow(ppr1.Value, ppr2.Value, sf);

            if (psr.Status != PromptStatus.OK)
            {
                return;
            }

            //获取框选范围内柱集中标注文字高度
            TypedValue[] tvs1 = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,searchFields.ZhuJiZhongMarkLayerName),
                new TypedValue((int)DxfCode.Start,"TEXT")
            };
            SelectionFilter sf1 = new SelectionFilter(tvs1);

            //PromptSelectionResult psr1 = editor.SelectAll(sf1);
            PromptSelectionResult psr1 = editor.SelectCrossingWindow(ppr1.Value, ppr2.Value, sf1);

            List<ObjectId> zhuJiZhongMarkTextIds = new List<ObjectId>(); 
            if (psr1.Status == PromptStatus.OK)
            {
                zhuJiZhongMarkTextIds = psr1.Value.GetObjectIds().ToList();
            }

            //获取框选范围内柱原位标注文字高度
            TypedValue[] tvs2 = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,searchFields.ZhuYuanWeiMarkLayerName),
                new TypedValue((int)DxfCode.Start,"TEXT")
            };
            SelectionFilter sf2 = new SelectionFilter(tvs2);

            //PromptSelectionResult psr2 = editor.SelectAll(sf2);
            PromptSelectionResult psr2 = editor.SelectCrossingWindow(ppr1.Value, ppr2.Value, sf2);

            List<ObjectId> zhuYuanWeiMarkTextIds= new List<ObjectId>();
            if (psr2.Status == PromptStatus.OK)
            {
                zhuYuanWeiMarkTextIds = psr2.Value.GetObjectIds().ToList();
            }

            List<ObjectId> columnOutlineIds = psr.Value.GetObjectIds().ToList();
            if (zhuJiZhongMarkTextIds.Count > 0)
            {
                List<DBText> dbTexts = zhuJiZhongMarkTextIds.Select(i => ThColumnInfDbUtils.GetEntity(database,i) as DBText).ToList();
                double textHeight = dbTexts.OrderByDescending(i => i.Height).First().Height;
                searchFields.ZhuJiZhongMarkTextSize = textHeight;
            }

            if (zhuYuanWeiMarkTextIds.Count > 0)
            {
                List<DBText> dbTexts = zhuYuanWeiMarkTextIds.Select(i => ThColumnInfDbUtils.GetEntity(database, i) as DBText).ToList();
                double textHeight = dbTexts.OrderByDescending(i => i.Height).First().Height;
                searchFields.ZhuYuanWeiMarkTextSize = textHeight;
            }

            for (int i = 0; i < columnOutlineIds.Count; i++)
            {
                Polyline2d polyline = ThColumnInfDbUtils.GetEntity(database, columnOutlineIds[i]) as Polyline2d;
                ColumnInfReorganize columnInfReorganize = new ColumnInfReorganize(polyline, searchFields);
                columnInfReorganize.Collect();
            }
        }
    }
}
