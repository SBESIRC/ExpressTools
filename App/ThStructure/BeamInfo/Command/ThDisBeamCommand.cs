using Linq2Acad;
using System.Linq;
using ThStructure.BeamInfo.Model;
using ThStructure.BeamInfo.Business;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThStructure.BeamInfo.Command
{
    public class ThDisBeamCommand
    {
        private Document document = Application.DocumentManager.MdiActiveDocument;

        public void CalBeamStruc(DBObjectCollection dBObjects)
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                //1.计算出匹配的梁
                CalBeamStruService calBeamService = new CalBeamStruService();
                var allBeam = calBeamService.GetBeamInfo(dBObjects);

                //2.计算出梁的搭接信息
                CalBeamIntersectService interService = new CalBeamIntersectService(document);
                interService.CalBeamIntersectInfo(allBeam, acdb);

                //3.计算出梁的标注信息
                GetBeamMarkInfo getBeamMark = new GetBeamMarkInfo(document, acdb);
                getBeamMark.FillBeamInfo(allBeam);

                //4.根据分跨信息合并梁
                CalSpanBeamInfo calSpanBeam = new CalSpanBeamInfo();
                calSpanBeam.FindBeamOfCentralizeMarking(ref allBeam);

                //5.填充梁的标注信息
                FillBeamInfo fillBeamInfo = new FillBeamInfo();
                fillBeamInfo.FillMarkingInfo(allBeam);




                foreach (var item in allBeam.Where(x=>x.StartIntersect == null || x.EndIntersect == null))
                {
                    acdb.ModelSpace.Add(item.BeamBoundary);
                    //acdb.ModelSpace.Add(item.BeamSPointSolid);
                    //acdb.ModelSpace.Add(item.BeamEPointSolid);
                }

                foreach (var item in allBeam)
                {
                    if (item is MergeLineBeam)
                    {
                        document.Editor.WriteMessage("这跟柱是合并后的柱 \n");
                        var s = item as MergeLineBeam;
                        foreach (var line in s.UpBeamLines)
                        {
                            document.Editor.WriteMessage("梁上边线句柄：" + line.Id.Handle.ToString() + "\n");
                        }
                        foreach (var line in s.DownBeamLines)
                        {
                            document.Editor.WriteMessage("梁下边线句柄：" + line.Id.Handle.ToString() + "\n");
                        }
                    }
                    else
                    {
                        document.Editor.WriteMessage("梁上边线句柄：" + item.UpBeamLine.Id.Handle.ToString() + "\n");
                        document.Editor.WriteMessage("梁下边线句柄：" + item.DownBeamLine.Id.Handle.ToString() + "\n");
                    }

                    string s1 = null, s2 = null;
                    if (item.StartIntersect != null && item.StartIntersect.EntityType == IntersectType.Beam)
                    {
                        s1 = "梁";
                    }
                    else if (item.StartIntersect != null && item.StartIntersect.EntityType == IntersectType.Column)
                    {
                        s1 = "柱";
                    }
                    else if (item.StartIntersect != null && item.StartIntersect.EntityType == IntersectType.Wall)
                    {
                        s1 = "墙";
                    }
                    if (item.EndIntersect != null && item.EndIntersect.EntityType == IntersectType.Beam)
                    {
                        s2 = "梁";
                    }
                    else if (item.EndIntersect != null && item.EndIntersect.EntityType == IntersectType.Column)
                    {
                        s2 = "柱";
                    }
                    else if (item.EndIntersect != null && item.EndIntersect.EntityType == IntersectType.Wall)
                    {
                        s2 = "墙";
                    }
                    document.Editor.WriteMessage("梁的两边分别搭接在：" + s1 + "和" + s2 + "上" + "\n");

                    //if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.BeamNum != null)
                    //{
                    //    document.Editor.WriteMessage("梁编号为：" + item.ThCentralizedMarkingP.BeamNum + "\n");
                    //}
                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.UpLeftSteel != null)
                    //{
                    //    document.Editor.WriteMessage("上部左支座钢筋：" + item.ThOriginMarkingcsP.UpLeftSteel + "\n");
                    //}
                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.UpRightSteel != null)
                    //{
                    //    document.Editor.WriteMessage("上部右支座钢筋：" + item.ThOriginMarkingcsP.UpRightSteel + "\n");
                    //}

                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.SectionSize != null)
                    //{
                    //    document.Editor.WriteMessage("梁尺寸：" + item.ThOriginMarkingcsP.SectionSize + "\n");
                    //}
                    //else
                    //{
                    //    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.SectionSize != null)
                    //    {
                    //        document.Editor.WriteMessage("梁尺寸：" + item.ThCentralizedMarkingP.SectionSize + "\n");
                    //    }
                    //}

                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.Hooping != null)
                    //{
                    //    document.Editor.WriteMessage("梁箍筋：" + item.ThOriginMarkingcsP.Hooping + "\n");
                    //}
                    //else
                    //{
                    //    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.Hooping != null)
                    //    {
                    //        document.Editor.WriteMessage("梁箍筋：" + item.ThCentralizedMarkingP.Hooping + "\n");
                    //    }
                    //}

                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.UpErectingBar != null)
                    //{
                    //    document.Editor.WriteMessage("梁上部通长筋或架立筋：" + item.ThOriginMarkingcsP.UpErectingBar + "\n");
                    //}
                    //else
                    //{
                    //    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.ExposedReinforcement != null)
                    //    {
                    //        document.Editor.WriteMessage("梁上部通长筋或架立筋：" + item.ThCentralizedMarkingP.ExposedReinforcement.Split(';')[0] + "\n");
                    //    }
                    //}

                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.DownErectingBar != null)
                    //{
                    //    document.Editor.WriteMessage("梁下部通长筋或架立筋：" + item.ThOriginMarkingcsP.DownErectingBar + "\n");
                    //}
                    //else
                    //{
                    //    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.ExposedReinforcement != null)
                    //    {
                    //        string[] strAry = item.ThCentralizedMarkingP.ExposedReinforcement.Split(';');
                    //        if (strAry.Length >= 2)
                    //        {
                    //            document.Editor.WriteMessage("梁下部通长筋或架立筋：" + strAry[1] + "\n");
                    //        }
                    //    }
                    //}

                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.TwistedSteel != null)
                    //{
                    //    document.Editor.WriteMessage("构造钢筋或受扭钢筋：" + item.ThOriginMarkingcsP.TwistedSteel + "\n");
                    //}
                    //else
                    //{
                    //    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.TwistedSteel != null)
                    //    {
                    //        document.Editor.WriteMessage("构造钢筋或受扭钢筋：" + item.ThCentralizedMarkingP.TwistedSteel + "\n");
                    //    }
                    //}

                    //if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.LevelDValue != null)
                    //{
                    //    document.Editor.WriteMessage("顶面标高与结构楼面标高差值：" + item.ThOriginMarkingcsP.LevelDValue + "\n");
                    //}
                    //else
                    //{
                    //    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.LevelDValue != null)
                    //    {
                    //        document.Editor.WriteMessage("顶面标高与结构楼面标高差值：" + item.ThCentralizedMarkingP.LevelDValue + "\n");
                    //    }
                    //}
                    document.Editor.WriteMessage("======================================\n");
                }
            }
        }
    }
}
