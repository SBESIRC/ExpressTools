using System;
using Linq2Acad;
using System.IO;
using System.Collections.Generic;
using ThStructure.BeamInfo.Model;
using ThStructure.BeamInfo.Business;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.BeamInfo.Command
{
    public class ThDisBeamCommand
    {
        public List<Beam> CalBeamStruc(DBObjectCollection dBObjects)
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                //0.预处理
                //  0.1 将多段线“炸”成线段
                var curves = ThBeamGeometryPreprocessor.ExplodeCurves(dBObjects);
                //  0.2 为了处理法向量和Z轴不平行的情况，需要将曲线投影到XY平面
                var beamCurves = ThBeamGeometryPreprocessor.ProjectXYCurves(curves);
                //  0.3 为了处理Z值不为0的情况，需要将曲线Z值设为0
                ThBeamGeometryPreprocessor.Z0Curves(ref beamCurves);

                //1.计算出匹配的梁
                CalBeamStruService calBeamService = new CalBeamStruService();
                var allBeam = calBeamService.GetBeamInfo(beamCurves);

                //TODO：
                //  需要支持识别外参中的梁的标注信息
                //2.计算出梁的搭接信息
                //CalBeamIntersectService interService = new CalBeamIntersectService(Active.Document);
                //interService.CalBeamIntersectInfo(allBeam, acdb);

                ////3.计算出梁的标注信息
                //GetBeamMarkInfo getBeamMark = new GetBeamMarkInfo(Active.Document, acdb);
                //getBeamMark.FillBeamInfo(allBeam);

                ////4.根据分跨信息合并梁
                //CalSpanBeamInfo calSpanBeam = new CalSpanBeamInfo();
                //calSpanBeam.FindBeamOfCentralizeMarking(ref allBeam, out List<Beam> divisionBeams);

                ////5.填充梁的标注信息
                //FillBeamInfo fillBeamInfo = new FillBeamInfo();
                //fillBeamInfo.FillMarkingInfo(allBeam);

                ////6.根据分跨信息分割梁
                //foreach (var beam in divisionBeams)
                //{
                //    List<Beam> dBeams = calSpanBeam.DivisionBeams(beam, allBeam);
                //    allBeam.Remove(beam);
                //    allBeam.AddRange(dBeams);
                //}

                //7.打印梁外边框
                foreach (var item in allBeam)
                {
                   // acdb.ModelSpace.Add(item.BeamBoundary);
                }

                ////8.打印梁信息
                //PrintInfo(allBeam, true);

                return allBeam;
            }
        }

        private void PrintInfo(List<Beam> allBeams, bool print)
        {
            if (print)
            {
                string result1 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                    "beaminfo.txt");
                FileStream fs = new FileStream(result1, FileMode.Append);
                StreamWriter wr = null;
                wr = new StreamWriter(fs);
                foreach (var item in allBeams)
                {
                    if (item is MergeLineBeam)
                    {
                        wr.WriteLine("这跟梁是合并后的梁 \n");
                        var s = item as MergeLineBeam;
                        foreach (var line in s.UpBeamLines)
                        {
                            wr.WriteLine("梁上边线句柄：" + line.Id.Handle.ToString() + "\n");
                        }
                        foreach (var line in s.DownBeamLines)
                        {
                            wr.WriteLine("梁下边线句柄：" + line.Id.Handle.ToString() + "\n");
                        }
                    }
                    else
                    {
                        wr.WriteLine("梁上边线句柄：" + item.UpBeamLine.Id.Handle.ToString() + "\n");
                        wr.WriteLine("梁下边线句柄：" + item.DownBeamLine.Id.Handle.ToString() + "\n");
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
                    wr.WriteLine("梁的两边分别搭接在：" + s1 + "和" + s2 + "上" + "\n");

                    if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.BeamNum != null)
                    {
                        wr.WriteLine("梁编号为：" + item.ThCentralizedMarkingP.BeamNum + "\n");
                    }
                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.UpLeftSteel != null)
                    {
                        wr.WriteLine("上部左支座钢筋：" + item.ThOriginMarkingcsP.UpLeftSteel + "\n");
                    }
                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.UpRightSteel != null)
                    {
                        wr.WriteLine("上部右支座钢筋：" + item.ThOriginMarkingcsP.UpRightSteel + "\n");
                    }

                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.SectionSize != null)
                    {
                        wr.WriteLine("梁尺寸：" + item.ThOriginMarkingcsP.SectionSize + "\n");
                    }
                    else
                    {
                        if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.SectionSize != null)
                        {
                            wr.WriteLine("梁尺寸：" + item.ThCentralizedMarkingP.SectionSize + "\n");
                        }
                    }

                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.Hooping != null)
                    {
                        wr.WriteLine("梁箍筋：" + item.ThOriginMarkingcsP.Hooping + "\n");
                    }
                    else
                    {
                        if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.Hooping != null)
                        {
                            wr.WriteLine("梁箍筋：" + item.ThCentralizedMarkingP.Hooping + "\n");
                        }
                    }

                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.UpErectingBar != null)
                    {
                        wr.WriteLine("梁上部通长筋或架立筋：" + item.ThOriginMarkingcsP.UpErectingBar + "\n");
                    }
                    else
                    {
                        if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.ExposedReinforcement != null)
                        {
                            wr.WriteLine("梁上部通长筋或架立筋：" + item.ThCentralizedMarkingP.ExposedReinforcement.Split(';')[0] + "\n");
                        }
                    }

                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.DownErectingBar != null)
                    {
                        wr.WriteLine("梁下部通长筋或架立筋：" + item.ThOriginMarkingcsP.DownErectingBar + "\n");
                    }
                    else
                    {
                        if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.ExposedReinforcement != null)
                        {
                            string[] strAry = item.ThCentralizedMarkingP.ExposedReinforcement.Split(';');
                            if (strAry.Length >= 2)
                            {
                                wr.WriteLine("梁下部通长筋或架立筋：" + strAry[1] + "\n");
                            }
                        }
                    }

                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.TwistedSteel != null)
                    {
                        wr.WriteLine("构造钢筋或受扭钢筋：" + item.ThOriginMarkingcsP.TwistedSteel + "\n");
                    }
                    else
                    {
                        if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.TwistedSteel != null)
                        {
                            wr.WriteLine("构造钢筋或受扭钢筋：" + item.ThCentralizedMarkingP.TwistedSteel + "\n");
                        }
                    }

                    if (item.ThOriginMarkingcsP != null && item.ThOriginMarkingcsP.LevelDValue != null)
                    {
                        wr.WriteLine("顶面标高与结构楼面标高差值：" + item.ThOriginMarkingcsP.LevelDValue + "\n");
                    }
                    else
                    {
                        if (item.ThCentralizedMarkingP != null && item.ThCentralizedMarkingP.LevelDValue != null)
                        {
                            wr.WriteLine("顶面标高与结构楼面标高差值：" + item.ThCentralizedMarkingP.LevelDValue + "\n");
                        }
                    }
                    wr.WriteLine("======================================\n");
                }
                wr.Close();
            }
        }
    }
}
