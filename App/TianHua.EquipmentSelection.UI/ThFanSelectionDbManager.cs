using System;
using Linq2Acad;
using DotNetARX;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.FanSelection.UI
{
    public class ThFanSelectionDbManager : IDisposable
    {
        private Database HostDb { get; set; }
        private ObjectIdCollection Geometries { get; set; }
        public Dictionary<string, List<int>> Models { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThFanSelectionDbManager(Database database)
        {
            HostDb = database;
            LoadFromDb(database);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 从图纸中提取风机图块
        /// </summary>
        /// <param name="database"></param>
        private void LoadFromDb(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                Geometries = new ObjectIdCollection();
                var blkRefs = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o =>
                    {
                        if (o.GetEffectiveName() == ThFanSelectionCommon.AXIAL_BLOCK_NAME)
                        {
                            return true;
                        }

                        if (o.GetEffectiveName() == ThFanSelectionCommon.HTFC_BLOCK_NAME)
                        {
                            return true;
                        }

                        return false;
                    });
                blkRefs.ForEachDbObject(o => Geometries.Add(o.ObjectId));
                Models = Geometries.ExtractModels();
            }
        }

        /// <summary>
        /// 是否存在风机
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool Contains(string identifier)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(HostDb))
            {
                foreach(ObjectId obj in Geometries)
                {
                    var values = obj.GetXData(ThFanSelectionCommon.RegAppName_FanSelection);
                    if (values != null)
                    {
                        var handles = values.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataAsciiString);

                    }
                }
                return false;
            }
        }
    }
}
