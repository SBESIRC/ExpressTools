using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine {
      /// <summary>
      /// 解构图集生成器
      /// </summary>
      public class ThSitePlanContentGenerator : ThSitePlanGenerator {
            public override ObjectId OriginFrame { get; set; }
            public override Tuple<ObjectId, Vector3d> Frame { get; set; }
            public ThSitePlanContentGenerator () {
                  //
            }
            public override bool Generate (Database database, ThSitePlanConfigItem configItem) {
                  var options = new ThSitePlanOptions()
                  {
                        Options = new Dictionary<string, object>() {
                            { "Frame", Frame.Item1 },
                            { "Offset", Frame.Item2 },
                            { "OriginFrame", OriginFrame },
                        }
                  };

                  var scriptId = configItem.Properties["CADScriptID"].ToString ();
                  if (scriptId == "0")
                  {
                        //如果CAD脚本为0，即为线稿图框，直接从原始复制图框将相应的图形元素移动到当前图框中
                        var worker = new ThSitePlanMoveWorker ();
                        worker.DoProcess (database, configItem, options);
                  } else if (scriptId == "1" || scriptId == "2" || scriptId == "4")
                  {
                        ////如果CAD脚本为1，即为区域填充图框，直接从原始复制图框将相应的图形元素移动到当前图框中
                        //using (AcadDatabase acadDatabase = AcadDatabase.Active ()) {
                        //      //查找当前item对应的分组
                        //      ThSitePlanConfigService.Instance.Initialize ();
                        //      var currentgroup = ThSitePlanConfigService.Instance.FindGroupByItemName (configItem.Properties["Name"].ToString ());

                        //      //获取当前色块填充图框
                        //      ThSitePlanDbEngine.Instance.Initialize (Active.Database);
                        //      var currenthatchframe = ThSitePlanDbEngine.Instance.FrameByName (configItem.Properties["Name"].ToString ());

                        //      //遍历当前group，逐一获取group中各个item的图框，将图框中的所有色块图层的元素拷贝到当前色块图框
                        //      foreach (var item in currentgroup.Items) {
                        //            if (item is ThSitePlanConfigItem it) {
                        //                  //获取item的图框
                        //                  var groupitemframe = ThSitePlanDbEngine.Instance.FrameByName (it.Properties["Name"].ToString ());
                        //                  if (groupitemframe.IsNull) {
                                            
                        //                  }

                        //                  //计算该图框与当前色块图框的vector3d
                        //                  Vector3d frameoffset = acadDatabase.Database.FrameOffset (currenthatchframe, groupitemframe);

                        //                  //将item图框中的元素拷贝到当前色块图框
                        //                  var newoptions = new ThSitePlanOptions () {
                        //                        Options = new Dictionary<string, object> () { { "Frame", currenthatchframe }, { "Offset", frameoffset }, { "OriginFrame", groupitemframe },
                        //                        }
                        //                  };
                        //                  var itemworker = new ThSitePlanCopyWorker ();
                        //                  itemworker.DoProcess (database, configItem, newoptions);
                        //            }
                        //      }
                        //}
                  } else if (scriptId == "3") {
                        //如果CAD脚本为3，即为阴影图框，直接从原始复制图框将相应的图形元素移动到当前图框中
                        var worker = new ThSitePlanMoveWorker ();
                        worker.DoProcess (database, configItem, options);
                  } else {
                        throw new NotSupportedException ();
                  }

                  return true;
            }
      }
}