using System;
using System.Drawing;
using System.Collections.Generic;
using TianHua.Publics.BaseCode;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System.Linq;

namespace ThSitePlan.Configuration
{
    public abstract class ThSitePlanConfigObj
    {
        public abstract bool IsEnabled { get; set; }
        public abstract Dictionary<string, object> Properties { get; set; }
    }

    public class ThSitePlanConfigItem : ThSitePlanConfigObj
    {
        public override bool IsEnabled { get; set; }
        public override Dictionary<string, object> Properties { get; set; }
        public ThSitePlanConfigItem()
        {
            Properties = new Dictionary<string, object>();
        }
    }

    public class ThSitePlanConfigItemGroup : ThSitePlanConfigObj
    {
        public override bool IsEnabled { get; set; }
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
        public string RootJsonString { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                DBDictionary dbdc = acadDatabase.Element<DBDictionary>(acadDatabase.Database.NamedObjectsDictionaryId, false);
                if (dbdc.Contains(ThSitePlanCommon.Configuration_Xrecord_Name))
                {
                    InitializeWithDb();
                }
                else
                {
                    InitializeWithResource();
                }
            }
        }

        private void InitializeWithDb()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                DBDictionary dbdc = acadDatabase.Element<DBDictionary>(acadDatabase.Database.NamedObjectsDictionaryId, false);
                ObjectId obj = dbdc.GetAt(ThSitePlanCommon.Configuration_Xrecord_Name);
                Xrecord bck = acadDatabase.Element<Xrecord>(obj, false);
                string xrecorddata = bck.Data.AsArray().First().Value.ToString();
                if (xrecorddata != null)
                {
                    RootJsonString = xrecorddata;
                    InitializeFromString(xrecorddata);
                }
                else
                {
                    InitializeWithResource();
                }
            }
        }

        private void InitalizeWithCode()
        {
            Root = new ThSitePlanConfigItemGroup();
            Root.Properties.Add("Name", ThSitePlanCommon.ThSitePlan_Frame_Name_Unused);
            Root.AddItem(new ThSitePlanConfigItem()
            {
                Properties = new Dictionary<string, object>()
                {
                    { "Name", ThSitePlanCommon.ThSitePlan_Frame_Name_Unrecognized},
                    { "Color", Color.Black},
                    { "Opacity", 100 },
                    { "CADFrame", "" },
                    { "CADLayer", new  List<string>()}
                }
            });

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

            // 建筑物
            Root.AddGroup(ConstructBuilding());

            // 全局阴影
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
                            "P-BUID-HACH"
                        }
                    }
                }
            });

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

        private void InitializeWithResource()
        {
            string _Txt = FuncStr.NullToStr(Properties.Resources.BasicStyle);
            RootJsonString = _Txt;
            InitializeFromString(_Txt);
        }

        private void InitializeFromString(string orgstring)
        {
            var _ListColorGeneral = FuncJson.Deserialize<List<ColorGeneralDataModel>>(orgstring);
            Root = new ThSitePlanConfigItemGroup();
            Root.Properties.Add("Name", ThSitePlanCommon.ThSitePlan_Frame_Name_Unused);
            FuncFile.ToConfigItemGroup(_ListColorGeneral, Root);
            Root = ReConstructItemName(Root, null);
        }

        public void EnableAll(bool bEnable)
        {
            EnableItem(bEnable, Root);
        }

        public void EnableItemAndItsAncestor(string name, bool bEnable)
        {
            //打开所有分组
            EnableAllGroup(true,Root);

            //查找要打开的item子项，打开该项以及其兄弟节点
            string[] namegroup = name.Split('-');
            ThSitePlanConfigItem FindItem = null;

            var SearchItems = Root.Items;
            for (int i = 0; i < namegroup.Length; i++)
            {
                foreach (var item in SearchItems)
                {
                    if (item.Properties["Name"].ToString() == namegroup[i] || item.Properties["Name"].ToString() == name)
                    {
                        //找到item直接打开，对于某些只有一层的图层
                        if (item is ThSitePlanConfigItem fdit)
                        {
                            item.IsEnabled = bEnable;
                            FindItem = fdit;
                            break;
                        }

                        //找到该项对应的Group,则进入该group继续查找，当查找子项的上一级父节点group，打开该节点中每一个子项
                        else if (item is ThSitePlanConfigItemGroup fdgp)
                        {
                            if (i == namegroup.Length - 2)
                            {
                                item.IsEnabled = bEnable;
                                foreach (var item2 in fdgp.Items)
                                {
                                    item2.IsEnabled = bEnable;
                                }
                            }

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
        }

        public void EnableItemAndItsSiblings(string name, bool bEnable)
        {
            ThSitePlanConfigItemGroup siblinggroup = FindGroupByItemName(name);
            EnableItem(true, siblinggroup);
        }

        public void EnableItemAndAncestorNoSib(string name, bool bEnable)
        {
            //打开所有分组
            EnableAllGroup(true, Root);

            //查找要打开的item子项，打开该项以及其兄弟节点
            string[] namegroup = name.Split('-');
            ThSitePlanConfigItem FindItem = null;

            var SearchItems = Root.Items;
            for (int i = 0; i < namegroup.Length; i++)
            {
                foreach (var item in SearchItems)
                {
                    if (item.Properties["Name"].ToString() == namegroup[i] || item.Properties["Name"].ToString() == name)
                    {
                        //找到item直接打开，对于某些只有一层的图层
                        if (item is ThSitePlanConfigItem fdit)
                        {
                            item.IsEnabled = bEnable;
                            FindItem = fdit;
                            break;
                        }

                        //找到该项父结点，打开该节点
                        else if (item is ThSitePlanConfigItemGroup fdgp)
                        {
                            fdgp.IsEnabled = bEnable;
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
        }

        private void EnableItem(bool bEnable, ThSitePlanConfigObj itgrp)
        {
            if (itgrp is ThSitePlanConfigItemGroup group)
            {
                group.IsEnabled = bEnable;
                foreach (var groupItem in group.Items)
                {
                    EnableItem(bEnable, groupItem);
                }
            }
            else if (itgrp is ThSitePlanConfigItem item)
            {
                item.IsEnabled = bEnable;
            }
        }

        private void EnableAllGroup(bool bEnable, ThSitePlanConfigObj itgrp)
        {
            if (itgrp is ThSitePlanConfigItemGroup group)
            {
                group.IsEnabled = bEnable;
                foreach (var groupItem in group.Items)
                {
                    EnableAllGroup(bEnable, groupItem);
                }
            }

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
                OuterBuilding.Properties.Add("Name", "场地外建筑");
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
                // 建筑物-场地外建筑-建筑色块
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
                // 建筑物-场地外建筑-建筑线稿
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
                            ThSitePlanCommon.LAYER_SITE_FENCE,
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
                            ThSitePlanCommon.LAYER_SITE_FENCE,
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
                                "P-TRAF-CITY"
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
                Pavement_Outd.Properties.Add("Name", "场地外铺地");

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

        public ThSitePlanConfigItemGroup FindGroupByItemName(string name)
        {
            string[] namegroup = name.Split('-');
            if (namegroup.Length<=1)
            {
                return Root;
            }

            return FindGroupByName(namegroup[namegroup.Length - 2],Root);
        }

        private ThSitePlanConfigItemGroup FindGroupByName(string name, ThSitePlanConfigItemGroup itgrp)
        {
            ThSitePlanConfigItemGroup FindGroup = null;
            foreach (var item in itgrp.Items)
            {
                if (item is ThSitePlanConfigItemGroup findgp)
                {
                    if (findgp.Properties["Name"].ToString() == name)
                    {
                        return findgp;
                    }
                    FindGroup = FindGroupByName(name, findgp);
                    if (FindGroup != null)
                    {
                        return FindGroup;
                    }
                }
            }
            return FindGroup;
        }

        private ThSitePlanConfigItem FindItemByLayer(string layername, ThSitePlanConfigItemGroup findgrp)
        {
            ThSitePlanConfigItem finditem = null;
            foreach (var item in findgrp.Items)
            {
                if (item is ThSitePlanConfigItem fdit)
                {
                    List<string> fdlist = fdit.Properties["CADLayer"] as List<string>;
                    if (fdlist.Contains(layername))
                    {
                        return fdit;
                    }
                }
                else if (item is ThSitePlanConfigItemGroup fdgp)
                {
                    finditem = FindItemByLayer(layername, fdgp);
                    if (finditem != null)
                    {
                        return finditem;
                    }
                }
            }
            return finditem;
        }

        public void FindItemsByCADScript(ThSitePlanConfigItemGroup findgrp, string spid, ref List<ThSitePlanConfigItem> shadowitems)
        {
            foreach (var item in findgrp.Items)
            {
                if (item is ThSitePlanConfigItem fdit)
                {
                    var scriptId = fdit.Properties["CADScriptID"].ToString();
                    if (scriptId == spid)
                    {
                        shadowitems.Add(fdit);
                    }
                }
                else if (item is ThSitePlanConfigItemGroup fdgp)
                {
                    FindItemsByCADScript(fdgp,spid, ref shadowitems) ;
                }
            }
        }

        public ThSitePlanConfigItemGroup FindGroupByLayer(string layername)
        {
            ThSitePlanConfigItem finditem = FindItemByLayer(layername, Root);
            if (finditem == null)
            {
                return null;
            }
            string itemname = finditem.Properties["Name"].ToString();
            return FindGroupByItemName(itemname);
        }

        private ThSitePlanConfigItemGroup ReConstructItemName(ThSitePlanConfigItemGroup origingroup, string outergroupname)
        {
            //遍历传入的Group中所有子项，若是Group,记住名字并继续向深层遍历，若遍历到子item,将Item名扩展重建
            foreach (var item in origingroup.Items)
            {
                if (item is ThSitePlanConfigItemGroup gp)
                {
                    string innergroupname = gp.Properties["Name"].ToString();
                    ReConstructItemName(gp, outergroupname + innergroupname + "-");
                }
                else if (item is ThSitePlanConfigItem it)
                {
                    it.Properties["Name"] = outergroupname + it.Properties["Name"].ToString();
                }
            }

            return origingroup;
        }
    }
}
