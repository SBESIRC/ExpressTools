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
            Root.Groups.Add(ConstructBuilding());

            // 树木
            Root.Groups.Add(ConstructTree());

            // 场地
            Root.Groups.Add(ConstructSites());

            // 道路
            Root.Groups.Add(ConstructRoads());
        }

        private ThSitePlanConfigItemGroup ConstructBuilding()
        {
            var building = new ThSitePlanConfigItemGroup();
            building.Properties.Add("Name", "建筑物");
            {
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
                            { "建筑高度", "P-BUID-HIGH" },
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
            }
            return building;
        }

        private ThSitePlanConfigItemGroup ConstructTree()
        {
            var tree = new ThSitePlanConfigItemGroup();
            tree.Properties.Add("Name", "树木");
            {
                //树木.景观树
                var landscape_tree = new ThSitePlanConfigItemGroup();
                landscape_tree.Properties.Add("Name", "景观树");
                // 树木.景观树.树木线稿
                landscape_tree.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "景观树木线稿", ThSitePlanCommon.LAYER_LANDSCAPE_TREE },
                        }
                    }
                }
                });
                // 树木.景观树.树木色块
                landscape_tree.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "景观树木色块", ThSitePlanCommon.LAYER_LANDSCAPE_TREE }
                        }
                    }
                }
                });
                tree.Groups.Add(landscape_tree);
            }

            // 树木.行道树
            {
                var street_tree = new ThSitePlanConfigItemGroup();
                street_tree.Properties.Add("Name", "行道树");
                // 树木.行道树.树木线稿
                street_tree.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "行道树线稿", ThSitePlanCommon.LAYER_STREET_TREE },
                        }
                    }
                }
                });
                // 树木.行道树.树木色块
                street_tree.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "行道树色块", ThSitePlanCommon.LAYER_STREET_TREE }
                        }
                    }
                }
                });
                tree.Groups.Add(street_tree);
            }
            return tree;
        }

        private ThSitePlanConfigItemGroup ConstructSites()
        {
            var sites = new ThSitePlanConfigItemGroup();
            sites.Properties.Add("Name", "场地");
            {
                //场地.消防登高场地
                var fire_site = new ThSitePlanConfigItemGroup();
                fire_site.Properties.Add("Name", "消防登高场地");
                // 场地.消防登高场地.场地线稿
                fire_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "消防场地线稿", ThSitePlanCommon.LAYER_SITE_FIRE },
                        }
                    }
                }
                });
                // 场地.消防登高场地.场地色块
                fire_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "消防场地色块", ThSitePlanCommon.LAYER_SITE_FIRE }
                        }
                    }
                }
                });
                sites.Groups.Add(fire_site);
            }

            {
                //场地.停车场地
                var parking_site = new ThSitePlanConfigItemGroup();
                parking_site.Properties.Add("Name", "停车场地");
                // 场地.停车场地.场地线稿
                parking_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "停车场地线稿", ThSitePlanCommon.LAYER_SITE_PARKING },
                        }
                    }
                }
                });
                // 场地.停车场地.场地色块
                parking_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "停车场地色块", ThSitePlanCommon.LAYER_SITE_PARKING }
                        }
                    }
                }
                });
                sites.Groups.Add(parking_site);
            }

            {
                //场地.活动场地
                var parking_site = new ThSitePlanConfigItemGroup();
                parking_site.Properties.Add("Name", "活动场地");
                // 场地.活动场地.场地线稿
                parking_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "活动场地线稿", ThSitePlanCommon.LAYER_SITE_ACTIVITY },
                        }
                    }
                }
                });
                // 场地.活动场地.场地色块
                parking_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "活动场地色块", ThSitePlanCommon.LAYER_SITE_ACTIVITY }
                        }
                    }
                }
                });
                sites.Groups.Add(parking_site);
            }

            {
                //场地.其他场地
                var parking_site = new ThSitePlanConfigItemGroup();
                parking_site.Properties.Add("Name", "其他场地");
                // 场地.其他场地.场地线稿
                parking_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地线稿"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "其他场地线稿", ThSitePlanCommon.LAYER_SITE_MISCELLANEOUS },
                        }
                    }
                }
                });
                // 场地.其他场地.场地色块
                parking_site.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地色块"},
                    { "Color", new Color()},
                    { "Transparency", 0 },
                    { "CADFrame", "" },
                    { "CADLayer", new  Dictionary<string, string>()
                        {
                            { "其他场地色块", ThSitePlanCommon.LAYER_SITE_MISCELLANEOUS }
                        }
                    }
                }
                });
                sites.Groups.Add(parking_site);
            }

            return sites;
        }


        private ThSitePlanConfigItemGroup ConstructRoads()
        {
            var roads = new ThSitePlanConfigItemGroup();
            roads.Properties.Add("Name", "道路");
            {
                var internal_road = new ThSitePlanConfigItemGroup();
                internal_road.Properties.Add("Name", "内部行车道路");
                internal_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路线稿"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "道路线稿", ThSitePlanCommon.LAYER_ROAD_INTERNAL },
                                { "道路中心线", ThSitePlanCommon.LAYER_ROAD_INTERNAL_AXIS },
                            }
                        }
                    }
                });
                internal_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路色块"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "内部车道色块", ThSitePlanCommon.LAYER_ROAD_INTERNAL }
                            }
                        }
                    }
                });
                roads.Groups.Add(internal_road);
            }

            {
                var pedestrian_road= new ThSitePlanConfigItemGroup();
                pedestrian_road.Properties.Add("Name", "内部人行道路");
                pedestrian_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路线稿"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "人行道路线稿", ThSitePlanCommon.LAYER_ROAD_PEDESTRIAN },
                            }
                        }
                    }
                });
                pedestrian_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路色块"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "人行道路色块", ThSitePlanCommon.LAYER_ROAD_PEDESTRIAN }
                            }
                        }
                    }
                });
                roads.Groups.Add(pedestrian_road);
            }

            {
                var external_road = new ThSitePlanConfigItemGroup();
                external_road.Properties.Add("Name", "外部车行道路");
                external_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路线稿"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "外部车行道路", ThSitePlanCommon.LAYER_ROAD_EXTERNAL },
                            }
                        }
                    }
                });
                external_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路色块"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "外部车道色块", ThSitePlanCommon.LAYER_ROAD_EXTERNAL }
                            }
                        }
                    }
                });
                roads.Groups.Add(external_road);
            }

            {
                var external_road = new ThSitePlanConfigItemGroup();
                external_road.Properties.Add("Name", "外部景观道路");
                external_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路线稿"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "景观道路线稿", ThSitePlanCommon.LAYER_ROAD_LANDSCAPE },
                            }
                        }
                    }
                });
                external_road.Items.Add(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>() {
                        { "Name", "道路色块"},
                        { "Color", new Color()},
                        { "Transparency", 0 },
                        { "CADFrame", "" },
                        { "CADLayer", new  Dictionary<string, string>()
                            {
                                { "景观道路色块", ThSitePlanCommon.LAYER_ROAD_LANDSCAPE }
                            }
                        }
                    }
                });
                roads.Groups.Add(external_road);
            }

            return roads;
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
