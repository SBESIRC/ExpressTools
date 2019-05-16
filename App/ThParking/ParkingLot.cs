using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.Parking
{
    public class ParkingLot
    {
        public BlockReference Ent { get; set; }//车位实体
        public Polyline Boundry { get; set; }//实体边界
        public List<DBText> Numbers { get; set; }//车位编号
        public int Count { get; set; }//编号次数

        public ParkingLot(BlockReference blockReference, int count)
        {
            this.Ent = blockReference;
            this.Boundry = GetWaiJieRec();
            this.Count = count;
        }

        /// <summary>
        ///为车位赋值编号
        /// </summary>
        /// <param name="number"></param>
        public void SetNumber(string number, ThParkingManager manager)
        {
            //复制对象
            var cloneBoundry = (Polyline)this.Boundry.Clone();
            //转成水平的
            cloneBoundry.TransformBy(this.Ent.BlockTransform.Inverse());

            //找到块定义中的中心位置
            var position = cloneBoundry.GetCenter().toPoint3d();
            //回到世界坐标系求出中心
            position = position.TransformBy(this.Ent.BlockTransform);

            //在世界坐标系下，实例化第一个编号，中心为上述中心,旋转角度为块参照的旋转角度（后面这个不知道对不对，再看看）
            //var baseRotation=this.Ent.Normal.Z>0?
            var baseText = new DBTextEx(number, position, manager.NumberHeight, manager.NumberLayerName, this.Ent.Rotation);

            //接下来开始实现在世界坐标系下的，多个编号的偏移
            var numbers = new List<DBText>();
            for (int i = 0; i < Count; i++)
            {
                //System.Windows.Forms.MessageBox.Show(Math.Cos(baseText.Rotation).ToString());
                //计算当前编号，基于其实编号的偏移量，确定中心位置
                var addx = Math.Abs(Math.Sin(baseText.Rotation) * manager.OffsetDis * i);

                //第一、第三象限都是负的
                addx = (baseText.Rotation <= Math.PI / 2 && baseText.Rotation >= 0) || (baseText.Rotation <= Math.PI * 1.5 && baseText.Rotation >= Math.PI) ? -addx : addx;
                var bcsPosition = new Point3d(position.X + addx, position.Y + Math.Abs(Math.Cos(baseText.Rotation) * manager.OffsetDis * i), 0);
                //var bcsPosition = new Point3d(position.X + this.Ent.Normal.X * manager.OffsetDis * i, position.Y + this.Ent.Normal.Y * manager.OffsetDis * i, 0);

                //实例化第一个编号,中心位置如上述，旋转角度跟随块参照
                var gg = new DBTextEx((Convert.ToInt32(number) + i).ToString(), bcsPosition, manager.NumberHeight, manager.NumberLayerName, this.Ent.Rotation);

                //如果文字的旋转角度为[0,90]或者[270,360)，文字的显示就是我们要的向上或者向左，其他情况减掉180翻过来
                if (!((gg.Rotation <= Math.PI / 2 + 0.001 && gg.Rotation >= 0 - 0.001) || (gg.Rotation < Math.PI * 2 + 0.001 && gg.Rotation >= Math.PI * 1.5 - 0.001)))
                {
                    gg.Rotation -= Math.PI;
                }

                numbers.Add(gg);
            }
            this.Numbers = numbers;

            //var numbers = new List<DBText>();
            //for (int i = 0; i < Count; i++)
            //{
            //    //在块定义的坐标系下进行向上偏移值的设定
            //    var bcsPosition = new Point3d(position.X, position.Y + manager.OffsetDis * i, 0);
            //    //实例化第一个编号
            //    var gg = new DBTextEx((Convert.ToInt32(number) + i).ToString(), bcsPosition, manager.NumberHeight, manager.NumberLayerName);
            //    //将编号对应的块参照的实际位置变回去
            //    gg.TransformBy(this.Ent.BlockTransform);

            //    //如果文字的旋转角度为[0,90]或者(270,360)，文字的显示就是我们要的向上或者向左，其他情况减掉180翻过来
            //    if (!((gg.Rotation <= Math.PI / 2 && gg.Rotation >= 0) || (gg.Rotation < Math.PI * 2 && gg.Rotation > Math.PI * 1.5)))
            //    {
            //        gg.Rotation -= Math.PI;
            //    }


            //    numbers.Add(gg);
            //}
            //this.Numbers = numbers;



            #region 在块定义下，实现的车位多编号，除了镜像都是正确的
            //****希望实现的效果，是文字和块参照永远呈水平，但存在原始块本身就不是水平的状态，故不能以块参照的角度决定文字角度，而是要以块定义和块参照共同决定文字角度

            ////复制对象
            //var cloneBoundry = (Polyline)this.Boundry.Clone();
            ////转成水平的
            //cloneBoundry.TransformBy(this.Ent.BlockTransform.Inverse());

            ////编号位置为块定义的外边接中心
            //var position = cloneBoundry.GetCenter().toPoint3d();

            //var numbers = new List<DBText>();
            //for (int i = 0; i < Count; i++)
            //{
            //    //在块定义的坐标系下进行向上偏移值的设定
            //    var bcsPosition = new Point3d(position.X, position.Y + manager.OffsetDis * i, 0);
            //    //实例化第一个编号
            //    var gg = new DBTextEx((Convert.ToInt32(number) + i).ToString(), bcsPosition, manager.NumberHeight, manager.NumberLayerName);
            //    //将编号对应的块参照的实际位置变回去
            //    gg.TransformBy(this.Ent.BlockTransform);

            //    //如果文字的旋转角度为[0,90]或者(270,360)，文字的显示就是我们要的向上或者向左，其他情况减掉180翻过来
            //    if (!((gg.Rotation <= Math.PI / 2 && gg.Rotation >= 0) || (gg.Rotation < Math.PI * 2 && gg.Rotation > Math.PI * 1.5)))
            //    {
            //        gg.Rotation -= Math.PI;
            //    }

            //    numbers.Add(gg);
            //}
            //this.Numbers = numbers; 
            #endregion

        }

        /// <summary>
        /// 求块参照的外边界
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private Polyline GetWaiJieRec()
        {
            //var angel = block.Rotation;
            ////如果有角度就需要旋转

            //复制对象
            var cloneBlock = (BlockReference)this.Ent.Clone();
            //转成水平的
            cloneBlock.TransformBy(this.Ent.BlockTransform.Inverse());

            //Polyline poly = new Polyline();
            //poly.CreateRectangle(cloneBlock.GeometricExtents.MinPoint.toPoint2d(), cloneBlock.GeometricExtents.MaxPoint.toPoint2d());

            var poly = new PolylineRec(cloneBlock.GeometricExtents.MinPoint.toPoint2d(), cloneBlock.GeometricExtents.MaxPoint.toPoint2d());

            //转回去
            poly.TransformBy(this.Ent.BlockTransform);

            return poly;

        }


    }
}
