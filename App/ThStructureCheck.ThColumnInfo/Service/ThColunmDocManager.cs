using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Service
{
    /// <summary>
    /// 管理有柱子的图纸
    /// </summary>
    public class ThColunmDocManager
    {
        public static List<ThStandardSignManager> ThStandardSignManagerCollection = new List<ThStandardSignManager>();
        public static void AddThStandardSignManager(string docFullPath)
        {
            var res= ThStandardSignManagerCollection.Where(o => o.DocPath == docFullPath);
            if(res.Count()>0)
            {
                return;
            }
            ThStandardSignManagerCollection.Add(ThStandardSignManager.LoadData());
        }
        public static ThStandardSignManager GetThStandardSignManager(string docFullPath)
        {
            return ThStandardSignManagerCollection.Where(o => o.DocPath == docFullPath).FirstOrDefault();
        }
        public static void DeleteThStandardSignManager(string docFullPath)
        {
            if (string.IsNullOrEmpty(docFullPath))
            {
                return;
            }
            var res= ThStandardSignManagerCollection.Where(o => o.DocPath == docFullPath).ToList();
            res.ForEach(o => ThStandardSignManagerCollection.Remove(o));
        }
        public static bool IsExisted(string docFullPath)
        {
            var res = ThStandardSignManagerCollection.Where(o => o.DocPath == docFullPath);
            if (res.Count() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
