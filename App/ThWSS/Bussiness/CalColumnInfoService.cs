using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.Engine;
using ThWSS.Utlis;

namespace ThWSS.Bussiness
{
    public class CalColumnInfoService
    {
        public static readonly string layerName = "Th-LayoutSpray-StandAlone-Column";
        public List<Polyline> GetColumnStruc()
        {
            List<Polyline> column = new List<Polyline>();
            var thcolumn = ThSprayLayoutEngine.Instance.RoomEngine.Elements.Where(x => x is ThColumn).ToList();
            if (thcolumn.Count > 0)
            {
                column = thcolumn.SelectMany(x => x.Properties.Values).Cast<Polyline>().ToList();
            }
            return column;
        }

        /// <summary>
        /// 计算在柱外的喷淋
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="sprays"></param>
        /// <param name="ptInColmns"></param>
        /// <returns></returns>
        public List<SprayLayoutData> CalColumnSpray(List<Polyline> columns, List<SprayLayoutData> sprays, out Dictionary<Polyline, List<SprayLayoutData>> ptInColmns)
        {
            List<SprayLayoutData> polyline = new List<SprayLayoutData>();
            ptInColmns = new Dictionary<Polyline, List<SprayLayoutData>>();
            foreach (var spray in sprays)
            {
                bool inColumn = false;
                foreach (var columnPoly in columns)
                {
                    var res = GeUtils.CheckPointInPolyline(columnPoly, spray.Position, 1.0E-4);
                    if (res != -1)
                    {
                        inColumn = true;
                        if (ptInColmns.Keys.Contains(columnPoly))
                        {
                            ptInColmns[columnPoly].Add(spray);
                        }
                        else
                        {
                            var sprayLst = new List<SprayLayoutData>();
                            sprayLst.Add(spray);
                            ptInColmns.Add(columnPoly, sprayLst);
                        }
                    }
                }
                if (!inColumn)
                {
                    polyline.Add(spray);
                }
            }
            
            return polyline;
        }

        /// <summary>
        /// 将孤立柱的polyline打入特定图层
        /// </summary>
        /// <param name="standAloneColumn"></param>
        public void SetStandAloneColumnInLayer(List<Polyline> standAloneColumn)
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                ObjectId layerId = LayerTools.AddLayer(acdb.Database, layerName);
                foreach (var columns in standAloneColumn)
                {
                    columns.SetLayerId(layerId, true);
                }
            }
        }
    }
}
