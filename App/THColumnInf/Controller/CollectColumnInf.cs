using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using THColumnInfo.View;

namespace THColumnInfo.Controller
{
    public class CollectColumnInf
    {
        public static List<ColumnInf> CollectInfs()
        {
            List<ColumnInf> columnInfs = new List<ColumnInf>();

#if ACAD2012
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
#else
            Application.DocumentManager.MdiActiveDocument.Window.Focus();
#endif
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            Editor editor = document.Editor;
            PromptPointResult ppr1 = editor.GetPoint("\n选择左下角点");
            if (ppr1.Status != PromptStatus.OK)
            {
                return columnInfs;
            }
            PromptCornerOptions pco = new PromptCornerOptions("\n选择右上角点：", ppr1.Value);
            pco.AllowArbitraryInput = false;
            PromptPointResult ppr2 = editor.GetCorner(pco);
            if (ppr2.Status != PromptStatus.OK)
            {
                return columnInfs;
            }
            TypedValue[] tvs = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,CheckResult._searchFields.ColumnRangeLayerName),
                 new TypedValue((int)DxfCode.Start,"POLYLINE")
            };

            SelectionFilter sf = new SelectionFilter(tvs);
            //把框选范围内的柱子全部找到
            PromptSelectionResult psr = editor.SelectCrossingWindow(ppr1.Value, ppr2.Value, sf);

            if (psr.Status != PromptStatus.OK)
            {
                return columnInfs;
            }

            //获取框选范围内柱集中标注文字高度
            TypedValue[] tvs1 = new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,CheckResult._searchFields.ZhuJiZhongMarkLayerName),
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
                new TypedValue((int)DxfCode.LayerName,CheckResult._searchFields.ZhuYuanWeiMarkLayerName),
                new TypedValue((int)DxfCode.Start,"TEXT")
            };
            SelectionFilter sf2 = new SelectionFilter(tvs2);

            //PromptSelectionResult psr2 = editor.SelectAll(sf2);
            PromptSelectionResult psr2 = editor.SelectCrossingWindow(ppr1.Value, ppr2.Value, sf2);

            List<ObjectId> zhuYuanWeiMarkTextIds = new List<ObjectId>();
            if (psr2.Status == PromptStatus.OK)
            {
                zhuYuanWeiMarkTextIds = psr2.Value.GetObjectIds().ToList();
            }

            List<ObjectId> columnOutlineIds = psr.Value.GetObjectIds().ToList();
            if (zhuJiZhongMarkTextIds.Count > 0)
            {
                List<DBText> dbTexts = zhuJiZhongMarkTextIds.Select(i => ThColumnInfDbUtils.GetEntity(database, i) as DBText).ToList();
                double textHeight = dbTexts.OrderByDescending(i => i.Height).First().Height;
                CheckResult._searchFields.ZhuJiZhongMarkTextSize = textHeight;
            }

            if (zhuYuanWeiMarkTextIds.Count > 0)
            {
                List<DBText> dbTexts = zhuYuanWeiMarkTextIds.Select(i => ThColumnInfDbUtils.GetEntity(database, i) as DBText).ToList();
                double textHeight = dbTexts.OrderByDescending(i => i.Height).First().Height;
                CheckResult._searchFields.ZhuYuanWeiMarkTextSize = textHeight;
            }

            for (int i = 0; i < columnOutlineIds.Count; i++)
            {
                Polyline2d polyline = ThColumnInfDbUtils.GetEntity(database, columnOutlineIds[i]) as Polyline2d;
                ColumnInfReorganize columnInfReorganize = new ColumnInfReorganize(polyline, CheckResult._searchFields);
                columnInfReorganize.Collect();
                columnInfs.Add(columnInfReorganize.ColumnInfs);
            }
            return columnInfs;
        }
    }
}
