using System;
using System.Drawing;
using System.Collections.Generic;

namespace ThSitePlan.Configuration
{
    public class ThSitePlanConfigItem
    {
        public Dictionary<string, object> Properties { get; set; }
    }

    public class ThSitePlanConfigItemGroup
    {
        public List<ThSitePlanConfigItem> Items { get; set; }
        public List<ThSitePlanConfigItemGroup> Groups { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public ThSitePlanConfigItemGroup()
        {
            Items = new List<ThSitePlanConfigItem>();
            Groups = new List<ThSitePlanConfigItemGroup>();
            Properties = new Dictionary<string, object>();
        }
    }

    public class ThSitePlanConfigService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanConfigService instance = new ThSitePlanConfigService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanConfigService() { }
        internal ThSitePlanConfigService() { }
        public static ThSitePlanConfigService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public ThSitePlanConfigItemGroup Root { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            Root = new ThSitePlanConfigItemGroup();
            Root.Properties.Add("Name", "天华彩总");
            Root.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "原始场地线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "界外用地", "P-OUTD" }
                        }
                    }
                }
            });

            // 建筑物
            var building = new ThSitePlanConfigItemGroup();
            building.Properties.Add("Name", "建筑物");
            Root.Groups.Add(building);

            // 建筑物.高层建筑
            var highBuilding = new ThSitePlanConfigItemGroup();
            highBuilding.Properties.Add("Name", "高层建筑");
            // 建筑物.高层建筑.建筑信息
            highBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑信息"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "建筑层数", "P-BUID-FLOR" },
                            { "建筑高度", "P-BUID-HIGH" }
                        }
                    }
                }
            });
            // 建筑物.高层建筑.建筑线稿
            highBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "建筑最大外轮廓",    "P-BUID-BMAX" },
                            { "建筑错层看线",      "P-BUID-BCHA" },
                            { "围墙线",           "P-BUID-ROOF" }
                        }
                    }
                }
            });
            // 建筑物.高层建筑.建筑色块
            highBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "新建建筑物填充",    "P-BUID-HACH" }
                        }
                    }
                }
            });
            building.Groups.Add(highBuilding);

            // 建筑物.多层建筑
            var multiBuilding = new ThSitePlanConfigItemGroup();
            multiBuilding.Properties.Add("Name", "多层建筑");
            // 建筑物.多层建筑.建筑信息
            multiBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑信息"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "建筑层数", "P-BUID-FLOR" },
                            { "建筑高度", "P-BUID-HIGH" }
                        }
                    }
                }
            });
            // 建筑物.多层建筑.建筑线稿
            multiBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "建筑最大外轮廓",    "P-BUID-BMAX" },
                            { "建筑错层看线",      "P-BUID-BCHA" },
                            { "围墙线",           "P-BUID-ROOF" }
                        }
                    }
                }
            });
            // 建筑物.多层建筑.建筑色块
            multiBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "新建建筑物填充",    "P-BUID-HACH" }
                        }
                    }
                }
            });
            building.Groups.Add(multiBuilding);

            // 建筑物.低层建筑
            var lowBuilding = new ThSitePlanConfigItemGroup();
            lowBuilding.Properties.Add("Name", "低层建筑");
            // 建筑物.低层建筑.建筑信息
            lowBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑信息"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "建筑层数", "P-BUID-FLOR" },
                            { "建筑高度", "P-BUID-HIGH" }
                        }
                    }
                }
            });
            // 建筑物.低层建筑.建筑线稿
            lowBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "建筑最大外轮廓",    "P-BUID-BMAX" },
                            { "建筑错层看线",      "P-BUID-BCHA" },
                            { "围墙线",           "P-BUID-ROOF" }
                        }
                    }
                }
            });
            // 建筑物.低层建筑.建筑色块
            lowBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "新建建筑物填充",    "P-BUID-HACH" }
                        }
                    }
                }
            });
            building.Groups.Add(lowBuilding);

            // 建筑物.场地外建筑
            var outBuilding = new ThSitePlanConfigItemGroup();
            outBuilding.Properties.Add("Name", "场地外建筑");
            // 建筑物.场地外建筑.建筑信息
            outBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑信息"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "场地外建筑层数", "P-OUTD-FLOR" }
                        }
                    }
                }
            });
            // 建筑物.场地外建筑.建筑线稿
            outBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "场地外建筑线稿",    "P-OUTD-BUID" }
                        }
                    }
                }
            });
            // 建筑物.场地外建筑.建筑色块
            outBuilding.Items.Add(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "场地外建筑色块",    "P-OUTD-BUID" }
                        }
                    }
                }
            });
            building.Groups.Add(outBuilding);
        }

        /// <summary>
        /// 从文件中读取
        /// </summary>
        /// <param name="file"></param>
        public void LoadFromFile(string file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 导出到文件中
        /// </summary>
        /// <param name="file"></param>
        public void ExportToFile(string file)
        {
            throw new NotImplementedException();
        }
    }
}
