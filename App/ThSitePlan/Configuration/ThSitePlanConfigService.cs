using System;
using System.Drawing;
using System.Collections.Generic;

namespace ThSitePlan.Configuration
{
    public abstract class ThSitePlanConfigObj
    {
        public abstract Dictionary<string, object> Properties { get; set; }
    }

    public class ThSitePlanConfigItem : ThSitePlanConfigObj
    {
        public override Dictionary<string, object> Properties { get; set; }
        public ThSitePlanConfigItem()
        {
            Properties = new Dictionary<string, object>();
        }
    }

    public class ThSitePlanConfigItemGroup : ThSitePlanConfigObj
    {
        public Queue<ThSitePlanConfigObj> Items { get; set; }
        public override Dictionary<string, object> Properties { get; set; }

        public ThSitePlanConfigItemGroup()
        {
            Items = new Queue<ThSitePlanConfigObj>();
            Properties = new Dictionary<string, object>();
        }

        public void AddItem(ThSitePlanConfigItem item)
        {
            Items.Enqueue(item);
        }

        public void AddGroup(ThSitePlanConfigItemGroup group)
        {
            Items.Enqueue(group);
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
            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "基本文字说明及图例"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-NOTE-PLTB",
                            "P-BUID-NUMB",
                        }
                    }
                }
            });

            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地标高"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-ROAD-BEAE",
                            "P-ROAD-ELEV",
                            "P-BUID-ELEV",
                        }
                    }
                }
            });

            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "尺寸标注"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-FIRE-DIMS",
                            "P-ROAD-DIMS",
                            "P-BUID-DIMS",
                        }
                    }
                }
            });

            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "界线"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-LIMT",
                            "P-LIMT-LIPB",
                            "P-LIMT-BUID",
                            "P-LIMT-COOR",
                            "P-BUID-UDBD",
                            "P-CONS-FENC",
                        }
                    }
                }
            });

            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "原始场地叠加线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-OUTD"
                        }
                    }
                }
            });

            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", "全局阴影"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-AI-shadow"
                        }
                    }
                }
            });

            // 建筑物
            Root.AddGroup(ConstructBuilding());

            // 树木
            Root.AddGroup(ConstructTree());

            // 场地
            Root.AddGroup(ConstructSites());

            // 道路
            Root.AddGroup(ConstructRoads());

            // 铺装
            Root.AddGroup(ConstructPavement());

            // 景观绿地
            Root.AddGroup(ConstructGreenland());

        }

        private ThSitePlanConfigItemGroup ConstructBuilding()
        {
            var building = new ThSitePlanConfigItemGroup();
            building.Properties.Add("Name", "建筑物");
            {
                // 建筑物-场地内建筑
                var InnerBuilding = new ThSitePlanConfigItemGroup();
                InnerBuilding.Properties.Add("Name", "场地内建筑");
                // 建筑物-场地内建筑-建筑信息
                InnerBuilding.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑物-场地内建筑-建筑信息"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-BUID-FLOR",
                            "P-BUID-HIGH",
                        }
                    }
                }
                });
                // 建筑物-场地内建筑-建筑色块
                InnerBuilding.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑物-场地内建筑-建筑色块"},
                    { "Color", Color.Red},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-BUID-BMAX",
                            "P-BUID-BCHA",
                            "P-BUID-ROOF",
                        }
                    }
                }
                });
                // 建筑物-场地内建筑-建筑线稿
                InnerBuilding.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑物-场地内建筑-建筑线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-BUID-BMAX",
                            "P-BUID-BCHA",
                            "P-BUID-ROOF",
                        }
                    }
                }
                });
                building.AddGroup(InnerBuilding);
            }

            {
                // 建筑物-场地外建筑
                var OuterBuilding = new ThSitePlanConfigItemGroup();
                OuterBuilding.Properties.Add("Name", "场地内建筑");
                // 建筑物-场地外建筑-建筑信息
                OuterBuilding.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑物-场地外建筑-建筑信息"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-OUTD-FLOR",
                        }
                    }
                }
                });
                // 建筑物-场地内建筑-建筑色块
                OuterBuilding.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑物-场地外建筑-建筑色块"},
                    { "Color", Color.Red},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-OUTD-BUID",
                        }
                    }
                }
                });
                // 建筑物-场地内建筑-建筑线稿
                OuterBuilding.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "建筑物-场地外建筑-建筑线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            "P-OUTD-BUID",
                        }
                    }
                }
                });
                building.AddGroup(OuterBuilding);
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
                // 树木.景观树.树木色块
                landscape_tree.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木-景观树-树木色块"},
                    { "Color", Color.Green},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_LANDSCAPE_TREE
                        }
                    }
                }
                });

                // 树木.景观树.树木线稿
                landscape_tree.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木-景观树-树木线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_LANDSCAPE_TREE
                        }
                    }
                }
                });
                tree.AddGroup(landscape_tree);
            }

            // 树木.行道树
            {
                var street_tree = new ThSitePlanConfigItemGroup();
                street_tree.Properties.Add("Name", "行道树");
                // 树木.行道树.树木色块
                street_tree.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木-行道树-树木色块"},
                    { "Color", Color.Green},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_STREET_TREE
                        }
                    }
                }
                });
                // 树木.行道树.树木线稿
                street_tree.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "树木-行道树-树木线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_STREET_TREE
                        }
                    }
                }
                });
                tree.AddGroup(street_tree);
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
                // 场地.消防登高场地.场地色块
                fire_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-消防登高场地-场地色块"},
                    { "Color", Color.Yellow},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_FIRE
                        }
                    }
                }
                });

                // 场地.消防登高场地.场地线稿
                fire_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-消防登高场地-场地线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_FIRE
                        }
                    }
                }
                });
                sites.AddGroup(fire_site);
            }

            {
                //场地.停车场地
                var parking_site = new ThSitePlanConfigItemGroup();
                parking_site.Properties.Add("Name", "停车场地");
                // 场地.停车场地.场地色块
                parking_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-停车场地-场地色块"},
                    { "Color", Color.Red},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_PARKING
                        }
                    }
                }
                });

                // 场地.停车场地.场地线稿
                parking_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-停车场地-场地线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_PARKING
                        }
                    }
                }
                });
                sites.AddGroup(parking_site);
            }

            {
                //场地.活动场地
                var activity_site = new ThSitePlanConfigItemGroup();
                activity_site.Properties.Add("Name", "活动场地");
                // 场地.活动场地.场地色块
                activity_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-活动场地-场地色块"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_ACTIVITY
                        }
                    }
                }
                });

                // 场地.活动场地.场地线稿
                activity_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-活动场地-场地线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_ACTIVITY
                        }
                    }
                }
                });
                
                sites.AddGroup(activity_site);
            }

            {
                //场地.其他场地
                var other_site = new ThSitePlanConfigItemGroup();
                other_site.Properties.Add("Name", "其他场地");
                // 场地.其他场地.场地色块
                other_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-其他场地-场地色块"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_MISCELLANEOUS
                        }
                    }
                }
                });

                // 场地.其他场地.场地线稿
                other_site.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "场地-其他场地-场地线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_SITE_MISCELLANEOUS
                        }
                    }
                }
                });
                sites.AddGroup(other_site);
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

                internal_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-内部车行道路-道路色块"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_INTERNAL,
                            }
                        }
                    }
                });

                internal_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-内部车行道路-道路线稿"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_INTERNAL,
                                ThSitePlanCommon.LAYER_ROAD_INTERNAL_AXIS,
                            }
                        }
                    }
                });
               
                roads.AddGroup(internal_road);
            }

            {
                var pedestrian_road= new ThSitePlanConfigItemGroup();
                pedestrian_road.Properties.Add("Name", "内部人行道路");

                pedestrian_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-内部人行道路-道路色块"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_PEDESTRIAN,
                            }
                        }
                    }
                });

                pedestrian_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-内部人行道路-道路线稿"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_PEDESTRIAN,
                            }
                        }
                    }
                });
                roads.AddGroup(pedestrian_road);
            }

            {
                var external_road = new ThSitePlanConfigItemGroup();
                external_road.Properties.Add("Name", "外部车行道路");

                external_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-外部车行道路-道路色块"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_EXTERNAL,
                            }
                        }
                    }
                });

                external_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-外部车行道路-道路线稿"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_EXTERNAL,
                            }
                        }
                    }
                });
                
                roads.AddGroup(external_road);
            }

            {
                var extland_road = new ThSitePlanConfigItemGroup();
                extland_road.Properties.Add("Name", "外部景观道路");

                extland_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-外部景观道路-道路色块"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_LANDSCAPE,
                            }
                        }
                    }
                });

                extland_road.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                    {
                        { "Name", "道路-外部景观道路-道路线稿"},
                        { "Color", Color.Black},
                        { "Opacity", 100 },
                        { "CADFrame", "" },
                        { "CADLayer", new  List<string>()
                            {
                                ThSitePlanCommon.LAYER_ROAD_LANDSCAPE,
                            }
                        }
                    }
                });
                roads.AddGroup(extland_road);
            }

            return roads;
        }

        private ThSitePlanConfigItemGroup ConstructPavement()
        {
            var pave = new ThSitePlanConfigItemGroup();
            pave.Properties.Add("Name", "铺装");
            {
                //铺装-场地外铺地
                var Pavement_Outd = new ThSitePlanConfigItemGroup();
                Pavement_Outd.Properties.Add("Name", "景观树");

                // 铺装-场地外铺地-铺装色块
                Pavement_Outd.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "铺装-场地外铺地-铺装色块"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_PAVE_OUTD
                        }
                    }
                }
                });

                // 铺装-场地外铺地-铺装线稿
                Pavement_Outd.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "铺装-场地外铺地-铺装线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_PAVE_OUTD
                        }
                    }
                }
                });

                pave.AddGroup(Pavement_Outd);
            }
            return pave;
        }

        private ThSitePlanConfigItemGroup ConstructGreenland()
        {
            var greenLd = new ThSitePlanConfigItemGroup();
            greenLd.Properties.Add("Name", "景观绿地");
            {
                //景观绿地-水景
                var GreenLand_Watersp = new ThSitePlanConfigItemGroup();
                GreenLand_Watersp.Properties.Add("Name", "水景");

                // 景观绿地-水景-水景色块
                GreenLand_Watersp.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "景观绿地-水景-水景色块"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_GREEN_WATER
                        }
                    }
                }
                });

                // 景观绿地-水景-水景线稿
                GreenLand_Watersp.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "景观绿地-水景-水景线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_GREEN_WATER
                        }
                    }
                }
                });
                greenLd.AddGroup(GreenLand_Watersp);
            }

            {
                //景观绿地-景观
                var GreenLand_LandSp = new ThSitePlanConfigItemGroup();
                GreenLand_LandSp.Properties.Add("Name", "景观");

                // 景观绿地-景观-景观色块
                GreenLand_LandSp.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "景观绿地-景观-景观色块"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_GREEN_LANDSP
                        }
                    }
                }
                });

                // 景观绿地-景观-景观线稿
                GreenLand_LandSp.AddItem(new ThSitePlanConfigItem()
                {
                    Properties = new Dictionary<string, object>()
                {
                    { "Name", "景观绿地-景观-景观线稿"},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()
                        {
                            ThSitePlanCommon.LAYER_GREEN_LANDSP
                        }
                    }
                }
                });
                greenLd.AddGroup(GreenLand_LandSp);
            }
            return greenLd;
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

        public ThSitePlanConfigItem FindItemByName(string name)
        {
            string[] namegroup = name.Split('-');
            ThSitePlanConfigItem FindItem = null;

            var SearchItems = Root.Items;
            for (int i = 0; i < namegroup.Length; i++)
            {
                foreach (var item in SearchItems)
                {
                    if (item.Properties["Name"].ToString() == namegroup[i] || item.Properties["Name"].ToString() == name)
                    {
                        if (item is ThSitePlanConfigItem fdit)
                        {
                            FindItem = fdit;
                            break;
                        }
                        else if (item is ThSitePlanConfigItemGroup fdgp)
                        {
                            SearchItems = fdgp.Items;
                            break;
                        }
                    }
                }

                if (FindItem != null)
                {
                    break;
                }
            }
            return FindItem;
        }
    }
}
