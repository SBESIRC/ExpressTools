using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class BlockTool
    {
        /// <summary>
        /// 从动态块的角度去获取块名
        /// </summary>
        /// <param name="bref"></param>
        /// <returns></returns>
        public static string GetRealBlockName(this BlockReference bref)
        {
            //不管原始块是不是动态块，全部都从动态块去拿其名字（仅设置可见性的块，不一定是动态块）

            string blockName;//存储块名
            if (bref == null) return null;//如果块参照不存在，则返回

            //获取动态块所属的动态块表记录
            ObjectId idDyn = bref.DynamicBlockTableRecord;

            using (var trans = idDyn.Database.TransactionManager.StartOpenCloseTransaction())
            {
                //打开动态块表记录
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(idDyn, OpenMode.ForRead);
                blockName = btr.Name;//获取块名

                trans.Commit();
            }



            return blockName;//返回块名
        }



        /// <summary>
        /// 从动态块定义的角度去获取普通块的handle
        /// </summary>
        /// <param name="btr"></param>
        /// <returns></returns>
        public static string GetNormalBlockHandle(this BlockTableRecord btr)
        {
            var result = btr.Handle.ToString();
            var xData = btr.XData;
            //如果没有扩展数据，肯定是普通块
            if (xData != null)
            {
                // Get the XData as an array of TypeValues and loop
                // through it
                var tvs = xData.AsArray();
                for (int i = 0; i < tvs.Length; i++)
                {
                    // The first value should be the RegAppName
                    var tv = tvs[i];
                    if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                    {
                        // If it's the one we care about...
                        if ((string)tv.Value == "AcDbBlockRepBTag")
                        {
                            // ... then loop through until we find a
                            // handle matching our blocks or otherwise
                            // another RegAppName
                            for (int j = i + 1; j < tvs.Length; j++)
                            {
                                tv = tvs[j];
                                if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                                {
                                    // If we have another RegAppName, then
                                    // we'll break out of this for loop and
                                    // let the outer loop have a chance to
                                    // process this section
                                    i = j - 1;
                                    break;
                                }

                                if (tv.TypeCode == (int)DxfCode.ExtendedDataHandle)
                                {
                                    return (string)tv.Value;
                                    //// If we have a matching handle...
                                    //if ((string)tv.Value == blkHand.ToString())
                                    //{
                                    //    // ... then we can add the block's name
                                    //    // to the list and break from both loops
                                    //    // (which we do by setting the outer index
                                    //    // to the end)
                                    //    blkNames.Add(btr2.Name);

                                    //    i = tvs.Length - 1;

                                    //    break;
                                    //}

                                }

                            }

                        }

                    }
                }
            }


            return result;

        }


    }
}
