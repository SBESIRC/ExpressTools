using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class ThStandardSignManager:IEnumerable
    {
        private List<BlockReference> brs = new List<BlockReference>();
        List<ThStandardSign> standardSigns=new List<ThStandardSign>();
        private string documentName = "";
        private string docFullPath = "";
        public string DocName
        {
            get
            {
                return documentName;
            }
        }

        public string DocPath
        {
            get
            {
                return docFullPath;
            }
        }
        public int StartPoint { get; set; } = 0;
        public List<ThStandardSign> StandardSigns
        {
            get
            {
                return standardSigns;
            }
            private set
            {
                standardSigns = value;
            }
        }

        public ThStandardSignManager(List<BlockReference> brs,string docFullPath)
        {
            this.brs = brs;
            this.docFullPath = docFullPath;
            this.documentName = Path.GetFileNameWithoutExtension(docFullPath);
            this.brs.ForEach(i => this.standardSigns.Add(new ThStandardSign(i)));
            this.standardSigns = this.standardSigns.Where(i => i.IsValid).Select(i => i).ToList();
            this.standardSigns.Sort(new InnerFrameNameDesc());
        }
        public ThStandardSignManager()
        {
            this.brs = new List<BlockReference>();
            this.documentName = "";
        }
        public void CreateAllSigns()
        {
            this.standardSigns.ForEach(i=> {
                ExtractColumnPosition extractColumnPosition = new ExtractColumnPosition(i);
                extractColumnPosition.Extract();
            });
        }
        private void CreateSingleSign(string innerFrameName = "")
        {
            if(innerFrameName=="")
            {
               return;
            }
            ThStandardSign thStandardSign= this.standardSigns.Where(i => i.InnerFrameName == innerFrameName).Select(i => i).First();
            ExtractColumnPosition extractColumnPosition = new ExtractColumnPosition(thStandardSign);
            extractColumnPosition.Extract(); 
        }
        public static void UpdateThStandardSign(ThStandardSign thStandardSign)
        {
            if(thStandardSign==null)
            {
                return;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = doc.LockDocument())
            {
                ExtractColumnPosition extractColumnPosition = new ExtractColumnPosition(thStandardSign);
                extractColumnPosition.Extract();
            }     
        }
        public IEnumerator GetEnumerator()
        {
            for(int index=0;index<this.standardSigns.Count;index++)
            {
                yield return this.standardSigns[(index + StartPoint) % this.standardSigns.Count];
            }            
        }
        /// <summary>
        /// 加载内框的数据
        /// </summary>
        /// <param name="innerFrameName">内框名称</param>
        /// <param name="loadAll">为true,则加载所有内框；为false,则加载指定innerFrameName中的数据</param>
        /// <returns></returns>
        public static ThStandardSignManager LoadData(string innerFrameName="",bool loadAll=false)
        {
            ThStandardSignManager tm=new ThStandardSignManager();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = doc.LockDocument())
            {
                List<string> lockedLayerNames = ThColumnInfoUtils.UnlockedAllLayers();
                try
                {
                    Editor ed = doc.Editor;
                    TypedValue[] tvs = new TypedValue[]
                    {
                new TypedValue((int)DxfCode.Start,"Insert")
                    };
                    SelectionFilter sf = new SelectionFilter(tvs);
                    PromptSelectionResult psr = ed.SelectAll(sf);
                    if (psr.Status != PromptStatus.OK)
                    {
                        return tm;
                    }
                    List<BlockReference> brs = new List<BlockReference>();
                    using (Transaction trans = doc.TransactionManager.StartTransaction())
                    {
                        List<ObjectId> selectBrIds = psr.Value.GetObjectIds().ToList();
                        selectBrIds.ForEach(i => brs.Add(trans.GetObject(i, OpenMode.ForRead) as BlockReference));
                        trans.Commit();
                    }
                    FileInfo fi = new FileInfo(doc.Name);
                    if (fi.Exists)
                    {
                        tm = new ThStandardSignManager(brs, doc.Name);
                        if (loadAll)
                        {
                            tm.CreateAllSigns();
                        }
                        else
                        {
                            tm.CreateSingleSign(innerFrameName);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "LoadData");
                }
                finally
                {
                    if (lockedLayerNames.Count > 0)
                    {
                        ThColumnInfoUtils.LockedLayers(lockedLayerNames);
                    }
                }
            }
            return tm;
        }
        public static ThStandardSignManager LoadData()
        {
            ThStandardSignManager tm = new ThStandardSignManager();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = doc.LockDocument())
            {
                List<string> lockedLayerNames = ThColumnInfoUtils.UnlockedAllLayers();
                try
                {
                    Editor ed = doc.Editor;
                    TypedValue[] tvs = new TypedValue[]
                    {
                new TypedValue((int)DxfCode.Start,"Insert")
                    };
                    SelectionFilter sf = new SelectionFilter(tvs);
                    PromptSelectionResult psr = ed.SelectAll(sf);
                    if (psr.Status != PromptStatus.OK)
                    {
                        return tm;
                    }
                    List<BlockReference> brs = new List<BlockReference>();
                    using (Transaction trans = doc.TransactionManager.StartTransaction())
                    {
                        List<ObjectId> selectBrIds = psr.Value.GetObjectIds().ToList();
                        selectBrIds.ForEach(i => brs.Add(trans.GetObject(i, OpenMode.ForRead) as BlockReference));
                        trans.Commit();
                    }
                    FileInfo fi = new FileInfo(doc.Name);
                    if (fi.Exists)
                    {
                        tm = new ThStandardSignManager(brs, doc.Name);
                    }
                }
                catch (System.Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "LoadData");
                }
                finally
                {
                    if (lockedLayerNames.Count > 0)
                    {
                        ThColumnInfoUtils.LockedLayers(lockedLayerNames);
                    }
                }
            }
            return tm;
        }
    }
}
